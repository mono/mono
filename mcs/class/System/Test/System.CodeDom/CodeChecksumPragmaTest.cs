//
// CodeChecksumPragmaTest.cs
//	- Unit tests for System.CodeDom.CodeChecksumPragma
//
// Author:
//	Gert Driesen  <drieseng@users.sourceforge.net>
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
using NUnit.Framework;

using System;
using System.CodeDom;

namespace MonoTests.System.CodeDom
{
	[TestFixture]
	public class CodeChecksumPragmaTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodeChecksumPragma ccp = new CodeChecksumPragma ();
			Assert.AreEqual (Guid.Empty, ccp.ChecksumAlgorithmId, "#1");
			Assert.IsNull (ccp.ChecksumData, "#2");
			Assert.IsNotNull (ccp.FileName, "#3");
			Assert.AreEqual (string.Empty, ccp.FileName, "#4");

			ccp.FileName = null;
			Assert.IsNotNull (ccp.FileName, "#5");
			Assert.AreEqual (string.Empty, ccp.FileName, "#6");
		}

		[Test]
		public void Constructor1 ()
		{
			string fileName = "mono";
			Guid algorithmId = Guid.NewGuid();
			byte[] data = new byte[] {0,1};

			CodeChecksumPragma ccp = new CodeChecksumPragma (fileName, algorithmId, data);
			Assert.AreEqual (algorithmId, ccp.ChecksumAlgorithmId, "#1");
			Assert.AreEqual (data, ccp.ChecksumData, "#2");
			Assert.AreEqual (fileName, ccp.FileName, "#3");
			Assert.AreSame (data, ccp.ChecksumData, "#4");
			Assert.AreSame (fileName, ccp.FileName, "#5");

			ccp.ChecksumAlgorithmId = Guid.Empty;
			Assert.AreEqual (Guid.Empty, ccp.ChecksumAlgorithmId, "#6");

			ccp.ChecksumData = null;
			Assert.IsNull (ccp.ChecksumData, "#7");

			ccp.FileName = null;
			Assert.IsNotNull (ccp.FileName, "#8");
			Assert.AreEqual (string.Empty, ccp.FileName, "#9");

			ccp = new CodeChecksumPragma ((string) null, Guid.Empty, (byte[]) null);
			Assert.AreEqual (Guid.Empty, ccp.ChecksumAlgorithmId, "#10");
			Assert.IsNull (ccp.ChecksumData, "#11");
			Assert.IsNotNull (ccp.FileName, "#12");
			Assert.AreEqual (string.Empty, ccp.FileName, "#13");
		}
	}
}
#endif
