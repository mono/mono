//
// Tests for System.Web.UI.WebControls.Unit.cs 
//
// Author:
//	Miguel de Icaza (miguel@novell.com)
//

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
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]
	public class UnitTest
	{
		[Test]
		public void UnitConstructors ()
		{
			CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
			CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
			try {
				CultureInfo ciUS = CultureInfo.GetCultureInfo ("en-US");

				Thread.CurrentThread.CurrentCulture = ciUS;
				Thread.CurrentThread.CurrentUICulture = ciUS;
				RunUnitConstructorsTests ();
			} finally {
				Thread.CurrentThread.CurrentCulture = currentCulture;
				Thread.CurrentThread.CurrentUICulture = currentUICulture;
			}
		}

		void RunUnitConstructorsTests ()
		{
			Unit a1 = new Unit (1.0);

			Assert.AreEqual (a1.Type, UnitType.Pixel, "A1");
			Assert.AreEqual (a1.Value, 1.0, "A2");
			Assert.AreEqual ("1px", a1.ToString (), "A2.1");

			Unit a2 = new Unit (1);
			Assert.AreEqual (a2.Type, UnitType.Pixel, "A3");
			Assert.AreEqual (a1.Value, 1.0, "A4");
			Assert.AreEqual ("1px", a1.ToString (), "A4.1");

			Unit a3 = new Unit (32767);
			Assert.AreEqual (a3.Type, UnitType.Pixel, "A5");
			Assert.AreEqual (a3.Value, 32767.0, "A6");
			Assert.AreEqual ("32767px", a3.ToString (), "A6.1");

			a3 = new Unit (-32768);
			Assert.AreEqual (a3.Type, UnitType.Pixel, "A7");
			Assert.AreEqual (a3.Value, -32768.0, "A8");
			Assert.AreEqual ("-32768px", a3.ToString (), "A8.1");

			//
			// String constructor
			//
			Unit s1 = new Unit ("-45cm");
			Assert.AreEqual (s1.Type, UnitType.Cm, "A9");
			Assert.AreEqual (s1.Value, -45, "A10");
			Assert.AreEqual ("-45cm", s1.ToString (), "A10.1");
			
			s1 = new Unit ("\t-45cm");
			Assert.AreEqual (s1.Type, UnitType.Cm, "A11");
			Assert.AreEqual (s1.Value, -45, "A12");
			Assert.AreEqual ("-45cm", s1.ToString (), "A12.1");

			s1 = new Unit ("45\tcm");
			Assert.AreEqual (s1.Type, UnitType.Cm, "A13");
			Assert.AreEqual (s1.Value, 45, "A14");

			s1 = new Unit ("45\tcm");
			Assert.AreEqual (s1.Type, UnitType.Cm, "A15");
			Assert.AreEqual (s1.Value, 45, "A16");

			s1 = new Unit (null);
			Assert.AreEqual (s1.Type, UnitType.Pixel, "A17");
			Assert.AreEqual (s1.Value, 0, "A18");
			
			s1 = new Unit ("-45");
			Assert.AreEqual (s1.Type, UnitType.Pixel, "A19");
			Assert.AreEqual (s1.Value, -45, "A20");

			s1 = new Unit ("-45%");
			Assert.AreEqual (s1.Type, UnitType.Percentage, "A21");
			Assert.AreEqual (s1.Value, -45, "A22");
			Assert.AreEqual ("-45%", s1.ToString (), "A22.1");
			
			s1 = new Unit ("-45%  \t ");
			Assert.AreEqual (s1.Type, UnitType.Percentage, "A23");
			Assert.AreEqual (s1.Value, -45, "A24");
			
			s1 = new Unit ("-45 %  \t ");
			Assert.AreEqual (s1.Type, UnitType.Percentage, "A25");
			Assert.AreEqual (s1.Value, -45, "A26");

			s1 = new Unit ("45in");
			Assert.AreEqual (s1.Type, UnitType.Inch, "A27");
			Assert.AreEqual (s1.Value, 45, "A28");
			Assert.AreEqual ("45in", s1.ToString (), "A28.1");
			s1 = new Unit ("45cm");
			Assert.AreEqual (s1.Type, UnitType.Cm, "A29");
			Assert.AreEqual (s1.Value, 45, "A30");
			Assert.AreEqual ("45cm", s1.ToString (), "A30.1");
			s1 = new Unit ("45pt");
			Assert.AreEqual (s1.Type, UnitType.Point, "A31");
			Assert.AreEqual (s1.Value, 45, "A32");
			Assert.AreEqual ("45pt", s1.ToString (), "A32.1");
			s1 = new Unit ("45pc");
			Assert.AreEqual (s1.Type, UnitType.Pica, "A33");
			Assert.AreEqual (s1.Value, 45, "A34");
			Assert.AreEqual ("45pc", s1.ToString (), "A34.1");
			s1 = new Unit ("45mm");
			Assert.AreEqual (s1.Type, UnitType.Mm, "A35");
			Assert.AreEqual (s1.Value, 45, "A36");
			Assert.AreEqual ("45mm", s1.ToString (), "A36.1");
			s1 = new Unit ("45em");
			Assert.AreEqual (s1.Type, UnitType.Em, "A37");
			Assert.AreEqual (s1.Value, 45, "A38");
			Assert.AreEqual ("45em", s1.ToString (), "A38.1");
			s1 = new Unit ("45ex");
			Assert.AreEqual (s1.Type, UnitType.Ex, "A39");
			Assert.AreEqual (s1.Value, 45, "A40");
			Assert.AreEqual ("45ex", s1.ToString (), "A40.1");
			s1 = new Unit ("45px");
			Assert.AreEqual (s1.Type, UnitType.Pixel, "A41");
			Assert.AreEqual (s1.Value, 45, "A42");
			Assert.AreEqual ("45px", s1.ToString (), "A42.1");

			s1 = new Unit ("1.75cm");
			Assert.AreEqual (s1.Type, UnitType.Cm, "A43");
			Assert.AreEqual (s1.Value, 1.75, "A44");
			
			s1 = new Unit ("1.75%");
			Assert.AreEqual (s1.Type, UnitType.Percentage, "A45");
			Assert.AreEqual (s1.Value, 1.75, "A46");

			s1 = new Unit (null);
			Assert.AreEqual (s1.Type, UnitType.Pixel, "A47");
			Assert.AreEqual (s1.Value, 0, "A48");
			Assert.AreEqual (s1.IsEmpty, true, "A49");
			Assert.AreEqual (s1.ToString (), "", "A50");

			s1 = new Unit ("");
			Assert.AreEqual (s1.Type, UnitType.Pixel, "A51");
			Assert.AreEqual (s1.Value, 0, "A52");
			Assert.AreEqual (s1.IsEmpty, true, "A53");
			Assert.AreEqual (s1.ToString (), "", "A54");

			s1 = new Unit ("45.75cm");
			Assert.AreEqual (s1.Type, UnitType.Cm, "A55");
			Assert.AreEqual (s1.Value, 45.75, "A56");
			Assert.AreEqual ("45.75cm", s1.ToString (), "A57");
			
			a3 = new Unit (1.5);
			Assert.AreEqual (UnitType.Pixel, a3.Type, "A58");
			Assert.AreEqual (1.0, a3.Value, "A59");

			s1 = new Unit (".9em");
			Assert.AreEqual (s1.Type, UnitType.Em, "B1");
			Assert.AreEqual (0.9d, s1.Value, "B2");
			Assert.AreEqual ("0.9em", s1.ToString (), "B3");
		}

		[Test]
		public void ParseCultures ()
		{
			// Test cultures where the decimal separator is not "."

			CultureInfo ci = new CultureInfo ("es-ES", false);

			Unit s1 = new Unit ("1,5cm", ci);
			Assert.AreEqual (s1.Type, UnitType.Cm, "C1");
			Assert.AreEqual (s1.Value, 1.5, "C2");

			CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
			CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
			try {
				CultureInfo ciUS = CultureInfo.GetCultureInfo ("en-US");

				Thread.CurrentThread.CurrentCulture = ciUS;
				Thread.CurrentThread.CurrentUICulture = ciUS;
				Assert.AreEqual (s1.ToString (), "1.5cm", "A54");
			} finally {
				Thread.CurrentThread.CurrentCulture = currentCulture;
				Thread.CurrentThread.CurrentUICulture = currentUICulture;
			}

			Assert.AreEqual (s1.ToString (ci), "1,5cm", "A55");
		}
		
		[Test]
		public void UnitEquality ()
		{
			Unit u1 = new Unit ("1px");
			Unit u2 = new Unit ("2px");
			Unit t1 = new Unit ("1px");
			Unit c2 = new Unit ("2cm");

			Assert.AreEqual (u1 == t1, true, "U1");
			Assert.AreEqual (u1 != u2, true, "U2");
			Assert.AreEqual (u1 == u2, false, "U3");
			Assert.AreEqual (u1 != t1, false, "U4");

			// Test that its comparing the units and value
			Assert.AreEqual (u2 != c2, true, "U5");
		}

		[Test]
		public void UnitEqualityWithEmpty ()
		{
			Unit unit = Unit.Parse ("");
			Assert.AreEqual (Unit.Empty, unit, "A1");

			unit = Unit.Parse ("0px");
			Assert.IsTrue (unit != Unit.Empty, "B1");
		}
		
		[Test]
		public void UnitImplicit ()
		{
			Unit u = 1;
			Assert.AreEqual (u.Value, 1.0, "M1");
			Assert.AreEqual (u.Type, UnitType.Pixel, "M1");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IncorrectConstructor ()
		{
			Unit a = new Unit (32768.0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IncorrectConstructor2 ()
		{
			Unit a = new Unit (32768);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IncorrectConstructor3 ()
		{
			Unit a = new Unit (-32769.0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IncorrectConstructor4 ()
		{
			Unit a = new Unit (-32769);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IncorrectConstructor5 ()
		{
			// throws because of space between - and 45
			Unit a = new Unit ("- 45cm");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IncorrectConstructor6 ()
		{
			// Throws becaues "cma" is not a unit
			Unit a = new Unit ("-45cma");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IncorrectConstructor7 ()
		{
			// Throws because there is stuff after "%"
			Unit a = new Unit ("-45%cm");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IncorrectConstructor8 ()
		{
			// throws because there is stuff after cm (a)
			Unit a = new Unit ("-45cm a");
		}
	
		[Test]
		[ExpectedException (typeof (FormatException))]
		public void IncorrectConstructor9()
		{
		    // throws because floating point values are not valid for Pixel.
		    Unit a = new Unit("34.4px");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IncorrectConstructor10 ()
		{
			// throws because there is a space before the dot
			Unit a = new Unit ("34 .4pt");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IncorrectConstructor11 ()
		{
			// throws because there is a space after the dot
			Unit a = new Unit ("34. 4pt");
		}

#if NET_2_0
		class MyFormatProvider : IFormatProvider
		{
			public object GetFormat (Type format_type)
			{
				return Activator.CreateInstance (format_type);
			}
		}

		[Test]
		public void Unit_IFormatProviderToString ()
		{
			CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
			CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
			try {
				CultureInfo ciUS = CultureInfo.GetCultureInfo ("en-US");

				Thread.CurrentThread.CurrentCulture = ciUS;
				Thread.CurrentThread.CurrentUICulture = ciUS;
				RunUnit_IFormatProviderToStringTests ();
			} finally {
				Thread.CurrentThread.CurrentCulture = currentCulture;
				Thread.CurrentThread.CurrentUICulture = currentUICulture;
			}
		}

		void RunUnit_IFormatProviderToStringTests ()
		{
			MyFormatProvider mfp = new MyFormatProvider ();

			Unit a1 = new Unit (1.0);
			Assert.AreEqual ("1px", a1.ToString (mfp), "A1");

			a1 = new Unit (1);
			Assert.AreEqual ("1px", a1.ToString (mfp), "A2");

			a1 = new Unit (32767);
			Assert.AreEqual ("32767px", a1.ToString (mfp), "A3");

			a1 = new Unit (-32768);
			Assert.AreEqual ("-32768px", a1.ToString (mfp), "A4");

			//
			// String constructor
			//
			Unit s1 = new Unit ("-45cm");
			Assert.AreEqual ("-45cm", s1.ToString (mfp), "A5");
			
			s1 = new Unit ("\t-45cm");
			Assert.AreEqual ("-45cm", s1.ToString (mfp), "A6");

			s1 = new Unit ("45\tcm");
			Assert.AreEqual ("45cm", s1.ToString (mfp), "A7");

			s1 = new Unit (null);
			Assert.AreEqual ("", s1.ToString (mfp), "A8");
			
			s1 = new Unit ("-45");
			Assert.AreEqual ("-45px", s1.ToString (mfp), "A9");

			s1 = new Unit ("-45%");
			Assert.AreEqual ("-45%", s1.ToString (mfp), "A10");
			
			s1 = new Unit ("-45%  \t ");
			Assert.AreEqual ("-45%", s1.ToString (mfp), "A11");
			
			s1 = new Unit ("-45 %  \t ");
			Assert.AreEqual ("-45%", s1.ToString (mfp), "A12");

			s1 = new Unit ("45in");
			Assert.AreEqual ("45in", s1.ToString (mfp), "A13");

			s1 = new Unit ("45cm");
			Assert.AreEqual ("45cm", s1.ToString (mfp), "A14");

			s1 = new Unit ("45pt");
			Assert.AreEqual ("45pt", s1.ToString (mfp), "A15");

			s1 = new Unit ("45pc");
			Assert.AreEqual ("45pc", s1.ToString (mfp), "A16");

			s1 = new Unit ("45mm");
			Assert.AreEqual ("45mm", s1.ToString (mfp), "A17");

			s1 = new Unit ("45em");
			Assert.AreEqual ("45em", s1.ToString (mfp), "A18");

			s1 = new Unit ("45ex");
			Assert.AreEqual ("45ex", s1.ToString (mfp), "A19");

			s1 = new Unit ("45px");
			Assert.AreEqual ("45px", s1.ToString (mfp), "A20");

			s1 = new Unit ("1.75cm");
			Assert.AreEqual ("1.75cm", s1.ToString (mfp), "A21");
			
			s1 = new Unit ("1.75%");
			Assert.AreEqual ("1.75%", s1.ToString (mfp), "A22");

			s1 = new Unit (null);
			Assert.AreEqual ("", s1.ToString (mfp), "A23");

			s1 = new Unit ("");
			Assert.AreEqual ("", s1.ToString (mfp), "A24");

			s1 = new Unit ("45.75cm");
			Assert.AreEqual ("45.75cm", s1.ToString (mfp), "A25");
			
			a1 = new Unit (1.5);
			Assert.AreEqual ("1px", a1.ToString (mfp), "A26");
		}
#endif
	}
}

