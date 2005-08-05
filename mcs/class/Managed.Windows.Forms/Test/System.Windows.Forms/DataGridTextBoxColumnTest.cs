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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Jordi Mas i Hernandez <jordi@ximian.com>
//
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	class DataGridTextBoxColumnTest
	{
		private bool eventhandled;
		private object Element;
		private CollectionChangeAction Action;

		[Test]
		public void TestDefaultValues ()
		{
			DataGridTextBoxColumn col = new DataGridTextBoxColumn ();

			Assert.AreEqual (HorizontalAlignment.Left, col.Alignment, "HorizontalAlignment property");
			Assert.AreEqual ("", col.HeaderText, "HeaderText property");
			Assert.AreEqual ("", col.MappingName, "MappingName property");
			Assert.AreEqual ("(null)", col.NullText, "NullText property");
			Assert.AreEqual (false, col.ReadOnly, "ReadOnly property");
			Assert.AreEqual (-1, col.Width, "Width property");
			Assert.AreEqual ("", col.Format, "Format property");
			Assert.AreEqual (null, col.FormatInfo, "FormatInfo property");
		}

		[Test]
		public void TestMappingNameChangedEvent ()
		{
			DataGridTextBoxColumn col = new DataGridTextBoxColumn ();
			eventhandled = false;
			col.MappingNameChanged += new EventHandler (OnEventHandler);
			col.MappingName = "name1";
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestAlignmentChangedEvent ()
		{
			DataGridTextBoxColumn col = new DataGridTextBoxColumn ();
			eventhandled = false;
			col.AlignmentChanged += new EventHandler (OnEventHandler);
			col.Alignment = HorizontalAlignment.Center;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestHeaderTextChangedEvent ()
		{
			DataGridTextBoxColumn col = new DataGridTextBoxColumn ();
			eventhandled = false;
			col.HeaderTextChanged += new EventHandler (OnEventHandler);
			col.HeaderText = "Header";
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestNullTextChangedEvent ()
		{
			DataGridTextBoxColumn col = new DataGridTextBoxColumn ();
			eventhandled = false;
			col.NullTextChanged += new EventHandler (OnEventHandler);
			col.NullText = "Null";
			Assert.AreEqual (true, eventhandled, "A1");
		}


		private void OnEventHandler (object sender, EventArgs e)
	        {
	            	eventhandled = true;
	        }
	}
}
