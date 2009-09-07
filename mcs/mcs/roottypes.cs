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
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Mono.CSharp
{
	//
	// Compiled top-level types
	//
	public sealed class ModuleContainer : TypeContainer
	{
		// TODO: It'd be so nice to have generics
		Hashtable anonymous_types;
		public ModuleBuilder Builder;
		readonly bool is_unsafe;
		readonly CompilerContext context;

		bool has_default_charset;

		public CharSet DefaultCharSet = CharSet.Ansi;
		public TypeAttributes DefaultCharSetType = TypeAttributes.AnsiClass;

		static readonly string[] attribute_targets = new string[] { "module" };

		public ModuleContainer (CompilerContext context, bool isUnsafe)
			: base (null, null, MemberName.Null, null, Kind.Root)
		{
			this.is_unsafe = isUnsafe;
			this.context = context;

			types = new ArrayList ();
			anonymous_types = new Hashtable ();
		}

 		public override AttributeTargets AttributeTargets {
 			get {
 				return AttributeTargets.Module;
 			}
		}

		public void AddAnonymousType (AnonymousTypeClass type)
		{
			ArrayList existing = (ArrayList)anonymous_types [type.Parameters.Count];
			if (existing == null) {
				existing = new ArrayList ();
				anonymous_types.Add (type.Parameters.Count, existing);
			}
			existing.Add (type);
		}

		public void AddAttributes (ArrayList attrs)
		{
			foreach (Attribute a in attrs)
				a.AttachTo (this, CodeGen.Assembly);

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

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.Type == pa.CLSCompliant) {
				if (CodeGen.Assembly.ClsCompliantAttribute == null) {
					Report.Warning (3012, 1, a.Location, "You must specify the CLSCompliant attribute on the assembly, not the module, to enable CLS compliance checking");
				} else if (CodeGen.Assembly.IsClsCompliant != a.GetBoolean ()) {
					Report.SymbolRelatedToPreviousError (CodeGen.Assembly.ClsCompliantAttribute.Location, CodeGen.Assembly.ClsCompliantAttribute.GetSignatureForError ());
					Report.Warning (3017, 1, a.Location, "You cannot specify the CLSCompliant attribute on a module that differs from the CLSCompliant attribute on the assembly");
					return;
				}
			}

			Builder.SetCustomAttribute (cb);
		}

		public override CompilerContext Compiler {
			get { return context; }
		}

		public override void Emit ()
		{
			if (OptAttributes != null)
				OptAttributes.Emit ();

			if (is_unsafe) {
				Type t = TypeManager.CoreLookupType (context, "System.Security", "UnverifiableCodeAttribute", Kind.Class, true);
				if (t != null) {
					ConstructorInfo unverifiable_code_ctor = TypeManager.GetPredefinedConstructor (t, Location.Null, Type.EmptyTypes);
					if (unverifiable_code_ctor != null)
						Builder.SetCustomAttribute (new CustomAttributeBuilder (unverifiable_code_ctor, new object [0]));
				}
			}
		}

		public AnonymousTypeClass GetAnonymousType (ArrayList parameters)
		{
			ArrayList candidates = (ArrayList) anonymous_types [parameters.Count];
			if (candidates == null)
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

		public override bool GetClsCompliantAttributeValue ()
		{
			return CodeGen.Assembly.IsClsCompliant;
		}

		public bool HasDefaultCharSet {
			get {
				return has_default_charset;
			}
		}

		public override string GetSignatureForError ()
		{
			return "<module>";
		}

		public override bool IsClsComplianceRequired ()
		{
			return true;
		}

		public override ModuleContainer Module {
			get {
				return this;
			}
		}

		protected override bool AddMemberType (DeclSpace ds)
		{
			if (!AddToContainer (ds, ds.Name))
				return false;
			ds.NamespaceEntry.NS.AddDeclSpace (ds.Basename, ds);
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
		public void Resolve ()
		{
			if (OptAttributes == null)
				return;

			if (!OptAttributes.CheckTargets ())
				return;

			Attribute a = ResolveAttribute (PredefinedAttributes.Get.DefaultCharset);
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
					Report.Error (1724, a.Location, "Value specified for the argument to 'System.Runtime.InteropServices.DefaultCharSetAttribute' is not valid");
					break;
				}
			}
		}

		Attribute ResolveAttribute (PredefinedAttribute a_type)
		{
			Attribute a = OptAttributes.Search (a_type);
			if (a != null) {
				a.Resolve ();
			}
			return a;
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}
	}

	class RootDeclSpace : DeclSpace {
		public RootDeclSpace (NamespaceEntry ns)
			: base (ns, null, MemberName.Null, null)
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

		public override bool Define ()
		{
			throw new InternalErrorException ("should not be called");
		}

		public override TypeBuilder DefineType ()
		{
			throw new InternalErrorException ("should not be called");
		}

		public override MemberCache MemberCache {
			get { return PartialContainer.MemberCache; }
		}

		public override ModuleContainer Module {
			get {
				return PartialContainer.Module;
			}
		}

		public override bool GetClsCompliantAttributeValue ()
		{
			return PartialContainer.GetClsCompliantAttributeValue ();
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
