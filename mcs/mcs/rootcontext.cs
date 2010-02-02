//
// rootcontext.cs: keeps track of our tree representation, and assemblies loaded.
//
// Author: Miguel de Icaza (miguel@ximian.com)
//            Ravi Pratap  (ravi@ximian.com)
//            Marek Safar  (marek.safar@gmail.com)
//
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001 Ximian, Inc (http://www.ximian.com)
// Copyright 2004-2008 Novell, Inc

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace Mono.CSharp {

	public enum LanguageVersion
	{
		ISO_1		= 1,
		ISO_2		= 2,
		V_3			= 3,
		V_4			= 4,
		Future		= 100,

		Default		= LanguageVersion.V_4,
	}

	public enum MetadataVersion
	{
		v1,
		v2,
		v4
	}

	public class RootContext {

		//
		// COMPILER OPTIONS CLASS
		//
		public static Target Target;
#if GMCS_SOURCE
		public static Platform Platform;
#endif
		public static string TargetExt;
		public static bool VerifyClsCompliance = true;
		public static bool Optimize = true;
		public static LanguageVersion Version;
		public static bool EnhancedWarnings;

		public static MetadataVersion MetadataCompatibilityVersion;

		//
		// We keep strongname related info here because
		// it's also used as complier options from CSC 8.x
		//
		public static string StrongNameKeyFile;
		public static string StrongNameKeyContainer;
		public static bool StrongNameDelaySign;

		//
		// If set, enable XML documentation generation
		//
		public static Documentation Documentation;

		static public string MainClass;

		// 
		// The default compiler checked state
		//
		static public bool Checked;

		//
		// If true, it means that the compiler is executing as
		// in eval mode so unresolved variables are resolved in
		// static classes maintained by the eval engine.
		//
		static public bool EvalMode;

		//
		// If true, the compiler is operating in statement mode,
		// this currently turns local variable declaration into
		// static variables of a class
		//
		static public bool StatementMode;
		
		//
		// Whether to allow Unsafe code
		//
		static public bool Unsafe;

		//
		// Whether we are being linked against the standard libraries.
		// This is only used to tell whether `System.Object' should
		// have a base class or not.
		//
		public static bool StdLib;

		public static bool NeedsEntryPoint {
			get { return Target == Target.Exe || Target == Target.WinExe; }
		}

		//
		// COMPILER OPTIONS CLASS END
		//

		//
		// Contains the parsed tree
		//
		static ModuleContainer root;

		//
		// This hashtable contains all of the #definitions across the source code
		// it is used by the ConditionalAttribute handler.
		//
		static ArrayList AllDefines;
		
		//
		// This keeps track of the order in which classes were defined
		// so that we can poulate them in that order.
		//
		// Order is important, because we need to be able to tell, by
		// examining the list of methods of the base class, which ones are virtual
		// or abstract as well as the parent names (to implement new, 
		// override).
		//
		static ArrayList type_container_resolve_order;

		//
		// Holds a reference to the Private Implementation Details
		// class.
		//
		static ArrayList helper_classes;
		
		static TypeBuilder impl_details_class;

		//
		// Constructor
		//
		static RootContext ()
		{
			Reset (true);
		}

		public static void PartialReset ()
		{
			Reset (false);
		}
		
		public static void Reset (bool full)
		{
			if (full)
				root = null;
			
			type_container_resolve_order = new ArrayList ();
			EntryPoint = null;
			Checked = false;
			Unsafe = false;
			StdLib = true;
			StrongNameKeyFile = null;
			StrongNameKeyContainer = null;
			StrongNameDelaySign = false;
			MainClass = null;
			Target = Target.Exe;
			TargetExt = ".exe";
#if GMCS_SOURCE
			Platform = Platform.AnyCPU;
#endif
			Version = LanguageVersion.Default;
			Documentation = null;
			impl_details_class = null;
			helper_classes = null;

#if GMCS_SOURCE
			MetadataCompatibilityVersion = MetadataVersion.v2;
#else
			MetadataCompatibilityVersion = MetadataVersion.v1;
#endif

			//
			// Setup default defines
			//
			AllDefines = new ArrayList ();
			AddConditional ("__MonoCS__");
		}

		public static void AddConditional (string p)
		{
			if (AllDefines.Contains (p))
				return;
			AllDefines.Add (p);
		}

		public static bool IsConditionalDefined (string value)
		{
			return AllDefines.Contains (value);
		}

		static public ModuleContainer ToplevelTypes {
			get { return root; }
			set { root = value; }
		}

		public static void RegisterOrder (TypeContainer tc)
		{
			type_container_resolve_order.Add (tc);
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
			root.Resolve ();

			//
			// Interfaces are processed next, as classes and
			// structs might inherit from an object or implement
			// a set of interfaces, we need to be able to tell
			// them appart by just using the TypeManager.
			//
			foreach (TypeContainer tc in root.Types)
				tc.CreateType ();

			foreach (TypeContainer tc in root.Types)
				tc.DefineType ();

			if (root.Delegates != null)
				foreach (Delegate d in root.Delegates) 
					d.DefineType ();
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
			//
			// We do this in two passes, first we close the structs,
			// then the classes, because it seems the code needs it this
			// way.  If this is really what is going on, we should probably
			// make sure that we define the structs in order as well.
			//
			foreach (TypeContainer tc in type_container_resolve_order){
				if (tc.Kind == Kind.Struct && tc.Parent == root){
					tc.CloseType ();
				}
			}

			foreach (TypeContainer tc in type_container_resolve_order){
				if (!(tc.Kind == Kind.Struct && tc.Parent == root))
					tc.CloseType ();					
			}
			
			if (root.Delegates != null)
				foreach (Delegate d in root.Delegates)
					d.CloseType ();


			//
			// If we have a <PrivateImplementationDetails> class, close it
			//
			if (helper_classes != null){
				foreach (TypeBuilder type_builder in helper_classes) {
					PredefinedAttributes.Get.CompilerGenerated.EmitAttribute (type_builder);
					type_builder.CreateType ();
				}
			}
			
			type_container_resolve_order = null;
			helper_classes = null;
			//root = null;
			TypeManager.CleanUp ();
		}

		/// <summary>
		///   Used to register classes that need to be closed after all the
		///   user defined classes
		/// </summary>
		public static void RegisterCompilerGeneratedType (TypeBuilder helper_class)
		{
			if (helper_classes == null)
				helper_classes = new ArrayList ();

			helper_classes.Add (helper_class);
		}
		
		static public void PopulateCoreType (TypeContainer root, string name)
		{
			DeclSpace ds = (DeclSpace) root.GetDefinition (name);
			// Core type was imported
			if (ds == null)
				return;

			ds.Define ();
		}
		
		static public void BootCorlib_PopulateCoreTypes ()
		{
			PopulateCoreType (root, "System.Object");
			PopulateCoreType (root, "System.ValueType");
			PopulateCoreType (root, "System.Attribute");
			PopulateCoreType (root, "System.Runtime.CompilerServices.IndexerNameAttribute");
		}
		
		// <summary>
		//   Populates the structs and classes with fields and methods
		// </summary>
		//
		// This is invoked after all interfaces, structs and classes
		// have been defined through `ResolveTree' 
		static public void PopulateTypes ()
		{

			if (type_container_resolve_order != null){
				foreach (TypeContainer tc in type_container_resolve_order)
					tc.ResolveType ();
				foreach (TypeContainer tc in type_container_resolve_order) {
					try {
						tc.Define ();
					} catch (Exception e) {
						throw new InternalErrorException (tc, e);
					}
				}
			}

			ArrayList delegates = root.Delegates;
			if (delegates != null){
				foreach (Delegate d in delegates)
					d.Define ();
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

		static public void EmitCode ()
		{
			if (type_container_resolve_order != null) {
				foreach (TypeContainer tc in type_container_resolve_order)
					tc.EmitType ();

				if (RootContext.ToplevelTypes.Compiler.Report.Errors > 0)
					return;

				foreach (TypeContainer tc in type_container_resolve_order)
					tc.VerifyMembers ();
			}
			
			if (root.Delegates != null) {
				foreach (Delegate d in root.Delegates)
					d.Emit ();
			}			

			CodeGen.Assembly.Emit (root);
			root.Emit ();
		}
		
		//
		// Public Field, used to track which method is the public entry
		// point.
		//
		static public Method EntryPoint;

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
				impl_details_class = ToplevelTypes.Builder.DefineType (
					"<PrivateImplementationDetails>",
                                        TypeAttributes.NotPublic,
                                        TypeManager.object_type);
                                
				RegisterCompilerGeneratedType (impl_details_class);
			}

			fb = impl_details_class.DefineInitializedData (
				"$$field-" + (field_count++), data,
				FieldAttributes.Static | FieldAttributes.Assembly);
			
			return fb;
		}

		public static void CheckUnsafeOption (Location loc, Report Report)
		{
			if (!Unsafe) {
				Report.Error (227, loc, 
					"Unsafe code requires the `unsafe' command line option to be specified");
			}
		}
	}
}
