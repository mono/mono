#if !MOBILE || MOBILE_DESKTOP_HOST

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace System {

	static class MonoToolsLocator
	{
		public static readonly string Mono;
		public static readonly string McsCSharpCompiler;
		public static readonly string VBCompiler;
		public static readonly string AssemblyLinker;

		// TODO: Should be lazy
		static MonoToolsLocator ()
		{
			var gac = typeof (Environment).GetProperty ("GacPath", BindingFlags.Static | BindingFlags.NonPublic);
			var getGacMethod = gac.GetGetMethod (true);
			var GacPath = Path.GetDirectoryName ((string) getGacMethod.Invoke (null, null));

			if (Path.DirectorySeparatorChar == '\\') {
				StringBuilder moduleName = new StringBuilder (1024);
				GetModuleFileName (IntPtr.Zero, moduleName, moduleName.Capacity);
				string processExe = moduleName.ToString ();
				string fileName = Path.GetFileName (processExe);
				if (fileName.StartsWith ("mono") && fileName.EndsWith (".exe"))
					Mono = processExe;

				if (!File.Exists (Mono))
					Mono = Path.Combine (
						Path.GetDirectoryName (
							Path.GetDirectoryName (GacPath)),
						"bin\\mono.exe");

				if (!File.Exists (Mono))
					Mono = Path.Combine (
						Path.GetDirectoryName (
							Path.GetDirectoryName (
								Path.GetDirectoryName (GacPath))),
						"mono\\mini\\mono.exe");

				//if (!File.Exists (Mono))
				//	throw new FileNotFoundException ("Windows mono path not found: " + Mono);

				McsCSharpCompiler = Path.Combine (GacPath, "4.5", "mcs.exe");
				if (!File.Exists (McsCSharpCompiler)) {
					// Starting from mono\mcs\class
					McsCSharpCompiler = Path.Combine (Path.GetDirectoryName (GacPath), "lib", "net_4_x", "mcs.exe");
				}

				//if (!File.Exists (CSharpCompiler))
				//	throw new FileNotFoundException ("C# compiler not found at " + CSharpCompiler);

				VBCompiler = Path.Combine (GacPath,  "4.5\\vbnc.exe");
				AssemblyLinker = Path.Combine (GacPath, "4.5\\al.exe");

				if (!File.Exists (AssemblyLinker)) {
					AssemblyLinker = Path.Combine (Path.GetDirectoryName (GacPath), "lib\\net_4_x\\al.exe");
				//	if (!File.Exists (AssemblyLinker))
				//		throw new FileNotFoundException ("Windows al path not found: " + AssemblyLinker);
				}
			} else {
				Mono = Path.Combine (GacPath, "bin", "mono");
				if (!File.Exists (Mono))
					Mono = "mono";

				var mscorlibPath = new Uri (typeof (object).Assembly.CodeBase).LocalPath;
				McsCSharpCompiler = Path.GetFullPath (Path.Combine (mscorlibPath, "..", "..", "..", "..", "bin", "mcs"));
				if (!File.Exists (McsCSharpCompiler))
					McsCSharpCompiler = "mcs";

				VBCompiler = Path.GetFullPath (Path.Combine (mscorlibPath, "..", "..", "..", "..", "bin", "vbnc"));
				if (!File.Exists (VBCompiler))
					VBCompiler = "vbnc";

				AssemblyLinker = Path.GetFullPath (Path.Combine (mscorlibPath, "..", "..", "..", "..", "bin", "al"));
				if (!File.Exists (AssemblyLinker))
					AssemblyLinker = "al";
			}
		}

		// Due to an issue with shadow copying  and app domains in mono, we cannot currently use 
		// Process.GetCurrentProcess ().MainModule.FileName (which would give the same result)
		// when running in an AppDomain (eg System.Web hosts).
		//
		// Using native Windows API to get current process filename. This will only
		// be called when running on Windows. 
		[DllImport ("kernel32.dll")]
		static extern uint GetModuleFileName ([In] IntPtr hModule, [Out] StringBuilder lpFilename, [In] int nSize);

	}
}

#endif
