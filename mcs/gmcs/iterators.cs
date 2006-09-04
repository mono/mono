//
// iterators.cs: Support for implementing iterators
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2003 Ximian, Inc.
//
// TODO:
//    Flow analysis for Yield.
//    Emit calls to base object constructor.
//
// Generics note:
//    Current should be defined to return T, and IEnumerator.Current returns object
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp {

	public interface IIteratorContainer {

		//
		// Invoked if a yield statement is found in the body
		//
		void SetYields ();
	}
	
	public class Yield : Statement {
		Expression expr;
		ArrayList finally_blocks;

		public Yield (Expression expr, Location l)
		{
			this.expr = expr;
			loc = l;
		}

		public static bool CheckContext (EmitContext ec, Location loc, bool isYieldBreak)
		{
			if (ec.InFinally) {
				Report.Error (1625, loc, "Cannot yield in the body of a " +
					      "finally clause");
				return false;
			} 
			
			if (ec.InUnsafe) {
				Report.Error (1629, loc, "Unsafe code may not appear in iterators");
				return false;
			}

			AnonymousContainer am = ec.CurrentAnonymousMethod;
			if ((am != null) && !am.IsIterator){
				Report.Error (1621, loc, "The yield statement cannot be used inside anonymous method blocks");
				return false;
			}

			if (ec.CurrentBranching.InTryWithCatch () && (!isYieldBreak || !ec.InCatch)) {
				if (!ec.InCatch)
					Report.Error (1626, loc, "Cannot yield a value in the body of a " +
					"try block with a catch clause");
				else
					Report.Error (1631, loc, "Cannot yield a value in the body of a catch clause");
				return false;
			}
			return true;
		}
		
		public override bool Resolve (EmitContext ec)
		{
			Report.Debug (64, "RESOLVE YIELD", this, expr, expr.GetType ());
			expr = expr.Resolve (ec);
			if (expr == null)
				return false;

			Report.Debug (64, "RESOLVE YIELD #1", this, expr, expr.GetType ());

			if (!CheckContext (ec, loc, false))
				return false;

			Iterator iterator = ec.CurrentIterator;

			if (expr.Type != iterator.IteratorType){
				expr = Convert.ImplicitConversionRequired (
					ec, expr, iterator.IteratorType, loc);
				if (expr == null)
					return false;
			}

			ec.CurrentBranching.StealFinallyClauses (ref finally_blocks);
			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			ec.CurrentIterator.MarkYield (ec, expr, finally_blocks);
		}
	}

	public class YieldBreak : Statement {

		public YieldBreak (Location l)
		{
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			if (!Yield.CheckContext (ec, loc, true))
				return false;

			ec.CurrentBranching.CurrentUsageVector.Goto ();
			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			ec.CurrentIterator.EmitYieldBreak (ec.ig);
		}
	}

	public class Iterator : AnonymousContainer {
		protected ToplevelBlock original_block;
		protected ToplevelBlock block;

		protected readonly Type original_iterator_type;
		protected readonly bool is_enumerable;
		TypeExpr iterator_type_expr;
		public readonly bool IsStatic;

		//
		// The state as we generate the iterator
		//
		Label move_next_ok, move_next_error;
		ArrayList resume_points = new ArrayList ();
		int pc;
		
		//
		// Context from the original method
		//
		GenericMethod generic_method;
		TypeContainer container;
		TypeExpr current_type;
		Type this_type;
		Parameters parameters;
		Parameters original_parameters;
		IMethodData orig_method;

		MethodInfo dispose_method;
		Method move_next_method;
		Constructor ctor;
		CaptureContext cc;

		Expression enumerator_type;
		Expression enumerable_type;
		Expression generic_enumerator_type;
		Expression generic_enumerable_type;
		TypeArguments generic_args;
		EmitContext ec;

		protected enum State {
			Uninitialized	= -2,
			After,
			Running
		}

		public void EmitYieldBreak (ILGenerator ig)
		{
			ig.Emit (OpCodes.Ldarg_0);
			IntConstant.EmitInt (ig, (int) State.After);
			ig.Emit (OpCodes.Stfld, pc_field.FieldBuilder);
			ig.Emit (OpCodes.Br, move_next_error);
		}

		protected void EmitMoveNext (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			move_next_ok = ig.DefineLabel ();
			move_next_error = ig.DefineLabel ();

			LocalBuilder retval = ec.GetTemporaryLocal (TypeManager.int32_type);

			ig.BeginExceptionBlock ();

			Label dispatcher = ig.DefineLabel ();
			ig.Emit (OpCodes.Br, dispatcher);

			ResumePoint entry_point = new ResumePoint (null);
			resume_points.Add (entry_point);
			entry_point.Define (ig);

			ec.EmitTopBlock (orig_method, original_block);

			EmitYieldBreak (ig);

			ig.MarkLabel (dispatcher);

			Label [] labels = new Label [resume_points.Count];
			for (int i = 0; i < labels.Length; i++)
				labels [i] = ((ResumePoint) resume_points [i]).Label;

			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, pc_field.FieldBuilder);
			ig.Emit (OpCodes.Switch, labels);

			Label end = ig.DefineLabel ();

			ig.MarkLabel (move_next_error);
  			ig.Emit (OpCodes.Ldc_I4_0); 
			ig.Emit (OpCodes.Stloc, retval);
			ig.Emit (OpCodes.Leave, end);

			ig.MarkLabel (move_next_ok);
			ig.Emit (OpCodes.Ldc_I4_1);
			ig.Emit (OpCodes.Stloc, retval);
			ig.Emit (OpCodes.Leave, end);

			ig.BeginFaultBlock ();

			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Callvirt, dispose_method);

			ig.EndExceptionBlock ();

			ig.MarkLabel (end);
			ig.Emit (OpCodes.Ldloc, retval);
			ig.Emit (OpCodes.Ret);
		}

		public void EmitDispose (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			Label end = ig.DefineLabel ();
			Label dispatcher = ig.DefineLabel ();
			ig.Emit (OpCodes.Br, dispatcher);

			Label [] labels = new Label [resume_points.Count];
			for (int i = 0; i < labels.Length; i++) {
				ResumePoint point = (ResumePoint) resume_points [i];

				if (point.FinallyBlocks == null) {
					labels [i] = end;
					continue;
				}

				labels [i] = ig.DefineLabel ();
				ig.MarkLabel (labels [i]);

				ig.BeginExceptionBlock ();
				ig.BeginFinallyBlock ();

				foreach (ExceptionStatement stmt in point.FinallyBlocks) {
					if (stmt != null)
						stmt.EmitFinally (ec);
				}

				ig.EndExceptionBlock ();
				ig.Emit (OpCodes.Br, end);
			}

			ig.MarkLabel (dispatcher);
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, pc_field.FieldBuilder);
			ig.Emit (OpCodes.Switch, labels);

			ig.Emit (OpCodes.Ldarg_0);
			IntConstant.EmitInt (ig, (int) State.After);
			ig.Emit (OpCodes.Stfld, pc_field.FieldBuilder);

			ig.MarkLabel (end);
		}

		protected class ResumePoint
		{
			public Label Label;
			public readonly ExceptionStatement[] FinallyBlocks;

			public ResumePoint (ArrayList list)
			{
				if (list != null) {
					FinallyBlocks = new ExceptionStatement [list.Count];
					list.CopyTo (FinallyBlocks, 0);
				}
			}

			public void Define (ILGenerator ig)
			{
				Label = ig.DefineLabel ();
				ig.MarkLabel (Label);
			}
		}

		//
		// Called back from Yield
		//
		public void MarkYield (EmitContext ec, Expression expr,
				       ArrayList finally_blocks)
		{
			ILGenerator ig = ec.ig;

			// Store the new current
			ig.Emit (OpCodes.Ldarg_0);
			expr.Emit (ec);
			ig.Emit (OpCodes.Stfld, current_field.FieldBuilder);

			// increment pc
			pc++;
			ig.Emit (OpCodes.Ldarg_0);
			IntConstant.EmitInt (ig, pc);
			ig.Emit (OpCodes.Stfld, pc_field.FieldBuilder);

			// Return ok
			ig.Emit (OpCodes.Br, move_next_ok);

			ResumePoint point = new ResumePoint (finally_blocks);
			resume_points.Add (point);
			point.Define (ig);
		}

		public void MarkFinally (EmitContext ec, ArrayList finally_blocks)
		{
			ILGenerator ig = ec.ig;

			// increment pc
			pc++;
			ig.Emit (OpCodes.Ldarg_0);
			IntConstant.EmitInt (ig, pc);
			ig.Emit (OpCodes.Stfld, pc_field.FieldBuilder);

			ResumePoint point = new ResumePoint (finally_blocks);
			resume_points.Add (point);
			point.Define (ig);
		}

		//
		// Our constructor
		//
		public Iterator (IMethodData m_container, TypeContainer parent, GenericMethod generic,
				 int modifiers, Type iterator_type, bool is_enumerable)
			: base (parent, generic, m_container.ParameterInfo, m_container.Block,
				(modifiers & Modifiers.UNSAFE) | Modifiers.PRIVATE,
				m_container.Location)
		{
			this.orig_method = m_container;
			this.original_iterator_type = iterator_type;
			this.is_enumerable = is_enumerable;

			this.generic_method = generic;
			this.container = ((TypeContainer) parent).PartialContainer;
			this.original_parameters = m_container.ParameterInfo;
			this.original_block = orig_method.Block;
			this.block = new ToplevelBlock (orig_method.Block, parameters, orig_method.Location);

			IsStatic = (modifiers & Modifiers.STATIC) != 0;
		}

		public override ConstructorInfo Constructor {
			get { return ctor.ConstructorBuilder; }
		}

		protected bool DefineIterator ()
		{
			ec = new EmitContext (this, this, Location, null, null, ModFlags);
			ec.CurrentAnonymousMethod = this;
			ec.InIterator = true;

			for (int i = 0; i < original_parameters.Count; i++){
				Parameter.Modifier mod = original_parameters.ParameterModifier (i);
				if ((mod & (Parameter.Modifier.REF | Parameter.Modifier.OUT)) != 0){
					Report.Error (
						1623, Location,
						"Iterators cannot have ref or out parameters");
					return false;
				}

				if ((mod & Parameter.Modifier.ARGLIST) != 0) {
					Report.Error (1636, Location, "__arglist is not allowed in parameter list of iterators");
					return false;
				}

				if (original_parameters.ParameterType (i).IsPointer) {
					Report.Error (1637, Location, "Iterators cannot have unsafe parameters or yield types");
					return false;
				}
			}

			if (container.CurrentType != null)
				this_type = container.CurrentType;
			else
				this_type = container.TypeBuilder;

			orig_method.Block = block;

			return Resolve (ec);
		}

		public override bool Resolve (EmitContext ec)
		{
			if (!CreateMethodHost (ec))
				return false;

			return true;
		}

		protected override bool CreateMethodHost (EmitContext ec)
		{
			Report.Debug (64, "ITERATOR CREATE METHOD HOST", this, Block);

			move_next_method = new Method (
				this, null, TypeManager.system_boolean_expr,
				Modifiers.PUBLIC, false,
				new MemberName ("MoveNext", Location),
				Parameters.EmptyReadOnlyParameters, null);

			move_next_method.Block = Block;
			AddMethod (move_next_method);

			MoveNextStatement inline = new MoveNextStatement (this, Location);
			Block.AddStatement (inline);

			return true;
		}

		public override CompilerGeneratedClass CreateScopeType (ScopeInfo scope)
		{
			return this;
		}

		public override bool IsIterator {
			get { return true; }
		}

		MethodInfo FetchMethodDispose ()
		{
			MemberList dispose_list;

			dispose_list = FindMembers (
				current_type.Type,
				MemberTypes.Method, BindingFlags.Public | BindingFlags.Instance,
				Type.FilterName, "Dispose");

			if (dispose_list.Count != 1)
				throw new InternalErrorException ("Cannot find Dipose() method.");

			return (MethodInfo) dispose_list [0];
		}

		protected override bool DoDefineMembers ()
		{
			ec.InIterator = true;
			ec.CurrentAnonymousMethod = this;
			ec.capture_context = cc;

			if (!base.DoDefineMembers ())
				return false;

			dispose_method = FetchMethodDispose ();
			if (dispose_method == null)
				return false;

			Report.Debug (64, "ITERATOR DEFINE MEMBERS", this);

			return true;
		}

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			ec.InIterator = true;
			ec.CurrentAnonymousMethod = this;
			ec.capture_context = cc;

			ec.TypeContainer = ec.TypeContainer.Parent;

			if (ec.TypeContainer.CurrentType != null)
				ec.ContainerType = ec.TypeContainer.CurrentType;
			else
				ec.ContainerType = ec.TypeContainer.TypeBuilder;

			// ec.ig = move_next_method.method.MethodBuilder.GetILGenerator ();

			if (!ctor.Define ())
				return false;

			bool unreachable;

			if (!ec.ResolveTopBlock (null, original_block, parameters, orig_method, out unreachable))
				return false;

			if (!ec.ResolveTopBlock (null, block, parameters, orig_method, out unreachable))
				return false;

			original_block.CompleteContexts (ec);

			cc.EmitAnonymousHelperClasses (ec);

			ComputeMethodHost ();

			return DefineMembers ();
		}

		Parameter InflateParameter (Parameter param)
		{
			TypeExpr te = InflateType (param.ParameterType);
			return new Parameter (
				te, param.Name, param.ModFlags, param.OptAttributes, param.Location);
		}

		Parameters InflateParameters (Parameters parameters, EmitContext ec)
		{
			int count = parameters.FixedParameters.Length;
			if (count == 0)
				return Parameters.EmptyReadOnlyParameters;
			Parameter[] fixed_params = new Parameter [count];
			for (int i = 0; i < count; i++)
				fixed_params [i] = InflateParameter (parameters.FixedParameters [i]);

			return new Parameters (fixed_params, parameters.HasArglist);
		}

		public override TypeExpr [] GetClassBases (out TypeExpr base_class)
		{
			iterator_type_expr = InflateType (original_iterator_type);

			generic_args = new TypeArguments (Location);
			generic_args.Add (iterator_type_expr);

			ArrayList list = new ArrayList ();
			if (is_enumerable) {
				enumerable_type = new TypeExpression (
					TypeManager.ienumerable_type, Location);
				list.Add (enumerable_type);

				generic_enumerable_type = new ConstructedType (
					TypeManager.generic_ienumerable_type,
					generic_args, Location);
				list.Add (generic_enumerable_type);
			}

			enumerator_type = new TypeExpression (
				TypeManager.ienumerator_type, Location);
			list.Add (enumerator_type);

			list.Add (new TypeExpression (TypeManager.idisposable_type, Location));

			generic_enumerator_type = new ConstructedType (
				TypeManager.generic_ienumerator_type,
				generic_args, Location);
			list.Add (generic_enumerator_type);

			Bases = list;

			return base.GetClassBases (out base_class);
		}

		//
		// Returns the new block for the method, or null on failure
		//
		protected override bool DefineNestedTypes ()
		{
			if (CurrentType != null)
				current_type = new TypeExpression (CurrentType, Location);
			else
				current_type = new TypeExpression (TypeBuilder, Location);

			parameters = InflateParameters (original_parameters, ec);
			if (!parameters.Resolve (ec)) {
				// TODO:
			}

			Define_Fields ();
			Define_Current (false);
			Define_Current (true);
			Define_Reset ();
			Define_Dispose ();

			Define_Constructor ();

			Create_Block ();

			if (is_enumerable) {
				Define_GetEnumerator (false);
				Define_GetEnumerator (true);
			}

			return base.DefineNestedTypes ();
		}

		Field pc_field;
		Field current_field;
		Method dispose;

		void Create_Block ()
		{
			original_block.SetHaveAnonymousMethods (Location, this);
			block.SetHaveAnonymousMethods (Location, this);

			cc = original_block.CaptureContext;

			int first = IsStatic ? 0 : 1;

			ArrayList args = new ArrayList ();
			if (!IsStatic) {
				Type t = this_type;
				args.Add (new Argument (
					new ThisParameterReference (t, Location)));
				cc.CaptureThis (this);
			}

			args.Add (new Argument (new BoolLiteral (false, Location)));

			for (int i = 0; i < parameters.Count; i++) {
				Type t = original_parameters.ParameterType (i);
				Type inflated = parameters.ParameterType (i);
				string name = parameters.ParameterName (i);

				args.Add (new Argument (
					new SimpleParameterReference (t, first + i, Location)));

				Report.Debug (64, "ITERATOR ADD PARAMETER", this, i, t, name,
					      inflated);

				cc.AddParameterToContext (
					this, parameters [i], first + i, Location);
			}

			TypeExpr proxy_type;
			if (generic_method != null) {
				TypeArguments new_args = new TypeArguments (Location);
				if (Parent.IsGeneric) {
					foreach (TypeParameter tparam in Parent.TypeParameters)
						new_args.Add (new TypeParameterExpr (tparam, Location));
				}
				foreach (TypeParameter tparam in generic_method.TypeParameters)
					new_args.Add (new TypeParameterExpr (tparam, Location));
				ConstructedType ct = new ConstructedType (CurrentType, new_args, Location);
				proxy_type = ct.ResolveAsTypeTerminal (ec, false);
			} else
				proxy_type = current_type;

			Expression new_expr = new New (proxy_type, args, Location);
			block.AddStatement (new NoCheckReturn (new_expr, Location));
		}

		void Define_Fields ()
		{
			pc_field = new Field (
				this, TypeManager.system_int32_expr, Modifiers.PRIVATE, "$PC",
				null, Location);
			AddField (pc_field);

			current_field = new Field (
				this, iterator_type_expr, Modifiers.PRIVATE, "$current",
				null, Location);
			AddField (current_field);
		}

		void Define_Constructor ()
		{
			Parameters ctor_params;

			ArrayList list = new ArrayList ();

			if (!IsStatic)
				list.Add (new Parameter (
					new TypeExpression (this_type, Location),
					"this", Parameter.Modifier.NONE,
					null, Location));
			list.Add (new Parameter (
				TypeManager.bool_type, "initialized",
				Parameter.Modifier.NONE, null, Location));

			Parameter[] old_fixed = parameters.FixedParameters;
			list.AddRange (old_fixed);

			Parameter[] fixed_params = new Parameter [list.Count];
			list.CopyTo (fixed_params);

			ctor_params = new Parameters (fixed_params);

			ctor = new Constructor (
				this, MemberName.Name, Modifiers.PUBLIC, ctor_params,
				new GeneratedBaseInitializer (Location),
				Location);
			AddConstructor (ctor);

			ctor.Block = new ToplevelBlock (block, parameters, Location);

			int first = IsStatic ? 2 : 3;

			State initial = is_enumerable ? State.Uninitialized : State.Running;
			ctor.Block.AddStatement (new SetState (this, initial, Location));

			ctor.Block.AddStatement (new If (
				new SimpleParameterReference (
					TypeManager.bool_type, first - 1, Location),
				new SetState (this, State.Running, Location),
				Location));

			ExpressionStatement init_scope = GetScopeInitializer (Location);
			ctor.Block.AddStatement (new StatementExpression (init_scope));
		}

		Statement Create_ThrowInvalidOperation ()
		{
			TypeExpr ex_type = new TypeExpression (
				TypeManager.invalid_operation_exception_type, Location);

			return new Throw (new New (ex_type, null, Location), Location);
		}

		Statement Create_ThrowNotSupported ()
		{
			TypeExpr ex_type = new TypeExpression (
				TypeManager.not_supported_exception_type, Location);

			return new Throw (new New (ex_type, null, Location), Location);
		}

		void Define_Current (bool is_generic)
		{
			MemberName left;
			Expression type;
			if (is_generic) {
				left = new MemberName (
					"System.Collections.Generic.IEnumerator",
					generic_args, Location);
				type = iterator_type_expr;
			} else {
				left = new MemberName ("System.Collections.IEnumerator", Location);
				type = TypeManager.system_object_expr;
			}

			MemberName name = new MemberName (left, "Current", null, Location);

			ToplevelBlock get_block = new ToplevelBlock (
				block, parameters, Location);

			get_block.AddStatement (new If (
				new Binary (
					Binary.Operator.LessThanOrEqual,
					new FieldExpression (this, pc_field),
					new IntLiteral ((int) State.Running, pc_field.Location)),
				Create_ThrowInvalidOperation (),
				new Return (
					new FieldExpression (this, current_field), Location),
				Location));

			Accessor getter = new Accessor (get_block, 0, null, Location);

			Property current = new Property (
				this, type, 0, false, name, null, getter, null);
			AddProperty (current);
		}

		void Define_GetEnumerator (bool is_generic)
		{
			MemberName left;
			Expression type;
			if (is_generic) {
				left = new MemberName (
					"System.Collections.Generic.IEnumerable",
					generic_args, Location);
				type = generic_enumerator_type;
			} else {
				left = new MemberName ("System.Collections.IEnumerable", Location);
				type = enumerator_type;
			}

			MemberName name = new MemberName (left, "GetEnumerator", Location);

			Method get_enumerator = new Method (
				this, null, type, 0, false, name,
				Parameters.EmptyReadOnlyParameters, null);

			//
			// We call append instead of add, as we need to make sure that
			// this method is resolved after the MoveNext method, as that one
			// triggers the computation of the AnonymousMethod Scope, which is
			// required during the code generation of the enumerator
			//
			AppendMethod (get_enumerator);

			get_enumerator.Block = new ToplevelBlock (
				block, parameters, Location);

			get_enumerator.Block.SetHaveAnonymousMethods (Location, this);

			Expression ce = new MemberAccess (
				new SimpleName ("System.Threading.Interlocked", Location),
				"CompareExchange");

			Expression pc = new FieldExpression (this, pc_field);
			Expression before = new IntLiteral ((int) State.Running, Location);
			Expression uninitialized = new IntLiteral ((int) State.Uninitialized, Location);

			ArrayList args = new ArrayList ();
			args.Add (new Argument (pc, Argument.AType.Ref));
			args.Add (new Argument (before, Argument.AType.Expression));
			args.Add (new Argument (uninitialized, Argument.AType.Expression));

			get_enumerator.Block.AddStatement (new If (
				new Binary (
					Binary.Operator.Equality,
					new Invocation (ce, args),
					uninitialized),
				new Return (new ThisParameterReference (type.Type, Location),
					    Location),
				Location));

			args = new ArrayList ();
			if (!IsStatic) {
				args.Add (new Argument (new CapturedThisReference (this, Location)));
			}

			args.Add (new Argument (new BoolLiteral (true, Location)));

			for (int i = 0; i < parameters.Count; i++) {
				Expression cp = new CapturedParameterReference (
					this, parameters.ParameterType (i),
					parameters.ParameterName (i), Location);
				args.Add (new Argument (cp));
			}

			Expression new_expr = new New (current_type, args, Location);
			get_enumerator.Block.AddStatement (new Return (new_expr, Location));
		}

		protected class SimpleParameterReference : Expression
		{
			int idx;

			public SimpleParameterReference (Type type, int idx, Location loc)
			{
				this.idx = idx;
				this.loc = loc;
				this.type = type;
				eclass = ExprClass.Variable;
			}

			public override Expression DoResolve (EmitContext ec)
			{
				return this;
			}

			public override void Emit (EmitContext ec)
			{
				DoEmit (ec);
			}

			protected virtual void DoEmit (EmitContext ec)
			{
				ParameterReference.EmitLdArg (ec.ig, idx);
			}
		}

		protected class ThisParameterReference : SimpleParameterReference, IMemoryLocation
		{
			public ThisParameterReference (Type type, Location loc)
				: base (type, 0, loc)
			{ }

			protected override void DoEmit (EmitContext ec)
			{
				base.DoEmit (ec);
				if (ec.TypeContainer is Struct)
					ec.ig.Emit (OpCodes.Ldobj, type);
			}

			public void AddressOf (EmitContext ec, AddressOp mode)
			{
				if (ec.TypeContainer is Struct)
					ec.ig.Emit (OpCodes.Ldarga, 0);
				else
					ec.ig.Emit (OpCodes.Ldarg, 0);
			}
		}

		protected class CapturedParameterReference : Expression
		{
			Iterator iterator;
			string name;

			public CapturedParameterReference (Iterator iterator, Type type,
							   string name, Location loc)
			{
				this.iterator = iterator;
				this.loc = loc;
				this.type = type;
				this.name = name;
				eclass = ExprClass.Variable;
			}

			public override Expression DoResolve (EmitContext ec)
			{
				return this;
			}

			public override void Emit (EmitContext ec)
			{
				ec.CurrentAnonymousMethod = iterator;

				LocalTemporary dummy = null;
				
				iterator.cc.EmitParameter (ec, name, false, false, ref dummy);
			}
		}

		protected class CapturedThisReference : Expression
		{
			public CapturedThisReference (Iterator iterator, Location loc)
			{
				this.loc = loc;
				this.type = iterator.this_type;
				eclass = ExprClass.Variable;
			}

			public override Expression DoResolve (EmitContext ec)
			{
				return this;
			}

			public override void Emit (EmitContext ec)
			{
				ec.EmitThis (false);
			}
		}

		protected class FieldExpression : Expression
		{
			Iterator iterator;
			Field field;

			public FieldExpression (Iterator iterator, Field field)
			{
				this.iterator = iterator;
				this.field = field;
				this.loc = iterator.Location;
			}

			public override Expression DoResolveLValue (EmitContext ec, Expression right_side)
			{
				FieldExpr fexpr = new FieldExpr (field.FieldBuilder, loc);
				fexpr.InstanceExpression = new ThisParameterReference (
					iterator.this_type, loc);
				return fexpr.ResolveLValue (ec, right_side, loc);
			}

			public override Expression DoResolve (EmitContext ec)
			{
				FieldExpr fexpr = new FieldExpr (field.FieldBuilder, loc);
				fexpr.InstanceExpression = new ThisParameterReference (
					iterator.this_type, loc);
				return fexpr.Resolve (ec);
			}

			public override void Emit (EmitContext ec)
			{
				throw new InvalidOperationException ();
			}
		}

		protected class MoveNextStatement : Statement {
			Iterator iterator;

			public MoveNextStatement (Iterator iterator, Location loc)
			{
				this.loc = loc;
				this.iterator = iterator;
			}

			public override bool Resolve (EmitContext ec)
			{
				return true;
			}

			protected override void DoEmit (EmitContext ec)
			{
				ec.CurrentAnonymousMethod = iterator;
				ec.InIterator = true;

				iterator.EmitMoveNext (ec);
			}
		}

		protected class DisposeMethod : Statement {
			Iterator iterator;

			public DisposeMethod (Iterator iterator, Location loc)
			{
				this.loc = loc;
				this.iterator = iterator;
			}

			public override bool Resolve (EmitContext ec)
			{
				return true;
			}

			protected override void DoEmit (EmitContext ec)
			{
				iterator.EmitDispose (ec);
			}
		}

		protected class StatementList : Statement {
			ArrayList statements;

			public StatementList (Location loc)
			{
				this.loc = loc;
				statements = new ArrayList ();
			}

			public void Add (Statement statement)
			{
				statements.Add (statement);
			}

			public override bool Resolve (EmitContext ec)
			{
				foreach (Statement stmt in statements) {
					if (!stmt.Resolve (ec))
						return false;
				}

				return true;
			}

			protected override void DoEmit (EmitContext ec)
			{
				foreach (Statement stmt in statements)
					stmt.Emit (ec);
			}
		}

		protected class SetState : Statement
		{
			Iterator iterator;
			State state;

			public SetState (Iterator iterator, State state, Location loc)
			{
				this.iterator = iterator;
				this.state = state;
				this.loc = loc;
			}

			public override bool Resolve (EmitContext ec)
			{
				return true;
			}

			protected override void DoEmit (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Ldarg_0);
				IntConstant.EmitInt (ec.ig, (int) state);
				ec.ig.Emit (OpCodes.Stfld, iterator.pc_field.FieldBuilder);
			}
		}

		void Define_Reset ()
		{
			Method reset = new Method (
				this, null, TypeManager.system_void_expr, Modifiers.PUBLIC,
				false, new MemberName ("Reset", Location),
				Parameters.EmptyReadOnlyParameters, null);
			AddMethod (reset);

			reset.Block = new ToplevelBlock (Location);
			reset.Block = new ToplevelBlock (block, parameters, Location);
			reset.Block.SetHaveAnonymousMethods (Location, this);

			reset.Block.AddStatement (Create_ThrowNotSupported ());
		}

		void Define_Dispose ()
		{
			dispose = new Method (
				this, null, TypeManager.system_void_expr, Modifiers.PUBLIC,
				false, new MemberName ("Dispose", Location),
				Parameters.EmptyReadOnlyParameters, null);
			AddMethod (dispose);

			dispose.Block = new ToplevelBlock (block, parameters, Location);
			dispose.Block.SetHaveAnonymousMethods (Location, this);

			dispose.Block.AddStatement (new DisposeMethod (this, Location));
		}

		public Type IteratorType {
			get { return iterator_type_expr.Type; }
		}

		//
		// This return statement tricks return into not flagging an error for being
		// used in a Yields method
		//
		class NoCheckReturn : Statement {
			public Expression Expr;
		
			public NoCheckReturn (Expression expr, Location l)
			{
				Expr = expr;
				loc = l;
			}

			public override bool Resolve (EmitContext ec)
			{
				Expr = Expr.Resolve (ec);
				if (Expr == null)
					return false;

				ec.CurrentBranching.CurrentUsageVector.Return ();

				return true;
			}

			protected override void DoEmit (EmitContext ec)
			{
				Expr.Emit (ec);
				ec.ig.Emit (OpCodes.Ret);
			}
		}

		public static Iterator CreateIterator (IMethodData method, TypeContainer parent,
						       GenericMethod generic, int modifiers)
		{
			bool is_enumerable;
			Type iterator_type;

			if (!CheckType (method.ReturnType, out iterator_type, out is_enumerable)) {
				Report.Error (1624, method.Location,
					      "The body of `{0}' cannot be an iterator block " +
					      "because `{1}' is not an iterator interface type",
					      method.GetSignatureForError (),
					      TypeManager.CSharpName (method.ReturnType));
				return null;
			}

			Iterator iterator = new Iterator (
				method, parent, generic, modifiers, iterator_type, is_enumerable);

			if (!iterator.DefineIterator ())
				return null;

			return iterator;
		}

		static bool CheckType (Type ret, out Type original_iterator_type, out bool is_enumerable)
		{
			original_iterator_type = null;
			is_enumerable = false;

			if (ret == TypeManager.ienumerable_type) {
				original_iterator_type = TypeManager.object_type;
				is_enumerable = true;
				return true;
			}
			if (ret == TypeManager.ienumerator_type) {
				original_iterator_type = TypeManager.object_type;
				is_enumerable = false;
				return true;
			}

			if (!ret.IsGenericType)
				return false;

			Type[] args = TypeManager.GetTypeArguments (ret);
			if (args.Length != 1)
				return false;

			Type gt = ret.GetGenericTypeDefinition ();
			if (gt == TypeManager.generic_ienumerable_type) {
				original_iterator_type = args [0];
				is_enumerable = true;
				return true;
			} else if (gt == TypeManager.generic_ienumerator_type) {
				original_iterator_type = args [0];
				is_enumerable = false;
				return true;
			}

			return false;
		}
	}
}

