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
//
// Author:
//   Jordi Mas i Hernandez (jordi@ximian.com)
//

using System;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Drawing
{

	[TestFixture]
	public class TestColorMatrix
	{

		[TearDown]
		public void Clean() {}

		[SetUp]
		public void GetReady()
		{

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
