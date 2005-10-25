//
// TempFileCollectionTest.cs 
//	- Unit tests for System.CodeDom.Compiler.TempFileCollection
//
// Author:
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

using NUnit.Framework;

using System;
using System.CodeDom.Compiler;

namespace MonoTests.System.CodeDom.Compiler {

	[TestFixture]
	public class TempFileCollectionTest {

		[Test]
		public void Contructor_NullTempDir ()
		{
			TempFileCollection tfc = new TempFileCollection (null);
			Assert.AreEqual (String.Empty, tfc.TempDir, "TempDir");
		}

		[Test]
		public void TempDir_Stays_Empty ()
		{
			TempFileCollection tfc = new TempFileCollection ();
			Assert.IsNotNull (tfc.BasePath, "BasePath");
			Assert.AreEqual (String.Empty, tfc.TempDir, "TempDir");
		}
	}
}
