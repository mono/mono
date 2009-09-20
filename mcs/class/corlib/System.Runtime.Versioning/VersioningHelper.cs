//
// System.Runtime.Versioning.VersioningHelper class
//
// Authors
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

namespace System.Runtime.Versioning {

	public static class VersioningHelper {

		static private int GetDomainId ()
		{
#if NET_2_1
			return 0;
#else
			return AppDomain.CurrentDomain.Id;
#endif
		}

		static private int GetProcessId ()
		{
			// TODO - System.Diagnostics.Process class is located in System.dll
			return 0;
		}

		static string SafeName (string name, bool process, bool appdomain)
		{
			if (process && appdomain) {
				return String.Concat (name, "_", GetProcessId ().ToString (),
					"_", GetDomainId ().ToString ());
			} else if (process) {
				return String.Concat (name, "_", GetProcessId ().ToString ());
			} else if (appdomain) {
				return String.Concat (name, "_", GetDomainId ().ToString ());
			}
			// nothing, return original string
			return name;
		}

		static private string ConvertFromMachine (string name, ResourceScope to, Type type)
		{
			switch (to) {
			case ResourceScope.Machine:
				return SafeName (name, false, false);
			case ResourceScope.Process:
				return SafeName (name, true, false);
			case ResourceScope.AppDomain:
				return SafeName (name, true, true);
			default:
				throw new ArgumentException ("to");
			}
		}

		static private string ConvertFromProcess (string name, ResourceScope to, Type type)
		{
			if ((to < ResourceScope.Process) || (to >= ResourceScope.Private))
				throw new ArgumentException ("to");
			bool ad = ((to & ResourceScope.AppDomain) == ResourceScope.AppDomain);
			return SafeName (name, false, ad);
		}

		static private string ConvertFromAppDomain (string name, ResourceScope to, Type type)
		{
			if ((to < ResourceScope.AppDomain) || (to >= ResourceScope.Private))
				throw new ArgumentException ("to");
			return SafeName (name, false, false);
		}

		[MonoTODO ("process id is always 0")]
		static public string MakeVersionSafeName (string name, ResourceScope from, ResourceScope to)
		{
			return MakeVersionSafeName (name, from, to, null);
		}

		[MonoTODO ("type?")]
		static public string MakeVersionSafeName (string name, ResourceScope from, ResourceScope to, Type type)
		{
			if ((from & ResourceScope.Private) != 0) {
				to &= ~(ResourceScope.Private | ResourceScope.Assembly);
			} else if ((from & ResourceScope.Assembly) != 0) {
				to &= ~ResourceScope.Assembly;
			}
			
			string result = (name == null) ? String.Empty : name;
			switch (from) {
			case ResourceScope.Machine:
			case ResourceScope.Machine | ResourceScope.Private:
			case ResourceScope.Machine | ResourceScope.Assembly:
				return ConvertFromMachine (result, to, type);
			case ResourceScope.Process:
			case ResourceScope.Process | ResourceScope.Private:
			case ResourceScope.Process | ResourceScope.Assembly:
				return ConvertFromProcess (result, to, type);
			case ResourceScope.AppDomain:
			case ResourceScope.AppDomain | ResourceScope.Private:
			case ResourceScope.AppDomain | ResourceScope.Assembly:
				return ConvertFromAppDomain (result, to, type);
			default:
				throw new ArgumentException ("from");
			}
		}
	}
}

#endif
