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
				"The member `" + parent.Name + "." + Name + "' does not hide an " +
				"inherited member.  The keyword new is not required");
							   
		}

		static string MethodBaseName (MethodBase mb)
		{
			return "`" + mb.ReflectedType.Name + "." + mb.Name + "'";
		}

		//
		// Performs various checks on the MethodInfo `mb' regarding the modifier flags
		// that have been defined.
		//
		// `name' is the user visible name for reporting errors (this is used to
		// provide the right name regarding method names and properties)
		//
		protected bool CheckMethodAgainstBase (TypeContainer parent, MethodInfo mb)
		{
			bool ok = true;
			
			if ((ModFlags & Modifiers.OVERRIDE) != 0){
				if (!(mb.IsAbstract || mb.IsVirtual)){
					Report.Error (
						506, Location, parent.MakeName (Name) +
						": cannot override inherited member `" +
						mb.ReflectedType.Name + "' because it is not " +
						"virtual, abstract or override");
					ok = false;
				}
			}

			if (mb.IsVirtual || mb.IsAbstract){
				if ((ModFlags & (Modifiers.NEW | Modifiers.OVERRIDE)) == 0){
					Report.Warning (
						114, Location, parent.MakeName (Name) + 
						" hides inherited member " + MethodBaseName (mb) +
						".  To make the current member override that " +
						"implementation, add the override keyword, " +
						"otherwise use the new keyword");
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

			Report.Error (214, Location, "Pointers can only be used in an unsafe context");
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
		TypeBuilder definition;
		public bool Created = false;
		
		//
		// This is the namespace in which this typecontainer
		// was declared.  We use this to resolve names.
		//
		public Namespace Namespace;

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

		public TypeBuilder TypeBuilder {
			get {
				return definition;
			}

			set {
				definition = value;
			}
		}

		public TypeContainer Parent {
			get {
				return parent;
			}
		}

		public virtual void CloseType ()
		{
			if (!Created){
				try {
					definition.CreateType ();
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

	}
}
