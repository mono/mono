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
	public class RectTest
	{
		[Test]
		public void Ctor_Accessor ()
		{
			Rect r;

			r = new Rect (new Size (40, 50));
			Assert.AreEqual (0, r.X);
			Assert.AreEqual (0, r.Y);
			Assert.AreEqual (40, r.Width);
			Assert.AreEqual (50, r.Height);
			Assert.AreEqual (0, r.Left);
			Assert.AreEqual (0, r.Top);
			Assert.AreEqual (40, r.Right);
			Assert.AreEqual (50, r.Bottom);
			Assert.AreEqual (new Point (0, 0), r.TopLeft);
			Assert.AreEqual (new Point (40, 0), r.TopRight);
			Assert.AreEqual (new Point (0, 50), r.BottomLeft);
			Assert.AreEqual (new Point (40, 50), r.BottomRight);
			Assert.AreEqual (new Point (0, 0), r.Location);
			Assert.AreEqual (new Size (40, 50), r.Size);

			r = new Rect (new Point (4, 5), new Vector (20, 30));
			Assert.AreEqual (4, r.X);
			Assert.AreEqual (5, r.Y);
			Assert.AreEqual (20, r.Width);
			Assert.AreEqual (30, r.Height);
			Assert.AreEqual (4, r.Left);
			Assert.AreEqual (5, r.Top);
			Assert.AreEqual (24, r.Right);
			Assert.AreEqual (35, r.Bottom);
			Assert.AreEqual (new Point (4, 5), r.TopLeft);
			Assert.AreEqual (new Point (24, 5), r.TopRight);
			Assert.AreEqual (new Point (4, 35), r.BottomLeft);
			Assert.AreEqual (new Point (24, 35), r.BottomRight);
			Assert.AreEqual (new Point (4, 5), r.Location);
			Assert.AreEqual (new Size (20, 30), r.Size);

			r = new Rect (new Point (4, 5), new Point (20, 30));
			Assert.AreEqual (4, r.X);
			Assert.AreEqual (5, r.Y);
			Assert.AreEqual (16, r.Width);
			Assert.AreEqual (25, r.Height);
			Assert.AreEqual (4, r.Left);
			Assert.AreEqual (5, r.Top);
			Assert.AreEqual (20, r.Right);
			Assert.AreEqual (30, r.Bottom);
			Assert.AreEqual (new Point (4, 5), r.TopLeft);
			Assert.AreEqual (new Point (20, 5), r.TopRight);
			Assert.AreEqual (new Point (4, 30), r.BottomLeft);
			Assert.AreEqual (new Point (20, 30), r.BottomRight);
			Assert.AreEqual (new Point (4, 5), r.Location);
			Assert.AreEqual (new Size (16,25), r.Size);

			r = new Rect (new Point (20, 30), new Point (4, 5));
			Assert.AreEqual (4, r.X);
			Assert.AreEqual (5, r.Y);
			Assert.AreEqual (16, r.Width);
			Assert.AreEqual (25, r.Height);
			Assert.AreEqual (4, r.Left);
			Assert.AreEqual (5, r.Top);
			Assert.AreEqual (20, r.Right);
			Assert.AreEqual (30, r.Bottom);
			Assert.AreEqual (new Point (4, 5), r.TopLeft);
			Assert.AreEqual (new Point (20, 5), r.TopRight);
			Assert.AreEqual (new Point (4, 30), r.BottomLeft);
			Assert.AreEqual (new Point (20, 30), r.BottomRight);
			Assert.AreEqual (new Point (4, 5), r.Location);
			Assert.AreEqual (new Size (16,25), r.Size);

			r = new Rect (10, 15, 20, 30);
			Assert.AreEqual (10, r.X);
			Assert.AreEqual (15, r.Y);
			Assert.AreEqual (20, r.Width);
			Assert.AreEqual (30, r.Height);
			Assert.AreEqual (10, r.Left);
			Assert.AreEqual (15, r.Top);
			Assert.AreEqual (30, r.Right);
			Assert.AreEqual (45, r.Bottom);
			Assert.AreEqual (new Point (10, 15), r.TopLeft);
			Assert.AreEqual (new Point (30, 15), r.TopRight);
			Assert.AreEqual (new Point (10, 45), r.BottomLeft);
			Assert.AreEqual (new Point (30, 45), r.BottomRight);
			Assert.AreEqual (new Point (10, 15), r.Location);
			Assert.AreEqual (new Size (20, 30), r.Size);

			r = new Rect (new Point (10, 15), new Size (20, 30));
			Assert.AreEqual (10, r.X);
			Assert.AreEqual (15, r.Y);
			Assert.AreEqual (20, r.Width);
			Assert.AreEqual (30, r.Height);
			Assert.AreEqual (10, r.Left);
			Assert.AreEqual (15, r.Top);
			Assert.AreEqual (30, r.Right);
			Assert.AreEqual (45, r.Bottom);
			Assert.AreEqual (new Point (10, 15), r.TopLeft);
			Assert.AreEqual (new Point (30, 15), r.TopRight);
			Assert.AreEqual (new Point (10, 45), r.BottomLeft);
			Assert.AreEqual (new Point (30, 45), r.BottomRight);
			Assert.AreEqual (new Point (10, 15), r.Location);
			Assert.AreEqual (new Size (20, 30), r.Size);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Ctor_NegativeWidth ()
		{
			new Rect (10, 10, -10, 10);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Ctor_NegativeHeight ()
		{
			new Rect (10, 10, 10, -10);
		}

		[Test]
		public void Empty ()
		{
			Rect r = Rect.Empty;
			Assert.AreEqual (Double.PositiveInfinity, r.X);
			Assert.AreEqual (Double.PositiveInfinity, r.Y);
			Assert.AreEqual (Double.NegativeInfinity, r.Width);
			Assert.AreEqual (Double.NegativeInfinity, r.Height);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ModifyEmpty_x ()
		{
			Rect r = Rect.Empty;
			r.X = 5;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ModifyEmpty_y ()
		{
			Rect r = Rect.Empty;
			r.Y = 5;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ModifyEmpty_width ()
		{
			Rect r = Rect.Empty;
			r.Width = 5;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ModifyEmpty_height ()
		{
			Rect r = Rect.Empty;
			r.Height = 5;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ModifyEmpty_negative_width ()
		{
			Rect r = Rect.Empty;
			r.Width = -5;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ModifyEmpty_negative_height ()
		{
			Rect r = Rect.Empty;
			r.Height = -5;
		}


		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Modify_negative_width ()
		{
			Rect r = new Rect (0, 0, 10, 10);
			r.Width = -5;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Modify_negative_height ()
		{
			Rect r = new Rect (0, 0, 10, 10);
			r.Height = -5;
		}

		[Test]
		public void Empty_Size ()
		{
			Assert.AreEqual (Size.Empty, Rect.Empty.Size);
		}

		[Test]
		public void IsEmpty ()
		{
			Assert.IsTrue (Rect.Empty.IsEmpty);
			Assert.IsFalse ((new Rect (5, 5, 5, 5)).IsEmpty);
		}

		[Test]
		public void Location ()
		{
			Rect r = new Rect (0, 0, 0, 0);

			r.Location = new Point (10, 15);
			Assert.AreEqual (10, r.X);
			Assert.AreEqual (15, r.Y);
		}

		[Test]
		public void RectSize ()
		{
			Rect r = new Rect (0, 0, 5, 5);

			r.Size = new Size (10, 15);
			Assert.AreEqual (10, r.Width);
			Assert.AreEqual (15, r.Height);
		}

		[Test]
		public void ToStringTest ()
		{
			Rect r = new Rect (1.0, 2.5, 3, 4);

			string expectedStringOutput = "1,2.5,3,4";
			Assert.AreEqual (expectedStringOutput, r.ToString ());
			Assert.AreEqual (expectedStringOutput, r.ToString (null));
			Assert.AreEqual ("Empty", Rect.Empty.ToString ());

			// IFormattable.ToString
			IFormattable rFormattable = r;
			Assert.AreEqual (expectedStringOutput,
				rFormattable.ToString (null, null),
				"IFormattable.ToString with null format");
			Assert.AreEqual (expectedStringOutput,
				rFormattable.ToString (string.Empty, null),
				"IFormattable.ToString with empty format");
			Assert.AreEqual ("1.00,2.50,3.00,4.00",
				rFormattable.ToString ("N2", null),
				"IFormattable.ToString with N2 format");
			Assert.AreEqual ("blah,blah,blah,blah",
				rFormattable.ToString ("blah", null),
				"IFormattable.ToString with blah format");
			Assert.AreEqual (":,:,:,:",
				rFormattable.ToString (":", null),
				"IFormattable.ToString with : format");
			Assert.AreEqual ("Empty",
				((IFormattable) Rect.Empty).ToString ("blah", null),
				"IFormattable.ToString on Rect.Empty with blah format");

			foreach (CultureInfo culture in CultureInfo.GetCultures (CultureTypes.AllCultures)) {
				if (culture.IsNeutralCulture)
					continue;
				string separator = ",";
				if (culture.NumberFormat.NumberDecimalSeparator == separator)
					separator = ";";
				expectedStringOutput =
					1.ToString (culture) + separator +
					2.5.ToString (culture) + separator +
					3.ToString (culture) + separator +
					4.ToString (culture);
				Assert.AreEqual (expectedStringOutput,
					r.ToString (culture),
					"ToString with Culture: " + culture.Name);
				Assert.AreEqual ("Empty",
					Rect.Empty.ToString (culture),
					"ToString on Empty with Culture: " + culture.Name);

				// IFormattable.ToString
				Assert.AreEqual (expectedStringOutput,
					rFormattable.ToString (null, culture),
					"IFormattable.ToString with null format with Culture: " + culture.Name);
				Assert.AreEqual (expectedStringOutput,
					rFormattable.ToString (string.Empty, culture),
					"IFormattable.ToString with empty format with Culture: " + culture.Name);
				expectedStringOutput =
					1.ToString ("N2", culture) + separator +
					2.5.ToString ("N2", culture) + separator +
					3.ToString ("N2", culture) + separator +
					4.ToString ("N2", culture);
				Assert.AreEqual (expectedStringOutput,
					rFormattable.ToString ("N2", culture),
					"IFormattable.ToString with N2 format with Culture: " + culture.Name);
			}
		}
		
		[Test]
		[Category ("NotWorking")]
		public void ToString_FormatException ()
		{
			// This test does not currently work because
			// String.Format does not throw all necessary exceptions
			IFormattable rFormattable = new Rect (1.0, 2.5, 3, 4);
			bool exceptionRaised = false;
			try {
				rFormattable.ToString ("{", null);
			} catch (FormatException) {
				exceptionRaised = true;
			}
			Assert.IsTrue (exceptionRaised, "Expected FormatException with IFormattable.ToString (\"{\", null)");
		}

		[Test]
		[Category ("NotWorking")]
		public void Parse ()
		{
			Rect r = Rect.Parse ("1 , 2, 3, 4");
			Assert.AreEqual (new Rect (1, 2, 3, 4), r);
		}

		[Test]
		[Category ("NotWorking")]
		public void Parse2 ()
		{
			Rect r = Rect.Parse ("1 2 3 4");
			Assert.AreEqual (new Rect (1, 2, 3, 4), r);
		}

		[Test]
		[Category ("NotWorking")]
		public void Parse3 ()
		{
			Rect r = Rect.Parse ("  1 2 3 4  ");
			Assert.AreEqual (new Rect (1, 2, 3, 4), r);
		}

		[Test]
		[Category ("NotWorking")]
		public void ParseWithBothSeparators ()
		{
			Rect.Parse ("1.0, 3 2.0, 5.0");
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (ArgumentException))]
		public void ParseNegative ()
		{
			Rect.Parse ("1, 2, -3, -4");
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))] // "Premature string termination encountered."
		public void Parse3Doubles ()
		{
			Rect.Parse ("1.0, 3, -5");
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (FormatException))]
		public void ParseInvalidString1 ()
		{
			Rect.Parse ("1.0, 3, -x, 5.0");
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ParseInvalidString3 ()
		{
			Rect.Parse ("1.0, 3, 2.0, 5.0, 2");
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (FormatException))]
		public void ParseInvalidString4 ()
		{
			Rect.Parse ("1.0-3, 2.0, 5.0, 2");
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ParseInvalidString5 ()
		{
			Rect.Parse ("1.0, 2.0, 5.0, 2,");
		}

		[Test]
		[Category ("NotWorking")]
		public void ParseInvalidString6 ()
		{
			Rect.Parse ("\n1.0, 2.0, 5.0, 2");
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ParseInvalidString7 ()
		{
			Rect r = Rect.Parse ("1,, 2, 3, 4");
			Assert.AreEqual (new Rect (1, 2, 3, 4), r);
		}

		[Test]
		public void Equals ()
		{
			Rect r1 = new Rect (1, 2, 3, 4);
			Rect r2 = r1;

			Assert.IsTrue (r1.Equals (r1));

			r2.X = 0;
			Assert.IsFalse (r1.Equals (r2));
			r2.X = r1.X;

			r2.Y = 0;
			Assert.IsFalse (r1.Equals (r2));
			r2.Y = r1.Y;

			r2.Width = 0;
			Assert.IsFalse (r1.Equals (r2));
			r2.Width = r1.Width;

			r2.Height = 0;
			Assert.IsFalse (r1.Equals (r2));
			r2.Height = r1.Height;

			Assert.IsFalse (r1.Equals (new object ()));

			r1 = Rect.Empty;
			r2 = Rect.Empty;

			Assert.AreEqual (true, r1.Equals (r2));
			Assert.AreEqual (true, r2.Equals (r1));
		}

		[Test]
		public void ContainsRect ()
		{
			Rect r = new Rect (0, 0, 50, 50);

			// fully contained
			Assert.IsTrue (r.Contains (new Rect (10, 10, 10, 10)));

			// crosses top side
			Assert.IsFalse (r.Contains (new Rect (5, -5, 10, 10)));

			// crosses right side
			Assert.IsFalse (r.Contains (new Rect (5, 5, 50, 10)));

			// crosses bottom side
			Assert.IsFalse (r.Contains (new Rect (5, 5, 10, 50)));

			// crosses left side
			Assert.IsFalse (r.Contains (new Rect (-5, 5, 10, 10)));

			// completely outside (top)
			Assert.IsFalse (r.Contains (new Rect (5, -5, 1, 1)));

			// completely outside (right)
			Assert.IsFalse (r.Contains (new Rect (75, 5, 1, 1)));

			// completely outside (bottom)
			Assert.IsFalse (r.Contains (new Rect (5, 75, 1, 1)));

			// completely outside (left)
			Assert.IsFalse (r.Contains (new Rect (-25, 5, 1, 1)));
		}

		[Test]
		public void ContainsDoubles ()
		{
			Rect r = new Rect (0, 0, 50, 50);

			Assert.IsTrue (r.Contains (10, 10));
			Assert.IsFalse (r.Contains (-5, -5));
		}

		[Test]
		public void ContainsPoint ()
		{
			Rect r = new Rect (0, 0, 50, 50);

			Assert.IsTrue (r.Contains (new Point (10, 10)));
			Assert.IsFalse (r.Contains (new Point (-5, -5)));
		}

		[Test]
		public void Inflate ()
		{
			Rect r = Rect.Inflate (new Rect (0, 0, 20, 20), 10, 15);
			Assert.AreEqual (new Rect (-10, -15, 40, 50), r);

			r = Rect.Inflate (new Rect (0, 0, 20, 20), new Size (10, 15));
			Assert.AreEqual (new Rect (-10, -15, 40, 50), r);

			r = new Rect (0, 0, 20, 20);
			r.Inflate (10, 15);
			Assert.AreEqual (new Rect (-10, -15, 40, 50), r);

			r = new Rect (0, 0, 20, 20);
			r.Inflate (new Size (10, 15));
			Assert.AreEqual (new Rect (-10, -15, 40, 50), r);
		}

		[Test]
		public void IntersectsWith ()
		{
			Rect r = new Rect (0, 0, 50, 50);

			// fully contained
			Assert.IsTrue (r.IntersectsWith (new Rect (10, 10, 10, 10)));

			// crosses top side
			Assert.IsTrue (r.IntersectsWith (new Rect (5, -5, 10, 10)));

			// crosses right side
			Assert.IsTrue (r.IntersectsWith (new Rect (5, 5, 50, 10)));

			// crosses bottom side
			Assert.IsTrue (r.IntersectsWith (new Rect (5, 5, 10, 50)));

			// crosses left side
			Assert.IsTrue (r.IntersectsWith (new Rect (-5, 5, 10, 10)));

			// completely outside (top)
			Assert.IsFalse (r.IntersectsWith (new Rect (5, -5, 1, 1)));

			// completely outside (right)
			Assert.IsFalse (r.IntersectsWith (new Rect (75, 5, 1, 1)));

			// completely outside (bottom)
			Assert.IsFalse (r.IntersectsWith (new Rect (5, 75, 1, 1)));

			// completely outside (left)
			Assert.IsFalse (r.IntersectsWith (new Rect (-25, 5, 1, 1)));
		}

		[Test]
		public void Intersect ()
		{
			Rect r;

			// fully contained
			r = new Rect (0, 0, 50, 50);
			r.Intersect (new Rect (10, 10, 10, 10));
			Assert.AreEqual (new Rect (10, 10, 10, 10), r);

			// crosses top side
			r = new Rect (0, 0, 50, 50);
			r.Intersect (new Rect (5, -5, 10, 10));
			Assert.AreEqual (new Rect (5, 0, 10, 5), r);

			// crosses right side
			r = new Rect (0, 0, 50, 50);
			r.Intersect (new Rect (5, 5, 50, 10));
			Assert.AreEqual (new Rect (5, 5, 45, 10), r);

			// crosses bottom side
			r = new Rect (0, 0, 50, 50);
			r.Intersect (new Rect (5, 5, 10, 50));
			Assert.AreEqual(new Rect(5, 5, 10, 45), r);

			// crosses left side
			r = new Rect (0, 0, 50, 50);
			r.Intersect (new Rect (-5, 5, 10, 10));
			Assert.AreEqual(new Rect(0, 5, 5, 10), r);

			// completely outside (top)
			r = new Rect (0, 0, 50, 50);
			r.Intersect (new Rect (5, -5, 1, 1));
			Assert.AreEqual (Rect.Empty, r);

			// completely outside (right)
			r = new Rect (0, 0, 50, 50);
			r.Intersect (new Rect (75, 5, 1, 1));
			Assert.AreEqual (Rect.Empty, r);

			// completely outside (bottom)
			r = new Rect (0, 0, 50, 50);
			r.Intersect (new Rect (5, 75, 1, 1));
			Assert.AreEqual (Rect.Empty, r);

			// completely outside (left)
			r = new Rect (0, 0, 50, 50);
			r.Intersect (new Rect (-25, 5, 1, 1));
			Assert.AreEqual (Rect.Empty, r);
		}

		[Test]
		public void Union()
		{
			Rect r;
			
			// fully contained
			r = new Rect(0, 0, 50, 50);
			r.Union(new Rect(10, 10, 10, 10));
			Assert.AreEqual(new Rect(0, 0, 50, 50), r);

			// crosses top side
			r = new Rect(0, 0, 50, 50);
			r.Union(new Rect(5, -5, 10, 10));
			Assert.AreEqual(new Rect(0, -5, 50, 55), r);

			// crosses right side
			r = new Rect(0, 0, 50, 50);
			r.Union(new Rect(5, 5, 50, 10));
			Assert.AreEqual(new Rect(0, 0, 55, 50), r);

			// crosses bottom side
			r = new Rect(0, 0, 50, 50);
			r.Union(new Rect(5, 5, 10, 50));
			Assert.AreEqual(new Rect(0, 0, 50, 55), r);

			// crosses left side
			r = new Rect(0, 0, 50, 50);
			r.Union(new Rect(-5, 5, 10, 10));
			Assert.AreEqual(new Rect(-5, 0, 55, 50), r);

			// completely outside (top)
			r = new Rect(0, 0, 50, 50);
			r.Union(new Rect(5, -5, 1, 1));
			Assert.AreEqual(new Rect(0, -5, 50, 55), r);

			// completely outside (right)
			r = new Rect(0, 0, 50, 50);
			r.Union(new Rect(75, 5, 1, 1));
			Assert.AreEqual(new Rect(0, 0, 76, 50), r);

			// completely outside (bottom)
			r = new Rect(0, 0, 50, 50);
			r.Union(new Rect(5, 75, 1, 1));
			Assert.AreEqual(new Rect(0,0, 50, 76), r);

			// completely outside (left)
			r = new Rect(0, 0, 50, 50);
			r.Union(new Rect(-25, 5, 1, 1));
			Assert.AreEqual(new Rect(-25, 0, 75, 50), r);
		}

		[Test]
		public void Equals_Operator ()
		{
			Rect r1 = new Rect (1, 2, 30, 30);
			Rect r2 = new Rect (1, 2, 30, 30);

			Assert.AreEqual (true,  r1 == r2);
			Assert.AreEqual (false, r1 != r2);

			r2 = new Rect (10, 20, 30, 30);

			Assert.AreEqual (false, r1 == r2);
			Assert.AreEqual (true,  r1 != r2);

			r1 = Rect.Empty;
			r2 = Rect.Empty;

			Assert.AreEqual (true, r1 == r2);
			Assert.AreEqual (false, r1 != r2);
		}

	}
}

