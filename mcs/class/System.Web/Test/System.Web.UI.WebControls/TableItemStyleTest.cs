//
// TableItemStyleTest.cs
//	- Unit tests for System.Web.UI.WebControls.TableItemStyle
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

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls {

	public class TestTableItemStyle : TableItemStyle {

		public TestTableItemStyle ()
			: base ()
		{
		}

		public TestTableItemStyle (StateBag bag)
			: base (bag)
		{
		}

		public bool Empty {
			get { return base.IsEmpty; }
		}

		public StateBag StateBag {
			get { return base.ViewState; }
		}

		public bool SetBitCalledFlag = false;
		public int SetBitCalledValue = 0;
		protected internal override void SetBit (int bit) {
			SetBitCalledFlag = true;
			SetBitCalledValue = bit;
			base.SetBit (bit);
		}
	}

	[TestFixture]
	public class TableItemStyleTest {

		private void DefaultProperties (TestTableItemStyle tis)
		{
			Assert.AreEqual (0, tis.StateBag.Count, "ViewState.Count");

			Assert.AreEqual (HorizontalAlign.NotSet, tis.HorizontalAlign, "HorizontalAlign");
			Assert.AreEqual (VerticalAlign.NotSet, tis.VerticalAlign, "VerticalAlign");
			Assert.IsTrue (tis.Wrap, "Wrap");

			Assert.AreEqual (0, tis.StateBag.Count, "ViewState.Count-2");
			tis.Reset ();
			Assert.AreEqual (0, tis.StateBag.Count, "Reset");
		}

		private void NullProperties (TestTableItemStyle tis)
		{
			Assert.IsTrue (tis.Empty, "Empty");

			tis.HorizontalAlign = HorizontalAlign.NotSet;
			Assert.AreEqual (HorizontalAlign.NotSet, tis.HorizontalAlign, "HorizontalAlign");
			Assert.IsFalse (tis.Empty, "!Empty");
			tis.VerticalAlign = VerticalAlign.NotSet;
			Assert.AreEqual (VerticalAlign.NotSet, tis.VerticalAlign, "VerticalAlign");
			tis.Wrap = true;
			Assert.IsTrue (tis.Wrap, "Wrap");

			Assert.AreEqual (3, tis.StateBag.Count, "ViewState.Count-1");
			tis.Reset ();
			Assert.AreEqual (0, tis.StateBag.Count, "Reset");
			Assert.IsTrue (tis.Empty, "Empty/Reset");
		}

		[Test]
		public void Constructor_Default ()
		{
			TestTableItemStyle tis = new TestTableItemStyle ();
			DefaultProperties (tis);
			NullProperties (tis);
		}

		[Test]
		public void Constructor_StateBag_Null ()
		{
			TestTableItemStyle tis = new TestTableItemStyle (null);
			Assert.IsNotNull (tis.StateBag, "StateBag");
			DefaultProperties (tis);
			NullProperties (tis);
		}

		[Test]
		public void Constructor_StateBag ()
		{
			TestTableItemStyle tis = new TestTableItemStyle (new StateBag ());
			Assert.IsNotNull (tis.StateBag, "StateBag");
			DefaultProperties (tis);
			NullProperties (tis);
		}

		[Test]
		// LAMESPEC: documented as ArgumentException
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void HorizontalAlign_Invalid ()
		{
			TableItemStyle tis = new TableItemStyle ();
			tis.HorizontalAlign = (HorizontalAlign)Int32.MinValue;
		}

		[Test]
		// LAMESPEC: documented as ArgumentException
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void VerticalAlign_Invalid ()
		{
			TableItemStyle tis = new TableItemStyle ();
			tis.VerticalAlign = (VerticalAlign)Int32.MinValue;
		}

		[Test]
		public void AddAttributesToRender_Null_WebControl ()
		{
			TableItemStyle tis = new TableItemStyle ();
			tis.AddAttributesToRender (null, new TableRow ());
			// no exception
		}

		[Test]
		public void AddAttributesToRender_HtmlTextWriter_Null ()
		{
			TableItemStyle tis = GetTableItemStyle ();
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			tis.AddAttributesToRender (writer, null);
			Assert.AreEqual (String.Empty, writer.InnerWriter.ToString (), "empty");
		}

		[Test]
		public void AddAttributesToRender ()
		{
			TableItemStyle tis = GetTableItemStyle ();
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			tis.AddAttributesToRender (writer, new Table ());
			Assert.AreEqual (String.Empty, writer.InnerWriter.ToString (), "empty");
		}

		private TableItemStyle GetTableItemStyle ()
		{
			TableItemStyle tis = new TableItemStyle ();
			tis.HorizontalAlign = HorizontalAlign.Justify;
			tis.VerticalAlign = VerticalAlign.Bottom;
			tis.Wrap = false;
			return tis;
		}

		private void CheckTableStyle (TableItemStyle tis)
		{
			Assert.AreEqual (HorizontalAlign.Justify, tis.HorizontalAlign, "HorizontalAlign");
			Assert.AreEqual (VerticalAlign.Bottom, tis.VerticalAlign, "VerticalAlign");
			Assert.IsFalse (tis.Wrap, "Wrap");
		}

		[Test]
		public void CopyFrom_Null ()
		{
			TableItemStyle tis = GetTableItemStyle ();
			tis.CopyFrom (null);
			CheckTableStyle (tis);
		}

		[Test]
		public void CopyFrom_Self ()
		{
			TableItemStyle tis = GetTableItemStyle ();
			tis.CopyFrom (tis);
			CheckTableStyle (tis);
		}

		[Test]
		public void CopyFrom_Empty ()
		{
			TestTableItemStyle tis = new TestTableItemStyle ();
			tis.CopyFrom (new TableItemStyle ());
			DefaultProperties (tis);
		}

		[Test]
		public void CopyFrom ()
		{
			TableItemStyle tis = new TableItemStyle ();
			tis.HorizontalAlign = HorizontalAlign.Left;
			tis.VerticalAlign = VerticalAlign.Top;
			tis.Wrap = true;

			tis.CopyFrom (GetTableItemStyle ());
			CheckTableStyle (tis);
		}

		[Test]
		public void MergeWith_Null ()
		{
			TableItemStyle tis = GetTableItemStyle ();
			tis.MergeWith (null);
			CheckTableStyle (tis);
		}

		[Test]
		public void MergeWith_Self ()
		{
			TableItemStyle tis = GetTableItemStyle ();
			tis.MergeWith (tis);
			CheckTableStyle (tis);
		}

		[Test]
		public void MergeWith_Empty ()
		{
			TestTableItemStyle tis = new TestTableItemStyle ();
			tis.MergeWith (new TableItemStyle ());
			DefaultProperties (tis);
		}

		[Test]
		public void MergeWith ()
		{
			TableItemStyle tis = new TableItemStyle ();
			tis.HorizontalAlign = HorizontalAlign.Left;
			tis.VerticalAlign = VerticalAlign.Top;
			tis.Wrap = true;

			tis.MergeWith (GetTableItemStyle ());

			Assert.AreEqual (HorizontalAlign.Left, tis.HorizontalAlign, "HorizontalAlign");
			Assert.AreEqual (VerticalAlign.Top, tis.VerticalAlign, "VerticalAlign");
			Assert.IsTrue (tis.Wrap, "Wrap");
		}

		[Test]
		public void SetBitCalledWhenSetProperty () {
			TestTableItemStyle s = new TestTableItemStyle ();

			s.SetBitCalledFlag = false;
			s.HorizontalAlign = HorizontalAlign.Right;
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : HorizontalAlign");
			Assert.AreEqual (0x10000, s.SetBitCalledValue, "SetBit() was called with wrong argument : HorizontalAlign");

			s.SetBitCalledFlag = false;
			s.VerticalAlign = VerticalAlign.Bottom;
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : VerticalAlign");
			Assert.AreEqual (0x20000, s.SetBitCalledValue, "SetBit() was called with wrong argument : VerticalAlign");

			s.SetBitCalledFlag = false;
			s.Wrap = false;
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : Wrap");
			Assert.AreEqual (0x40000, s.SetBitCalledValue, "SetBit() was called with wrong argument : Wrap");
		}
	}
}
