//
// roottypes.cs: keeps a tree representation of the generated code
//
// Authors: Miguel de Icaza (miguel@gnu.org)
//          Marek Safar  (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001-2003 Ximian, Inc (http://www.ximian.com)
// Copyright 2003-2008 Novell, Inc.
//

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp
{
	//
	// Module (top-level type) container
	//
	public class ModuleContainer : TypeContainer
	{
		public CharSet DefaultCharSet = CharSet.Ansi;
		public TypeAttributes DefaultCharSetType = TypeAttributes.AnsiClass;

		Dictionary<int, List<AnonymousTypeClass>> anonymous_types;

		AssemblyDefinition assembly;
		readonly CompilerContext context;
		readonly RootNamespace global_ns;
		Dictionary<string, RootNamespace> alias_ns;

		ModuleBuilder builder;
		int static_data_counter;

		// HACK
		public List<Enum> hack_corlib_enums = new List<Enum> ();

		bool has_default_charset;
		bool has_extenstion_method;

		PredefinedAttributes predefined_attributes;
		PredefinedTypes predefined_types;

		static readonly string[] attribute_targets = new string[] { "assembly", "module" };

		public ModuleContainer (CompilerContext context)
			: base (null, null, MemberName.Null, null, 0)
		{
			this.context = context;

			caching_flags &= ~(Flags.Obsolete_Undetected | Flags.Excluded_Undetected);

			types = new List<TypeContainer> ();
			anonymous_types = new Dictionary<int, List<AnonymousTypeClass>> ();
			global_ns = new GlobalRootNamespace ();
			alias_ns = new Dictionary<string, RootNamespace> ();
		}

		#region Properties

 		public override AttributeTargets AttributeTargets {
 			get {
 				return AttributeTargets.Assembly;
 			}
		}

		public ModuleBuilder Builder {
			get {
				return builder;
			}
		}

		public override CompilerContext Compiler {
			get {
				return context;
			}
		}

		public override AssemblyDefinition DeclaringAssembly {
			get {
				return assembly;
			}
		}

		public bool HasDefaultCharSet {
			get {
				return has_default_charset;
			}
		}

		public bool HasExtensionMethod {
			get {
				return has_extenstion_method;
			}
			set {
				has_extenstion_method = value;
			}
		}

		//
		// Returns module global:: namespace
		//
		public RootNamespace GlobalRootNamespace {
		    get {
		        return global_ns;
		    }
		}

		public override ModuleContainer Module {
			get {
				return this;
			}
		}

		internal PredefinedAttributes PredefinedAttributes {
			get {
				return predefined_attributes;
			}
		}

		internal PredefinedTypes PredefinedTypes {
			get {
				return predefined_types;
			}
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		#endregion

		public void AddAnonymousType (AnonymousTypeClass type)
		{
			List<AnonymousTypeClass> existing;
			if (!anonymous_types.TryGetValue (type.Parameters.Count, out existing))
			if (existing == null) {
				existing = new List<AnonymousTypeClass> ();
				anonymous_types.Add (type.Parameters.Count, existing);
			}

			existing.Add (type);
		}

		public void AddAttributes (List<Attribute> attrs)
		{
			AddAttributes (attrs, this);
		}

		public void AddAttributes (List<Attribute> attrs, IMemberContext context)
		{
			foreach (Attribute a in attrs)
				a.AttachTo (this, context);

			if (attributes == null) {
				attributes = new Attributes (attrs);
				return;
			}
			attributes.AddAttributes (attrs);
		}

		public override TypeContainer AddPartial (TypeContainer nextPart)
		{
			return AddPartial (nextPart, nextPart.Name);
		}

		public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Target == AttributeTargets.Assembly) {
				assembly.ApplyAttributeBuilder (a, ctor, cdata, pa);
				return;
			}

			if (a.Type == pa.CLSCompliant) {
				Attribute cls = DeclaringAssembly.CLSCompliantAttribute;
				if (cls == null) {
					Report.Warning (3012, 1, a.Location,
						"You must specify the CLSCompliant attribute on the assembly, not the module, to enable CLS compliance checking");
				} else if (DeclaringAssembly.IsCLSCompliant != a.GetBoolean ()) {
					Report.SymbolRelatedToPreviousError (cls.Location, cls.GetSignatureForError ());
					Report.Warning (3017, 1, a.Location,
						"You cannot specify the CLSCompliant attribute on a module that differs from the CLSCompliant attribute on the assembly");
					return;
				}
			}

			builder.SetCustomAttribute ((ConstructorInfo) ctor.GetMetaInfo (), cdata);
		}

		public new void CloseType ()
		{
			HackCorlibEnums ();

			foreach (TypeContainer tc in types) {
				tc.CloseType ();
			}

			if (compiler_generated != null)
				foreach (CompilerGeneratedClass c in compiler_generated)
					c.CloseType ();

			//
			// If we have a <PrivateImplementationDetails> class, close it
			//
			if (TypeBuilder != null) {
				var cg = PredefinedAttributes.CompilerGenerated;
				cg.EmitAttribute (TypeBuilder);
				TypeBuilder.CreateType ();
			}
		}

		public TypeBuilder CreateBuilder (string name, TypeAttributes attr, int typeSize)
		{
			return builder.DefineType (name, attr, null, typeSize);
		}

		//
		// Creates alias global namespace
		//
		public RootNamespace CreateRootNamespace (string alias)
		{
			if (alias == global_ns.Alias) {
				NamespaceEntry.Error_GlobalNamespaceRedefined (Location.Null, Report);
				return global_ns;
			}

			RootNamespace rn;
			if (!alias_ns.TryGetValue (alias, out rn)) {
				rn = new RootNamespace (alias);
				alias_ns.Add (alias, rn);
			}

			return rn;
		}

		public new void Define ()
		{
			builder = assembly.CreateModuleBuilder ();

			// FIXME: Temporary hack for repl to reset
			TypeBuilder = null;

			// TODO: It should be done much later when the types are resolved
			// but that require DefineType clean-up
			ResolveGlobalAttributes ();

			foreach (TypeContainer tc in types)
				tc.CreateType ();

			InitializePredefinedTypes ();

			foreach (TypeContainer tc in types)
				tc.DefineType ();

			foreach (TypeContainer tc in types)
				tc.ResolveTypeParameters ();

			foreach (TypeContainer tc in types) {
				try {
					tc.Define ();
				} catch (Exception e) {
					throw new InternalErrorException (tc, e);
				}
			}
		}

		public override void Emit ()
		{
			if (OptAttributes != null)
				OptAttributes.Emit ();

			if (RootContext.Unsafe) {
				var pa = PredefinedAttributes.UnverifiableCode;
				if (pa.IsDefined)
					pa.EmitAttribute (builder);
			}

			foreach (var tc in types)
				tc.DefineConstants ();

			HackCorlib ();

			foreach (TypeContainer tc in types)
				tc.EmitType ();

			if (Compiler.Report.Errors > 0)
				return;

			foreach (TypeContainer tc in types)
				tc.VerifyMembers ();

			if (compiler_generated != null)
				foreach (var c in compiler_generated)
					c.EmitType ();
		}

		public AnonymousTypeClass GetAnonymousType (IList<AnonymousTypeParameter> parameters)
		{
			List<AnonymousTypeClass> candidates;
			if (!anonymous_types.TryGetValue (parameters.Count, out candidates))
				return null;

			int i;
			foreach (AnonymousTypeClass at in candidates) {
				for (i = 0; i < parameters.Count; ++i) {
					if (!parameters [i].Equals (at.Parameters [i]))
						break;
				}

				if (i == parameters.Count)
					return at;
			}

			return null;
		}

		public RootNamespace GetRootNamespace (string name)
		{
			RootNamespace rn;
			alias_ns.TryGetValue (name, out rn);
			return rn;
		}

		public override string GetSignatureForError ()
		{
			return "<module>";
		}

		void HackCorlib ()
		{
			if (RootContext.StdLib)
				return;

			//
			// HACK: When building corlib mcs uses loaded mscorlib which
			// has different predefined types and this method sets mscorlib types
			// to be same to avoid type check errors in CreateType.
			//
			var type = typeof (Type);
			var system_4_type_arg = new[] { type, type, type, type };

			MethodInfo set_corlib_type_builders =
				typeof (System.Reflection.Emit.AssemblyBuilder).GetMethod (
				"SetCorlibTypeBuilders", BindingFlags.NonPublic | BindingFlags.Instance, null,
				system_4_type_arg, null);

			if (set_corlib_type_builders == null) {
				Compiler.Report.Warning (-26, 3,
					"The compilation may fail due to missing `System.Reflection.Emit.AssemblyBuilder.SetCorlibTypeBuilders(...)' method");
				return;
			}

			object[] args = new object[4];
			args[0] = TypeManager.object_type.GetMetaInfo ();
			args[1] = TypeManager.value_type.GetMetaInfo ();
			args[2] = TypeManager.enum_type.GetMetaInfo ();
			args[3] = TypeManager.void_type.GetMetaInfo ();
			set_corlib_type_builders.Invoke (assembly.Builder, args);
		}

		void HackCorlibEnums ()
		{
			if (RootContext.StdLib)
				return;

			// Another Mono corlib HACK
			// mono_class_layout_fields requires to have enums created
			// before creating a class which used the enum for any of its fields
			foreach (var e in hack_corlib_enums)
				e.CloseType ();
		}

		public void InitializePredefinedTypes ()
		{
			predefined_attributes = new PredefinedAttributes (this);
			predefined_types = new PredefinedTypes (this);
		}

		public override bool IsClsComplianceRequired ()
		{
			return DeclaringAssembly.IsCLSCompliant;
		}
		
		public AssemblyDefinition MakeExecutable (string name)
		{
			assembly = new AssemblyDefinition (this, name);
			return assembly;
		}
		
		public AssemblyDefinition MakeExecutable (string name, string fileName)
		{
			assembly = new AssemblyDefinition (this, name, fileName);
			return assembly;
		}

		//
		// Makes an initialized struct, returns the field builder that
		// references the data.  Thanks go to Sergey Chaban for researching
		// how to do this.  And coming up with a shorter mechanism than I
		// was able to figure out.
		//
		// This works but makes an implicit public struct $ArrayType$SIZE and
		// makes the fields point to it.  We could get more control if we did
		// use instead:
		//
		// 1. DefineNestedType on the impl_details_class with our struct.
		//
		// 2. Define the field on the impl_details_class
		//
		public FieldBuilder MakeStaticData (byte[] data)
		{
			if (TypeBuilder == null) {
				TypeBuilder = builder.DefineType ("<PrivateImplementationDetails>",
					TypeAttributes.NotPublic, TypeManager.object_type.GetMetaInfo ());
			}

			var fb = TypeBuilder.DefineInitializedData (
				"$$field-" + (static_data_counter++), data,
				FieldAttributes.Static | FieldAttributes.Assembly);

			return fb;
		}

		protected override bool AddMemberType (TypeContainer ds)
		{
			if (!AddToContainer (ds, ds.Name))
				return false;
			ds.NamespaceEntry.NS.AddType (ds.Definition);
			return true;
		}

		protected override void RemoveMemberType (DeclSpace ds)
		{
			ds.NamespaceEntry.NS.RemoveDeclSpace (ds.Basename);
			base.RemoveMemberType (ds);
		}

		/// <summary>
		/// It is called very early therefore can resolve only predefined attributes
		/// </summary>
		void ResolveGlobalAttributes ()
		{
			if (OptAttributes == null)
				return;

			if (!OptAttributes.CheckTargets ())
				return;

			// FIXME: Define is wrong as the type may not exist yet
			var DefaultCharSet_attr = new PredefinedAttribute (this, "System.Runtime.InteropServices", "DefaultCharSetAttribute");
			DefaultCharSet_attr.Define ();
			Attribute a = ResolveModuleAttribute (DefaultCharSet_attr);
			if (a != null) {
				has_default_charset = true;
				DefaultCharSet = a.GetCharSetValue ();
				switch (DefaultCharSet) {
				case CharSet.Ansi:
				case CharSet.None:
					break;
				case CharSet.Auto:
					DefaultCharSetType = TypeAttributes.AutoClass;
					break;
				case CharSet.Unicode:
					DefaultCharSetType = TypeAttributes.UnicodeClass;
					break;
				default:
					Report.Error (1724, a.Location, "Value specified for the argument to `{0}' is not valid", 
						DefaultCharSet_attr.GetSignatureForError ());
					break;
				}
			}
		}

		public Attribute ResolveAssemblyAttribute (PredefinedAttribute a_type)
		{
			Attribute a = OptAttributes.Search ("assembly", a_type);
			if (a != null) {
				a.Resolve ();
			}
			return a;
		}

		Attribute ResolveModuleAttribute (PredefinedAttribute a_type)
		{
			Attribute a = OptAttributes.Search ("module", a_type);
			if (a != null) {
				a.Resolve ();
			}
			return a;
		}
	}

	class RootDeclSpace : TypeContainer {
		public RootDeclSpace (NamespaceEntry ns)
			: base (ns, null, MemberName.Null, null, 0)
		{
			PartialContainer = RootContext.ToplevelTypes;
		}

		public override AttributeTargets AttributeTargets {
			get { throw new InternalErrorException ("should not be called"); }
		}

		public override CompilerContext Compiler {
			get {
				return PartialContainer.Compiler;
			}
		}

		public override string DocCommentHeader {
			get { throw new InternalErrorException ("should not be called"); }
		}

		public override void DefineType ()
		{
			throw new InternalErrorException ("should not be called");
		}

		public override ModuleContainer Module {
			get {
				return PartialContainer.Module;
			}
		}

		public override bool IsClsComplianceRequired ()
		{
			return PartialContainer.IsClsComplianceRequired ();
		}

		public override FullNamedExpression LookupNamespaceAlias (string name)
		{
			return NamespaceEntry.LookupNamespaceAlias (name);
		}
	}
}
