using System;
using System.Web.Hosting;
using System.IO;

namespace dumb2
{
	class Program
	{
		static void SetupAppHost (string baseDir)
		{
			if (File.Exists (baseDir))
				File.Delete (baseDir);
			Console.Write ("App base: ");
			Console.WriteLine (baseDir);
			Directory.CreateDirectory (baseDir);
			string binDir = Path.Combine (baseDir, "bin");
			Directory.CreateDirectory (binDir);
			foreach (System.Reflection.Assembly a in AppDomain.CurrentDomain.GetAssemblies ()) {
				if (a.GlobalAssemblyCache) continue;
				string loc = a.ManifestModule.FullyQualifiedName;
				if (loc.EndsWith (".exe", true, System.Globalization.CultureInfo.CurrentCulture))
					continue;
				string fn = Path.GetFileName (loc);
				File.Copy (loc, Path.Combine (binDir, fn));
			}
		}

		static void Main (string[] args)
		{
			string baseDir1 = Path.GetTempFileName ();
			SetupAppHost (baseDir1);
			ClassLib.Host h1 = (ClassLib.Host) ApplicationHost.CreateApplicationHost (
				typeof (ClassLib.Host), "/test", baseDir1);
			h1.Run ();

			string baseDir2 = Path.GetTempFileName ();
			SetupAppHost (baseDir2);
			FileStream fs = new FileStream (Path.Combine (baseDir2, "Web.config"), FileMode.CreateNew);
			StreamWriter sw = new StreamWriter (fs);
			sw.Write ("<?xml version=\"1.0\"?><configuration><system.web><pages styleSheetTheme=\"White\"/></system.web></configuration>");
			sw.Close ();
			ClassLib.Host h2 = (ClassLib.Host) ApplicationHost.CreateApplicationHost (
				typeof (ClassLib.Host), "/test", baseDir2);
			h2.Run ();

		}
	}
}
