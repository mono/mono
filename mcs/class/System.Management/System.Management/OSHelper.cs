//
// AssemblyRef
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace System.Management
{
	internal static class OSHelper
	{
		public static PlatformID Platform {
			get { return Environment.OSVersion.Platform; }
		}

		public static bool IsUnix 
		{
			get { return Platform == PlatformID.MacOSX || Platform == PlatformID.Unix; }
		}

		public static bool IsWindows {
			get { return !IsUnix; }
		}

		public static bool IsMacOSX {
			get { return IsUnix && Kernel == KernelVersion.MacOSX; }
		}

		private static KernelVersion _kernel;

		public static KernelVersion Kernel {
			get {
				if (_kernel == KernelVersion.Undefined)
				{
					var str = DetectUnixKernel ();
					switch(str) {
						case "Linux":
							_kernel = KernelVersion.Linux;
							break;
						case "FreeBSD":
							_kernel = KernelVersion.FreeBSD;
							break;
						case "Darwin":
							_kernel = KernelVersion.MacOSX;
							break;

						default:
							_kernel = KernelVersion.Unix;
							break;
					}
				}
				return _kernel;
			}
		}

		#region private static string DetectUnixKernel()
		 
         [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
         struct utsname
         {
	             [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	             public string sysname;
	 
	             [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	             public string nodename;
	 
	             [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	             public string release;
	 
	             [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	             public string version;
	 
	             [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	             public string machine;
	 
	             [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
	             public string extraJustInCase;
	 
         }

		internal class OSInformation
		{
			public string ProductName { get; set; }

			public string Version { get; set; }

			public string BuildNumber { get; set; }

			public override string ToString ()
			{
				return string.Format ("{0} {1} {2}", ProductName, Version, BuildNumber);
			}
		}

		private static OSInformation _osInfo;

		public static OSInformation GetComputerDescription ()
		{
			if (_osInfo == null) {
				OSInformation info = new OSInformation ();
				if (OSHelper.IsUnix) {
					if (OSHelper.IsMacOSX) {
						string[] resultLines = new string[3];
						int i = 0;
						var versionProcess = Process.Start (new ProcessStartInfo ("bash", "-c \"sw_vers\"") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true, RedirectStandardInput = true }); 
						versionProcess.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
							if (i >= resultLines.Length)
								return;
							resultLines [i] += e.Data;
							i++;
						};
						versionProcess.BeginOutputReadLine ();
						versionProcess.WaitForExit ();
						info.ProductName = FixSwVersString (resultLines [0]);
						info.Version = FixSwVersString (resultLines [1]);
						info.BuildNumber = FixSwVersString (resultLines [2]);
					} else {
						info.ProductName = "UNIX";
						info.Version = "Undefined Version";
						/* TODO: Complete with uname */
					}
				} else {
					//Get Operating system information.
					OperatingSystem os = Environment.OSVersion;
					//Get version information about the os.
					Version vs = os.Version;
					
					//Variable to hold our return value
					string operatingSystem = "";
					info.ProductName = "Windows";
					if (os.Platform == PlatformID.Win32Windows) {
						//This is a pre-NT version of Windows
						switch (vs.Minor) {
						case 0:
							operatingSystem = "95";
							break;
						case 10:
							if (vs.Revision.ToString () == "2222A")
								operatingSystem = "98SE";
							else
								operatingSystem = "98";
							break;
						case 90:
							operatingSystem = "Me";
							break;
						default:
							break;
						}
					} else if (os.Platform == PlatformID.Win32NT) {
						switch (vs.Major) {
						case 3:
							operatingSystem = "NT 3.51";
							break;
						case 4:
							operatingSystem = "NT 4.0";
							break;
						case 5:
							if (vs.Minor == 0)
								operatingSystem = "2000";
							else
								operatingSystem = "XP";
							break;
						case 6:
							if (vs.Minor == 0)
								operatingSystem = "Vista";
							else if (vs.Minor == 1)
								operatingSystem = "7";
							else
								operatingSystem = "8";
							break;
						default:
							break;
						}
					}
					//Make sure we actually got something in our OS check
					//We don't want to just return " Service Pack 2" or " 32-bit"
					//That information is useless without the OS version.
					if (operatingSystem != "") {
						//Got something.  Let's prepend "Windows" and get more info.
						info.Version = operatingSystem;
						//See if there's a service pack installed.
						if (os.ServicePack != "") {
							//Append it to the OS name.  i.e. "Windows XP Service Pack 3"
							info.BuildNumber = os.ServicePack;
						}
						//Append the OS architecture.  i.e. "Windows XP Service Pack 3 32-bit"
						//operatingSystem += " " + getOSArchitecture().ToString() + "-bit";
					}
				}
				_osInfo = info;
			}
			return _osInfo;
		}

		private static string FixSwVersString (string s)
		{
			int index = s.IndexOf (":");
			if (index != -1) s = s.Substring (index +1);
			return s.Trim ();
		}
		 
         private static string DetectUnixKernel()
         {
	             utsname uts = new utsname();
	             uname(out uts);
	 			 /*
		         Debug.WriteLine("System:");
	             Debug.Indent();
	             Debug.WriteLine(uts.sysname);
	             Debug.WriteLine(uts.nodename);
	             Debug.WriteLine(uts.release);
	             Debug.WriteLine(uts.version);
	             Debug.WriteLine(uts.machine);
	             Debug.Unindent();
	             */
	 
	            return uts.sysname.ToString();
		}
	
     	[DllImport("libc")]
        private static extern void uname(out utsname uname_struct);
		 
         #endregion

		public enum KernelVersion
		{
			Undefined,
			Linux,
			FreeBSD,
			Unix,
			MacOSX
		}
	}


}
