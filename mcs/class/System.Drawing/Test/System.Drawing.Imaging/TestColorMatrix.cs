//
// Copyright (C) 2005-2006 Novell, Inc (http://www.novell.com)
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
//
// Authors:
//   Jordi Mas i Hernandez (jordi@ximian.com)
//   Sebastien Pouliot  <sebastien@ximian.com>
//

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing.Imaging {

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class ColorMatrixTest {

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor_Null ()
		{
			new ColorMatrix (null);
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void Constructor_TooSmallArraySize ()
		{
			new ColorMatrix (new float[][] { });
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void Constructor_TooWideArraySize ()
		{
			new ColorMatrix (new float[][] {
				new float[] { 0.0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f }
			});
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void Constructor_TooTallArraySize ()
		{
			new ColorMatrix (new float[][] {
				new float[] { 0.0f },
				new float[] { 1.0f },
				new float[] { 2.0f },
				new float[] { 3.0f },
				new float[] { 4.0f },
				new float[] { 5.0f }
			});
		}

		[Test]
		public void Constructor_TooBigArraySize ()
		{
			ColorMatrix cm = new ColorMatrix (new float[][] {
				new float[] { 0.0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f },
				new float[] { 1.0f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f },
				new float[] { 2.0f, 2.1f, 2.2f, 2.3f, 2.4f, 2.5f },
				new float[] { 3.0f, 3.1f, 3.2f, 3.3f, 3.4f, 3.5f },
				new float[] { 4.0f, 4.1f, 4.2f, 4.3f, 4.4f, 4.5f },
				new float[] { 5.0f, 5.1f, 5.2f, 5.3f, 5.4f, 5.5f }
			});

			Assert.AreEqual (cm.Matrix00, 0.0f, "00");
			Assert.AreEqual (cm.Matrix01, 0.1f, "01");
			Assert.AreEqual (cm.Matrix02, 0.2f, "02");
			Assert.AreEqual (cm.Matrix03, 0.3f, "03");
			Assert.AreEqual (cm.Matrix04, 0.4f, "04");
			Assert.AreEqual (cm.Matrix10, 1.0f, "10");
			Assert.AreEqual (cm.Matrix11, 1.1f, "11");
			Assert.AreEqual (cm.Matrix12, 1.2f, "12");
			Assert.AreEqual (cm.Matrix13, 1.3f, "13");
			Assert.AreEqual (cm.Matrix14, 1.4f, "14");
			Assert.AreEqual (cm.Matrix20, 2.0f, "20");
			Assert.AreEqual (cm.Matrix21, 2.1f, "21");
			Assert.AreEqual (cm.Matrix22, 2.2f, "22");
			Assert.AreEqual (cm.Matrix23, 2.3f, "23");
			Assert.AreEqual (cm.Matrix24, 2.4f, "24");
			Assert.AreEqual (cm.Matrix30, 3.0f, "30");
			Assert.AreEqual (cm.Matrix31, 3.1f, "31");
			Assert.AreEqual (cm.Matrix32, 3.2f, "32");
			Assert.AreEqual (cm.Matrix33, 3.3f, "33");
			Assert.AreEqual (cm.Matrix34, 3.4f, "34");
			Assert.AreEqual (cm.Matrix40, 4.0f, "40");
			Assert.AreEqual (cm.Matrix41, 4.1f, "41");
			Assert.AreEqual (cm.Matrix42, 4.2f, "42");
			Assert.AreEqual (cm.Matrix43, 4.3f, "43");
			Assert.AreEqual (cm.Matrix44, 4.4f, "44");
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void TooBigItems ()
		{
			ColorMatrix cm = new ColorMatrix (new float[][] {
				new float[] { 0.0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f },
				new float[] { 1.0f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f },
				new float[] { 2.0f, 2.1f, 2.2f, 2.3f, 2.4f, 2.5f },
				new float[] { 3.0f, 3.1f, 3.2f, 3.3f, 3.4f, 3.5f },
				new float[] { 4.0f, 4.1f, 4.2f, 4.3f, 4.4f, 4.5f },
				new float[] { 5.0f, 5.1f, 5.2f, 5.3f, 5.4f, 5.5f }
			});
			Assert.AreEqual (5.5f, cm[5,5], "out");
		}

		[Test]
		public void DefaultConstructor ()
		{
			ColorMatrix cm = new ColorMatrix ();

			Assert.AreEqual (cm.Matrix00, 1);
			Assert.AreEqual (cm.Matrix11, 1);
			Assert.AreEqual (cm.Matrix22, 1);
			Assert.AreEqual (cm.Matrix33, 1);
			Assert.AreEqual (cm.Matrix44, 1);
			Assert.AreEqual (cm.Matrix01, 0);
			Assert.AreEqual (cm.Matrix02, 0);
			Assert.AreEqual (cm.Matrix03, 0);
			Assert.AreEqual (cm.Matrix04, 0);
			Assert.AreEqual (cm.Matrix10, 0);
			Assert.AreEqual (cm.Matrix12, 0);
			Assert.AreEqual (cm.Matrix13, 0);
			Assert.AreEqual (cm.Matrix14, 0);
			Assert.AreEqual (cm.Matrix20, 0);
			Assert.AreEqual (cm.Matrix21, 0);
			Assert.AreEqual (cm.Matrix23, 0);
			Assert.AreEqual (cm.Matrix24, 0);
			Assert.AreEqual (cm.Matrix30, 0);
			Assert.AreEqual (cm.Matrix31, 0);
			Assert.AreEqual (cm.Matrix32, 0);
			Assert.AreEqual (cm.Matrix34, 0);
			Assert.AreEqual (cm.Matrix40, 0);
			Assert.AreEqual (cm.Matrix41, 0);
			Assert.AreEqual (cm.Matrix42, 0);
			Assert.AreEqual (cm.Matrix43, 0);
#if !TARGET_JVM
			Assert.AreEqual (100, Marshal.SizeOf (cm), "object");
			Assert.AreEqual (100, Marshal.SizeOf (typeof (ColorMatrix)), "type");
#endif
		}

		[Test]
		public void ConstructorArrayAndMethods ()
		{
			ColorMatrix cm = new ColorMatrix (new float[][] {
				new float[] {0.393f, 0.349f, 0.272f, 0, 0},
			        new float[] {0.769f, 0.686f, 0.534f, 0, 0},
			        new float[] {0.189f, 0.168f, 0.131f, 0, 0},
			        new float[] {     0,      0,      0, 1, 0},
			        new float[] {     0,      0,      0, 0, 1}
			});

			Assert.AreEqual (cm.Matrix00, 0.393f);
			Assert.AreEqual (cm.Matrix01, 0.349f);
			Assert.AreEqual (cm.Matrix02, 0.272f);
			Assert.AreEqual (cm.Matrix03, 0);
			Assert.AreEqual (cm.Matrix04, 0);

			Assert.AreEqual (cm.Matrix10, 0.769f);
			Assert.AreEqual (cm.Matrix11, 0.686f);
			Assert.AreEqual (cm.Matrix12, 0.534f);
			Assert.AreEqual (cm.Matrix13, 0);
			Assert.AreEqual (cm.Matrix14, 0);

			Assert.AreEqual (cm.Matrix20, 0.189f);
			Assert.AreEqual (cm.Matrix21, 0.168f);
			Assert.AreEqual (cm.Matrix22, 0.131f);
			Assert.AreEqual (cm.Matrix23, 0);
			Assert.AreEqual (cm.Matrix24, 0);

			Assert.AreEqual (cm.Matrix30, 0);
			Assert.AreEqual (cm.Matrix31, 0);
			Assert.AreEqual (cm.Matrix32, 0);
			Assert.AreEqual (cm.Matrix33, 1);
			Assert.AreEqual (cm.Matrix34, 0);

			Assert.AreEqual (cm.Matrix40, 0);
			Assert.AreEqual (cm.Matrix41, 0);
			Assert.AreEqual (cm.Matrix42, 0);
			Assert.AreEqual (cm.Matrix43, 0);
			Assert.AreEqual (cm.Matrix44, 1);
		}

		[Test]
		public void Property ()
		{
			ColorMatrix cm = new ColorMatrix (new float[][] {
				new float[] 	{1,	0,	0, 	0, 	0},
				new float[] 	{0.5f,	1,	0, 	0, 	0},
				new float[] 	{0,	0.1f,	1.5f, 	0, 	0},
				new float[] 	{0.5f,	3,	0.5f, 	1, 	0},
				new float[] 	{0,	0,	0, 	0, 	0}
			});

			Assert.AreEqual (cm[0,0], 1);
			Assert.AreEqual (cm[0,1], 0);
			Assert.AreEqual (cm[0,2], 0);
			Assert.AreEqual (cm[0,3], 0);
			Assert.AreEqual (cm[0,4], 0);
			
			Assert.AreEqual (cm[1,0], 0.5f);
			Assert.AreEqual (cm[1,1], 1);
			Assert.AreEqual (cm[1,2], 0);
			Assert.AreEqual (cm[1,3], 0);
			Assert.AreEqual (cm[1,4], 0);
			
			Assert.AreEqual (cm[2,0], 0);
			Assert.AreEqual (cm[2,1], 0.1f);
			Assert.AreEqual (cm[2,2], 1.5f);
			Assert.AreEqual (cm[2,3], 0);
			Assert.AreEqual (cm[2,4], 0);
			
			Assert.AreEqual (cm[3,0], 0.5f);
			Assert.AreEqual (cm[3,1], 3);
			Assert.AreEqual (cm[3,2], 0.5f);
			Assert.AreEqual (cm[3,3], 1);
			Assert.AreEqual (cm[3,4], 0);
			
			Assert.AreEqual (cm[4,0], 0);
			Assert.AreEqual (cm[4,1], 0);
			Assert.AreEqual (cm[4,2], 0);
			Assert.AreEqual (cm[4,3], 0);
			Assert.AreEqual (cm[4,4], 0);
		}
	}
}
