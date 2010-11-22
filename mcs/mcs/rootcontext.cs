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

		static public string Win32ResourceFile;
		static public string Win32IconFile;

		//
		// A list of resource files for embedding
		//
		static public  List<AssemblyResource> Resources;

		static public bool GenerateDebugInfo;

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
		static List<string> AllDefines;

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
			if (!full)
				return;
			
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
			GenerateDebugInfo = false;
			Win32IconFile = null;
			Win32ResourceFile = null;
			Resources = null;

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

		static public ModuleContainer ToplevelTypes {
			get { return root; }
			set { root = value; }
		}
	}
}
