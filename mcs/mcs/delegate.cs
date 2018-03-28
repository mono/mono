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
// Copyright 2011 Xamarin Inc
//

using System;

#if STATIC
using SecurityType = System.Collections.Generic.List<IKVM.Reflection.Emit.CustomAttributeBuilder>;
using IKVM.Reflection;
using IKVM.Reflection.Emit;
#else
using SecurityType = System.Collections.Generic.Dictionary<System.Security.Permissions.SecurityAction, System.Security.PermissionSet>;
using System.Reflection;
using System.Reflection.Emit;
#endif

namespace Mono.CSharp {

	//
	// Delegate container implementation
	//
	public class Delegate : TypeDefinition, IParametersMember
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

		SecurityType declarative_security;

		const Modifiers MethodModifiers = Modifiers.PUBLIC | Modifiers.VIRTUAL;

		const Modifiers AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.UNSAFE |
			Modifiers.PRIVATE;

 		public Delegate (TypeContainer parent, FullNamedExpression type, Modifiers mod_flags, MemberName name, ParametersCompiled param_list,
				 Attributes attrs)
			: base (parent, name, attrs, MemberKind.Delegate)

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

		public FullNamedExpression TypExpression {
			get {
				return ReturnType;
			}
		}

		#endregion

		public override void Accept (StructuralVisitor visitor)
		{
			visitor.Visit (this);
		}

		public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Target == AttributeTargets.ReturnValue) {
				CreateReturnBuilder ();
				return_attributes.ApplyAttributeBuilder (a, ctor, cdata, pa);
				return;
			}

			if (a.IsValidSecurityAttribute ()) {
				a.ExtractSecurityPermissionSet (ctor, ref declarative_security);
				return;
			}

			base.ApplyAttributeBuilder (a, ctor, cdata, pa);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Delegate;
			}
		}

		ReturnParameter CreateReturnBuilder ()
		{
			return return_attributes ?? (return_attributes = new ReturnParameter (this, InvokeBuilder.MethodBuilder, Location));
		}

		protected override bool DoDefineMembers ()
		{
			var builtin_types = Compiler.BuiltinTypes;

			var ctor_parameters = ParametersCompiled.CreateFullyResolved (
				new [] {
					new Parameter (new TypeExpression (builtin_types.Object, Location), "object", Parameter.Modifier.NONE, null, Location),
					new Parameter (new TypeExpression (builtin_types.IntPtr, Location), "method", Parameter.Modifier.NONE, null, Location)
				},
				new [] {
					builtin_types.Object,
					builtin_types.IntPtr
				}
			);

			Constructor = new Constructor (this, Constructor.ConstructorName,
				Modifiers.PUBLIC, null, ctor_parameters, Location);
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
						partype.GetSignatureForError (), GetSignatureForError ());
				}
			}

			var ret_type = ReturnType.ResolveAsType (this);
			if (ret_type == null)
				return false;

			//
			// We don't have to check any others because they are all
			// guaranteed to be accessible - they are standard types.
			//
			if (!IsAccessibleAs (ret_type)) {
				Report.SymbolRelatedToPreviousError (ret_type);
				Report.Error (58, Location,
						  "Inconsistent accessibility: return type `" +
						  ret_type.GetSignatureForError () + "' is less " +
						  "accessible than delegate `" + GetSignatureForError () + "'");
				return false;
			}

			CheckProtectedModifier ();

			if (ret_type.IsSpecialRuntimeType && Compiler.Settings.StdLib) {
				Method.Error_ReturnTypeCantBeRefAny (Location, ret_type, Report);
				return false;
			}

			VarianceDecl.CheckTypeVariance (ret_type, Variance.Covariant, this);

			var resolved_rt = new TypeExpression (ret_type, Location);
			InvokeBuilder = new Method (this, resolved_rt, MethodModifiers, new MemberName (InvokeMethodName), p, null);
			InvokeBuilder.Define ();

			//
			// Don't emit async method for compiler generated delegates (e.g. dynamic site containers)
			//
			if (!IsCompilerGenerated) {
				DefineAsyncMethods (resolved_rt);
			}

			return true;
		}

		void DefineAsyncMethods (TypeExpression returnType)
		{
			var iasync_result = Module.PredefinedTypes.IAsyncResult;
			var async_callback = Module.PredefinedTypes.AsyncCallback;

			//
			// It's ok when async types don't exist, the delegate will have Invoke method only
			//
			if (!iasync_result.Define () || !async_callback.Define ())
				return;

			//
			// BeginInvoke
			//
			ParametersCompiled async_parameters;
			if (Parameters.Count == 0) {
				async_parameters = ParametersCompiled.EmptyReadOnlyParameters;
			} else {
				var compiled = new Parameter[Parameters.Count];
				for (int i = 0; i < compiled.Length; ++i) {
					var p = parameters[i];
					compiled[i] = new Parameter (new TypeExpression (parameters.Types[i], Location),
						p.Name,
						p.ModFlags & Parameter.Modifier.RefOutMask,
						p.OptAttributes == null ? null : p.OptAttributes.Clone (), Location);
				}

				async_parameters = new ParametersCompiled (compiled);
			}

			async_parameters = ParametersCompiled.MergeGenerated (Compiler, async_parameters, false,
				new Parameter[] {
					new Parameter (new TypeExpression (async_callback.TypeSpec, Location), "callback", Parameter.Modifier.NONE, null, Location),
					new Parameter (new TypeExpression (Compiler.BuiltinTypes.Object, Location), "object", Parameter.Modifier.NONE, null, Location)
				},
				new [] {
					async_callback.TypeSpec,
					Compiler.BuiltinTypes.Object
				}
			);

			BeginInvokeBuilder = new Method (this,
				new TypeExpression (iasync_result.TypeSpec, Location), MethodModifiers,
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
				if ((p.ModFlags & Parameter.Modifier.RefOutMask) != 0)
					++out_params;
			}

			if (out_params > 0) {
				Parameter[] end_params = new Parameter[out_params];

				int param = 0;
				for (int i = 0; i < Parameters.FixedParameters.Length; ++i) {
					Parameter p = parameters [i];
					if ((p.ModFlags & Parameter.Modifier.RefOutMask) == 0)
						continue;

					end_params [param++] = new Parameter (new TypeExpression (p.Type, Location),
						p.Name,
						p.ModFlags & Parameter.Modifier.RefOutMask,
						p.OptAttributes == null ? null : p.OptAttributes.Clone (), Location);
				}

				end_parameters = new ParametersCompiled (end_params);
			} else {
				end_parameters = ParametersCompiled.EmptyReadOnlyParameters;
			}

			end_parameters = ParametersCompiled.MergeGenerated (Compiler, end_parameters, false,
				new Parameter (
					new TypeExpression (iasync_result.TypeSpec, Location),
					"result", Parameter.Modifier.NONE, null, Location),
				iasync_result.TypeSpec);

			//
			// Create method, define parameters, register parameters with type system
			//
			EndInvokeBuilder = new Method (this, returnType, MethodModifiers, new MemberName ("EndInvoke"), end_parameters, null);
			EndInvokeBuilder.Define ();
		}

		public override void PrepareEmit ()
		{
			if ((caching_flags & Flags.CloseTypeCreated) != 0)
				return;

			InvokeBuilder.PrepareEmit ();
			if (BeginInvokeBuilder != null) {
				BeginInvokeBuilder.TypeExpression = null;
				EndInvokeBuilder.TypeExpression = null;
				BeginInvokeBuilder.PrepareEmit ();
				EndInvokeBuilder.PrepareEmit ();
			}
		}

		public override void Emit ()
		{
			base.Emit ();

			if (declarative_security != null) {
				foreach (var de in declarative_security) {
#if STATIC
					TypeBuilder.__AddDeclarativeSecurity (de);
#else
					TypeBuilder.AddDeclarativeSecurity (de.Key, de.Value);
#endif
				}
			}

			var rtype = ReturnType.Type;
			if (rtype != null) {
				if (rtype.BuiltinType == BuiltinTypeSpec.Type.Dynamic) {
					Module.PredefinedAttributes.Dynamic.EmitAttribute (CreateReturnBuilder ().Builder);
				} else if (rtype.HasDynamicElement) {
					Module.PredefinedAttributes.Dynamic.EmitAttribute (CreateReturnBuilder ().Builder, rtype, Location);
				} else if (rtype is ReadOnlyReferenceContainer) {
					Module.PredefinedAttributes.IsReadOnly.EmitAttribute (CreateReturnBuilder ().Builder);
				}

				if (rtype.HasNamedTupleElement) {
					Module.PredefinedAttributes.TupleElementNames.EmitAttribute (CreateReturnBuilder ().Builder, rtype, Location);
				}

				ConstraintChecker.Check (this, ReturnType.Type, ReturnType.Location);
			}

			Constructor.ParameterInfo.ApplyAttributes (this, Constructor.ConstructorBuilder);
			Constructor.ConstructorBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			parameters.CheckConstraints (this);
			parameters.ApplyAttributes (this, InvokeBuilder.MethodBuilder);
			InvokeBuilder.MethodBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			if (BeginInvokeBuilder != null) {
				BeginInvokeBuilder.ParameterInfo.ApplyAttributes (this, BeginInvokeBuilder.MethodBuilder);
				EndInvokeBuilder.ParameterInfo.ApplyAttributes (this, EndInvokeBuilder.MethodBuilder);

				BeginInvokeBuilder.MethodBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);
				EndInvokeBuilder.MethodBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);
			}
		}

		protected override TypeSpec[] ResolveBaseTypes (out FullNamedExpression base_class)
		{
			base_type = Compiler.BuiltinTypes.MulticastDelegate;
			base_class = null;
			return null;
		}

		protected override TypeAttributes TypeAttr {
			get {
				return base.TypeAttr | TypeAttributes.Class | TypeAttributes.Sealed;
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

			if (!InvokeBuilder.MemberType.IsCLSCompliant ()) {
				Report.Warning (3002, 1, Location, "Return type of `{0}' is not CLS-compliant",
					GetSignatureForError ());
			}
			return true;
		}


		public static MethodSpec GetConstructor (TypeSpec delType)
		{
			var ctor = MemberCache.FindMember (delType, MemberFilter.Constructor (null), BindingRestriction.DeclaredOnly);
			return (MethodSpec) ctor;
		}

		//
		// Returns the "Invoke" from a delegate type
		//
		public static MethodSpec GetInvokeMethod (TypeSpec delType)
		{
			var invoke = MemberCache.FindMember (delType,
				MemberFilter.Method (InvokeMethodName, 0, null, null),
				BindingRestriction.DeclaredOnly);

			return (MethodSpec) invoke;
		}

		public static AParametersCollection GetParameters (TypeSpec delType)
		{
			var invoke_mb = GetInvokeMethod (delType);
			return invoke_mb.Parameters;
		}

		//
		// 15.2 Delegate compatibility
		//
		public static bool IsTypeCovariant (ResolveContext rc, TypeSpec a, TypeSpec b)
		{
			//
			// For each value parameter (a parameter with no ref or out modifier), an 
			// identity conversion or implicit reference conversion exists from the
			// parameter type in D to the corresponding parameter type in M
			//
			if (a == b)
				return true;

			if (rc.Module.Compiler.Settings.Version == LanguageVersion.ISO_1)
				return false;

			if (a.IsGenericParameter && b.IsGenericParameter)
				return a == b;

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

		public bool AllowSpecialMethodsInvocation { get; set; }

		public override bool ContainsEmitWithAwait ()
		{
			var instance = method_group.InstanceExpression;
			return instance != null && instance.ContainsEmitWithAwait ();
		}

		public static Arguments CreateDelegateMethodArguments (ResolveContext rc, AParametersCollection pd, TypeSpec[] types, Location loc)
		{
			Arguments delegate_arguments = new Arguments (pd.Count);
			for (int i = 0; i < pd.Count; ++i) {
				Argument.AType atype_modifier;
				switch (pd.FixedParameters [i].ModFlags & Parameter.Modifier.RefOutMask) {
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

				var ptype = types[i];
				if (ptype.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
					ptype = rc.BuiltinTypes.Object;

				delegate_arguments.Add (new Argument (new TypeExpression (ptype, loc), atype_modifier));
			}

			return delegate_arguments;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = new Arguments (2);
			args.Add (new Argument (new TypeOf (type, loc)));

			if (method_group.InstanceExpression == null)
				args.Add (new Argument (new NullLiteral (loc)));
			else
				args.Add (new Argument (method_group.InstanceExpression));

			Expression ma;
			var create_v45 = ec.Module.PredefinedMembers.MethodInfoCreateDelegate.Get ();
			if (create_v45 != null) {
				//
				// .NET 4.5 has better API but it produces different instance than Delegate::CreateDelegate
				// and because csc uses this enhancement we have to as well to be fully compatible
				//
				var mg = MethodGroupExpr.CreatePredefined (create_v45, create_v45.DeclaringType, loc);
				mg.InstanceExpression = method_group.CreateExpressionTree (ec);
				ma = mg;
			} else {
				ma = new MemberAccess (new MemberAccess (new QualifiedAliasMember ("global", "System", loc), "Delegate", loc), "CreateDelegate", loc);
				args.Add (new Argument (method_group.CreateExpressionTree (ec)));
			}

			Expression e = new Invocation (ma, args).Resolve (ec);
			if (e == null)
				return null;

			e = Convert.ExplicitConversion (ec, e, type, loc);
			if (e == null)
				return null;

			return e.CreateExpressionTree (ec);
		}

		void ResolveConditionalAccessReceiver (ResolveContext rc)
		{
			// LAMESPEC: Not sure why this is explicitly disallowed with very odd error message
			if (!rc.HasSet (ResolveContext.Options.DontSetConditionalAccessReceiver) && method_group.HasConditionalAccess ()) {
				Error_OperatorCannotBeApplied (rc, loc, "?", method_group.Type);
			}
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			constructor_method = Delegate.GetConstructor (type);

			var invoke_method = Delegate.GetInvokeMethod (type);

			ResolveConditionalAccessReceiver (ec);

			Arguments arguments = CreateDelegateMethodArguments (ec, invoke_method.Parameters, invoke_method.Parameters.Types, loc);
			method_group = method_group.OverloadResolve (ec, ref arguments, this, OverloadResolver.Restrictions.CovariantDelegate);

			if (method_group == null)
				return null;

			var delegate_method = method_group.BestCandidate;
			
			if (delegate_method.DeclaringType.IsNullableType) {
				ec.Report.Error (1728, loc, "Cannot create delegate from method `{0}' because it is a member of System.Nullable<T> type",
					delegate_method.GetSignatureForError ());
				return null;
			}		
			
			if (!AllowSpecialMethodsInvocation)
				Invocation.IsSpecialMethodInvocation (ec, delegate_method, loc);

			ExtensionMethodGroupExpr emg = method_group as ExtensionMethodGroupExpr;
			if (emg != null) {
				method_group.InstanceExpression = emg.ExtensionExpression;
				TypeSpec e_type = emg.ExtensionExpression.Type;
				if (TypeSpec.IsValueType (e_type)) {
					ec.Report.Error (1113, loc, "Extension method `{0}' of value type `{1}' cannot be used to create delegates",
						delegate_method.GetSignatureForError (), e_type.GetSignatureForError ());
				}
			}

			TypeSpec rt = method_group.BestCandidateReturnType;
			if (rt.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
				rt = ec.BuiltinTypes.Object;

			if (!Delegate.IsTypeCovariant (ec, rt, invoke_method.ReturnType)) {
				Error_ConversionFailed (ec, delegate_method, delegate_method.ReturnType);
			}

			if (method_group.IsConditionallyExcluded) {
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
			if (expr != null && (expr.Type.IsGenericParameter || !TypeSpec.IsReferenceType (expr.Type))) {
				if (expr.Type.IsByRefLike || expr.Type.IsSpecialRuntimeType) {
					// CSC: Should be better error code
					Error_ConversionFailed (ec, delegate_method, null);
				}

				method_group.InstanceExpression = new BoxedCast (expr, ec.BuiltinTypes.Object);
			}

			eclass = ExprClass.Value;
			return this;
		}
		
		public override void Emit (EmitContext ec)
		{
			if (method_group.InstanceExpression == null) {
				ec.EmitNull ();
			} else {
				var ie = new InstanceEmitter (method_group.InstanceExpression, false);
				ie.Emit (ec, method_group.ConditionalAccess);
			}

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

		public override void FlowAnalysis (FlowAnalysisContext fc)
		{
			base.FlowAnalysis (fc);
			method_group.FlowAnalysis (fc);
		}

		void Error_ConversionFailed (ResolveContext ec, MethodSpec method, TypeSpec return_type)
		{
			var invoke_method = Delegate.GetInvokeMethod (type);
			string member_name = method_group.InstanceExpression != null ?
				Delegate.FullDelegateDesc (method) :
				TypeManager.GetFullNameSignature (method);

			ec.Report.SymbolRelatedToPreviousError (type);
			ec.Report.SymbolRelatedToPreviousError (method);
			if (ec.Module.Compiler.Settings.Version == LanguageVersion.ISO_1) {
				ec.Report.Error (410, loc, "A method or delegate `{0} {1}' parameters and return type must be same as delegate `{2} {3}' parameters and return type",
					method.ReturnType.GetSignatureForError (), member_name,
					invoke_method.ReturnType.GetSignatureForError (), Delegate.FullDelegateDesc (invoke_method));
				return;
			}

			if (return_type == null) {
				ec.Report.Error (123, loc, "A method or delegate `{0}' parameters do not match delegate `{1}' parameters",
					member_name, Delegate.FullDelegateDesc (invoke_method));
				return;
			}

			if (invoke_method.ReturnType.Kind == MemberKind.ByRef) {
				ec.Report.Error (8189, loc, "By reference return delegate does not match `{0}' return type",
					Delegate.FullDelegateDesc (invoke_method));
				return;
			}

			ec.Report.Error (407, loc, "A method or delegate `{0} {1}' return type does not match delegate `{2} {3}' return type",
				return_type.GetSignatureForError (), member_name,
				invoke_method.ReturnType.GetSignatureForError (), Delegate.FullDelegateDesc (invoke_method));
		}

		public static bool ImplicitStandardConversionExists (ResolveContext ec, MethodGroupExpr mg, TypeSpec target_type)
		{
//			if (target_type == TypeManager.delegate_type || target_type == TypeManager.multicast_delegate_type)
//				return false;

			var invoke = Delegate.GetInvokeMethod (target_type);

			Arguments arguments = CreateDelegateMethodArguments (ec, invoke.Parameters, invoke.Parameters.Types, mg.Location);
			mg = mg.OverloadResolve (ec, ref arguments, null, OverloadResolver.Restrictions.CovariantDelegate | OverloadResolver.Restrictions.ProbingOnly);
			return mg != null && Delegate.IsTypeCovariant (ec, mg.BestCandidateReturnType, invoke.ReturnType);
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
		Field mg_cache;

		public ImplicitDelegateCreation (TypeSpec delegateType, MethodGroupExpr mg, Location loc)
		{
			type = delegateType;
			this.method_group = mg;
			this.loc = loc;
		}

		//
		// Returns true when type is MVAR or has MVAR reference
		//
		public static bool ContainsMethodTypeParameter (TypeSpec type)
		{
			var tps = type as TypeParameterSpec;
			if (tps != null)
				return tps.IsMethodOwned;

			var ec = type as ElementTypeSpec;
			if (ec != null)
				return ContainsMethodTypeParameter (ec.Element);

			foreach (var t in type.TypeArguments) {
				if (ContainsMethodTypeParameter (t)) {
					return true;
				}
			}

			if (type.IsNested)
				return ContainsMethodTypeParameter (type.DeclaringType);

			return false;
		}
		
		bool HasMvar ()
		{
			if (ContainsMethodTypeParameter (type))
				return false;

			var best = method_group.BestCandidate;
			if (ContainsMethodTypeParameter (best.DeclaringType))
				return false;

			if (best.TypeArguments != null) {
				foreach (var ta in best.TypeArguments) {
					if (ContainsMethodTypeParameter (ta))
						return false;
				}
			}

			return true;
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			var expr = base.DoResolve (ec);
			if (expr == null)
				return ErrorExpression.Instance;

			if (ec.IsInProbingMode)
				return expr;

			//
			// Cache any static delegate creation
			//
			if (method_group.InstanceExpression != null)
				return expr;

			//
			// Cannot easily cache types with MVAR
			//
			if (!HasMvar ())
				return expr;

			//
			// Create type level cache for a delegate instance
			//
			var parent = ec.CurrentMemberDefinition.Parent.PartialContainer;
			int id = parent.MethodGroupsCounter++;

			mg_cache = new Field (parent, new TypeExpression (type, loc),
				Modifiers.STATIC | Modifiers.PRIVATE | Modifiers.COMPILER_GENERATED,
				new MemberName (CompilerGeneratedContainer.MakeName (null, "f", "mg$cache", id), loc), null);
			mg_cache.Define ();
			parent.AddField (mg_cache);

			return expr;
		}

		public override void Emit (EmitContext ec)
		{
			Label l_initialized = ec.DefineLabel ();

			if (mg_cache != null) {
				ec.Emit (OpCodes.Ldsfld, mg_cache.Spec);
				ec.Emit (OpCodes.Brtrue_S, l_initialized);
			}

			base.Emit (ec);

			if (mg_cache != null) {
				ec.Emit (OpCodes.Stsfld, mg_cache.Spec);
				ec.MarkLabel (l_initialized);
				ec.Emit (OpCodes.Ldsfld, mg_cache.Spec);
			}
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
			if (ame != null && ec.Module.Compiler.Settings.Version != LanguageVersion.ISO_1) {
				e = ame.Compatible (ec, type);
				if (e == null)
					return null;

				return e.Resolve (ec);
			}

			method_group = e as MethodGroupExpr;
			if (method_group == null) {
				if (e.Type.BuiltinType == BuiltinTypeSpec.Type.Dynamic) {
					e = Convert.ImplicitConversionRequired (ec, e, type, loc);
				} else if (!e.Type.IsDelegate) {
					e.Error_UnexpectedKind (ec, ResolveFlags.MethodGroup | ResolveFlags.Type, loc);
					return null;
				}

				//
				// An argument is not a method but another delegate
				//
				method_group = new MethodGroupExpr (Delegate.GetInvokeMethod (e.Type), e.Type, loc);
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
		readonly bool conditionalAccessReceiver;
		Arguments arguments;
		MethodSpec method;
		
		public DelegateInvocation (Expression instance_expr, Arguments args, bool conditionalAccessReceiver, Location loc)
		{
			this.InstanceExpr = instance_expr;
			this.arguments = args;
			this.conditionalAccessReceiver = conditionalAccessReceiver;
			this.loc = loc;
		}

		public override bool ContainsEmitWithAwait ()
		{
			return InstanceExpr.ContainsEmitWithAwait () || (arguments != null && arguments.ContainsEmitWithAwait ());
		}
		
		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = Arguments.CreateForExpressionTree (ec, this.arguments,
				InstanceExpr.CreateExpressionTree (ec));

			return CreateExpressionFactoryCall (ec, "Invoke", args);
		}

		public override void FlowAnalysis (FlowAnalysisContext fc)
		{
			InstanceExpr.FlowAnalysis (fc);
			if (arguments != null)
				arguments.FlowAnalysis (fc);
		}

		protected override Expression DoResolve (ResolveContext ec)
		{		
			TypeSpec del_type = InstanceExpr.Type;
			if (del_type == null)
				return null;

			//
			// Do only core overload resolution the rest of the checks has been
			// done on primary expression
			//
			method = Delegate.GetInvokeMethod (del_type);
			var res = new OverloadResolver (new MemberSpec[] { method }, OverloadResolver.Restrictions.DelegateInvoke, loc);
			var valid = res.ResolveMember<MethodSpec> (ec, ref arguments);
			if (valid == null && !res.BestCandidateIsDynamic)
				return null;

			type = method.ReturnType;
			if (conditionalAccessReceiver)
				type = LiftMemberType (ec, type);

			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			if (conditionalAccessReceiver) {
				ec.ConditionalAccess = new ConditionalAccessContext (type, ec.DefineLabel ());
			}

			//
			// Invocation on delegates call the virtual Invoke member
			// so we are always `instance' calls
			//
			var call = new CallEmitter ();
			call.InstanceExpression = InstanceExpr;
			call.Emit (ec, method, arguments, loc);

			if (conditionalAccessReceiver)
				ec.CloseConditionalAccess (type.IsNullableType && type !=  method.ReturnType ? type : null);
		}

		public override void EmitStatement (EmitContext ec)
		{
			if (conditionalAccessReceiver) {
				ec.ConditionalAccess = new ConditionalAccessContext (type, ec.DefineLabel ()) {
					Statement = true
				};
			}

			var call = new CallEmitter ();
			call.InstanceExpression = InstanceExpr;
			call.EmitStatement (ec, method, arguments, loc);

			if (conditionalAccessReceiver)
				ec.CloseConditionalAccess (null);
		}

		public override System.Linq.Expressions.Expression MakeExpression (BuilderContext ctx)
		{
			return Invocation.MakeExpression (ctx, InstanceExpr, method, arguments);
		}
	}
}
