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
//    Emit calls to parent object constructor.
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
		public Expression expr;
		bool in_exc;
		
		public Yield (Expression expr, Location l)
		{
			this.expr = expr;
			loc = l;
		}

		public static bool CheckContext (EmitContext ec, Location loc)
		{
			if (ec.CurrentBranching.InFinally (true)){
				Report.Error (1625, loc, "Cannot yield in the body of a " +
					      "finally clause");
				return false;
			}
			if (ec.CurrentBranching.InCatch ()){
				Report.Error (1631, loc, "Cannot yield in the body of a " +
					      "catch clause");
				return false;
			}
			if (ec.InAnonymousMethod){
				Report.Error (1621, loc, "yield statement can not appear " +
					      "inside an anonymoud method");
				return false;
			}

			//
			// FIXME: Missing check for Yield inside try block that contains catch clauses
			//
			return true;
		}
		
		public override bool Resolve (EmitContext ec)
		{
			expr = expr.Resolve (ec);
			if (expr == null)
				return false;
			if (!CheckContext (ec, loc))
				return false;

			in_exc = ec.CurrentBranching.InTryOrCatch (false);
			Type iterator_type = ec.CurrentIterator.IteratorType;
			if (expr.Type != iterator_type){
				expr = Convert.ImplicitConversionRequired (ec, expr, iterator_type, loc);
				if (expr == null)
					return false;
			}
			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			ec.CurrentIterator.MarkYield (ec, expr, in_exc);
		}
	}

	public class YieldBreak : Statement {

		public YieldBreak (Location l)
		{
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			if (!Yield.CheckContext (ec, loc))
				return false;

			ec.CurrentBranching.CurrentUsageVector.Goto ();
			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			ec.CurrentIterator.EmitYieldBreak (ec.ig, true);
		}
	}

	public class Iterator : Class {
		string original_name;
		Block original_block;
		Block block;

		Type iterator_type;
		TypeExpr iterator_type_expr;
		bool is_enumerable;
		bool is_static;

		Hashtable fields;

		//
		// The state as we generate the iterator
		//
		ArrayList resume_labels = new ArrayList ();
		int pc;
		
		//
		// Context from the original method
		//
		TypeContainer container;
		Type return_type;
		Type [] param_types;
		InternalParameters parameters;

		static int proxy_count;

		public void EmitYieldBreak (ILGenerator ig, bool add_return)
		{
			ig.Emit (OpCodes.Ldarg_0);
			IntConstant.EmitInt (ig, -1);
			ig.Emit (OpCodes.Stfld, pc_field.FieldBuilder);
			if (add_return){
				ig.Emit (OpCodes.Ldc_I4_0);
				ig.Emit (OpCodes.Ret);
			}
		}

		public void EmitMoveNext (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			Label dispatcher = ig.DefineLabel ();
			ig.Emit (OpCodes.Br, dispatcher);
			Label entry_point = ig.DefineLabel ();
			ig.MarkLabel (entry_point);
			resume_labels.Add (entry_point);

			ec.EmitTopBlock (original_block, parameters, Location);

			EmitYieldBreak (ig, true);

			//
			// FIXME: Split the switch in blocks that can be consumed
			//        by switch.
			//
			ig.MarkLabel (dispatcher);

			Label [] labels = new Label [resume_labels.Count];
			resume_labels.CopyTo (labels);
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, pc_field.FieldBuilder);
			ig.Emit (OpCodes.Switch, labels);
			ig.Emit (OpCodes.Ldc_I4_0); 
			ig.Emit (OpCodes.Ret);
		}

		// 
		// Invoked when a local variable declaration needs to be mapped to
		// a field in our proxy class
		//
		// Prefixes registered:
		//   v_   for EmitContext.MapVariable
		//   s_   for Storage
		//
		public FieldBuilder MapVariable (string pfx, string name, Type t)
		{
			string full_name = pfx + name;
			FieldBuilder fb = (FieldBuilder) fields [full_name];
			if (fb != null)
				return fb;

			fb = TypeBuilder.DefineField (full_name, t, FieldAttributes.Public);
			fields.Add (full_name, fb);
			return fb;
		}
		
		//
		// Called back from Yield
		//
		public void MarkYield (EmitContext ec, Expression expr, bool in_exc)
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
			ig.Emit (OpCodes.Ldc_I4_1);

			// Find out how to "leave"
			if (in_exc || !ec.IsLastStatement)
				ig.Emit (OpCodes.Stloc, ec.TemporaryReturn ());

			if (in_exc){
				ec.NeedReturnLabel ();
				ig.Emit (OpCodes.Leave, ec.ReturnLabel);
			} else if (ec.IsLastStatement){
				ig.Emit (OpCodes.Ret);
			} else {
				ec.NeedReturnLabel ();
				ig.Emit (OpCodes.Br, ec.ReturnLabel);
			}
			
			Label resume_point = ig.DefineLabel ();
			ig.MarkLabel (resume_point);
			resume_labels.Add (resume_point);
		}

		private static string MakeProxyName (string name)
		{
			int pos = name.LastIndexOf ('.');
			if (pos > 0)
				name = name.Substring (pos + 1);

			return "<" + name + ">__" + (proxy_count++);
		}

		//
		// Our constructor
		//
		public Iterator (TypeContainer container, string name, Type return_type,
				 Type [] param_types, InternalParameters parameters,
				 int modifiers, Block block, Location loc)
			: base (container.NamespaceEntry, container, MakeProxyName (name),
				Modifiers.PRIVATE, null, loc)
		{
			this.container = container;
			this.return_type = return_type;
			this.param_types = param_types;
			this.parameters = parameters;
			this.original_name = name;
			this.original_block = block;
			this.block = new Block (null);

			fields = new Hashtable ();

			is_static = (modifiers & Modifiers.STATIC) != 0;
		}

		public bool Define ()
		{
			if (!CheckType (return_type)) {
				Report.Error (
					1624, Location,
					"The body of `{0}' cannot be an iterator block " +
					"because '{1}' is not an iterator interface type",
					original_name, TypeManager.CSharpName (return_type));
				return false;
			}

			for (int i = 0; i < parameters.Count; i++){
				Parameter.Modifier mod = parameters.ParameterModifier (i);
				if ((mod & (Parameter.Modifier.REF | Parameter.Modifier.OUT)) != 0){
					Report.Error (
						1623, Location,
						"Iterators cannot have ref or out parameters");
					return false;
				}
			}

			ArrayList list = new ArrayList ();
			if (is_enumerable)
				list.Add (new TypeExpression (
						  TypeManager.ienumerable_type, Location));
			list.Add (new TypeExpression (TypeManager.ienumerator_type, Location));
			list.Add (new TypeExpression (TypeManager.idisposable_type, Location));

			iterator_type_expr = new TypeExpression (iterator_type, Location);

			container.AddIterator (this);

			Bases = list;
			return true;
		}

		//
		// Returns the new block for the method, or null on failure
		//
		protected override bool DoDefineType ()
		{
			Define_Fields ();
			Define_Constructor ();
			Define_Current ();
			Define_MoveNext ();
			Define_Reset ();
			Define_Dispose ();

			if (is_enumerable)
				Define_GetEnumerator ();

			Create_Block ();

			return true;
		}


		Field pc_field;
		Field current_field;
		public Field this_field;
		public Field[] parameter_fields;

		void Create_Block ()
		{
			int first = is_static ? 0 : 1;

			ArrayList args = new ArrayList ();
			if (!is_static) {
				Type t = container.TypeBuilder;
				args.Add (new Argument (
					new SimpleParameterReference (t, 0, Location),
					Argument.AType.Expression));
			}

			for (int i = 0; i < parameters.Count; i++) {
				Type t = parameters.ParameterType (i);
				args.Add (new Argument (
					new SimpleParameterReference (t, first + i, Location),
					Argument.AType.Expression));
			}

			Expression new_expr = new New (
				new TypeExpression (TypeBuilder, Location), args, Location);

			block.AddStatement (new NoCheckReturn (new_expr, Location));
		}

		void Define_Fields ()
		{
			Location loc = Location.Null;

			pc_field = new Field (
				TypeManager.system_int32_expr, Modifiers.PRIVATE, "PC",
				null, null, loc);
			AddField (pc_field);

			current_field = new Field (
				iterator_type_expr, Modifiers.PRIVATE, "current",
				null, null, loc);
			AddField (current_field);

			if (!is_static) {
				this_field = new Field (
					new TypeExpression (container.TypeBuilder, Location),
					Modifiers.PRIVATE, "this", null, null, loc);
				AddField (this_field);
			}

			parameter_fields = new Field [parameters.Count];
			for (int i = 0; i < parameters.Count; i++) {
				string fname = String.Format (
					"field{0}_{1}", i, parameters.ParameterName (i));

				parameter_fields [i] = new Field (
					new TypeExpression (parameters.ParameterType (i), loc),
					Modifiers.PRIVATE, fname, null, null, loc);
				AddField (parameter_fields [i]);
			}
		}

		void Define_Constructor ()
		{
			Parameters ctor_params;

			if (!is_static) {
				Parameter this_param = new Parameter (
					new TypeExpression (container.TypeBuilder, Location),
					"this", Parameter.Modifier.NONE, null);

				Parameter[] old_fixed = parameters.Parameters.FixedParameters;
				Parameter[] fixed_params;
				if (old_fixed != null) {
					fixed_params = new Parameter [old_fixed.Length + 1];
					old_fixed.CopyTo (fixed_params, 1);
				} else {
					fixed_params = new Parameter [1];
				}
				fixed_params [0] = this_param;

				ctor_params = new Parameters (
					fixed_params, parameters.Parameters.ArrayParameter,
					Location);
			} else
				ctor_params = parameters.Parameters;

			Constructor ctor = new Constructor (
				this, Name, Modifiers.PUBLIC, ctor_params,
				new ConstructorBaseInitializer (
					null, Parameters.EmptyReadOnlyParameters, Location),
				Location);
			AddConstructor (ctor);

			Block block = ctor.Block = new Block (null);

			if (!is_static) {
				Type t = container.TypeBuilder;

				Assign assign = new Assign (
					new FieldExpression (this_field),
					new SimpleParameterReference (t, 1, Location),
					Location);

				block.AddStatement (new StatementExpression (assign, Location));
			}

			int first = is_static ? 1 : 2;

			for (int i = 0; i < parameters.Count; i++) {
				Type t = parameters.ParameterType (i);

				Assign assign = new Assign (
					new FieldExpression (parameter_fields [i]),
					new SimpleParameterReference (t, first + i, Location),
					Location);

				block.AddStatement (new StatementExpression (assign, Location));
			}
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

		void Define_Current ()
		{
			Block get_block = new Block (null);

			get_block.AddStatement (new If (
				new Binary (
					Binary.Operator.LessThanOrEqual,
					new FieldExpression (pc_field),
					new IntLiteral (0), Location),
				Create_ThrowInvalidOperation (),
				new Return (
					new FieldExpression (current_field), Location),
				Location));

			Accessor getter = new Accessor (get_block, null);

			Property current = new Property (
				this, iterator_type_expr, Modifiers.PUBLIC,
				false, "Current", null, getter, null, Location);
			AddProperty (current);
		}

		void Define_MoveNext ()
		{
			Method move_next = new Method (
				this, TypeManager.system_boolean_expr,
				Modifiers.PUBLIC, false, "MoveNext",
				Parameters.EmptyReadOnlyParameters, null,
				Location.Null);
			AddMethod (move_next);

			Block block = move_next.Block = new Block (null);

			MoveNextMethod inline = new MoveNextMethod (this, Location);
			block.AddStatement (inline);
		}

		void Define_GetEnumerator ()
		{
			Method get_enumerator = new Method (
				this,
				new TypeExpression (TypeManager.ienumerator_type, Location),
				Modifiers.PUBLIC, false, "GetEnumerator",
				Parameters.EmptyReadOnlyParameters, null,
				Location.Null);
			AddMethod (get_enumerator);

			get_enumerator.Block = new Block (null);

			This the_this = new This (block, Location);
			get_enumerator.Block.AddStatement (new Return (the_this, Location));
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
				ParameterReference.EmitLdArg (ec.ig, idx);
			}
		}

		protected class FieldExpression : Expression
		{
			Field field;

			public FieldExpression (Field field)
			{
				this.field = field;
			}

			public override Expression DoResolve (EmitContext ec)
			{
				FieldExpr fexpr = new FieldExpr (field.FieldBuilder, loc);
				fexpr.InstanceExpression = ec.GetThis (loc);
				return fexpr.Resolve (ec);
			}

			public override void Emit (EmitContext ec)
			{
				throw new InvalidOperationException ();
			}
		}

		public class MoveNextMethod : Statement {
			Iterator iterator;

			public MoveNextMethod (Iterator iterator, Location loc)
			{
				this.loc = loc;
				this.iterator = iterator;
			}

			public override bool Resolve (EmitContext ec)
			{
				ec.CurrentBranching.CurrentUsageVector.Return ();
				return true;
			}

			protected override void DoEmit (EmitContext ec)
			{
				int code_flags = Modifiers.METHOD_YIELDS;
				if (iterator.is_static)
					code_flags |= Modifiers.STATIC;

				EmitContext new_ec = new EmitContext (
					iterator.container, loc, ec.ig,
					TypeManager.int32_type, code_flags);

				new_ec.CurrentIterator = iterator;

				iterator.EmitMoveNext (new_ec);
			}
		}

		protected class DoYieldBreak : Statement
		{
			Iterator iterator;
			bool add_return;

			public DoYieldBreak (Iterator iterator, bool add_return,
					     Location loc)
			{
				this.iterator = iterator;
				this.add_return = add_return;
				this.loc = loc;
			}

			public override bool Resolve (EmitContext ec)
			{
				if (add_return)
					ec.CurrentBranching.CurrentUsageVector.Return ();
				return true;
			}

			protected override void DoEmit (EmitContext ec)
			{
			       iterator.EmitYieldBreak (ec.ig, add_return);
			}
		}

		void Define_Reset ()
		{
			Method reset = new Method (
				this, TypeManager.system_void_expr, Modifiers.PUBLIC,
				false, "Reset", Parameters.EmptyReadOnlyParameters,
				null, Location);
			AddMethod (reset);

			reset.Block = new Block (null);
			reset.Block.AddStatement (Create_ThrowNotSupported ());
		}

		void Define_Dispose ()
		{
			Method dispose = new Method (
				this, TypeManager.system_void_expr, Modifiers.PUBLIC,
				false, "Dispose", Parameters.EmptyReadOnlyParameters,
				null, Location);
			AddMethod (dispose);

			dispose.Block = new Block (null);
			dispose.Block.AddStatement (new DoYieldBreak (this, false, Location));
			dispose.Block.AddStatement (new Return (null, Location));
		}

		public Block Block {
			get { return block; }
		}

		public Type IteratorType {
			get { return iterator_type; }
		}

		//
		// This return statement tricks return into not flagging an error for being
		// used in a Yields method
		//
		class NoCheckReturn : Return {
			public NoCheckReturn (Expression expr, Location loc) : base (expr, loc)
			{
			}

			public override bool Resolve (EmitContext ec)
			{
				ec.InIterator = false;
				bool ret_val = base.Resolve (ec);
				ec.InIterator = true;

				return ret_val;
			}
		}

		bool CheckType (Type t)
		{
			if (t == TypeManager.ienumerable_type) {
				iterator_type = TypeManager.object_type;
				is_enumerable = true;
				return true;
			} else if (t == TypeManager.ienumerator_type) {
				iterator_type = TypeManager.object_type;
				is_enumerable = false;
				return true;
			}

			return false;
		}
	}
}

