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

		public Yield (Expression expr, Location l)
		{
			this.expr = expr;
			loc = l;
		}

		public static bool CheckContext (EmitContext ec, Location loc)
		{
			if (ec.InFinally){
				Report.Error (-208, loc, "yield statement can not appear in finally clause");
				return false;
			}
			if (ec.InCatch){
				Report.Error (-209, loc, "yield statement can not appear in the catch clause");
				return false;
			}
			if (ec.InAnonymousMethod){
				Report.Error (-209, loc, "yield statement can not appear inside an anonymoud method");
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
			
			Type iterator_type = IteratorHandler.Current.IteratorType;
			if (expr.Type != iterator_type){
				expr = Convert.ImplicitConversionRequired (ec, expr, iterator_type, loc);
				if (expr == null)
					return false;
			}
			return true;
		}

		protected override bool DoEmit (EmitContext ec)
		{
			IteratorHandler.Current.MarkYield (ec, expr);
			
			return false;
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

			ec.CurrentBranching.CurrentUsageVector.Breaks = FlowReturns.ALWAYS;
			ec.CurrentBranching.CurrentUsageVector.Returns = FlowReturns.ALWAYS;
			return true;
		}

		protected override bool DoEmit (EmitContext ec)
		{
			IteratorHandler.Current.EmitYieldBreak (ec.ig, true);
			return false;
		}
	}

	public class IteratorHandler {
		//
		// Points to the current iterator handler, will be probed by
		// Yield and YieldBreak to get their context information
		//
		public static IteratorHandler Current;
		
		//
		// The typebuilder to the proxy class we create
		//
		TypeBuilder enumerator_proxy_class;
		TypeBuilder enumerable_proxy_class;

		//
		// The type of this iterator, object by default.
		//
		public Type IteratorType;
		
		//
		// The members we create on the proxy class
		//
		MethodBuilder move_next_method;
		MethodBuilder reset_method;
		MethodBuilder get_current_method;
		MethodBuilder dispose_method;
		MethodBuilder getenumerator_method;
		PropertyBuilder current_property;
		ConstructorBuilder enumerator_proxy_constructor;
		ConstructorBuilder enumerable_proxy_constructor;

		//
		// The PC for the state machine.
		//
		FieldBuilder pc_field;

		//
		// The value computed for Current
		//
		FieldBuilder current_field;

		//
		// Used to reference fields on the container class (instance methods)
		//
		public FieldBuilder this_field;
		public FieldBuilder enumerable_this_field;

		//
		// References the parameters
		//

		public FieldBuilder [] parameter_fields;
		FieldBuilder [] enumerable_parameter_fields;
		
		//
		// The state as we generate the iterator
		//
		ArrayList resume_labels = new ArrayList ();
		int pc;
		
		//
		// Context from the original method
		//
		string name;
		TypeContainer container;
		Type return_type;
		Type [] param_types;
		InternalParameters parameters;
		Block original_block;
		Location loc;
		int modifiers;

		static int proxy_count;
		string MakeProxyName ()
		{
			return String.Format ("<Proxy_{0}>", proxy_count++);
		}

		public void EmitYieldBreak (ILGenerator ig, bool add_return)
		{
			ig.Emit (OpCodes.Ldarg_0);
			IntConstant.EmitInt (ig, -1);
			ig.Emit (OpCodes.Stfld, pc_field);
			if (add_return){
				ig.Emit (OpCodes.Ldc_I4_0);
				ig.Emit (OpCodes.Ret);
			}
		}

		void EmitThrowInvalidOp (ILGenerator ig)
		{
			ig.Emit (OpCodes.Newobj, TypeManager.invalid_operation_ctor);
			ig.Emit (OpCodes.Throw);
		}
		
		void Create_MoveNext ()
		{
			move_next_method = enumerator_proxy_class.DefineMethod (
				"System.IEnumerator.MoveNext",
				MethodAttributes.HideBySig | MethodAttributes.NewSlot |
				MethodAttributes.Virtual,
				CallingConventions.HasThis, TypeManager.bool_type, TypeManager.NoTypes);
			enumerator_proxy_class.DefineMethodOverride (move_next_method, TypeManager.bool_movenext_void);

			ILGenerator ig = move_next_method.GetILGenerator ();
			EmitContext ec = new EmitContext (
				container, loc, ig,
				TypeManager.void_type, modifiers);

			Label dispatcher = ig.DefineLabel ();
			ig.Emit (OpCodes.Br, dispatcher);
			Label entry_point = ig.DefineLabel ();
			ig.MarkLabel (entry_point);
			resume_labels.Add (entry_point);
			
			Current = this;
			SymbolWriter sw = CodeGen.SymbolWriter;
			if ((sw != null) && !Location.IsNull (loc) && !Location.IsNull (original_block.EndLocation)) {
				sw.OpenMethod (container, move_next_method, loc, original_block.EndLocation);

				ec.EmitTopBlock (original_block, parameters, loc);

				sw.CloseMethod ();
			} else {
				ec.EmitTopBlock (original_block, parameters, loc);
			}
			Current = null;

			EmitYieldBreak (ig, true);

			//
			// FIXME: Split the switch in blocks that can be consumed by switch.
			//
			ig.MarkLabel (dispatcher);
			
			Label [] labels = new Label [resume_labels.Count];
			resume_labels.CopyTo (labels);
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, pc_field);
			ig.Emit (OpCodes.Switch, labels);
			ig.Emit (OpCodes.Ldc_I4_0); 
			ig.Emit (OpCodes.Ret); 
		}

		// 
		// Invoked when a local variable declaration needs to be mapped to
		// a field in our proxy class
		//
		public FieldBuilder MapVariable (string name, Type t)
		{
			return enumerator_proxy_class.DefineField ("v" + name, t, FieldAttributes.Public);
		}
		
		void Create_Reset ()
		{
			reset_method = enumerator_proxy_class.DefineMethod (
				"System.IEnumerator.Reset",
				MethodAttributes.HideBySig | MethodAttributes.NewSlot |
				MethodAttributes.Virtual,
				CallingConventions.HasThis, TypeManager.void_type, TypeManager.NoTypes);
			enumerator_proxy_class.DefineMethodOverride (reset_method, TypeManager.void_reset_void);
			ILGenerator ig = reset_method.GetILGenerator ();
			EmitThrowInvalidOp (ig);
		}

		void Create_Current ()
		{
			get_current_method = enumerator_proxy_class.DefineMethod (
				"System.IEnumerator.get_Current",
				MethodAttributes.HideBySig | MethodAttributes.SpecialName |
				MethodAttributes.NewSlot | MethodAttributes.Virtual,
				CallingConventions.HasThis, TypeManager.object_type, TypeManager.NoTypes);
			enumerator_proxy_class.DefineMethodOverride (get_current_method, TypeManager.object_getcurrent_void);

			current_property = enumerator_proxy_class.DefineProperty (
				"Current",
				PropertyAttributes.RTSpecialName | PropertyAttributes.SpecialName,
				TypeManager.object_type, null);

			current_property.SetGetMethod (get_current_method);
			
			ILGenerator ig = get_current_method.GetILGenerator ();

			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, pc_field);
			ig.Emit (OpCodes.Ldc_I4_0);
			Label return_current = ig.DefineLabel ();
			ig.Emit (OpCodes.Bgt, return_current);
			EmitThrowInvalidOp (ig);
			
			ig.MarkLabel (return_current);
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, current_field);
			ig.Emit (OpCodes.Ret);
		}

		void Create_Dispose ()
		{
			dispose_method = enumerator_proxy_class.DefineMethod (
				"System.IDisposable.Dispose",
				MethodAttributes.HideBySig | MethodAttributes.SpecialName |
				MethodAttributes.NewSlot | MethodAttributes.Virtual,
				CallingConventions.HasThis, TypeManager.void_type, TypeManager.NoTypes);
			enumerator_proxy_class.DefineMethodOverride (dispose_method, TypeManager.void_dispose_void);
			ILGenerator ig = dispose_method.GetILGenerator (); 

			EmitYieldBreak (ig, false);
			ig.Emit (OpCodes.Ret);
		}
		
		void Create_GetEnumerator ()
		{
			getenumerator_method = enumerable_proxy_class.DefineMethod (
				"IEnumerable.GetEnumerator",
				MethodAttributes.HideBySig | MethodAttributes.SpecialName |
				MethodAttributes.NewSlot | MethodAttributes.Virtual,
				CallingConventions.HasThis, TypeManager.ienumerator_type, TypeManager.NoTypes);

			enumerable_proxy_class.DefineMethodOverride  (getenumerator_method, TypeManager.ienumerable_getenumerator_void);
			ILGenerator ig = getenumerator_method.GetILGenerator ();

			if (enumerable_this_field != null){
				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Ldfld, enumerable_this_field);
			}
			for (int i = 0; i < parameters.Count; i++){
				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Ldfld, enumerable_parameter_fields [i]);
			}
			ig.Emit (OpCodes.Newobj, (ConstructorInfo) enumerator_proxy_constructor);
			ig.Emit (OpCodes.Ret);
		}

		void LoadArgs (ILGenerator ig)
		{
			int count = parameters.Count;
			if ((modifiers & Modifiers.STATIC) == 0)
				count++;

			for (int i = 0; i < count; i++)
				ParameterReference.EmitLdArg (ig, i);
		}
		
		//
		// Called back from Yield
		//
		public void MarkYield (EmitContext ec, Expression expr)
		{
			ILGenerator ig = ec.ig;

			// Store the new current
			ig.Emit (OpCodes.Ldarg_0);
			expr.Emit (ec);
			ig.Emit (OpCodes.Stfld, current_field);

			// increment pc
			pc++;
			ig.Emit (OpCodes.Ldarg_0);
			IntConstant.EmitInt (ig, pc);
			ig.Emit (OpCodes.Stfld, pc_field);
			
			// Return ok.
			ig.Emit (OpCodes.Ldc_I4_1);
			ig.Emit (OpCodes.Ret);
			
			Label resume_point = ig.DefineLabel ();
			ig.MarkLabel (resume_point);
			resume_labels.Add (resume_point);
		}

		void ComputeConstructorTypes (out Type [] constructor_types, out Parameters constructor_parameters)
		{
			bool is_static =  (modifiers & Modifiers.STATIC) != 0;
			
			if (is_static && parameters.Count == 0){
				constructor_types = TypeManager.NoTypes;
				constructor_parameters = Parameters.EmptyReadOnlyParameters;
				return;
			}

			int count = (is_static ? 0 : 1) + parameters.Count;
			constructor_types = new Type [count];
			Parameter [] pars = new Parameter [count];
			constructor_parameters = new Parameters (pars, null, loc);
			
			int i = 0;
			if (!is_static){
				constructor_types [0] = container.TypeBuilder;

				Parameter THIS = new Parameter (
					new TypeExpr (container.TypeBuilder, loc), "this", Parameter.Modifier.NONE, null);
				pars [0] = THIS;
				i++;
			}

			for (int j = 0; j < parameters.Count; j++, i++){
				Type partype = parameters.ParameterType (j);
				
				pars [i] = new Parameter (new TypeExpr (partype, loc),
							  parameters.ParameterName (j),
							  Parameter.Modifier.NONE, null);
				constructor_types [i] = partype;
			}
		}
		
		//
		// Creates the IEnumerator Proxy class
		//
		void MakeEnumeratorProxy ()
		{
			Type [] proxy_base_interfaces = new Type [2];
			proxy_base_interfaces [0] = TypeManager.ienumerator_type;
			proxy_base_interfaces [1] = TypeManager.idisposable_type;
			TypeBuilder container_builder = container.TypeBuilder;

			//
			// Create the class
			//
			enumerator_proxy_class = container_builder.DefineNestedType (
				MakeProxyName (), TypeAttributes.AutoLayout | TypeAttributes.Class |TypeAttributes.NestedPublic,
				TypeManager.object_type, proxy_base_interfaces);

			TypeManager.RegisterBuilder (enumerator_proxy_class, proxy_base_interfaces);

			//
			// Define our fields
			//
			pc_field = enumerator_proxy_class.DefineField ("PC", TypeManager.int32_type, FieldAttributes.Private);
			current_field = enumerator_proxy_class.DefineField ("current", IteratorType, FieldAttributes.Private);
			if ((modifiers & Modifiers.STATIC) == 0)
				this_field = enumerator_proxy_class.DefineField ("THIS", container.TypeBuilder, FieldAttributes.Private);

			parameter_fields = new FieldBuilder [parameters.Count];
			for (int i = 0; i < parameters.Count; i++){
				parameter_fields [i] = enumerator_proxy_class.DefineField (
					String.Format ("p{0}_{1}", i, parameters.ParameterName (i)),
					parameters.ParameterType (i), FieldAttributes.Private);
			}
			
			//
			// Define a constructor 
			//
			// FIXME: currently its parameterless
			Type [] constructor_types;
			Parameters constructor_parameters;

			ComputeConstructorTypes (out constructor_types, out constructor_parameters);
			
			enumerator_proxy_constructor = enumerator_proxy_class.DefineConstructor (
				MethodAttributes.Public | MethodAttributes.HideBySig |
				MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
				CallingConventions.HasThis, constructor_types);
			InternalParameters parameter_info = new InternalParameters (constructor_types, constructor_parameters);
			TypeManager.RegisterMethod (enumerator_proxy_constructor, parameter_info, constructor_types);

			//
			// Our constructor
			//
			ILGenerator ig = enumerator_proxy_constructor.GetILGenerator ();
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Call, TypeManager.object_ctor);

			int arg_start;
			if (this_field != null){
				arg_start = 2;
				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Ldarg_1);
				ig.Emit (OpCodes.Stfld, this_field);
			} else {
				arg_start = 1;
			}
			for (int i = 0; i < parameters.Count; i++){
				ig.Emit (OpCodes.Ldarg_0);
				ParameterReference.EmitLdArg (ig, i + arg_start);
				ig.Emit (OpCodes.Stfld, parameter_fields [i]);
			}
			ig.Emit (OpCodes.Ret);
		}

		//
		// Creates the IEnumerable proxy class
		//
		void MakeEnumerableProxy ()
		{
			TypeBuilder container_builder = container.TypeBuilder;
			Type [] proxy_base_interfaces = new Type [1];
			proxy_base_interfaces [0] = TypeManager.ienumerable_type;

			//
			// Creates the Enumerable proxy class.
			//
			enumerable_proxy_class = container_builder.DefineNestedType (
				MakeProxyName (), TypeAttributes.AutoLayout | TypeAttributes.Class |TypeAttributes.NestedPublic,
				TypeManager.object_type, proxy_base_interfaces);

			//
			// Constructor
			//
			Type [] constructor_types;
			Parameters constructor_parameters;

			ComputeConstructorTypes (out constructor_types, out constructor_parameters);
			if ((modifiers & Modifiers.STATIC) == 0){
				enumerable_this_field = enumerable_proxy_class.DefineField (
					"THIS", container.TypeBuilder, FieldAttributes.Private);
			}
			enumerable_parameter_fields = new FieldBuilder [parameters.Count];
			for (int i = 0; i < parameters.Count; i++){
				enumerable_parameter_fields [i] = enumerable_proxy_class.DefineField (
					String.Format ("p{0}_{1}", i, parameters.ParameterName (i)),
					parameters.ParameterType (i), FieldAttributes.Private);
			}
			
			enumerable_proxy_constructor = enumerable_proxy_class.DefineConstructor (
				MethodAttributes.Public | MethodAttributes.HideBySig |
				MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
				CallingConventions.HasThis, constructor_types);
			InternalParameters parameter_info = new InternalParameters (constructor_types, constructor_parameters);
			TypeManager.RegisterMethod (enumerable_proxy_constructor, parameter_info, constructor_types);
			
			ILGenerator ig = enumerable_proxy_constructor.GetILGenerator ();
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Call, TypeManager.object_ctor);

			int first_arg;
			if (enumerable_this_field != null){
				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Ldarg_1);
				ig.Emit (OpCodes.Stfld, enumerable_this_field);
				first_arg = 2;
			} else
				first_arg = 1;
			
			for (int i = 0; i < parameters.Count; i++){
				ig.Emit (OpCodes.Ldarg_0);
				ParameterReference.EmitLdArg (ig, i + first_arg);
				ig.Emit (OpCodes.Stfld, enumerable_parameter_fields [i]);
			}
			ig.Emit (OpCodes.Ret);
		}

		//
		// Populates the Enumerator Proxy class
		//
		void PopulateProxy ()
		{
			RootContext.RegisterHelperClass (enumerator_proxy_class);
			
			Create_MoveNext ();
			Create_Reset ();
			Create_Current ();
			Create_Dispose ();

			if (return_type == TypeManager.ienumerable_type){
				Create_GetEnumerator ();
				RootContext.RegisterHelperClass (enumerable_proxy_class);
			}
		}
		

		//
		// This is invoked by the EmitCode hook
		//
		void SetupIterator ()
		{
			PopulateProxy ();
		}

		//
		// Our constructor
		//
		public IteratorHandler (string name, TypeContainer container, Type return_type, Type [] param_types,
					InternalParameters parameters, int modifiers, Location loc)
		{
			this.name = name;
			this.container = container;
			this.return_type = return_type;
			this.param_types = param_types;
			this.parameters = parameters;
			this.modifiers = modifiers;
			this.loc = loc;

			IteratorType = TypeManager.object_type;
			
			RootContext.EmitCodeHook += new RootContext.Hook (SetupIterator);
		}

		//
		// This class is just an expression that evaluates to a type, and the
		// type is our internal proxy class.  Used in the generated new body
		// of the original method
		//
		class NewInnerType : Expression {
			IteratorHandler handler;
			
			public NewInnerType (IteratorHandler handler, Location l) 
			{
				this.handler = handler;
				eclass = ExprClass.Value;
				loc = l;
			}

			public override Expression DoResolve (EmitContext ec)
			{
				// Create the proxy class type.
				handler.MakeEnumeratorProxy ();

				if (handler.return_type == TypeManager.ienumerable_type)
					handler.MakeEnumerableProxy ();

				type = handler.return_type;
				return this;
			}

			public override Expression ResolveAsTypeStep (EmitContext ec)
			{
				return DoResolve (ec);
			}

			public override void Emit (EmitContext ec)
			{
				handler.LoadArgs (ec.ig);
				
				if (handler.return_type == TypeManager.ienumerable_type)
					ec.ig.Emit (OpCodes.Newobj, (ConstructorInfo) handler.enumerable_proxy_constructor);
				else 
					ec.ig.Emit (OpCodes.Newobj, (ConstructorInfo) handler.enumerator_proxy_constructor);
			}
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
		
		//
		// Returns the new block for the method, or null on failure
		//
		public Block Setup (Block block)
		{
			if (return_type != TypeManager.ienumerator_type &&
			    return_type != TypeManager.ienumerable_type){
				Report.Error (
					-205, loc, String.Format (
						"The method `{0}' contains a yield statement, but has an invalid return type for an iterator",
						name));
				return null;
			}

			for (int i = 0; i < parameters.Count; i++){
				Parameter.Modifier mod = parameters.ParameterModifier (i);
				if ((mod & (Parameter.Modifier.REF | Parameter.Modifier.OUT)) != 0){
					Report.Error (-207, loc, String.Format (
							      "Parameter {0} of `{1}' is {2} and not allowed for an iterator method",
							      i+1, name, parameters.ParameterDesc (i)));
					return null;
				}
			}

			original_block = block;
			Block b = new Block (null);

			// return new InnerClass ()
			b.AddStatement (new NoCheckReturn (new NewInnerType (this, loc), loc));
			return b;
		}
	}
}

