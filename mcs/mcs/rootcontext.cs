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
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace Mono.CSharp {

	public enum LanguageVersion
	{
		ISO_1		= 1,
		ISO_2		= 2,
		V_3		= 3,
		V_4		= 4,
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
		public static Platform Platform;
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
		static ModuleCompiled root;

		//
		// This hashtable contains all of the #definitions across the source code
		// it is used by the ConditionalAttribute handler.
		//
		static List<string> AllDefines;
		
		//
		// Holds a reference to the Private Implementation Details
		// class.
		//
		static List<TypeBuilder> helper_classes;
		
		static TypeBuilder impl_details_class;

		public static List<Enum> hack_corlib_enums = new List<Enum> ();

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
			impl_details_class = null;
			helper_classes = null;

			if (!full)
				return;
			
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
			Platform = Platform.AnyCPU;
			Version = LanguageVersion.Default;
			Documentation = null;
			impl_details_class = null;
			helper_classes = null;

#if NET_4_0
			MetadataCompatibilityVersion = MetadataVersion.v4;
#else
			MetadataCompatibilityVersion = MetadataVersion.v2;
#endif

			//
			// Setup default defines
			//
			AllDefines = new List<string> ();
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

		static public ModuleCompiled ToplevelTypes {
			get { return root; }
			set { root = value; }
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
		}

		static void HackCorlib ()
		{
			if (StdLib)
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
				root.Compiler.Report.Warning (-26, 3, "The compilation may fail due to missing `{0}.SetCorlibTypeBuilders(...)' method",
					typeof (System.Reflection.Emit.AssemblyBuilder).FullName);
				return;
			}

			object[] args = new object[4];
			args[0] = TypeManager.object_type.GetMetaInfo ();
			args[1] = TypeManager.value_type.GetMetaInfo ();
			args[2] = TypeManager.enum_type.GetMetaInfo ();
			args[3] = TypeManager.void_type.GetMetaInfo ();
			set_corlib_type_builders.Invoke (CodeGen.Assembly.Builder, args);

			// Another Mono corlib HACK
			// mono_class_layout_fields requires to have enums created
			// before creating a class which used the enum for any of its fields
			foreach (var e in hack_corlib_enums)
				e.CloseType ();
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
			HackCorlib ();

			foreach (TypeContainer tc in root.Types){
				tc.CloseType ();
			}

			if (root.CompilerGeneratedClasses != null)
				foreach (CompilerGeneratedClass c in root.CompilerGeneratedClasses)
					c.CloseType ();

			//
			// If we have a <PrivateImplementationDetails> class, close it
			//
			if (helper_classes != null){
				foreach (TypeBuilder type_builder in helper_classes) {
					PredefinedAttributes.Get.CompilerGenerated.EmitAttribute (type_builder);
					type_builder.CreateType ();
				}
			}
			
			helper_classes = null;
		}

		/// <summary>
		///   Used to register classes that need to be closed after all the
		///   user defined classes
		/// </summary>
		public static void RegisterCompilerGeneratedType (TypeBuilder helper_class)
		{
			if (helper_classes == null)
				helper_classes = new List<TypeBuilder> ();

			helper_classes.Add (helper_class);
		}
		
		// <summary>
		//   Populates the structs and classes with fields and methods
		// </summary>
		//
		// This is invoked after all interfaces, structs and classes
		// have been defined through `ResolveTree' 
		static public void PopulateTypes ()
		{
			foreach (TypeContainer tc in ToplevelTypes.Types)
				tc.ResolveTypeParameters ();

			foreach (TypeContainer tc in ToplevelTypes.Types) {
				try {
					tc.Define ();
				} catch (Exception e) {
					throw new InternalErrorException (tc, e);
				}
			}
		}

		static public void EmitCode ()
		{
			foreach (var tc in ToplevelTypes.Types)
				tc.DefineConstants ();

			foreach (TypeContainer tc in ToplevelTypes.Types)
				tc.EmitType ();

			if (ToplevelTypes.Compiler.Report.Errors > 0)
				return;

			foreach (TypeContainer tc in ToplevelTypes.Types)
				tc.VerifyMembers ();

			if (root.CompilerGeneratedClasses != null)
				foreach (CompilerGeneratedClass c in root.CompilerGeneratedClasses)
					c.EmitType ();

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
                                        TypeManager.object_type.GetMetaInfo ());
                                
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
