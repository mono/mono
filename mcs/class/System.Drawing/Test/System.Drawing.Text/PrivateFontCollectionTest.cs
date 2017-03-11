//
// System.Drawing.Text.PrivateFontCollection unit tests
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing.Text {

	[TestFixture]
	public class PrivateFontCollectionTest {

		[Test]
		public void Constructor ()
		{
			PrivateFontCollection pfc = new PrivateFontCollection ();
			Assert.IsNotNull (pfc.Families);
		}

		[Test]
		public void AddFontFile_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new PrivateFontCollection ().AddFontFile (null));
		}

		[Test]
		public void AddFontFile_Empty ()
		{
			// badly formetted filename
			Assert.Throws<ArgumentException> (() => new PrivateFontCollection ().AddFontFile (String.Empty));
		}

		[Test]
		[Category ("NotWorking")] // it seems fontconfig doesn't validate on add...
		public void AddFontFile_NotAFontFile ()
		{
			string file = Path.GetTempFileName ();
			Assert.IsTrue (File.Exists (file), "Exists");
			// even if the file exists....
			Assert.Throws<FileNotFoundException> (() => new PrivateFontCollection ().AddFontFile (file));
		}

		// tests for AddMemoryFont are available in the CAS unit tests

		[Test]
		public void Dispose_Family ()
		{
			PrivateFontCollection pfc = new PrivateFontCollection ();
			pfc.Dispose ();
			Assert.Throws<ArgumentException> (() => { var x = pfc.Families; });
			// no it's not a ObjectDisposedException
		}
	}
}
