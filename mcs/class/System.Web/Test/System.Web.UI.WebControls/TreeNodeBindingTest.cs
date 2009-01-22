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
	public class TreeNodeBindingTest {
		[Test]
		public void TreeNodeBinding_DefaultProperties () {
			TreeNodeBinding tnb = new TreeNodeBinding ();
			Assert.AreEqual (string.Empty, tnb.DataMember, "DataMember");
			Assert.AreEqual (-1, tnb.Depth, "Depth");
			Assert.AreEqual (string.Empty, tnb.FormatString, "FormatString");
			Assert.AreEqual (string.Empty, tnb.ImageToolTip, "ImageToolTip");
			Assert.AreEqual (string.Empty, tnb.ImageToolTipField, "ImageToolTipField");
			Assert.AreEqual (string.Empty, tnb.NavigateUrl, "NavigateUrl");
			Assert.AreEqual (string.Empty, tnb.NavigateUrlField, "NavigateUrlField");
			Assert.AreEqual (false, tnb.PopulateOnDemand, "PopulateOnDemand");
			Assert.AreEqual (TreeNodeSelectAction.Select, tnb.SelectAction, "SelectAction");
			Assert.AreEqual (null, tnb.ShowCheckBox, "ShowCheckBox");
			Assert.AreEqual (string.Empty, tnb.Target, "Target");
			Assert.AreEqual (string.Empty, tnb.TargetField, "TargetField");
			Assert.AreEqual (string.Empty, tnb.Text, "Text");
			Assert.AreEqual (string.Empty, tnb.TextField, "TextField");
			Assert.AreEqual (string.Empty, tnb.ToolTip, "ToolTip");
			Assert.AreEqual (string.Empty, tnb.ToolTipField, "ToolTipField");
			Assert.AreEqual (string.Empty, tnb.Value, "Value");
			Assert.AreEqual (string.Empty, tnb.ValueField, "ValueField");
		}

		[Test]
		public void TreeNodeBinding_AssignToDefaultProperties () {
			TreeNodeBinding tnb = new TreeNodeBinding ();

			tnb.DataMember = string.Empty;
			Assert.AreEqual (string.Empty, tnb.DataMember, "DataMember");

			tnb.Depth = -1;
			Assert.AreEqual (-1, tnb.Depth, "Depth");

			tnb.FormatString = string.Empty;
			Assert.AreEqual (string.Empty, tnb.FormatString, "FormatString");

			tnb.ImageToolTip = string.Empty;
			Assert.AreEqual (string.Empty, tnb.ImageToolTip, "ImageToolTip");

			tnb.ImageToolTipField = string.Empty;
			Assert.AreEqual (string.Empty, tnb.ImageToolTipField, "ImageToolTipField");

			tnb.NavigateUrl = string.Empty;
			Assert.AreEqual (string.Empty, tnb.NavigateUrl, "NavigateUrl");

			tnb.NavigateUrlField = string.Empty;
			Assert.AreEqual (string.Empty, tnb.NavigateUrlField, "NavigateUrlField");

			tnb.PopulateOnDemand = false;
			Assert.AreEqual (false, tnb.PopulateOnDemand, "PopulateOnDemand");

			tnb.SelectAction = TreeNodeSelectAction.Select;
			Assert.AreEqual (TreeNodeSelectAction.Select, tnb.SelectAction, "SelectAction");

			tnb.ShowCheckBox = null;
			Assert.AreEqual (null, tnb.ShowCheckBox, "ShowCheckBox");

			tnb.Target = string.Empty;
			Assert.AreEqual (string.Empty, tnb.Target, "Target");

			tnb.TargetField = string.Empty;
			Assert.AreEqual (string.Empty, tnb.TargetField, "TargetField");

			tnb.Text = string.Empty;
			Assert.AreEqual (string.Empty, tnb.Text, "Text");

			tnb.TextField = string.Empty;
			Assert.AreEqual (string.Empty, tnb.TextField, "TextField");

			tnb.ToolTip = string.Empty;
			Assert.AreEqual (string.Empty, tnb.ToolTip, "ToolTip");

			tnb.ToolTipField = string.Empty;
			Assert.AreEqual (string.Empty, tnb.ToolTipField, "ToolTipField");

			tnb.Value = string.Empty;
			Assert.AreEqual (string.Empty, tnb.Value, "Value");

			tnb.ValueField = string.Empty;
			Assert.AreEqual (string.Empty, tnb.ValueField, "ValueField");
		}

		[Test]
		public void TreeNodeBinding_Method_ToString () {
			TreeNodeBinding tnb = new TreeNodeBinding ();
			string str = tnb.ToString ();
			Assert.AreEqual ("(Empty)", str, "BeforeToString");
			tnb.DataMember = "DataMember";
			str = tnb.ToString ();
			Assert.AreEqual (tnb.DataMember, str, "AfterToString");
		}
	}
}


#endif
