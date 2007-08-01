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

			for (Block block = ec.CurrentBlock; block != null; block = block.Parent) {
				if (!block.Unsafe)
					continue;

				Report.Error (1629, loc, "Unsafe code may not appear in iterators");
				return false;
			}

			//
			// We can't use `ec.InUnsafe' here because it's allowed to have an iterator
			// inside an unsafe class.  See test-martin-29.cs for an example.
			//
			if (!ec.CurrentAnonymousMethod.IsIterator) {
				Report.Error (1621, loc,
					      "The yield statement cannot be used inside " +
					      "anonymous method blocks");
				return false;
			}

			if (ec.CurrentBranching.InTryWithCatch () && (!isYieldBreak || !ec.InCatch)) {
				if (!ec.InCatch)
					Report.Error (1626, loc, "Cannot yield a value in the body " +
						      "of a try block with a catch clause");
				else
					Report.Error (1631, loc, "Cannot yield a value in the body " +
						      "of a catch clause");
				return false;
			}

			return true;
		}
		
		public override bool Resolve (EmitContext ec)
		{
			Report.Debug (64, "RESOLVE YIELD", this, ec, expr, expr.GetType ());
			expr = expr.Resolve (ec);
			if (expr == null)
				return false;

			Report.Debug (64, "RESOLVE YIELD #1", this, ec, expr, expr.GetType (),
				      ec.CurrentAnonymousMethod, ec.CurrentIterator);

			if (!CheckContext (ec, loc, false))
				return false;

			Iterator iterator = ec.CurrentIterator;
			if (expr.Type != iterator.IteratorType) {
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

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			Yield target = (Yield) t;

			target.expr = expr.Clone (clonectx);
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

	public class IteratorHost : RootScopeInfo
	{
		public readonly Iterator Iterator;

		TypeExpr iterator_type_expr;
		Field pc_field;
		Field current_field;
		MethodInfo dispose_method;

		TypeExpr enumerator_type;
		TypeExpr enumerable_type;
		TypeArguments generic_args;
		TypeExpr generic_enumerator_type;
#if GMCS_SOURCE		
		TypeExpr generic_enumerable_type;
#endif
		
		public IteratorHost (Iterator iterator)
			: base (iterator.Container.Toplevel, iterator.Host, iterator.GenericMethod,
				iterator.Location)
		{
			this.Iterator = iterator;
		}

		public override bool IsIterator {
			get { return true; }
		}

		public MethodInfo Dispose {
			get { return dispose_method; }
		}

		public Field PC {
			get { return pc_field; }
		}

		public Field CurrentField {
			get { return current_field; }
		}

		public Type IteratorType {
			get { return iterator_type_expr.Type; }
		}

		public override TypeExpr [] GetClassBases (out TypeExpr base_class)
		{
			iterator_type_expr = InflateType (Iterator.OriginalIteratorType);

#if GMCS_SOURCE
			generic_args = new TypeArguments (Location);
			generic_args.Add (iterator_type_expr);
#endif

			ArrayList list = new ArrayList ();
			if (Iterator.IsEnumerable) {
				enumerable_type = new TypeExpression (
					TypeManager.ienumerable_type, Location);
				list.Add (enumerable_type);

#if GMCS_SOURCE
				generic_enumerable_type = new ConstructedType (
					TypeManager.generic_ienumerable_type,
					generic_args, Location);
				list.Add (generic_enumerable_type);
#endif
			}

			enumerator_type = new TypeExpression (
				TypeManager.ienumerator_type, Location);
			list.Add (enumerator_type);

			list.Add (new TypeExpression (TypeManager.idisposable_type, Location));

#if GMCS_SOURCE
			generic_enumerator_type = new ConstructedType (
				TypeManager.generic_ienumerator_type,
				generic_args, Location);
			list.Add (generic_enumerator_type);
#endif

			Bases = list;

			return base.GetClassBases (out base_class);
		}

		protected override bool DoResolveMembers ()
		{
			pc_field = CaptureVariable ("$PC", TypeManager.system_int32_expr);
			current_field = CaptureVariable ("$current", iterator_type_expr);

#if GMCS_SOURCE
			Define_Current (true);
#endif
			Define_Current (false);
			new DisposeMethod (this);
			Define_Reset ();

			if (Iterator.IsEnumerable) {
				new GetEnumeratorMethod (this, false);
#if GMCS_SOURCE
				new GetEnumeratorMethod (this, true);
#endif
			}

			return base.DoResolveMembers ();
		}

		public void CaptureScopes ()
		{
			Report.Debug (128, "DEFINE ITERATOR HOST", this, scopes);

			foreach (ScopeInfo si in scopes)
				CaptureScope (si);

			foreach (ScopeInfo si in scopes) {
				if (!si.Define ())
					throw new InternalErrorException ();
				if (si.DefineType () == null)
					throw new InternalErrorException ();
				if (!si.ResolveType ())
					throw new InternalErrorException ();
				if (!si.ResolveMembers ())
					throw new InternalErrorException ();
				if (!si.DefineMembers ())
					throw new InternalErrorException ();
			}
		}

		protected override bool DoDefineMembers ()
		{
			if (!base.DoDefineMembers ())
				return false;

			FetchMethodDispose ();

			return true;
		}

		protected override void EmitScopeConstructor (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldarg_0);
			ec.ig.Emit (OpCodes.Ldarg_1);
			ec.ig.Emit (OpCodes.Stfld, pc_field.FieldBuilder);
			base.EmitScopeConstructor (ec);
		}

		void FetchMethodDispose ()
		{
			MemberList dispose_list;

			dispose_list = FindMembers (
				CurrentType != null ? CurrentType : TypeBuilder,
				MemberTypes.Method, BindingFlags.Public | BindingFlags.Instance,
				Type.FilterName, "Dispose");

			if (dispose_list.Count != 1)
				throw new InternalErrorException ("Cannot find Dipose() method.");

			dispose_method = (MethodInfo) dispose_list [0];
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

			ToplevelBlock get_block = new ToplevelBlock (Location);
			get_block.AddStatement (new CurrentBlock (this, is_generic));

			Accessor getter = new Accessor (get_block, 0, null, Location);

			Property current = new Property (
				this, type, 0, false, name, null, getter, null, false);
			AddProperty (current);
		}

		void Define_Reset ()
		{
			Method reset = new Method (
				this, null, TypeManager.system_void_expr, Modifiers.PUBLIC,
				false, new MemberName ("Reset", Location),
				Parameters.EmptyReadOnlyParameters, null);
			AddMethod (reset);

			reset.Block = new ToplevelBlock (Location);
			reset.Block.AddStatement (Create_ThrowNotSupported ());
		}

		Statement Create_ThrowNotSupported ()
		{
			TypeExpr ex_type = new TypeExpression (
				TypeManager.not_supported_exception_type, Location);

			return new Throw (new New (ex_type, null, Location), Location);
		}

		ConstructorInfo GetInvalidOperationException ()
		{
			return TypeManager.GetConstructor (
				TypeManager.invalid_operation_exception_type, Type.EmptyTypes);
		}

		protected override ScopeInitializer CreateScopeInitializer ()
		{
			return new IteratorHostInitializer (this);
		}

		protected class IteratorHostInitializer : RootScopeInitializer
		{
			new public readonly IteratorHost Host;
			protected Iterator.State state;

			public IteratorHostInitializer (IteratorHost host)
				: base (host)
			{
				this.Host = host;
			}

			protected override bool DoResolveInternal (EmitContext ec)
			{
				if (this is EnumeratorScopeInitializer)
					state = Iterator.State.Running;
				else if (Host.Iterator.IsEnumerable)
					state = Iterator.State.Uninitialized;
				else
					state = Iterator.State.Running;

				return base.DoResolveInternal (ec);
			}

			protected override void EmitScopeConstructor (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Ldc_I4, (int) state);
				base.EmitScopeConstructor (ec);
			}
		}

		protected class GetEnumeratorMethod : Method
		{
			public IteratorHost Host;

			static MemberName GetMemberName (IteratorHost host, bool is_generic)
			{
				MemberName left;
				if (is_generic) {
					left = new MemberName (
						"System.Collections.Generic.IEnumerable",
						host.generic_args, host.Location);
				} else {
					left = new MemberName (
						"System.Collections.IEnumerable", host.Location);
				}

				return new MemberName (left, "GetEnumerator", host.Location);
			}

			public GetEnumeratorMethod (IteratorHost host, bool is_generic)
				: base (host, null, is_generic ?
					host.generic_enumerator_type : host.enumerator_type,
					0, false, GetMemberName (host, is_generic),
					Parameters.EmptyReadOnlyParameters, null)
			{
				this.Host = host;

				host.AddMethod (this);

				Block = new ToplevelBlock (host.Iterator.Container.Toplevel, null, Location);
				Block.AddStatement (new GetEnumeratorStatement (host, Type));
			}

			public override EmitContext CreateEmitContext (DeclSpace tc, ILGenerator ig)
			{
				EmitContext ec = new EmitContext (
					this, tc, this.ds, Location, ig, MemberType, ModFlags, false);

				ec.CurrentAnonymousMethod = Host.Iterator;
				return ec;
			}

			protected class GetEnumeratorStatement : Statement
			{
				IteratorHost host;
				Expression type;

				ExpressionStatement initializer;
				Expression cast;
				MethodInfo ce;

				public GetEnumeratorStatement (IteratorHost host, Expression type)
				{
					this.host = host;
					this.type = type;
					loc = host.Location;
				}

				public override bool Resolve (EmitContext ec)
				{
					type = type.ResolveAsTypeTerminal (ec, false);
					if ((type == null) || (type.Type == null))
						return false;

					initializer = host.GetEnumeratorInitializer (ec);
					if (initializer == null)
						return false;

					cast = new ClassCast (initializer, type.Type);

					ce = TypeManager.int_interlocked_compare_exchange;

					ec.CurrentBranching.CurrentUsageVector.Goto ();
					return true;
				}

				protected override void DoEmit (EmitContext ec)
				{
					ILGenerator ig = ec.ig;
					Label label_init = ig.DefineLabel ();

					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Ldflda, host.PC.FieldBuilder);
					ig.Emit (OpCodes.Ldc_I4, (int) Iterator.State.Running);
					ig.Emit (OpCodes.Ldc_I4, (int) Iterator.State.Uninitialized);
					ig.Emit (OpCodes.Call, ce);

					ig.Emit (OpCodes.Ldc_I4, (int) Iterator.State.Uninitialized);
					ig.Emit (OpCodes.Bne_Un, label_init);

					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Ret);

					ig.MarkLabel (label_init);

					initializer.EmitStatement (ec);
					cast.Emit (ec);
					ig.Emit (OpCodes.Ret);
				}
			}
		}

		protected class DisposeMethod : Method
		{
			public IteratorHost Host;

			public DisposeMethod (IteratorHost host)
				: base (host, null, TypeManager.system_void_expr, Modifiers.PUBLIC,
					false, new MemberName ("Dispose", host.Location),
					Parameters.EmptyReadOnlyParameters, null)
			{
				this.Host = host;

				host.AddMethod (this);

				Block = new ToplevelBlock (host.Iterator.Block, null, Location);
				Block.AddStatement (new DisposeMethodStatement (Host.Iterator));

				Report.Debug (64, "DISPOSE METHOD", host, Block);
			}

			public override EmitContext CreateEmitContext (DeclSpace tc, ILGenerator ig)
			{
				EmitContext ec = new EmitContext (
					this, tc, this.ds, Location, ig, MemberType, ModFlags, false);

				ec.CurrentAnonymousMethod = Host.Iterator;
				return ec;
			}

			protected class DisposeMethodStatement : Statement
			{
				Iterator iterator;

				public DisposeMethodStatement (Iterator iterator)
				{
					this.iterator = iterator;
					this.loc = iterator.Location;
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
		}

		protected ScopeInitializer GetEnumeratorInitializer (EmitContext ec)
		{
			ScopeInitializer init = new EnumeratorScopeInitializer (this);
			if (init.Resolve (ec) == null)
				throw new InternalErrorException ();
			return init;
		}

		protected class EnumeratorScopeInitializer : IteratorHostInitializer
		{
			IteratorHost host;

			public EnumeratorScopeInitializer (IteratorHost host)
				: base (host)
			{
				this.host = host;
			}

			protected override bool DoResolveInternal (EmitContext ec)
			{
				type = host.IsGeneric ? host.CurrentType : host.TypeBuilder;
				return base.DoResolveInternal (ec);
			}

			protected override void DoEmit (EmitContext ec)
			{
				DoEmitInstance (ec);
			}

			protected override bool IsGetEnumerator {
				get { return true; }
			}

			protected override void EmitParameterReference (EmitContext ec,
									CapturedParameter cp)
			{
				ec.ig.Emit (OpCodes.Ldarg_0);
				ec.ig.Emit (OpCodes.Ldfld, cp.Field.FieldBuilder);
			}
		}

		protected class CurrentBlock : Statement {
			IteratorHost host;
			bool is_generic;

			public CurrentBlock (IteratorHost host, bool is_generic)
			{
				this.host = host;
				this.is_generic = is_generic;
				loc = host.Location;
			}

			public override bool Resolve (EmitContext ec)
			{
				ec.CurrentBranching.CurrentUsageVector.Goto ();
				return true;
			}

			protected override void DoEmit (EmitContext ec)
			{
				ILGenerator ig = ec.ig;
				Label label_ok = ig.DefineLabel ();

				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Ldfld, host.PC.FieldBuilder);
				ig.Emit (OpCodes.Ldc_I4, (int) Iterator.State.Running);
				ig.Emit (OpCodes.Bgt, label_ok);

				ig.Emit (OpCodes.Newobj, host.GetInvalidOperationException ());
				ig.Emit (OpCodes.Throw);

				ig.MarkLabel (label_ok);
				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Ldfld, host.CurrentField.FieldBuilder);
				if (!is_generic)
					ig.Emit (OpCodes.Box, host.CurrentField.MemberType);
				ig.Emit (OpCodes.Ret);
			}
		}
	}

	public class Iterator : AnonymousContainer {
		protected readonly ToplevelBlock OriginalBlock;
		protected readonly IMethodData OriginalMethod;
		protected ToplevelBlock block;

		public readonly bool IsEnumerable;
		public readonly bool IsStatic;

		//
		// The state as we generate the iterator
		//
		Label move_next_ok, move_next_error;
		ArrayList resume_points = new ArrayList ();
		int pc;
		
		public readonly Type OriginalIteratorType;
		public readonly IteratorHost IteratorHost;

		public enum State {
			Uninitialized	= -2,
			After,
			Running
		}

		public void EmitYieldBreak (ILGenerator ig)
		{
			ig.Emit (OpCodes.Ldarg_0);
			IntConstant.EmitInt (ig, (int) State.After);
			ig.Emit (OpCodes.Stfld, IteratorHost.PC.FieldBuilder);
			ig.Emit (OpCodes.Br, move_next_error);
		}

		internal void EmitMoveNext (EmitContext ec, Block original_block)
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

			original_block.Emit (ec);

			EmitYieldBreak (ig);

			ig.MarkLabel (dispatcher);

			Label [] labels = new Label [resume_points.Count];
			for (int i = 0; i < labels.Length; i++)
				labels [i] = ((ResumePoint) resume_points [i]).Label;

			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, IteratorHost.PC.FieldBuilder);
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
			ig.Emit (OpCodes.Callvirt, IteratorHost.Dispose);

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
			ig.Emit (OpCodes.Ldfld, IteratorHost.PC.FieldBuilder);
			ig.Emit (OpCodes.Switch, labels);

			ig.Emit (OpCodes.Ldarg_0);
			IntConstant.EmitInt (ig, (int) State.After);
			ig.Emit (OpCodes.Stfld, IteratorHost.PC.FieldBuilder);

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
			ig.Emit (OpCodes.Stfld, IteratorHost.CurrentField.FieldBuilder);

			// increment pc
			pc++;
			ig.Emit (OpCodes.Ldarg_0);
			IntConstant.EmitInt (ig, pc);
			ig.Emit (OpCodes.Stfld, IteratorHost.PC.FieldBuilder);

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
			ig.Emit (OpCodes.Stfld, IteratorHost.PC.FieldBuilder);

			ResumePoint point = new ResumePoint (finally_blocks);
			resume_points.Add (point);
			point.Define (ig);
		}

		public override string ContainerType {
			get { return "iterator"; }
		}

		public override bool IsIterator {
			get { return true; }
		}

		public override RootScopeInfo RootScope {
			get { return IteratorHost; }
		}

		public override ScopeInfo Scope {
			get { return IteratorHost; }
		}

		//
		// Our constructor
		//
		private Iterator (IMethodData m_container, DeclSpace host, GenericMethod generic,
				 int modifiers, Type iterator_type, bool is_enumerable)
			: base (host, generic, m_container.ParameterInfo,
				new ToplevelBlock (m_container.ParameterInfo, m_container.Location),
				m_container.Block, TypeManager.bool_type, modifiers,
				m_container.Location)
		{
			this.OriginalBlock = m_container.Block;
			this.OriginalMethod = m_container;
			this.OriginalIteratorType = iterator_type;
			this.IsEnumerable = is_enumerable;

			Report.Debug (64, "NEW ITERATOR", host, generic, OriginalBlock,
				      Container, Block);

			IteratorHost = new IteratorHost (this);
			Block.CreateIteratorHost (IteratorHost);

			OriginalBlock.ReParent (Container.Toplevel);

			m_container.Block = Container.Toplevel;

			OriginalBlock.MakeIterator (this);
		}

		protected class TestStatement : Statement
		{
			public override bool Resolve (EmitContext ec)
			{
				return true;
			}

			protected override void DoEmit (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Nop);
				ec.ig.Emit (OpCodes.Neg);
				ec.ig.Emit (OpCodes.Pop);
				ec.ig.Emit (OpCodes.Ret);
			}
		}

		public override string GetSignatureForError ()
		{
			return OriginalMethod.GetSignatureForError ();
		}

		public override bool Define (EmitContext ec)
		{
			Report.Debug (64, "RESOLVE ITERATOR", this, Container, Block);

			Parameters parameters = OriginalMethod.ParameterInfo;
			for (int i = 0; i < parameters.Count; i++){
				Parameter.Modifier mod = parameters.ParameterModifier (i);
				if ((mod & (Parameter.Modifier.REF | Parameter.Modifier.OUT)) != 0){
					Report.Error (1623, Location,
						      "Iterators cannot have ref or out parameters");
					return false;
				}

				if ((mod & Parameter.Modifier.ARGLIST) != 0) {
					Report.Error (1636, Location,
						      "__arglist is not allowed in parameter list " +
						      "of iterators");
					return false;
				}

				if (parameters.ParameterType (i).IsPointer) {
					Report.Error (1637, Location,
						      "Iterators cannot have unsafe parameters or " +
						      "yield types");
					return false;
				}
			}

			if ((ModFlags & Modifiers.UNSAFE) != 0) {
				Report.Error (1629, Location, "Unsafe code may not appear in iterators");
				return false;
			}

			if (!base.Define (ec))
				return false;

			Report.Debug (64, "RESOLVE ITERATOR #1", this, method, method.Parent,
				      RootScope, ec);

			if (!RootScope.ResolveType ())
				return false;
			if (!RootScope.ResolveMembers ())
				return false;
			if (!RootScope.DefineMembers ())
				return false;

			ExpressionStatement scope_init = RootScope.GetScopeInitializer (ec);
			Container.AddStatement (new StatementExpression (scope_init));
			Expression cast = new ClassCast (scope_init, OriginalMethod.ReturnType);
			Container.AddStatement (new NoCheckReturn (cast));

			return true;
		}

		protected override Method DoCreateMethodHost (EmitContext ec)
		{
			Report.Debug (128, "CREATE METHOD HOST", this, IteratorHost);

			IteratorHost.CaptureScopes ();

			return new AnonymousMethodMethod (
				this, RootScope, null, TypeManager.system_boolean_expr,
				Modifiers.PUBLIC, new MemberName ("MoveNext", Location),
				Parameters.EmptyReadOnlyParameters);
		}

		public override Expression Resolve (EmitContext ec)
		{
			throw new NotSupportedException ();
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
				return iterator.OriginalBlock.Resolve (ec);
			}

			protected override void DoEmit (EmitContext ec)
			{
				iterator.EmitMoveNext (ec, iterator.Block);
			}
		}

		public Type IteratorType {
			get { return IteratorHost.IteratorType; }
		}

		//
		// This return statement tricks return into not flagging an error for being
		// used in a Yields method
		//
		class NoCheckReturn : Statement {
			public Expression Expr;
		
			public NoCheckReturn (Expression expr)
			{
				Expr = expr;
				loc = expr.Location;
			}

			public override bool Resolve (EmitContext ec)
			{
				Expr = Expr.Resolve (ec);
				if (Expr == null)
					return false;

				ec.CurrentBranching.CurrentUsageVector.Goto ();

				return true;
			}

			protected override void DoEmit (EmitContext ec)
			{
				Expr.Emit (ec);
				ec.ig.Emit (OpCodes.Ret);
			}
		}

		public static Iterator CreateIterator (IMethodData method, DeclSpace parent,
						       GenericMethod generic, int modifiers)
		{
			bool is_enumerable;
			Type iterator_type;

			Type ret = method.ReturnType;
			if (ret == null)
				return null;

			if (!CheckType (ret, out iterator_type, out is_enumerable)) {
				Report.Error (1624, method.Location,
					      "The body of `{0}' cannot be an iterator block " +
					      "because `{1}' is not an iterator interface type",
					      method.GetSignatureForError (),
					      TypeManager.CSharpName (ret));
				return null;
			}

			return new Iterator (method, parent, generic, modifiers,
					     iterator_type, is_enumerable);
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

#if GMCS_SOURCE
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
#endif

			return false;
		}
	}
}

