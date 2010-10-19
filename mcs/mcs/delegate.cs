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
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Mono.CSharp {

	//
	// Delegate container implementation
	//
	public class Delegate : TypeContainer, IParametersMember
	{
 		FullNamedExpression ReturnType;
		readonly ParametersCompiled parameters;

		Constructor Constructor;
		Method InvokeBuilder;
		Method BeginInvokeBuilder;
		Method EndInvokeBuilder;

		static readonly string[] attribute_targets = new string [] { "type", "return" };

		public static readonly string InvokeMethodName = "Invoke";
		
		Expression instance_expr;
		ReturnParameter return_attributes;

		const Modifiers MethodModifiers = Modifiers.PUBLIC | Modifiers.VIRTUAL;

		const Modifiers AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.UNSAFE |
			Modifiers.PRIVATE;

 		public Delegate (NamespaceEntry ns, DeclSpace parent, FullNamedExpression type,
				 Modifiers mod_flags, MemberName name, ParametersCompiled param_list,
				 Attributes attrs)
			: base (ns, parent, name, attrs, MemberKind.Delegate)

		{
			this.ReturnType = type;
			ModFlags        = ModifiersExtensions.Check (AllowedModifiers, mod_flags,
							   IsTopLevel ? Modifiers.INTERNAL :
							   Modifiers.PRIVATE, name.Location, Report);
			parameters      = param_list;
			spec = new TypeSpec (Kind, null, this, null, ModFlags | Modifiers.SEALED);
		}

		#region Properties
		public TypeSpec MemberType {
			get {
				return ReturnType.Type;
			}
		}

		public AParametersCollection Parameters {
			get {
				return parameters;
			}
		}
		#endregion

		public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Target == AttributeTargets.ReturnValue) {
				if (return_attributes == null)
					return_attributes = new ReturnParameter (this, InvokeBuilder.MethodBuilder, Location);

				return_attributes.ApplyAttributeBuilder (a, ctor, cdata, pa);
				return;
			}

			base.ApplyAttributeBuilder (a, ctor, cdata, pa);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Delegate;
			}
		}

		protected override bool DoDefineMembers ()
		{
			var ctor_parameters = ParametersCompiled.CreateFullyResolved (
				new [] {
					new Parameter (new TypeExpression (TypeManager.object_type, Location), "object", Parameter.Modifier.NONE, null, Location),
					new Parameter (new TypeExpression (TypeManager.intptr_type, Location), "method", Parameter.Modifier.NONE, null, Location)
				},
				new [] {
					TypeManager.object_type,
					TypeManager.intptr_type
				}
			);

			Constructor = new Constructor (this, System.Reflection.ConstructorInfo.ConstructorName,
				Modifiers.PUBLIC, null, ctor_parameters, null, Location);
			Constructor.Define ();

			//
			// Here the various methods like Invoke, BeginInvoke etc are defined
			//
			// First, call the `out of band' special method for
			// defining recursively any types we need:
			//
			var p = parameters;

			if (!p.Resolve (this))
				return false;

			//
			// Invoke method
			//

			// Check accessibility
			foreach (var partype in p.Types) {
				if (!IsAccessibleAs (partype)) {
					Report.SymbolRelatedToPreviousError (partype);
					Report.Error (59, Location,
						"Inconsistent accessibility: parameter type `{0}' is less accessible than delegate `{1}'",
						TypeManager.CSharpName (partype), GetSignatureForError ());
				}
			}

			ReturnType = ReturnType.ResolveAsTypeTerminal (this, false);
			if (ReturnType == null)
				return false;

			var ret_type = ReturnType.Type;

			//
			// We don't have to check any others because they are all
			// guaranteed to be accessible - they are standard types.
			//
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

			InvokeBuilder = new Method (this, null, ReturnType, MethodModifiers, new MemberName (InvokeMethodName), p, null);
			InvokeBuilder.Define ();

			//
			// Don't emit async method for compiler generated delegates (e.g. dynamic site containers)
			//
			if (TypeManager.iasyncresult_type != null && TypeManager.asynccallback_type != null && !IsCompilerGenerated) {
				DefineAsyncMethods (Parameters.CallingConvention);
			}

			return true;
		}

		void DefineAsyncMethods (CallingConventions cc)
		{
			//
			// BeginInvoke
			//
			ParametersCompiled async_parameters;
			if (Parameters.Count == 0) {
				async_parameters = ParametersCompiled.EmptyReadOnlyParameters;
			} else {
				var compiled = new Parameter[Parameters.Count];
				for (int i = 0; i < compiled.Length; ++i)
					compiled[i] = new Parameter (new TypeExpression (Parameters.Types[i], Location),
						Parameters.FixedParameters[i].Name,
						Parameters.FixedParameters[i].ModFlags & (Parameter.Modifier.REF | Parameter.Modifier.OUT),
						null, Location);

				async_parameters = new ParametersCompiled (compiled);
			}

			async_parameters = ParametersCompiled.MergeGenerated (Compiler, async_parameters, false,
				new Parameter[] {
					new Parameter (new TypeExpression (TypeManager.asynccallback_type, Location), "callback", Parameter.Modifier.NONE, null, Location),
					new Parameter (new TypeExpression (TypeManager.object_type, Location), "object", Parameter.Modifier.NONE, null, Location)
				},
				new [] {
					TypeManager.asynccallback_type,
					TypeManager.object_type
				}
			);

			BeginInvokeBuilder = new Method (this, null,
				new TypeExpression (TypeManager.iasyncresult_type, Location), MethodModifiers,
				new MemberName ("BeginInvoke"), async_parameters, null);
			BeginInvokeBuilder.Define ();

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
				var end_param_types = new TypeSpec [out_params];
				Parameter[] end_params = new Parameter[out_params];

				int param = 0;
				for (int i = 0; i < Parameters.FixedParameters.Length; ++i) {
					Parameter p = parameters [i];
					if ((p.ModFlags & Parameter.Modifier.ISBYREF) == 0)
						continue;

					end_param_types[param] = Parameters.Types[i];
					end_params[param] = p;
					++param;
				}
				end_parameters = ParametersCompiled.CreateFullyResolved (end_params, end_param_types);
			} else {
				end_parameters = ParametersCompiled.EmptyReadOnlyParameters;
			}

			end_parameters = ParametersCompiled.MergeGenerated (Compiler, end_parameters, false,
				new Parameter (
					new TypeExpression (TypeManager.iasyncresult_type, Location),
					"result", Parameter.Modifier.NONE, null, Location),
				TypeManager.iasyncresult_type);

			//
			// Create method, define parameters, register parameters with type system
			//
			EndInvokeBuilder = new Method (this, null, ReturnType, MethodModifiers, new MemberName ("EndInvoke"), end_parameters, null);
			EndInvokeBuilder.Define ();
		}

		public override void DefineConstants ()
		{
			if (!Parameters.IsEmpty) {
				parameters.ResolveDefaultValues (this);
			}
		}

		public override void EmitType ()
		{
			if (ReturnType.Type != null) {
				if (ReturnType.Type == InternalType.Dynamic) {
					return_attributes = new ReturnParameter (this, InvokeBuilder.MethodBuilder, Location);
					Compiler.PredefinedAttributes.Dynamic.EmitAttribute (return_attributes.Builder);
				} else if (ReturnType.Type.HasDynamicElement) {
					return_attributes = new ReturnParameter (this, InvokeBuilder.MethodBuilder, Location);
					Compiler.PredefinedAttributes.Dynamic.EmitAttribute (return_attributes.Builder, ReturnType.Type);
				}
			}

			parameters.ApplyAttributes (this, InvokeBuilder.MethodBuilder);
			
			Constructor.ConstructorBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);
			InvokeBuilder.MethodBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			if (BeginInvokeBuilder != null) {
				BeginInvokeBuilder.ParameterInfo.ApplyAttributes (this, BeginInvokeBuilder.MethodBuilder);

				BeginInvokeBuilder.MethodBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);
				EndInvokeBuilder.MethodBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);
			}

			if (OptAttributes != null) {
				OptAttributes.Emit ();
			}

			base.Emit ();
		}

		protected override TypeExpr[] ResolveBaseTypes (out TypeExpr base_class)
		{
			base_type = TypeManager.multicast_delegate_type;
			base_class = null;
			return null;
		}

		protected override TypeAttributes TypeAttr {
			get {
				return ModifiersExtensions.TypeAttr (ModFlags, IsTopLevel) |
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

			parameters.VerifyClsCompliance (this);

			if (!ReturnType.Type.IsCLSCompliant ()) {
				Report.Warning (3002, 1, Location, "Return type of `{0}' is not CLS-compliant",
					GetSignatureForError ());
			}
			return true;
		}


		public static MethodSpec GetConstructor (CompilerContext ctx, TypeSpec container_type, TypeSpec delType)
		{
			var ctor = MemberCache.FindMember (delType, MemberFilter.Constructor (null), BindingRestriction.DeclaredOnly);
			return (MethodSpec) ctor;
		}

		//
		// Returns the "Invoke" from a delegate type
		//
		public static MethodSpec GetInvokeMethod (CompilerContext ctx, TypeSpec delType)
		{
			var invoke = MemberCache.FindMember (delType,
				MemberFilter.Method (InvokeMethodName, 0, null, null),
				BindingRestriction.DeclaredOnly);

			return (MethodSpec) invoke;
		}

		public static AParametersCollection GetParameters (CompilerContext ctx, TypeSpec delType)
		{
			var invoke_mb = GetInvokeMethod (ctx, delType);
			return invoke_mb.Parameters;
		}

		//
		// 15.2 Delegate compatibility
		//
		public static bool IsTypeCovariant (Expression a, TypeSpec b)
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

		public static string FullDelegateDesc (MethodSpec invoke_method)
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
	}

	//
	// Base class for `NewDelegate' and `ImplicitDelegateCreation'
	//
	public abstract class DelegateCreation : Expression, OverloadResolver.IErrorHandler
	{
		protected MethodSpec constructor_method;
		protected MethodGroupExpr method_group;

		public static Arguments CreateDelegateMethodArguments (AParametersCollection pd, TypeSpec[] types, Location loc)
		{
			Arguments delegate_arguments = new Arguments (pd.Count);
			for (int i = 0; i < pd.Count; ++i) {
				Argument.AType atype_modifier;
				switch (pd.FixedParameters [i].ModFlags) {
				case Parameter.Modifier.REF:
					atype_modifier = Argument.AType.Ref;
					break;
				case Parameter.Modifier.OUT:
					atype_modifier = Argument.AType.Out;
					break;
				default:
					atype_modifier = 0;
					break;
				}

				delegate_arguments.Add (new Argument (new TypeExpression (types [i], loc), atype_modifier));
			}

			return delegate_arguments;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			MemberAccess ma = new MemberAccess (new MemberAccess (new QualifiedAliasMember ("global", "System", loc), "Delegate", loc), "CreateDelegate", loc);

			Arguments args = new Arguments (3);
			args.Add (new Argument (new TypeOf (new TypeExpression (type, loc), loc)));
			args.Add (new Argument (new NullLiteral (loc)));
			args.Add (new Argument (method_group.CreateExpressionTree (ec)));
			Expression e = new Invocation (ma, args).Resolve (ec);
			if (e == null)
				return null;

			e = Convert.ExplicitConversion (ec, e, type, loc);
			if (e == null)
				return null;

			return e.CreateExpressionTree (ec);
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			constructor_method = Delegate.GetConstructor (ec.Compiler, ec.CurrentType, type);

			var invoke_method = Delegate.GetInvokeMethod (ec.Compiler, type);

			Arguments arguments = CreateDelegateMethodArguments (invoke_method.Parameters, invoke_method.Parameters.Types, loc);
			method_group = method_group.OverloadResolve (ec, ref arguments, this, OverloadResolver.Restrictions.CovariantDelegate);
			if (method_group == null)
				return null;

			var delegate_method = method_group.BestCandidate;
			
			if (TypeManager.IsNullableType (delegate_method.DeclaringType)) {
				ec.Report.Error (1728, loc, "Cannot create delegate from method `{0}' because it is a member of System.Nullable<T> type",
					delegate_method.GetSignatureForError ());
				return null;
			}		
			
			Invocation.IsSpecialMethodInvocation (ec, delegate_method, loc);

			ExtensionMethodGroupExpr emg = method_group as ExtensionMethodGroupExpr;
			if (emg != null) {
				method_group.InstanceExpression = emg.ExtensionExpression;
				TypeSpec e_type = emg.ExtensionExpression.Type;
				if (TypeManager.IsValueType (e_type)) {
					ec.Report.Error (1113, loc, "Extension method `{0}' of value type `{1}' cannot be used to create delegates",
						delegate_method.GetSignatureForError (), TypeManager.CSharpName (e_type));
				}
			}

			TypeSpec rt = delegate_method.ReturnType;
			Expression ret_expr = new TypeExpression (rt, loc);
			if (!Delegate.IsTypeCovariant (ret_expr, invoke_method.ReturnType)) {
				Error_ConversionFailed (ec, delegate_method, ret_expr);
			}

			if (delegate_method.IsConditionallyExcluded (loc)) {
				ec.Report.SymbolRelatedToPreviousError (delegate_method);
				MethodOrOperator m = delegate_method.MemberDefinition as MethodOrOperator;
				if (m != null && m.IsPartialDefinition) {
					ec.Report.Error (762, loc, "Cannot create delegate from partial method declaration `{0}'",
						delegate_method.GetSignatureForError ());
				} else {
					ec.Report.Error (1618, loc, "Cannot create delegate with `{0}' because it has a Conditional attribute",
						TypeManager.CSharpSignature (delegate_method));
				}
			}

			var expr = method_group.InstanceExpression;
			if (expr != null && (expr.Type.IsGenericParameter || !TypeManager.IsReferenceType (expr.Type)))
				method_group.InstanceExpression = new BoxedCast (expr, TypeManager.object_type);

			eclass = ExprClass.Value;
			return this;
		}
		
		public override void Emit (EmitContext ec)
		{
			if (method_group.InstanceExpression == null)
				ec.Emit (OpCodes.Ldnull);
			else
				method_group.InstanceExpression.Emit (ec);

			var delegate_method = method_group.BestCandidate;

			// Any delegate must be sealed
			if (!delegate_method.DeclaringType.IsDelegate && delegate_method.IsVirtual && !method_group.IsBase) {
				ec.Emit (OpCodes.Dup);
				ec.Emit (OpCodes.Ldvirtftn, delegate_method);
			} else {
				ec.Emit (OpCodes.Ldftn, delegate_method);
			}

			ec.Emit (OpCodes.Newobj, constructor_method);
		}

		void Error_ConversionFailed (ResolveContext ec, MethodSpec method, Expression return_type)
		{
			var invoke_method = Delegate.GetInvokeMethod (ec.Compiler, type);
			string member_name = method_group.InstanceExpression != null ?
				Delegate.FullDelegateDesc (method) :
				TypeManager.GetFullNameSignature (method);

			ec.Report.SymbolRelatedToPreviousError (type);
			ec.Report.SymbolRelatedToPreviousError (method);
			if (RootContext.Version == LanguageVersion.ISO_1) {
				ec.Report.Error (410, loc, "A method or delegate `{0} {1}' parameters and return type must be same as delegate `{2} {3}' parameters and return type",
					TypeManager.CSharpName (method.ReturnType), member_name,
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

		public static bool ImplicitStandardConversionExists (ResolveContext ec, MethodGroupExpr mg, TypeSpec target_type)
		{
			if (target_type == TypeManager.delegate_type || target_type == TypeManager.multicast_delegate_type)
				return false;

			var invoke = Delegate.GetInvokeMethod (ec.Compiler, target_type);

			Arguments arguments = CreateDelegateMethodArguments (invoke.Parameters, invoke.Parameters.Types, mg.Location);
			return mg.OverloadResolve (ec, ref arguments, null, OverloadResolver.Restrictions.CovariantDelegate | OverloadResolver.Restrictions.ProbingOnly) != null;
		}

		#region IErrorHandler Members

		bool OverloadResolver.IErrorHandler.AmbiguousCandidates (ResolveContext ec, MemberSpec best, MemberSpec ambiguous)
		{
			return false;
		}

		bool OverloadResolver.IErrorHandler.ArgumentMismatch (ResolveContext rc, MemberSpec best, Argument arg, int index)
		{
			Error_ConversionFailed (rc, best as MethodSpec, null);
			return true;
		}

		bool OverloadResolver.IErrorHandler.NoArgumentMatch (ResolveContext rc, MemberSpec best)
		{
			Error_ConversionFailed (rc, best as MethodSpec, null);
			return true;
		}

		bool OverloadResolver.IErrorHandler.TypeInferenceFailed (ResolveContext rc, MemberSpec best)
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
		ImplicitDelegateCreation (TypeSpec t, MethodGroupExpr mg, Location l)
		{
			type = t;
			this.method_group = mg;
			loc = l;
		}

		static public Expression Create (ResolveContext ec, MethodGroupExpr mge,
						 TypeSpec target_type, Location loc)
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
		public NewDelegate (TypeSpec type, Arguments Arguments, Location loc)
		{
			this.type = type;
			this.Arguments = Arguments;
			this.loc  = loc; 
		}

		protected override Expression DoResolve (ResolveContext ec)
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
				if (e.Type == InternalType.Dynamic) {
					e = Convert.ImplicitConversionRequired (ec, e, type, loc);
				} else if (!e.Type.IsDelegate) {
					e.Error_UnexpectedKind (ec, ResolveFlags.MethodGroup | ResolveFlags.Type, loc);
					return null;
				}

				//
				// An argument is not a method but another delegate
				//
				method_group = new MethodGroupExpr (Delegate.GetInvokeMethod (ec.Compiler, e.Type), e.Type, loc);
				method_group.InstanceExpression = e;
			}

			return base.DoResolve (ec);
		}
	}

	//
	// Invocation converted to delegate Invoke call
	//
	class DelegateInvocation : ExpressionStatement
	{
		readonly Expression InstanceExpr;
		Arguments arguments;
		MethodSpec method;
		
		public DelegateInvocation (Expression instance_expr, Arguments args, Location loc)
		{
			this.InstanceExpr = instance_expr;
			this.arguments = args;
			this.loc = loc;
		}
		
		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = Arguments.CreateForExpressionTree (ec, this.arguments,
				InstanceExpr.CreateExpressionTree (ec));

			return CreateExpressionFactoryCall (ec, "Invoke", args);
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			if (InstanceExpr is EventExpr) {
				((EventExpr) InstanceExpr).Error_CannotAssign (ec);
				return null;
			}
			
			TypeSpec del_type = InstanceExpr.Type;
			if (del_type == null)
				return null;

			//
			// Do only core overload resolution the rest of the checks has been
			// done on primary expression
			//
			method = Delegate.GetInvokeMethod (ec.Compiler, del_type);
			var res = new OverloadResolver (new MemberSpec[] { method }, OverloadResolver.Restrictions.DelegateInvoke, loc);
			var valid = res.ResolveMember<MethodSpec> (ec, ref arguments);
			if (valid == null && !res.BestCandidateIsDynamic)
				return null;

			type = method.ReturnType;
			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			//
			// Invocation on delegates call the virtual Invoke member
			// so we are always `instance' calls
			//
			Invocation.EmitCall (ec, InstanceExpr, method, arguments, loc);
		}

		public override void EmitStatement (EmitContext ec)
		{
			Emit (ec);
			// 
			// Pop the return value if there is one
			//
			if (type != TypeManager.void_type)
				ec.Emit (OpCodes.Pop);
		}

		public override System.Linq.Expressions.Expression MakeExpression (BuilderContext ctx)
		{
			return Invocation.MakeExpression (ctx, InstanceExpr, method, arguments);
		}
	}
}
