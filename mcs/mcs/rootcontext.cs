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

namespace Mono.CSharp {

	public enum LanguageVersion
	{
		Default	= 0,
		ISO_1	= 1
	}

	public class RootContext {

		//
		// Contains the parsed tree
		//
		static Tree tree;

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
		static ArrayList attribute_types;

		//
		// Holds a reference to the Private Implementation Details
		// class.
		//
		static ArrayList helper_classes;
		
		static TypeBuilder impl_details_class;

		public static int WarningLevel = 2;
		
		public static Target Target = Target.Exe;
		public static string TargetExt = ".exe";

		public static bool VerifyClsCompliance = true;

		public static LanguageVersion Version = LanguageVersion.Default;

		//
		// We keep strongname related info here because
		// it's also used as complier options from CSC 8.x
		//
		public static string StrongNameKeyFile;
		public static string StrongNameKeyContainer;
		public static bool StrongNameDelaySign = false;

		//
		// Constructor
		//
		static RootContext ()
		{
			tree = new Tree ();
			type_container_resolve_order = new ArrayList ();
		}
		
		public static bool NeedsEntryPoint {
			get {
				return RootContext.Target == Target.Exe || RootContext.Target == Target.WinExe;
			}
		}

		static public Tree Tree {
			get {
				return tree;
			}
		}

		static public string MainClass;
		
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
		
		// 
		// The default compiler checked state
		//
		static public bool Checked = false;

		//
		// Whether to allow Unsafe code
		//
		static public bool Unsafe = false;
		
		static string MakeFQN (string nsn, string name)
		{
			if (nsn == "")
				return name;
			return String.Concat (nsn, ".", name);
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
				Environment.Exit (1);
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
				"System.RuntimeArgumentHandle",
				"System.RuntimeTypeHandle",
				"System.IntPtr",
				"System.TypedReference",
				"System.ArgIterator"
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
				"System.CLSCompliantAttribute",
				"System.Security.UnverifiableCodeAttribute",
				"System.Runtime.CompilerServices.IndexerNameAttribute",
				"System.Runtime.InteropServices.InAttribute",
				"System.Runtime.InteropServices.StructLayoutAttribute",
				"System.Runtime.InteropServices.FieldOffsetAttribute",
				"System.InvalidOperationException",
				"System.NotSupportedException",
				"System.MarshalByRefObject"
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
			
			if (root.Enums != null)
				foreach (Enum en in root.Enums)
					en.CloseType ();

			if (attribute_types != null)
				foreach (TypeContainer tc in attribute_types)
					tc.CloseType ();
			
			//
			// We do this in two passes, first we close the structs,
			// then the classes, because it seems the code needs it this
			// way.  If this is really what is going on, we should probably
			// make sure that we define the structs in order as well.
			//
			foreach (TypeContainer tc in type_container_resolve_order){
				if (tc.Kind == Kind.Struct && tc.Parent == tree.Types){
					tc.CloseType ();
				}
			}

			foreach (TypeContainer tc in type_container_resolve_order){
				if (!(tc.Kind == Kind.Struct && tc.Parent == tree.Types))
					tc.CloseType ();					
			}
			
			if (root.Delegates != null)
				foreach (Delegate d in root.Delegates)
					d.CloseType ();


			//
			// If we have a <PrivateImplementationDetails> class, close it
			//
			if (helper_classes != null){
				foreach (TypeBuilder type_builder in helper_classes)
					type_builder.CreateType ();
			}
			
			attribute_types = null;
			type_container_resolve_order = null;
			helper_classes = null;
			//tree = null;
			TypeManager.CleanUp ();
		}

		/// <summary>
		///   Used to register classes that need to be closed after all the
		///   user defined classes
		/// </summary>
		public static void RegisterHelperClass (TypeBuilder helper_class)
		{
			if (helper_classes == null)
				helper_classes = new ArrayList ();
			helper_classes.Add (helper_class);
		}
		
		//
		// This idea is from Felix Arrese-Igor
		//
		// Returns : the implicit parent of a composite namespace string
		//   eg. Implicit parent of A.B is A
		//
		static public string ImplicitParent (string ns)
		{
			int i = ns.LastIndexOf ('.');
			if (i < 0)
				return null;
			
			return ns.Substring (0, i);
		}

		static Type NamespaceLookup (DeclSpace ds, string name, bool silent,
					     Location loc)
		{
			//
			// Try in the current namespace and all its implicit parents
			//
			for (NamespaceEntry ns = ds.NamespaceEntry; ns != null; ns = ns.ImplicitParent) {
				object result = ns.Lookup (ds, name, silent, loc);
				if (result == null)
					continue;

				if (result is Type)
					return (Type) result;

				return null;
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

			if (ds.Cache.Contains (name)) {
				t = (Type) ds.Cache [name];
			} else {
				//
				// For the case the type we are looking for is nested within this one
				// or is in any base class
				//
				DeclSpace containing_ds = ds;
				while (containing_ds != null){
					
					// if the member cache has been created, lets use it.
					// the member cache is MUCH faster.
					if (containing_ds.MemberCache != null) {
						t = containing_ds.MemberCache.FindNestedType (name);
						if (t == null) {
							containing_ds = containing_ds.Parent;
							continue;
						}
						
						ds.Cache [name] = t;
						return t;
					}
					
					// no member cache. Do it the hard way -- reflection
					Type current_type = containing_ds.TypeBuilder;
					
					while (current_type != null &&
					       current_type != TypeManager.object_type) {
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
				
				t = NamespaceLookup (ds, name, silent, loc);
				if (!silent)
					ds.Cache [name] = t;
			}

			if (t == null && !silent)
				Report.Error (246, loc, "Cannot find type `"+name+"'");
			
			return t;
		}

		// <summary>
		//   This is the silent version of LookupType, you can use this
		//   to `probe' for a type
		// </summary>
		static public Type LookupType (TypeContainer tc, string name, Location loc)
		{
			return LookupType (tc, name, true, loc);
		}

		static void Report1530 (Location loc)
		{
			Report.Error (1530, loc, "Keyword new not allowed for namespace elements");
		}
		
		static public void PopulateCoreType (TypeContainer root, string name)
		{
			DeclSpace ds = (DeclSpace) root.GetDefinition (name);

			ds.DefineMembers (root);
			ds.Define ();
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
			
			if (type_container_resolve_order != null){
				if (RootContext.StdLib){
					foreach (TypeContainer tc in type_container_resolve_order)
						tc.DefineMembers (root);
				} else {
					foreach (TypeContainer tc in type_container_resolve_order) {
						// When compiling corlib, these types have already been
						// populated from BootCorlib_PopulateCoreTypes ().
						if (((tc.Name == "System.Object") ||
						     (tc.Name == "System.Attribute") ||
						     (tc.Name == "System.ValueType")))
						continue;

						tc.DefineMembers (root);
					}
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

			//
			// Check for cycles in the struct layout
			//
			if (type_container_resolve_order != null){
				Hashtable seen = new Hashtable ();
				foreach (TypeContainer tc in type_container_resolve_order)
					TypeManager.CheckStructCycles (tc, seen);
			}
		}

		//
		// A generic hook delegate
		//
		public delegate void Hook ();

		//
		// A hook invoked when the code has been generated.
		//
		public static event Hook EmitCodeHook;

		//
		// DefineTypes is used to fill in the members of each type.
		//
		static public void DefineTypes ()
		{
			TypeContainer root = Tree.Types;

			if (attribute_types != null)
				foreach (TypeContainer tc in attribute_types)
					tc.Define ();
			
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
						tc.Define ();
				}
			}

			ArrayList delegates = root.Delegates;
			if (delegates != null){
				foreach (Delegate d in delegates)
					if ((d.ModFlags & Modifiers.NEW) == 0)
						d.Define ();
			}

			ArrayList enums = root.Enums;
			if (enums != null){
				foreach (Enum en in enums)
					if ((en.ModFlags & Modifiers.NEW) == 0)
						en.Define ();
			}
		}

		static public void EmitCode ()
		{
			if (attribute_types != null)
				foreach (TypeContainer tc in attribute_types)
					tc.EmitType ();

			CodeGen.Assembly.Emit (Tree.Types);
			CodeGen.Module.Emit (Tree.Types);
                        
			if (Tree.Types.Enums != null) {
				foreach (Enum e in Tree.Types.Enums)
					e.Emit ();
			}

			if (type_container_resolve_order != null) {
				foreach (TypeContainer tc in type_container_resolve_order)
					tc.EmitConstants ();
				
				foreach (TypeContainer tc in type_container_resolve_order)
					tc.EmitType ();
			}
			
			if (Tree.Types.Delegates != null) {
				foreach (Delegate d in Tree.Types.Delegates)
					d.Emit ();
			}			
			//
			// Run any hooks after all the types have been defined.
			// This is used to create nested auxiliary classes for example
			//

			if (EmitCodeHook != null)
				EmitCodeHook ();
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
			
			if (impl_details_class == null){
				impl_details_class = CodeGen.Module.Builder.DefineType (
					"<PrivateImplementationDetails>",
                                        TypeAttributes.NotPublic,
                                        TypeManager.object_type);
                                
				RegisterHelperClass (impl_details_class);
			}

			fb = impl_details_class.DefineInitializedData (
				"$$field-" + (field_count++), data,
				FieldAttributes.Static | FieldAttributes.Assembly);
			
			return fb;
		}
	}
}
	      

