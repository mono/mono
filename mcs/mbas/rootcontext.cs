//
// rootcontext.cs: keeps track of our tree representation, and assemblies loaded.
//
// Author: Miguel de Icaza (miguel@ximian.com)
//         Ravi Pratap     (ravi@ximian.com)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace Mono.MonoBASIC {

	public class RootContext {

		//
		// Contains the parsed tree
		//
		static Tree tree;

		//
		// Tracks Imported namespaces
		//
		static SourceBeingCompiled sourceBeingCompiled = new SourceBeingCompiled();

		//
		// This hashtable contains all of the #definitions across the source code
		// it is used by the ConditionalAttribute handler.
		//
		public static Hashtable AllDefines = new Hashtable ();
		
		//
		// Whether we are being linked against the standard libraries.
		// This is only used to tell whether `System.Object' should
		// have a parent or not.
		//
		public static bool StdLib = true;

		//
		// This keeps track of the order in which classes were defined
		// so that we can poulate them in that order.
		//
		// Order is important, because we need to be able to tell by
		// examining the parent's list of methods which ones are virtual
		// or abstract as well as the parent names (to implement new, 
		// override).
		//
		static ArrayList type_container_resolve_order;
		static ArrayList interface_resolve_order;
		static ArrayList attribute_types;

		//
		// Holds a reference to the Private Implementation Details
		// class.
		//
		static TypeBuilder impl_details_class;

		public static int WarningLevel = 2;
		
		//
		// Constructor
		//
		static RootContext ()
		{
			tree = new Tree ();
			interface_resolve_order = new ArrayList ();
			type_container_resolve_order = new ArrayList ();
		}

		static public Tree Tree {
			get {
				return tree;
			}
		}

		static public string MainClass;
		
		public static void RegisterOrder (Interface iface)
		{
			interface_resolve_order.Add (iface);
		}
		
		public static void RegisterOrder (TypeContainer tc)
		{
			type_container_resolve_order.Add (tc);
		}

		public static void RegisterAttribute (TypeContainer tc)
		{
			if (attribute_types == null)
				attribute_types = new ArrayList ();
			
			attribute_types.Add (tc);
		}
		
		public static void InitializeImports(ArrayList ImportsList)
		{
			sourceBeingCompiled.InitializeImports (ImportsList);
		}

		public static SourceBeingCompiled SourceBeingCompiled
		{
			get { return sourceBeingCompiled; }
		}

		public static void VerifyImports()
		{
			sourceBeingCompiled.VerifyImports();
		}

		// 
		// The default compiler checked state
		//
		static public bool Checked = false;

		//
		// Whether to allow Unsafe code
		//
		static public bool Unsafe = false;
		
		// Root namespace name (implicit namespace)
		static public string RootNamespace = "";
		
		static string MakeFQN (string nsn, string name)
		{
			string prefix = (nsn == "" ? "" : nsn + ".");

			return prefix + name;
		}
		       
		// <remarks>
		//   This function is used to resolve the hierarchy tree.
		//   It processes interfaces, structs and classes in that order.
		//
		//   It creates the TypeBuilder's as it processes the user defined
		//   types.  
		// </remarks>
		static public void ResolveTree ()
		{
			//
			// Process the attribute types separately and before anything else
			//
			if (attribute_types != null)
				foreach (TypeContainer tc in attribute_types)
					tc.DefineType ();
			
			//
			// Interfaces are processed next, as classes and
			// structs might inherit from an object or implement
			// a set of interfaces, we need to be able to tell
			// them appart by just using the TypeManager.
			//
			TypeContainer root = Tree.Types;

			ArrayList ifaces = root.Interfaces;
			if (ifaces != null){
				foreach (Interface i in ifaces) 
					i.DefineType ();
			}

			
			foreach (TypeContainer tc in root.Types) 
				tc.DefineType ();

			if (root.Delegates != null)
				foreach (Delegate d in root.Delegates) 
					d.DefineType ();

			if (root.Enums != null)
				foreach (Enum e in root.Enums)
					e.DefineType ();
			
		}

		static void Error_TypeConflict (string name, Location loc)
		{
			Report.Error (
				520, loc, "`" + name + "' conflicts with a predefined type");
		}

		static void Error_TypeConflict (string name)
		{
			Report.Error (
				520, "`" + name + "' conflicts with a predefined type");
		}

		//
		// Resolves a single class during the corlib bootstrap process
		//
		static TypeBuilder BootstrapCorlib_ResolveClass (TypeContainer root, string name)
		{
			object o = root.GetDefinition (name);
			if (o == null){
				Report.Error (518, "The predefined type `" + name + "' is not defined");
				return null;
			}

			if (!(o is Class)){
				if (o is DeclSpace){
					DeclSpace d = (DeclSpace) o;

					Error_TypeConflict (name, d.Location);
				} else
					Error_TypeConflict (name);

				return null;
			}

			return ((DeclSpace) o).DefineType ();
		}

		//
		// Resolves a struct during the corlib bootstrap process
		//
		static void BootstrapCorlib_ResolveStruct (TypeContainer root, string name)
		{
			object o = root.GetDefinition (name);
			if (o == null){
				Report.Error (518, "The predefined type `" + name + "' is not defined");
				return;
			}

			if (!(o is Struct)){
				if (o is DeclSpace){
					DeclSpace d = (DeclSpace) o;

					Error_TypeConflict (name, d.Location);
				} else
					Error_TypeConflict (name);

				return;
			}

			((DeclSpace) o).DefineType ();
		}

		//
		// Resolves a struct during the corlib bootstrap process
		//
		static void BootstrapCorlib_ResolveInterface (TypeContainer root, string name)
		{
			object o = root.GetDefinition (name);
			if (o == null){
				Report.Error (518, "The predefined type `" + name + "' is not defined");
				return;
			}

			if (!(o is Interface)){
				if (o is DeclSpace){
					DeclSpace d = (DeclSpace) o;

					Error_TypeConflict (name, d.Location);
				} else
					Error_TypeConflict (name);

				return;
			}

			((DeclSpace) o).DefineType ();
		}

		//
		// Resolves a delegate during the corlib bootstrap process
		//
		static void BootstrapCorlib_ResolveDelegate (TypeContainer root, string name)
		{
			object o = root.GetDefinition (name);
			if (o == null){
				Report.Error (518, "The predefined type `" + name + "' is not defined");
				Environment.Exit (0);
			}

			if (!(o is Delegate)){
				Error_TypeConflict (name);
				return;
			}

			((DeclSpace) o).DefineType ();
		}
		

		/// <summary>
		///    Resolves the core types in the compiler when compiling with --nostdlib
		/// </summary>
		static public void ResolveCore ()
		{
			TypeContainer root = Tree.Types;

			TypeManager.object_type = BootstrapCorlib_ResolveClass (root, "System.Object");
			TypeManager.value_type = BootstrapCorlib_ResolveClass (root, "System.ValueType");
			TypeManager.attribute_type = BootstrapCorlib_ResolveClass (root, "System.Attribute");
			
			string [] interfaces_first_stage = {
				"System.IComparable", "System.ICloneable",
				"System.IConvertible",
				
				"System.Collections.IEnumerable",
				"System.Collections.ICollection",
				"System.Collections.IEnumerator",
				"System.Collections.IList", 
				"System.IAsyncResult",
				"System.IDisposable",
				
				"System.Runtime.Serialization.ISerializable",

				"System.Reflection.IReflect",
				"System.Reflection.ICustomAttributeProvider"
			};

			foreach (string iname in interfaces_first_stage)
				BootstrapCorlib_ResolveInterface (root, iname);

			//
			// These are the base value types
			//
			string [] structs_first_stage = {
				"System.Byte",    "System.SByte",
				"System.Int16",   "System.UInt16",
				"System.Int32",   "System.UInt32",
				"System.Int64",   "System.UInt64",
			};

			foreach (string cname in structs_first_stage)
				BootstrapCorlib_ResolveStruct (root, cname);

			//
			// Now, we can load the enumerations, after this point,
			// we can use enums.
			//
			TypeManager.InitEnumUnderlyingTypes ();

			string [] structs_second_stage = {
				"System.Single",  "System.Double",
				"System.Char",    "System.Boolean",
				"System.Decimal", "System.Void",
				"System.RuntimeFieldHandle",
				"System.RuntimeTypeHandle",
				"System.IntPtr"
			};
			
			foreach (string cname in structs_second_stage)
				BootstrapCorlib_ResolveStruct (root, cname);
			
			//
			// These are classes that depends on the core interfaces
			//
			string [] classes_second_stage = {
				"System.Reflection.MemberInfo",
				"System.Type",
				"System.Exception",

				//
				// These are not really important in the order, but they
				// are used by the compiler later on (typemanager/CoreLookupType-d)
				//
				"System.Runtime.CompilerServices.RuntimeHelpers",
				"System.Reflection.DefaultMemberAttribute",
				"System.Threading.Monitor",
				
				"System.AttributeUsageAttribute",
				"System.Runtime.InteropServices.DllImportAttribute",
				"System.Runtime.CompilerServices.MethodImplAttribute",
				"System.Runtime.InteropServices.MarshalAsAttribute",
				"System.Diagnostics.ConditionalAttribute",
				"System.ObsoleteAttribute",
				"System.ParamArrayAttribute",
				"System.Security.UnverifiableCodeAttribute",
				"System.Runtime.CompilerServices.IndexerNameAttribute",
			};

			// We must store them here before calling BootstrapCorlib_ResolveDelegate.
			TypeManager.string_type = BootstrapCorlib_ResolveClass (root, "System.String");
			TypeManager.enum_type = BootstrapCorlib_ResolveClass (root, "System.Enum");
			TypeManager.array_type = BootstrapCorlib_ResolveClass (root, "System.Array");
			TypeManager.multicast_delegate_type = BootstrapCorlib_ResolveClass (root, "System.MulticastDelegate");
			TypeManager.delegate_type = BootstrapCorlib_ResolveClass (root, "System.Delegate");
			
			foreach (string cname in classes_second_stage)
				BootstrapCorlib_ResolveClass (root, cname);

			BootstrapCorlib_ResolveDelegate (root, "System.AsyncCallback");
		}
			
		// <summary>
		//   Closes all open types
		// </summary>
		//
		// <remarks>
		//   We usually use TypeBuilder types.  When we are done
		//   creating the type (which will happen after we have added
		//   methods, fields, etc) we need to "Define" them before we
		//   can save the Assembly
		// </remarks>
		static public void CloseTypes ()
		{
			TypeContainer root = Tree.Types;
			
			ArrayList ifaces = root.Interfaces;

			if (root.Enums != null)
				foreach (Enum en in root.Enums)
					en.CloseType ();

			if (attribute_types != null)
				foreach (TypeContainer tc in attribute_types)
					tc.CloseType ();
			
			foreach (Interface iface in interface_resolve_order)
				iface.CloseType ();

			//
			// We do this in two passes, first we close the structs,
			// then the classes, because it seems the code needs it this
			// way.  If this is really what is going on, we should probably
			// make sure that we define the structs in order as well.
			//
			foreach (TypeContainer tc in type_container_resolve_order){
				if (tc is Struct && tc.Parent == tree.Types){
					tc.CloseType ();
				}
			}

			foreach (TypeContainer tc in type_container_resolve_order){
				if (!(tc is Struct && tc.Parent == tree.Types))
					tc.CloseType ();					
			}
			
			if (root.Delegates != null)
				foreach (Delegate d in root.Delegates)
					d.CloseType ();


			//
			// If we have a <PrivateImplementationDetails> class, close it
			//
			if (impl_details_class != null){
				impl_details_class.CreateType ();
			}
		}

		//
		// This idea is from Felix Arrese-Igor
		//
		// Returns : the implicit parent of a composite namespace string
		//   eg. Implicit parent of A.B is A
		//
		static public string ImplicitParent (string ns)
		{
			int i = ns.LastIndexOf (".");
			if (i < 0)
				return null;
			
			return ns.Substring (0, i);
		}

		static Type NamespaceLookup (Namespace curr_ns, string name, Location loc)
		{
			Type t;
			
			//
			// Try in the current namespace and all its implicit parents
			//
			for (string ns = curr_ns.Name; ns != null; ns = ImplicitParent (ns)) {
				t = TypeManager.LookupType (MakeFQN (ns, name));
				if (t != null)
					return t;
			}
			
			//
			// It's possible that name already is fully qualified. So we do
			// a simple direct lookup without adding any namespace names
			//
			t = TypeManager.LookupType (name); 
			if (t != null)
				return t;

			//
			// Try the aliases in the current namespace
			//
			string alias = sourceBeingCompiled.LookupAlias (name);

			if (alias != null) {
				t = TypeManager.LookupType (alias);
				if (t != null)
					return t;

				t = TypeManager.LookupType (MakeFQN (alias, name));
				if (t != null)
					return t;
			}
			
			for (Namespace ns = curr_ns; ns != null; ns = ns.Parent) {
				//
				// Look in the namespace ns
				//
				t = TypeManager.LookupType (MakeFQN (ns.Name, name));
				if (t != null)
					return t;
				
				//
				// Then try with the using clauses
				//

				ICollection imports_list = sourceBeingCompiled.ImportsTable;

				if (imports_list == null)
					continue;

				Type match = null;
				foreach (SourceBeingCompiled.ImportsEntry ue in imports_list) {
				//TODO: deal with standard modules
					match = TypeManager.LookupType (MakeFQN (ue.Name, name));
					if (match != null){
						if (t != null){
							DeclSpace.Error_AmbiguousTypeReference (loc, name, t, match);
							return null;
						}
						
						t = match;
						ue.Used = true;
					}
				}
				if (t != null)
					return t;

				//
				// Try with aliases
				//
				string a = sourceBeingCompiled.LookupAlias (name);
				if (a != null) {
					t = TypeManager.LookupType (a);
					if (t != null)
						return t;

					t = TypeManager.LookupType (MakeFQN (a, name));
					if (t != null)
						return t;
				}
			}

			return null;
		}
		
		//
		// Public function used to locate types, this can only
		// be used after the ResolveTree function has been invoked.
		//
		// Returns: Type or null if they type can not be found.
		//
		// Come to think of it, this should be a DeclSpace
		//
		static public Type LookupType (DeclSpace ds, string name, bool silent, Location loc)
		{
			Type t;

			if (ds.Cache.Contains (name)){
				t = (Type) ds.Cache [name];
				if (t != null)
					return t;
			} else {
				//
				// For the case the type we are looking for is nested within this one
				// or is in any base class
				//
				DeclSpace containing_ds = ds;
				while (containing_ds != null){
					Type current_type = containing_ds.TypeBuilder;
					
					while (current_type != null) {
						//
						// nested class
						//
						t = TypeManager.LookupType (current_type.FullName + "." + name);
						if (t != null){
							ds.Cache [name] = t;
							return t;
						}
						
						current_type = current_type.BaseType;
					}
					
					containing_ds = containing_ds.Parent;
				}
				
				t = NamespaceLookup (ds.Namespace, name, loc);
				if (t != null){
					ds.Cache [name] = t;
					return t;
				}
			}

			if (!silent)
				Report.Error (30002, loc, "Cannot find type `"+name+"'");
			
			return null;
		}

		// <summary>
		//   This is the silent version of LookupType, you can use this
		//   to `probe' for a type
		// </summary>
		static public Type LookupType (TypeContainer tc, string name, Location loc)
		{
			return LookupType (tc, name, true, loc);
		}

		static public bool IsNamespace (string name)
		{
			Namespace ns;

			if (tree.Namespaces != null){
				ns = (Namespace) tree.Namespaces [name];

				if (ns != null)
					return true;
			}

			return false;
		}

		static void Report1530 (Location loc)
		{
			Report.Error (1530, loc, "Keyword new not allowed for namespace elements");
		}
		
		static public void PopulateCoreType (TypeContainer root, string name)
		{
			DeclSpace ds = (DeclSpace) root.GetDefinition (name);

			ds.DefineMembers (root);
			ds.Define (root);
		}
		
		static public void BootCorlib_PopulateCoreTypes ()
		{
			TypeContainer root = tree.Types;

			PopulateCoreType (root, "System.Object");
			PopulateCoreType (root, "System.ValueType");
			PopulateCoreType (root, "System.Attribute");
		}
		
		// <summary>
		//   Populates the structs and classes with fields and methods
		// </summary>
		//
		// This is invoked after all interfaces, structs and classes
		// have been defined through `ResolveTree' 
		static public void PopulateTypes ()
		{
			TypeContainer root = Tree.Types;

			if (attribute_types != null)
				foreach (TypeContainer tc in attribute_types)
					tc.DefineMembers (root);
			
			if (interface_resolve_order != null){
				foreach (Interface iface in interface_resolve_order)
					if ((iface.ModFlags & Modifiers.NEW) == 0)
						iface.DefineMembers (root);
					else
						Report1530 (iface.Location);
			}


			if (type_container_resolve_order != null){
				foreach (TypeContainer tc in type_container_resolve_order) {
					// When compiling corlib, these types have already been
					// populated from BootCorlib_PopulateCoreTypes ().
					if (!RootContext.StdLib &&
					    ((tc.Name == "System.Object") ||
					     (tc.Name == "System.Attribute") ||
					     (tc.Name == "System.ValueType")))
						continue;

					if ((tc.ModFlags & Modifiers.NEW) == 0)
						tc.DefineMembers (root);
					else
						Report1530 (tc.Location);
				}
			}

			ArrayList delegates = root.Delegates;
			if (delegates != null){
				foreach (Delegate d in delegates)
					if ((d.ModFlags & Modifiers.NEW) == 0)
						d.DefineMembers (root);
					else
						Report1530 (d.Location);
			}

			ArrayList enums = root.Enums;
			if (enums != null){
				foreach (Enum en in enums)
					if ((en.ModFlags & Modifiers.NEW) == 0)
						en.DefineMembers (root);
					else
						Report1530 (en.Location);
			}
		}

		static public void DefineTypes ()
		{
			TypeContainer root = Tree.Types;

			if (attribute_types != null)
				foreach (TypeContainer tc in attribute_types)
					tc.Define (root);
			
			if (interface_resolve_order != null){
				foreach (Interface iface in interface_resolve_order)
					if ((iface.ModFlags & Modifiers.NEW) == 0)
						iface.Define (root);
			}


			if (type_container_resolve_order != null){
				foreach (TypeContainer tc in type_container_resolve_order) {
					// When compiling corlib, these types have already been
					// populated from BootCorlib_PopulateCoreTypes ().
					if (!RootContext.StdLib &&
					    ((tc.Name == "System.Object") ||
					     (tc.Name == "System.Attribute") ||
					     (tc.Name == "System.ValueType")))
						continue;

					if ((tc.ModFlags & Modifiers.NEW) == 0)
						tc.Define (root);
				}
			}

			ArrayList delegates = root.Delegates;
			if (delegates != null){
				foreach (Delegate d in delegates)
					if ((d.ModFlags & Modifiers.NEW) == 0)
						d.Define (root);
			}

			ArrayList enums = root.Enums;
			if (enums != null){
				foreach (Enum en in enums)
					if ((en.ModFlags & Modifiers.NEW) == 0)
						en.Define (root);
			}
		}

		static public void EmitCode ()
		{
			if (attribute_types != null)
				foreach (TypeContainer tc in attribute_types)
					tc.Emit ();
			
			if (type_container_resolve_order != null) {
				foreach (TypeContainer tc in type_container_resolve_order)
					tc.EmitConstants ();
				
				foreach (TypeContainer tc in type_container_resolve_order)
					tc.Emit ();
			}
			
			if (Unsafe) {
				if (TypeManager.unverifiable_code_ctor == null) {
					Console.WriteLine ("Internal error ! Cannot set unverifiable code attribute.");
					return;
				}
				
				CustomAttributeBuilder cb = new CustomAttributeBuilder (TypeManager.unverifiable_code_ctor, new object [0]);
				CodeGen.ModuleBuilder.SetCustomAttribute (cb);
			}
		}
		
		//
		// Public Field, used to track which method is the public entry
		// point.
		//
		static public MethodInfo EntryPoint;

                //
                // Track the location of the entry point.
                //
                static public Location EntryPointLocation;

		//
		// These are used to generate unique names on the structs and fields.
		//
		static int field_count;
		
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
		static public FieldBuilder MakeStaticData (byte [] data)
		{
			FieldBuilder fb;
			int size = data.Length;
			
			if (impl_details_class == null)
				impl_details_class = CodeGen.ModuleBuilder.DefineType (
					"<PrivateImplementationDetails>", TypeAttributes.NotPublic, TypeManager.object_type);

			fb = impl_details_class.DefineInitializedData (
				"$$field-" + (field_count++), data,
				FieldAttributes.Static | FieldAttributes.Assembly);
			
			return fb;
		}

	}
}
