//
// decl.cs: Declaration base class for structs, classes, enums and interfaces.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//         Marek Safar (marek.safar@seznam.cz)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
// TODO: Move the method verification stuff from the class.cs and interface.cs here
//

using System;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Reflection.Emit;
using System.Reflection;

namespace Mono.CSharp {

	public class MemberName {
		public string Name;
		public readonly TypeArguments TypeArguments;

		public readonly MemberName Left;

		public static readonly MemberName Null = new MemberName ("");

		public MemberName (string name)
		{
			this.Name = name;
		}

		public MemberName (string name, TypeArguments args)
			: this (name)
		{
			this.TypeArguments = args;
		}

		public MemberName (MemberName left, string name, TypeArguments args)
			: this (name, args)
		{
			this.Left = left;
		}

		public MemberName (MemberName left, MemberName right)
			: this (left, right.Name, right.TypeArguments)
		{
		}

		public string GetName ()
		{
			if (Left != null)
				return Left.GetName () + "." + Name;
			else
				return Name;
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
				return Left.GetName (is_generic) + "." + name;
			else
				return name;
		}

		public int CountTypeArguments {
			get {
				if (TypeArguments == null)
					return 0;
				else
					return TypeArguments.Count;
			}
		}

		public string GetMethodName ()
		{
			if (Left != null)
				return Left.GetFullName () + "." + Name;
			else
				return Name;
		}

		public string GetFullName ()
		{
			string full_name;
			if (TypeArguments != null)
				full_name = Name + "<" + TypeArguments + ">";
			else
				full_name = Name;
			if (Left != null)
				return Left.GetFullName () + "." + full_name;
			else
				return full_name;
		}

		public static string MakeName (string name, TypeArguments args)
		{
			if (args == null)
				return name;
			else
				return name + "`" + args.Count;
		}

		public static string MakeName (string name, int count)
		{
			return name + "`" + count;
		}

		public string GetTypeName ()
		{
			string suffix = "";
			if (Left != null)
				return Left.GetTypeName () + "." +
					MakeName (Name, TypeArguments);
			else
				return MakeName (Name, TypeArguments);
		}

		protected bool IsUnbound {
			get {
				if ((Left != null) && Left.IsUnbound)
					return true;
				else if (TypeArguments == null)
					return false;
				else
					return TypeArguments.IsUnbound;
			}
		}

		protected bool CheckUnbound (Location loc)
		{
			if ((Left != null) && !Left.CheckUnbound (loc))
				return false;
			if ((TypeArguments != null) && !TypeArguments.IsUnbound) {
				Report.Error (1031, loc, "Type expected");
				return false;
			}

			return true;
		}

		public Expression GetTypeExpression (Location loc)
		{
			if (IsUnbound) {
				if (!CheckUnbound (loc))
					return null;

				return new UnboundTypeExpression (GetTypeName ());
			}

			if (Left != null) {
				Expression lexpr = Left.GetTypeExpression (loc);

				return new MemberAccess (lexpr, Name, TypeArguments, loc);
			} else {
				if (TypeArguments != null)
					return new ConstructedType (Name, TypeArguments, loc);
				else
					return new SimpleName (Name, loc);
			}
		}

		public MemberName Clone ()
		{
			if (Left != null)
				return new MemberName (Left.Clone (), Name, TypeArguments);
			else
				return new MemberName (Name, TypeArguments);
		}

		public string Basename {
			get {
				if (TypeArguments != null)
					return MakeName (Name, TypeArguments);
				else
					return Name;
			}
		}

		public override string ToString ()
		{
			string full_name;
			if (TypeArguments != null)
				full_name = Name + "<" + TypeArguments + ">";
			else
				full_name = Name;

			if (Left != null)
				return Left + "." + full_name;
			else
				return full_name;
		}
	}

	/// <summary>
	///   Base representation for members.  This is used to keep track
	///   of Name, Location and Modifier flags, and handling Attributes.
	/// </summary>
	public abstract class MemberCore : Attributable {
		/// <summary>
		///   Public name
		/// </summary>
		public string Name {
			get {
				return MemberName.GetName (!(this is GenericMethod) && !(this is Method));
			}
		}

		public readonly MemberName MemberName;

		/// <summary>
		///   Modifier flags that the user specified in the source code
		/// </summary>
		public int ModFlags;

		public readonly TypeContainer Parent;

		/// <summary>
		///   Location where this declaration happens
		/// </summary>
		public readonly Location Location;

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
			TestMethodDuplication = 1 << 10		// Test for duplication must be performed
		}
  
		/// <summary>
		///   MemberCore flags at first detected then cached
 		/// </summary>
		internal Flags caching_flags;

		public MemberCore (TypeContainer parent, MemberName name, Attributes attrs,
				   Location loc)
			: base (attrs)
		{
			Parent = parent;
			MemberName = name;
			Location = loc;
			caching_flags = Flags.Obsolete_Undetected | Flags.ClsCompliance_Undetected | Flags.HasCompliantAttribute_Undetected | Flags.Excluded_Undetected;
		}

		/// <summary>
		/// Tests presence of ObsoleteAttribute and report proper error
		/// </summary>
		protected void CheckUsageOfObsoleteAttribute (Type type)
		{
			if (type == null)
				return;

			ObsoleteAttribute obsolete_attr = AttributeTester.GetObsoleteAttribute (type);
			if (obsolete_attr == null)
				return;

			AttributeTester.Report_ObsoleteMessage (obsolete_attr, type.FullName, Location);
		}

		public abstract bool Define ();

		// 
		// Returns full member name for error message
		//
		public virtual string GetSignatureForError ()
		{
			return Name;
		}

		/// <summary>
		/// Use this method when MethodBuilder is null
		/// </summary>
		public virtual string GetSignatureForError (TypeContainer tc)
		{
			return Name;
		}

		/// <summary>
		/// Base Emit method. This is also entry point for CLS-Compliant verification.
		/// </summary>
		public virtual void Emit ()
		{
			VerifyObsoleteAttribute ();

			if (!RootContext.VerifyClsCompliance)
				return;

			VerifyClsCompliance (Parent);
		}

		public bool InUnsafe {
			get {
				return ((ModFlags & Modifiers.UNSAFE) != 0) || Parent.UnsafeContext;
			}
		}

		// 
		// Whehter is it ok to use an unsafe pointer in this type container
		//
		public bool UnsafeOK (DeclSpace parent)
		{
			//
			// First check if this MemberCore modifier flags has unsafe set
			//
			if ((ModFlags & Modifiers.UNSAFE) != 0)
				return true;

			if (parent.UnsafeContext)
				return true;

			Expression.UnsafeError (Location);
			return false;
		}

		/// <summary>
		/// Returns instance of ObsoleteAttribute for this MemberCore
		/// </summary>
		public ObsoleteAttribute GetObsoleteAttribute (DeclSpace ds)
		{
			// ((flags & (Flags.Obsolete_Undetected | Flags.Obsolete)) == 0) is slower, but why ?
			if ((caching_flags & Flags.Obsolete_Undetected) == 0 && (caching_flags & Flags.Obsolete) == 0) {
				return null;
			}

			caching_flags &= ~Flags.Obsolete_Undetected;

			if (OptAttributes == null)
				return null;

			Attribute obsolete_attr = OptAttributes.Search (
				TypeManager.obsolete_attribute_type, ds.EmitContext);
			if (obsolete_attr == null)
				return null;

			ObsoleteAttribute obsolete = obsolete_attr.GetObsoleteAttribute (ds);
			if (obsolete == null)
				return null;

			caching_flags |= Flags.Obsolete;
			return obsolete;
		}

		/// <summary>
		/// Analyze whether CLS-Compliant verification must be execute for this MemberCore.
		/// </summary>
		public override bool IsClsCompliaceRequired (DeclSpace container)
		{
			if ((caching_flags & Flags.ClsCompliance_Undetected) == 0)
				return (caching_flags & Flags.ClsCompliant) != 0;

			if (GetClsCompliantAttributeValue (container) && IsExposedFromAssembly (container)) {
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
		protected bool IsExposedFromAssembly (DeclSpace ds)
		{
			if ((ModFlags & (Modifiers.PUBLIC | Modifiers.PROTECTED)) == 0)
				return false;
			
			DeclSpace parentContainer = ds;
			while (parentContainer != null && parentContainer.ModFlags != 0) {
				if ((parentContainer.ModFlags & (Modifiers.PUBLIC | Modifiers.PROTECTED)) == 0)
					return false;
				parentContainer = parentContainer.Parent;
			}
			return true;
		}

		/// <summary>
		/// Resolve CLSCompliantAttribute value or gets cached value.
		/// </summary>
		bool GetClsCompliantAttributeValue (DeclSpace ds)
		{
			if (OptAttributes != null) {
				Attribute cls_attribute = OptAttributes.GetClsCompliantAttribute (ds.EmitContext);
				if (cls_attribute != null) {
					caching_flags |= Flags.HasClsCompliantAttribute;
					return cls_attribute.GetClsCompliantAttributeValue (ds);
				}
			}
			return ds.GetClsCompliantAttributeValue ();
		}

		/// <summary>
		/// Returns true if MemberCore is explicitly marked with CLSCompliantAttribute
		/// </summary>
		protected bool HasClsCompliantAttribute {
			get {
				return (caching_flags & Flags.HasClsCompliantAttribute) != 0;
			}
		}

		/// <summary>
		/// The main virtual method for CLS-Compliant verifications.
		/// The method returns true if member is CLS-Compliant and false if member is not
		/// CLS-Compliant which means that CLS-Compliant tests are not necessary. A descendants override it
		/// and add their extra verifications.
		/// </summary>
		protected virtual bool VerifyClsCompliance (DeclSpace ds)
		{
			if (!IsClsCompliaceRequired (ds)) {
				if ((RootContext.WarningLevel >= 2) && HasClsCompliantAttribute && !IsExposedFromAssembly (ds)) {
					Report.Warning (3019, Location, "CLS compliance checking will not be performed on '{0}' because it is private or internal", GetSignatureForError ());
				}
				return false;
			}

			if (!CodeGen.Assembly.IsClsCompliant) {
				if (HasClsCompliantAttribute) {
					Report.Error (3014, Location, "'{0}' cannot be marked as CLS-compliant because the assembly does not have a CLSCompliant attribute", GetSignatureForError ());
				}
				return false;
			}

			int index = Name.LastIndexOf ('.');
			if (Name [index > 0 ? index + 1 : 0] == '_') {
				Report.Error (3008, Location, "Identifier '{0}' is not CLS-compliant", GetSignatureForError () );
			}
			return true;
		}

		protected abstract void VerifyObsoleteAttribute ();

	}

	/// <summary>
	///   Base class for structs, classes, enumerations and interfaces.  
	/// </summary>
	/// <remarks>
	///   They all create new declaration spaces.  This
	///   provides the common foundation for managing those name
	///   spaces.
	/// </remarks>
	public abstract class DeclSpace : MemberCore, IAlias {
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
		public TypeExpr CurrentType;

		// The emit context for toplevel objects.
		protected EmitContext ec;

		//
		// This is the namespace in which this typecontainer
		// was declared.  We use this to resolve names.
		//
		public NamespaceEntry NamespaceEntry;

		public Hashtable Cache = new Hashtable ();
		
		public string Basename;
		
		protected Hashtable defined_names;

		readonly bool is_generic;
		readonly int count_type_params;

		// The emit context for toplevel objects.
		protected EmitContext ec;
		
		public EmitContext EmitContext {
			get { return ec; }
		}

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

		public DeclSpace (NamespaceEntry ns, TypeContainer parent, MemberName name,
				  Attributes attrs, Location l)
			: base (parent, name, attrs, l)
		{
			NamespaceEntry = ns;
			Basename = name.Name;
			defined_names = new Hashtable ();
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
		protected bool AddToContainer (MemberCore symbol, bool is_method, string fullname, string basename)
		{
			if (basename == Basename) {
				Report.SymbolRelatedToPreviousError (this);
				Report.Error (542, "'{0}': member names cannot be the same as their enclosing type", symbol.Location, symbol.GetSignatureForError ());
				return false;
			}

			MemberCore mc = (MemberCore)defined_names [fullname];

			if (is_method && (mc is MethodCore || mc is IMethodData)) {
				symbol.caching_flags |= Flags.TestMethodDuplication;
				mc.caching_flags |= Flags.TestMethodDuplication;
				return true;
			}

			if (mc != null) {
				Report.SymbolRelatedToPreviousError (mc);
				Report.Error (102, symbol.Location, "The type '{0}' already contains a definition for '{1}'", GetSignatureForError (), basename);
				return false;
			}

			defined_names.Add (fullname, symbol);
			return true;
		}

		public void RecordDecl ()
		{
			if ((NamespaceEntry != null) && (Parent == RootContext.Tree.Types))
				NamespaceEntry.DefineName (MemberName.Basename, this);
		}

		/// <summary>
		///   Returns the MemberCore associated with a given name in the declaration
		///   space. It doesn't return method based symbols !!
		/// </summary>
		/// 
		public MemberCore GetDefinition (string name)
		{
			return (MemberCore)defined_names [name];
		}

		bool in_transit = false;
		
		/// <summary>
		///   This function is used to catch recursive definitions
		///   in declarations.
		/// </summary>
		public bool InTransit {
			get {
				return in_transit;
			}

			set {
				in_transit = value;
			}
		}

		public EmitContext EmitContext {
			get {
				return ec;
			}
		}

		/// <summary>
		///   Looks up the alias for the name
		/// </summary>
		public IAlias LookupAlias (string name)
		{
			if (NamespaceEntry != null)
				return NamespaceEntry.LookupAlias (name);
			else
				return null;
		}
		
		// 
		// root_types contains all the types.  All TopLevel types
		// hence have a parent that points to `root_types', that is
		// why there is a non-obvious test down here.
		//
		public bool IsTopLevel {
			get {
				if (Parent != null){
					if (Parent.Parent == null)
						return true;
				}
				return false;
			}
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

		/// <remarks>
		///  Should be overriten by the appropriate declaration space
		/// </remarks>
		public abstract TypeBuilder DefineType ();
		
		/// <summary>
		///   Define all members, but don't apply any attributes or do anything which may
		///   access not-yet-defined classes.  This method also creates the MemberCache.
		/// </summary>
		public abstract bool DefineMembers (TypeContainer parent);

		//
		// Whether this is an `unsafe context'
		//
		public bool UnsafeContext {
			get {
				if ((ModFlags & Modifiers.UNSAFE) != 0)
					return true;
				if (Parent != null)
					return Parent.UnsafeContext;
				return false;
			}
		}

		public static string MakeFQN (string nsn, string name)
		{
			if (nsn == "")
				return name;
			return String.Concat (nsn, ".", name);
		}

		EmitContext type_resolve_ec;
		EmitContext GetTypeResolveEmitContext (TypeContainer parent, Location loc)
		{
			type_resolve_ec = new EmitContext (parent, this, loc, null, null, ModFlags, false);
			type_resolve_ec.ResolvingTypeTree = true;

			return type_resolve_ec;
		}

		// <summary>
		//    Looks up the type, as parsed into the expression `e'.
		// </summary>
		[Obsolete ("This method is going away soon")]
		public Type ResolveType (Expression e, bool silent, Location loc)
		{
			TypeExpr d = ResolveTypeExpr (e, silent, loc);
			return d == null ? null : d.Type;
		}

		public Type ResolveNestedType (Type t, Location loc)
		{
			TypeContainer tc = TypeManager.LookupTypeContainer (t);
			if ((tc != null) && tc.IsGeneric) {
				if (!IsGeneric) {
					int tnum = TypeManager.GetNumberOfTypeArguments (t);
					Report.Error (305, loc,
						      "Using the generic type `{0}' " +
						      "requires {1} type arguments",
						      TypeManager.GetFullName (t), tnum);
					return null;
				}

				TypeParameter[] args;
				if (this is GenericMethod)
					args = Parent.TypeParameters;
				else
					args = TypeParameters;

				ConstructedType ctype = new ConstructedType (t, args, loc);
				t = ctype.ResolveType (ec);
			}

			return t;
		}

		// <summary>
		//    Resolves the expression `e' for a type, and will recursively define
		//    types.  This should only be used for resolving base types.
		// </summary>
		public TypeExpr ResolveTypeExpr (Expression e, bool silent, Location loc)
		{
			if (type_resolve_ec == null)
				type_resolve_ec = GetTypeResolveEmitContext (Parent, loc);
			type_resolve_ec.loc = loc;
			if (this is GenericMethod)
				type_resolve_ec.ContainerType = Parent.TypeBuilder;
			else
				type_resolve_ec.ContainerType = TypeBuilder;

			return e.ResolveAsTypeTerminal (type_resolve_ec, silent);
		}
		
		public bool CheckAccessLevel (Type check_type) 
		{
			TypeBuilder tb;
			if ((this is GenericMethod) || (this is Iterator))
				tb = Parent.TypeBuilder;
			else
				tb = TypeBuilder;

			if (check_type.IsGenericInstance)
				check_type = check_type.GetGenericTypeDefinition ();

			if (check_type == tb)
				return true;

			if (check_type.IsGenericParameter)
				return true; // FIXME
			
			TypeAttributes check_attr = check_type.Attributes & TypeAttributes.VisibilityMask;

			//
			// Broken Microsoft runtime, return public for arrays, no matter what 
			// the accessibility is for their underlying class, and they return 
			// NonPublic visibility for pointers
			//
			if (check_type.IsArray || check_type.IsPointer)
				return CheckAccessLevel (TypeManager.GetElementType (check_type));

			switch (check_attr){
			case TypeAttributes.Public:
				return true;

			case TypeAttributes.NotPublic:

				if (TypeBuilder == null)
					// FIXME: TypeBuilder will be null when invoked by Class.GetNormalBases().
					//        However, this is invoked again later -- so safe to return true.
					//        May also be null when resolving top-level attributes.
					return true;
				//
				// This test should probably use the declaringtype.
				//
				return check_type.Assembly == TypeBuilder.Assembly;

			case TypeAttributes.NestedPublic:
				return true;

			case TypeAttributes.NestedPrivate:
				return NestedAccessible (tb, check_type);

			case TypeAttributes.NestedFamily:
				//
				// Only accessible to methods in current type or any subtypes
				//
				return FamilyAccessible (check_type);

			case TypeAttributes.NestedFamANDAssem:
				return (check_type.Assembly == tb.Assembly) &&
					FamilyAccessible (check_type);

			case TypeAttributes.NestedFamORAssem:
				return (check_type.Assembly == tb.Assembly) ||
					FamilyAccessible (check_type);

			case TypeAttributes.NestedAssembly:
				return check_type.Assembly == tb.Assembly;
			}

			Console.WriteLine ("HERE: " + check_attr);
			return false;

		}

		protected bool NestedAccessible (Type tb, Type check_type)
		{
			string check_type_name = check_type.FullName;
			
			// At this point, we already know check_type is a nested class.
			int cio = check_type_name.LastIndexOf ('+');
			
			// Ensure that the string 'container' has a '+' in it to avoid false matches
			string container = check_type_name.Substring (0, cio + 1);

			// Ensure that type_name ends with a '+' so that it can match 'container', if necessary
			string type_name = tb.FullName + "+";

			// If the current class is nested inside the container of check_type,
			// we can access check_type even if it is private or protected.
			return type_name.StartsWith (container);
		}

		protected bool FamilyAccessible (Type tb, Type check_type)
		{
			Type declaring = check_type.DeclaringType;
			if (tb == declaring || TypeManager.IsFamilyAccessible (tb, declaring))
				return true;

			return NestedAccessible (tb, check_type);
		}

		// Access level of a type.
		const int X = 1;
		enum AccessLevel { // Each column represents `is this scope larger or equal to Blah scope'
			// Public    Assembly   Protected
			Protected           = (0 << 0) | (0 << 1) | (X << 2),
			Public              = (X << 0) | (X << 1) | (X << 2),
			Private             = (0 << 0) | (0 << 1) | (0 << 2),
			Internal            = (0 << 0) | (X << 1) | (0 << 2),
			ProtectedOrInternal = (0 << 0) | (X << 1) | (X << 2),
		}

		static AccessLevel GetAccessLevelFromModifiers (int flags)
		{
			if ((flags & Modifiers.INTERNAL) != 0) {

				if ((flags & Modifiers.PROTECTED) != 0)
					return AccessLevel.ProtectedOrInternal;
				else
					return AccessLevel.Internal;

			} else if ((flags & Modifiers.PROTECTED) != 0)
				return AccessLevel.Protected;
			else if ((flags & Modifiers.PRIVATE) != 0)
				return AccessLevel.Private;
			else
				return AccessLevel.Public;
		}

		// What is the effective access level of this?
		// TODO: Cache this?
		AccessLevel EffectiveAccessLevel {
			get {
				AccessLevel myAccess = GetAccessLevelFromModifiers (ModFlags);
				if (!IsTopLevel && (Parent != null))
					return myAccess & Parent.EffectiveAccessLevel;
				return myAccess;
			}
		}

		// Return the access level for type `t'
		static AccessLevel TypeEffectiveAccessLevel (Type t)
		{
			if (t.IsPublic)
				return AccessLevel.Public;
			if (t.IsNestedPrivate)
				return AccessLevel.Private;
			if (t.IsNotPublic)
				return AccessLevel.Internal;

			// By now, it must be nested
			AccessLevel parentLevel = TypeEffectiveAccessLevel (t.DeclaringType);

 			if (t.IsNestedPublic)
				return parentLevel;
			if (t.IsNestedAssembly)
				return parentLevel & AccessLevel.Internal;
			if (t.IsNestedFamily)
				return parentLevel & AccessLevel.Protected;
			if (t.IsNestedFamORAssem)
				return parentLevel & AccessLevel.ProtectedOrInternal;
			if (t.IsNestedFamANDAssem)
				throw new NotImplementedException ("NestedFamANDAssem not implemented, cant make this kind of type from c# anyways");

			// nested private is taken care of

			throw new Exception ("I give up, what are you?");
		}

		//
		// This answers `is the type P, as accessible as a member M which has the
		// accessability @flags which is declared as a nested member of the type T, this declspace'
		//
		public bool AsAccessible (Type p, int flags)
		{
			if (p.IsGenericParameter)
				return true; // FIXME

			//
			// 1) if M is private, its accessability is the same as this declspace.
			// we already know that P is accessible to T before this method, so we
			// may return true.
			//

			if ((flags & Modifiers.PRIVATE) != 0)
				return true;

			while (p.IsArray || p.IsPointer || p.IsByRef)
				p = TypeManager.GetElementType (p);

			AccessLevel pAccess = TypeEffectiveAccessLevel (p);
			AccessLevel mAccess = this.EffectiveAccessLevel &
				GetAccessLevelFromModifiers (flags);

			// for every place from which we can access M, we must
			// be able to access P as well. So, we want
			// For every bit in M and P, M_i -> P_1 == true
			// or, ~ (M -> P) == 0 <-> ~ ( ~M | P) == 0

			return ~ (~ mAccess | pAccess) == 0;
		}
		
		static DoubleHash dh = new DoubleHash (1000);

		Type DefineTypeAndParents (DeclSpace tc)
		{
			DeclSpace container = tc.Parent;

			if (container.TypeBuilder == null && container.Name != "")
				DefineTypeAndParents (container);

			return tc.DefineType ();
		}
		
		Type LookupInterfaceOrClass (string ns, string name, out bool error)
		{
			DeclSpace parent;
			Type t;
			object r;
			
			error = false;

			if (dh.Lookup (ns, name, out r))
				return (Type) r;
			else {
				if (ns != ""){
					if (Namespace.IsNamespace (ns)){
						string fullname = (ns != "") ? ns + "." + name : name;
						t = TypeManager.LookupType (fullname);
					} else
						t = null;
				} else
					t = TypeManager.LookupType (name);
			}
			
			if (t != null) {
				dh.Insert (ns, name, t);
				return t;
			}

			//
			// In case we are fed a composite name, normalize it.
			//
			int p = name.LastIndexOf ('.');
			if (p != -1){
				ns = MakeFQN (ns, name.Substring (0, p));
				name = name.Substring (p+1);
			}
			
			parent = RootContext.Tree.LookupByNamespace (ns, name);
			if (parent == null) {
				dh.Insert (ns, name, null);
				return null;
			}

			t = DefineTypeAndParents (parent);
			if (t == null){
				error = true;
				return null;
			}
			
			dh.Insert (ns, name, t);
			return t;
		}

		public static void Error_AmbiguousTypeReference (Location loc, string name, string t1, string t2)
		{
			Report.Error (104, loc,
				      "`{0}' is an ambiguous reference ({1} or {2})",
				      name, t1, t2);
		}

		public Type FindNestedType (Location loc, string name,
					    out DeclSpace containing_ds)
		{
			Type t;
			bool error;

			containing_ds = this;
			while (containing_ds != null){
				Type container_type = containing_ds.TypeBuilder;
				Type current_type = container_type;

				while (current_type != null && current_type != TypeManager.object_type) {
					string pre = current_type.FullName;

					t = LookupInterfaceOrClass (pre, name, out error);
					if (error)
						return null;

					if ((t != null) && containing_ds.CheckAccessLevel (t))
						return t;

					current_type = current_type.BaseType;
				}
				containing_ds = containing_ds.Parent;
			}

			return null;
		}

		/// <summary>
		///   GetType is used to resolve type names at the DeclSpace level.
		///   Use this to lookup class/struct bases, interface bases or 
		///   delegate type references
		/// </summary>
		///
		/// <remarks>
		///   Contrast this to LookupType which is used inside method bodies to 
		///   lookup types that have already been defined.  GetType is used
		///   during the tree resolution process and potentially define
		///   recursively the type
		/// </remarks>
		public Type FindType (Location loc, string name)
		{
			Type t;
			bool error;

			//
			// For the case the type we are looking for is nested within this one
			// or is in any base class
			//
			DeclSpace containing_ds = this;

			while (containing_ds != null){
				Type container_type = containing_ds.TypeBuilder;
				Type current_type = container_type;

				while (current_type != null && current_type != TypeManager.object_type) {
					string pre = current_type.FullName;

					t = LookupInterfaceOrClass (pre, name, out error);
					if (error)
						return null;

					if ((t != null) && containing_ds.CheckAccessLevel (t))
						return ResolveNestedType (t, loc);

					current_type = current_type.BaseType;
				}
				containing_ds = containing_ds.Parent;
			}

			//
			// Attempt to lookup the class on our namespace and all it's implicit parents
			//
			for (NamespaceEntry ns = NamespaceEntry; ns != null; ns = ns.ImplicitParent) {
				t = LookupInterfaceOrClass (ns.FullName, name, out error);
				if (error)
					return null;

				if (t != null)
					return t;
			}
			
			//
			// Attempt to do a direct unqualified lookup
			//
			t = LookupInterfaceOrClass ("", name, out error);
			if (error)
				return null;
			
			if (t != null)
				return t;
			
			//
			// Attempt to lookup the class on any of the `using'
			// namespaces
			//

			for (NamespaceEntry ns = NamespaceEntry; ns != null; ns = ns.Parent){

				t = LookupInterfaceOrClass (ns.FullName, name, out error);
				if (error)
					return null;

				if (t != null)
					return t;

				if (name.IndexOf ('.') > 0)
					continue;

				IAlias alias_value = ns.LookupAlias (name);
				if (alias_value != null) {
					t = LookupInterfaceOrClass ("", alias_value.Name, out error);
					if (error)
						return null;

					if (t != null)
						return t;
				}

				//
				// Now check the using clause list
				//
				Type match = null;
				foreach (Namespace using_ns in ns.GetUsingTable ()) {
					match = LookupInterfaceOrClass (using_ns.Name, name, out error);
					if (error)
						return null;

					if (match != null) {
						if (t != null){
							if (CheckAccessLevel (match)) {
								Error_AmbiguousTypeReference (loc, name, t.FullName, match.FullName);
								return null;
							}
							continue;
						}
						
						t = match;
					}
				}
				if (t != null)
					return t;
			}

			//Report.Error (246, Location, "Can not find type `"+name+"'");
			return null;
		}

		/// <remarks>
		///   This function is broken and not what you're looking for.  It should only
		///   be used while the type is still being created since it doesn't use the cache
		///   and relies on the filter doing the member name check.
		/// </remarks>
		public abstract MemberList FindMembers (MemberTypes mt, BindingFlags bf,
							MemberFilter filter, object criteria);

		/// <remarks>
		///   If we have a MemberCache, return it.  This property may return null if the
		///   class doesn't have a member cache or while it's still being created.
		/// </remarks>
		public abstract MemberCache MemberCache {
			get;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			try {
				TypeBuilder.SetCustomAttribute (cb);
			} catch (System.ArgumentException e) {
				Report.Warning (-21, a.Location,
						"The CharSet named property on StructLayout\n"+
						"\tdoes not work correctly on Microsoft.NET\n"+
						"\tYou might want to remove the CharSet declaration\n"+
						"\tor compile using the Mono runtime instead of the\n"+
						"\tMicrosoft .NET runtime\n"+
						"\tThe runtime gave the error: " + e);
			}
		}

		/// <summary>
		/// Goes through class hierarchy and get value of first CLSCompliantAttribute that found.
		/// If no is attribute exists then return assembly CLSCompliantAttribute.
		/// </summary>
		public bool GetClsCompliantAttributeValue ()
		{
			if ((caching_flags & Flags.HasCompliantAttribute_Undetected) == 0)
				return (caching_flags & Flags.ClsCompliantAttributeTrue) != 0;

			caching_flags &= ~Flags.HasCompliantAttribute_Undetected;

			if (OptAttributes != null) {
				Attribute cls_attribute = OptAttributes.GetClsCompliantAttribute (ec);
				if (cls_attribute != null) {
					caching_flags |= Flags.HasClsCompliantAttribute;
					if (cls_attribute.GetClsCompliantAttributeValue (this)) {
						caching_flags |= Flags.ClsCompliantAttributeTrue;
						return true;
					}
					return false;
				}
			}

			if (Parent == null) {
				if (CodeGen.Assembly.IsClsCompliant) {
					caching_flags |= Flags.ClsCompliantAttributeTrue;
					return true;
				}
				return false;
			}

			if (Parent.GetClsCompliantAttributeValue ()) {
				caching_flags |= Flags.ClsCompliantAttributeTrue;
				return true;
			}
			return false;
		}

		//
		// Extensions for generics
		//
		TypeParameter[] type_params;
		TypeParameter[] type_param_list;

		protected string GetInstantiationName ()
		{
			StringBuilder sb = new StringBuilder (Name);
			sb.Append ("<");
			for (int i = 0; i < type_param_list.Length; i++) {
				if (i > 0)
					sb.Append (",");
				sb.Append (type_param_list [i].Name);
			}
			sb.Append (">");
			return sb.ToString ();
		}

		bool check_type_parameter (ArrayList list, int start, string name)
		{
			for (int i = 0; i < start; i++) {
				TypeParameter param = (TypeParameter) list [i];

				if (param.Name != name)
					continue;

				if (RootContext.WarningLevel >= 3)
					Report.Warning (
						693, Location,
						"Type parameter `{0}' has same name " +
						"as type parameter from outer type `{1}'",
						name, Parent.GetInstantiationName ());

				return false;
			}

			return true;
		}

		TypeParameter[] initialize_type_params ()
		{
			if (type_param_list != null)
				return type_param_list;

			DeclSpace the_parent = Parent;
			if (this is GenericMethod)
				the_parent = null;

			int start = 0;
			TypeParameter[] parent_params = null;
			if ((the_parent != null) && the_parent.IsGeneric) {
				parent_params = the_parent.initialize_type_params ();
				start = parent_params != null ? parent_params.Length : 0;
			}

			ArrayList list = new ArrayList ();
			if (parent_params != null)
				list.AddRange (parent_params);

			int count = type_params != null ? type_params.Length : 0;
			for (int i = 0; i < count; i++) {
				TypeParameter param = type_params [i];
				check_type_parameter (list, start, param.Name);
				list.Add (param);
			}

			type_param_list = new TypeParameter [list.Count];
			list.CopyTo (type_param_list, 0);
			return type_param_list;
		}

		public void SetParameterInfo (ArrayList constraints_list)
		{
			if (!is_generic) {
				if (constraints_list != null) {
					Report.Error (
						80, Location, "Contraints are not allowed " +
						"on non-generic declarations");
				}

				return;
			}

			string[] names = MemberName.TypeArguments.GetDeclarations ();
			type_params = new TypeParameter [names.Length];

			//
			// Register all the names
			//
			for (int i = 0; i < type_params.Length; i++) {
				string name = names [i];

				Constraints constraints = null;
				if (constraints_list != null) {
					foreach (Constraints constraint in constraints_list) {
						if (constraint.TypeParameter == name) {
							constraints = constraint;
							break;
						}
					}
				}

				type_params [i] = new TypeParameter (Parent, name, constraints, Location);

				string full_name = Name + "." + name;
				AddToContainer (type_params [i], false, full_name, name);
			}
		}

		public TypeParameter[] TypeParameters {
			get {
				if (!IsGeneric)
					throw new InvalidOperationException ();
				if (type_param_list == null)
					initialize_type_params ();

				return type_param_list;
			}
		}

		protected TypeParameter[] CurrentTypeParameters {
			get {
				if (!IsGeneric)
					throw new InvalidOperationException ();
				if (type_params != null)
					return type_params;
				else
					return new TypeParameter [0];
			}
		}

		public int CountTypeParameters {
			get {
				return count_type_params;
			}
		}

		public TypeParameterExpr LookupGeneric (string name, Location loc)
		{
			if (!IsGeneric)
				return null;

			foreach (TypeParameter type_param in CurrentTypeParameters) {
				if (type_param.Name != name)
					continue;

				return new TypeParameterExpr (type_param, loc);
			}

			if (Parent != null)
				return Parent.LookupGeneric (name, loc);

			return null;
		}

		bool IAlias.IsType {
			get { return true; }
		}

		string IAlias.Name {
			get { return Name; }
		}

		TypeExpr IAlias.Type
		{
			get {
				if (TypeBuilder == null)
					throw new InvalidOperationException ();

				if (CurrentType != null)
					return CurrentType;

				return new TypeExpression (TypeBuilder, Location);
			}
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}
	}

	/// <summary>
	///   This is a readonly list of MemberInfo's.      
	/// </summary>
	public class MemberList : IList {
		public readonly IList List;
		int count;

		/// <summary>
		///   Create a new MemberList from the given IList.
		/// </summary>
		public MemberList (IList list)
		{
			if (list != null)
				this.List = list;
			else
				this.List = new ArrayList ();
			count = List.Count;
		}

		/// <summary>
		///   Concatenate the ILists `first' and `second' to a new MemberList.
		/// </summary>
		public MemberList (IList first, IList second)
		{
			ArrayList list = new ArrayList ();
			list.AddRange (first);
			list.AddRange (second);
			count = list.Count;
			List = list;
		}

		public static readonly MemberList Empty = new MemberList (new ArrayList ());

		/// <summary>
		///   Cast the MemberList into a MemberInfo[] array.
		/// </summary>
		/// <remarks>
		///   This is an expensive operation, only use it if it's really necessary.
		/// </remarks>
		public static explicit operator MemberInfo [] (MemberList list)
		{
			Timer.StartTimer (TimerType.MiscTimer);
			MemberInfo [] result = new MemberInfo [list.Count];
			list.CopyTo (result, 0);
			Timer.StopTimer (TimerType.MiscTimer);
			return result;
		}

		// ICollection

		public int Count {
			get {
				return count;
			}
		}

		public bool IsSynchronized {
			get {
				return List.IsSynchronized;
			}
		}

		public object SyncRoot {
			get {
				return List.SyncRoot;
			}
		}

		public void CopyTo (Array array, int index)
		{
			List.CopyTo (array, index);
		}

		// IEnumerable

		public IEnumerator GetEnumerator ()
		{
			return List.GetEnumerator ();
		}

		// IList

		public bool IsFixedSize {
			get {
				return true;
			}
		}

		public bool IsReadOnly {
			get {
				return true;
			}
		}

		object IList.this [int index] {
			get {
				return List [index];
			}

			set {
				throw new NotSupportedException ();
			}
		}

		// FIXME: try to find out whether we can avoid the cast in this indexer.
		public MemberInfo this [int index] {
			get {
				return (MemberInfo) List [index];
			}
		}

		public int Add (object value)
		{
			throw new NotSupportedException ();
		}

		public void Clear ()
		{
			throw new NotSupportedException ();
		}

		public bool Contains (object value)
		{
			return List.Contains (value);
		}

		public int IndexOf (object value)
		{
			return List.IndexOf (value);
		}

		public void Insert (int index, object value)
		{
			throw new NotSupportedException ();
		}

		public void Remove (object value)
		{
			throw new NotSupportedException ();
		}

		public void RemoveAt (int index)
		{
			throw new NotSupportedException ();
		}
	}

	/// <summary>
	///   This interface is used to get all members of a class when creating the
	///   member cache.  It must be implemented by all DeclSpace derivatives which
	///   want to support the member cache and by TypeHandle to get caching of
	///   non-dynamic types.
	/// </summary>
	public interface IMemberContainer {
		/// <summary>
		///   The name of the IMemberContainer.  This is only used for
		///   debugging purposes.
		/// </summary>
		string Name {
			get;
		}

		/// <summary>
		///   The type of this IMemberContainer.
		/// </summary>
		Type Type {
			get;
		}

		/// <summary>
		///   Returns the IMemberContainer of the parent class or null if this
		///   is an interface or TypeManger.object_type.
		///   This is used when creating the member cache for a class to get all
		///   members from the parent class.
		/// </summary>
		IMemberContainer ParentContainer {
			get;
		}

		/// <summary>
		///   Whether this is an interface.
		/// </summary>
		bool IsInterface {
			get;
		}

		/// <summary>
		///   Returns all members of this class with the corresponding MemberTypes
		///   and BindingFlags.
		/// </summary>
		/// <remarks>
		///   When implementing this method, make sure not to return any inherited
		///   members and check the MemberTypes and BindingFlags properly.
		///   Unfortunately, System.Reflection is lame and doesn't provide a way to
		///   get the BindingFlags (static/non-static,public/non-public) in the
		///   MemberInfo class, but the cache needs this information.  That's why
		///   this method is called multiple times with different BindingFlags.
		/// </remarks>
		MemberList GetMembers (MemberTypes mt, BindingFlags bf);

		/// <summary>
		///   Return the container's member cache.
		/// </summary>
		MemberCache MemberCache {
			get;
		}
	}

	/// <summary>
	///   The MemberCache is used by dynamic and non-dynamic types to speed up
	///   member lookups.  It has a member name based hash table; it maps each member
	///   name to a list of CacheEntry objects.  Each CacheEntry contains a MemberInfo
	///   and the BindingFlags that were initially used to get it.  The cache contains
	///   all members of the current class and all inherited members.  If this cache is
	///   for an interface types, it also contains all inherited members.
	///
	///   There are two ways to get a MemberCache:
	///   * if this is a dynamic type, lookup the corresponding DeclSpace and then
	///     use the DeclSpace.MemberCache property.
	///   * if this not a dynamic type, call TypeHandle.GetTypeHandle() to get a
	///     TypeHandle instance for the type and then use TypeHandle.MemberCache.
	/// </summary>
	public class MemberCache {
		public readonly IMemberContainer Container;
		protected Hashtable member_hash;
		protected Hashtable method_hash;
		
		/// <summary>
		///   Create a new MemberCache for the given IMemberContainer `container'.
		/// </summary>
		public MemberCache (IMemberContainer container)
		{
			this.Container = container;

			Timer.IncrementCounter (CounterType.MemberCache);
			Timer.StartTimer (TimerType.CacheInit);

			

			// If we have a parent class (we have a parent class unless we're
			// TypeManager.object_type), we deep-copy its MemberCache here.
			if (Container.IsInterface) {
				MemberCache parent;
				
				if (Container.ParentContainer != null)
					parent = Container.ParentContainer.MemberCache;
				else
					parent = TypeHandle.ObjectType.MemberCache;
				member_hash = SetupCacheForInterface (parent);
			} else if (Container.ParentContainer != null)
				member_hash = SetupCache (Container.ParentContainer.MemberCache);
			else
				member_hash = new Hashtable ();

			// If this is neither a dynamic type nor an interface, create a special
			// method cache with all declared and inherited methods.
			Type type = container.Type;
			if (!(type is TypeBuilder) && !type.IsInterface && !type.IsGenericParameter) {
				method_hash = new Hashtable ();
				AddMethods (type);
			}

			// Add all members from the current class.
			AddMembers (Container);

			Timer.StopTimer (TimerType.CacheInit);
		}

		/// <summary>
		///   Bootstrap this member cache by doing a deep-copy of our parent.
		/// </summary>
		Hashtable SetupCache (MemberCache parent)
		{
			Hashtable hash = new Hashtable ();

			IDictionaryEnumerator it = parent.member_hash.GetEnumerator ();
			while (it.MoveNext ()) {
				hash [it.Key] = ((ArrayList) it.Value).Clone ();
                        }
                                
			return hash;
		}


		/// <summary>
		///   Add the contents of `new_hash' to `hash'.
		/// </summary>
		void AddHashtable (Hashtable hash, MemberCache cache)
		{
			Hashtable new_hash = cache.member_hash;
			IDictionaryEnumerator it = new_hash.GetEnumerator ();
			while (it.MoveNext ()) {
				ArrayList list = (ArrayList) hash [it.Key];
				if (list == null)
					hash [it.Key] = list = new ArrayList ();

				foreach (CacheEntry entry in (ArrayList) it.Value) {
					if (entry.Container != cache.Container)
						break;
					list.Add (entry);
				}
			}
		}

		/// <summary>
		///   Bootstrap the member cache for an interface type.
		///   Type.GetMembers() won't return any inherited members for interface types,
		///   so we need to do this manually.  Interfaces also inherit from System.Object.
		/// </summary>
		Hashtable SetupCacheForInterface (MemberCache parent)
		{
			Hashtable hash = SetupCache (parent);
			Type [] ifaces = TypeManager.GetInterfaces (Container.Type);

			foreach (Type itype in ifaces) {
				IMemberContainer iface_container =
					TypeManager.LookupMemberContainer (itype);

				MemberCache iface_cache = iface_container.MemberCache;

				AddHashtable (hash, iface_cache);
			}

			return hash;
		}

		/// <summary>
		///   Add all members from class `container' to the cache.
		/// </summary>
		void AddMembers (IMemberContainer container)
		{
			// We need to call AddMembers() with a single member type at a time
			// to get the member type part of CacheEntry.EntryType right.
			AddMembers (MemberTypes.Constructor, container);
			AddMembers (MemberTypes.Field, container);
			AddMembers (MemberTypes.Method, container);
			AddMembers (MemberTypes.Property, container);
			AddMembers (MemberTypes.Event, container);
			// Nested types are returned by both Static and Instance searches.
			AddMembers (MemberTypes.NestedType,
				    BindingFlags.Static | BindingFlags.Public, container);
			AddMembers (MemberTypes.NestedType,
				    BindingFlags.Static | BindingFlags.NonPublic, container);
		}

		void AddMembers (MemberTypes mt, IMemberContainer container)
		{
			AddMembers (mt, BindingFlags.Static | BindingFlags.Public, container);
			AddMembers (mt, BindingFlags.Static | BindingFlags.NonPublic, container);
			AddMembers (mt, BindingFlags.Instance | BindingFlags.Public, container);
			AddMembers (mt, BindingFlags.Instance | BindingFlags.NonPublic, container);
		}

		/// <summary>
		///   Add all members from class `container' with the requested MemberTypes and
		///   BindingFlags to the cache.  This method is called multiple times with different
		///   MemberTypes and BindingFlags.
		/// </summary>
		void AddMembers (MemberTypes mt, BindingFlags bf, IMemberContainer container)
		{
			MemberList members = container.GetMembers (mt, bf);

			foreach (MemberInfo member in members) {
				string name = member.Name;

				int pos = name.IndexOf ('<');
				if (pos > 0)
					name = name.Substring (0, pos);

				// We use a name-based hash table of ArrayList's.
				ArrayList list = (ArrayList) member_hash [name];
				if (list == null) {
					list = new ArrayList ();
					member_hash.Add (name, list);
				}

				// When this method is called for the current class, the list will
				// already contain all inherited members from our parent classes.
				// We cannot add new members in front of the list since this'd be an
				// expensive operation, that's why the list is sorted in reverse order
				// (ie. members from the current class are coming last).
				list.Add (new CacheEntry (container, member, mt, bf));
			}
		}

		/// <summary>
		///   Add all declared and inherited methods from class `type' to the method cache.
		/// </summary>
		void AddMethods (Type type)
		{
			AddMethods (BindingFlags.Static | BindingFlags.Public |
				    BindingFlags.FlattenHierarchy, type);
			AddMethods (BindingFlags.Static | BindingFlags.NonPublic |
				    BindingFlags.FlattenHierarchy, type);
			AddMethods (BindingFlags.Instance | BindingFlags.Public, type);
			AddMethods (BindingFlags.Instance | BindingFlags.NonPublic, type);
		}

		void AddMethods (BindingFlags bf, Type type)
		{
			MemberInfo [] members = type.GetMethods (bf);

			Array.Reverse (members);

			foreach (MethodBase member in members) {
				string name = member.Name;

				// We use a name-based hash table of ArrayList's.
				ArrayList list = (ArrayList) method_hash [name];
				if (list == null) {
					list = new ArrayList ();
					method_hash.Add (name, list);
				}

				// Unfortunately, the elements returned by Type.GetMethods() aren't
				// sorted so we need to do this check for every member.
				BindingFlags new_bf = bf;
				if (member.DeclaringType == type)
					new_bf |= BindingFlags.DeclaredOnly;

				list.Add (new CacheEntry (Container, member, MemberTypes.Method, new_bf));
			}
		}

		/// <summary>
		///   Compute and return a appropriate `EntryType' magic number for the given
		///   MemberTypes and BindingFlags.
		/// </summary>
		protected static EntryType GetEntryType (MemberTypes mt, BindingFlags bf)
		{
			EntryType type = EntryType.None;

			if ((mt & MemberTypes.Constructor) != 0)
				type |= EntryType.Constructor;
			if ((mt & MemberTypes.Event) != 0)
				type |= EntryType.Event;
			if ((mt & MemberTypes.Field) != 0)
				type |= EntryType.Field;
			if ((mt & MemberTypes.Method) != 0)
				type |= EntryType.Method;
			if ((mt & MemberTypes.Property) != 0)
				type |= EntryType.Property;
			// Nested types are returned by static and instance searches.
			if ((mt & MemberTypes.NestedType) != 0)
				type |= EntryType.NestedType | EntryType.Static | EntryType.Instance;

			if ((bf & BindingFlags.Instance) != 0)
				type |= EntryType.Instance;
			if ((bf & BindingFlags.Static) != 0)
				type |= EntryType.Static;
			if ((bf & BindingFlags.Public) != 0)
				type |= EntryType.Public;
			if ((bf & BindingFlags.NonPublic) != 0)
				type |= EntryType.NonPublic;
			if ((bf & BindingFlags.DeclaredOnly) != 0)
				type |= EntryType.Declared;

			return type;
		}

		/// <summary>
		///   The `MemberTypes' enumeration type is a [Flags] type which means that it may
		///   denote multiple member types.  Returns true if the given flags value denotes a
		///   single member types.
		/// </summary>
		public static bool IsSingleMemberType (MemberTypes mt)
		{
			switch (mt) {
			case MemberTypes.Constructor:
			case MemberTypes.Event:
			case MemberTypes.Field:
			case MemberTypes.Method:
			case MemberTypes.Property:
			case MemberTypes.NestedType:
				return true;

			default:
				return false;
			}
		}

		/// <summary>
		///   We encode the MemberTypes and BindingFlags of each members in a "magic"
		///   number to speed up the searching process.
		/// </summary>
		[Flags]
		protected enum EntryType {
			None		= 0x000,

			Instance	= 0x001,
			Static		= 0x002,
			MaskStatic	= Instance|Static,

			Public		= 0x004,
			NonPublic	= 0x008,
			MaskProtection	= Public|NonPublic,

			Declared	= 0x010,

			Constructor	= 0x020,
			Event		= 0x040,
			Field		= 0x080,
			Method		= 0x100,
			Property	= 0x200,
			NestedType	= 0x400,

			MaskType	= Constructor|Event|Field|Method|Property|NestedType
		}

		protected struct CacheEntry {
			public readonly IMemberContainer Container;
			public readonly EntryType EntryType;
			public readonly MemberInfo Member;

			public CacheEntry (IMemberContainer container, MemberInfo member,
					   MemberTypes mt, BindingFlags bf)
			{
				this.Container = container;
				this.Member = member;
				this.EntryType = GetEntryType (mt, bf);
			}
		}

		/// <summary>
		///   This is called each time we're walking up one level in the class hierarchy
		///   and checks whether we can abort the search since we've already found what
		///   we were looking for.
		/// </summary>
		protected bool DoneSearching (ArrayList list)
		{
			//
			// We've found exactly one member in the current class and it's not
			// a method or constructor.
			//
			if (list.Count == 1 && !(list [0] is MethodBase))
				return true;

			//
			// Multiple properties: we query those just to find out the indexer
			// name
			//
			if ((list.Count > 0) && (list [0] is PropertyInfo))
				return true;

			return false;
		}

		/// <summary>
		///   Looks up members with name `name'.  If you provide an optional
		///   filter function, it'll only be called with members matching the
		///   requested member name.
		///
		///   This method will try to use the cache to do the lookup if possible.
		///
		///   Unlike other FindMembers implementations, this method will always
		///   check all inherited members - even when called on an interface type.
		///
		///   If you know that you're only looking for methods, you should use
		///   MemberTypes.Method alone since this speeds up the lookup a bit.
		///   When doing a method-only search, it'll try to use a special method
		///   cache (unless it's a dynamic type or an interface) and the returned
		///   MemberInfo's will have the correct ReflectedType for inherited methods.
		///   The lookup process will automatically restart itself in method-only
		///   search mode if it discovers that it's about to return methods.
		/// </summary>
		ArrayList global = new ArrayList ();
		bool using_global = false;
		
		static MemberInfo [] emptyMemberInfo = new MemberInfo [0];
		
		public MemberInfo [] FindMembers (MemberTypes mt, BindingFlags bf, string name,
					       MemberFilter filter, object criteria)
		{
			if (using_global)
				throw new Exception ();
			
			bool declared_only = (bf & BindingFlags.DeclaredOnly) != 0;
			bool method_search = mt == MemberTypes.Method;
			// If we have a method cache and we aren't already doing a method-only search,
			// then we restart a method search if the first match is a method.
			bool do_method_search = !method_search && (method_hash != null);

			ArrayList applicable;

			// If this is a method-only search, we try to use the method cache if
			// possible; a lookup in the method cache will return a MemberInfo with
			// the correct ReflectedType for inherited methods.
			
			if (method_search && (method_hash != null))
				applicable = (ArrayList) method_hash [name];
			else
				applicable = (ArrayList) member_hash [name];

			if (applicable == null)
				return emptyMemberInfo;

			//
			// 32  slots gives 53 rss/54 size
			// 2/4 slots gives 55 rss
			//
			// Strange: from 25,000 calls, only 1,800
			// are above 2.  Why does this impact it?
			//
			global.Clear ();
			using_global = true;

			Timer.StartTimer (TimerType.CachedLookup);

			EntryType type = GetEntryType (mt, bf);

			IMemberContainer current = Container;


			// `applicable' is a list of all members with the given member name `name'
			// in the current class and all its parent classes.  The list is sorted in
			// reverse order due to the way how the cache is initialy created (to speed
			// things up, we're doing a deep-copy of our parent).

			for (int i = applicable.Count-1; i >= 0; i--) {
				CacheEntry entry = (CacheEntry) applicable [i];

				// This happens each time we're walking one level up in the class
				// hierarchy.  If we're doing a DeclaredOnly search, we must abort
				// the first time this happens (this may already happen in the first
				// iteration of this loop if there are no members with the name we're
				// looking for in the current class).
				if (entry.Container != current) {
					if (declared_only || DoneSearching (global))
						break;

					current = entry.Container;
				}

				// Is the member of the correct type ?
				if ((entry.EntryType & type & EntryType.MaskType) == 0)
					continue;

				// Is the member static/non-static ?
				if ((entry.EntryType & type & EntryType.MaskStatic) == 0)
					continue;

				// Apply the filter to it.
				if (filter (entry.Member, criteria)) {
					if ((entry.EntryType & EntryType.MaskType) != EntryType.Method)
						do_method_search = false;
					global.Add (entry.Member);
				}
			}

			Timer.StopTimer (TimerType.CachedLookup);

			// If we have a method cache and we aren't already doing a method-only
			// search, we restart in method-only search mode if the first match is
			// a method.  This ensures that we return a MemberInfo with the correct
			// ReflectedType for inherited methods.
			if (do_method_search && (global.Count > 0)){
				using_global = false;

				return FindMembers (MemberTypes.Method, bf, name, filter, criteria);
			}

			using_global = false;
			MemberInfo [] copy = new MemberInfo [global.Count];
			global.CopyTo (copy);
			return copy;
		}
		
		//
		// This finds the method or property for us to override. invocationType is the type where
		// the override is going to be declared, name is the name of the method/property, and
		// paramTypes is the parameters, if any to the method or property
		//
		// Because the MemberCache holds members from this class and all the base classes,
		// we can avoid tons of reflection stuff.
		//
		public MemberInfo FindMemberToOverride (Type invocationType, string name, Type [] paramTypes, bool is_property)
		{
			ArrayList applicable;
			if (method_hash != null && !is_property)
				applicable = (ArrayList) method_hash [name];
			else
				applicable = (ArrayList) member_hash [name];
			
			if (applicable == null)
				return null;
			//
			// Walk the chain of methods, starting from the top.
			//
			for (int i = applicable.Count - 1; i >= 0; i--) {
				CacheEntry entry = (CacheEntry) applicable [i];
				
				if ((entry.EntryType & (is_property ? (EntryType.Property | EntryType.Field) : EntryType.Method)) == 0)
					continue;

				PropertyInfo pi = null;
				MethodInfo mi = null;
				FieldInfo fi = null;
				Type [] cmpAttrs = null;
				
				if (is_property) {
					if ((entry.EntryType & EntryType.Field) != 0) {
						fi = (FieldInfo)entry.Member;

						// TODO: For this case we ignore member type
						//fb = TypeManager.GetField (fi);
						//cmpAttrs = new Type[] { fb.MemberType };
					} else {
						pi = (PropertyInfo) entry.Member;
						cmpAttrs = TypeManager.GetArgumentTypes (pi);
					}
				} else {
					mi = (MethodInfo) entry.Member;
					cmpAttrs = TypeManager.GetArgumentTypes (mi);
				}

				if (fi != null) {
					// TODO: Almost duplicate !
					// Check visibility
					switch (fi.Attributes & FieldAttributes.FieldAccessMask) {
						case FieldAttributes.Private:
							//
							// A private method is Ok if we are a nested subtype.
							// The spec actually is not very clear about this, see bug 52458.
							//
							if (invocationType != entry.Container.Type &
								TypeManager.IsNestedChildOf (invocationType, entry.Container.Type))
								continue;

							break;
						case FieldAttributes.FamANDAssem:
						case FieldAttributes.Assembly:
							//
							// Check for assembly methods
							//
							if (mi.DeclaringType.Assembly != CodeGen.Assembly.Builder)
								continue;
							break;
					}
					return entry.Member;
				}

				//
				// Check the arguments
				//
				if (cmpAttrs.Length != paramTypes.Length)
					continue;

				for (int j = cmpAttrs.Length - 1; j >= 0; j --) {
					if (!TypeManager.IsEqual (paramTypes [j], cmpAttrs [j]))
						goto next;
				}
				
				//
				// get one of the methods because this has the visibility info.
				//
				if (is_property) {
					mi = pi.GetGetMethod (true);
					if (mi == null)
						mi = pi.GetSetMethod (true);
				}
				
				//
				// Check visibility
				//
				switch (mi.Attributes & MethodAttributes.MemberAccessMask) {
				case MethodAttributes.Private:
					//
					// A private method is Ok if we are a nested subtype.
					// The spec actually is not very clear about this, see bug 52458.
					//
					if (invocationType.Equals (entry.Container.Type) ||
					    TypeManager.IsNestedChildOf (invocationType, entry.Container.Type))
						return entry.Member;
					
					break;
				case MethodAttributes.FamANDAssem:
				case MethodAttributes.Assembly:
					//
					// Check for assembly methods
					//
					if (mi.DeclaringType.Assembly == CodeGen.Assembly.Builder)
						return entry.Member;
					
					break;
				default:
					//
					// A protected method is ok, because we are overriding.
					// public is always ok.
					//
					return entry.Member;
				}
			next:
				;
			}
			
			return null;
		}

 		/// <summary>
 		/// The method is looking for conflict with inherited symbols (errors CS0108, CS0109).
 		/// We handle two cases. The first is for types without parameters (events, field, properties).
 		/// The second are methods, indexers and this is why ignore_complex_types is here.
 		/// The latest param is temporary hack. See DoDefineMembers method for more info.
 		/// </summary>
 		public MemberInfo FindMemberWithSameName (string name, bool ignore_complex_types, MemberInfo ignore_member)
 		{
 			ArrayList applicable = null;
 
 			if (method_hash != null)
 				applicable = (ArrayList) method_hash [name];
 
 			if (applicable != null) {
 				for (int i = applicable.Count - 1; i >= 0; i--) {
 					CacheEntry entry = (CacheEntry) applicable [i];
 					if ((entry.EntryType & EntryType.Public) != 0)
 						return entry.Member;
 				}
 			}
 
 			if (member_hash == null)
 				return null;
 			applicable = (ArrayList) member_hash [name];
 			
 			if (applicable != null) {
 				for (int i = applicable.Count - 1; i >= 0; i--) {
 					CacheEntry entry = (CacheEntry) applicable [i];
 					if ((entry.EntryType & EntryType.Public) != 0 & entry.Member != ignore_member) {
 						if (ignore_complex_types) {
 							if ((entry.EntryType & EntryType.Method) != 0)
 								continue;
 
 							// Does exist easier way how to detect indexer ?
 							if ((entry.EntryType & EntryType.Property) != 0) {
 								Type[] arg_types = TypeManager.GetArgumentTypes ((PropertyInfo)entry.Member);
 								if (arg_types.Length > 0)
 									continue;
 							}
 						}
 						return entry.Member;
 					}
 				}
 			}
  			return null;
  		}

 		Hashtable locase_table;
 
 		/// <summary>
 		/// Builds low-case table for CLS Compliance test
 		/// </summary>
 		public Hashtable GetPublicMembers ()
 		{
 			if (locase_table != null)
 				return locase_table;
 
 			locase_table = new Hashtable ();
 			foreach (DictionaryEntry entry in member_hash) {
 				ArrayList members = (ArrayList)entry.Value;
 				for (int ii = 0; ii < members.Count; ++ii) {
 					CacheEntry member_entry = (CacheEntry) members [ii];
 
 					if ((member_entry.EntryType & EntryType.Public) == 0)
 						continue;
 
 					// TODO: Does anyone know easier way how to detect that member is internal ?
 					switch (member_entry.EntryType & EntryType.MaskType) {
 						case EntryType.Constructor:
 							continue;
 
 						case EntryType.Field:
 							if ((((FieldInfo)member_entry.Member).Attributes & (FieldAttributes.Assembly | FieldAttributes.Public)) == FieldAttributes.Assembly)
 								continue;
 							break;
 
 						case EntryType.Method:
 							if ((((MethodInfo)member_entry.Member).Attributes & (MethodAttributes.Assembly | MethodAttributes.Public)) == MethodAttributes.Assembly)
 								continue;
 							break;
 
 						case EntryType.Property:
 							PropertyInfo pi = (PropertyInfo)member_entry.Member;
 							if (pi.GetSetMethod () == null && pi.GetGetMethod () == null)
 								continue;
 							break;
 
 						case EntryType.Event:
 							EventInfo ei = (EventInfo)member_entry.Member;
 							MethodInfo mi = ei.GetAddMethod ();
 							if ((mi.Attributes & (MethodAttributes.Assembly | MethodAttributes.Public)) == MethodAttributes.Assembly)
 								continue;
 							break;
 					}
 					string lcase = ((string)entry.Key).ToLower (System.Globalization.CultureInfo.InvariantCulture);
 					locase_table [lcase] = member_entry.Member;
 					break;
 				}
 			}
 			return locase_table;
 		}
 
 		public Hashtable Members {
 			get {
 				return member_hash;
 			}
 		}
 
 		/// <summary>
 		/// Cls compliance check whether methods or constructors parameters differing only in ref or out, or in array rank
 		/// </summary>
 		public void VerifyClsParameterConflict (ArrayList al, MethodCore method, MemberInfo this_builder)
 		{
 			EntryType tested_type = (method is Constructor ? EntryType.Constructor : EntryType.Method) | EntryType.Public;
 
 			for (int i = 0; i < al.Count; ++i) {
 				MemberCache.CacheEntry entry = (MemberCache.CacheEntry) al [i];
 		
 				// skip itself
 				if (entry.Member == this_builder)
 					continue;
 		
 				if ((entry.EntryType & tested_type) != tested_type)
 					continue;
 		
				MethodBase method_to_compare = (MethodBase)entry.Member;
 				if (AttributeTester.AreOverloadedMethodParamsClsCompliant (method.ParameterTypes, TypeManager.GetArgumentTypes (method_to_compare)))
 					continue;

				IMethodData md = TypeManager.GetMethod (method_to_compare);

				// TODO: now we are ignoring CLSCompliance(false) on method from other assembly which is buggy.
				// However it is exactly what csc does.
				if (md != null && !md.IsClsCompliaceRequired (method.Parent))
					continue;
 		
 				Report.SymbolRelatedToPreviousError (entry.Member);
 				Report.Error (3006, method.Location, "Overloaded method '{0}' differing only in ref or out, or in array rank, is not CLS-compliant", method.GetSignatureForError ());
 			}
  		}
	}
}
