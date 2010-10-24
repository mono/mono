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
		public TypeArguments TypeArguments;

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

		public int Arity {
			get {
				return TypeArguments == null ? 0 : TypeArguments.Count;
			}
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
					return new SimpleName (Name, TypeArguments, Location);
				
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
	[System.Diagnostics.DebuggerDisplay ("{GetSignatureForError()}")]
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

		string IMemberDefinition.Name {
			get {
				return member_name.Name;
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

		public /*readonly*/ TypeContainer Parent;

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
			ClsCompliantAttributeFalse = 1 << 7,			// Member has CLSCompliant(false)
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
			this.Parent = parent as TypeContainer;
			member_name = name;
			caching_flags = Flags.Obsolete_Undetected | Flags.ClsCompliance_Undetected | Flags.HasCompliantAttribute_Undetected | Flags.Excluded_Undetected;
			AddAttributes (attrs, this);
		}

		public virtual Assembly Assembly {
			get { return Parent.Module.Assembly; }
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

						if (pm != null && pm.Property.AccessorSecond == null) {
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

			VerifyClsCompliance ();
		}

		public bool IsCompilerGenerated {
			get	{
				if ((mod_flags & Modifiers.COMPILER_GENERATED) != 0)
					return true;

				return Parent == null ? false : Parent.IsCompilerGenerated;
			}
		}

		public bool IsImported {
			get {
				return false;
			}
		}

		public virtual bool IsUsed {
			get {
				return (caching_flags & Flags.IsUsed) != 0;
			}
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

		public void SetIsAssigned ()
		{
			caching_flags |= Flags.IsAssigned;
		}

		/// <summary>
		/// Returns instance of ObsoleteAttribute for this MemberCore
		/// </summary>
		public virtual ObsoleteAttribute GetAttributeObsolete ()
		{
			if ((caching_flags & (Flags.Obsolete_Undetected | Flags.Obsolete)) == 0)
				return null;

			caching_flags &= ~Flags.Obsolete_Undetected;

			if (OptAttributes == null)
				return null;

			Attribute obsolete_attr = OptAttributes.Search (Compiler.PredefinedAttributes.Obsolete);
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
			ObsoleteAttribute oa = GetAttributeObsolete ();
			if (oa != null)
				AttributeTester.Report_ObsoleteMessage (oa, GetSignatureForError (), loc, Report);
		}

		//
		// Checks whether the type P is as accessible as this member
		//
		public bool IsAccessibleAs (TypeSpec p)
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

			if (p.IsGenericParameter)
				return true;

			for (TypeSpec p_parent; p != null; p = p_parent) {
				p_parent = p.DeclaringType;

				if (p.IsGeneric) {
					foreach (TypeSpec t in p.TypeArguments) {
						if (!IsAccessibleAs (t))
							return false;
					}
				}

				var pAccess = p.Modifiers & Modifiers.AccessibilityMask;
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
							same_access_restrictions = mc.Parent.IsBaseTypeDefinition (p_parent);
							break;
						}

						if (al == Modifiers.PRIVATE) {
							//
							// When type is private and any of its parents derives from
							// protected type then the type is accessible
							//
							while (mc.Parent != null) {
								if (mc.Parent.IsBaseTypeDefinition (p_parent))
									same_access_restrictions = true;
								mc = mc.Parent; 
							}
						}
						
						break;

					case Modifiers.PROTECTED | Modifiers.INTERNAL:
						if (al == Modifiers.INTERNAL)
							same_access_restrictions = TypeManager.IsThisOrFriendAssembly (Parent.Module.Assembly, p.Assembly);
						else if (al == (Modifiers.PROTECTED | Modifiers.INTERNAL))
							same_access_restrictions = mc.Parent.IsBaseTypeDefinition (p_parent) &&
								TypeManager.IsThisOrFriendAssembly (Parent.Module.Assembly, p.Assembly);
						else
							goto case Modifiers.PROTECTED;

						break;

					case Modifiers.PRIVATE:
						//
						// Both are private and share same parent
						//
						if (al == Modifiers.PRIVATE) {
							var decl = mc.Parent;
							do {
								same_access_restrictions = decl.CurrentType == p_parent;
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

			caching_flags &= ~Flags.ClsCompliance_Undetected;

			if (HasClsCompliantAttribute) {
				if ((caching_flags & Flags.ClsCompliantAttributeFalse) != 0)
					return false;

				caching_flags |= Flags.ClsCompliant;
				return true;
			}

			if (Parent.PartialContainer.IsClsComplianceRequired ()) {
				caching_flags |= Flags.ClsCompliant;
				return true;
			}

			return false;
		}

		public virtual string[] ConditionalConditions ()
		{
			return null;
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

		public virtual IList<MethodSpec> LookupExtensionMethod (TypeSpec extensionType, string name, int arity, ref NamespaceEntry scope)
		{
			return Parent.LookupExtensionMethod (extensionType, name, arity, ref scope);
		}

		public virtual FullNamedExpression LookupNamespaceAlias (string name)
		{
			return Parent.NamespaceEntry.LookupNamespaceAlias (name);
		}

		public virtual FullNamedExpression LookupNamespaceOrType (string name, int arity, Location loc, bool ignore_cs0104)
		{
			return Parent.LookupNamespaceOrType (name, arity, loc, ignore_cs0104);
		}

		/// <summary>
		/// Goes through class hierarchy and gets value of first found CLSCompliantAttribute.
		/// If no is attribute exists then assembly CLSCompliantAttribute is returned.
		/// </summary>
		public bool IsNotCLSCompliant ()
		{
			if ((caching_flags & Flags.HasCompliantAttribute_Undetected) == 0)
				return (caching_flags & Flags.ClsCompliantAttributeFalse) != 0;

			caching_flags &= ~Flags.HasCompliantAttribute_Undetected;

			if (OptAttributes != null) {
				Attribute cls_attribute = OptAttributes.Search (Compiler.PredefinedAttributes.CLSCompliant);
				if (cls_attribute != null) {
					caching_flags |= Flags.HasClsCompliantAttribute;
					if (cls_attribute.GetClsCompliantAttributeValue ())
						return false;

					caching_flags |= Flags.ClsCompliantAttributeFalse;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Returns true if MemberCore is explicitly marked with CLSCompliantAttribute
		/// </summary>
		protected bool HasClsCompliantAttribute {
			get {
				if ((caching_flags & Flags.HasCompliantAttribute_Undetected) != 0)
					IsNotCLSCompliant ();
				
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
			if (HasClsCompliantAttribute) {
				if (CodeGen.Assembly.ClsCompliantAttribute == null) {
					Attribute a = OptAttributes.Search (Compiler.PredefinedAttributes.CLSCompliant);
					if ((caching_flags & Flags.ClsCompliantAttributeFalse) != 0) {
						Report.Warning (3021, 2, a.Location,
							"`{0}' does not need a CLSCompliant attribute because the assembly is not marked as CLS-compliant",
							GetSignatureForError ());
					} else {
						Report.Warning (3014, 1, a.Location,
							"`{0}' cannot be marked as CLS-compliant because the assembly is not marked as CLS-compliant",
							GetSignatureForError ());
					}
					return false;
				}

				if (!IsExposedFromAssembly ()) {
					Attribute a = OptAttributes.Search (Compiler.PredefinedAttributes.CLSCompliant);
					Report.Warning (3019, 2, a.Location, "CLS compliance checking will not be performed on `{0}' because it is not visible from outside this assembly", GetSignatureForError ());
					return false;
				}

				if ((caching_flags & Flags.ClsCompliantAttributeFalse) != 0) {
					if (Parent.Kind == MemberKind.Interface && Parent.IsClsComplianceRequired ()) {
						Report.Warning (3010, 1, Location, "`{0}': CLS-compliant interfaces must have only CLS-compliant members", GetSignatureForError ());
					} else if (Parent.Kind == MemberKind.Class && (ModFlags & Modifiers.ABSTRACT) != 0 && Parent.IsClsComplianceRequired ()) {
						Report.Warning (3011, 1, Location, "`{0}': only CLS-compliant members can be abstract", GetSignatureForError ());
					}

					return false;
				}

				if (Parent.Parent != null && !Parent.IsClsComplianceRequired ()) {
					Attribute a = OptAttributes.Search (Compiler.PredefinedAttributes.CLSCompliant);
					Report.Warning (3018, 1, a.Location, "`{0}' cannot be marked as CLS-compliant because it is a member of non CLS-compliant type `{1}'",
						GetSignatureForError (), Parent.GetSignatureForError ());
					return false;
				}
			} else {
				if (!IsExposedFromAssembly ())
					return false;

				if (!Parent.PartialContainer.IsClsComplianceRequired ())
					return false;
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
			get { return Parent.Compiler; }
		}

		public virtual TypeSpec CurrentType {
			get { return Parent.CurrentType; }
		}

		public MemberCore CurrentMemberDefinition {
			get { return this; }
		}

		public virtual TypeParameter[] CurrentTypeParameters {
			get { return null; }
		}

		public virtual bool HasUnresolvedConstraints {
			get { return false; }
		}

		public bool IsObsolete {
			get {
				if (GetAttributeObsolete () != null)
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
			get {
				return (ModFlags & Modifiers.STATIC) != 0;
			}
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
			Obsolete = 1 << 1,			// Member has obsolete attribute
			CLSCompliant_Undetected = 1 << 3,	// CLSCompliant attribute has not been detected yet
			CLSCompliant = 1 << 4,		// Member is CLS Compliant

			HasDynamicElement = 1 << 8,
			IsAccessor = 1 << 9,		// Method is an accessor
			IsGeneric = 1 << 10,		// Member contains type arguments

			PendingMetaInflate = 1 << 12,
			PendingMakeMethod = 1 << 13,
			PendingMemberCacheMembers = 1 << 14,
			PendingBaseTypeInflate = 1 << 15,
			InterfacesExpanded = 1 << 16,
			IsNotRealProperty = 1 << 17,
		}

		protected Modifiers modifiers;
		protected StateFlags state;
		protected IMemberDefinition definition;
		public readonly MemberKind Kind;
		protected TypeSpec declaringType;

#if DEBUG
		static int counter;
		public int ID = counter++;
#endif

		protected MemberSpec (MemberKind kind, TypeSpec declaringType, IMemberDefinition definition, Modifiers modifiers)
		{
			this.Kind = kind;
			this.declaringType = declaringType;
			this.definition = definition;
			this.modifiers = modifiers;

			state = StateFlags.Obsolete_Undetected | StateFlags.CLSCompliant_Undetected;
		}

		#region Properties

		public Assembly Assembly {
			get {
				return definition.Assembly;
			}
		}

		public virtual int Arity {
			get {
				return 0;
			}
		}

		public TypeSpec DeclaringType {
			get {
				return declaringType;
			}
			set {
				declaringType = value;
			}
		}

		public IMemberDefinition MemberDefinition {
			get {
				return definition;
			}
		}

		public Modifiers Modifiers {
			get {
				return modifiers;
			}
			set {
				modifiers = value;
			}
		}
		
		public virtual string Name {
			get {
				return definition.Name;
			}
		}

		public bool IsAbstract {
			get { return (modifiers & Modifiers.ABSTRACT) != 0; }
		}

		public bool IsAccessor {
			get {
				return (state & StateFlags.IsAccessor) != 0;
			}
			set {
				state = value ? state | StateFlags.IsAccessor : state & ~StateFlags.IsAccessor;
			}
		}

		//
		// Return true when this member is a generic in C# terms
		// A nested non-generic type of generic type will return false
		//
		public bool IsGeneric {
			get {
				return (state & StateFlags.IsGeneric) != 0;
			}
			set {
				state = value ? state | StateFlags.IsGeneric : state & ~StateFlags.IsGeneric;
			}
		}

		public bool IsPrivate {
			get { return (modifiers & Modifiers.PRIVATE) != 0; }
		}

		public bool IsPublic {
			get { return (modifiers & Modifiers.PUBLIC) != 0; }
		}

		public bool IsStatic {
			get { 
				return (modifiers & Modifiers.STATIC) != 0;
			}
		}

		#endregion

		public virtual ObsoleteAttribute GetAttributeObsolete ()
		{
			if ((state & (StateFlags.Obsolete | StateFlags.Obsolete_Undetected)) == 0)
				return null;

			state &= ~StateFlags.Obsolete_Undetected;

			var oa = definition.GetAttributeObsolete ();
			if (oa != null)
				state |= StateFlags.Obsolete;

			return oa;
		}

		protected virtual bool IsNotCLSCompliant ()
		{
			return MemberDefinition.IsNotCLSCompliant ();
		}

		public virtual string GetSignatureForError ()
		{
			var bf = MemberDefinition as Property.BackingField;
			var name = bf == null ? Name : bf.OriginalName;
			return DeclaringType.GetSignatureForError () + "." + name;
		}

		public virtual MemberSpec InflateMember (TypeParameterInflator inflator)
		{
			var inflated = (MemberSpec) MemberwiseClone ();
			inflated.declaringType = inflator.TypeInstance;
			inflated.state |= StateFlags.PendingMetaInflate;
#if DEBUG
			if (inflated.ID > 0)
				inflated.ID = -inflated.ID;
#endif
			return inflated;
		}

		//
		// Is this member accessible from invocationType
		//
		public bool IsAccessible (TypeSpec invocationType)
		{
			var ma = Modifiers & Modifiers.AccessibilityMask;
			if (ma == Modifiers.PUBLIC)
				return true;

			var parentType = /* this as TypeSpec ?? */ DeclaringType;
		
			//
			// If only accessible to the current class or children
			//
			if (ma == Modifiers.PRIVATE)
				return invocationType.MemberDefinition == parentType.MemberDefinition ||
					TypeManager.IsNestedChildOf (invocationType, parentType);

			if ((ma & Modifiers.INTERNAL) != 0) {
				var b = TypeManager.IsThisOrFriendAssembly (invocationType == InternalType.FakeInternalType ?
					 CodeGen.Assembly.Builder : invocationType.Assembly, Assembly);
				if (b || ma == Modifiers.INTERNAL)
					return b;
			}

			// PROTECTED
			if (!TypeManager.IsNestedFamilyAccessible (invocationType, parentType))
				return false;

			return true;
		}

		//
		// Returns member CLS compliance based on full member hierarchy
		//
		public bool IsCLSCompliant ()
		{
			if ((state & StateFlags.CLSCompliant_Undetected) != 0) {
				state &= ~StateFlags.CLSCompliant_Undetected;

				if (IsNotCLSCompliant ())
					return false;

				bool compliant;
				if (DeclaringType != null) {
					compliant = DeclaringType.IsCLSCompliant ();
				} else {
					// TODO: NEED AssemblySpec
					if (MemberDefinition.IsImported) {
						var attr = MemberDefinition.Assembly.GetCustomAttributes (typeof (CLSCompliantAttribute), false);
						compliant = attr.Length > 0 && ((CLSCompliantAttribute) attr[0]).IsCompliant;
					} else {
						compliant = CodeGen.Assembly.IsClsCompliant;
					}
				}

				if (compliant)
					state |= StateFlags.CLSCompliant;
			}

			return (state & StateFlags.CLSCompliant) != 0;
		}

		public bool IsConditionallyExcluded (Location loc)
		{
			if ((Kind & (MemberKind.Class | MemberKind.Method)) == 0)
				return false;

			var conditions = MemberDefinition.ConditionalConditions ();
			if (conditions == null)
				return false;

			foreach (var condition in conditions) {
				if (loc.CompilationUnit.IsConditionalDefined (condition))
					return false;
			}

			return true;
		}

		public override string ToString ()
		{
			return GetSignatureForError ();
		}
	}

	//
	// Member details which are same between all member
	// specifications
	//
	public interface IMemberDefinition
	{
		Assembly Assembly { get; }
		string Name { get; }
		bool IsImported { get; }

		string[] ConditionalConditions ();
		ObsoleteAttribute GetAttributeObsolete ();
		bool IsNotCLSCompliant ();
		void SetIsAssigned ();
		void SetIsUsed ();
	}

	public interface IParametersMember : IInterfaceMemberSpec
	{
		AParametersCollection Parameters { get; }
	}

	public interface IInterfaceMemberSpec
	{
		TypeSpec MemberType { get; }
	}

	//
	// Base type container declaration. It exists to handle partial types
	// which share same definition (PartialContainer) but have different
	// resolve scopes
	//
	public abstract class DeclSpace : MemberCore {
		/// <summary>
		///   This points to the actual definition that is being
		///   created with System.Reflection.Emit
		/// </summary>
		public TypeBuilder TypeBuilder;

		//
		// This is the namespace in which this typecontainer
		// was declared.  We use this to resolve names.
		//
		public NamespaceEntry NamespaceEntry;

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

		public override string GetSignatureForError ()
		{
			return MemberName.GetSignatureForError ();
		}
		
		public bool CheckAccessLevel (TypeSpec check_type)
		{
// TODO: Use this instead
//			return PartialContainer.Definition.IsAccessible (check_type);

			TypeSpec tb = PartialContainer.Definition;
			check_type = check_type.GetDefinition ();

			var check_attr = check_type.Modifiers & Modifiers.AccessibilityMask;

			switch (check_attr){
			case Modifiers.PUBLIC:
				return true;

			case Modifiers.INTERNAL:
				return TypeManager.IsThisOrFriendAssembly (Assembly, check_type.Assembly);
				
			case Modifiers.PRIVATE:
				TypeSpec declaring = check_type.DeclaringType;
				return tb == declaring.GetDefinition () || TypeManager.IsNestedChildOf (tb, declaring);	

			case Modifiers.PROTECTED:
				//
				// Only accessible to methods in current type or any subtypes
				//
				return TypeManager.IsNestedFamilyAccessible (tb, check_type.DeclaringType);

			case Modifiers.PROTECTED | Modifiers.INTERNAL:
				if (TypeManager.IsThisOrFriendAssembly (Assembly, check_type.Assembly))
					return true;

				goto case Modifiers.PROTECTED;
			}

			throw new NotImplementedException (check_attr.ToString ());
		}

		public override Assembly Assembly {
			get { return Module.Assembly; }
		}

		public virtual ModuleContainer Module {
			get { return Parent.Module; }
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
					Parent, i, new MemberName (name.Name, Location), constraints, name.OptAttributes, variance);

				AddToContainer (type_params [i], name.Name);
			}

			if (constraints_list != null && constraints_list.Count > 0) {
				foreach (Constraints constraint in constraints_list) {
					Report.Error(699, constraint.Location, "`{0}': A constraint references nonexistent type parameter `{1}'", 
						GetSignatureForError (), constraint.TypeParameter.Value);
				}
			}
		}

		protected TypeParameter[] TypeParameters {
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

		public int CountTypeParameters {
			get {
				return count_type_params;
			}
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
					tp.VerifyClsCompliance ();
				}
			}

			return true;
		}
	}
}
