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
// (C) 2006 Novell, Inc.
//

#if NET_2_0

using System;
using System.Windows.Forms;
using System.Drawing;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class RowStyleTest : TestHelper {

		[Test]
		public void CtorTest1 ()
		{
			RowStyle rs = new RowStyle ();
			Assert.AreEqual (0.0f, rs.Height, "1");
			Assert.AreEqual (SizeType.AutoSize, rs.SizeType, "2");
		}

		[Test]
		public void CtorTest2 ()
		{
			RowStyle rs = new RowStyle (SizeType.Absolute);
			
			Assert.AreEqual (0.0f, rs.Height, "1");
			Assert.AreEqual (SizeType.Absolute, rs.SizeType, "2");
		}

		[Test]
		public void CtorTest3 ()
		{
			RowStyle rs = new RowStyle (SizeType.Absolute, 5.0f);
			
			Assert.AreEqual (5.0, rs.Height, "1");
			Assert.AreEqual (SizeType.Absolute, rs.SizeType, "2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CtorTest4 ()
		{
			RowStyle rs = new RowStyle (SizeType.Absolute, -1.0f);
			
			TestHelper.RemoveWarning (rs);
		}

		[Test]
		public void HeightTest1 ()
		{
			RowStyle rs = new RowStyle ();
			rs.Height = 1.0f;
			Assert.AreEqual (1.0f, rs.Height, "1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void HeightTest2 ()
		{
			RowStyle rs = new RowStyle ();
			rs.Height = -1.0f;
		}
	}
}

#endif
