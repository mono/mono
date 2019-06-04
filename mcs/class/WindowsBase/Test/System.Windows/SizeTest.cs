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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using NUnit.Framework;

namespace MonoTests.System.Windows {

	[TestFixture]
	public class SizeTest
	{
		[Test]
		public void Ctor_Accessors ()
		{
			Size s = new Size (10, 20);
			Assert.AreEqual (10, s.Width);
			Assert.AreEqual (20, s.Height);
		}

		[Test]
		public void Equals ()
		{
			Size s = new Size (10, 20);

			Assert.IsTrue (s.Equals (s));
			Assert.IsTrue (s.Equals (new Size (10, 20)));

			Assert.IsTrue (Size.Equals (s, s));

			Assert.IsFalse (s.Equals (new Size (5, 10)));
			Assert.IsFalse (Size.Equals (s, new Size (5, 10)));

			Assert.IsFalse (s.Equals (new object()));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Ctor_negative_width()
		{
			new Size (-10, 5);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Ctor_negative_height()
		{
			new Size (5, -10);
		}

		[Test]
		public void Modify_width ()
		{
			Size s = new Size (10, 10);
			s.Width = 20;
			Assert.AreEqual (new Size (20, 10), s);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ModifyEmpty_width ()
		{
			Size s = Size.Empty;
			s.Width = 20;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Modify_negative_width ()
		{
			Size s = new Size (10, 10);
			s.Width = -20;
		}

		[Test]
		public void Modify_height ()
		{
			Size s = new Size (10, 10);
			s.Height = 20;
			Assert.AreEqual (new Size (10, 20), s);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ModifyEmpty_height ()
		{
			Size s = Size.Empty;
			s.Height = 20;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Modify_negative_height ()
		{
			Size s = new Size (10, 10);
			s.Height = -20;
		}

		[Test]
		public void ParseWithoutWhiteSpaces ()
		{
			Assert.AreEqual (new Size (1, 2), Size.Parse ("1,2"));
		}

		[Test]
		public void ParseWithWhiteSpaces ()
		{
			Assert.AreEqual (new Size (1, 2), Size.Parse (" 1, 2 "));
		}

		[Test]
		public void ParseValueWithFloatingPoint ()
		{
			Assert.AreEqual (new Size (1.234, 5.678), Size.Parse ("1.234,5.678"));
		}
		[Test]
		public void ParseEmpty ()
		{
			Assert.AreEqual (Size.Empty, Size.Parse ("Empty"));
		}

		[Test]
		public void ParseEmptyWithWhiteSpaces ()
		{
			Assert.AreEqual (Size.Empty, Size.Parse (" Empty "));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ParseNegative ()
		{
			Assert.AreEqual (new Size (-1, 2), Size.Parse ("-1, 2"));
		}

		[Test]
		public void ToStringTest ()
		{
			Assert.AreEqual ("1,2", (new Size (1, 2)).ToString (CultureInfo.InvariantCulture));
		}

		[Test]
		public void Empty ()
		{
			Assert.AreEqual (Double.NegativeInfinity, Size.Empty.Width);
			Assert.AreEqual (Double.NegativeInfinity, Size.Empty.Height);
		}

		[Test]
		public void IsEmpty ()
		{
			Assert.IsTrue (Size.Empty.IsEmpty);
			Assert.IsFalse ((new Size (10, 10)).IsEmpty);
			Assert.IsFalse ((new Size (0, 0)).IsEmpty);
		}
	}
}
