//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
// Author:
//
//	Jordi Mas i Hernandez, jordimash@gmail.com
//

using NUnit.Framework;
using System;
using System.IO;
using System.Drawing.Printing;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Drawing.Printing {

	[TestFixture]
	public class PrinterUnitConvertTest
	{
		static int n = 100, r;

		[Test]
		public void ConvertFromDisplay ()
		{
			r = PrinterUnitConvert.Convert (n, PrinterUnit.Display,
				PrinterUnit.Display);

			Assert.AreEqual (100, r, "CFD#1");

			r = PrinterUnitConvert.Convert (n, PrinterUnit.Display,
				PrinterUnit.HundredthsOfAMillimeter);

			Assert.AreEqual (2540, r, "CFD#2");

			r = PrinterUnitConvert.Convert (n, PrinterUnit.Display,
				PrinterUnit.TenthsOfAMillimeter);

			Assert.AreEqual (254, r, "CFD#3");

			r = PrinterUnitConvert.Convert (n, PrinterUnit.Display,
				PrinterUnit.ThousandthsOfAnInch);

			Assert.AreEqual (1000, r, "CFD#4");
		}

		[Test]
		public void ConvertFromHundredthsOfAMillimeter ()
		{
			r = PrinterUnitConvert.Convert (n, PrinterUnit.HundredthsOfAMillimeter,
				PrinterUnit.Display);

			Assert.AreEqual (4, r, "CFH#1");

			r = PrinterUnitConvert.Convert (n, PrinterUnit.HundredthsOfAMillimeter,
				PrinterUnit.HundredthsOfAMillimeter);

			Assert.AreEqual (100, r, "CFH#2");

			r = PrinterUnitConvert.Convert (n, PrinterUnit.HundredthsOfAMillimeter,
				PrinterUnit.TenthsOfAMillimeter);

			Assert.AreEqual (10, r, "CFH#3");

			r = PrinterUnitConvert.Convert (n, PrinterUnit.HundredthsOfAMillimeter,
				PrinterUnit.ThousandthsOfAnInch);

			Assert.AreEqual (39, r, "CFH#4");
		}

		[Test]
		public void ConvertFromTenthsOfAMillimeter ()
		{
			r = PrinterUnitConvert.Convert (n, PrinterUnit.TenthsOfAMillimeter,
				PrinterUnit.Display);

			Assert.AreEqual (39, r, "CFT#1");

			r = PrinterUnitConvert.Convert (n, PrinterUnit.TenthsOfAMillimeter,
				PrinterUnit.HundredthsOfAMillimeter);

			Assert.AreEqual (1000, r, "CFT#2");

			r = PrinterUnitConvert.Convert (n, PrinterUnit.TenthsOfAMillimeter,
				PrinterUnit.TenthsOfAMillimeter);

			Assert.AreEqual (100, r, "CFT#3");

			r = PrinterUnitConvert.Convert (n, PrinterUnit.TenthsOfAMillimeter,
				PrinterUnit.ThousandthsOfAnInch);

			Assert.AreEqual (394, r, "CFT#4");
		}

		[Test]
		public void ConvertFromThousandthsOfAnInch ()
		{
			r = PrinterUnitConvert.Convert (n, PrinterUnit.ThousandthsOfAnInch,
				PrinterUnit.Display);

			Assert.AreEqual (10, r, "CFI#1");

			r = PrinterUnitConvert.Convert (n, PrinterUnit.ThousandthsOfAnInch,
				PrinterUnit.HundredthsOfAMillimeter);

			Assert.AreEqual (254, r, "CFI#2");

			r = PrinterUnitConvert.Convert (n, PrinterUnit.ThousandthsOfAnInch,
				PrinterUnit.TenthsOfAMillimeter);

			Assert.AreEqual (25, r, "CFI#3");

			r = PrinterUnitConvert.Convert (n, PrinterUnit.ThousandthsOfAnInch,
				PrinterUnit.ThousandthsOfAnInch);

			Assert.AreEqual (100, r, "CFI#4");
		}
	}
}

