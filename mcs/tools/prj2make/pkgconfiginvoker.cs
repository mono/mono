// created on 6/8/2004 at 5:44 AM
using System;
using System.Diagnostics;

namespace Mfconsulting.General
{
	public sealed class PkgConfigInvoker
	{
		
		public static string GetPkgConfigVersion()
		{
			string pkgout = null;

			pkgout = RunPkgConfig("--version");

			if(pkgout != null)
			{
				return pkgout;
			}

			return null;
		}

		public static string GetPkgVariableValue(string strPkg, string strVarName)
		{
			string pkgout = null;

			pkgout = RunPkgConfig(String.Format("--variable={0} {1}", 
				strVarName, strPkg));

			if(pkgout != null)
			{
				return pkgout;
			}

			return null;
		}

		public static string GetPkgConfigModuleVersion(string strPkg)
		{
			string pkgout = null;

			pkgout = RunPkgConfig(String.Format("--modversion {0}", strPkg));

			if(pkgout != null)
			{
				return pkgout;
			}

			return null;
		}

		public static string RunPkgConfig(string strArgLine)
		{
			string pkgout;

			ProcessStartInfo pi = new ProcessStartInfo ();
			pi.FileName = "pkg-config";
			pi.RedirectStandardOutput = true;
			pi.UseShellExecute = false;
			pi.Arguments = strArgLine;
			Process p = null;
			try 
			{
				p = Process.Start (pi);
			} 
			catch (Exception e) 
			{
				Console.WriteLine("Couldn't run pkg-config: " + e.Message);
				return null;
			}

			if (p.StandardOutput == null)
			{
				Console.WriteLine("Specified package did not return any information");
			}
			
			pkgout = p.StandardOutput.ReadToEnd ();		
			p.WaitForExit ();
			if (p.ExitCode != 0) 
			{
				Console.WriteLine("Error running pkg-config. Check the above output.");
				return null;
			}

			if (pkgout != null)
			{
				p.Close ();
				return pkgout;
			}

			p.Close ();

			return null;
		}
	}
}