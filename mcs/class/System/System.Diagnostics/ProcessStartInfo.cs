//
// System.Diagnostics.ProcessStartInfo.cs
//
// Authors:
//   Dick Porter (dick@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
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
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Runtime.InteropServices;

namespace System.Diagnostics 
{
	[StructLayout (LayoutKind.Sequential)]
	public sealed partial class ProcessStartInfo 
	{
		internal bool HaveEnvVars {
			get { return (environmentVariables != null); }
		}

		Collection<string> _argumentList;

		public Collection<string> ArgumentList {
			get {
				if (_argumentList == null) {
					_argumentList = new Collection<string>();
				}
				return _argumentList;
			}
		}

		public Encoding StandardInputEncoding { get; set; }

		static readonly string [] empty = new string [0];

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		public string[] Verbs {
			get {
#if MOBILE
				return empty;
#else
				switch (System.Environment.OSVersion.Platform) {
				case (PlatformID)4:
				case (PlatformID)6:
				case (PlatformID)128:
					return empty; // no verb on non-Windows
				default:
					string ext = String.IsNullOrEmpty (fileName) ? null : Path.GetExtension (fileName);
					if (ext == null)
						return empty;

					RegistryKey rk = null, rk2 = null, rk3 = null;
					try {
						rk = Registry.ClassesRoot.OpenSubKey (ext);
						string k = rk != null ? rk.GetValue (null) as string : null;
						rk2 = k != null ? Registry.ClassesRoot.OpenSubKey (k) : null;
						rk3 = rk2 != null ? rk2.OpenSubKey ("shell") : null;
						return rk3 != null ? rk3.GetSubKeyNames () : null;
					} finally {
						if (rk3 != null)
							rk3.Close ();
						if (rk2 != null)
							rk2.Close ();
						if (rk != null)
							rk.Close ();
					}
				}
#endif
			}
		}
	}
}
