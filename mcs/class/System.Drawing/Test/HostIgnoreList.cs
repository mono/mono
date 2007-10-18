//
// HostIgnoreList helper
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.IO;
using NUnit.Framework;

namespace MonoTests {

	// now misnamed - we check for the DISTRO env variable
	public class HostIgnoreList {

		private const string IgnoreListName = "nunit-host-ignore-list";

		private static ArrayList ignore_list;

		static HostIgnoreList ()
		{
			string hostname = Environment.GetEnvironmentVariable ("DISTRO");
			if (hostname == null)
				return;

			if (File.Exists (IgnoreListName)) {
				using (StreamReader sr = new StreamReader (IgnoreListName)) {
					string line = sr.ReadLine ();
					while (line != null) {
						if (line.StartsWith (hostname)) {
							IgnoreList.Add (line.Substring (hostname.Length + 1));
						}
						line = sr.ReadLine ();
					}
				}
			}
		}

		public static IList IgnoreList {
			get {
				if (ignore_list == null)
					ignore_list = new ArrayList ();
				return ignore_list;
			}
		}

		public static void CheckTest (string testname)
		{
			if (ignore_list == null)
				return;

			if (IgnoreList.Contains (testname)) {
				string msg = String.Format ("Test '{0}' was ignored because it's defined in the '{1}' ignore list.", 
					testname, IgnoreListName);
				Assert.Ignore (msg);
			}
		}
	}
}
