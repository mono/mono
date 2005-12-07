//
// TableSectionStyleTest.cs
//	- Unit tests for System.Web.UI.WebControls.TableSectionStyle
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

#if NET_2_0

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls {

	public class TestTableSectionStyle : TableSectionStyle {

		public TestTableSectionStyle ()
			: base ()
		{
		}

		public bool Empty {
			get { return base.IsEmpty; }
		}

		public StateBag StateBag {
			get { return base.ViewState; }
		}
	}

	[TestFixture]
	public class TableSectionStyleTest {

		private void DefaultProperties (TestTableSectionStyle tss)
		{
			Assert.AreEqual (0, tss.StateBag.Count, "ViewState.Count");

			Assert.IsTrue (tss.Visible, "Visible");

			Assert.AreEqual (0, tss.StateBag.Count, "ViewState.Count-2");
			tss.Reset ();
			Assert.AreEqual (0, tss.StateBag.Count, "Reset");
		}

		private void NullProperties (TestTableSectionStyle tss)
		{
			Assert.IsTrue (tss.Empty, "Empty");

			tss.Visible = false;
			Assert.IsFalse (tss.Visible, "Visible");

			Assert.IsTrue (tss.Empty, "!Empty"); // strange !!!

			Assert.AreEqual (1, tss.StateBag.Count, "ViewState.Count-1");
			tss.Reset ();
			
			// strange results because TableSectionStyle doesn't override
			// Reset
			Assert.AreEqual (1, tss.StateBag.Count, "Reset");
			Assert.IsTrue (tss.Empty, "Empty/Reset");
		}

		[Test]
		public void Constructor_Default ()
		{
			TestTableSectionStyle tss = new TestTableSectionStyle ();
			DefaultProperties (tss);
			NullProperties (tss);
		}

		private TableSectionStyle GetTableSectionStyle ()
		{
			TableSectionStyle tss = new TableSectionStyle ();
			tss.Visible = false;
			return tss;
		}


		[Test]
		public void CopyFrom_Null ()
		{
			TableSectionStyle tss = GetTableSectionStyle ();
			tss.CopyFrom (null);
			Assert.IsFalse (tss.Visible, "Visible");
		}

		[Test]
		public void CopyFrom_Self ()
		{
			TableSectionStyle tss = GetTableSectionStyle ();
			tss.CopyFrom (tss);
			Assert.IsFalse (tss.Visible, "Visible");
		}

		[Test]
		public void CopyFrom_Empty ()
		{
			TestTableSectionStyle tss = new TestTableSectionStyle ();
			tss.CopyFrom (new TableSectionStyle ());
			DefaultProperties (tss);
		}

		[Test]
		public void CopyFrom_IsEmpty ()
		{
			TestTableSectionStyle c = new TestTableSectionStyle ();
			TableSectionStyle s = new TableSectionStyle ();
			
			s.BorderWidth = Unit.Empty;
			c.CopyFrom (s);
			Assert.IsTrue (c.Empty, "A1");
			
			s.Visible = true;
			c.CopyFrom (s);
			// BUG -- setting Visible doesn't change the "emptyness" of this class ;-)
			Assert.IsTrue (c.Empty, "A2");
		}

		[Test]
		public void CopyFrom ()
		{
			TableSectionStyle tss = GetTableSectionStyle ();
			tss.Visible = true;

			tss.CopyFrom (GetTableSectionStyle ());
			// BUG - CopyFrom isn't overriden !!!
			Assert.IsTrue (tss.Visible, "Visible");
		}

		[Test]
		public void MergeWith_Null ()
		{
			TableSectionStyle tss = GetTableSectionStyle ();
			tss.MergeWith (null);
			Assert.IsFalse (tss.Visible, "Visible");
		}

		[Test]
		public void MergeWith_Self ()
		{
			TableSectionStyle tss = GetTableSectionStyle ();
			tss.MergeWith (tss);
			Assert.IsFalse (tss.Visible, "Visible");
		}

		[Test]
		public void MergeWith_Empty ()
		{
			TestTableSectionStyle tss = new TestTableSectionStyle ();
			tss.MergeWith (new TableSectionStyle ());
			DefaultProperties (tss);
		}

		[Test]
		public void MergeWith ()
		{
			TableSectionStyle tss = new TableSectionStyle ();
			tss.Visible = true;

			tss.MergeWith (GetTableSectionStyle ());

			Assert.IsTrue (tss.Visible, "Visible");
		}
	}
}

#endif
