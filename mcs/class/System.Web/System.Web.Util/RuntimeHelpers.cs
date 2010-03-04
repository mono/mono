//
// System.Web.Util.RuntimeHelpers
//
// Authors:
//      Marek Habersack (mhabersack@novell.com)
//
// (C) 2006-2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Web.Configuration;

namespace System.Web.Util
{
	static class RuntimeHelpers
	{
		public static bool CaseInsensitive {
			get; private set;
		}

		public static bool DebuggingEnabled {
			get {
				CompilationSection cs = WebConfigurationManager.GetSection ("system.web/compilation") as CompilationSection;
				if (cs != null)
					return cs.Debug;

				return false;
			}
		}

		public static IEqualityComparer <string> StringEqualityComparer {
			get; private set;
		}

		public static IEqualityComparer <string> StringEqualityComparerCulture {
			get; private set;
		}
		
		public static bool IsUncShare {
			get; private set;
		}

		public static string MonoVersion {
			get; private set;
		}

		public static bool RunningOnWindows {
			get; private set;
		}

		public static StringComparison StringComparison {
			get; private set;
		}

		public static StringComparison StringComparisonCulture {
			get; private set;
		}
		
		static RuntimeHelpers ()
		{
			PlatformID pid = Environment.OSVersion.Platform;
			RunningOnWindows = ((int) pid != 128 && pid != PlatformID.Unix && pid != PlatformID.MacOSX);

			if (RunningOnWindows) {
				CaseInsensitive = true;
				string appDomainAppPath = AppDomain.CurrentDomain.GetData (".appPath") as string;
				if (!String.IsNullOrEmpty (appDomainAppPath)) {
					try {
						IsUncShare = new Uri (appDomainAppPath).IsUnc;
					} catch {
						// ignore
					}
				}
			} else {
				string mono_iomap = Environment.GetEnvironmentVariable ("MONO_IOMAP");
				if (!String.IsNullOrEmpty (mono_iomap)) {
					if (mono_iomap == "all")
						CaseInsensitive = true;
					else {
						string[] parts = mono_iomap.Split (':');
						foreach (string p in parts) {
							if (p == "all" || p == "case") {
								CaseInsensitive = true;
								break;
							}
						}
					}
				}
			}

			if (CaseInsensitive) {
				StringEqualityComparer = StringComparer.OrdinalIgnoreCase;
				StringEqualityComparerCulture = StringComparer.CurrentCultureIgnoreCase;
				StringComparison = StringComparison.OrdinalIgnoreCase;
				StringComparisonCulture = StringComparison.CurrentCultureIgnoreCase;
			} else {
				StringEqualityComparer = StringComparer.Ordinal;
				StringEqualityComparerCulture = StringComparer.CurrentCulture;
				StringComparison = StringComparison.Ordinal;
				StringComparisonCulture = StringComparison.CurrentCulture;
			}
			
			string monoVersion = null;
			try {
				Type monoRuntime = Type.GetType ("Mono.Runtime", false);
				if (monoRuntime != null) {
					MethodInfo mi = monoRuntime.GetMethod ("GetDisplayName", BindingFlags.Static | BindingFlags.NonPublic);
					if (mi != null)
						monoVersion = mi.Invoke (null, new object [0]) as string;
				}
			} catch {
				// ignore
			}
			
			if (monoVersion == null)
				monoVersion = Environment.Version.ToString ();

			MonoVersion = monoVersion;
		}
	}
}
