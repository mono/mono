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
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Principal;

namespace System.Management.Instrumentation
{
	internal sealed class WMICapabilities
	{
		private const string WMIKeyPath = "Software\\Microsoft\\WBEM";

		private const string WMINetKeyPath = "Software\\Microsoft\\WBEM\\.NET";

		private const string WMICIMOMKeyPath = "Software\\Microsoft\\WBEM\\CIMOM";

		private const string MultiIndicateSupportedValueNameVal = "MultiIndicateSupported";

		private const string AutoRecoverMofsVal = "Autorecover MOFs";

		private const string AutoRecoverMofsTimestampVal = "Autorecover MOFs timestamp";

		private const string InstallationDirectoryVal = "Installation Directory";

		private const string FrameworkSubDirectory = "Framework";

		private static RegistryKey wmiNetKey;

		private static RegistryKey wmiKey;

		private static int multiIndicateSupported;

		private static string installationDirectory;

		public static string FrameworkDirectory
		{
			get
			{
				return Path.Combine(WMICapabilities.InstallationDirectory, "Framework");
			}
		}

		public static string InstallationDirectory
		{
			get
			{
				if (WMICapabilities.installationDirectory == null && WMICapabilities.wmiKey != null)
				{
					WMICapabilities.installationDirectory = WMICapabilities.wmiKey.GetValue("Installation Directory").ToString();
				}
				return WMICapabilities.installationDirectory;
			}
		}

		public static bool MultiIndicateSupported
		{
			get
			{
				int num;
				if (OSHelper.IsUnix) return true;
				if (-1 == WMICapabilities.multiIndicateSupported)
				{
					if (WMICapabilities.MultiIndicatePossible())
					{
						num = 1;
					}
					else
					{
						num = 0;
					}
					WMICapabilities.multiIndicateSupported = num;
					if (WMICapabilities.wmiNetKey != null)
					{
						object value = WMICapabilities.wmiNetKey.GetValue("MultiIndicateSupported", WMICapabilities.multiIndicateSupported);
						if (value.GetType() == typeof(int) && (int)value == 1)
						{
							WMICapabilities.multiIndicateSupported = 1;
						}
					}
				}
				return WMICapabilities.multiIndicateSupported == 1;
			}
		}

		static WMICapabilities()
		{
			WMICapabilities.multiIndicateSupported = -1;
			WMICapabilities.installationDirectory = null;
			if (OSHelper.IsUnix) return;
			WMICapabilities.wmiNetKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\WBEM\\.NET", false);
			WMICapabilities.wmiKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\WBEM", false);
		}

		public WMICapabilities()
		{

		}

		public static void AddAutorecoverMof(string path)
		{
			if (OSHelper.IsUnix) return;
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\WBEM\\CIMOM", true);
			if (registryKey != null)
			{
				object value = registryKey.GetValue("Autorecover MOFs");
				string[] strArrays = value as string[];
				if (strArrays == null)
				{
					if (value == null)
					{
						strArrays = new string[0];
					}
					else
					{
						return;
					}
				}
				DateTime now = DateTime.Now;
				long fileTime = now.ToFileTime();
				registryKey.SetValue("Autorecover MOFs timestamp", fileTime.ToString((IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(long))));
				string[] strArrays1 = strArrays;
				int num = 0;
				while (num < (int)strArrays1.Length)
				{
					string str = strArrays1[num];
					if (string.Compare(str, path, StringComparison.OrdinalIgnoreCase) != 0)
					{
						num++;
					}
					else
					{
						return;
					}
				}
				string[] strArrays2 = new string[(int)strArrays.Length + 1];
				strArrays.CopyTo(strArrays2, 0);
				strArrays2[(int)strArrays2.Length - 1] = path;
				registryKey.SetValue("Autorecover MOFs", strArrays2);
				DateTime dateTime = DateTime.Now;
				long fileTime1 = dateTime.ToFileTime();
				registryKey.SetValue("Autorecover MOFs timestamp", fileTime1.ToString((IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(long))));
			}
		}

		private static bool IsNovaFile(FileVersionInfo info)
		{
			if (info.FileMajorPart != 1 || info.FileMinorPart != 50)
			{
				return false;
			}
			else
			{
				return info.FileBuildPart == 0x43d;
			}
		}

		public static bool IsUserAdmin()
		{
			if (OSHelper.IsWindows)
			{
				WindowsPrincipal windowsPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
				if (!windowsPrincipal.Identity.IsAuthenticated)
				{
					return false;
				}
				else
				{
					return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
				}
			}
			else
			{
				return true;
			}
		}

		public static bool IsWindowsXPOrHigher()
		{
			OperatingSystem oSVersion = Environment.OSVersion;
			if (oSVersion.Platform != PlatformID.Win32NT || !(oSVersion.Version >= new Version(5, 1)))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		private static bool MultiIndicatePossible()
		{
			OperatingSystem oSVersion = Environment.OSVersion;
			if (oSVersion.Platform != PlatformID.Win32NT || !(oSVersion.Version >= new Version(5, 1)))
			{
				string str = Path.Combine(Environment.SystemDirectory, "wbem\\fastprox.dll");
				FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(str);
				if (!WMICapabilities.IsNovaFile(versionInfo) || versionInfo.FilePrivatePart < 56)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return true;
			}
		}
	}
}