//
// TableStyleTest.cs
//	- Unit tests for System.Web.UI.WebControls.TableStyle
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

	public class TestTableStyle : TableStyle {

		public TestTableStyle ()
			: base ()
		{
		}

		public TestTableStyle (StateBag bag)
			: base (bag)
		{
		}

		public bool Empty {
			get { return base.IsEmpty; }
		}

		public StateBag StateBag {
			get { return base.ViewState; }
		}

		public void LoadVS (object o)
		{
			LoadViewState (o);
		}

		public void TrackVS ()
		{
			TrackViewState ();
		}

		public object SaveVS ()
		{
			return SaveViewState ();
		}

		public void Fill (CssStyleCollection attributes, IUrlResolutionService urlResolver)
		{
			base.FillStyleAttributes (attributes, urlResolver);
		}

		public bool SetBitCalledFlag = false;
		public int SetBitCalledValue = 0;
		protected internal override void SetBit (int bit) {
			SetBitCalledFlag = true;
			SetBitCalledValue = bit;
			base.SetBit (bit);
		}
	}

#if NET_2_0
	public class TestResolutionService : IUrlResolutionService {

		public string ResolveClientUrl (string relativeUrl)
		{
			return "http://www.mono-project.com";
		}
	}
#endif

	[TestFixture]
	public class TableStyleTest {

		private const string imageUrl = "http://www.mono-project.com/stylesheets/images.wiki.png";

		private void DefaultProperties (TestTableStyle ts)
		{
			Assert.AreEqual (0, ts.StateBag.Count, "ViewState.Count");

			Assert.AreEqual (String.Empty, ts.BackImageUrl, "BackImageUrl");
			Assert.AreEqual (-1, ts.CellPadding, "CellPadding");
			Assert.AreEqual (-1, ts.CellSpacing, "CellSpacing");
			// LAMESPEC: default is document to be GridLines.Both
			Assert.AreEqual (GridLines.None, ts.GridLines, "GridLines");
			Assert.AreEqual (HorizontalAlign.NotSet, ts.HorizontalAlign, "HorizontalAlign");

			Assert.AreEqual (0, ts.StateBag.Count, "ViewState.Count-2");
			ts.Reset ();
			Assert.AreEqual (0, ts.StateBag.Count, "Reset");
		}

		private void NullProperties (TestTableStyle ts)
		{
			Assert.IsTrue (ts.Empty, "Empty");
			ts.BackImageUrl = String.Empty; // doesn't accept null, see specific test
			Assert.AreEqual (String.Empty, ts.BackImageUrl, "BackImageUrl");
			Assert.IsFalse (ts.Empty, "!Empty");

			ts.CellPadding = -1;
			Assert.AreEqual (-1, ts.CellPadding, "CellPadding");
			ts.CellSpacing = -1;
			Assert.AreEqual (-1, ts.CellSpacing, "CellSpacing");
			ts.GridLines = GridLines.None;
			Assert.AreEqual (GridLines.None, ts.GridLines, "GridLines");
			ts.HorizontalAlign = HorizontalAlign.NotSet;
			Assert.AreEqual (HorizontalAlign.NotSet, ts.HorizontalAlign, "HorizontalAlign");

			Assert.AreEqual (5, ts.StateBag.Count, "ViewState.Count-1");
			ts.Reset ();
			Assert.AreEqual (0, ts.StateBag.Count, "Reset");
			Assert.IsTrue (ts.Empty, "Empty/Reset");
		}

		[Test]
		public void Constructor_Default ()
		{
			TestTableStyle ts = new TestTableStyle ();
			DefaultProperties (ts);
			NullProperties (ts);
		}

		[Test]
		public void Constructor_StateBag_Null ()
		{
			TestTableStyle ts = new TestTableStyle (null);
			Assert.IsNotNull (ts.StateBag, "StateBag");
			DefaultProperties (ts);
			NullProperties (ts);
		}

		[Test]
		public void Constructor_StateBag ()
		{
			TestTableStyle ts = new TestTableStyle (new StateBag ());
			Assert.IsNotNull (ts.StateBag, "StateBag");
			DefaultProperties (ts);
			NullProperties (ts);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void BackImageUrl_Null ()
		{
			TableStyle ts = new TableStyle ();
			ts.BackImageUrl = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CellPadding_Invalid ()
		{
			TableStyle ts = new TableStyle ();
			ts.CellPadding = Int32.MinValue;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CellSpacing_Invalid ()
		{
			TableStyle ts = new TableStyle ();
			ts.CellSpacing = Int32.MinValue;
		}

		[Test]
		// LAMESPEC: documented as ArgumentException
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GridLines_Invalid ()
		{
			TableStyle ts = new TableStyle ();
			ts.GridLines = (GridLines)Int32.MinValue;
		}

		[Test]
		// LAMESPEC: documented as ArgumentException
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void HorizontalAlign_Invalid ()
		{
			TableStyle ts = new TableStyle ();
			ts.HorizontalAlign = (HorizontalAlign)Int32.MinValue;
		}

		[Test]
		public void AddAttributesToRender_Null_WebControl ()
		{
			TableStyle ts = new TableStyle ();
			ts.AddAttributesToRender (null, new Table ());
			// no exception
		}

		[Test]
		public void AddAttributesToRender_HtmlTextWriter_Null ()
		{
			TableStyle ts = GetTableStyle ();
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			ts.AddAttributesToRender (writer, null);
			Assert.AreEqual (String.Empty, writer.InnerWriter.ToString (), "empty");
		}

		[Test]
		public void AddAttributesToRender ()
		{
			TableStyle ts = GetTableStyle ();
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			ts.AddAttributesToRender (writer, new Table ());
			Assert.AreEqual (String.Empty, writer.InnerWriter.ToString (), "empty");
		}

		private TableStyle GetTableStyle ()
		{
			TableStyle ts = new TableStyle ();
			ts.BackImageUrl = imageUrl;
			ts.CellPadding = 1;
			ts.CellSpacing = 2;
			ts.GridLines = GridLines.Both;
			ts.HorizontalAlign = HorizontalAlign.Justify;
			return ts;
		}

		private void CheckTableStyle (TableStyle ts)
		{
			Assert.AreEqual (imageUrl, ts.BackImageUrl, "BackImageUrl");
			Assert.AreEqual (1, ts.CellPadding, "CellPadding");
			Assert.AreEqual (2, ts.CellSpacing, "CellSpacing");
			Assert.AreEqual (GridLines.Both, ts.GridLines, "GridLines");
			Assert.AreEqual (HorizontalAlign.Justify, ts.HorizontalAlign, "HorizontalAlign");
		}

		[Test]
		public void CopyFrom_Null ()
		{
			TableStyle ts = GetTableStyle ();
			ts.CopyFrom (null);
			CheckTableStyle (ts);
		}

		[Test]
		public void CopyFrom_Self ()
		{
			TableStyle ts = GetTableStyle ();
			ts.CopyFrom (ts);
			CheckTableStyle (ts);
		}

		[Test]
		public void CopyFrom_Empty ()
		{
			TestTableStyle ts = new TestTableStyle ();
			ts.CopyFrom (new TableStyle ());
			DefaultProperties (ts);
		}

		[Test]
		public void CopyFrom_IsEmpty ()
		{
			TestTableStyle c = new TestTableStyle ();
			TableStyle s = new TableStyle ();

			
			s.BorderWidth = Unit.Empty;
			c.CopyFrom (s);
			Assert.IsTrue (c.Empty, "A1");
			
			s.GridLines = GridLines.Both;
			c.CopyFrom (s);
			Assert.IsFalse (c.Empty, "A2");
		}

		[Test]
		public void CopyFrom ()
		{
			TableStyle ts = new TableStyle ();
			ts.BackImageUrl = imageUrl + "1";
			ts.CellPadding = 2;
			ts.CellSpacing = 3;
			ts.GridLines = GridLines.Horizontal;
			ts.HorizontalAlign = HorizontalAlign.Left;

			ts.CopyFrom (GetTableStyle ());
			CheckTableStyle (ts);
		}

		[Test]
		public void MergeWith_Null ()
		{
			TableStyle ts = GetTableStyle ();
			ts.MergeWith (null);
			CheckTableStyle (ts);
		}

		[Test]
		public void MergeWith_Self ()
		{
			TableStyle ts = GetTableStyle ();
			ts.MergeWith (ts);
			CheckTableStyle (ts);
		}

		[Test]
		public void MergeWith_Empty ()
		{
			TestTableStyle ts = new TestTableStyle ();
			ts.MergeWith (new TableStyle ());
			DefaultProperties (ts);
		}

		[Test]
		public void MergeWith ()
		{
			TableStyle ts = new TableStyle ();
			ts.BackImageUrl = imageUrl + "1";
			ts.CellPadding = 2;
			ts.CellSpacing = 3;
			ts.GridLines = GridLines.Horizontal;
			ts.HorizontalAlign = HorizontalAlign.Left;

			ts.MergeWith (GetTableStyle ());

			Assert.AreEqual (imageUrl + "1", ts.BackImageUrl, "BackImageUrl");
			Assert.AreEqual (2, ts.CellPadding, "CellPadding");
			Assert.AreEqual (3, ts.CellSpacing, "CellSpacing");
			Assert.AreEqual (GridLines.Horizontal, ts.GridLines, "GridLines");
			Assert.AreEqual (HorizontalAlign.Left, ts.HorizontalAlign, "HorizontalAlign");
		}

		[Test]
		public void GridLines_VS ()
		{
			TestTableStyle ts = new TestTableStyle ();
			ts.TrackVS ();
			ts.GridLines = GridLines.Both;
			object o = ts.SaveVS ();
			ts = new TestTableStyle ();
			ts.LoadVS (o);
			Assert.AreEqual (GridLines.Both, ts.GridLines, "GL");
		}
#if NET_2_0
		private CssStyleCollection GetCssCollection ()
		{
			return new AttributeCollection (new StateBag ()).CssStyle;
		}

		[Test]
		public void FillStyleAttributes_Null_Resolver ()
		{
			TestTableStyle ts = new TestTableStyle ();
			ts.Fill (null, new TestResolutionService ());
			// no exception
		}

		[Test]
		public void FillStyleAttributes_Css_Null ()
		{
			TestTableStyle ts = new TestTableStyle ();
			ts.Fill (GetCssCollection (), null);
			// no exception
		}

		[Test]
		public void FillStyleAttributes_Empty ()
		{
			CssStyleCollection css = GetCssCollection ();
			TestTableStyle ts = new TestTableStyle ();
			ts.Fill (css, new TestResolutionService ());
			Assert.AreEqual (0, css.Count, "Count");
		}

		[Test]
		public void FillStyleAttributes_NotCss ()
		{
			CssStyleCollection css = GetCssCollection ();
			TestTableStyle ts = new TestTableStyle ();
			ts.CellPadding = 1;
			ts.CellSpacing = 1;
			ts.GridLines = GridLines.Both;
			ts.HorizontalAlign = HorizontalAlign.Justify;
			ts.Fill (css, new TestResolutionService ());
			Assert.AreEqual (0, css.Count, "Count");
		}

		[Test]
		public void FillStyleAttributes_Css_WithoutResolution ()
		{
			CssStyleCollection css = GetCssCollection ();
			TestTableStyle ts = new TestTableStyle ();
			ts.BackImageUrl = "http://www.go-mono.com";
			ts.Fill (css, null);
			Assert.AreEqual (1, css.Count, "Count");
			Assert.AreEqual ("http://www.go-mono.com", css["background-image"], "css[string]");
			Assert.AreEqual ("http://www.go-mono.com", css[HtmlTextWriterStyle.BackgroundImage], "css[HtmlTextWriterStyle]");
			Assert.AreEqual ("background-image:url(http://www.go-mono.com);", css.Value, "css.Value");
		}

		[Test]
		public void FillStyleAttributes_Css_WithResolution ()
		{
			CssStyleCollection css = GetCssCollection ();
			TestTableStyle ts = new TestTableStyle ();
			ts.BackImageUrl = "http://www.go-mono.com";
			ts.Fill (css, new TestResolutionService ());
			Assert.AreEqual (1, css.Count, "Count");
			Assert.AreEqual ("http://www.mono-project.com", css["background-image"], "css[string]");
			Assert.AreEqual ("http://www.mono-project.com", css[HtmlTextWriterStyle.BackgroundImage], "css[HtmlTextWriterStyle]");
			Assert.AreEqual ("background-image:url(http://www.mono-project.com);", css.Value, "css.Value");
			Assert.AreEqual ("http://www.go-mono.com", ts.BackImageUrl, "BackImageUrl");
		}
#endif
		[Test]
		[Category ("NotWorking")]
		public void BackImageUrl ()
		{
			TableStyle ts = new TableStyle ();
			ts.BackImageUrl = "test 1.jpg";
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			HtmlTextWriter htw = new HtmlTextWriter (sw);
			ts.AddAttributesToRender (htw);
			htw.RenderBeginTag ("tagName");
			string res = htw.InnerWriter.ToString ();
			string expected = "<tagName style=\"background-image:url(";
#if NET_2_0
			expected += "test%201.jpg";
#else
			expected += "test 1.jpg";
#endif
			expected += ");\">\n";
			Assert.AreEqual (expected, res);
		}

		[Test]
		public void SetBitCalledWhenSetProperty () {
			TestTableStyle s = new TestTableStyle ();

			s.SetBitCalledFlag = false;
			s.BackImageUrl = "http://www.mono-project.com";
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : BackImageUrl");
			Assert.AreEqual (0x10000, s.SetBitCalledValue, "SetBit() was called with wrong argument : BackImageUrl");

			s.SetBitCalledFlag = false;
			s.CellPadding = 1;
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : CellPadding");
			Assert.AreEqual (0x20000, s.SetBitCalledValue, "SetBit() was called with wrong argument : CellPadding");

			s.SetBitCalledFlag = false;
			s.CellSpacing = 1;
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : CellSpacing");
			Assert.AreEqual (0x40000, s.SetBitCalledValue, "SetBit() was called with wrong argument : CellSpacing");

			s.SetBitCalledFlag = false;
			s.GridLines = GridLines.Vertical;
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : GridLines");
			Assert.AreEqual (0x80000, s.SetBitCalledValue, "SetBit() was called with wrong argument : GridLines");

			s.SetBitCalledFlag = false;
			s.HorizontalAlign = HorizontalAlign.Right;
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : HorizontalAlign");
			Assert.AreEqual (0x100000, s.SetBitCalledValue, "SetBit() was called with wrong argument : HorizontalAlign");
		}
	}
}
