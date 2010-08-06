//
// method.cs: Method based declarations
//
// Authors: Miguel de Icaza (miguel@gnu.org)
//          Martin Baulig (martin@ximian.com)
//          Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001, 2002, 2003 Ximian, Inc (http://www.ximian.com)
// Copyright 2004-2008 Novell, Inc
//

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Linq;

#if NET_2_1
using XmlElement = System.Object;
#else
using System.Xml;
#endif

using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp {

	public abstract class MethodCore : InterfaceMemberBase, IParametersMember
	{
		protected ParametersCompiled parameters;
		protected ToplevelBlock block;
		protected MethodSpec spec;

		public MethodCore (DeclSpace parent, GenericMethod generic,
			FullNamedExpression type, Modifiers mod, Modifiers allowed_mod,
			MemberName name, Attributes attrs, ParametersCompiled parameters)
			: base (parent, generic, type, mod, allowed_mod, name, attrs)
		{
			this.parameters = parameters;
		}

		//
		//  Returns the System.Type array for the parameters of this method
		//
		public TypeSpec [] ParameterTypes {
			get {
				return parameters.Types;
			}
		}

		public ParametersCompiled ParameterInfo {
			get {
				return parameters;
			}
		}

		AParametersCollection IParametersMember.Parameters {
			get { return parameters; }
		}
		
		public ToplevelBlock Block {
			get {
				return block;
			}

			set {
				block = value;
			}
		}

		public CallingConventions CallingConventions {
			get {
				CallingConventions cc = parameters.CallingConvention;
				if (!IsInterface)
					if ((ModFlags & Modifiers.STATIC) == 0)
						cc |= CallingConventions.HasThis;

				// FIXME: How is `ExplicitThis' used in C#?
			
				return cc;
			}
		}

		protected override bool CheckOverrideAgainstBase (MemberSpec base_member)
		{
			bool res = base.CheckOverrideAgainstBase (base_member);

			//
			// Check that the permissions are not being changed
			//
			if (!CheckAccessModifiers (this, base_member)) {
				Error_CannotChangeAccessModifiers (this, base_member);
				res = false;
			}

			return res;
		}

		protected override bool CheckBase ()
		{
			// Check whether arguments were correct.
			if (!DefineParameters (parameters))
				return false;

			return base.CheckBase ();
		}

		//
		// Returns a string that represents the signature for this 
		// member which should be used in XML documentation.
		//
		public override string GetDocCommentName (DeclSpace ds)
		{
			return DocUtil.GetMethodDocCommentName (this, parameters, ds);
		}

		//
		// Raised (and passed an XmlElement that contains the comment)
		// when GenerateDocComment is writing documentation expectedly.
		//
		// FIXME: with a few effort, it could be done with XmlReader,
		// that means removal of DOM use.
		//
		internal override void OnGenerateDocComment (XmlElement el)
		{
			DocUtil.OnMethodGenerateDocComment (this, el, Report);
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader 
		{
			get { return "M:"; }
		}

		public override bool EnableOverloadChecks (MemberCore overload)
		{
			if (overload is MethodCore) {
				caching_flags |= Flags.MethodOverloadsExist;
				return true;
			}

			if (overload is AbstractPropertyEventMethod)
				return true;

			return base.EnableOverloadChecks (overload);
		}

		public MethodSpec Spec {
			get { return spec; }
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			if (parameters.HasArglist) {
				Report.Warning (3000, 1, Location, "Methods with variable arguments are not CLS-compliant");
			}

			if (member_type != null && !member_type.IsCLSCompliant ()) {
				Report.Warning (3002, 1, Location, "Return type of `{0}' is not CLS-compliant",
					GetSignatureForError ());
			}

			parameters.VerifyClsCompliance (this);
			return true;
		}
	}

	public interface IGenericMethodDefinition : IMemberDefinition
	{
		TypeParameterSpec[] TypeParameters { get; }
		int TypeParametersCount { get; }

//		MethodInfo MakeGenericMethod (TypeSpec[] targs);
	}

	public class MethodSpec : MemberSpec, IParametersMember
	{
		MethodBase metaInfo;
		AParametersCollection parameters;
		TypeSpec returnType;

		TypeSpec[] targs;
		TypeParameterSpec[] constraints;

		public MethodSpec (MemberKind kind, TypeSpec declaringType, IMemberDefinition details, TypeSpec returnType,
			MethodBase info, AParametersCollection parameters, Modifiers modifiers)
			: base (kind, declaringType, details, modifiers)
		{
			this.metaInfo = info;
			this.parameters = parameters;
			this.returnType = returnType;
		}

		#region Properties

		public override int Arity {
			get {
				return IsGeneric ? GenericDefinition.TypeParametersCount : 0;
			}
		}

		public TypeParameterSpec[] Constraints {
			get {
				if (constraints == null && IsGeneric)
					constraints = GenericDefinition.TypeParameters;

				return constraints;
			}
		}

		public bool IsConstructor {
			get {
				return Kind == MemberKind.Constructor;
			}
		}

		public IGenericMethodDefinition GenericDefinition {
			get {
				return (IGenericMethodDefinition) definition;
			}
		}

		public bool IsExtensionMethod {
			get {
				return IsStatic && parameters.HasExtensionMethodType;
			}
		}

		public bool IsSealed {
			get {
				return (Modifiers & Modifiers.SEALED) != 0;
			}
		}

		// When is virtual or abstract
		public bool IsVirtual {
			get {
				return (Modifiers & (Modifiers.VIRTUAL | Modifiers.ABSTRACT | Modifiers.OVERRIDE)) != 0;
			}
		}

		public bool IsReservedMethod {
			get {
				return Kind == MemberKind.Operator || IsAccessor;
			}
		}

		TypeSpec IInterfaceMemberSpec.MemberType {
			get {
				return returnType;
			}
		}

		public AParametersCollection Parameters {
			get { 
				return parameters;
			}
		}

		public TypeSpec ReturnType {
			get {
				return returnType;
			}
		}

		public TypeSpec[] TypeArguments {
			get {
				return targs;
			}
		}

		#endregion

		public MethodSpec GetGenericMethodDefinition ()
		{
			if (!IsGeneric && !DeclaringType.IsGeneric)
				return this;

			return MemberCache.GetMember (declaringType, this);
		}

		public MethodBase GetMetaInfo ()
		{
			if ((state & StateFlags.PendingMetaInflate) != 0) {
				if (DeclaringType.IsTypeBuilder) {
					if (IsConstructor)
						metaInfo = TypeBuilder.GetConstructor (DeclaringType.GetMetaInfo (), (ConstructorInfo) metaInfo);
					else
						metaInfo = TypeBuilder.GetMethod (DeclaringType.GetMetaInfo (), (MethodInfo) metaInfo);
				} else {
					metaInfo = MethodInfo.GetMethodFromHandle (metaInfo.MethodHandle, DeclaringType.GetMetaInfo ().TypeHandle);
				}

				state &= ~StateFlags.PendingMetaInflate;
			}

			if ((state & StateFlags.PendingMakeMethod) != 0) {
				metaInfo = ((MethodInfo) metaInfo).MakeGenericMethod (targs.Select (l => l.GetMetaInfo ()).ToArray ());
				state &= ~StateFlags.PendingMakeMethod;
			}

			return metaInfo;
		}

		public override string GetSignatureForError ()
		{
			string name;
			if (IsConstructor) {
				name = DeclaringType.GetSignatureForError () + "." + DeclaringType.Name;
			} else if (Kind == MemberKind.Operator) {
				var op = Operator.GetType (Name).Value;
				if (op == Operator.OpType.Implicit || op == Operator.OpType.Explicit) {
					name = DeclaringType.GetSignatureForError () + "." + Operator.GetName (op) + " operator " + returnType.GetSignatureForError ();
				} else {
					name = DeclaringType.GetSignatureForError () + ".operator " + Operator.GetName (op);
				}
			} else if (IsAccessor) {
				int split = Name.IndexOf ('_');
				name = Name.Substring (split + 1);
				var postfix = Name.Substring (0, split);
				if (split == 3) {
					var pc = parameters.Count;
					if (pc > 0 && postfix == "get") {
						name = "this" + parameters.GetSignatureForError ("[", "]", pc);
					} else if (pc > 1 && postfix == "set") {
						name = "this" + parameters.GetSignatureForError ("[", "]", pc - 1);
					}
				}

				return DeclaringType.GetSignatureForError () + "." + name + "." + postfix;
			} else {
				name = base.GetSignatureForError ();
				if (targs != null)
					name += "<" + TypeManager.CSharpName (targs) + ">";
				else if (IsGeneric)
					name += "<" + TypeManager.CSharpName (GenericDefinition.TypeParameters) + ">";
			}

			return name + parameters.GetSignatureForError ();
		}

		public override MemberSpec InflateMember (TypeParameterInflator inflator)
		{
			var ms = (MethodSpec) base.InflateMember (inflator);
			ms.returnType = inflator.Inflate (returnType);
			ms.parameters = parameters.Inflate (inflator);
			if (IsGeneric)
				ms.constraints = TypeParameterSpec.InflateConstraints (inflator, GenericDefinition.TypeParameters);

			return ms;
		}

		public MethodSpec MakeGenericMethod (params TypeSpec[] targs)
		{
			if (targs == null)
				throw new ArgumentNullException ();
// TODO MemberCache
//			if (generic_intances != null && generic_intances.TryGetValue (targs, out ginstance))
//				return ginstance;

			//if (generic_intances == null)
			//    generic_intances = new Dictionary<TypeSpec[], Method> (TypeSpecArrayComparer.Default);

			var inflator = new TypeParameterInflator (DeclaringType, GenericDefinition.TypeParameters, targs);

			var inflated = (MethodSpec) MemberwiseClone ();
			inflated.declaringType = inflator.TypeInstance;
			inflated.returnType = inflator.Inflate (returnType);
			inflated.parameters = parameters.Inflate (inflator);
			inflated.targs = targs;
			inflated.constraints = TypeParameterSpec.InflateConstraints (inflator, constraints ?? GenericDefinition.TypeParameters);
			inflated.state |= StateFlags.PendingMakeMethod;

			//			if (inflated.parent == null)
			//				inflated.parent = parent;

			//generic_intances.Add (targs, inflated);
			return inflated;
		}

		public MethodSpec Mutate (TypeParameterMutator mutator)
		{
			var targs = TypeArguments;
			if (targs != null)
				targs = mutator.Mutate (targs);

			var decl = DeclaringType;
			if (DeclaringType.IsGenericOrParentIsGeneric) {
				decl = mutator.Mutate (decl);
			}

			if (targs == TypeArguments && decl == DeclaringType)
				return this;

			var ms = (MethodSpec) MemberwiseClone ();
			if (decl != DeclaringType) {
				// Gets back MethodInfo in case of metaInfo was inflated
				ms.metaInfo = MemberCache.GetMember (DeclaringType.GetDefinition (), this).metaInfo;

				ms.declaringType = decl;
				ms.state |= StateFlags.PendingMetaInflate;
			}

			if (targs != null) {
				ms.targs = targs;
				ms.state |= StateFlags.PendingMakeMethod;
			}

			return ms;
		}

		public void SetMetaInfo (MethodInfo info)
		{
			if (this.metaInfo != null)
				throw new InternalErrorException ("MetaInfo reset");

			this.metaInfo = info;
		}
	}

	public abstract class MethodOrOperator : MethodCore, IMethodData
	{
		public MethodBuilder MethodBuilder;
		ReturnParameter return_attributes;
		Dictionary<SecurityAction, PermissionSet> declarative_security;
		protected MethodData MethodData;

		static string[] attribute_targets = new string [] { "method", "return" };

		protected MethodOrOperator (DeclSpace parent, GenericMethod generic, FullNamedExpression type, Modifiers mod,
				Modifiers allowed_mod, MemberName name,
				Attributes attrs, ParametersCompiled parameters)
			: base (parent, generic, type, mod, allowed_mod, name,
					attrs, parameters)
		{
		}

		public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Target == AttributeTargets.ReturnValue) {
				if (return_attributes == null)
					return_attributes = new ReturnParameter (this, MethodBuilder, Location);

				return_attributes.ApplyAttributeBuilder (a, ctor, cdata, pa);
				return;
			}

			if (a.IsInternalMethodImplAttribute) {
				is_external_implementation = true;
			}

			if (a.Type == pa.DllImport) {
				const Modifiers extern_static = Modifiers.EXTERN | Modifiers.STATIC;
				if ((ModFlags & extern_static) != extern_static) {
					Report.Error (601, a.Location, "The DllImport attribute must be specified on a method marked `static' and `extern'");
				}
				is_external_implementation = true;
			}

			if (a.IsValidSecurityAttribute ()) {
				if (declarative_security == null)
					declarative_security = new Dictionary<SecurityAction, PermissionSet> ();
				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			if (MethodBuilder != null)
				MethodBuilder.SetCustomAttribute ((ConstructorInfo) ctor.GetMetaInfo (), cdata);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Method; 
			}
		}

		protected override bool CheckForDuplications ()
		{
			return Parent.MemberCache.CheckExistingMembersOverloads (this, parameters);
		}

		public virtual EmitContext CreateEmitContext (ILGenerator ig)
		{
			return new EmitContext (this, ig, MemberType);
		}

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			if (!CheckBase ())
				return false;

			MemberKind kind;
			if (this is Operator)
				kind = MemberKind.Operator;
			else if (this is Destructor)
				kind = MemberKind.Destructor;
			else
				kind = MemberKind.Method;

			if (IsPartialDefinition) {
				caching_flags &= ~Flags.Excluded_Undetected;
				caching_flags |= Flags.Excluded;

				// Add to member cache only when a partial method implementation has not been found yet
				if ((caching_flags & Flags.PartialDefinitionExists) == 0) {
//					MethodBase mb = new PartialMethodDefinitionInfo (this);

					spec = new MethodSpec (kind, Parent.Definition, this, ReturnType, null, parameters, ModFlags);
					Parent.MemberCache.AddMember (spec);
				}

				return true;
			}

			MethodData = new MethodData (
				this, ModFlags, flags, this, MethodBuilder, GenericMethod, base_method);

			if (!MethodData.Define (Parent.PartialContainer, GetFullName (MemberName), Report))
				return false;
					
			MethodBuilder = MethodData.MethodBuilder;

			spec = new MethodSpec (kind, Parent.Definition, this, ReturnType, MethodBuilder, parameters, ModFlags);
			if (MemberName.Arity > 0)
				spec.IsGeneric = true;
			
			Parent.MemberCache.AddMember (this, MethodBuilder.Name, spec);

			return true;
		}

		protected override void DoMemberTypeIndependentChecks ()
		{
			base.DoMemberTypeIndependentChecks ();

			CheckAbstractAndExtern (block != null);

			if ((ModFlags & Modifiers.PARTIAL) != 0) {
				for (int i = 0; i < parameters.Count; ++i) {
					IParameterData p = parameters.FixedParameters [i];
					if (p.ModFlags == Parameter.Modifier.OUT) {
						Report.Error (752, Location, "`{0}': A partial method parameters cannot use `out' modifier",
							GetSignatureForError ());
					}

					if (p.HasDefaultValue && IsPartialImplementation)
						((Parameter) p).Warning_UselessOptionalParameter (Report);
				}
			}
		}

		protected override void DoMemberTypeDependentChecks ()
		{
			base.DoMemberTypeDependentChecks ();

			if (MemberType.IsStatic) {
				Error_StaticReturnType ();
			}
		}

		public override void Emit ()
		{
			if ((ModFlags & Modifiers.COMPILER_GENERATED) != 0 && !Parent.IsCompilerGenerated)
				PredefinedAttributes.Get.CompilerGenerated.EmitAttribute (MethodBuilder);
			if ((ModFlags & Modifiers.DEBUGGER_HIDDEN) != 0)
				PredefinedAttributes.Get.DebuggerHidden.EmitAttribute (MethodBuilder);

			if (ReturnType == InternalType.Dynamic) {
				return_attributes = new ReturnParameter (this, MethodBuilder, Location);
				PredefinedAttributes.Get.Dynamic.EmitAttribute (return_attributes.Builder);
			} else {
				var trans_flags = TypeManager.HasDynamicTypeUsed (ReturnType);
				if (trans_flags != null) {
					var pa = PredefinedAttributes.Get.DynamicTransform;
					if (pa.Constructor != null || pa.ResolveConstructor (Location, ArrayContainer.MakeType (TypeManager.bool_type))) {
						return_attributes = new ReturnParameter (this, MethodBuilder, Location);
						return_attributes.Builder.SetCustomAttribute (
							new CustomAttributeBuilder (pa.Constructor, new object [] { trans_flags }));
					}
				}
			}

			if (OptAttributes != null)
				OptAttributes.Emit ();

			if (declarative_security != null) {
				foreach (var de in declarative_security) {
					MethodBuilder.AddDeclarativeSecurity (de.Key, de.Value);
				}
			}

			if (MethodData != null)
				MethodData.Emit (Parent);

			base.Emit ();

			Block = null;
			MethodData = null;
		}

		protected void Error_ConditionalAttributeIsNotValid ()
		{
			Report.Error (577, Location,
				"Conditional not valid on `{0}' because it is a constructor, destructor, operator or explicit interface implementation",
				GetSignatureForError ());
		}

		public bool IsPartialDefinition {
			get {
				return (ModFlags & Modifiers.PARTIAL) != 0 && Block == null;
			}
		}

		public bool IsPartialImplementation {
			get {
				return (ModFlags & Modifiers.PARTIAL) != 0 && Block != null;
			}
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		#region IMethodData Members

		public TypeSpec ReturnType {
			get {
				return MemberType;
			}
		}

		public MemberName MethodName {
			get {
				return MemberName;
			}
		}

		/// <summary>
		/// Returns true if method has conditional attribute and the conditions is not defined (method is excluded).
		/// </summary>
		public override string[] ConditionalConditions ()
		{
			if ((caching_flags & (Flags.Excluded_Undetected | Flags.Excluded)) == 0)
				return null;

			if ((ModFlags & Modifiers.PARTIAL) != 0 && (caching_flags & Flags.Excluded) != 0)
				return new string [0];

			caching_flags &= ~Flags.Excluded_Undetected;
			string[] conditions;

			if (base_method == null) {
				if (OptAttributes == null)
					return null;

				Attribute[] attrs = OptAttributes.SearchMulti (PredefinedAttributes.Get.Conditional);
				if (attrs == null)
					return null;

				conditions = new string[attrs.Length];
				for (int i = 0; i < conditions.Length; ++i)
					conditions[i] = attrs[i].GetConditionalAttributeValue ();
			} else {
				conditions = base_method.MemberDefinition.ConditionalConditions();
			}

			if (conditions != null)
				caching_flags |= Flags.Excluded;

			return conditions;
		}

		GenericMethod IMethodData.GenericMethod {
			get {
				return GenericMethod;
			}
		}

		public virtual void EmitExtraSymbolInfo (SourceMethod source)
		{ }

		#endregion

	}

	public class SourceMethod : IMethodDef
	{
		MethodBase method;
		SourceMethodBuilder builder;

		protected SourceMethod (DeclSpace parent, MethodBase method, ICompileUnit file)
		{
			this.method = method;
			
			builder = SymbolWriter.OpenMethod (file, parent.NamespaceEntry.SymbolFileID, this);
		}

		public string Name {
			get { return method.Name; }
		}

		public int Token {
			get {
				if (method is MethodBuilder)
					return ((MethodBuilder) method).GetToken ().Token;
				else if (method is ConstructorBuilder)
					return ((ConstructorBuilder) method).GetToken ().Token;
				else
					throw new NotSupportedException ();
			}
		}

		public void CloseMethod ()
		{
			SymbolWriter.CloseMethod ();
		}

		public void SetRealMethodName (string name)
		{
			if (builder != null)
				builder.SetRealMethodName (name);
		}

		public static SourceMethod Create (DeclSpace parent, MethodBase method, Block block)
		{
			if (!SymbolWriter.HasSymbolWriter)
				return null;
			if (block == null)
				return null;

			Location start_loc = block.StartLocation;
			if (start_loc.IsNull)
				return null;

			ICompileUnit compile_unit = start_loc.CompilationUnit;
			if (compile_unit == null)
				return null;

			return new SourceMethod (parent, method, compile_unit);
		}
	}

	public class Method : MethodOrOperator, IGenericMethodDefinition
	{
		Method partialMethodImplementation;

		public Method (DeclSpace parent, GenericMethod generic,
			       FullNamedExpression return_type, Modifiers mod,
			       MemberName name, ParametersCompiled parameters, Attributes attrs)
			: base (parent, generic, return_type, mod,
				parent.PartialContainer.Kind == MemberKind.Interface ? AllowedModifiersClass :
				parent.PartialContainer.Kind == MemberKind.Struct ? AllowedModifiersStruct :
				AllowedModifiersClass,
				name, attrs, parameters)
		{
		}

		protected Method (DeclSpace parent, FullNamedExpression return_type, Modifiers mod, Modifiers amod,
					MemberName name, ParametersCompiled parameters, Attributes attrs)
			: base (parent, null, return_type, mod, amod, name, attrs, parameters)
		{
		}

#region Properties

		public override TypeParameter[] CurrentTypeParameters {
			get {
				if (GenericMethod != null)
					return GenericMethod.CurrentTypeParameters;

				return null;
			}
		}

		public override bool HasUnresolvedConstraints {
			get {
				if (CurrentTypeParameters == null)
					return false;

				// When overriding base method constraints are fetched from
				// base method but to find it we have to resolve parameters
				// to find exact base method match
				if (IsExplicitImpl || (ModFlags & Modifiers.OVERRIDE) != 0)
					return base_method == null;

				// Even for non-override generic method constraints check has to be
				// delayed after all constraints are resolved
				return true;
			}
		}

		public TypeParameterSpec[] TypeParameters {
			get {
				return CurrentTypeParameters.Select (l => l.Type).ToArray ();
			}
		}

		public int TypeParametersCount {
			get {
				return CurrentTypeParameters == null ? 0 : CurrentTypeParameters.Length;
			}
		}

#endregion

		public override string GetSignatureForError()
		{
			return base.GetSignatureForError () + parameters.GetSignatureForError ();
		}

		void Error_DuplicateEntryPoint (Method b)
		{
			Report.Error (17, b.Location,
				"Program `{0}' has more than one entry point defined: `{1}'",
				CodeGen.FileName, b.GetSignatureForError ());
		}

		bool IsEntryPoint ()
		{
			if (ReturnType != TypeManager.void_type &&
				ReturnType != TypeManager.int32_type)
				return false;

			if (parameters.IsEmpty)
				return true;

			if (parameters.Count > 1)
				return false;

			var ac = parameters.Types [0] as ArrayContainer;
			return ac != null && ac.Rank == 1 && ac.Element == TypeManager.string_type &&
					(parameters[0].ModFlags & ~Parameter.Modifier.PARAMS) == Parameter.Modifier.NONE;
		}

		public override FullNamedExpression LookupNamespaceOrType (string name, int arity, Location loc, bool ignore_cs0104)
		{
			if (arity == 0) {
				TypeParameter[] tp = CurrentTypeParameters;
				if (tp != null) {
					TypeParameter t = TypeParameter.FindTypeParameter (tp, name);
					if (t != null)
						return new TypeParameterExpr (t, loc);
				}
			}

			return base.LookupNamespaceOrType (name, arity, loc, ignore_cs0104);
		}

		public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Type == pa.Conditional) {
				if (IsExplicitImpl) {
					Error_ConditionalAttributeIsNotValid ();
					return;
				}

				if ((ModFlags & Modifiers.OVERRIDE) != 0) {
					Report.Error (243, Location, "Conditional not valid on `{0}' because it is an override method", GetSignatureForError ());
					return;
				}

				if (ReturnType != TypeManager.void_type) {
					Report.Error (578, Location, "Conditional not valid on `{0}' because its return type is not void", GetSignatureForError ());
					return;
				}

				if (IsInterface) {
					Report.Error (582, Location, "Conditional not valid on interface members");
					return;
				}

				if (MethodData.implementing != null) {
					Report.SymbolRelatedToPreviousError (MethodData.implementing.DeclaringType);
					Report.Error (629, Location, "Conditional member `{0}' cannot implement interface member `{1}'",
						GetSignatureForError (), TypeManager.CSharpSignature (MethodData.implementing));
					return;
				}

				for (int i = 0; i < parameters.Count; ++i) {
					if (parameters.FixedParameters [i].ModFlags == Parameter.Modifier.OUT) {
						Report.Error (685, Location, "Conditional method `{0}' cannot have an out parameter", GetSignatureForError ());
						return;
					}
				}
			}

			if (a.Type == pa.Extension) {
				a.Error_MisusedExtensionAttribute ();
				return;
			}

			base.ApplyAttributeBuilder (a, ctor, cdata, pa);
		}

		protected virtual void DefineTypeParameters ()
		{
			var tparams = CurrentTypeParameters;

			TypeParameterSpec[] base_tparams = null;
			TypeParameterSpec[] base_decl_tparams = TypeParameterSpec.EmptyTypes;
			TypeSpec[] base_targs = TypeSpec.EmptyTypes;
			if (((ModFlags & Modifiers.OVERRIDE) != 0 || IsExplicitImpl)) {
				if (base_method != null) {
					base_tparams = base_method.GenericDefinition.TypeParameters;
					if (base_method.DeclaringType.IsGeneric) {
						base_decl_tparams = base_method.DeclaringType.MemberDefinition.TypeParameters;
						base_targs = Parent.BaseType.TypeArguments;
					}
				} else if (MethodData.implementing != null) {
					base_tparams = MethodData.implementing.GenericDefinition.TypeParameters;
					if (MethodData.implementing.DeclaringType.IsGeneric) {
						base_decl_tparams = MethodData.implementing.DeclaringType.MemberDefinition.TypeParameters;
						foreach (var iface in Parent.CurrentType.Interfaces) {
							if (iface == MethodData.implementing.DeclaringType) {
								base_targs = iface.TypeArguments;
								break;
							}
						}
					}
				}
			}

			for (int i = 0; i < tparams.Length; ++i) {
				var tp = tparams[i];

				if (!tp.ResolveConstraints (this))
					continue;

				//
				// Copy base constraints for override/explicit methods
				//
				if (base_tparams != null) {
					var base_tparam = base_tparams[i];
					tp.Type.SpecialConstraint = base_tparam.SpecialConstraint;

					var inflator = new TypeParameterInflator (CurrentType, base_decl_tparams, base_targs);
					base_tparam.InflateConstraints (inflator, tp.Type);
				} else if (MethodData.implementing != null) {
					var base_tp = MethodData.implementing.Constraints[i];
					if (!tp.Type.HasSameConstraintsImplementation (base_tp)) {
						Report.SymbolRelatedToPreviousError (MethodData.implementing);
						Report.Error (425, Location,
							"The constraints for type parameter `{0}' of method `{1}' must match the constraints for type parameter `{2}' of interface method `{3}'. Consider using an explicit interface implementation instead",
							tp.GetSignatureForError (), GetSignatureForError (), base_tp.GetSignatureForError (), MethodData.implementing.GetSignatureForError ());
					}
				}
			}
		}

		//
		// Creates the type
		//
		public override bool Define ()
		{
			if (type_expr.Type == TypeManager.void_type && parameters.IsEmpty && MemberName.Arity == 0 && MemberName.Name == Destructor.MetadataName) {
				Report.Warning (465, 1, Location, "Introducing `Finalize' method can interfere with destructor invocation. Did you intend to declare a destructor?");
			}

			if (!base.Define ())
				return false;

			if (partialMethodImplementation != null && IsPartialDefinition)
				MethodBuilder = partialMethodImplementation.MethodBuilder;

			if (RootContext.StdLib && TypeManager.IsSpecialType (ReturnType)) {
				Error1599 (Location, ReturnType, Report);
				return false;
			}

			if (CurrentTypeParameters == null) {
				if (base_method != null) {
					if (parameters.Count == 1 && ParameterTypes[0] == TypeManager.object_type && Name == "Equals")
						Parent.PartialContainer.Mark_HasEquals ();
					else if (parameters.IsEmpty && Name == "GetHashCode")
						Parent.PartialContainer.Mark_HasGetHashCode ();
				}
					
			} else {
				DefineTypeParameters ();
			}

			if (block != null && block.IsIterator && !(Parent is IteratorStorey)) {
				//
				// Current method is turned into automatically generated
				// wrapper which creates an instance of iterator
				//
				Iterator.CreateIterator (this, Parent.PartialContainer, ModFlags, Compiler);
				ModFlags |= Modifiers.DEBUGGER_HIDDEN;
			}

			if ((ModFlags & Modifiers.STATIC) == 0)
				return true;

			if (parameters.HasExtensionMethodType) {
				if (Parent.PartialContainer.IsStatic && !Parent.IsGeneric) {
					if (!Parent.IsTopLevel)
						Report.Error (1109, Location, "`{0}': Extension methods cannot be defined in a nested class",
							GetSignatureForError ());

					PredefinedAttribute pa = PredefinedAttributes.Get.Extension;
					if (!pa.IsDefined) {
						Report.Error (1110, Location,
							"`{0}': Extension methods cannot be declared without a reference to System.Core.dll assembly. Add the assembly reference or remove `this' modifer from the first parameter",
							GetSignatureForError ());
					}

					ModFlags |= Modifiers.METHOD_EXTENSION;
					Parent.PartialContainer.ModFlags |= Modifiers.METHOD_EXTENSION;
					Spec.DeclaringType.SetExtensionMethodContainer ();
					CodeGen.Assembly.HasExtensionMethods = true;
				} else {
					Report.Error (1106, Location, "`{0}': Extension methods must be defined in a non-generic static class",
						GetSignatureForError ());
				}
			}

			//
			// This is used to track the Entry Point,
			//
			if (RootContext.NeedsEntryPoint &&
				Name == "Main" &&
				(RootContext.MainClass == null ||
				RootContext.MainClass == Parent.TypeBuilder.FullName)){
				if (IsEntryPoint ()) {

					if (RootContext.EntryPoint == null) {
						if (Parent.IsGeneric || MemberName.IsGeneric) {
							Report.Warning (402, 4, Location, "`{0}': an entry point cannot be generic or in a generic type",
								GetSignatureForError ());
						} else {
							SetIsUsed ();
							RootContext.EntryPoint = this;
						}
					} else {
						Error_DuplicateEntryPoint (RootContext.EntryPoint);
						Error_DuplicateEntryPoint (this);
					}
				} else {
					Report.Warning (28, 4, Location, "`{0}' has the wrong signature to be an entry point",
						GetSignatureForError ());
				}
			}

			return true;
		}

		//
		// Emits the code
		// 
		public override void Emit ()
		{
			try {
				Report.Debug (64, "METHOD EMIT", this, MethodBuilder, Location, Block, MethodData);
				if (IsPartialDefinition) {
					//
					// Use partial method implementation builder for partial method declaration attributes
					//
					if (partialMethodImplementation != null) {
						MethodBuilder = partialMethodImplementation.MethodBuilder;
						return;
					}
				} else if ((ModFlags & Modifiers.PARTIAL) != 0 && (caching_flags & Flags.PartialDefinitionExists) == 0) {
					Report.Error (759, Location, "A partial method `{0}' implementation is missing a partial method declaration",
						GetSignatureForError ());
				}

				if (CurrentTypeParameters != null) {
					var ge = type_expr as GenericTypeExpr;
					if (ge != null)
						ge.CheckConstraints (this);

					foreach (Parameter p in parameters.FixedParameters) {
						ge = p.TypeExpression as GenericTypeExpr;
						if (ge != null)
							ge.CheckConstraints (this);
					}

					for (int i = 0; i < CurrentTypeParameters.Length; ++i) {
						var tp = CurrentTypeParameters [i];
						tp.CheckGenericConstraints ();
						tp.Emit ();
					}
				}

				base.Emit ();
				
				if ((ModFlags & Modifiers.METHOD_EXTENSION) != 0)
					PredefinedAttributes.Get.Extension.EmitAttribute (MethodBuilder);
			} catch {
				Console.WriteLine ("Internal compiler error at {0}: exception caught while emitting {1}",
						   Location, MethodBuilder);
				throw;
			}
		}

		public override bool EnableOverloadChecks (MemberCore overload)
		{
			// TODO: It can be deleted when members will be defined in correct order
			if (overload is Operator)
				return overload.EnableOverloadChecks (this);

			if (overload is Indexer)
				return false;

			return base.EnableOverloadChecks (overload);
		}

		public static void Error1599 (Location loc, TypeSpec t, Report Report)
		{
			Report.Error (1599, loc, "Method or delegate cannot return type `{0}'", TypeManager.CSharpName (t));
		}

		protected override bool ResolveMemberType ()
		{
			if (GenericMethod != null) {
				MethodBuilder = Parent.TypeBuilder.DefineMethod (GetFullName (MemberName), flags);
				if (!GenericMethod.Define (this))
					return false;
			}

			return base.ResolveMemberType ();
		}

		public void SetPartialDefinition (Method methodDefinition)
		{
			caching_flags |= Flags.PartialDefinitionExists;
			methodDefinition.partialMethodImplementation = this;

			// Ensure we are always using method declaration parameters
			for (int i = 0; i < methodDefinition.parameters.Count; ++i ) {
				parameters [i].Name = methodDefinition.parameters [i].Name;
				parameters [i].DefaultValue = methodDefinition.parameters [i].DefaultValue;
			}

			if (methodDefinition.attributes == null)
				return;

			if (attributes == null) {
				attributes = methodDefinition.attributes;
			} else {
				attributes.Attrs.AddRange (methodDefinition.attributes.Attrs);
			}
		}
	}

	public abstract class ConstructorInitializer : ExpressionStatement
	{
		Arguments argument_list;
		MethodSpec base_ctor;

		public ConstructorInitializer (Arguments argument_list, Location loc)
		{
			this.argument_list = argument_list;
			this.loc = loc;
		}

		public Arguments Arguments {
			get {
				return argument_list;
			}
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			eclass = ExprClass.Value;

			// FIXME: Hack
			var caller_builder = (Constructor) ec.MemberContext;

			if (argument_list != null) {
				bool dynamic;

				//
				// Spec mandates that constructor initializer will not have `this' access
				//
				using (ec.Set (ResolveContext.Options.BaseInitializer)) {
					argument_list.Resolve (ec, out dynamic);
				}

				if (dynamic) {
					ec.Report.Error (1975, loc,
						"The constructor call cannot be dynamically dispatched within constructor initializer");

					return null;
				}
			}

			type = ec.CurrentType;
			if (this is ConstructorBaseInitializer) {
				if (ec.CurrentType.BaseType == null)
					return this;

				type = ec.CurrentType.BaseType;
				if (ec.CurrentType.IsStruct) {
					ec.Report.Error (522, loc,
						"`{0}': Struct constructors cannot call base constructors", caller_builder.GetSignatureForError ());
					return this;
				}
			} else {
				//
				// It is legal to have "this" initializers that take no arguments
				// in structs, they are just no-ops.
				//
				// struct D { public D (int a) : this () {}
				//
				if (TypeManager.IsStruct (ec.CurrentType) && argument_list == null)
					return this;			
			}

			base_ctor = ConstructorLookup (ec, type, ref argument_list, loc);
	
			// TODO MemberCache: Does it work for inflated types ?
			if (base_ctor == caller_builder.Spec){
				ec.Report.Error (516, loc, "Constructor `{0}' cannot call itself",
					caller_builder.GetSignatureForError ());
			}
						
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			// It can be null for static initializers
			if (base_ctor == null)
				return;
			
			ec.Mark (loc);

			Invocation.EmitCall (ec, new CompilerGeneratedThis (type, loc), base_ctor, argument_list, loc);
		}

		public override void EmitStatement (EmitContext ec)
		{
			Emit (ec);
		}
	}

	public class ConstructorBaseInitializer : ConstructorInitializer {
		public ConstructorBaseInitializer (Arguments argument_list, Location l) :
			base (argument_list, l)
		{
		}
	}

	class GeneratedBaseInitializer: ConstructorBaseInitializer {
		public GeneratedBaseInitializer (Location loc):
			base (null, loc)
		{
		}
	}

	public class ConstructorThisInitializer : ConstructorInitializer {
		public ConstructorThisInitializer (Arguments argument_list, Location l) :
			base (argument_list, l)
		{
		}
	}
	
	public class Constructor : MethodCore, IMethodData {
		public ConstructorBuilder ConstructorBuilder;
		public ConstructorInitializer Initializer;
		Dictionary<SecurityAction, PermissionSet> declarative_security;
		bool has_compliant_args;

		// <summary>
		//   Modifiers allowed for a constructor.
		// </summary>
		public const Modifiers AllowedModifiers =
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.STATIC |
			Modifiers.UNSAFE |
			Modifiers.EXTERN |		
			Modifiers.PRIVATE;

		static readonly string[] attribute_targets = new string [] { "method" };

		//
		// The spec claims that static is not permitted, but
		// my very own code has static constructors.
		//
		public Constructor (DeclSpace parent, string name, Modifiers mod, Attributes attrs, ParametersCompiled args,
				    ConstructorInitializer init, Location loc)
			: base (parent, null, null, mod, AllowedModifiers,
				new MemberName (name, loc), attrs, args)
		{
			Initializer = init;
		}

		public bool HasCompliantArgs {
			get { return has_compliant_args; }
		}

		public override AttributeTargets AttributeTargets {
			get { return AttributeTargets.Constructor; }
		}

		//
		// Returns true if this is a default constructor
		//
		public bool IsDefault ()
		{
			if ((ModFlags & Modifiers.STATIC) != 0)
				return parameters.IsEmpty;

			return parameters.IsEmpty &&
					(Initializer is ConstructorBaseInitializer) &&
					(Initializer.Arguments == null);
		}

		public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.IsValidSecurityAttribute ()) {
				if (declarative_security == null) {
					declarative_security = new Dictionary<SecurityAction, PermissionSet> ();
				}
				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			if (a.IsInternalMethodImplAttribute) {
				is_external_implementation = true;
			}

			ConstructorBuilder.SetCustomAttribute ((ConstructorInfo) ctor.GetMetaInfo (), cdata);
		}

		protected override bool CheckBase ()
		{
			if ((ModFlags & Modifiers.STATIC) != 0) {
				if (!parameters.IsEmpty) {
					Report.Error (132, Location, "`{0}': The static constructor must be parameterless",
						GetSignatureForError ());
					return false;
				}

				// the rest can be ignored
				return true;
			}

			// Check whether arguments were correct.
			if (!DefineParameters (parameters))
				return false;

			if ((caching_flags & Flags.MethodOverloadsExist) != 0)
				Parent.MemberCache.CheckExistingMembersOverloads (this, parameters);

			if (Parent.PartialContainer.Kind == MemberKind.Struct && parameters.IsEmpty) {
				Report.Error (568, Location, 
					"Structs cannot contain explicit parameterless constructors");
				return false;
			}

			CheckProtectedModifier ();
			
			return true;
		}
		
		//
		// Creates the ConstructorBuilder
		//
		public override bool Define ()
		{
			if (ConstructorBuilder != null)
				return true;

			var ca = MethodAttributes.RTSpecialName | MethodAttributes.SpecialName;
			
			if ((ModFlags & Modifiers.STATIC) != 0) {
				ca |= MethodAttributes.Static | MethodAttributes.Private;
			} else {
				ca |= ModifiersExtensions.MethodAttr (ModFlags);
			}

			if (!CheckAbstractAndExtern (block != null))
				return false;
			
			// Check if arguments were correct.
			if (!CheckBase ())
				return false;

			ConstructorBuilder = Parent.TypeBuilder.DefineConstructor (
				ca, CallingConventions,
				parameters.GetMetaInfo ());

			spec = new MethodSpec (MemberKind.Constructor, Parent.Definition, this, TypeManager.void_type, ConstructorBuilder, parameters, ModFlags);
			
			Parent.MemberCache.AddMember (spec);
			
			// It's here only to report an error
			if (block != null && block.IsIterator) {
				member_type = TypeManager.void_type;
				Iterator.CreateIterator (this, Parent.PartialContainer, ModFlags, Compiler);
			}

			return true;
		}

		//
		// Emits the code
		//
		public override void Emit ()
		{
			if (Parent.PartialContainer.IsComImport) {
				if (!IsDefault ()) {
					Report.Error (669, Location, "`{0}': A class with the ComImport attribute cannot have a user-defined constructor",
						Parent.GetSignatureForError ());
				}
				ConstructorBuilder.SetImplementationFlags (MethodImplAttributes.InternalCall);
			}

			if ((ModFlags & Modifiers.DEBUGGER_HIDDEN) != 0)
				PredefinedAttributes.Get.DebuggerHidden.EmitAttribute (ConstructorBuilder);

			if (OptAttributes != null)
				OptAttributes.Emit ();

			base.Emit ();

			//
			// If we use a "this (...)" constructor initializer, then
			// do not emit field initializers, they are initialized in the other constructor
			//
			bool emit_field_initializers = ((ModFlags & Modifiers.STATIC) != 0) ||
				!(Initializer is ConstructorThisInitializer);

			BlockContext bc = new BlockContext (this, block, TypeManager.void_type);
			bc.Set (ResolveContext.Options.ConstructorScope);

			if (emit_field_initializers)
				Parent.PartialContainer.ResolveFieldInitializers (bc);

			if (block != null) {
				// If this is a non-static `struct' constructor and doesn't have any
				// initializer, it must initialize all of the struct's fields.
				if ((Parent.PartialContainer.Kind == MemberKind.Struct) &&
					((ModFlags & Modifiers.STATIC) == 0) && (Initializer == null))
					block.AddThisVariable (Parent, Location);

				if (block != null && (ModFlags & Modifiers.STATIC) == 0){
					if (Parent.PartialContainer.Kind == MemberKind.Class && Initializer == null)
						Initializer = new GeneratedBaseInitializer (Location);

					if (Initializer != null) {
						block.AddScopeStatement (new StatementExpression (Initializer));
					}
				}
			}

			parameters.ApplyAttributes (ConstructorBuilder);

			SourceMethod source = SourceMethod.Create (Parent, ConstructorBuilder, block);

			if (block != null) {
				if (block.Resolve (null, bc, parameters, this)) {
					EmitContext ec = new EmitContext (this, ConstructorBuilder.GetILGenerator (), bc.ReturnType);
					ec.With (EmitContext.Options.ConstructorScope, true);

					if (!ec.HasReturnLabel && bc.HasReturnLabel) {
						ec.ReturnLabel = bc.ReturnLabel;
						ec.HasReturnLabel = true;
					}

					block.Emit (ec);
				}
			}

			if (source != null)
				source.CloseMethod ();

			if (declarative_security != null) {
				foreach (var de in declarative_security) {
					ConstructorBuilder.AddDeclarativeSecurity (de.Key, de.Value);
				}
			}

			block = null;
		}

		protected override MemberSpec FindBaseMember (out MemberSpec bestCandidate)
		{
			// Is never override
			bestCandidate = null;
			return null;
		}

		public override string GetSignatureForError()
		{
			return base.GetSignatureForError () + parameters.GetSignatureForError ();
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance () || !IsExposedFromAssembly ()) {
				return false;
			}

			if (!parameters.IsEmpty && Parent.Definition.IsAttribute) {
				foreach (TypeSpec param in parameters.Types) {
					if (param.IsArray) {
						return true;
					}
				}
			}

			has_compliant_args = true;
			return true;
		}

		#region IMethodData Members

		public MemberName MethodName {
			get {
				return MemberName;
			}
		}

		public TypeSpec ReturnType {
			get {
				return MemberType;
			}
		}

		public EmitContext CreateEmitContext (ILGenerator ig)
		{
			throw new NotImplementedException ();
		}

		public bool IsExcluded()
		{
			return false;
		}

		GenericMethod IMethodData.GenericMethod {
			get {
				return null;
			}
		}

		void IMethodData.EmitExtraSymbolInfo (SourceMethod source)
		{ }

		#endregion
	}

	/// <summary>
	/// Interface for MethodData class. Holds links to parent members to avoid member duplication.
	/// </summary>
	public interface IMethodData
	{
		CallingConventions CallingConventions { get; }
		Location Location { get; }
		MemberName MethodName { get; }
		TypeSpec ReturnType { get; }
		GenericMethod GenericMethod { get; }
		ParametersCompiled ParameterInfo { get; }
		MethodSpec Spec { get; }

		Attributes OptAttributes { get; }
		ToplevelBlock Block { get; set; }

		EmitContext CreateEmitContext (ILGenerator ig);
		string GetSignatureForError ();
		void EmitExtraSymbolInfo (SourceMethod source);
	}

	//
	// Encapsulates most of the Method's state
	//
	public class MethodData {
		static FieldInfo methodbuilder_attrs_field;
		public readonly IMethodData method;

		public readonly GenericMethod GenericMethod;

		//
		// Are we implementing an interface ?
		//
		public MethodSpec implementing;

		//
		// Protected data.
		//
		protected InterfaceMemberBase member;
		protected Modifiers modifiers;
		protected MethodAttributes flags;
		protected TypeSpec declaring_type;
		protected MethodSpec parent_method;

		MethodBuilder builder;
		public MethodBuilder MethodBuilder {
			get {
				return builder;
			}
		}

		public TypeSpec DeclaringType {
			get {
				return declaring_type;
			}
		}

		public MethodData (InterfaceMemberBase member,
				   Modifiers modifiers, MethodAttributes flags, IMethodData method)
		{
			this.member = member;
			this.modifiers = modifiers;
			this.flags = flags;

			this.method = method;
		}

		public MethodData (InterfaceMemberBase member,
				   Modifiers modifiers, MethodAttributes flags, 
				   IMethodData method, MethodBuilder builder,
				   GenericMethod generic, MethodSpec parent_method)
			: this (member, modifiers, flags, method)
		{
			this.builder = builder;
			this.GenericMethod = generic;
			this.parent_method = parent_method;
		}

		public bool Define (DeclSpace parent, string method_full_name, Report Report)
		{
			TypeContainer container = parent.PartialContainer;

			PendingImplementation pending = container.PendingImplementations;
			if (pending != null){
				implementing = pending.IsInterfaceMethod (method.MethodName, member.InterfaceType, this);

				if (member.InterfaceType != null){
					if (implementing == null){
						if (member is PropertyBase) {
							Report.Error (550, method.Location, "`{0}' is an accessor not found in interface member `{1}{2}'",
								      method.GetSignatureForError (), TypeManager.CSharpName (member.InterfaceType),
								      member.GetSignatureForError ().Substring (member.GetSignatureForError ().LastIndexOf ('.')));

						} else {
							Report.Error (539, method.Location,
								      "`{0}.{1}' in explicit interface declaration is not a member of interface",
								      TypeManager.CSharpName (member.InterfaceType), member.ShortName);
						}
						return false;
					}
					if (implementing.IsAccessor && !(method is AbstractPropertyEventMethod)) {
						Report.SymbolRelatedToPreviousError (implementing);
						Report.Error (683, method.Location, "`{0}' explicit method implementation cannot implement `{1}' because it is an accessor",
							member.GetSignatureForError (), TypeManager.CSharpSignature (implementing));
						return false;
					}
				} else {
					if (implementing != null) {
						AbstractPropertyEventMethod prop_method = method as AbstractPropertyEventMethod;
						if (prop_method == null) {
							if (implementing.IsAccessor) {
								Report.SymbolRelatedToPreviousError (implementing);
								Report.Error (470, method.Location, "Method `{0}' cannot implement interface accessor `{1}'",
									method.GetSignatureForError (), TypeManager.CSharpSignature (implementing));
							}
						} else if (implementing.DeclaringType.IsInterface) {
							if (!implementing.IsAccessor) {
								Report.SymbolRelatedToPreviousError (implementing);
								Report.Error (686, method.Location, "Accessor `{0}' cannot implement interface member `{1}' for type `{2}'. Use an explicit interface implementation",
									method.GetSignatureForError (), TypeManager.CSharpSignature (implementing), container.GetSignatureForError ());
							} else {
								PropertyBase.PropertyMethod pm = prop_method as PropertyBase.PropertyMethod;
								if (pm != null && pm.HasCustomAccessModifier && (pm.ModFlags & Modifiers.PUBLIC) == 0) {
									Report.SymbolRelatedToPreviousError (implementing);
									Report.Error (277, method.Location, "Accessor `{0}' must be declared public to implement interface member `{1}'",
										method.GetSignatureForError (), implementing.GetSignatureForError ());
								}
							}
						}
					}
				}
			}

			//
			// For implicit implementations, make sure we are public, for
			// explicit implementations, make sure we are private.
			//
			if (implementing != null){
				//
				// Setting null inside this block will trigger a more
				// verbose error reporting for missing interface implementations
				//
				// The "candidate" function has been flagged already
				// but it wont get cleared
				//
				if (member.IsExplicitImpl){
					if (method.ParameterInfo.HasParams && !implementing.Parameters.HasParams) {
						Report.SymbolRelatedToPreviousError (implementing);
						Report.Error (466, method.Location, "`{0}': the explicit interface implementation cannot introduce the params modifier",
							method.GetSignatureForError ());
					}
				} else {
					if (implementing.DeclaringType.IsInterface) {
						//
						// If this is an interface method implementation,
						// check for public accessibility
						//
						if ((flags & MethodAttributes.MemberAccessMask) != MethodAttributes.Public)
						{
							implementing = null;
						}
					} else if ((flags & MethodAttributes.MemberAccessMask) == MethodAttributes.Private){
						// We may never be private.
						implementing = null;

					} else if ((modifiers & Modifiers.OVERRIDE) == 0){
						//
						// We may be protected if we're overriding something.
						//
						implementing = null;
					}
				}
					
				//
				// Static is not allowed
				//
				if ((modifiers & Modifiers.STATIC) != 0){
					implementing = null;
				}
			}
			
			//
			// If implementing is still valid, set flags
			//
			if (implementing != null){
				//
				// When implementing interface methods, set NewSlot
				// unless, we are overwriting a method.
				//
				if (implementing.DeclaringType.IsInterface){
					if ((modifiers & Modifiers.OVERRIDE) == 0)
						flags |= MethodAttributes.NewSlot;
				}

				flags |= MethodAttributes.Virtual | MethodAttributes.HideBySig;

				// Set Final unless we're virtual, abstract or already overriding a method.
				if ((modifiers & (Modifiers.VIRTUAL | Modifiers.ABSTRACT | Modifiers.OVERRIDE)) == 0)
					flags |= MethodAttributes.Final;

				//
				// clear the pending implementation flag (requires explicit methods to be defined first)
				//
				parent.PartialContainer.PendingImplementations.ImplementMethod (method.MethodName,
					member.InterfaceType, this, member.IsExplicitImpl);

				//
				// Update indexer accessor name to match implementing abstract accessor
				//
				if (!implementing.DeclaringType.IsInterface && !member.IsExplicitImpl && implementing.IsAccessor)
					method_full_name = implementing.MemberDefinition.Name;
			}

			DefineMethodBuilder (container, method_full_name, method.ParameterInfo);

			if (builder == null)
				return false;

//			if (container.CurrentType != null)
//				declaring_type = container.CurrentType;
//			else
				declaring_type = container.Definition;

			if (implementing != null && member.IsExplicitImpl) {
				container.TypeBuilder.DefineMethodOverride (builder, (MethodInfo) implementing.GetMetaInfo ());
			}

			return true;
		}


		/// <summary>
		/// Create the MethodBuilder for the method 
		/// </summary>
		void DefineMethodBuilder (TypeContainer container, string method_name, ParametersCompiled param)
		{
			var return_type = method.ReturnType.GetMetaInfo ();
			var p_types = param.GetMetaInfo ();

			if (builder == null) {
				builder = container.TypeBuilder.DefineMethod (
					method_name, flags, method.CallingConventions,
					return_type, p_types);
				return;
			}

			//
			// Generic method has been already defined to resolve method parameters
			// correctly when they use type parameters
			//
			builder.SetParameters (p_types);
			builder.SetReturnType (return_type);
			if (builder.Attributes != flags) {
				try {
					if (methodbuilder_attrs_field == null)
						methodbuilder_attrs_field = typeof (MethodBuilder).GetField ("attrs", BindingFlags.NonPublic | BindingFlags.Instance);
					methodbuilder_attrs_field.SetValue (builder, flags);
				} catch {
					container.Compiler.Report.RuntimeMissingSupport (method.Location, "Generic method MethodAttributes");
				}
			}
		}

		//
		// Emits the code
		// 
		public void Emit (DeclSpace parent)
		{
			if (GenericMethod != null)
				GenericMethod.EmitAttributes ();

			method.ParameterInfo.ApplyAttributes (MethodBuilder);

			SourceMethod source = SourceMethod.Create (parent, MethodBuilder, method.Block);

			ToplevelBlock block = method.Block;
			if (block != null) {
				BlockContext bc = new BlockContext ((IMemberContext) method, block, method.ReturnType);
				if (block.Resolve (null, bc, method.ParameterInfo, method)) {
					EmitContext ec = method.CreateEmitContext (MethodBuilder.GetILGenerator ());
					if (!ec.HasReturnLabel && bc.HasReturnLabel) {
						ec.ReturnLabel = bc.ReturnLabel;
						ec.HasReturnLabel = true;
					}

					block.Emit (ec);
				}
			}

			if (source != null) {
				method.EmitExtraSymbolInfo (source);
				source.CloseMethod ();
			}
		}
	}

	public class Destructor : MethodOrOperator
	{
		const Modifiers AllowedModifiers =
			Modifiers.UNSAFE |
			Modifiers.EXTERN;

		static readonly string[] attribute_targets = new string [] { "method" };

		public static readonly string MetadataName = "Finalize";

		public Destructor (DeclSpace parent, Modifiers mod, ParametersCompiled parameters, Attributes attrs, Location l)
			: base (parent, null, null, mod, AllowedModifiers,
				new MemberName (MetadataName, l), attrs, parameters)
		{
			ModFlags &= ~Modifiers.PRIVATE;
			ModFlags |= Modifiers.PROTECTED | Modifiers.OVERRIDE;
		}

		public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Type == pa.Conditional) {
				Error_ConditionalAttributeIsNotValid ();
				return;
			}

			base.ApplyAttributeBuilder (a, ctor, cdata, pa);
		}

		protected override bool CheckBase ()
		{
			// Don't check base, destructors have special syntax
			return true;
		}

		public override void Emit()
		{
			var base_type = Parent.PartialContainer.BaseType;
			if (base_type != null && Block != null) {
				var base_dtor = MemberCache.FindMember (base_type,
					new MemberFilter (MetadataName, 0, MemberKind.Destructor, null, null), BindingRestriction.InstanceOnly) as MethodSpec;

				if (base_dtor == null)
					throw new NotImplementedException ();

				MethodGroupExpr method_expr = MethodGroupExpr.CreatePredefined (base_dtor, base_type, Location);
				method_expr.InstanceExpression = new BaseThis (base_type, Location);

				ToplevelBlock new_block = new ToplevelBlock (Compiler, Block.StartLocation);
				new_block.EndLocation = Block.EndLocation;

				Block finaly_block = new ExplicitBlock (new_block, Location, Location);
				Block try_block = new Block (new_block, block);

				//
				// 0-size arguments to avoid CS0250 error
				// TODO: Should use AddScopeStatement or something else which emits correct
				// debugger scope
				//
				finaly_block.AddStatement (new StatementExpression (new Invocation (method_expr, new Arguments (0))));
				new_block.AddStatement (new TryFinally (try_block, finaly_block, Location));

				block = new_block;
			}

			base.Emit ();
		}

		public override string GetSignatureForError ()
		{
			return Parent.GetSignatureForError () + ".~" + Parent.MemberName.Name + "()";
		}

		protected override bool ResolveMemberType ()
		{
			member_type = TypeManager.void_type;
			return true;
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}
	}

	// Ooouh Martin, templates are missing here.
	// When it will be possible move here a lot of child code and template method type.
	public abstract class AbstractPropertyEventMethod : MemberCore, IMethodData {
		protected MethodData method_data;
		protected ToplevelBlock block;
		protected Dictionary<SecurityAction, PermissionSet> declarative_security;

		protected readonly string prefix;

		ReturnParameter return_attributes;

		public AbstractPropertyEventMethod (InterfaceMemberBase member, string prefix, Attributes attrs, Location loc)
			: base (member.Parent, SetupName (prefix, member, loc), attrs)
		{
			this.prefix = prefix;
		}

		static MemberName SetupName (string prefix, InterfaceMemberBase member, Location loc)
		{
			return new MemberName (member.MemberName.Left, prefix + member.ShortName, loc);
		}

		public void UpdateName (InterfaceMemberBase member)
		{
			SetMemberName (SetupName (prefix, member, Location));
		}

		#region IMethodData Members

		public ToplevelBlock Block {
			get {
				return block;
			}

			set {
				block = value;
			}
		}

		public CallingConventions CallingConventions {
			get {
				return CallingConventions.Standard;
			}
		}

		public EmitContext CreateEmitContext (ILGenerator ig)
		{
			return new EmitContext (this, ig, ReturnType);
		}

		public bool IsExcluded ()
		{
			return false;
		}

		GenericMethod IMethodData.GenericMethod {
			get {
				return null;
			}
		}

		public MemberName MethodName {
			get {
				return MemberName;
			}
		}

		public TypeSpec[] ParameterTypes { 
			get {
				return ParameterInfo.Types;
			}
		}

		public abstract ParametersCompiled ParameterInfo { get ; }
		public abstract TypeSpec ReturnType { get; }

		#endregion

		public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Type == pa.CLSCompliant || a.Type == pa.Obsolete || a.Type == pa.Conditional) {
				Report.Error (1667, a.Location,
					"Attribute `{0}' is not valid on property or event accessors. It is valid on `{1}' declarations only",
					TypeManager.CSharpName (a.Type), a.GetValidTargets ());
				return;
			}

			if (a.IsValidSecurityAttribute ()) {
				if (declarative_security == null)
					declarative_security = new Dictionary<SecurityAction, PermissionSet> ();
				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			if (a.Target == AttributeTargets.Method) {
				method_data.MethodBuilder.SetCustomAttribute ((ConstructorInfo) ctor.GetMetaInfo (), cdata);
				return;
			}

			if (a.Target == AttributeTargets.ReturnValue) {
				if (return_attributes == null)
					return_attributes = new ReturnParameter (this, method_data.MethodBuilder, Location);

				return_attributes.ApplyAttributeBuilder (a, ctor, cdata, pa);
				return;
			}

			ApplyToExtraTarget (a, ctor, cdata, pa);
		}

		protected virtual void ApplyToExtraTarget (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			throw new NotSupportedException ("You forgot to define special attribute target handling");
		}

		// It is not supported for the accessors
		public sealed override bool Define()
		{
			throw new NotSupportedException ();
		}

		public virtual void Emit (DeclSpace parent)
		{
			method_data.Emit (parent);

			if ((ModFlags & Modifiers.COMPILER_GENERATED) != 0 && !Parent.IsCompilerGenerated)
				PredefinedAttributes.Get.CompilerGenerated.EmitAttribute (method_data.MethodBuilder);
			if (((ModFlags & Modifiers.DEBUGGER_HIDDEN) != 0))
				PredefinedAttributes.Get.DebuggerHidden.EmitAttribute (method_data.MethodBuilder);

			if (ReturnType == InternalType.Dynamic) {
				return_attributes = new ReturnParameter (this, method_data.MethodBuilder, Location);
				PredefinedAttributes.Get.Dynamic.EmitAttribute (return_attributes.Builder);
			} else {
				var trans_flags = TypeManager.HasDynamicTypeUsed (ReturnType);
				if (trans_flags != null) {
					var pa = PredefinedAttributes.Get.DynamicTransform;
					if (pa.Constructor != null || pa.ResolveConstructor (Location, ArrayContainer.MakeType (TypeManager.bool_type))) {
						return_attributes = new ReturnParameter (this, method_data.MethodBuilder, Location);
						return_attributes.Builder.SetCustomAttribute (
							new CustomAttributeBuilder (pa.Constructor, new object [] { trans_flags }));
					}
				}
			}

			if (OptAttributes != null)
				OptAttributes.Emit ();

			if (declarative_security != null) {
				foreach (var de in declarative_security) {
					method_data.MethodBuilder.AddDeclarativeSecurity (de.Key, de.Value);
				}
			}

			block = null;
		}

		public override bool EnableOverloadChecks (MemberCore overload)
		{
			if (overload is MethodCore) {
				caching_flags |= Flags.MethodOverloadsExist;
				return true;
			}

			// This can only happen with indexers and it will
			// be catched as indexer difference
			if (overload is AbstractPropertyEventMethod)
				return true;

			return false;
		}

		public override bool IsClsComplianceRequired()
		{
			return false;
		}

		public MethodSpec Spec { get; protected set; }

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { throw new InvalidOperationException ("Unexpected attempt to get doc comment from " + this.GetType () + "."); }
		}

		void IMethodData.EmitExtraSymbolInfo (SourceMethod source)
		{ }
	}

	public class Operator : MethodOrOperator {

		const Modifiers AllowedModifiers =
			Modifiers.PUBLIC |
			Modifiers.UNSAFE |
			Modifiers.EXTERN |
			Modifiers.STATIC;

		public enum OpType : byte {

			// Unary operators
			LogicalNot,
			OnesComplement,
			Increment,
			Decrement,
			True,
			False,

			// Unary and Binary operators
			Addition,
			Subtraction,

			UnaryPlus,
			UnaryNegation,
			
			// Binary operators
			Multiply,
			Division,
			Modulus,
			BitwiseAnd,
			BitwiseOr,
			ExclusiveOr,
			LeftShift,
			RightShift,
			Equality,
			Inequality,
			GreaterThan,
			LessThan,
			GreaterThanOrEqual,
			LessThanOrEqual,

			// Implicit and Explicit
			Implicit,
			Explicit,

			// Just because of enum
			TOP
		};

		public readonly OpType OperatorType;

		static readonly string [] [] names;

		static Operator ()
		{
			names = new string[(int)OpType.TOP][];
			names [(int) OpType.LogicalNot] = new string [] { "!", "op_LogicalNot" };
			names [(int) OpType.OnesComplement] = new string [] { "~", "op_OnesComplement" };
			names [(int) OpType.Increment] = new string [] { "++", "op_Increment" };
			names [(int) OpType.Decrement] = new string [] { "--", "op_Decrement" };
			names [(int) OpType.True] = new string [] { "true", "op_True" };
			names [(int) OpType.False] = new string [] { "false", "op_False" };
			names [(int) OpType.Addition] = new string [] { "+", "op_Addition" };
			names [(int) OpType.Subtraction] = new string [] { "-", "op_Subtraction" };
			names [(int) OpType.UnaryPlus] = new string [] { "+", "op_UnaryPlus" };
			names [(int) OpType.UnaryNegation] = new string [] { "-", "op_UnaryNegation" };
			names [(int) OpType.Multiply] = new string [] { "*", "op_Multiply" };
			names [(int) OpType.Division] = new string [] { "/", "op_Division" };
			names [(int) OpType.Modulus] = new string [] { "%", "op_Modulus" };
			names [(int) OpType.BitwiseAnd] = new string [] { "&", "op_BitwiseAnd" };
			names [(int) OpType.BitwiseOr] = new string [] { "|", "op_BitwiseOr" };
			names [(int) OpType.ExclusiveOr] = new string [] { "^", "op_ExclusiveOr" };
			names [(int) OpType.LeftShift] = new string [] { "<<", "op_LeftShift" };
			names [(int) OpType.RightShift] = new string [] { ">>", "op_RightShift" };
			names [(int) OpType.Equality] = new string [] { "==", "op_Equality" };
			names [(int) OpType.Inequality] = new string [] { "!=", "op_Inequality" };
			names [(int) OpType.GreaterThan] = new string [] { ">", "op_GreaterThan" };
			names [(int) OpType.LessThan] = new string [] { "<", "op_LessThan" };
			names [(int) OpType.GreaterThanOrEqual] = new string [] { ">=", "op_GreaterThanOrEqual" };
			names [(int) OpType.LessThanOrEqual] = new string [] { "<=", "op_LessThanOrEqual" };
			names [(int) OpType.Implicit] = new string [] { "implicit", "op_Implicit" };
			names [(int) OpType.Explicit] = new string [] { "explicit", "op_Explicit" };
		}
		
		public Operator (DeclSpace parent, OpType type, FullNamedExpression ret_type,
				 Modifiers mod_flags, ParametersCompiled parameters,
				 ToplevelBlock block, Attributes attrs, Location loc)
			: base (parent, null, ret_type, mod_flags, AllowedModifiers,
				new MemberName (GetMetadataName (type), loc), attrs, parameters)
		{
			OperatorType = type;
			Block = block;
		}

		public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Type == pa.Conditional) {
				Error_ConditionalAttributeIsNotValid ();
				return;
			}

			base.ApplyAttributeBuilder (a, ctor, cdata, pa);
		}
		
		public override bool Define ()
		{
			const Modifiers RequiredModifiers = Modifiers.PUBLIC | Modifiers.STATIC;
			if ((ModFlags & RequiredModifiers) != RequiredModifiers){
				Report.Error (558, Location, "User-defined operator `{0}' must be declared static and public", GetSignatureForError ());
			}

			if (!base.Define ())
				return false;

			if (block != null && block.IsIterator && !(Parent is IteratorStorey)) {
				//
				// Current method is turned into automatically generated
				// wrapper which creates an instance of iterator
				//
				Iterator.CreateIterator (this, Parent.PartialContainer, ModFlags, Compiler);
				ModFlags |= Modifiers.DEBUGGER_HIDDEN;
			}

			// imlicit and explicit operator of same types are not allowed
			if (OperatorType == OpType.Explicit)
				Parent.MemberCache.CheckExistingMembersOverloads (this, GetMetadataName (OpType.Implicit), parameters);
			else if (OperatorType == OpType.Implicit)
				Parent.MemberCache.CheckExistingMembersOverloads (this, GetMetadataName (OpType.Explicit), parameters);

			TypeSpec declaring_type = Parent.CurrentType;
			TypeSpec return_type = MemberType;
			TypeSpec first_arg_type = ParameterTypes [0];
			
			TypeSpec first_arg_type_unwrap = first_arg_type;
			if (TypeManager.IsNullableType (first_arg_type))
				first_arg_type_unwrap = TypeManager.GetTypeArguments (first_arg_type) [0];
			
			TypeSpec return_type_unwrap = return_type;
			if (TypeManager.IsNullableType (return_type))
				return_type_unwrap = TypeManager.GetTypeArguments (return_type) [0];

			//
			// Rules for conversion operators
			//
			if (OperatorType == OpType.Implicit || OperatorType == OpType.Explicit) {
				if (first_arg_type_unwrap == return_type_unwrap && first_arg_type_unwrap == declaring_type) {
					Report.Error (555, Location,
						"User-defined operator cannot take an object of the enclosing type and convert to an object of the enclosing type");
					return false;
				}

				TypeSpec conv_type;
				if (declaring_type == return_type || declaring_type == return_type_unwrap) {
					conv_type = first_arg_type;
				} else if (declaring_type == first_arg_type || declaring_type == first_arg_type_unwrap) {
					conv_type = return_type;
				} else {
					Report.Error (556, Location,
						"User-defined conversion must convert to or from the enclosing type");
					return false;
				}

				if (conv_type == InternalType.Dynamic) {
					Report.Error (1964, Location,
						"User-defined conversion `{0}' cannot convert to or from the dynamic type",
						GetSignatureForError ());

					return false;
				}

				if (conv_type.IsInterface) {
					Report.Error (552, Location, "User-defined conversion `{0}' cannot convert to or from an interface type",
						GetSignatureForError ());
					return false;
				}

				if (conv_type.IsClass) {
					if (TypeSpec.IsBaseClass (declaring_type, conv_type, true)) {
						Report.Error (553, Location, "User-defined conversion `{0}' cannot convert to or from a base class",
							GetSignatureForError ());
						return false;
					}

					if (TypeSpec.IsBaseClass (conv_type, declaring_type, false)) {
						Report.Error (554, Location, "User-defined conversion `{0}' cannot convert to or from a derived class",
							GetSignatureForError ());
						return false;
					}
				}
			} else if (OperatorType == OpType.LeftShift || OperatorType == OpType.RightShift) {
				if (first_arg_type != declaring_type || parameters.Types[1] != TypeManager.int32_type) {
					Report.Error (564, Location, "Overloaded shift operator must have the type of the first operand be the containing type, and the type of the second operand must be int");
					return false;
				}
			} else if (parameters.Count == 1) {
				// Checks for Unary operators

				if (OperatorType == OpType.Increment || OperatorType == OpType.Decrement) {
					if (return_type != declaring_type && !TypeSpec.IsBaseClass (return_type, declaring_type, false)) {
						Report.Error (448, Location,
							"The return type for ++ or -- operator must be the containing type or derived from the containing type");
						return false;
					}
					if (first_arg_type != declaring_type) {
						Report.Error (
							559, Location, "The parameter type for ++ or -- operator must be the containing type");
						return false;
					}
				}

				if (first_arg_type_unwrap != declaring_type) {
					Report.Error (562, Location,
						"The parameter type of a unary operator must be the containing type");
					return false;
				}

				if (OperatorType == OpType.True || OperatorType == OpType.False) {
					if (return_type != TypeManager.bool_type) {
						Report.Error (
							215, Location,
							"The return type of operator True or False " +
							"must be bool");
						return false;
					}
				}

			} else if (first_arg_type_unwrap != declaring_type) {
				// Checks for Binary operators

				var second_arg_type = ParameterTypes[1];
				if (TypeManager.IsNullableType (second_arg_type))
					second_arg_type = TypeManager.GetTypeArguments (second_arg_type)[0];

				if (second_arg_type != declaring_type) {
					Report.Error (563, Location,
						"One of the parameters of a binary operator must be the containing type");
					return false;
				}
			}

			return true;
		}

		protected override bool ResolveMemberType ()
		{
			if (!base.ResolveMemberType ())
				return false;

			flags |= MethodAttributes.SpecialName | MethodAttributes.HideBySig;
			return true;
		}

		protected override MemberSpec FindBaseMember (out MemberSpec bestCandidate)
		{
			// Operator cannot be override
			bestCandidate = null;
			return null;
		}

		public static string GetName (OpType ot)
		{
			return names [(int) ot] [0];
		}

		public static string GetName (string metadata_name)
		{
			for (int i = 0; i < names.Length; ++i) {
				if (names [i] [1] == metadata_name)
					return names [i] [0];
			}
			return null;
		}

		public static string GetMetadataName (OpType ot)
		{
			return names [(int) ot] [1];
		}

		public static string GetMetadataName (string name)
		{
			for (int i = 0; i < names.Length; ++i) {
				if (names [i] [0] == name)
					return names [i] [1];
			}
			return null;
		}

		public static OpType? GetType (string metadata_name)
		{
			for (int i = 0; i < names.Length; ++i) {
				if (names[i][1] == metadata_name)
					return (OpType) i;
			}

			return null;
		}

		public OpType GetMatchingOperator ()
		{
			switch (OperatorType) {
			case OpType.Equality:
				return OpType.Inequality;
			case OpType.Inequality:
				return OpType.Equality;
			case OpType.True:
				return OpType.False;
			case OpType.False:
				return OpType.True;
			case OpType.GreaterThan:
				return OpType.LessThan;
			case OpType.LessThan:
				return OpType.GreaterThan;
			case OpType.GreaterThanOrEqual:
				return OpType.LessThanOrEqual;
			case OpType.LessThanOrEqual:
				return OpType.GreaterThanOrEqual;
			default:
				return OpType.TOP;
			}
		}

		public override string GetSignatureForError ()
		{
			StringBuilder sb = new StringBuilder ();
			if (OperatorType == OpType.Implicit || OperatorType == OpType.Explicit) {
				sb.AppendFormat ("{0}.{1} operator {2}",
					Parent.GetSignatureForError (), GetName (OperatorType), type_expr.GetSignatureForError ());
			}
			else {
				sb.AppendFormat ("{0}.operator {1}", Parent.GetSignatureForError (), GetName (OperatorType));
			}

			sb.Append (parameters.GetSignatureForError ());
			return sb.ToString ();
		}
	}
}

