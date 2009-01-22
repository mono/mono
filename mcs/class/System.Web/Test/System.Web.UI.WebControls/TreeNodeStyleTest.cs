//
// Tests for System.Web.UI.WebControls.ImageMap.cs
//
// Author:
//  Hagit Yidov (hagity@mainsoft.com
//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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
using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;
using System.Threading;

namespace MonoTests.System.Web.UI.WebControls {
	[TestFixture]
	public class TreeNodeStyleTest {

		[Test]
		public void TreeNodeStyle_DefaultProperties () {
			TreeNodeStyle tns = new TreeNodeStyle ();
			Assert.AreEqual (0.0, tns.ChildNodesPadding.Value, "ChildNodesPadding");
			Assert.AreEqual (0.0, tns.HorizontalPadding.Value, "HorizontalPadding");
			Assert.AreEqual ("", tns.ImageUrl, "ImageUrl");
			Assert.AreEqual (0.0, tns.NodeSpacing.Value, "NodeSpacing");
			Assert.AreEqual (0.0, tns.VerticalPadding.Value, "VerticalPadding");
		}

		[Test]
		public void TreeNodeStyle_AssignToDefaultProperties () {
			TreeNodeStyle tns = new TreeNodeStyle ();
			tns.ChildNodesPadding = 0;
			Assert.AreEqual (0.0, tns.ChildNodesPadding.Value, "ChildNodesPadding");
			tns.HorizontalPadding = 0;
			Assert.AreEqual (0.0, tns.HorizontalPadding.Value, "HorizontalPadding");
			tns.ImageUrl = "";
			Assert.AreEqual ("", tns.ImageUrl, "ImageUrl");
			tns.NodeSpacing = 0;
			Assert.AreEqual (0.0, tns.NodeSpacing.Value, "NodeSpacing");
			tns.VerticalPadding = 0;
			Assert.AreEqual (0.0, tns.VerticalPadding.Value, "VerticalPadding");
		}

		[Test]
		public void TreeNodeStyle_Method_CopyFrom () {
			TreeNodeStyle tns = new TreeNodeStyle ();
			TreeNodeStyle copy = new TreeNodeStyle ();
			tns.BorderStyle = BorderStyle.Double;
			tns.BorderWidth = 3;
			tns.ChildNodesPadding = 4;
			tns.Height = 5;
			tns.HorizontalPadding = 6;
			tns.ImageUrl = "ImageUrl";
			tns.NodeSpacing = 7;
			tns.VerticalPadding = 8;
			tns.Width = 9;
			copy.CopyFrom (tns);
			Assert.AreEqual (tns.BorderStyle, copy.BorderStyle, "BorderStyle");
			Assert.AreEqual (tns.BorderWidth, copy.BorderWidth, "Borderidth");
			Assert.AreEqual (tns.ChildNodesPadding, copy.ChildNodesPadding, "ChildNodesPadding");
			Assert.AreEqual (tns.Height, copy.Height, "Height");
			Assert.AreEqual (tns.HorizontalPadding, copy.HorizontalPadding, "HorizontalPadding");
			Assert.AreEqual (tns.ImageUrl, copy.ImageUrl, "ImageUrl");
			Assert.AreEqual (tns.NodeSpacing, copy.NodeSpacing, "NodeSpacing");
			Assert.AreEqual (tns.VerticalPadding, copy.VerticalPadding, "VerticalPadding");
			Assert.AreEqual (tns.Width, copy.Width, "Width");
		}

		[Test]
		public void TreeNodeStyle_Method_MergeWith () {
			TreeNodeStyle tns = new TreeNodeStyle ();
			TreeNodeStyle copy = new TreeNodeStyle ();
			tns.BorderStyle = BorderStyle.Double;
			tns.BorderWidth = 3;
			tns.ChildNodesPadding = 4;
			tns.Height = 5;
			tns.HorizontalPadding = 6;
			tns.ImageUrl = "ImageUrl";
			tns.NodeSpacing = 7;
			tns.VerticalPadding = 8;
			tns.Width = 9;
			copy.ImageUrl = "NewImageUrl";
			copy.NodeSpacing = 10;
			copy.VerticalPadding = 11;
			copy.Width = 12;
			copy.MergeWith (tns);
			Assert.AreEqual (tns.BorderStyle, copy.BorderStyle, "BorderStyle");
			Assert.AreEqual (tns.BorderWidth, copy.BorderWidth, "Borderidth");
			Assert.AreEqual (tns.ChildNodesPadding, copy.ChildNodesPadding, "ChildNodesPadding");
			Assert.AreEqual (tns.Height, copy.Height, "Height");
			Assert.AreEqual (tns.HorizontalPadding, copy.HorizontalPadding, "HorizontalPadding");
			Assert.AreEqual ("NewImageUrl", copy.ImageUrl, "ImageUrl1");
			Assert.AreEqual (Unit.Pixel (10), copy.NodeSpacing, "NodeSpacing1");
			Assert.AreEqual (Unit.Pixel (11), copy.VerticalPadding, "VerticalPadding1");
			Assert.AreEqual (Unit.Pixel (12), copy.Width, "Width1");
			Assert.AreEqual ("NewImageUrl", copy.ImageUrl, "ImageUrl2");
			Assert.AreEqual (10.0, copy.NodeSpacing.Value, "NodeSpacing2");
			Assert.AreEqual (11.0, copy.VerticalPadding.Value, "VerticalPadding2");
			Assert.AreEqual (12.0, copy.Width.Value, "Width2");
		}

		[Test]
		public void TreeNodeStyle_Method_Reset () {
			TreeNodeStyle tns = new TreeNodeStyle ();
			tns.BorderStyle = BorderStyle.Double;
			tns.BorderWidth = 3;
			tns.ChildNodesPadding = 4;
			tns.Height = 5;
			tns.HorizontalPadding = 6;
			tns.ImageUrl = "ImageUrl";
			tns.NodeSpacing = 7;
			tns.VerticalPadding = 8;
			tns.Width = 9;
			Assert.AreEqual (BorderStyle.Double, tns.BorderStyle, "BorderStyle1");
			Assert.AreEqual (3.0, tns.BorderWidth.Value, "BorderWidth1");
			Assert.AreEqual (4.0, tns.ChildNodesPadding.Value, "ChildNodesPadding1");
			Assert.AreEqual (5.0, tns.Height.Value, "Height1");
			Assert.AreEqual (6.0, tns.HorizontalPadding.Value, "HorizontalPadding1");
			Assert.AreEqual ("ImageUrl", tns.ImageUrl, "ImageUrl1");
			Assert.AreEqual (7.0, tns.NodeSpacing.Value, "NodeSpacing1");
			Assert.AreEqual (8.0, tns.VerticalPadding.Value, "VerticalPadding1");
			Assert.AreEqual (9.0, tns.Width.Value, "Width1");
			tns.Reset ();
			Assert.AreEqual (0.0, tns.BorderWidth.Value, "BorderWidth2");
			Assert.AreEqual (0.0, tns.ChildNodesPadding.Value, "ChildNodesPadding2");
			Assert.AreEqual (0.0, tns.Height.Value, "Height2");
			Assert.AreEqual (0.0, tns.HorizontalPadding.Value, "HorizontalPadding2");
			Assert.AreEqual ("", tns.ImageUrl, "ImageUrl2");
			Assert.AreEqual (0.0, tns.NodeSpacing.Value, "NodeSpacing2");
			Assert.AreEqual (0.0, tns.VerticalPadding.Value, "VerticalPadding2");
			Assert.AreEqual (0.0, tns.Width.Value, "Width2");
		}

		[Test]
		public void TreeNodeStyle_ViewState () {

			TreeNodeStyle orig = new TreeNodeStyle ();
			((IStateManager) orig).TrackViewState ();
			TreeNodeStyle copy = new TreeNodeStyle ();

			Assert.AreEqual (true, orig.IsEmpty, "TreeNodeStyle_ViewState");
			orig.ChildNodesPadding = 4;
			orig.HorizontalPadding = 6;
			orig.ImageUrl = "ImageUrl";
			orig.NodeSpacing = 7;
			orig.VerticalPadding = 8;
			Assert.AreEqual (false, orig.IsEmpty, "TreeNodeStyle_ViewState");

			object state = ((IStateManager) orig).SaveViewState ();
			((IStateManager) copy).LoadViewState (state);

			Assert.AreEqual (false, copy.IsEmpty, "TreeNodeStyle_ViewState");
			Assert.AreEqual (4.0, copy.ChildNodesPadding.Value, "TreeNodeStyle_ViewState");
			Assert.AreEqual (6.0, copy.HorizontalPadding.Value, "TreeNodeStyle_ViewState");
			Assert.AreEqual ("ImageUrl", copy.ImageUrl, "TreeNodeStyle_ViewState");
			Assert.AreEqual (7.0, copy.NodeSpacing.Value, "TreeNodeStyle_ViewState");
			Assert.AreEqual (8.0, copy.VerticalPadding.Value, "TreeNodeStyle_ViewState");
		}

	}
}


#endif
