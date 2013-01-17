//
// PrintDialogTest.cs: Tests for PrintDialog class.
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
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Carlos Alberto Cortez <calberto.cortez@gmail.com>
//

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class PrintDialogTest : TestHelper
	{
		[Test]
		[Category("Printing")]
		public void DefaultValues ()
		{
			PrintDialog pd = new PrintDialog ();

			Assert.IsTrue (pd.AllowPrintToFile, "#1");
			Assert.IsFalse (pd.AllowSelection, "#2");
			Assert.IsFalse (pd.AllowSomePages, "#3");
			Assert.IsNull (pd.Document, "#4");
#if NET_2_0
			Assert.IsNotNull (pd.PrinterSettings, "#5");
#else
			Assert.IsNull (pd.PrinterSettings, "#5");
#endif
			Assert.IsFalse (pd.PrintToFile, "#6");
			Assert.IsFalse (pd.ShowHelp, "#7");
			Assert.IsTrue (pd.ShowNetwork, "#8");
		}

		[Test]
		[Category("Printing")]
		public void DocumentTest ()
		{
			PrintDialog pd = new PrintDialog ();

			PrintDocument pdoc1 = new PrintDocument ();
			PrinterSettings ps1 = new PrinterSettings ();
			pdoc1.PrinterSettings = ps1;
			pd.Document = pdoc1;
			Assert.AreEqual (pdoc1, pd.Document, "#1");
			Assert.AreEqual (ps1, pd.PrinterSettings, "#2");

			PrinterSettings ps2 = new PrinterSettings ();
			pdoc1.PrinterSettings = ps2;
			pd.Document = pdoc1;
			Assert.AreEqual (pdoc1, pd.Document, "#3");
			Assert.AreEqual (ps2, pd.PrinterSettings, "#4");

			pd.Document = null;
			Assert.IsNull (pd.Document, "#5");
			Assert.IsNotNull (pd.PrinterSettings, "#6");
			if (pd.PrinterSettings == ps1)
				Assert.Fail ("#7");
		}

		[Test]
		[Category("Printing")]
		public void PrinterSettingsTest ()
		{
			PrintDialog pd = new PrintDialog ();

			PrinterSettings ps1 = new PrinterSettings ();
			pd.PrinterSettings = ps1;
			Assert.AreEqual (ps1, pd.PrinterSettings, "#1");
			Assert.IsNull (pd.Document, "#2");

			pd.PrinterSettings = null;
			Assert.IsNotNull (pd.PrinterSettings, "#3");
			Assert.IsNull (pd.Document, "#4");
			if (pd.PrinterSettings == ps1)
				Assert.Fail ("#5");
		}
	}
}

