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

#if NET_2_1
using XmlElement = System.Object;
#else
using System.Xml;
#endif

using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp {

	public abstract class MethodCore : InterfaceMemberBase
	{
		public readonly ParametersCompiled Parameters;
		protected ToplevelBlock block;
		protected MethodSpec spec;

		public MethodCore (DeclSpace parent, GenericMethod generic,
			FullNamedExpression type, Modifiers mod, Modifiers allowed_mod,
			MemberName name, Attributes attrs, ParametersCompiled parameters)
			: base (parent, generic, type, mod, allowed_mod, name, attrs)
		{
			Parameters = parameters;
		}

		//
		//  Returns the System.Type array for the parameters of this method
		//
		public Type [] ParameterTypes {
			get {
				return Parameters.Types;
			}
		}

		public ParametersCompiled ParameterInfo {
			get {
				return Parameters;
			}
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
				CallingConventions cc = Parameters.CallingConvention;
				if (!IsInterface)
					if ((ModFlags & Modifiers.STATIC) == 0)
						cc |= CallingConventions.HasThis;

				// FIXME: How is `ExplicitThis' used in C#?
			
				return cc;
			}
		}

		protected override bool CheckBase ()
		{
			// Check whether arguments were correct.
			if (!DefineParameters (Parameters))
				return false;

			return base.CheckBase ();
		}

		//
		// Returns a string that represents the signature for this 
		// member which should be used in XML documentation.
		//
		public override string GetDocCommentName (DeclSpace ds)
		{
			return DocUtil.GetMethodDocCommentName (this, Parameters, ds);
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
			if (overload is MethodCore || overload is AbstractPropertyEventMethod) {
				caching_flags |= Flags.MethodOverloadsExist;
				return true;
			}

			return base.EnableOverloadChecks (overload);
		}

		public MethodSpec Spec {
			get { return spec; }
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			if (Parameters.HasArglist) {
				Report.Warning (3000, 1, Location, "Methods with variable arguments are not CLS-compliant");
			}

			if (!AttributeTester.IsClsCompliant (MemberType)) {
				Report.Warning (3002, 1, Location, "Return type of `{0}' is not CLS-compliant",
					GetSignatureForError ());
			}

			Parameters.VerifyClsCompliance (this);
			return true;
		}
	}

	interface IGenericMethodDefinition : IMemberDefinition
	{
		MethodInfo MakeGenericMethod (Type[] targs);
	}

	public class MethodSpec : MemberSpec
	{
		MethodBase metaInfo;
		readonly AParametersCollection parameters;

		public MethodSpec (IMemberDefinition details, MethodBase info, AParametersCollection parameters, Modifiers modifiers)
			: base (details, info.Name, modifiers)
		{
			this.MetaInfo = info;
			this.parameters = parameters;
		}

		public override Type DeclaringType {
			get {
				return MetaInfo.DeclaringType;
			}
		}

		public Type[] GetGenericArguments ()
		{
			return MetaInfo.GetGenericArguments ();
		}

		public MethodSpec Inflate (Type[] targs)
		{
			// TODO: Only create MethodSpec and inflate parameters, defer the call for later
			var mb = ((IGenericMethodDefinition) definition).MakeGenericMethod (targs);

			// TODO: Does not work on .NET
			var par = TypeManager.GetParameterData (mb);

			return new MethodSpec (definition, mb, par, modifiers);
		}

		public bool IsAbstract {
			get {
				return (modifiers & Modifiers.ABSTRACT) != 0;
			}
		}

		public bool IsConstructor {
			get {
				return MetaInfo.IsConstructor;
			}
		}

		public bool IsGenericMethod {
			get {
				return MetaInfo.IsGenericMethod;
			}
		}

		// When is virtual or abstract
		public bool IsVirtual {
			get {
				return (modifiers & (Modifiers.VIRTUAL | Modifiers.ABSTRACT | Modifiers.OVERRIDE)) != 0;
			}
		}

		public MethodBase MetaInfo {
			get {
				return metaInfo;
			}
			set {
				metaInfo = value;
			}
		}

		public AParametersCollection Parameters {
			get { return parameters; }
		}

		public Type ReturnType {
			get {
				return IsConstructor ?
					TypeManager.void_type : ((MethodInfo) MetaInfo).ReturnType;
			}
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

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.Target == AttributeTargets.ReturnValue) {
				if (return_attributes == null)
					return_attributes = new ReturnParameter (this, MethodBuilder, Location);

				return_attributes.ApplyAttributeBuilder (a, cb, pa);
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
				MethodBuilder.SetCustomAttribute (cb);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Method; 
			}
		}

		protected override bool CheckForDuplications ()
		{
			string name = GetFullName (MemberName);
			if (MemberName.IsGeneric)
				name = MemberName.MakeName (name, MemberName.TypeArguments);

			return Parent.MemberCache.CheckExistingMembersOverloads (this, name, Parameters, Report);
		}

		public virtual EmitContext CreateEmitContext (ILGenerator ig)
		{
			return new EmitContext (
				this, ig, MemberType);
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

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			if (!CheckBase ())
				return false;

			if (block != null && block.IsIterator && !(Parent is IteratorStorey)) {
				//
				// Current method is turned into automatically generated
				// wrapper which creates an instance of iterator
				//
				Iterator.CreateIterator (this, Parent.PartialContainer, ModFlags, Compiler);
				ModFlags |= Modifiers.DEBUGGER_HIDDEN;
			}

			if (IsPartialDefinition) {
				caching_flags &= ~Flags.Excluded_Undetected;
				caching_flags |= Flags.Excluded;

				// Add to member cache only when a partial method implementation has not been found yet
				if ((caching_flags & Flags.PartialDefinitionExists) == 0) {
					MethodBase mb = new PartialMethodDefinitionInfo (this);
					Parent.MemberCache.AddMember (mb, this);
					TypeManager.AddMethod (mb, this);

					spec = new MethodSpec (this, mb, Parameters, ModFlags);
				}

				return true;
			}

			MethodData = new MethodData (
				this, ModFlags, flags, this, MethodBuilder, GenericMethod, base_method);

			if (!MethodData.Define (Parent.PartialContainer, GetFullName (MemberName), Report))
				return false;
					
			MethodBuilder = MethodData.MethodBuilder;

			spec = new MethodSpec (this, MethodBuilder, Parameters, ModFlags);

			if (TypeManager.IsGenericMethod (MethodBuilder))
				Parent.MemberCache.AddGenericMember (MethodBuilder, this);
			
			Parent.MemberCache.AddMember (MethodBuilder, this);

			return true;
		}

		protected override void DoMemberTypeIndependentChecks ()
		{
			base.DoMemberTypeIndependentChecks ();

			CheckAbstractAndExtern (block != null);

			if ((ModFlags & Modifiers.PARTIAL) != 0) {
				for (int i = 0; i < Parameters.Count; ++i) {
					IParameterData p = Parameters.FixedParameters [i];
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

			if (!TypeManager.IsGenericParameter (MemberType)) {
				if (MemberType.IsAbstract && MemberType.IsSealed) {
					Report.Error (722, Location, Error722, TypeManager.CSharpName (MemberType));
				}
			}
		}

		public override void Emit ()
		{
			if ((ModFlags & Modifiers.COMPILER_GENERATED) != 0 && !Parent.IsCompilerGenerated)
				PredefinedAttributes.Get.CompilerGenerated.EmitAttribute (MethodBuilder);
			if ((ModFlags & Modifiers.DEBUGGER_HIDDEN) != 0)
				PredefinedAttributes.Get.DebuggerHidden.EmitAttribute (MethodBuilder);

			if (TypeManager.IsDynamicType (ReturnType)) {
				return_attributes = new ReturnParameter (this, MethodBuilder, Location);
				PredefinedAttributes.Get.Dynamic.EmitAttribute (return_attributes.Builder);
			} else {
				var trans_flags = TypeManager.HasDynamicTypeUsed (ReturnType);
				if (trans_flags != null) {
					var pa = PredefinedAttributes.Get.DynamicTransform;
					if (pa.Constructor != null || pa.ResolveConstructor (Location, TypeManager.bool_type.MakeArrayType ())) {
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

		public Type ReturnType {
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
		public bool IsExcluded () {
			if ((caching_flags & Flags.Excluded_Undetected) == 0)
				return (caching_flags & Flags.Excluded) != 0;

			caching_flags &= ~Flags.Excluded_Undetected;

			if (base_method == null) {
				if (OptAttributes == null)
					return false;

				Attribute[] attrs = OptAttributes.SearchMulti (PredefinedAttributes.Get.Conditional);

				if (attrs == null)
					return false;

				foreach (Attribute a in attrs) {
					string condition = a.GetConditionalAttributeValue ();
					if (condition == null)
						return false;

					if (Location.CompilationUnit.IsConditionalDefined (condition))
						return false;
				}

				caching_flags |= Flags.Excluded;
				return true;
			}

			IMethodData md = TypeManager.GetMethod (TypeManager.DropGenericMethodArguments (base_method));
			if (md == null) {
				if (AttributeTester.IsConditionalMethodExcluded (base_method, Location)) {
					caching_flags |= Flags.Excluded;
					return true;
				}
				return false;
			}

			if (md.IsExcluded ()) {
				caching_flags |= Flags.Excluded;
				return true;
			}
			return false;
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
		/// <summary>
		///   Modifiers allowed in a class declaration
		/// </summary>
		const Modifiers AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC |
			Modifiers.VIRTUAL |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.ABSTRACT |
			Modifiers.UNSAFE |
			Modifiers.EXTERN;

		const Modifiers AllowedInterfaceModifiers = 
			Modifiers.NEW | Modifiers.UNSAFE;

		Method partialMethodImplementation;

		public Method (DeclSpace parent, GenericMethod generic,
			       FullNamedExpression return_type, Modifiers mod,
			       MemberName name, ParametersCompiled parameters, Attributes attrs)
			: base (parent, generic, return_type, mod,
				parent.PartialContainer.Kind == Kind.Interface ? AllowedInterfaceModifiers : AllowedModifiers,
				name, attrs, parameters)
		{
		}

		protected Method (DeclSpace parent, FullNamedExpression return_type, Modifiers mod, Modifiers amod,
					MemberName name, ParametersCompiled parameters, Attributes attrs)
			: base (parent, null, return_type, mod, amod, name, attrs, parameters)
		{
		}
		
		public override string GetSignatureForError()
		{
			return base.GetSignatureForError () + Parameters.GetSignatureForError ();
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

			if (Parameters.Count == 0)
				return true;

			if (Parameters.Count > 1)
				return false;

			Type t = Parameters.Types [0];
			return t.IsArray && t.GetArrayRank () == 1 &&
					TypeManager.GetElementType (t) == TypeManager.string_type &&
					(Parameters[0].ModFlags & ~Parameter.Modifier.PARAMS) == Parameter.Modifier.NONE;
		}

		public override FullNamedExpression LookupNamespaceOrType (string name, Location loc, bool ignore_cs0104)
		{
			TypeParameter[] tp = CurrentTypeParameters;
			if (tp != null) {
				TypeParameter t = TypeParameter.FindTypeParameter (tp, name);
				if (t != null)
					return new TypeParameterExpr (t, loc);
			}

			return base.LookupNamespaceOrType (name, loc, ignore_cs0104);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.Type == pa.Conditional) {
				if (IsExplicitImpl) {
					Error_ConditionalAttributeIsNotValid ();
					return;
				}

				if (ReturnType != TypeManager.void_type) {
					Report.Error (578, Location, "Conditional not valid on `{0}' because its return type is not void", GetSignatureForError ());
					return;
				}

				if ((ModFlags & Modifiers.OVERRIDE) != 0) {
					Report.Error (243, Location, "Conditional not valid on `{0}' because it is an override method", GetSignatureForError ());
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

				for (int i = 0; i < Parameters.Count; ++i) {
					if (Parameters.FixedParameters [i].ModFlags == Parameter.Modifier.OUT) {
						Report.Error (685, Location, "Conditional method `{0}' cannot have an out parameter", GetSignatureForError ());
						return;
					}
				}
			}

			if (a.Type == pa.Extension) {
				a.Error_MisusedExtensionAttribute ();
				return;
			}

			base.ApplyAttributeBuilder (a, cb, pa);
		}

  		protected override bool CheckForDuplications ()
   		{
			if (!base.CheckForDuplications ())
				return false;

			var ar = Parent.PartialContainer.Properties;
			if (ar != null) {
				for (int i = 0; i < ar.Count; ++i) {
					PropertyBase pb = (PropertyBase) ar [i];
					if (pb.AreAccessorsDuplicateImplementation (this))
						return false;
				}
			}

			ar = Parent.PartialContainer.Indexers;
			if (ar != null) {
				for (int i = 0; i < ar.Count; ++i) {
					PropertyBase pb = (PropertyBase) ar [i];
					if (pb.AreAccessorsDuplicateImplementation (this))
						return false;
				}
			}

			return true;
		}

		protected override bool CheckBase ()
		{
			if (!base.CheckBase ())
				return false;

			if (base_method != null && (ModFlags & Modifiers.OVERRIDE) != 0 && Name == Destructor.MetadataName) {
				Report.Error (249, Location, "Do not override `{0}'. Use destructor syntax instead",
					TypeManager.CSharpSignature (base_method));
			}

			return true;
		}

		public override TypeParameter[] CurrentTypeParameters {
			get {
				if (GenericMethod != null)
					return GenericMethod.CurrentTypeParameters;

				return null;
			}
		}

		//
		// Creates the type
		//
		public override bool Define ()
		{
			if (type_name == TypeManager.system_void_expr && Parameters.IsEmpty && Name == Destructor.MetadataName) {
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

			if (base_method != null && (ModFlags & Modifiers.NEW) == 0) {
				if (Parameters.Count == 1 && ParameterTypes [0] == TypeManager.object_type && Name == "Equals")
					Parent.PartialContainer.Mark_HasEquals ();
				else if (Parameters.IsEmpty && Name == "GetHashCode")
					Parent.PartialContainer.Mark_HasGetHashCode ();
			}

			if ((ModFlags & Modifiers.STATIC) == 0)
				return true;

			if (Parameters.HasExtensionMethodType) {
				if (Parent.PartialContainer.IsStaticClass && !Parent.IsGeneric) {
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

		public static void Error1599 (Location loc, Type t, Report Report)
		{
			Report.Error (1599, loc, "Method or delegate cannot return type `{0}'", TypeManager.CSharpName (t));
		}

		protected override MethodInfo FindOutBaseMethod (ref Type base_ret_type)
		{
			MethodInfo mi = (MethodInfo) Parent.PartialContainer.BaseCache.FindMemberToOverride (
				Parent.TypeBuilder, Name, Parameters, GenericMethod, false);

			if (mi == null)
				return null;

			if (mi.IsSpecialName)
				return null;

			base_ret_type = TypeManager.TypeToCoreType (mi.ReturnType);
			return mi;
		}

		public MethodInfo MakeGenericMethod (Type[] targs)
		{
			return MethodBuilder.MakeGenericMethod (targs);
		}

		public void SetPartialDefinition (Method methodDefinition)
		{
			caching_flags |= Flags.PartialDefinitionExists;
			methodDefinition.partialMethodImplementation = this;

			// Ensure we are always using method declaration parameters
			for (int i = 0; i < methodDefinition.Parameters.Count; ++i ) {
				Parameters [i].Name = methodDefinition.Parameters [i].Name;
				Parameters [i].DefaultValue = methodDefinition.Parameters [i].DefaultValue;
			}

			if (methodDefinition.attributes == null)
				return;

			if (attributes == null) {
				attributes = methodDefinition.attributes;
			} else {
				attributes.Attrs.AddRange (methodDefinition.attributes.Attrs);
			}
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			if (!Parameters.IsEmpty) {
				var al = Parent.PartialContainer.MemberCache.Members [Name];
				if (al.Count > 1)
					MemberCache.VerifyClsParameterConflict (al, this, MethodBuilder, Report);
			}

			return true;
		}
	}

	public abstract class ConstructorInitializer : ExpressionStatement
	{
		Arguments argument_list;
		MethodGroupExpr base_constructor_group;

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

			// TODO: ec.GetSignatureForError ()
			ConstructorBuilder caller_builder = ((Constructor) ec.MemberContext).ConstructorBuilder;

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
				if (TypeManager.IsStruct (ec.CurrentType)) {
					ec.Report.Error (522, loc,
						"`{0}': Struct constructors cannot call base constructors", TypeManager.CSharpSignature (caller_builder));
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

			base_constructor_group = MemberLookupFinal (
				ec, null, type, ConstructorBuilder.ConstructorName, MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
				loc) as MethodGroupExpr;
			
			if (base_constructor_group == null)
				return this;
			
			base_constructor_group = base_constructor_group.OverloadResolve (
				ec, ref argument_list, false, loc);
			
			if (base_constructor_group == null)
				return this;

			if (!ec.IsStatic)
				base_constructor_group.InstanceExpression = ec.GetThis (loc);
			
			var base_ctor = base_constructor_group.BestCandidate;

			if (base_ctor.MetaInfo == caller_builder){
				ec.Report.Error (516, loc, "Constructor `{0}' cannot call itself", TypeManager.CSharpSignature (caller_builder));
			}
						
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			// It can be null for static initializers
			if (base_constructor_group == null)
				return;
			
			ec.Mark (loc);

			base_constructor_group.EmitCall (ec, argument_list);
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
				return Parameters.IsEmpty;
			
			return Parameters.IsEmpty &&
					(Initializer is ConstructorBaseInitializer) &&
					(Initializer.Arguments == null);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
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

			ConstructorBuilder.SetCustomAttribute (cb);
		}

		protected override bool CheckBase ()
		{
			if ((ModFlags & Modifiers.STATIC) != 0) {
				if (!Parameters.IsEmpty) {
					Report.Error (132, Location, "`{0}': The static constructor must be parameterless",
						GetSignatureForError ());
					return false;
				}

				// the rest can be ignored
				return true;
			}

			// Check whether arguments were correct.
			if (!DefineParameters (Parameters))
				return false;

			if ((caching_flags & Flags.MethodOverloadsExist) != 0)
				Parent.MemberCache.CheckExistingMembersOverloads (this, ConstructorInfo.ConstructorName,
					Parameters, Report);

			if (Parent.PartialContainer.Kind == Kind.Struct) {
				if (Parameters.Count == 0) {
					Report.Error (568, Location, 
						"Structs cannot contain explicit parameterless constructors");
					return false;
				}
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

			MethodAttributes ca = (MethodAttributes.RTSpecialName |
					       MethodAttributes.SpecialName);
			
			if ((ModFlags & Modifiers.STATIC) != 0) {
				ca |= MethodAttributes.Static | MethodAttributes.Private;
			} else {
				ca |= MethodAttributes.HideBySig;

				if ((ModFlags & Modifiers.PUBLIC) != 0)
					ca |= MethodAttributes.Public;
				else if ((ModFlags & Modifiers.PROTECTED) != 0){
					if ((ModFlags & Modifiers.INTERNAL) != 0)
						ca |= MethodAttributes.FamORAssem;
					else 
						ca |= MethodAttributes.Family;
				} else if ((ModFlags & Modifiers.INTERNAL) != 0)
					ca |= MethodAttributes.Assembly;
				else
					ca |= MethodAttributes.Private;
			}

			if (!CheckAbstractAndExtern (block != null))
				return false;
			
			// Check if arguments were correct.
			if (!CheckBase ())
				return false;

			ConstructorBuilder = Parent.TypeBuilder.DefineConstructor (
				ca, CallingConventions,
				Parameters.GetEmitTypes ());

			spec = new MethodSpec (this, ConstructorBuilder, Parameters, ModFlags);

			if (Parent.PartialContainer.IsComImport) {
				if (!IsDefault ()) {
					Report.Error (669, Location, "`{0}': A class with the ComImport attribute cannot have a user-defined constructor",
						Parent.GetSignatureForError ());
				}
				ConstructorBuilder.SetImplementationFlags (MethodImplAttributes.InternalCall);
			}
			
			Parent.MemberCache.AddMember (ConstructorBuilder, this);
			TypeManager.AddMethod (ConstructorBuilder, this);
			
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
				if ((Parent.PartialContainer.Kind == Kind.Struct) &&
					((ModFlags & Modifiers.STATIC) == 0) && (Initializer == null))
					block.AddThisVariable (Parent, Location);

				if (block != null && (ModFlags & Modifiers.STATIC) == 0){
					if (Parent.PartialContainer.Kind == Kind.Class && Initializer == null)
						Initializer = new GeneratedBaseInitializer (Location);

					if (Initializer != null) {
						block.AddScopeStatement (new StatementExpression (Initializer));
					}
				}
			}

			Parameters.ApplyAttributes (ConstructorBuilder);

			SourceMethod source = SourceMethod.Create (Parent, ConstructorBuilder, block);

			if (block != null) {
				if (block.Resolve (null, bc, Parameters, this)) {
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

		// Is never override
		protected override MethodInfo FindOutBaseMethod (ref Type base_ret_type)
		{
			return null;
		}

		public override string GetSignatureForError()
		{
			return base.GetSignatureForError () + Parameters.GetSignatureForError ();
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
			
 			if (!Parameters.IsEmpty) {
 				var al = Parent.MemberCache.Members [ConstructorInfo.ConstructorName];
 				if (al.Count > 2)
 					MemberCache.VerifyClsParameterConflict (al, this, ConstructorBuilder, Report);
 
				if (TypeManager.IsSubclassOf (Parent.TypeBuilder, TypeManager.attribute_type)) {
					foreach (Type param in Parameters.Types) {
						if (param.IsArray) {
							return true;
						}
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

		public Type ReturnType {
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
		Type ReturnType { get; }
		GenericMethod GenericMethod { get; }
		ParametersCompiled ParameterInfo { get; }

		Attributes OptAttributes { get; }
		ToplevelBlock Block { get; set; }

		EmitContext CreateEmitContext (ILGenerator ig);
		ObsoleteAttribute GetObsoleteAttribute ();
		string GetSignatureForError ();
		bool IsExcluded ();
		bool IsClsComplianceRequired ();
		void SetIsUsed ();
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
		public MethodInfo implementing;

		//
		// Protected data.
		//
		protected InterfaceMemberBase member;
		protected Modifiers modifiers;
		protected MethodAttributes flags;
		protected Type declaring_type;
		protected MethodInfo parent_method;

		MethodBuilder builder = null;
		public MethodBuilder MethodBuilder {
			get {
				return builder;
			}
		}

		public Type DeclaringType {
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
				   GenericMethod generic, MethodInfo parent_method)
			: this (member, modifiers, flags, method)
		{
			this.builder = builder;
			this.GenericMethod = generic;
			this.parent_method = parent_method;
		}

		public bool Define (DeclSpace parent, string method_full_name, Report Report)
		{
			string name = method.MethodName.Basename;

			TypeContainer container = parent.PartialContainer;

			PendingImplementation pending = container.PendingImplementations;
			if (pending != null){
				implementing = pending.IsInterfaceMethod (name, member.InterfaceType, this);

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
					if (implementing.IsSpecialName && !(method is AbstractPropertyEventMethod)) {
						Report.SymbolRelatedToPreviousError (implementing);
						Report.Error (683, method.Location, "`{0}' explicit method implementation cannot implement `{1}' because it is an accessor",
							member.GetSignatureForError (), TypeManager.CSharpSignature (implementing));
						return false;
					}
				} else {
					if (implementing != null) {
						AbstractPropertyEventMethod prop_method = method as AbstractPropertyEventMethod;
						if (prop_method == null) {
							if (TypeManager.IsSpecialMethod (implementing)) {
								Report.SymbolRelatedToPreviousError (implementing);
								Report.Error (470, method.Location, "Method `{0}' cannot implement interface accessor `{1}.{2}'",
									method.GetSignatureForError (), TypeManager.CSharpSignature (implementing),
									implementing.Name.StartsWith ("get_") ? "get" : "set");
							}
						} else if (implementing.DeclaringType.IsInterface) {
							if (!implementing.IsSpecialName) {
								Report.SymbolRelatedToPreviousError (implementing);
								Report.Error (686, method.Location, "Accessor `{0}' cannot implement interface member `{1}' for type `{2}'. Use an explicit interface implementation",
									method.GetSignatureForError (), TypeManager.CSharpSignature (implementing), container.GetSignatureForError ());
								return false;
							}
							PropertyBase.PropertyMethod pm = prop_method as PropertyBase.PropertyMethod;
							if (pm != null && pm.HasCustomAccessModifier && (pm.ModFlags & Modifiers.PUBLIC) == 0) {
								Report.SymbolRelatedToPreviousError (implementing);
								Report.Error (277, method.Location, "Accessor `{0}' must be declared public to implement interface member `{1}'",
									method.GetSignatureForError (), TypeManager.CSharpSignature (implementing, true));
								return false;
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
					if (method.ParameterInfo.HasParams && !TypeManager.GetParameterData (implementing).HasParams) {
						Report.SymbolRelatedToPreviousError (implementing);
						Report.Error (466, method.Location, "`{0}': the explicit interface implementation cannot introduce the params modifier",
							method.GetSignatureForError ());
						return false;
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
				flags |=
					MethodAttributes.Virtual |
					MethodAttributes.HideBySig;

				// Set Final unless we're virtual, abstract or already overriding a method.
				if ((modifiers & (Modifiers.VIRTUAL | Modifiers.ABSTRACT | Modifiers.OVERRIDE)) == 0)
					flags |= MethodAttributes.Final;
			}

			DefineMethodBuilder (container, method_full_name, method.ParameterInfo);

			if (builder == null)
				return false;

			if (container.CurrentType != null)
				declaring_type = container.CurrentType;
			else
				declaring_type = container.TypeBuilder;

			if (implementing != null && member.IsExplicitImpl) {
					container.TypeBuilder.DefineMethodOverride (builder, implementing);
			}

			TypeManager.AddMethod (builder, method);

			if (GenericMethod != null) {
				bool is_override = member.IsExplicitImpl |
					((modifiers & Modifiers.OVERRIDE) != 0);

				if (implementing != null)
					parent_method = implementing;

				if (!GenericMethod.DefineType (GenericMethod, builder, parent_method, is_override))
					return false;
			}

			return true;
		}


		/// <summary>
		/// Create the MethodBuilder for the method 
		/// </summary>
		void DefineMethodBuilder (TypeContainer container, string method_name, ParametersCompiled param)
		{
			if (builder == null) {
				builder = container.TypeBuilder.DefineMethod (
					method_name, flags, method.CallingConventions,
					method.ReturnType,
					param.GetEmitTypes ());
				return;
			}

			//
			// Generic method has been already defined to resolve method parameters
			// correctly when they use type parameters
			//
			builder.SetParameters (param.GetEmitTypes ());
			builder.SetReturnType (method.ReturnType);
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
			method.ParameterInfo.ApplyAttributes (MethodBuilder);

			if (GenericMethod != null)
				GenericMethod.EmitAttributes ();

			//
			// clear the pending implementation flag
			//
			if (implementing != null)
				parent.PartialContainer.PendingImplementations.ImplementMethod (method.MethodName.Basename,
					member.InterfaceType, this, member.IsExplicitImpl);

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
			: base (parent, null, TypeManager.system_void_expr, mod, AllowedModifiers,
				new MemberName (MetadataName, l), attrs, parameters)
		{
			ModFlags &= ~Modifiers.PRIVATE;
			ModFlags |= Modifiers.PROTECTED | Modifiers.OVERRIDE;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.Type == pa.Conditional) {
				Error_ConditionalAttributeIsNotValid ();
				return;
			}

			base.ApplyAttributeBuilder (a, cb, pa);
		}

		protected override bool CheckBase ()
		{
			if (!base.CheckBase ())
				return false;

			if (Parent.PartialContainer.BaseCache == null)
				return true;

			Type base_type = Parent.PartialContainer.BaseCache.Container.Type;
			if (base_type != null && Block != null) {
				MethodGroupExpr method_expr = Expression.MethodLookup (Parent.Module.Compiler, Parent.TypeBuilder, base_type, MetadataName, Location);
				if (method_expr == null)
					throw new NotImplementedException ();

				method_expr.IsBase = true;
				method_expr.InstanceExpression = new CompilerGeneratedThis (Parent.TypeBuilder, Location);

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

			return true;
		}

		public override string GetSignatureForError ()
		{
			return Parent.GetSignatureForError () + ".~" + Parent.MemberName.Name + "()";
		}

		protected override MethodInfo FindOutBaseMethod (ref Type base_ret_type)
		{
			return null;
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

		// The accessor are created even if they are not wanted.
		// But we need them because their names are reserved.
		// Field says whether accessor will be emited or not
		public readonly bool IsDummy;

		protected readonly string prefix;

		ReturnParameter return_attributes;

		public AbstractPropertyEventMethod (PropertyBasedMember member, string prefix)
			: base (member.Parent, SetupName (prefix, member, member.Location), null)
		{
			this.prefix = prefix;
			IsDummy = true;
		}

		public AbstractPropertyEventMethod (InterfaceMemberBase member, Accessor accessor,
						    string prefix)
			: base (member.Parent, SetupName (prefix, member, accessor.Location),
				accessor.Attributes)
		{
			this.prefix = prefix;
			this.block = accessor.Block;
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

		public Type[] ParameterTypes { 
			get {
				return ParameterInfo.Types;
			}
		}

		public abstract ParametersCompiled ParameterInfo { get ; }
		public abstract Type ReturnType { get; }

		#endregion

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
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
				method_data.MethodBuilder.SetCustomAttribute (cb);
				return;
			}

			if (a.Target == AttributeTargets.ReturnValue) {
				if (return_attributes == null)
					return_attributes = new ReturnParameter (this, method_data.MethodBuilder, Location);

				return_attributes.ApplyAttributeBuilder (a, cb, pa);
				return;
			}

			ApplyToExtraTarget (a, cb, pa);
		}

		protected virtual void ApplyToExtraTarget (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
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

			if (TypeManager.IsDynamicType (ReturnType)) {
				return_attributes = new ReturnParameter (this, method_data.MethodBuilder, Location);
				PredefinedAttributes.Get.Dynamic.EmitAttribute (return_attributes.Builder);
			} else {
				var trans_flags = TypeManager.HasDynamicTypeUsed (ReturnType);
				if (trans_flags != null) {
					var pa = PredefinedAttributes.Get.DynamicTransform;
					if (pa.Constructor != null || pa.ResolveConstructor (Location, TypeManager.bool_type.MakeArrayType ())) {
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
			// This can only happen with indexers and it will
			// be catched as indexer difference
			if (overload is AbstractPropertyEventMethod)
				return true;

			if (overload is MethodCore) {
				caching_flags |= Flags.MethodOverloadsExist;
				return true;
			}
			return false;
		}

		public override bool IsClsComplianceRequired()
		{
			return false;
		}

		public bool IsDuplicateImplementation (MethodCore method)
		{
			if (!MemberName.Equals (method.MemberName))
				return false;

			Type[] param_types = method.ParameterTypes;

			if (param_types == null || param_types.Length != ParameterTypes.Length)
				return false;

			for (int i = 0; i < param_types.Length; i++)
				if (param_types [i] != ParameterTypes [i])
					return false;

			Report.SymbolRelatedToPreviousError (method);
			Report.Error (82, Location, "A member `{0}' is already reserved",
				method.GetSignatureForError ());
			return true;
		}

		public override bool IsUsed
		{
			get {
				if (IsDummy)
					return false;

				return base.IsUsed;
			}
		}

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

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.Type == pa.Conditional) {
				Error_ConditionalAttributeIsNotValid ();
				return;
			}

			base.ApplyAttributeBuilder (a, cb, pa);
		}
		
		public override bool Define ()
		{
			const Modifiers RequiredModifiers = Modifiers.PUBLIC | Modifiers.STATIC;
			if ((ModFlags & RequiredModifiers) != RequiredModifiers){
				Report.Error (558, Location, "User-defined operator `{0}' must be declared static and public", GetSignatureForError ());
			}

			if (!base.Define ())
				return false;

			// imlicit and explicit operator of same types are not allowed
			if (OperatorType == OpType.Explicit)
				Parent.MemberCache.CheckExistingMembersOverloads (this, GetMetadataName (OpType.Implicit), Parameters, Report);
			else if (OperatorType == OpType.Implicit)
				Parent.MemberCache.CheckExistingMembersOverloads (this, GetMetadataName (OpType.Explicit), Parameters, Report);

			Type declaring_type = MethodData.DeclaringType;
			Type return_type = MemberType;
			Type first_arg_type = ParameterTypes [0];
			
			Type first_arg_type_unwrap = first_arg_type;
			if (TypeManager.IsNullableType (first_arg_type))
				first_arg_type_unwrap = TypeManager.TypeToCoreType (TypeManager.GetTypeArguments (first_arg_type) [0]);
			
			Type return_type_unwrap = return_type;
			if (TypeManager.IsNullableType (return_type))
				return_type_unwrap = TypeManager.TypeToCoreType (TypeManager.GetTypeArguments (return_type) [0]);

			if (TypeManager.IsDynamicType (return_type) || TypeManager.IsDynamicType (first_arg_type)) {
				Report.Error (1964, Location,
					"User-defined operator `{0}' cannot convert to or from the dynamic type",
					GetSignatureForError ());

				return false;
			}

			//
			// Rules for conversion operators
			//
			if (OperatorType == OpType.Implicit || OperatorType == OpType.Explicit) {
				if (first_arg_type_unwrap == return_type_unwrap && first_arg_type_unwrap == declaring_type){
					Report.Error (555, Location,
						"User-defined operator cannot take an object of the enclosing type and convert to an object of the enclosing type");
					return false;
				}
				
				Type conv_type;
				if (TypeManager.IsEqual (declaring_type, return_type) || declaring_type == return_type_unwrap) {
					conv_type = first_arg_type;
				} else if (TypeManager.IsEqual (declaring_type, first_arg_type) || declaring_type == first_arg_type_unwrap) {
					conv_type = return_type;
				} else {
					Report.Error (556, Location, 
						"User-defined conversion must convert to or from the enclosing type");
					return false;
				}

				//
				// Because IsInterface and IsClass are not supported
				//
				if (!TypeManager.IsGenericParameter (conv_type)) {
					if (conv_type.IsInterface) {
						Report.Error (552, Location, "User-defined conversion `{0}' cannot convert to or from an interface type",
							GetSignatureForError ());
						return false;
					}

					if (conv_type.IsClass) {
						if (TypeManager.IsSubclassOf (declaring_type, conv_type)) {
							Report.Error (553, Location, "User-defined conversion `{0}' cannot convert to or from a base class",
								GetSignatureForError ());
							return false;
						}

						if (TypeManager.IsSubclassOf (conv_type, declaring_type)) {
							Report.Error (554, Location, "User-defined conversion `{0}' cannot convert to or from a derived class",
								GetSignatureForError ());
							return false;
						}
					}
				}
			} else if (OperatorType == OpType.LeftShift || OperatorType == OpType.RightShift) {
				if (first_arg_type != declaring_type || Parameters.Types [1] != TypeManager.int32_type) {
					Report.Error (564, Location, "Overloaded shift operator must have the type of the first operand be the containing type, and the type of the second operand must be int");
					return false;
				}
			} else if (Parameters.Count == 1) {
				// Checks for Unary operators

				if (OperatorType == OpType.Increment || OperatorType == OpType.Decrement) {
					if (return_type != declaring_type && !TypeManager.IsSubclassOf (return_type, declaring_type)) {
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
				
				if (!TypeManager.IsEqual (first_arg_type_unwrap, declaring_type)){
					Report.Error (562, Location,
						"The parameter type of a unary operator must be the containing type");
					return false;
				}
				
				if (OperatorType == OpType.True || OperatorType == OpType.False) {
					if (return_type != TypeManager.bool_type){
						Report.Error (
							215, Location,
							"The return type of operator True or False " +
							"must be bool");
						return false;
					}
				}
				
			} else if (!TypeManager.IsEqual (first_arg_type_unwrap, declaring_type)) {
				// Checks for Binary operators

				var second_arg_type = ParameterTypes [1];
				if (TypeManager.IsNullableType (second_arg_type))
					second_arg_type = TypeManager.TypeToCoreType (TypeManager.GetTypeArguments (second_arg_type) [0]);

				if (!TypeManager.IsEqual (second_arg_type, declaring_type)) {
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

		// Operator cannot be override
		protected override MethodInfo FindOutBaseMethod (ref Type base_ret_type)
		{
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
					Parent.GetSignatureForError (), GetName (OperatorType), type_name.GetSignatureForError ());
			}
			else {
				sb.AppendFormat ("{0}.operator {1}", Parent.GetSignatureForError (), GetName (OperatorType));
			}

			sb.Append (Parameters.GetSignatureForError ());
			return sb.ToString ();
		}
	}

	//
	// This is used to compare method signatures
	//
	struct MethodSignature {
		public string Name;
		public Type RetType;
		public Type [] Parameters;
		
		/// <summary>
		///    This delegate is used to extract methods which have the
		///    same signature as the argument
		/// </summary>
		public static MemberFilter method_signature_filter = new MemberFilter (MemberSignatureCompare);
		
		public MethodSignature (string name, Type ret_type, Type [] parameters)
		{
			Name = name;
			RetType = ret_type;

			if (parameters == null)
				Parameters = Type.EmptyTypes;
			else
				Parameters = parameters;
		}

		public override string ToString ()
		{
			string pars = "";
			if (Parameters.Length != 0){
				System.Text.StringBuilder sb = new System.Text.StringBuilder ();
				for (int i = 0; i < Parameters.Length; i++){
					sb.Append (Parameters [i]);
					if (i+1 < Parameters.Length)
						sb.Append (", ");
				}
				pars = sb.ToString ();
			}

			return String.Format ("{0} {1} ({2})", RetType, Name, pars);
		}
		
		public override int GetHashCode ()
		{
			return Name.GetHashCode ();
		}

		public override bool Equals (Object o)
		{
			MethodSignature other = (MethodSignature) o;

			if (other.Name != Name)
				return false;

			if (other.RetType != RetType)
				return false;
			
			if (Parameters == null){
				if (other.Parameters == null)
					return true;
				return false;
			}

			if (other.Parameters == null)
				return false;
			
			int c = Parameters.Length;
			if (other.Parameters.Length != c)
				return false;

			for (int i = 0; i < c; i++)
				if (other.Parameters [i] != Parameters [i])
					return false;

			return true;
		}

		static bool MemberSignatureCompare (MemberInfo m, object filter_criteria)
		{
			MethodSignature sig = (MethodSignature) filter_criteria;

			if (m.Name != sig.Name)
				return false;

			Type ReturnType;
			MethodInfo mi = m as MethodInfo;
			PropertyInfo pi = m as PropertyInfo;

			if (mi != null)
				ReturnType = mi.ReturnType;
			else if (pi != null)
				ReturnType = pi.PropertyType;
			else
				return false;

			//
			// we use sig.RetType == null to mean `do not check the
			// method return value.  
			//
			if (sig.RetType != null) {
				if (!TypeManager.IsEqual (ReturnType, sig.RetType))
					return false;
			}

			Type [] args;
			if (mi != null)
				args = TypeManager.GetParameterData (mi).Types;
			else
				args = TypeManager.GetParameterData (pi).Types;
			Type [] sigp = sig.Parameters;

			if (args.Length != sigp.Length)
				return false;

			for (int i = args.Length - 1; i >= 0; i--)
				if (!TypeManager.IsEqual (args [i], sigp [i]))
					return false;

			return true;
		}
	}
}

