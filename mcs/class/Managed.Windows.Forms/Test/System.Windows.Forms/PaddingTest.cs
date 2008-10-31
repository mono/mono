//
//  PaddingTest.cs
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
// Copyright (c) 2006 Daniel Nauck
//
// Author:
//      Daniel Nauck    (dna(at)mono-project(dot)de)

#if NET_2_0

using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class PaddingTest : TestHelper
	{
		[Test]
		public void PaddingPropertiesTest()
		{
			Padding pad = new Padding();
			Assert.AreEqual(-1, pad.All, "#A1");
			Assert.AreEqual(0, pad.Top, "#A2");
			Assert.AreEqual(0, pad.Left, "#A3");
			Assert.AreEqual(0, pad.Right, "#A4");
			Assert.AreEqual(0, pad.Bottom, "#A5");
			Assert.AreEqual(0, pad.Horizontal, "#A6");
			Assert.AreEqual(0, pad.Vertical, "#A7");
			Assert.AreEqual("{Left=0,Top=0,Right=0,Bottom=0}", pad.ToString(), "#A8");
			Assert.AreEqual(new Size(0,0), pad.Size, "#A9");

			Padding pad2 = new Padding(5);
			Assert.AreEqual(5, pad2.All, "#B1");
			Assert.AreEqual(5, pad2.Top, "#B2");
			Assert.AreEqual(5, pad2.Left, "#B3");
			Assert.AreEqual(5, pad2.Right, "#B4");
			Assert.AreEqual(5, pad2.Bottom, "#B5");
			Assert.AreEqual(10, pad2.Horizontal, "#B6");
			Assert.AreEqual(10, pad2.Vertical, "#B7");
			Assert.AreEqual("{Left=5,Top=5,Right=5,Bottom=5}", pad2.ToString(), "#B8");
			Assert.AreEqual(new Size(10, 10), pad2.Size, "#B9");

			Padding pad3 = new Padding(5, 5, 10, 10);
			Assert.AreEqual(-1, pad3.All, "#C1");
			Assert.AreEqual(5, pad3.Top, "#C2");
			Assert.AreEqual(5, pad3.Left, "#C3");
			Assert.AreEqual(10, pad3.Right, "#C4");
			Assert.AreEqual(10, pad3.Bottom, "#C5");
			Assert.AreEqual(15, pad3.Horizontal, "#C6");
			Assert.AreEqual(15, pad3.Vertical, "#C7");
			Assert.AreEqual("{Left=5,Top=5,Right=10,Bottom=10}", pad3.ToString(), "#C8");
			Assert.AreEqual(new Size(15, 15), pad3.Size, "#C9");

			Padding pad4 = new Padding(10, 10, 10, 10);
			Assert.AreEqual(10, pad4.All, "#D1");

			Padding pad5 = Padding.Empty;
			Assert.AreEqual(0, pad5.All, "#E1");
			Assert.AreEqual(0, pad5.Top, "#E2");
			Assert.AreEqual(0, pad5.Left, "#E3");
			Assert.AreEqual(0, pad5.Right, "#E4");
			Assert.AreEqual(0, pad5.Bottom, "#E5");
			Assert.AreEqual(0, pad5.Horizontal, "#E6");
			Assert.AreEqual(0, pad5.Vertical, "#E7");
			Assert.AreEqual("{Left=0,Top=0,Right=0,Bottom=0}", pad5.ToString(), "#E8");
			Assert.AreEqual(new Size(0, 0), pad5.Size, "#E9");
		}

		[Test]
		public void PaddingOperatorTest()
		{
			Padding pad = new Padding(0);
			Assert.AreEqual(Padding.Empty, pad, "#A1");

			Padding pad1 = new Padding(2, 4, 6, 8);
			Padding pad2 = new Padding(5, 5, 10, 11);
			Padding pad3 = pad1 + pad2;
			Assert.AreEqual(-1, pad3.All, "#B1");
			Assert.AreEqual("{Left=7,Top=9,Right=16,Bottom=19}", pad3.ToString(), "#B2");

			pad3 = Padding.Add(pad1, pad2);
			Assert.AreEqual(-1, pad3.All, "#C1");
			Assert.AreEqual("{Left=7,Top=9,Right=16,Bottom=19}", pad3.ToString(), "#C2");

			Padding pad4 = pad3 - pad1;
			Assert.AreEqual(-1, pad4.All, "#D1");
			Assert.AreEqual("{Left=5,Top=5,Right=10,Bottom=11}", pad4.ToString(), "#D2");

			pad4 = Padding.Subtract(pad3, pad1);
			Assert.AreEqual(-1, pad4.All, "#E1");
			Assert.AreEqual("{Left=5,Top=5,Right=10,Bottom=11}", pad4.ToString(), "#E2");
		}
	}
}
#endif