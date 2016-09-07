using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System {

	internal static class MonoExeLocator {

		public static string GacPath { get; private set; }
		public static string MonoPath { get; private set; }
		public static string McsPath { get; private set; }
		public static string VbncPath { get; private set; }
		public static string AlPath { get; private set; }

		static MonoExeLocator () {

			PropertyInfo gac = typeof (Environment).GetProperty ("GacPath", BindingFlags.Static | BindingFlags.NonPublic);
			MethodInfo getGacMethod = gac.GetGetMethod (true);
			GacPath = Path.GetDirectoryName ((string) getGacMethod.Invoke (null, null));

			string monoPath = null;
			string mcsPath = null;
			string vbncPath = null;
			string alPath = null;

			if (Path.DirectorySeparatorChar == '\\') {
				string processExe = Process.GetCurrentProcess ().MainModule.FileName;
				if (processExe != null) {
					string fileName = Path.GetFileName (processExe);
					if (fileName.StartsWith ("mono") && fileName.EndsWith (".exe"))
						monoPath = processExe;
				}

				if (!File.Exists (monoPath))
					monoPath = Path.Combine (
						Path.GetDirectoryName (
							Path.GetDirectoryName (GacPath)),
						"bin\\mono.exe");

				if (!File.Exists (monoPath))
					monoPath = Path.Combine (
						Path.GetDirectoryName (
							Path.GetDirectoryName (
								Path.GetDirectoryName (GacPath))),
						"mono\\mini\\mono.exe");

				if (!File.Exists (monoPath))
					throw new FileNotFoundException ("Windows mono path not found: " + monoPath);

				mcsPath = Path.Combine (GacPath, "4.5\\mcs.exe");
				if (!File.Exists (mcsPath))
					mcsPath = Path.Combine (Path.GetDirectoryName (GacPath), "lib\\build\\mcs.exe");

				if (!File.Exists (mcsPath))
					throw new FileNotFoundException ("Windows mcs path not found: " + mcsPath);

				vbncPath = Path.Combine (GacPath,  "4.5\\vbnc.exe");
				vbncPath = Path.Combine (GacPath,  "4.5\\vbnc.exe");
				alPath = Path.Combine (GacPath, "4.5\\al.exe");

				if (!File.Exists (alPath)) {
					alPath = Path.Combine (Path.GetDirectoryName (GacPath), "lib\\net_4_x\\al.exe");
					if (!File.Exists (alPath))
						throw new FileNotFoundException ("Windows al path not found: " + alPath);
				}
			} else {
				monoPath = Path.Combine (GacPath, "bin/mono");
				if (!File.Exists (MonoPath))
					monoPath = "mono";

				var mscorlibPath = new Uri (typeof (object).Assembly.CodeBase).LocalPath;
				mcsPath = Path.GetFullPath( Path.Combine (mscorlibPath, "..", "..", "..", "..", "bin", "mcs"));
				if (!File.Exists (mcsPath))
					mcsPath = "mcs";

				vbncPath = Path.Combine (Path.GetDirectoryName (mcsPath), "vbnc");
				if (!File.Exists (vbncPath))
					vbncPath = "vbnc";

				alPath = "al";
			}

			McsPath = mcsPath;
			MonoPath = monoPath;
			VbncPath = vbncPath;
			AlPath = alPath;
		}

		
	}
}
