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
// Copyright (c) 2018 Filip Navara <filip.navara@gmail.com>
//

using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class PrintPreviewControlTest
	{
		[Test]
		public void StartPage ()
		{
			if (PrinterSettings.InstalledPrinters.Count == 0)
				Assert.Ignore ("The test depends on printer being available.");

			using (Form f = new Form ())
			{
				PrintPreviewControl p = new PrintPreviewControl ();
				f.Controls.Add (p);
				f.Show ();

				Assert.AreEqual (0, p.StartPage);
				Assert.AreEqual (1, p.Rows);
				Assert.AreEqual (1, p.Columns);
				p.StartPage = 4;
				Assert.AreEqual (4, p.StartPage);

				PrintDocument document = new PrintDocument ();
				int page_number = 0;
				int page_count = 1;
				document.BeginPrint += (sender, e) => page_number = 0;
				document.PrintPage += (sender, e) => e.HasMorePages = ++page_number < page_count;

				p.Document = document;
				p.Refresh ();
				Assert.AreEqual (0, p.StartPage);
				
				page_count = 8;
				p.InvalidatePreview ();
				p.Refresh ();
				Assert.AreEqual (4, p.StartPage);
			}
		}
	}
}
