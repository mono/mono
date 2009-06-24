//
// delegate.cs: Delegate Handler
//
// Authors:
//     Ravi Pratap (ravi@ximian.com)
//     Miguel de Icaza (miguel@ximian.com)
//     Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001 Ximian, Inc (http://www.ximian.com)
// Copyright 2003-2008 Novell, Inc (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Mono.CSharp {

	/// <summary>
	///   Holds Delegates
	/// </summary>
	public class Delegate : DeclSpace, IMemberContainer
	{
 		FullNamedExpression ReturnType;
		public ParametersCompiled      Parameters;

		public ConstructorBuilder ConstructorBuilder;
		public MethodBuilder      InvokeBuilder;
		public MethodBuilder      BeginInvokeBuilder;
		public MethodBuilder      EndInvokeBuilder;
		
		Type ret_type;

		static string[] attribute_targets = new string [] { "type", "return" };
		
		Expression instance_expr;
		MethodBase delegate_method;
		ReturnParameter return_attributes;

		MemberCache member_cache;

		const MethodAttributes mattr = MethodAttributes.Public | MethodAttributes.HideBySig |
			MethodAttributes.Virtual | MethodAttributes.NewSlot;

		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
		        Modifiers.UNSAFE |
			Modifiers.PRIVATE;

 		public Delegate (NamespaceEntry ns, DeclSpace parent, FullNamedExpression type,
				 int mod_flags, MemberName name, ParametersCompiled param_list,
				 Attributes attrs)
			: base (ns, parent, name, attrs)

		{
			this.ReturnType = type;
			ModFlags        = Modifiers.Check (AllowedModifiers, mod_flags,
							   IsTopLevel ? Modifiers.INTERNAL :
							   Modifiers.PRIVATE, name.Location);
			Parameters      = param_list;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.Target == AttributeTargets.ReturnValue) {
				if (return_attributes == null)
					return_attributes = new ReturnParameter (InvokeBuilder, Location);

				return_attributes.ApplyAttributeBuilder (a, cb, pa);
				return;
			}

			base.ApplyAttributeBuilder (a, cb, pa);
		}

		public override TypeBuilder DefineType ()
		{
			if (TypeBuilder != null)
				return TypeBuilder;

			if (IsTopLevel) {
				if (TypeManager.NamespaceClash (Name, Location))
					return null;
				
				ModuleBuilder builder = Module.Builder;

				TypeBuilder = builder.DefineType (
					Name, TypeAttr, TypeManager.multicast_delegate_type);
			} else {
				TypeBuilder builder = Parent.TypeBuilder;

				string name = Name.Substring (1 + Name.LastIndexOf ('.'));
				TypeBuilder = builder.DefineNestedType (
					name, TypeAttr, TypeManager.multicast_delegate_type);
			}

			TypeManager.AddUserType (this);

#if GMCS_SOURCE
			if (IsGeneric) {
				string[] param_names = new string [TypeParameters.Length];
				for (int i = 0; i < TypeParameters.Length; i++)
					param_names [i] = TypeParameters [i].Name;

				GenericTypeParameterBuilder[] gen_params;
				gen_params = TypeBuilder.DefineGenericParameters (param_names);

				int offset = CountTypeParameters - CurrentTypeParameters.Length;
				for (int i = offset; i < gen_params.Length; i++)
					CurrentTypeParameters [i - offset].Define (gen_params [i]);

				foreach (TypeParameter type_param in CurrentTypeParameters) {
					if (!type_param.Resolve (this))
						return null;
				}

				Expression current = new SimpleName (
					MemberName.Basename, TypeParameters, Location);
				current = current.ResolveAsTypeTerminal (this, false);
				if (current == null)
					return null;

				CurrentType = current.Type;
			}
#endif

			return TypeBuilder;
		}

 		public override bool Define ()
		{
			if (IsGeneric) {
				foreach (TypeParameter type_param in TypeParameters) {
					if (!type_param.Resolve (this))
						return false;
				}

				foreach (TypeParameter type_param in TypeParameters) {
					if (!type_param.DefineType (this))
						return false;
				}
			}

			member_cache = new MemberCache (TypeManager.multicast_delegate_type, this);

			// FIXME: POSSIBLY make this static, as it is always constant
			//
			Type [] const_arg_types = new Type [2];
			const_arg_types [0] = TypeManager.object_type;
			const_arg_types [1] = TypeManager.intptr_type;

			const MethodAttributes ctor_mattr = MethodAttributes.RTSpecialName | MethodAttributes.SpecialName |
				MethodAttributes.HideBySig | MethodAttributes.Public;

			ConstructorBuilder = TypeBuilder.DefineConstructor (ctor_mattr,
									    CallingConventions.Standard,
									    const_arg_types);

			ConstructorBuilder.DefineParameter (1, ParameterAttributes.None, "object");
			ConstructorBuilder.DefineParameter (2, ParameterAttributes.None, "method");
			//
			// HACK because System.Reflection.Emit is lame
			//
			IParameterData [] fixed_pars = new IParameterData [] {
				new ParameterData ("object", Parameter.Modifier.NONE),
				new ParameterData ("method", Parameter.Modifier.NONE)
			};

			AParametersCollection const_parameters = new ParametersImported (
				fixed_pars,
				new Type[] { TypeManager.object_type, TypeManager.intptr_type });
			
			TypeManager.RegisterMethod (ConstructorBuilder, const_parameters);
			member_cache.AddMember (ConstructorBuilder, this);
			
			ConstructorBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			//
			// Here the various methods like Invoke, BeginInvoke etc are defined
			//
			// First, call the `out of band' special method for
			// defining recursively any types we need:
			
			if (!Parameters.Resolve (this))
				return false;

			//
			// Invoke method
			//

			// Check accessibility
			foreach (Type partype in Parameters.Types){
				if (!IsAccessibleAs (partype)) {
					Report.SymbolRelatedToPreviousError (partype);
					Report.Error (59, Location,
						      "Inconsistent accessibility: parameter type `{0}' is less accessible than delegate `{1}'",
						      TypeManager.CSharpName (partype),
						      GetSignatureForError ());
					return false;
				}
			}
			
			ReturnType = ReturnType.ResolveAsTypeTerminal (this, false);
			if (ReturnType == null)
				return false;

			ret_type = ReturnType.Type;
            
			if (!IsAccessibleAs (ret_type)) {
				Report.SymbolRelatedToPreviousError (ret_type);
				Report.Error (58, Location,
					      "Inconsistent accessibility: return type `" +
					      TypeManager.CSharpName (ret_type) + "' is less " +
					      "accessible than delegate `" + GetSignatureForError () + "'");
				return false;
			}

			CheckProtectedModifier ();

			if (RootContext.StdLib && TypeManager.IsSpecialType (ret_type)) {
				Method.Error1599 (Location, ret_type);
				return false;
			}

			TypeManager.CheckTypeVariance (ret_type, Variance.Covariant, this);

			//
			// We don't have to check any others because they are all
			// guaranteed to be accessible - they are standard types.
			//
			
  			CallingConventions cc = Parameters.CallingConvention;

 			InvokeBuilder = TypeBuilder.DefineMethod ("Invoke", 
 								  mattr,		     
 								  cc,
 								  ret_type,		     
 								  Parameters.GetEmitTypes ());
			
			InvokeBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			TypeManager.RegisterMethod (InvokeBuilder, Parameters);
			member_cache.AddMember (InvokeBuilder, this);

			if (TypeManager.iasyncresult_type != null && TypeManager.asynccallback_type != null) {
				DefineAsyncMethods (cc);
			}

			return true;
		}

		void DefineAsyncMethods (CallingConventions cc)
		{
			//
			// BeginInvoke
			//
			ParametersCompiled async_parameters = ParametersCompiled.MergeGenerated (Parameters, false,
				new Parameter [] {
					new Parameter (null, "callback", Parameter.Modifier.NONE, null, Location),
					new Parameter (null, "object", Parameter.Modifier.NONE, null, Location)
				},
				new Type [] {
					TypeManager.asynccallback_type,
					TypeManager.object_type
				}
			);

			BeginInvokeBuilder = TypeBuilder.DefineMethod ("BeginInvoke",
				mattr, cc, TypeManager.iasyncresult_type, async_parameters.GetEmitTypes ());

			BeginInvokeBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);
			TypeManager.RegisterMethod (BeginInvokeBuilder, async_parameters);
			member_cache.AddMember (BeginInvokeBuilder, this);

			//
			// EndInvoke is a bit more interesting, all the parameters labeled as
			// out or ref have to be duplicated here.
			//

			//
			// Define parameters, and count out/ref parameters
			//
			ParametersCompiled end_parameters;
			int out_params = 0;

			foreach (Parameter p in Parameters.FixedParameters) {
				if ((p.ModFlags & Parameter.Modifier.ISBYREF) != 0)
					++out_params;
			}

			if (out_params > 0) {
				Type [] end_param_types = new Type [out_params];
				Parameter[] end_params = new Parameter [out_params];

				int param = 0;
				for (int i = 0; i < Parameters.FixedParameters.Length; ++i) {
					Parameter p = Parameters [i];
					if ((p.ModFlags & Parameter.Modifier.ISBYREF) == 0)
						continue;

					end_param_types [param] = Parameters.Types [i];
					end_params [param] = p;
					++param;
				}
				end_parameters = ParametersCompiled.CreateFullyResolved (end_params, end_param_types);
			} else {
				end_parameters = ParametersCompiled.EmptyReadOnlyParameters;
			}

			end_parameters = ParametersCompiled.MergeGenerated (end_parameters, false,
				new Parameter (null, "result", Parameter.Modifier.NONE, null, Location), TypeManager.iasyncresult_type);

			//
			// Create method, define parameters, register parameters with type system
			//
			EndInvokeBuilder = TypeBuilder.DefineMethod ("EndInvoke", mattr, cc, ret_type, end_parameters.GetEmitTypes ());
			EndInvokeBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			end_parameters.ApplyAttributes (EndInvokeBuilder);
			TypeManager.RegisterMethod (EndInvokeBuilder, end_parameters);
			member_cache.AddMember (EndInvokeBuilder, this);
		}

		public override void Emit ()
		{
			Parameters.ApplyAttributes (InvokeBuilder);

			if (BeginInvokeBuilder != null) {
				ParametersCompiled p = (ParametersCompiled) TypeManager.GetParameterData (BeginInvokeBuilder);
				p.ApplyAttributes (BeginInvokeBuilder);
			}

			if (OptAttributes != null) {
				OptAttributes.Emit ();
			}

			base.Emit ();
		}

		protected override TypeAttributes TypeAttr {
			get {
				return Modifiers.TypeAttr (ModFlags, IsTopLevel) |
					TypeAttributes.Class | TypeAttributes.Sealed |
					base.TypeAttr;
			}
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		//TODO: duplicate
		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ()) {
				return false;
			}

			Parameters.VerifyClsCompliance ();

			if (!AttributeTester.IsClsCompliant (ReturnType.Type)) {
				Report.Warning (3002, 1, Location, "Return type of `{0}' is not CLS-compliant",
					GetSignatureForError ());
			}
			return true;
		}


		public static ConstructorInfo GetConstructor (Type container_type, Type delegate_type)
		{
			Type dt = delegate_type;
			Type[] g_args = null;
			if (TypeManager.IsGenericType (delegate_type)) {
				g_args = TypeManager.GetTypeArguments (delegate_type);
				delegate_type = TypeManager.DropGenericTypeArguments (delegate_type);
			}

			Delegate d = TypeManager.LookupDelegate (delegate_type);
			if (d != null) {
#if GMCS_SOURCE
				if (g_args != null)
					return TypeBuilder.GetConstructor (dt, d.ConstructorBuilder);
#endif
				return d.ConstructorBuilder;
			}

			Expression ml = Expression.MemberLookup (container_type,
				null, dt, ConstructorInfo.ConstructorName, MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, Location.Null);

			MethodGroupExpr mg = ml as MethodGroupExpr;
			if (mg == null) {
				Report.Error (-100, Location.Null, "Internal error: could not find delegate constructor!");
				// FIXME: null will cause a crash later
				return null;
			}

			return (ConstructorInfo) mg.Methods[0];
		}

		//
		// Returns the MethodBase for "Invoke" from a delegate type, this is used
		// to extract the signature of a delegate.
		//
		public static MethodInfo GetInvokeMethod (Type container_type, Type delegate_type)
		{
			Type dt = delegate_type;

			Type[] g_args = null;
			if (TypeManager.IsGenericType (delegate_type)) {
				g_args = TypeManager.GetTypeArguments (delegate_type);
				delegate_type = TypeManager.DropGenericTypeArguments (delegate_type);
			}

			Delegate d = TypeManager.LookupDelegate (delegate_type);
			MethodInfo invoke;
			if (d != null) {
#if GMCS_SOURCE
				if (g_args != null) {
					invoke = TypeBuilder.GetMethod (dt, d.InvokeBuilder);
#if MS_COMPATIBLE
					ParametersCompiled p = (ParametersCompiled) d.Parameters.InflateTypes (g_args, g_args);
					TypeManager.RegisterMethod (invoke, p);
#endif
					return invoke;
				}
#endif
				return d.InvokeBuilder;
			}

			Expression ml = Expression.MemberLookup (container_type, null, dt,
				"Invoke", Location.Null);

			MethodGroupExpr mg = ml as MethodGroupExpr;
			if (mg == null) {
				Report.Error (-100, Location.Null, "Internal error: could not find Invoke method!");
				// FIXME: null will cause a crash later
				return null;
			}

			invoke = (MethodInfo) mg.Methods[0];
#if MS_COMPATIBLE
			if (g_args != null) {
				AParametersCollection p = TypeManager.GetParameterData (invoke);
				p = p.InflateTypes (g_args, g_args);
				TypeManager.RegisterMethod (invoke, p);
				return invoke;
			}
#endif

			return invoke;
		}

		//
		// 15.2 Delegate compatibility
		//
		public static bool IsTypeCovariant (Expression a, Type b)
		{
			//
			// For each value parameter (a parameter with no ref or out modifier), an 
			// identity conversion or implicit reference conversion exists from the
			// parameter type in D to the corresponding parameter type in M
			//
			if (a.Type == b)
				return true;

			if (RootContext.Version == LanguageVersion.ISO_1)
				return false;

			return Convert.ImplicitReferenceConversionExists (a, b);
		}
		
		/// <summary>
		///  Verifies whether the method in question is compatible with the delegate
		///  Returns the method itself if okay and null if not.
		/// </summary>
		public static MethodBase VerifyMethod (Type container_type, Type delegate_type,
						       MethodGroupExpr old_mg, MethodBase mb)
		{
			bool is_method_definition = TypeManager.IsGenericMethodDefinition (mb);
			
			MethodInfo invoke_mb = GetInvokeMethod (container_type, delegate_type);
			if (invoke_mb == null)
				return null;
				
			if (is_method_definition)
				invoke_mb = (MethodInfo) TypeManager.DropGenericMethodArguments (invoke_mb);

			AParametersCollection invoke_pd = TypeManager.GetParameterData (invoke_mb);

#if GMCS_SOURCE
			if (!is_method_definition && old_mg.type_arguments == null &&
			    !TypeManager.InferTypeArguments (invoke_pd, ref mb))
				return null;
#endif
			AParametersCollection pd = TypeManager.GetParameterData (mb);

			if (invoke_pd.Count != pd.Count)
				return null;

			for (int i = pd.Count; i > 0; ) {
				i--;

				Type invoke_pd_type = invoke_pd.Types [i];
				Type pd_type = pd.Types [i];
				Parameter.Modifier invoke_pd_type_mod = invoke_pd.FixedParameters [i].ModFlags;
				Parameter.Modifier pd_type_mod = pd.FixedParameters [i].ModFlags;

				invoke_pd_type_mod &= ~Parameter.Modifier.PARAMS;
				pd_type_mod &= ~Parameter.Modifier.PARAMS;

				if (invoke_pd_type_mod != pd_type_mod)
					return null;

				if (TypeManager.IsEqual (invoke_pd_type, pd_type))
					continue;

				if (IsTypeCovariant (new EmptyExpression (invoke_pd_type), pd_type))
					continue;

				return null;
			}

			Type invoke_mb_retval = ((MethodInfo) invoke_mb).ReturnType;
			Type mb_retval = ((MethodInfo) mb).ReturnType;
			if (TypeManager.TypeToCoreType (invoke_mb_retval) == TypeManager.TypeToCoreType (mb_retval))
				return mb;

			//if (!IsTypeCovariant (mb_retval, invoke_mb_retval))
			//	return null;

			if (RootContext.Version == LanguageVersion.ISO_1) 
				return null;

			return mb;
		}

		// <summary>
		//  Verifies whether the invocation arguments are compatible with the
		//  delegate's target method
		// </summary>
		public static bool VerifyApplicability (EmitContext ec, Type delegate_type, ref Arguments args, Location loc)
		{
			int arg_count;

			if (args == null)
				arg_count = 0;
			else
				arg_count = args.Count;

			MethodBase mb = GetInvokeMethod (ec.ContainerType, delegate_type);
			MethodGroupExpr me = new MethodGroupExpr (new MemberInfo [] { mb }, delegate_type, loc);
			
			AParametersCollection pd = TypeManager.GetParameterData (mb);

			int pd_count = pd.Count;

			bool params_method = pd.HasParams;
			bool is_params_applicable = false;
			bool is_applicable = me.IsApplicable (ec, ref args, arg_count, ref mb, ref is_params_applicable) == 0;
			if (args != null)
				arg_count = args.Count;

			if (!is_applicable && !params_method && arg_count != pd_count) {
				Report.Error (1593, loc, "Delegate `{0}' does not take `{1}' arguments",
					TypeManager.CSharpName (delegate_type), arg_count.ToString ());
				return false;
			}

			return me.VerifyArgumentsCompat (
					ec, ref args, arg_count, mb, 
					is_params_applicable || (!is_applicable && params_method),
					false, loc);
		}
		
		public static string FullDelegateDesc (MethodBase invoke_method)
		{
			return TypeManager.GetFullNameSignature (invoke_method).Replace (".Invoke", "");
		}
		
		public override MemberCache MemberCache {
			get {
				return member_cache;
			}
		}

		public Expression InstanceExpression {
			get {
				return instance_expr;
			}
			set {
				instance_expr = value;
			}
		}

		public MethodBase TargetMethod {
			get {
				return delegate_method;
			}
			set {
				delegate_method = value;
			}
		}

		public Type TargetReturnType {
			get {
				return ret_type;
			}
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Delegate;
			}
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "T:"; }
		}

		#region IMemberContainer Members

		string IMemberContainer.Name
		{
			get { throw new NotImplementedException (); }
		}

		Type IMemberContainer.Type
		{
			get { throw new NotImplementedException (); }
		}

		MemberCache IMemberContainer.BaseCache
		{
			get { throw new NotImplementedException (); }
		}

		bool IMemberContainer.IsInterface {
			get {
				return false;
			}
		}

		MemberList IMemberContainer.GetMembers (MemberTypes mt, BindingFlags bf)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}

	//
	// Base class for `NewDelegate' and `ImplicitDelegateCreation'
	//
	public abstract class DelegateCreation : Expression, MethodGroupExpr.IErrorHandler
	{
		protected ConstructorInfo constructor_method;
		protected MethodInfo delegate_method;
		// We keep this to handle IsBase only
		protected MethodGroupExpr method_group;
		protected Expression delegate_instance_expression;

		// TODO: Should either cache it or use interface to abstract it
		public static Arguments CreateDelegateMethodArguments (AParametersCollection pd, Location loc)
		{
			Arguments delegate_arguments = new Arguments (pd.Count);
			for (int i = 0; i < pd.Count; ++i) {
				Argument.AType atype_modifier;
				Type atype = pd.Types [i];
				switch (pd.FixedParameters [i].ModFlags) {
				case Parameter.Modifier.REF:
					atype_modifier = Argument.AType.Ref;
					//atype = atype.GetElementType ();
					break;
				case Parameter.Modifier.OUT:
					atype_modifier = Argument.AType.Out;
					//atype = atype.GetElementType ();
					break;
				default:
					atype_modifier = 0;
					break;
				}
				delegate_arguments.Add (new Argument (new TypeExpression (atype, loc), atype_modifier));
			}
			return delegate_arguments;
		}

		public override Expression CreateExpressionTree (EmitContext ec)
		{
			MemberAccess ma = new MemberAccess (new MemberAccess (new QualifiedAliasMember ("global", "System", loc), "Delegate", loc), "CreateDelegate", loc);

			Arguments args = new Arguments (3);
			args.Add (new Argument (new TypeOf (new TypeExpression (type, loc), loc)));
			args.Add (new Argument (new NullLiteral (loc)));
			args.Add (new Argument (new TypeOfMethodInfo (delegate_method, loc)));
			Expression e = new Invocation (ma, args).Resolve (ec);
			if (e == null)
				return null;

			e = Convert.ExplicitConversion (ec, e, type, loc);
			if (e == null)
				return null;

			return e.CreateExpressionTree (ec);
		}

		public override Expression DoResolve (EmitContext ec)
		{
			constructor_method = Delegate.GetConstructor (ec.ContainerType, type);

			MethodInfo invoke_method = Delegate.GetInvokeMethod (ec.ContainerType, type);
			method_group.DelegateType = type;
			method_group.CustomErrorHandler = this;

			Arguments arguments = CreateDelegateMethodArguments (TypeManager.GetParameterData (invoke_method), loc);
			method_group = method_group.OverloadResolve (ec, ref arguments, false, loc);
			if (method_group == null)
				return null;

			delegate_method = (MethodInfo) method_group;
			
			if (TypeManager.IsNullableType (delegate_method.DeclaringType)) {
				Report.Error (1728, loc, "Cannot create delegate from method `{0}' because it is a member of System.Nullable<T> type",
					TypeManager.GetFullNameSignature (delegate_method));
				return null;
			}		
			
			Invocation.IsSpecialMethodInvocation (delegate_method, loc);

			ExtensionMethodGroupExpr emg = method_group as ExtensionMethodGroupExpr;
			if (emg != null) {
				delegate_instance_expression = emg.ExtensionExpression;
				Type e_type = delegate_instance_expression.Type;
				if (TypeManager.IsValueType (e_type)) {
					Report.Error (1113, loc, "Extension method `{0}' of value type `{1}' cannot be used to create delegates",
						TypeManager.CSharpSignature (delegate_method), TypeManager.CSharpName (e_type));
				}
			}

			Type rt = TypeManager.TypeToCoreType (delegate_method.ReturnType);
			Expression ret_expr = new TypeExpression (rt, loc);
			if (!Delegate.IsTypeCovariant (ret_expr, (TypeManager.TypeToCoreType (invoke_method.ReturnType)))) {
				Error_ConversionFailed (ec, delegate_method, ret_expr);
			}

			if (Invocation.IsMethodExcluded (delegate_method, loc)) {
				Report.SymbolRelatedToPreviousError (delegate_method);
				MethodOrOperator m = TypeManager.GetMethod (delegate_method) as MethodOrOperator;
				if (m != null && m.IsPartialDefinition) {
					Report.Error (762, loc, "Cannot create delegate from partial method declaration `{0}'",
						TypeManager.CSharpSignature (delegate_method));
				} else {
					Report.Error (1618, loc, "Cannot create delegate with `{0}' because it has a Conditional attribute",
						TypeManager.CSharpSignature (delegate_method));
				}
			}

			DoResolveInstanceExpression (ec);
			eclass = ExprClass.Value;
			return this;
		}

		void DoResolveInstanceExpression (EmitContext ec)
		{
			//
			// Argument is another delegate
			//
			if (delegate_instance_expression != null)
				return;

			Expression instance = method_group.InstanceExpression;
			if (instance != null && instance != EmptyExpression.Null) {
				delegate_instance_expression = instance;
				Type instance_type = delegate_instance_expression.Type;
				if (TypeManager.IsValueType (instance_type) || TypeManager.IsGenericParameter (instance_type)) {
					delegate_instance_expression = new BoxedCast (
						delegate_instance_expression, TypeManager.object_type);
				}
			} else if (!delegate_method.IsStatic && !ec.IsStatic) {
				delegate_instance_expression = ec.GetThis (loc);
			}
		}
		
		public override void Emit (EmitContext ec)
		{
			if (delegate_instance_expression == null)
				ec.ig.Emit (OpCodes.Ldnull);
			else
				delegate_instance_expression.Emit (ec);

			if (!delegate_method.DeclaringType.IsSealed && delegate_method.IsVirtual && !method_group.IsBase) {
				ec.ig.Emit (OpCodes.Dup);
				ec.ig.Emit (OpCodes.Ldvirtftn, delegate_method);
			} else {
				ec.ig.Emit (OpCodes.Ldftn, delegate_method);
			}

			ec.ig.Emit (OpCodes.Newobj, constructor_method);
		}

		void Error_ConversionFailed (EmitContext ec, MethodBase method, Expression return_type)
		{
			MethodInfo invoke_method = Delegate.GetInvokeMethod (ec.ContainerType, type);
			string member_name = delegate_instance_expression != null ?
				Delegate.FullDelegateDesc (method) :
				TypeManager.GetFullNameSignature (method);

			Report.SymbolRelatedToPreviousError (type);
			Report.SymbolRelatedToPreviousError (method);
			if (RootContext.Version == LanguageVersion.ISO_1) {
				Report.Error (410, loc, "A method or delegate `{0} {1}' parameters and return type must be same as delegate `{2} {3}' parameters and return type",
					TypeManager.CSharpName (((MethodInfo) method).ReturnType), member_name,
					TypeManager.CSharpName (invoke_method.ReturnType), Delegate.FullDelegateDesc (invoke_method));
				return;
			}
			if (return_type == null) {
				Report.Error (123, loc, "A method or delegate `{0}' parameters do not match delegate `{1}' parameters",
					member_name, Delegate.FullDelegateDesc (invoke_method));
				return;
			}

			Report.Error (407, loc, "A method or delegate `{0} {1}' return type does not match delegate `{2} {3}' return type",
				return_type.GetSignatureForError (), member_name,
				TypeManager.CSharpName (invoke_method.ReturnType), Delegate.FullDelegateDesc (invoke_method));
		}

		public static MethodBase ImplicitStandardConversionExists (MethodGroupExpr mg, Type target_type)
		{
			if (target_type == TypeManager.delegate_type || target_type == TypeManager.multicast_delegate_type)
				return null;

			foreach (MethodInfo mi in mg.Methods){
				MethodBase mb = Delegate.VerifyMethod (mg.DeclaringType, target_type, mg, mi);
				if (mb != null)
					return mb;
			}
			return null;
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			if (delegate_instance_expression != null)
				delegate_instance_expression.MutateHoistedGenericType (storey);

			delegate_method = storey.MutateGenericMethod (delegate_method);
			constructor_method = storey.MutateConstructor (constructor_method);
		}

		#region IErrorHandler Members

		public bool NoExactMatch (EmitContext ec, MethodBase method)
		{
			if (TypeManager.IsGenericMethod (method))
				return false;

			Error_ConversionFailed (ec, method, null);
			return true;
		}

		public bool AmbiguousCall (MethodBase ambiguous)
		{
			return false;
		}

		#endregion
	}

	//
	// Created from the conversion code
	//
	public class ImplicitDelegateCreation : DelegateCreation
	{
		ImplicitDelegateCreation (Type t, MethodGroupExpr mg, Location l)
		{
			type = t;
			this.method_group = mg;
			loc = l;
		}

		static public Expression Create (EmitContext ec, MethodGroupExpr mge,
						 Type target_type, Location loc)
		{
			ImplicitDelegateCreation d = new ImplicitDelegateCreation (target_type, mge, loc);
			return d.DoResolve (ec);
		}
	}
	
	//
	// A delegate-creation-expression, invoked from the `New' class 
	//
	public class NewDelegate : DelegateCreation
	{
		public Arguments Arguments;

		//
		// This constructor is invoked from the `New' expression
		//
		public NewDelegate (Type type, Arguments Arguments, Location loc)
		{
			this.type = type;
			this.Arguments = Arguments;
			this.loc  = loc; 
		}

		public override Expression DoResolve (EmitContext ec)
		{
			if (Arguments == null || Arguments.Count != 1) {
				Error_InvalidDelegateArgument ();
				return null;
			}

			Argument a = Arguments [0];
			if (!a.ResolveMethodGroup (ec))
				return null;

			Expression e = a.Expr;

			AnonymousMethodExpression ame = e as AnonymousMethodExpression;
			if (ame != null && RootContext.Version != LanguageVersion.ISO_1) {
				e = ame.Compatible (ec, type);
				if (e == null)
					return null;

				return e.Resolve (ec);
			}

			method_group = e as MethodGroupExpr;
			if (method_group == null) {
				if (!TypeManager.IsDelegateType (e.Type)) {
					e.Error_UnexpectedKind (ResolveFlags.MethodGroup | ResolveFlags.Type, loc);
					return null;
				}

				//
				// An argument is not a method but another delegate
				//
				delegate_instance_expression = e;
				method_group = new MethodGroupExpr (new MemberInfo [] { 
					Delegate.GetInvokeMethod (ec.ContainerType, e.Type) }, e.Type, loc);
			}

			return base.DoResolve (ec);
		}

		void Error_InvalidDelegateArgument ()
		{
			Report.Error (149, loc, "Method name expected");
		}
	}

	public class DelegateInvocation : ExpressionStatement {

		readonly Expression InstanceExpr;
		Arguments  Arguments;
		MethodInfo method;
		
		public DelegateInvocation (Expression instance_expr, Arguments args, Location loc)
		{
			this.InstanceExpr = instance_expr;
			this.Arguments = args;
			this.loc = loc;
		}
		
		public override Expression CreateExpressionTree (EmitContext ec)
		{
			Arguments args = Arguments.CreateForExpressionTree (ec, Arguments,
				InstanceExpr.CreateExpressionTree (ec));

			return CreateExpressionFactoryCall ("Invoke", args);
		}

		public override Expression DoResolve (EmitContext ec)
		{
			if (InstanceExpr is EventExpr) {
				((EventExpr) InstanceExpr).Error_CannotAssign ();
				return null;
			}
			
			Type del_type = InstanceExpr.Type;
			if (del_type == null)
				return null;
			
			if (Arguments != null){
				Arguments.Resolve (ec);
			}
			
			if (!Delegate.VerifyApplicability (ec, del_type, ref Arguments, loc))
				return null;

			method = Delegate.GetInvokeMethod (ec.ContainerType, del_type);
			type = TypeManager.TypeToCoreType (method.ReturnType);
			eclass = ExprClass.Value;
			
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			//
			// Invocation on delegates call the virtual Invoke member
			// so we are always `instance' calls
			//
			Invocation.EmitCall (ec, false, InstanceExpr, method, Arguments, loc);
		}

		public override void EmitStatement (EmitContext ec)
		{
			Emit (ec);
			// 
			// Pop the return value if there is one
			//
			if (type != TypeManager.void_type)
				ec.ig.Emit (OpCodes.Pop);
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			method = storey.MutateGenericMethod (method);
			type = storey.MutateType (type);

			if (Arguments != null)
				Arguments.MutateHoistedGenericType (storey);

			InstanceExpr.MutateHoistedGenericType (storey);
		}
	}
}
