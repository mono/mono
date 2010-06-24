//
// Tests for System.Web.UI.WebControls.FontUnit.cs 
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

using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.UI.WebControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class FontUnitTest {

		[Test]
		public void FontUnitConstructors1 ()
		{
			FontUnit f1 = new FontUnit (FontSize.Large);
			
			Assert.AreEqual (f1.Type, FontSize.Large, "A1");
			Assert.AreEqual (f1.Unit, Unit.Empty, "A1.1");
		}

		[Test]
		public void FontUnitConstructors2 ()
		{
			// Test the AsUnit values
			FontUnit f1 = new FontUnit (FontSize.AsUnit);
			Assert.AreEqual (f1.Type, FontSize.AsUnit, "A2");
			Assert.AreEqual (f1.Unit.Type, UnitType.Point, "A3");
			Assert.AreEqual (f1.Unit.Value, 10, "A4");
		}

		[Test]
		public void FontUnitConstructors3 ()
		{
			FontUnit f1 = new FontUnit (15);
			Assert.AreEqual (f1.Type, FontSize.AsUnit, "A5");
			Assert.AreEqual (f1.Unit.Type, UnitType.Point, "A6");
			Assert.AreEqual (f1.Unit.Value, 15, "A7");
		}

		[Test]
		public void FontUnitConstructors4 ()
		{
			// Test the string constructor: null and empty
			FontUnit f1 = new FontUnit (null);
			Assert.AreEqual (f1.Type, FontSize.NotSet, "A8");
			Assert.AreEqual (f1.Unit.IsEmpty, true, "A9");
		}

		[Test]
		public void FontUnitConstructors5 ()
		{
			FontUnit f1 = new FontUnit ("");
			Assert.AreEqual (f1.Type, FontSize.NotSet, "A10");
			Assert.AreEqual (f1.Unit.IsEmpty, true, "A11");
		}

#if NET_2_0
		[Test]
		public void FontUnitConstructors6 ()
		{
			FontUnit f1 = new FontUnit (2.5);
			Assert.AreEqual (f1.Type, FontSize.AsUnit, "A12");
			Assert.AreEqual (f1.Unit.Type, UnitType.Point, "A13");
			Assert.AreEqual (f1.Unit.Value, 2.5, "A14");
		}

		[Test]
		public void FontUnitConstructors7 ()
		{
			FontUnit f1 = new FontUnit (5.0, UnitType.Percentage);
			Assert.AreEqual (f1.Type, FontSize.AsUnit, "A15");
			Assert.AreEqual (f1.Unit.Type, UnitType.Percentage, "A17");
			Assert.AreEqual (f1.Unit.Value, 5.0, "A18");
		}
#endif

		[Test]
		public void FontUnitConstructors_Em ()
		{
			CultureInfo originalCulture = CultureInfo.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("nl-BE");
			try {
				FontUnit fu = new FontUnit ("4,5em");
				Assert.AreEqual (FontSize.AsUnit, fu.Type, "#1");
				Assert.AreEqual (UnitType.Em, fu.Unit.Type, "#2");
				Assert.AreEqual (4.5, fu.Unit.Value, "#3");
				Assert.AreEqual ("4,5em", fu.ToString (), "#4");
			} finally {
				// restore original culture
				Thread.CurrentThread.CurrentCulture = originalCulture;
			}
		}

		[Test]
		public void FontUnitConstructors_Pixel ()
		{
			FontUnit f1 = new FontUnit ("10px");
			Assert.AreEqual (FontSize.AsUnit, f1.Type, "#1");
			Assert.AreEqual (UnitType.Pixel, f1.Unit.Type, "#2");
			Assert.AreEqual (10, f1.Unit.Value, "#3");
			Assert.AreEqual ("10px", f1.ToString (), "#4");
		}

		[Test]
		public void FontUnitConstructors_Point ()
		{
			CultureInfo originalCulture = CultureInfo.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("nl-BE");
			try {
				FontUnit f1 = new FontUnit ("12,5pt");
				Assert.AreEqual (FontSize.AsUnit, f1.Type, "Type");
				Assert.AreEqual (UnitType.Point, f1.Unit.Type, "Unit.Type");
				Assert.AreEqual (12.5, f1.Unit.Value, "Unit.Value");
				Assert.AreEqual ("12,5pt", f1.ToString (), "ToString");
			} finally {
				// restore original culture
				Thread.CurrentThread.CurrentCulture = originalCulture;
			}
		}

		[Test]
		public void FontUnitConstructors_Enum1 ()
		{
			FontUnit fu = new FontUnit ("Large");
			Assert.AreEqual (FontSize.Large, fu.Type, "Large");
			Assert.IsTrue (fu.Unit.IsEmpty, "Large.IsEmpty");
			Assert.AreEqual ("Large", fu.ToString (), "Large.ToString");
		}

		[Test]
		public void FontUnitConstructors_Enum2 ()
		{
			FontUnit fu = new FontUnit ("Larger");
			Assert.AreEqual (FontSize.Larger, fu.Type, "Larger");
			Assert.IsTrue (fu.Unit.IsEmpty, "Larger.IsEmpty");
			Assert.AreEqual ("Larger", fu.ToString (), "Larger.ToString");
		}

		[Test]
		public void FontUnitConstructors_Enum3 ()
		{
			FontUnit fu = new FontUnit ("Medium");
			Assert.AreEqual (FontSize.Medium, fu.Type, "Medium");
			Assert.IsTrue (fu.Unit.IsEmpty, "Medium.IsEmpty");
			Assert.AreEqual ("Medium", fu.ToString (), "Medium.ToString");
		}

		[Test]
		public void FontUnitConstructors_Enum4 ()
		{
			FontUnit fu = new FontUnit ("Small");
			Assert.AreEqual (FontSize.Small, fu.Type, "Small");
			Assert.IsTrue (fu.Unit.IsEmpty, "Small.IsEmpty");
			Assert.AreEqual ("Small", fu.ToString (), "Small.ToString");
		}

		[Test]
		public void FontUnitConstructors_Enum5 ()
		{
			FontUnit fu = new FontUnit ("Smaller");
			Assert.AreEqual (FontSize.Smaller, fu.Type, "Smaller");
			Assert.IsTrue (fu.Unit.IsEmpty, "Smaller.IsEmpty");
			Assert.AreEqual ("Smaller", fu.ToString (), "Smaller.ToString");
		}

		[Test]
		public void FontUnitConstructors_Enum6 ()
		{
			FontUnit fu = new FontUnit ("XLarge");
			Assert.AreEqual (FontSize.XLarge, fu.Type, "XLarge");
			Assert.IsTrue (fu.Unit.IsEmpty, "XLarge.IsEmpty");
			Assert.AreEqual ("X-Large", fu.ToString (), "XLarge.ToString");
		}

		[Test]
		public void FontUnitConstructors_Enum7 ()
		{
			FontUnit fu = new FontUnit ("X-Large");
			Assert.AreEqual (FontSize.XLarge, fu.Type, "X-Large");
			Assert.IsTrue (fu.Unit.IsEmpty, "X-Large.IsEmpty");
			Assert.AreEqual ("X-Large", fu.ToString (), "X-Large.ToString");
		}

		[Test]
		public void FontUnitConstructors_Enum9 ()
		{
			FontUnit fu = new FontUnit ("XSmall");
			Assert.AreEqual (FontSize.XSmall, fu.Type, "XSmall");
			Assert.IsTrue (fu.Unit.IsEmpty, "XSmall.IsEmpty");
			Assert.AreEqual ("X-Small", fu.ToString (), "XSmall.ToString");
		}

		[Test]
		public void FontUnitConstructors_Enum10 ()
		{
			FontUnit fu = new FontUnit ("X-Small");
			Assert.AreEqual (FontSize.XSmall, fu.Type, "X-Small");
			Assert.IsTrue (fu.Unit.IsEmpty, "X-Small.IsEmpty");
			Assert.AreEqual ("X-Small", fu.ToString (), "X-Small.ToString");
		}

		[Test]
		public void FontUnitConstructors_Enum11 ()
		{
			FontUnit fu = new FontUnit ("XXLarge");
			Assert.AreEqual (FontSize.XXLarge, fu.Type, "XXLarge");
			Assert.IsTrue (fu.Unit.IsEmpty, "XXLarge.IsEmpty");
			Assert.AreEqual ("XX-Large", fu.ToString (), "XXLarge.ToString");
		}

		[Test]
		public void FontUnitConstructors_Enum12 ()
		{
			FontUnit fu = new FontUnit ("XX-Large");
			Assert.AreEqual (FontSize.XXLarge, fu.Type, "XX-Large");
			Assert.IsTrue (fu.Unit.IsEmpty, "XX-Large.IsEmpty");
			Assert.AreEqual ("XX-Large", fu.ToString (), "XX-Large.ToString");
		}

		[Test]
		public void FontUnitConstructors_Enum13 ()
		{
			FontUnit fu = new FontUnit ("XXSmall");
			Assert.AreEqual (FontSize.XXSmall, fu.Type, "XXSmall");
			Assert.IsTrue (fu.Unit.IsEmpty, "XXSmall.IsEmpty");
			Assert.AreEqual ("XX-Small", fu.ToString (), "XXSmall.ToString");
		}

		[Test]
		public void FontUnitConstructors_Enum14 ()
		{
			FontUnit fu = new FontUnit ("XX-Small");
			Assert.AreEqual (FontSize.XXSmall, fu.Type, "XX-Small");
			Assert.IsTrue (fu.Unit.IsEmpty, "XX-Small.IsEmpty");
			Assert.AreEqual ("XX-Small", fu.ToString (), "XX-Small.ToString");
		}

		[Test]
		public void UnitEquality ()
		{
			FontUnit u1 = new FontUnit ("1px");
			FontUnit u2 = new FontUnit ("2px");
			FontUnit t1 = new FontUnit ("1px");
			FontUnit c2 = new FontUnit ("2cm");

			Assert.AreEqual (u1 == t1, true, "U1");
			Assert.AreEqual (u1 != u2, true, "U2");
			Assert.AreEqual (u1 == u2, false, "U3");
			Assert.AreEqual (u1 != t1, false, "U4");

			// Test that its comparing the units and value
			Assert.AreEqual (u2 != c2, true, "U5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IncorrectConstructor ()
		{
			FontUnit a = new FontUnit ((FontSize) (-1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IncorrectConstructor2 ()
		{
			FontUnit a = new FontUnit ((FontSize) (FontSize.XXLarge + 1));
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void FontUnitConstructors_Enum_AsUnit ()
		{
			new FontUnit ("AsUnit");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void FontUnitConstructors_Enum_NotSet ()
		{
			new FontUnit ("NotSet");
		}

#if NET_2_0
		class MyFormatProvider : IFormatProvider
		{
			public object GetFormat (Type format_type)
			{
				if (format_type.IsAssignableFrom (this.GetType ())) {
					return this;
				}
				return null;
			}
		}

		[Test]
		public void FontUnit_IFormatProviderToString ()
		{
			CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
			CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;

			try {
				CultureInfo ci = CultureInfo.GetCultureInfo ("en-US");
				Thread.CurrentThread.CurrentCulture = ci;
				Thread.CurrentThread.CurrentUICulture = ci;
				RunFontUnit_IFormatProviderToString_Tests ();
			} finally {
				Thread.CurrentThread.CurrentCulture = currentCulture;
				Thread.CurrentThread.CurrentUICulture = currentUICulture;
			}
		}

		void RunFontUnit_IFormatProviderToString_Tests ()
		{
			MyFormatProvider mfp = new MyFormatProvider ();

			FontUnit f1 = new FontUnit (FontSize.Large);
			Assert.AreEqual ("Large", f1.ToString (mfp), "T1");

			f1 = new FontUnit (FontSize.AsUnit);
			Assert.AreEqual ("10pt", f1.ToString (mfp), "T2");

			f1 = new FontUnit (15);
			Assert.AreEqual ("15pt", f1.ToString (mfp), "T3");

			f1 = new FontUnit (null);
			Assert.AreEqual ("", f1.ToString (mfp), "T4");

			f1 = new FontUnit ("");
			Assert.AreEqual ("", f1.ToString (mfp), "T5");

			f1 = new FontUnit (2.5);
			Assert.AreEqual ("2.5pt", f1.ToString (mfp), "T6");

			f1 = new FontUnit (5.0, UnitType.Percentage);
			Assert.AreEqual ("5%", f1.ToString (mfp), "T7");
		}
#endif
	}
}
