//
// decl.cs: Declaration base class for structs, classes, enums and interfaces.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
// TODO: Move the method verification stuff from the class.cs and interface.cs here
//

using System;
using System.Collections;
using System.Reflection.Emit;
using System.Reflection;

namespace Mono.CSharp {

	/// <summary>
	///   Base representation for members.  This is only used to keep track
	///   of Name, Location and Modifier flags.
	/// </summary>
	public abstract class MemberCore {
		/// <summary>
		///   Public name
		/// </summary>
		public string Name;

		/// <summary>
		///   Modifier flags that the user specified in the source code
		/// </summary>
		public int ModFlags;

		/// <summary>
		///   Location where this declaration happens
		/// </summary>
		public readonly Location Location;

		public MemberCore (string name, Location loc)
		{
			Name = name;
			Location = loc;
		}

		protected void WarningNotHiding (TypeContainer parent)
		{
			Report.Warning (
				109, Location,
				"The member " + parent.MakeName (Name) + " does not hide an " +
				"inherited member.  The keyword new is not required");
							   
		}

		void Error_CannotChangeAccessModifiers (TypeContainer parent, MethodInfo parent_method,
							string name)
		{
			//
			// FIXME: report the old/new permissions?
			//
			Report.Error (
				507, Location, parent.MakeName (Name) +
				": can't change the access modifiers when overriding inherited " +
				"member `" + name + "'");
		}
		
		//
		// Performs various checks on the MethodInfo `mb' regarding the modifier flags
		// that have been defined.
		//
		// `name' is the user visible name for reporting errors (this is used to
		// provide the right name regarding method names and properties)
		//
		protected bool CheckMethodAgainstBase (TypeContainer parent, MethodAttributes my_attrs,
						       MethodInfo mb, string name)
		{
			bool ok = true;
			
			if ((ModFlags & Modifiers.OVERRIDE) != 0){
				if (!(mb.IsAbstract || mb.IsVirtual)){
					Report.Error (
						506, Location, parent.MakeName (Name) +
						": cannot override inherited member `" +
						name + "' because it is not " +
						"virtual, abstract or override");
					ok = false;
				}
				
				// Now we check that the overriden method is not final
				
				if (mb.IsFinal) {
					Report.Error (239, Location, parent.MakeName (Name) + " : cannot " +
						      "override inherited member `" + name +
						      "' because it is sealed.");
					ok = false;
				}

				//
				// Check that the permissions are not being changed
				//
				MethodAttributes thisp = my_attrs & MethodAttributes.MemberAccessMask;
				MethodAttributes parentp = mb.Attributes & MethodAttributes.MemberAccessMask;

				if (thisp != parentp){
					Error_CannotChangeAccessModifiers (parent, mb, name);
					ok = false;
				}
			}

			if (mb.IsVirtual || mb.IsAbstract){
				if ((ModFlags & (Modifiers.NEW | Modifiers.OVERRIDE)) == 0){
					if (Name != "Finalize" && (RootContext.WarningLevel >= 2)){
						Report.Warning (
							114, Location, parent.MakeName (Name) + 
							" hides inherited member `" + name +
							"'.  To make the current member override that " +
							"implementation, add the override keyword, " +
							"otherwise use the new keyword");
					}
				}
			} else {
				if ((ModFlags & (Modifiers.NEW | Modifiers.OVERRIDE)) == 0){
					if (Name != "Finalize" && (RootContext.WarningLevel >= 1)){
						Report.Warning (
							108, Location, "The keyword new is required on " +
							parent.MakeName (Name) + " because it hides " +
							"inherited member `" + name + "'");
					}
				}
			}

			return ok;
		}

		public abstract bool Define (TypeContainer parent);

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
	}

	//
	// FIXME: This is temporary outside DeclSpace, because I have to fix a bug
	// in MCS that makes it fail the lookup for the enum
	//

		/// <summary>
		///   The result value from adding an declaration into
		///   a struct or a class
		/// </summary>
		public enum AdditionResult {
			/// <summary>
			/// The declaration has been successfully
			/// added to the declation space.
			/// </summary>
			Success,

			/// <summary>
			///   The symbol has already been defined.
			/// </summary>
			NameExists,

			/// <summary>
			///   Returned if the declation being added to the
			///   name space clashes with its container name.
			///
			///   The only exceptions for this are constructors
			///   and static constructors
			/// </summary>
			EnclosingClash,

			/// <summary>
			///   Returned if a constructor was created (because syntactically
			///   it looked like a constructor) but was not (because the name
			///   of the method is not the same as the container class
			/// </summary>
			NotAConstructor,

			/// <summary>
			///   This is only used by static constructors to emit the
			///   error 111, but this error for other things really
			///   happens at another level for other functions.
			/// </summary>
			MethodExists
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
		///   this points to the actual definition that is being
		///   created with System.Reflection.Emit
		/// </summary>
		public TypeBuilder TypeBuilder;

		/// <summary>
		///   This variable tracks whether we have Closed the type
		/// </summary>
		public bool Created = false;
		
		//
		// This is the namespace in which this typecontainer
		// was declared.  We use this to resolve names.
		//
		public Namespace Namespace;

		public Hashtable Cache = new Hashtable ();
		
		public string Basename;
		
		/// <summary>
		///   defined_names is used for toplevel objects
		/// </summary>
		protected Hashtable defined_names;

		TypeContainer parent;		

		public DeclSpace (TypeContainer parent, string name, Location l)
			: base (name, l)
		{
			Basename = name.Substring (1 + name.LastIndexOf ('.'));
			defined_names = new Hashtable ();
			this.parent = parent;
		}

		/// <summary>
		///   Returns a status code based purely on the name
		///   of the member being added
		/// </summary>
		protected AdditionResult IsValid (string name)
		{
			if (name == Basename)
				return AdditionResult.EnclosingClash;

			if (defined_names.Contains (name))
				return AdditionResult.NameExists;

			return AdditionResult.Success;
		}

		/// <summary>
		///   Introduce @name into this declaration space and
		///   associates it with the object @o.  Note that for
		///   methods this will just point to the first method. o
		/// </summary>
		protected void DefineName (string name, object o)
		{
			defined_names.Add (name, o);
		}

		/// <summary>
		///   Returns the object associated with a given name in the declaration
		///   space.  This is the inverse operation of `DefineName'
		/// </summary>
		public object GetDefinition (string name)
		{
			return defined_names [name];
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

		public TypeContainer Parent {
			get {
				return parent;
			}
		}

		/// <summary>
		///   Looks up the alias for the name
		/// </summary>
		public string LookupAlias (string name)
		{
			if (Namespace != null)
				return Namespace.LookupAlias (name);
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
				if (parent != null){
					if (parent.parent == null)
						return true;
				}
				return false;
			}
		}

		public virtual void CloseType ()
		{
			if (!Created){
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
				Created = true;
			}
		}

		/// <remarks>
		///  Should be overriten by the appropriate declaration space
		/// <remarks>
		public abstract TypeBuilder DefineType ();
		
		//
		// Whether this is an `unsafe context'
		//
		public bool UnsafeContext {
			get {
				if ((ModFlags & Modifiers.UNSAFE) != 0)
					return true;
				if (parent != null)
					return parent.UnsafeContext;
				return false;
			}
		}

		public static string MakeFQN (string nsn, string name)
		{
			string prefix = (nsn == "" ? "" : nsn + ".");

			return prefix + name;
		}

		EmitContext type_resolve_ec;
		EmitContext GetTypeResolveEmitContext (TypeContainer parent, Location loc)
		{
			type_resolve_ec = new EmitContext (parent, this, loc, null, null, ModFlags, false);
			type_resolve_ec.ResolvingTypeTree = true;
			type_resolve_ec.OnlyLookupTypes = true;

			return type_resolve_ec;
		}

		// <summary>
		//    Looks up the type, as parsed into the expression `e' 
		// </summary>
		public Type ResolveType (Expression e, bool silent, Location loc)
		{
			if (type_resolve_ec == null)
				type_resolve_ec = GetTypeResolveEmitContext (parent, loc);
			type_resolve_ec.loc = loc;
			Expression d = e.Resolve (type_resolve_ec);
			if (d == null || d.eclass != ExprClass.Type){
				if (!silent){
					Report.Error (246, loc, "Cannot find type `"+ e.ToString () +"'");
				}
				return null;
			}

			return d.Type;
		}

		// <summary>
		//    Resolves the expression `e' for a type, and will recursively define
		//    types. 
		// </summary>
		public Expression ResolveTypeExpr (Expression e, bool silent, Location loc)
		{
			if (type_resolve_ec == null)
				type_resolve_ec = GetTypeResolveEmitContext (parent, loc);

			Expression d = e.Resolve (type_resolve_ec);
			if (d == null || d.eclass != ExprClass.Type){
				if (!silent){
					Report.Error (246, loc, "Cannot find type `"+ e +"'");
				}
				return null;
			}

			return d;
		}
		
		Type LookupInterfaceOrClass (string ns, string name, out bool error)
		{
			DeclSpace parent;
			Type t;

			error = false;
			name = MakeFQN (ns, name);
			
			t  = TypeManager.LookupType (name);
			if (t != null)
				return t;

			parent = (DeclSpace) RootContext.Tree.Decls [name];
			if (parent == null)
				return null;
			
			t = parent.DefineType ();
			if (t == null){
				Report.Error (146, "Class definition is circular: `"+name+"'");
				error = true;
				return null;
			}
			return t;
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
		public Type FindType (string name)
		{
			Type t;
			bool error;

			//
			// For the case the type we are looking for is nested within this one
			// or is in any base class
			//
			DeclSpace containing_ds = this;

			while (containing_ds != null){
				Type current_type = containing_ds.TypeBuilder;

				while (current_type != null) {
					string pre = current_type.FullName;
					
					t = LookupInterfaceOrClass (pre, name, out error);
					if (error)
						return null;
				
					if (t != null) 
						return t;

					current_type = current_type.BaseType;
				}
				containing_ds = containing_ds.Parent;
			}
			
			//
			// Attempt to lookup the class on our namespace and all it's implicit parents
			//
			for (string ns = Namespace.Name; ns != null; ns = RootContext.ImplicitParent (ns)) {

				t = LookupInterfaceOrClass (ns, name, out error);
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

			for (Namespace ns = Namespace; ns != null; ns = ns.Parent){

				t = LookupInterfaceOrClass (ns.Name, name, out error);
				if (error)
					return null;

				if (t != null)
					return t;

				//
				// Now check the using clause list
				//
				ArrayList using_list = ns.UsingTable;
				
				if (using_list == null)
					continue;

				foreach (string n in using_list){
					t = LookupInterfaceOrClass (n, name, out error);
					if (error)
						return null;

					if (t != null)
						return t;
				}
				
			}

			//Report.Error (246, Location, "Can not find type `"+name+"'");
			return null;
		}
	}
}
