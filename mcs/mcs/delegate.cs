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
// Copyright 2003-2009 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Mono.CSharp {

	//
	// Delegate container implementation
	//
	public class Delegate : TypeContainer
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
			: base (ns, parent, name, attrs, Kind.Delegate)

		{
			this.ReturnType = type;
			ModFlags        = Modifiers.Check (AllowedModifiers, mod_flags,
							   IsTopLevel ? Modifiers.INTERNAL :
							   Modifiers.PRIVATE, name.Location, Report);
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

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Delegate;
			}
		}

 		protected override bool DoDefineMembers ()
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
				Method.Error1599 (Location, ret_type, Report);
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

			//
			// Don't emit async method for compiler generated delegates (e.g. dynamic site containers)
			//
			if (TypeManager.iasyncresult_type != null && TypeManager.asynccallback_type != null && !IsCompilerGenerated) {
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
			if (TypeManager.IsDynamicType (ret_type)) {
				return_attributes = new ReturnParameter (InvokeBuilder, Location);
				return_attributes.EmitPredefined (PredefinedAttributes.Get.Dynamic, Location);
			}

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


		public static ConstructorInfo GetConstructor (CompilerContext ctx, Type container_type, Type delegate_type)
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

			Expression ml = Expression.MemberLookup (ctx, container_type,
				null, dt, ConstructorInfo.ConstructorName, MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, Location.Null);

			MethodGroupExpr mg = ml as MethodGroupExpr;
			if (mg == null) {
				ctx.Report.Error (-100, Location.Null, "Internal error: could not find delegate constructor!");
				// FIXME: null will cause a crash later
				return null;
			}

			return (ConstructorInfo) mg.Methods[0];
		}

		//
		// Returns the MethodBase for "Invoke" from a delegate type, this is used
		// to extract the signature of a delegate.
		//
		public static MethodInfo GetInvokeMethod (CompilerContext ctx, Type container_type, Type delegate_type)
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

			Expression ml = Expression.MemberLookup (ctx, container_type, null, dt,
				"Invoke", Location.Null);

			MethodGroupExpr mg = ml as MethodGroupExpr;
			if (mg == null) {
				ctx.Report.Error (-100, Location.Null, "Internal error: could not find Invoke method!");
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

		// <summary>
		//  Verifies whether the invocation arguments are compatible with the
		//  delegate's target method
		// </summary>
		public static bool VerifyApplicability (ResolveContext ec, Type delegate_type, ref Arguments args, Location loc)
		{
			int arg_count;

			if (args == null)
				arg_count = 0;
			else
				arg_count = args.Count;

			MethodBase mb = GetInvokeMethod (ec.Compiler, ec.CurrentType, delegate_type);
			MethodGroupExpr me = new MethodGroupExpr (new MemberInfo [] { mb }, delegate_type, loc);
			
			AParametersCollection pd = TypeManager.GetParameterData (mb);

			int pd_count = pd.Count;

			bool params_method = pd.HasParams;
			bool is_params_applicable = false;
			bool is_applicable = me.IsApplicable (ec, ref args, arg_count, ref mb, ref is_params_applicable) == 0;
			if (args != null)
				arg_count = args.Count;

			if (!is_applicable && !params_method && arg_count != pd_count) {
				ec.Report.Error (1593, loc, "Delegate `{0}' does not take `{1}' arguments",
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

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			MemberAccess ma = new MemberAccess (new MemberAccess (new QualifiedAliasMember ("global", "System", loc), "Delegate", loc), "CreateDelegate", loc);

			Arguments args = new Arguments (3);
			args.Add (new Argument (new TypeOf (new TypeExpression (type, loc), loc)));
			args.Add (new Argument (new NullLiteral (loc)));
			args.Add (new Argument (new TypeOfMethod (delegate_method, loc)));
			Expression e = new Invocation (ma, args).Resolve (ec);
			if (e == null)
				return null;

			e = Convert.ExplicitConversion (ec, e, type, loc);
			if (e == null)
				return null;

			return e.CreateExpressionTree (ec);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			constructor_method = Delegate.GetConstructor (ec.Compiler, ec.CurrentType, type);

			MethodInfo invoke_method = Delegate.GetInvokeMethod (ec.Compiler, ec.CurrentType, type);
			method_group.DelegateType = type;
			method_group.CustomErrorHandler = this;

			Arguments arguments = CreateDelegateMethodArguments (TypeManager.GetParameterData (invoke_method), loc);
			method_group = method_group.OverloadResolve (ec, ref arguments, false, loc);
			if (method_group == null)
				return null;

			delegate_method = (MethodInfo) method_group;
			
			if (TypeManager.IsNullableType (delegate_method.DeclaringType)) {
				ec.Report.Error (1728, loc, "Cannot create delegate from method `{0}' because it is a member of System.Nullable<T> type",
					TypeManager.GetFullNameSignature (delegate_method));
				return null;
			}		
			
			Invocation.IsSpecialMethodInvocation (ec, delegate_method, loc);

			ExtensionMethodGroupExpr emg = method_group as ExtensionMethodGroupExpr;
			if (emg != null) {
				delegate_instance_expression = emg.ExtensionExpression;
				Type e_type = delegate_instance_expression.Type;
				if (TypeManager.IsValueType (e_type)) {
					ec.Report.Error (1113, loc, "Extension method `{0}' of value type `{1}' cannot be used to create delegates",
						TypeManager.CSharpSignature (delegate_method), TypeManager.CSharpName (e_type));
				}
			}

			Type rt = TypeManager.TypeToCoreType (delegate_method.ReturnType);
			Expression ret_expr = new TypeExpression (rt, loc);
			if (!Delegate.IsTypeCovariant (ret_expr, (TypeManager.TypeToCoreType (invoke_method.ReturnType)))) {
				Error_ConversionFailed (ec, delegate_method, ret_expr);
			}

			if (Invocation.IsMethodExcluded (delegate_method, loc)) {
				ec.Report.SymbolRelatedToPreviousError (delegate_method);
				MethodOrOperator m = TypeManager.GetMethod (delegate_method) as MethodOrOperator;
				if (m != null && m.IsPartialDefinition) {
					ec.Report.Error (762, loc, "Cannot create delegate from partial method declaration `{0}'",
						TypeManager.CSharpSignature (delegate_method));
				} else {
					ec.Report.Error (1618, loc, "Cannot create delegate with `{0}' because it has a Conditional attribute",
						TypeManager.CSharpSignature (delegate_method));
				}
			}

			DoResolveInstanceExpression (ec);
			eclass = ExprClass.Value;
			return this;
		}

		void DoResolveInstanceExpression (ResolveContext ec)
		{
			//
			// Argument is another delegate
			//
			if (delegate_instance_expression != null)
				return;

			if (method_group.IsStatic) {
				delegate_instance_expression = null;
				return;
			}

			Expression instance = method_group.InstanceExpression;
			if (instance != null && instance != EmptyExpression.Null) {
				delegate_instance_expression = instance;
				Type instance_type = delegate_instance_expression.Type;
				if (TypeManager.IsValueType (instance_type) || TypeManager.IsGenericParameter (instance_type)) {
					delegate_instance_expression = new BoxedCast (
						delegate_instance_expression, TypeManager.object_type);
				}
			} else {
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

		void Error_ConversionFailed (ResolveContext ec, MethodBase method, Expression return_type)
		{
			MethodInfo invoke_method = Delegate.GetInvokeMethod (ec.Compiler, ec.CurrentType, type);
			string member_name = delegate_instance_expression != null ?
				Delegate.FullDelegateDesc (method) :
				TypeManager.GetFullNameSignature (method);

			ec.Report.SymbolRelatedToPreviousError (type);
			ec.Report.SymbolRelatedToPreviousError (method);
			if (RootContext.Version == LanguageVersion.ISO_1) {
				ec.Report.Error (410, loc, "A method or delegate `{0} {1}' parameters and return type must be same as delegate `{2} {3}' parameters and return type",
					TypeManager.CSharpName (((MethodInfo) method).ReturnType), member_name,
					TypeManager.CSharpName (invoke_method.ReturnType), Delegate.FullDelegateDesc (invoke_method));
				return;
			}
			if (return_type == null) {
				ec.Report.Error (123, loc, "A method or delegate `{0}' parameters do not match delegate `{1}' parameters",
					member_name, Delegate.FullDelegateDesc (invoke_method));
				return;
			}

			ec.Report.Error (407, loc, "A method or delegate `{0} {1}' return type does not match delegate `{2} {3}' return type",
				return_type.GetSignatureForError (), member_name,
				TypeManager.CSharpName (invoke_method.ReturnType), Delegate.FullDelegateDesc (invoke_method));
		}

		public static bool ImplicitStandardConversionExists (ResolveContext ec, MethodGroupExpr mg, Type target_type)
		{
			if (target_type == TypeManager.delegate_type || target_type == TypeManager.multicast_delegate_type)
				return false;

			mg.DelegateType = target_type;
			MethodInfo invoke = Delegate.GetInvokeMethod (ec.Compiler, null, target_type);

			Arguments arguments = CreateDelegateMethodArguments (TypeManager.GetParameterData (invoke), mg.Location);
			return mg.OverloadResolve (ec, ref arguments, true, mg.Location) != null;
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			if (delegate_instance_expression != null)
				delegate_instance_expression.MutateHoistedGenericType (storey);

			delegate_method = storey.MutateGenericMethod (delegate_method);
			constructor_method = storey.MutateConstructor (constructor_method);
		}

		#region IErrorHandler Members

		public bool NoExactMatch (ResolveContext ec, MethodBase method)
		{
			if (TypeManager.IsGenericMethod (method))
				return false;

			Error_ConversionFailed (ec, method, null);
			return true;
		}

		public bool AmbiguousCall (ResolveContext ec, MethodBase ambiguous)
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

		static public Expression Create (ResolveContext ec, MethodGroupExpr mge,
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

		public override Expression DoResolve (ResolveContext ec)
		{
			if (Arguments == null || Arguments.Count != 1) {
				ec.Report.Error (149, loc, "Method name expected");
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
					e.Error_UnexpectedKind (ec, ResolveFlags.MethodGroup | ResolveFlags.Type, loc);
					return null;
				}

				//
				// An argument is not a method but another delegate
				//
				delegate_instance_expression = e;
				method_group = new MethodGroupExpr (new MemberInfo [] { 
					Delegate.GetInvokeMethod (ec.Compiler, ec.CurrentType, e.Type) }, e.Type, loc);
			}

			return base.DoResolve (ec);
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
		
		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = Arguments.CreateForExpressionTree (ec, Arguments,
				InstanceExpr.CreateExpressionTree (ec));

			return CreateExpressionFactoryCall (ec, "Invoke", args);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (InstanceExpr is EventExpr) {
				((EventExpr) InstanceExpr).Error_CannotAssign (ec);
				return null;
			}
			
			Type del_type = InstanceExpr.Type;
			if (del_type == null)
				return null;
			
			if (!Delegate.VerifyApplicability (ec, del_type, ref Arguments, loc))
				return null;

			method = Delegate.GetInvokeMethod (ec.Compiler, ec.CurrentType, del_type);
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
