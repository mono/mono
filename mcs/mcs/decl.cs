//
// decl.cs: Declaration base class for structs, classes, enums and interfaces.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//         Marek Safar (marek.safar@seznam.cz)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001 Ximian, Inc (http://www.ximian.com)
// Copyright 2004-2008 Novell, Inc
//
//

using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Emit;
using System.Reflection;

#if NET_2_1
using XmlElement = System.Object;
#else
using System.Xml;
#endif

namespace Mono.CSharp {

	//
	// Better name would be DottenName
	//
	public class MemberName {
		public readonly string Name;
		public readonly TypeArguments TypeArguments;

		public readonly MemberName Left;
		public readonly Location Location;

		public static readonly MemberName Null = new MemberName ("");

		bool is_double_colon;

		private MemberName (MemberName left, string name, bool is_double_colon,
				    Location loc)
		{
			this.Name = name;
			this.Location = loc;
			this.is_double_colon = is_double_colon;
			this.Left = left;
		}

		private MemberName (MemberName left, string name, bool is_double_colon,
				    TypeArguments args, Location loc)
			: this (left, name, is_double_colon, loc)
		{
			if (args != null && args.Count > 0)
				this.TypeArguments = args;
		}

		public MemberName (string name)
			: this (name, Location.Null)
		{ }

		public MemberName (string name, Location loc)
			: this (null, name, false, loc)
		{ }

		public MemberName (string name, TypeArguments args, Location loc)
			: this (null, name, false, args, loc)
		{ }

		public MemberName (MemberName left, string name)
			: this (left, name, left != null ? left.Location : Location.Null)
		{ }

		public MemberName (MemberName left, string name, Location loc)
			: this (left, name, false, loc)
		{ }

		public MemberName (MemberName left, string name, TypeArguments args, Location loc)
			: this (left, name, false, args, loc)
		{ }

		public MemberName (string alias, string name, TypeArguments args, Location loc)
			: this (new MemberName (alias, loc), name, true, args, loc)
		{ }

		public MemberName (MemberName left, MemberName right)
			: this (left, right, right.Location)
		{ }

		public MemberName (MemberName left, MemberName right, Location loc)
			: this (null, right.Name, false, right.TypeArguments, loc)
		{
			if (right.is_double_colon)
				throw new InternalErrorException ("Cannot append double_colon member name");
			this.Left = (right.Left == null) ? left : new MemberName (left, right.Left);
		}

		// TODO: Remove
		public string GetName ()
		{
			return GetName (false);
		}

		public bool IsGeneric {
			get {
				if (TypeArguments != null)
					return true;
				else if (Left != null)
					return Left.IsGeneric;
				else
					return false;
			}
		}

		public string GetName (bool is_generic)
		{
			string name = is_generic ? Basename : Name;
			if (Left != null)
				return Left.GetName (is_generic) + (is_double_colon ? "::" : ".") + name;

			return name;
		}

		public ATypeNameExpression GetTypeExpression ()
		{
			if (Left == null) {
				if (TypeArguments != null)
					return new SimpleName (Basename, TypeArguments, Location);
				
				return new SimpleName (Name, Location);
			}

			if (is_double_colon) {
				if (Left.Left != null)
					throw new InternalErrorException ("The left side of a :: should be an identifier");
				return new QualifiedAliasMember (Left.Name, Name, TypeArguments, Location);
			}

			Expression lexpr = Left.GetTypeExpression ();
			return new MemberAccess (lexpr, Name, TypeArguments, Location);
		}

		public MemberName Clone ()
		{
			MemberName left_clone = Left == null ? null : Left.Clone ();
			return new MemberName (left_clone, Name, is_double_colon, TypeArguments, Location);
		}

		public string Basename {
			get {
				if (TypeArguments != null)
					return MakeName (Name, TypeArguments);
				return Name;
			}
		}

		public string GetSignatureForError ()
		{
			string append = TypeArguments == null ? "" : "<" + TypeArguments.GetSignatureForError () + ">";
			if (Left == null)
				return Name + append;
			string connect = is_double_colon ? "::" : ".";
			return Left.GetSignatureForError () + connect + Name + append;
		}

		public override bool Equals (object other)
		{
			return Equals (other as MemberName);
		}

		public bool Equals (MemberName other)
		{
			if (this == other)
				return true;
			if (other == null || Name != other.Name)
				return false;
			if (is_double_colon != other.is_double_colon)
				return false;

			if ((TypeArguments != null) &&
			    (other.TypeArguments == null || TypeArguments.Count != other.TypeArguments.Count))
				return false;

			if ((TypeArguments == null) && (other.TypeArguments != null))
				return false;

			if (Left == null)
				return other.Left == null;

			return Left.Equals (other.Left);
		}

		public override int GetHashCode ()
		{
			int hash = Name.GetHashCode ();
			for (MemberName n = Left; n != null; n = n.Left)
				hash ^= n.Name.GetHashCode ();
			if (is_double_colon)
				hash ^= 0xbadc01d;

			if (TypeArguments != null)
				hash ^= TypeArguments.Count << 5;

			return hash & 0x7FFFFFFF;
		}

		public int CountTypeArguments {
			get {
				if (TypeArguments != null)
					return TypeArguments.Count;
				else if (Left != null)
					return Left.CountTypeArguments; 
				else
					return 0;
			}
		}

		public static string MakeName (string name, TypeArguments args)
		{
			if (args == null)
				return name;

			return name + "`" + args.Count;
		}

		public static string MakeName (string name, int count)
		{
			return name + "`" + count;
		}
	}

	public class SimpleMemberName
	{
		public string Value;
		public Location Location;

		public SimpleMemberName (string name, Location loc)
		{
			this.Value = name;
			this.Location = loc;
		}
	}

	/// <summary>
	///   Base representation for members.  This is used to keep track
	///   of Name, Location and Modifier flags, and handling Attributes.
	/// </summary>
	public abstract class MemberCore : Attributable, IMemberContext, IMemberDefinition
	{
		/// <summary>
		///   Public name
		/// </summary>

		protected string cached_name;
		// TODO: Remove in favor of MemberName
		public string Name {
			get {
				if (cached_name == null)
					cached_name = MemberName.GetName (!(this is GenericMethod) && !(this is Method));
				return cached_name;
			}
		}

                // Is not readonly because of IndexerName attribute
		private MemberName member_name;
		public MemberName MemberName {
			get { return member_name; }
		}

		/// <summary>
		///   Modifier flags that the user specified in the source code
		/// </summary>
		private Modifiers mod_flags;
		public Modifiers ModFlags {
			set {
				mod_flags = value;
				if ((value & Modifiers.COMPILER_GENERATED) != 0)
					caching_flags = Flags.IsUsed | Flags.IsAssigned;
			}
			get {
				return mod_flags;
			}
		}

		public /*readonly*/ DeclSpace Parent;

		/// <summary>
		///   Location where this declaration happens
		/// </summary>
		public Location Location {
			get { return member_name.Location; }
		}

		/// <summary>
		///   XML documentation comment
		/// </summary>
		protected string comment;

		/// <summary>
		///   Represents header string for documentation comment 
		///   for each member types.
		/// </summary>
		public abstract string DocCommentHeader { get; }

		[Flags]
		public enum Flags {
			Obsolete_Undetected = 1,		// Obsolete attribute has not been detected yet
			Obsolete = 1 << 1,			// Type has obsolete attribute
			ClsCompliance_Undetected = 1 << 2,	// CLS Compliance has not been detected yet
			ClsCompliant = 1 << 3,			// Type is CLS Compliant
			CloseTypeCreated = 1 << 4,		// Tracks whether we have Closed the type
			HasCompliantAttribute_Undetected = 1 << 5,	// Presence of CLSCompliantAttribute has not been detected
			HasClsCompliantAttribute = 1 << 6,			// Type has CLSCompliantAttribute
			ClsCompliantAttributeTrue = 1 << 7,			// Type has CLSCompliant (true)
			Excluded_Undetected = 1 << 8,		// Conditional attribute has not been detected yet
			Excluded = 1 << 9,					// Method is conditional
			MethodOverloadsExist = 1 << 10,		// Test for duplication must be performed
			IsUsed = 1 << 11,
			IsAssigned = 1 << 12,				// Field is assigned
			HasExplicitLayout	= 1 << 13,
			PartialDefinitionExists	= 1 << 14,	// Set when corresponding partial method definition exists
			HasStructLayout		= 1 << 15			// Has StructLayoutAttribute
		}

		/// <summary>
		///   MemberCore flags at first detected then cached
		/// </summary>
		internal Flags caching_flags;

		public MemberCore (DeclSpace parent, MemberName name, Attributes attrs)
		{
			this.Parent = parent;
			member_name = name;
			caching_flags = Flags.Obsolete_Undetected | Flags.ClsCompliance_Undetected | Flags.HasCompliantAttribute_Undetected | Flags.Excluded_Undetected;
			AddAttributes (attrs, this);
		}

		protected virtual void SetMemberName (MemberName new_name)
		{
			member_name = new_name;
			cached_name = null;
		}

		protected bool CheckAbstractAndExtern (bool has_block)
		{
			if (Parent.PartialContainer.Kind == MemberKind.Interface)
				return true;

			if (has_block) {
				if ((ModFlags & Modifiers.EXTERN) != 0) {
					Report.Error (179, Location, "`{0}' cannot declare a body because it is marked extern",
						GetSignatureForError ());
					return false;
				}

				if ((ModFlags & Modifiers.ABSTRACT) != 0) {
					Report.Error (500, Location, "`{0}' cannot declare a body because it is marked abstract",
						GetSignatureForError ());
					return false;
				}
			} else {
				if ((ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN | Modifiers.PARTIAL)) == 0 && !(Parent is Delegate)) {
					if (RootContext.Version >= LanguageVersion.V_3) {
						Property.PropertyMethod pm = this as Property.PropertyMethod;
						if (pm is Indexer.GetIndexerMethod || pm is Indexer.SetIndexerMethod)
							pm = null;

						if (pm != null && (pm.Property.Get.IsDummy || pm.Property.Set.IsDummy)) {
							Report.Error (840, Location,
								"`{0}' must have a body because it is not marked abstract or extern. The property can be automatically implemented when you define both accessors",
								GetSignatureForError ());
							return false;
						}
					}

					Report.Error (501, Location, "`{0}' must have a body because it is not marked abstract, extern, or partial",
					              GetSignatureForError ());
					return false;
				}
			}

			return true;
		}

		protected void CheckProtectedModifier ()
		{
			if ((ModFlags & Modifiers.PROTECTED) == 0)
				return;

			if (Parent.PartialContainer.Kind == MemberKind.Struct) {
				Report.Error (666, Location, "`{0}': Structs cannot contain protected members",
					GetSignatureForError ());
				return;
			}

			if ((Parent.ModFlags & Modifiers.STATIC) != 0) {
				Report.Error (1057, Location, "`{0}': Static classes cannot contain protected members",
					GetSignatureForError ());
				return;
			}

			if ((Parent.ModFlags & Modifiers.SEALED) != 0 && (ModFlags & Modifiers.OVERRIDE) == 0 &&
				!(this is Destructor)) {
				Report.Warning (628, 4, Location, "`{0}': new protected member declared in sealed class",
					GetSignatureForError ());
				return;
			}
		}

		public abstract bool Define ();

		public virtual string DocComment {
			get {
				return comment;
			}
			set {
				comment = value;
			}
		}

		// 
		// Returns full member name for error message
		//
		public virtual string GetSignatureForError ()
		{
			if (Parent == null || Parent.Parent == null)
				return member_name.GetSignatureForError ();

			return Parent.GetSignatureForError () + "." + member_name.GetSignatureForError ();
		}

		/// <summary>
		/// Base Emit method. This is also entry point for CLS-Compliant verification.
		/// </summary>
		public virtual void Emit ()
		{
			if (!RootContext.VerifyClsCompliance)
				return;

			if (Report.WarningLevel > 0)
				VerifyClsCompliance ();
		}

		public bool IsCompilerGenerated {
			get	{
				if ((mod_flags & Modifiers.COMPILER_GENERATED) != 0)
					return true;

				return Parent == null ? false : Parent.IsCompilerGenerated;
			}
		}

		public virtual bool IsUsed {
			get { return (caching_flags & Flags.IsUsed) != 0; }
		}

		protected Report Report {
			get {
				return Compiler.Report;
			}
		}

		public void SetIsUsed ()
		{
			caching_flags |= Flags.IsUsed;
		}

		/// <summary>
		/// Returns instance of ObsoleteAttribute for this MemberCore
		/// </summary>
		public virtual ObsoleteAttribute GetObsoleteAttribute ()
		{
			if ((caching_flags & (Flags.Obsolete_Undetected | Flags.Obsolete)) == 0)
				return null;

			caching_flags &= ~Flags.Obsolete_Undetected;

			if (OptAttributes == null)
				return null;

			Attribute obsolete_attr = OptAttributes.Search (PredefinedAttributes.Get.Obsolete);
			if (obsolete_attr == null)
				return null;

			caching_flags |= Flags.Obsolete;

			ObsoleteAttribute obsolete = obsolete_attr.GetObsoleteAttribute ();
			if (obsolete == null)
				return null;

			return obsolete;
		}

		/// <summary>
		/// Checks for ObsoleteAttribute presence. It's used for testing of all non-types elements
		/// </summary>
		public virtual void CheckObsoleteness (Location loc)
		{
			ObsoleteAttribute oa = GetObsoleteAttribute ();
			if (oa != null)
				AttributeTester.Report_ObsoleteMessage (oa, GetSignatureForError (), loc, Report);
		}

		//
		// Returns the access level for type `t'
		//
		static Modifiers GetAccessLevelFromType (Type type)
		{
			var ma = type.Attributes;
			Modifiers mod;
			switch (ma & TypeAttributes.VisibilityMask) {
			case TypeAttributes.Public:
			case TypeAttributes.NestedPublic:
				mod = Modifiers.PUBLIC;
				break;
			case TypeAttributes.NestedPrivate:
				mod = Modifiers.PRIVATE;
				break;
			case TypeAttributes.NestedFamily:
				mod = Modifiers.PROTECTED;
				break;
			case TypeAttributes.NestedFamORAssem:
				mod = Modifiers.PROTECTED | Modifiers.INTERNAL;
				break;
			default:
				mod = Modifiers.INTERNAL;
				break;
			}

			return mod;
		}

		//
		// Checks whether the type P is as accessible as this member
		//
		public bool IsAccessibleAs (Type p)
		{
			//
			// if M is private, its accessibility is the same as this declspace.
			// we already know that P is accessible to T before this method, so we
			// may return true.
			//
			if ((mod_flags & Modifiers.PRIVATE) != 0)
				return true;

			while (TypeManager.HasElementType (p))
				p = TypeManager.GetElementType (p);

			if (TypeManager.IsGenericParameter (p))
				return true;

			if (TypeManager.IsGenericType (p)) {
				foreach (Type t in TypeManager.GetTypeArguments (p)) {
					if (!IsAccessibleAs (t))
						return false;
				}
			}

			for (Type p_parent = null; p != null; p = p_parent) {
				p_parent = p.DeclaringType;
				var pAccess = GetAccessLevelFromType (p);
				if (pAccess == Modifiers.PUBLIC)
					continue;

				bool same_access_restrictions = false;
				for (MemberCore mc = this; !same_access_restrictions && mc != null && mc.Parent != null; mc = mc.Parent) {
					var al = mc.ModFlags & Modifiers.AccessibilityMask;
					switch (pAccess) {
					case Modifiers.INTERNAL:
						if (al == Modifiers.PRIVATE || al == Modifiers.INTERNAL)
							same_access_restrictions = TypeManager.IsThisOrFriendAssembly (Parent.Module.Assembly, p.Assembly);
						
						break;

					case Modifiers.PROTECTED:
						if (al == Modifiers.PROTECTED) {
							same_access_restrictions = mc.Parent.IsBaseType (p_parent);
							break;
						}

						if (al == Modifiers.PRIVATE) {
							//
							// When type is private and any of its parents derives from
							// protected type then the type is accessible
							//
							while (mc.Parent != null) {
								if (mc.Parent.IsBaseType (p_parent))
									same_access_restrictions = true;
								mc = mc.Parent; 
							}
						}
						
						break;

					case Modifiers.PROTECTED | Modifiers.INTERNAL:
						if (al == Modifiers.INTERNAL)
							same_access_restrictions = TypeManager.IsThisOrFriendAssembly (Parent.Module.Assembly, p.Assembly);
						else if (al == Modifiers.PROTECTED)
							same_access_restrictions = mc.Parent.IsBaseType (p_parent);
						else if (al == (Modifiers.PROTECTED | Modifiers.INTERNAL))
							same_access_restrictions = mc.Parent.IsBaseType (p_parent) &&
								TypeManager.IsThisOrFriendAssembly (Parent.Module.Assembly, p.Assembly);
						break;

					case Modifiers.PRIVATE:
						//
						// Both are private and share same parent
						//
						if (al == Modifiers.PRIVATE) {
							var decl = mc.Parent;
							do {
								same_access_restrictions = TypeManager.IsEqual (decl.TypeBuilder, p_parent);
							} while (!same_access_restrictions && !decl.IsTopLevel && (decl = decl.Parent) != null);
						}
						
						break;
						
					default:
						throw new InternalErrorException (al.ToString ());
					}
				}
				
				if (!same_access_restrictions)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Analyze whether CLS-Compliant verification must be execute for this MemberCore.
		/// </summary>
		public override bool IsClsComplianceRequired ()
		{
			if ((caching_flags & Flags.ClsCompliance_Undetected) == 0)
				return (caching_flags & Flags.ClsCompliant) != 0;

			if (GetClsCompliantAttributeValue () && IsExposedFromAssembly ()) {
				caching_flags &= ~Flags.ClsCompliance_Undetected;
				caching_flags |= Flags.ClsCompliant;
				return true;
			}

			caching_flags &= ~Flags.ClsCompliance_Undetected;
			return false;
		}

		/// <summary>
		/// Returns true when MemberCore is exposed from assembly.
		/// </summary>
		public bool IsExposedFromAssembly ()
		{
			if ((ModFlags & (Modifiers.PUBLIC | Modifiers.PROTECTED)) == 0)
				return false;
			
			DeclSpace parentContainer = Parent.PartialContainer;
			while (parentContainer != null && parentContainer.ModFlags != 0) {
				if ((parentContainer.ModFlags & (Modifiers.PUBLIC | Modifiers.PROTECTED)) == 0)
					return false;
				parentContainer = parentContainer.Parent;
			}
			return true;
		}

		public virtual ExtensionMethodGroupExpr LookupExtensionMethod (Type extensionType, string name, Location loc)
		{
			return Parent.LookupExtensionMethod (extensionType, name, loc);
		}

		public virtual FullNamedExpression LookupNamespaceAlias (string name)
		{
			return Parent.NamespaceEntry.LookupNamespaceAlias (name);
		}

		public virtual FullNamedExpression LookupNamespaceOrType (string name, Location loc, bool ignore_cs0104)
		{
			return Parent.LookupNamespaceOrType (name, loc, ignore_cs0104);
		}

		/// <summary>
		/// Goes through class hierarchy and gets value of first found CLSCompliantAttribute.
		/// If no is attribute exists then assembly CLSCompliantAttribute is returned.
		/// </summary>
		public virtual bool GetClsCompliantAttributeValue ()
		{
			if ((caching_flags & Flags.HasCompliantAttribute_Undetected) == 0)
				return (caching_flags & Flags.ClsCompliantAttributeTrue) != 0;

			caching_flags &= ~Flags.HasCompliantAttribute_Undetected;

			if (OptAttributes != null) {
				Attribute cls_attribute = OptAttributes.Search (
					PredefinedAttributes.Get.CLSCompliant);
				if (cls_attribute != null) {
					caching_flags |= Flags.HasClsCompliantAttribute;
					bool value = cls_attribute.GetClsCompliantAttributeValue ();
					if (value)
						caching_flags |= Flags.ClsCompliantAttributeTrue;
					return value;
				}
			}
			
			// It's null for TypeParameter
			if (Parent == null)
				return false;			

			if (Parent.GetClsCompliantAttributeValue ()) {
				caching_flags |= Flags.ClsCompliantAttributeTrue;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Returns true if MemberCore is explicitly marked with CLSCompliantAttribute
		/// </summary>
		protected bool HasClsCompliantAttribute {
			get {
				if ((caching_flags & Flags.HasCompliantAttribute_Undetected) != 0)
					GetClsCompliantAttributeValue ();
				
				return (caching_flags & Flags.HasClsCompliantAttribute) != 0;
			}
		}

		/// <summary>
		/// Returns true when a member supports multiple overloads (methods, indexers, etc)
		/// </summary>
		public virtual bool EnableOverloadChecks (MemberCore overload)
		{
			return false;
		}

		/// <summary>
		/// The main virtual method for CLS-Compliant verifications.
		/// The method returns true if member is CLS-Compliant and false if member is not
		/// CLS-Compliant which means that CLS-Compliant tests are not necessary. A descendants override it
		/// and add their extra verifications.
		/// </summary>
		protected virtual bool VerifyClsCompliance ()
		{
			if (!IsClsComplianceRequired ()) {
				if (HasClsCompliantAttribute && Report.WarningLevel >= 2) {
					if (!IsExposedFromAssembly ()) {
						Attribute a = OptAttributes.Search (PredefinedAttributes.Get.CLSCompliant);
						Report.Warning (3019, 2, a.Location, "CLS compliance checking will not be performed on `{0}' because it is not visible from outside this assembly", GetSignatureForError ());
					}

					if (!CodeGen.Assembly.IsClsCompliant) {
						Attribute a = OptAttributes.Search (PredefinedAttributes.Get.CLSCompliant);
						Report.Warning (3021, 2, a.Location, "`{0}' does not need a CLSCompliant attribute because the assembly is not marked as CLS-compliant", GetSignatureForError ());
					}
				}
				return false;
			}

			if (HasClsCompliantAttribute) {
				if (CodeGen.Assembly.ClsCompliantAttribute == null && !CodeGen.Assembly.IsClsCompliant) {
					Attribute a = OptAttributes.Search (PredefinedAttributes.Get.CLSCompliant);
					Report.Warning (3014, 1, a.Location,
						"`{0}' cannot be marked as CLS-compliant because the assembly is not marked as CLS-compliant",
						GetSignatureForError ());
					return false;
				}

				if (!Parent.IsClsComplianceRequired ()) {
					Attribute a = OptAttributes.Search (PredefinedAttributes.Get.CLSCompliant);
					Report.Warning (3018, 1, a.Location, "`{0}' cannot be marked as CLS-compliant because it is a member of non CLS-compliant type `{1}'", 
						GetSignatureForError (), Parent.GetSignatureForError ());
					return false;
				}
			}

			if (member_name.Name [0] == '_') {
				Report.Warning (3008, 1, Location, "Identifier `{0}' is not CLS-compliant", GetSignatureForError () );
			}
			return true;
		}

		//
		// Raised (and passed an XmlElement that contains the comment)
		// when GenerateDocComment is writing documentation expectedly.
		//
		internal virtual void OnGenerateDocComment (XmlElement intermediateNode)
		{
		}

		//
		// Returns a string that represents the signature for this 
		// member which should be used in XML documentation.
		//
		public virtual string GetDocCommentName (DeclSpace ds)
		{
			if (ds == null || this is DeclSpace)
				return DocCommentHeader + Name;
			else
				return String.Concat (DocCommentHeader, ds.Name, ".", Name);
		}

		//
		// Generates xml doc comments (if any), and if required,
		// handle warning report.
		//
		internal virtual void GenerateDocComment (DeclSpace ds)
		{
			try {
				DocUtil.GenerateDocComment (this, ds, Report);
			} catch (Exception e) {
				throw new InternalErrorException (this, e);
			}
		}

		#region IMemberContext Members

		public virtual CompilerContext Compiler {
			get { return Parent.Module.Compiler; }
		}

		public virtual Type CurrentType {
			get { return Parent.CurrentType; }
		}

		public virtual TypeContainer CurrentTypeDefinition {
			get { return Parent.CurrentTypeDefinition; }
		}

		public virtual TypeParameter[] CurrentTypeParameters {
			get { return null; }
		}

		public bool IsObsolete {
			get {
				if (GetObsoleteAttribute () != null)
					return true;

				return Parent == null ? false : Parent.IsObsolete;
			}
		}

		public bool IsUnsafe {
			get {
				if ((ModFlags & Modifiers.UNSAFE) != 0)
					return true;

				return Parent == null ? false : Parent.IsUnsafe;
			}
		}

		public bool IsStatic {
			get { return (ModFlags & Modifiers.STATIC) != 0; }
		}

		#endregion
	}

	//
	// Base member specification. A member specification contains
	// member details which can alter in the context (e.g. generic instances)
	//
	public abstract class MemberSpec
	{
		[Flags]
		protected enum StateFlags
		{
			Obsolete_Undetected = 1,	// Obsolete attribute has not been detected yet
			Obsolete = 1 << 1			// Member has obsolete attribute
		}

		readonly Modifiers modifiers;
		readonly string name;
		protected StateFlags state;
		protected IMemberDefinition definition;
		public readonly MemberKind Kind;

		protected MemberSpec (MemberKind kind, IMemberDefinition definition, string name, Modifiers modifiers)
		{
			this.definition = definition;
			this.name = name;
			this.modifiers = modifiers;

			state = StateFlags.Obsolete_Undetected;
		}

		public abstract Type DeclaringType { get; }

		public ObsoleteAttribute GetObsoleteAttribute ()
		{
			if ((state & (StateFlags.Obsolete | StateFlags.Obsolete_Undetected)) == 0)
				return null;

			state &= ~StateFlags.Obsolete_Undetected;

			var oa = definition.GetObsoleteAttribute ();
			if (oa != null)
				state |= StateFlags.Obsolete;

			return oa;
		}

		public IMemberDefinition MemberDefinition {
			get { return definition; }
		}

		public Modifiers Modifiers {
			get { return modifiers; }
		}
		
		public string Name {
			get { return name; }
		}

		public bool IsStatic {
			get { return (modifiers & Modifiers.STATIC) != 0; }
		}
	}

	//
	// Member details which are same between all member
	// specifications
	//
	public interface IMemberDefinition
	{
		ObsoleteAttribute GetObsoleteAttribute ();
		void SetIsUsed ();
	}

	/// <summary>
	///   Base class for structs, classes, enumerations and interfaces.  
	/// </summary>
	/// <remarks>
	///   They all create new declaration spaces.  This
	///   provides the common foundation for managing those name
	///   spaces.
	/// </remarks>
	public abstract class DeclSpace : MemberCore {
		/// <summary>
		///   This points to the actual definition that is being
		///   created with System.Reflection.Emit
		/// </summary>
		public TypeBuilder TypeBuilder;

		/// <summary>
		///   If we are a generic type, this is the type we are
		///   currently defining.  We need to lookup members on this
		///   instead of the TypeBuilder.
		/// </summary>
		protected Type currentType;

		//
		// This is the namespace in which this typecontainer
		// was declared.  We use this to resolve names.
		//
		public NamespaceEntry NamespaceEntry;

		private Dictionary<string, FullNamedExpression> Cache = new Dictionary<string, FullNamedExpression> ();
		
		public readonly string Basename;
		
		protected Dictionary<string, MemberCore> defined_names;

		public TypeContainer PartialContainer;		

		protected readonly bool is_generic;
		readonly int count_type_params;
		protected TypeParameter[] type_params;
		TypeParameter[] type_param_list;

		//
		// Whether we are Generic
		//
		public bool IsGeneric {
			get {
				if (is_generic)
					return true;
				else if (Parent != null)
					return Parent.IsGeneric;
				else
					return false;
			}
		}

		static string[] attribute_targets = new string [] { "type" };

		public DeclSpace (NamespaceEntry ns, DeclSpace parent, MemberName name,
				  Attributes attrs)
			: base (parent, name, attrs)
		{
			NamespaceEntry = ns;
			Basename = name.Basename;
			defined_names = new Dictionary<string, MemberCore> ();
			PartialContainer = null;
			if (name.TypeArguments != null) {
				is_generic = true;
				count_type_params = name.TypeArguments.Count;
			}
			if (parent != null)
				count_type_params += parent.count_type_params;
		}

		/// <summary>
		/// Adds the member to defined_names table. It tests for duplications and enclosing name conflicts
		/// </summary>
		protected virtual bool AddToContainer (MemberCore symbol, string name)
		{
			MemberCore mc;
			if (!defined_names.TryGetValue (name, out mc)) {
				defined_names.Add (name, symbol);
				return true;
			}

			if (((mc.ModFlags | symbol.ModFlags) & Modifiers.COMPILER_GENERATED) != 0)
				return true;

			if (symbol.EnableOverloadChecks (mc))
				return true;

			InterfaceMemberBase im = mc as InterfaceMemberBase;
			if (im != null && im.IsExplicitImpl)
				return true;

			Report.SymbolRelatedToPreviousError (mc);
			if ((mc.ModFlags & Modifiers.PARTIAL) != 0 && (symbol is ClassOrStruct || symbol is Interface)) {
				Error_MissingPartialModifier (symbol);
				return false;
			}

			if (this is ModuleContainer) {
				Report.Error (101, symbol.Location, 
					"The namespace `{0}' already contains a definition for `{1}'",
					((DeclSpace)symbol).NamespaceEntry.GetSignatureForError (), symbol.MemberName.Name);
			} else if (symbol is TypeParameter) {
				Report.Error (692, symbol.Location,
					"Duplicate type parameter `{0}'", symbol.GetSignatureForError ());
			} else {
				Report.Error (102, symbol.Location,
					      "The type `{0}' already contains a definition for `{1}'",
					      GetSignatureForError (), symbol.MemberName.Name);
			}

			return false;
		}

		protected void RemoveFromContainer (string name)
		{
			defined_names.Remove (name);
		}
		
		/// <summary>
		///   Returns the MemberCore associated with a given name in the declaration
		///   space. It doesn't return method based symbols !!
		/// </summary>
		/// 
		public MemberCore GetDefinition (string name)
		{
			MemberCore mc = null;
			defined_names.TryGetValue (name, out mc);
			return mc;
		}

		public bool IsStaticClass {
			get { return (ModFlags & Modifiers.STATIC) != 0; }
		}
		
		// 
		// root_types contains all the types.  All TopLevel types
		// hence have a parent that points to `root_types', that is
		// why there is a non-obvious test down here.
		//
		public bool IsTopLevel {
			get { return (Parent != null && Parent.Parent == null); }
		}

		public virtual bool IsUnmanagedType ()
		{
			return false;
		}

		public virtual void CloseType ()
		{
			if ((caching_flags & Flags.CloseTypeCreated) == 0){
				try {
					TypeBuilder.CreateType ();
				} catch {
					//
					// The try/catch is needed because
					// nested enumerations fail to load when they
					// are defined.
					//
					// Even if this is the right order (enumerations
					// declared after types).
					//
					// Note that this still creates the type and
					// it is possible to save it
				}
				caching_flags |= Flags.CloseTypeCreated;
			}
		}

		protected virtual TypeAttributes TypeAttr {
			get { return Module.DefaultCharSetType; }
		}

		/// <remarks>
		///  Should be overriten by the appropriate declaration space
		/// </remarks>
		public abstract TypeBuilder DefineType ();

		protected void Error_MissingPartialModifier (MemberCore type)
		{
			Report.Error (260, type.Location,
				"Missing partial modifier on declaration of type `{0}'. Another partial declaration of this type exists",
				type.GetSignatureForError ());
		}

		public override void Emit ()
		{
			if (type_params != null) {
				int offset = count_type_params - type_params.Length;
				for (int i = offset; i < type_params.Length; i++)
					CurrentTypeParameters [i - offset].Emit ();
			}

			if ((ModFlags & Modifiers.COMPILER_GENERATED) != 0 && !Parent.IsCompilerGenerated)
				PredefinedAttributes.Get.CompilerGenerated.EmitAttribute (TypeBuilder);

			base.Emit ();
		}

		public override string GetSignatureForError ()
		{	
			return MemberName.GetSignatureForError ();
		}
		
		public bool CheckAccessLevel (Type check_type)
		{
			Type tb = TypeBuilder;

			if (this is GenericMethod) {
				tb = Parent.TypeBuilder;

				// FIXME: Generic container does not work with nested generic
				// anonymous method stories
				if (TypeBuilder == null)
					return true;
			}

			check_type = TypeManager.DropGenericTypeArguments (check_type);
			if (check_type == tb)
				return true;

			// TODO: When called from LocalUsingAliasEntry tb is null
			// because we are in RootDeclSpace
			if (tb == null)
				tb = typeof (RootDeclSpace);

			//
			// Broken Microsoft runtime, return public for arrays, no matter what 
			// the accessibility is for their underlying class, and they return 
			// NonPublic visibility for pointers
			//
			if (TypeManager.HasElementType (check_type))
				return CheckAccessLevel (TypeManager.GetElementType (check_type));

			if (TypeManager.IsGenericParameter (check_type))
				return true;

			TypeAttributes check_attr = check_type.Attributes & TypeAttributes.VisibilityMask;

			switch (check_attr){
			case TypeAttributes.Public:
				return true;

			case TypeAttributes.NotPublic:
				return TypeManager.IsThisOrFriendAssembly (Module.Assembly, check_type.Assembly);
				
			case TypeAttributes.NestedPublic:
				return CheckAccessLevel (check_type.DeclaringType);

			case TypeAttributes.NestedPrivate:
				Type declaring = check_type.DeclaringType;
				return tb == declaring || TypeManager.IsNestedChildOf (tb, declaring);	

			case TypeAttributes.NestedFamily:
				//
				// Only accessible to methods in current type or any subtypes
				//
				return FamilyAccessible (tb, check_type);

			case TypeAttributes.NestedFamANDAssem:
				return TypeManager.IsThisOrFriendAssembly (Module.Assembly, check_type.Assembly) && 
					FamilyAccessible (tb, check_type);

			case TypeAttributes.NestedFamORAssem:
				return FamilyAccessible (tb, check_type) ||
					TypeManager.IsThisOrFriendAssembly (Module.Assembly, check_type.Assembly);

			case TypeAttributes.NestedAssembly:
				return TypeManager.IsThisOrFriendAssembly (Module.Assembly, check_type.Assembly);
			}

			throw new NotImplementedException (check_attr.ToString ());
		}

		static bool FamilyAccessible (Type tb, Type check_type)
		{
			Type declaring = check_type.DeclaringType;
			return TypeManager.IsNestedFamilyAccessible (tb, declaring);
		}

		public bool IsBaseType (Type baseType)
		{
			// We are called from RootDeclspace
			if (TypeBuilder == null)
				return false;

			return TypeManager.IsSubclassOf (TypeBuilder, baseType);
		}

		private Type LookupNestedTypeInHierarchy (string name)
		{
			Type t = null;
			// if the member cache has been created, lets use it.
			// the member cache is MUCH faster.
			if (MemberCache != null) {
				t = MemberCache.FindNestedType (name);
				if (t == null)
					return null;
			}

			//
			// FIXME: This hack is needed because member cache does not work
			// with nested base generic types, it does only type name copy and
			// not type construction
			//

			// no member cache. Do it the hard way -- reflection
			for (Type current_type = TypeBuilder;
			     current_type != null && current_type != TypeManager.object_type;
			     current_type = current_type.BaseType) {

				Type ct = TypeManager.DropGenericTypeArguments (current_type);
				if (ct is TypeBuilder) {
					TypeContainer tc = ct == TypeBuilder
						? PartialContainer : TypeManager.LookupTypeContainer (ct);
					if (tc != null)
						t = tc.FindNestedType (name);
				} else {
					t = TypeManager.GetNestedType (ct, name);
				}

				if ((t == null) || !CheckAccessLevel (t))
					continue;

				if (!TypeManager.IsGenericType (current_type))
					return t;

				Type[] args = TypeManager.GetTypeArguments (current_type);
				Type[] targs = TypeManager.GetTypeArguments (t);
				for (int i = 0; i < args.Length; i++)
					targs [i] = TypeManager.TypeToCoreType (args [i]);

				return t.MakeGenericType (targs);
			}

			return null;
		}

		//
		// Public function used to locate types.
		//
		// Set 'ignore_cs0104' to true if you want to ignore cs0104 errors.
		//
		// Returns: Type or null if they type can not be found.
		//
		public override FullNamedExpression LookupNamespaceOrType (string name, Location loc, bool ignore_cs0104)
		{
			FullNamedExpression e;
			if (Cache.TryGetValue (name, out e))
				return e;

			e = null;
			int errors = Report.Errors;

			TypeParameter[] tp = CurrentTypeParameters;
			if (tp != null) {
				TypeParameter tparam = TypeParameter.FindTypeParameter (tp, name);
				if (tparam != null)
					e = new TypeParameterExpr (tparam, Location.Null);
			}

			if (e == null) {
				Type t = LookupNestedTypeInHierarchy (name);

				if (t != null)
					e = new TypeExpression (t, Location.Null);
				else if (Parent != null)
					e = Parent.LookupNamespaceOrType (name, loc, ignore_cs0104);
				else
					e = NamespaceEntry.LookupNamespaceOrType (name, loc, ignore_cs0104);
			}

			if (errors == Report.Errors)
				Cache [name] = e;
			
			return e;
		}

		/// <remarks>
		///   This function is broken and not what you're looking for.  It should only
		///   be used while the type is still being created since it doesn't use the cache
		///   and relies on the filter doing the member name check.
		/// </remarks>
		///
		// [Obsolete ("Only MemberCache approach should be used")]
		public virtual MemberList FindMembers (MemberTypes mt, BindingFlags bf,
							MemberFilter filter, object criteria)
		{
			throw new NotSupportedException ();
		}

		/// <remarks>
		///   If we have a MemberCache, return it.  This property may return null if the
		///   class doesn't have a member cache or while it's still being created.
		/// </remarks>
		public abstract MemberCache MemberCache {
			get;
		}

		public virtual ModuleContainer Module {
			get { return Parent.Module; }
		}

		public override void ApplyAttributeBuilder (Attribute a, ConstructorInfo ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Type == pa.Required) {
				Report.Error (1608, a.Location, "The RequiredAttribute attribute is not permitted on C# types");
				return;
			}

			TypeBuilder.SetCustomAttribute (ctor, cdata);
		}

		TypeParameter[] initialize_type_params ()
		{
			if (type_param_list != null)
				return type_param_list;

			DeclSpace the_parent = Parent;
			if (this is GenericMethod)
				the_parent = null;

			var list = new List<TypeParameter> ();
			if (the_parent != null && the_parent.IsGeneric) {
				// FIXME: move generics info out of DeclSpace
				TypeParameter[] parent_params = the_parent.TypeParameters;
				list.AddRange (parent_params);
			}
 
			int count = type_params != null ? type_params.Length : 0;
			for (int i = 0; i < count; i++) {
				TypeParameter param = type_params [i];
				list.Add (param);
				if (Parent.CurrentTypeParameters != null) {
					foreach (TypeParameter tp in Parent.CurrentTypeParameters) {
						if (tp.Name != param.Name)				
							continue;

						Report.SymbolRelatedToPreviousError (tp.Location, null);
						Report.Warning (693, 3, param.Location,
							"Type parameter `{0}' has the same name as the type parameter from outer type `{1}'",
							param.Name, Parent.GetSignatureForError ());
					}
				}
			}

			type_param_list = new TypeParameter [list.Count];
			list.CopyTo (type_param_list, 0);
			return type_param_list;
		}

		public virtual void SetParameterInfo (List<Constraints> constraints_list)
		{
			if (!is_generic) {
				if (constraints_list != null) {
					Report.Error (
						80, Location, "Constraints are not allowed " +
						"on non-generic declarations");
				}

				return;
			}

			TypeParameterName[] names = MemberName.TypeArguments.GetDeclarations ();
			type_params = new TypeParameter [names.Length];

			//
			// Register all the names
			//
			for (int i = 0; i < type_params.Length; i++) {
				TypeParameterName name = names [i];

				Constraints constraints = null;
				if (constraints_list != null) {
					int total = constraints_list.Count;
					for (int ii = 0; ii < total; ++ii) {
						Constraints constraints_at = (Constraints)constraints_list[ii];
						// TODO: it is used by iterators only
						if (constraints_at == null) {
							constraints_list.RemoveAt (ii);
							--total;
							continue;
						}
						if (constraints_at.TypeParameter.Value == name.Name) {
							constraints = constraints_at;
							constraints_list.RemoveAt(ii);
							break;
						}
					}
				}

				Variance variance = name.Variance;
				if (name.Variance != Variance.None && !(this is Delegate || this is Interface)) {
					Report.Error (1960, name.Location, "Variant type parameters can only be used with interfaces and delegates");
					variance = Variance.None;
				}

				type_params [i] = new TypeParameter (
					Parent, this, name.Name, constraints, name.OptAttributes, variance, Location);

				AddToContainer (type_params [i], name.Name);
			}

			if (constraints_list != null && constraints_list.Count > 0) {
				foreach (Constraints constraint in constraints_list) {
					Report.Error(699, constraint.Location, "`{0}': A constraint references nonexistent type parameter `{1}'", 
						GetSignatureForError (), constraint.TypeParameter.Value);
				}
			}
		}

		public TypeParameter[] TypeParameters {
			get {
				if (!IsGeneric)
					throw new InvalidOperationException ();
				if ((PartialContainer != null) && (PartialContainer != this))
					return PartialContainer.TypeParameters;
				if (type_param_list == null)
					initialize_type_params ();

				return type_param_list;
			}
		}

		public override Type CurrentType {
			get { return currentType != null ? currentType : TypeBuilder; }
		}

		public override TypeContainer CurrentTypeDefinition {
			get { return PartialContainer; }
		}

		public int CountTypeParameters {
			get {
				return count_type_params;
			}
		}

		// Used for error reporting only
		public virtual Type LookupAnyGeneric (string typeName)
		{
			return NamespaceEntry.NS.LookForAnyGenericType (typeName);
		}

		public override string[] ValidAttributeTargets {
			get { return attribute_targets; }
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ()) {
				return false;
			}

			if (type_params != null) {
				foreach (TypeParameter tp in type_params) {
					if (tp.Constraints == null)
						continue;

					tp.Constraints.VerifyClsCompliance (Report);
				}
			}

			var cache = TypeManager.AllClsTopLevelTypes;
			if (cache == null)
				return true;

			string lcase = Name.ToLower (System.Globalization.CultureInfo.InvariantCulture);
			if (!cache.ContainsKey (lcase)) {
				cache.Add (lcase, this);
				return true;
			}

			object val = cache [lcase];
			if (val == null) {
				Type t = AttributeTester.GetImportedIgnoreCaseClsType (lcase);
				if (t == null)
					return true;
				Report.SymbolRelatedToPreviousError (t);
			}
			else {
				Report.SymbolRelatedToPreviousError ((DeclSpace)val);
			}

			Report.Warning (3005, 1, Location, "Identifier `{0}' differing only in case is not CLS-compliant", GetSignatureForError ());
			return true;
		}
	}
}
