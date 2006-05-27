//
// UtilitiesTest.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2006 Marek Sieradzki
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

using System;
using System.Collections;
using Microsoft.Build.BuildEngine;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class UtilitiesTest {
		
		[Test]
		public void TestEscape ()
		{
			char[] list = new char [] {'$', '%', '\'', '(', ')', '*', ';', '?', '@'};
			Hashtable different = new Hashtable ();
			foreach (char c in list)
				different.Add (c, null);
			
			for (int i = 0; i < 256; i++) {
				char c = (char) i;
				if (Utilities.Escape (c.ToString ()) != c.ToString()) {
					if (different.Contains (c)) {
						string hex = String.Format ("%{0:x2}", i);
						if (hex != Utilities.Escape (c.ToString ()))
							Assert.Fail ("Char {0} should be escaped to {1} instead of {2}",
								c, hex, Utilities.Escape (c.ToString ()));
					} else
						Assert.Fail ("Char {0} should not be escaped", c);
				}
			}
		}
	}
}
