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
	class PokerTreeNode : TreeNode {
		// View state Stuff
		public PokerTreeNode () {
			TrackViewState ();
		}

		public object SaveState () {
			return SaveViewState ();
		}

		public void LoadState (object o) {
			LoadViewState (o);
		}

		public bool IsTrackingViewStateBase { get { return (base.IsTrackingViewState); } }

		public virtual object CloneBase () {
			return (base.Clone ());
		}

		public virtual void RenderPostTextBase (HtmlTextWriter writer) {
			base.RenderPostText (writer);
		}

		public virtual void RenderPreTextBase (HtmlTextWriter writer) {
			base.RenderPreText (writer);
		}

	}

	[TestFixture]
	public class TreeNodeTest {

		[Test]
		public void TreeNode_DefaultProperties () {
			PokerTreeNode tn = new PokerTreeNode ();
			Assert.AreEqual (false, tn.Checked, "Checked");
			Assert.AreEqual (0, tn.ChildNodes.Count, "ChildNodes.Count");
			Assert.AreEqual (false, tn.DataBound, "DataBound");
			Assert.AreEqual (null, tn.DataItem, "DataItem");
			Assert.AreEqual (string.Empty, tn.DataPath, "DataPath");
			Assert.AreEqual (0, tn.Depth, "Depth");
			Assert.AreEqual (null, tn.Expanded, "Expanded");
			Assert.AreEqual (string.Empty, tn.ImageToolTip, "ImageToolTip");
			Assert.AreEqual (string.Empty, tn.ImageUrl, "ImageUrl");
			Assert.AreEqual (string.Empty, tn.NavigateUrl, "NavigateUrl");
			Assert.AreEqual (null, tn.Parent, "Parent");
			Assert.AreEqual (false, tn.PopulateOnDemand, "PopulateOnDemand");
			Assert.AreEqual (TreeNodeSelectAction.Select, tn.SelectAction, "SelectAction");
			Assert.AreEqual (false, tn.Selected, "Selected");
			Assert.AreEqual (null, tn.ShowCheckBox, "ShowCheckBox");
			Assert.AreEqual (string.Empty, tn.Target, "Target");
			Assert.AreEqual (string.Empty, tn.Text, "Text");
			Assert.AreEqual (string.Empty, tn.Value, "Value");
			Assert.AreEqual (string.Empty, tn.ToolTip, "ToolTip");
			Assert.AreEqual (string.Empty, tn.ValuePath, "ValuePath");
			Assert.AreEqual (true, tn.IsTrackingViewStateBase, "IsTrackingViewState");
		}

		[Test]
		public void TreeNode_AssignToDefaultProperties ()
		{
			PokerTreeNode tn = new PokerTreeNode ();
			tn.Checked = false;
			Assert.AreEqual (false, tn.Checked, "Checked");
			tn.ChildNodes.Add (new TreeNode ());
			Assert.AreEqual (1, tn.ChildNodes.Count, "ChildNodes.Count");
			tn.Expanded = false;
			Assert.AreEqual (false, tn.Expanded, "Expanded");
			tn.ImageToolTip = string.Empty;
			Assert.AreEqual (string.Empty, tn.ImageToolTip, "ImageToolTip");
			tn.ImageUrl = string.Empty;
			Assert.AreEqual (string.Empty, tn.ImageUrl, "ImageUrl");
			tn.NavigateUrl = string.Empty;
			Assert.AreEqual (string.Empty, tn.NavigateUrl, "NavigateUrl");
			tn.PopulateOnDemand = false;
			Assert.AreEqual (false, tn.PopulateOnDemand, "PopulateOnDemand");
			tn.SelectAction = TreeNodeSelectAction.Select;
			Assert.AreEqual (TreeNodeSelectAction.Select, tn.SelectAction, "SelectAction");
			tn.Selected = false;
			Assert.AreEqual (false, tn.Selected, "Selected");
			tn.ShowCheckBox = false;
			Assert.AreEqual (false, tn.ShowCheckBox, "ShowCheckBox");
			tn.Target = string.Empty;
			Assert.AreEqual (string.Empty, tn.Target, "Target");
			tn.Text = string.Empty;
			Assert.AreEqual (string.Empty, tn.Text, "Text");
			tn.Value = string.Empty;
			Assert.AreEqual (string.Empty, tn.Value, "Value");
			tn.ToolTip = string.Empty;
			Assert.AreEqual (string.Empty, tn.ToolTip, "ToolTip");
		}

		[Test]
		public void TreeNode_Method_Select () {
			PokerTreeNode tn = new PokerTreeNode ();
			Assert.AreEqual (false, tn.Selected, "BeforeSelect");
			tn.Select ();
			Assert.AreEqual (true, tn.Selected, "AfterSelect");
		}

		[Test]
		public void TreeNode_Method_Clone () {
			PokerTreeNode tn1 = new PokerTreeNode ();
			TreeNode tn2 = new TreeNode ();
			tn1.Text = "CloneThisNode";
			tn1.Value = "111";
			Assert.AreEqual (string.Empty, tn2.Text, "BeforeClone1");
			Assert.AreEqual (string.Empty, tn2.Value, "BeforeClone2");
			tn2 = (TreeNode) tn1.CloneBase ();
			Assert.AreEqual ("CloneThisNode", tn2.Text, "AfterClone1");
			Assert.AreEqual ("111", tn2.Value, "AfterClone2");
		}

		[Test]
		[Category ("NunitWeb")]
		public void TreeNode_NavigateUrl () {
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (pageLoadNavigateUrl));
#if NET_4_0
			string strTarget = "<a href=\"#treeview1_SkipLink\"><img alt=\"Skip Navigation Links.\" src=\"/NunitWeb/WebResource.axd?d=8VpphgAbakKUC_J8R6hR0Q2&amp;t=634067491135766272\" width=\"0\" height=\"0\" style=\"border-width:0px;\" /></a><div id=\"treeview1\">\r\n\t<table cellpadding=\"0\" cellspacing=\"0\" style=\"border-width:0;\">\r\n\t\t<tr>\r\n\t\t\t<td><img src=\"/NunitWeb/WebResource.axd?d=Me-CdxEHiTTT3lXTDd0I2ilpe6vhhhJjssENmbNkrSY1&amp;t=634067491135766272\" alt=\"\" /></td><td style=\"white-space:nowrap;\"><a class=\"treeview1_0\" href=\"NavigateUrl\" id=\"treeview1t0\">TreeNode1</a></td>\r\n\t\t</tr>\r\n\t</table>\r\n</div><a id=\"treeview1_SkipLink\"></a>";
#else
			string strTarget =
@"<a href=""#treeview1_SkipLink""><img alt=""Skip Navigation Links."" src=""/NunitWeb/WebResource.axd?d=kffkK8wYLPknq-W8AKNdNQ2&amp;t=632883840303269703"" width=""0"" height=""0"" style=""border-width:0px;"" /></a><div id=""treeview1"">
	<table cellpadding=""0"" cellspacing=""0"" style=""border-width:0;"">
		<tr>
			<td><img src=""/NunitWeb/WebResource.axd?d=edXX1vkoy5lI0CekgaZ5zZhMbc1ZCZv4nlS9J-l53l41&amp;t=632883840303269703"" alt="""" /></td><td style=""white-space:nowrap;""><a class=""treeview1_0"" href=""NavigateUrl"" id=""treeview1t0"">TreeNode1</a></td>
		</tr>
	</table>
</div><a id=""treeview1_SkipLink""></a>";
#endif
			string str = HtmlDiff.GetControlFromPageHtml (t.Run ());
			HtmlDiff.AssertAreEqual (strTarget, str, "PostbackNavigate");
		}
		public static void pageLoadNavigateUrl (Page page) {
			TreeView tv = new TreeView ();
			tv.ID = "treeview1";
			PokerTreeNode tn1 = new PokerTreeNode ();
			tn1.Text = "TreeNode1";
			tn1.NavigateUrl = "NavigateUrl";
			tv.Nodes.Add (tn1);
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			page.Form.Controls.Add (lcb);
			page.Form.Controls.Add (tv);
			page.Form.Controls.Add (lce);
		}

		static void tv_SelectedNodeChanged (object sender, EventArgs e) {
			TreeView tv = (TreeView) sender;
			WebTest.CurrentTest.UserData = tv.SelectedNode.Text;
		}

		[Test]
		[Category ("NunitWeb")]
		public void TreeNode_Render () {
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (pageLoadRender));
#if NET_4_0
			string strTarget = "<a href=\"#treeview1_SkipLink\"><img alt=\"Skip Navigation Links.\" src=\"/NunitWeb/WebResource.axd?d=8VpphgAbakKUC_J8R6hR0Q2&amp;t=634067491135766272\" width=\"0\" height=\"0\" style=\"border-width:0px;\" /></a><div id=\"treeview1\">\r\n\t<table cellpadding=\"0\" cellspacing=\"0\" style=\"border-width:0;\">\r\n\t\t<tr>\r\n\t\t\t<td><a href=\"javascript:__doPostBack(&#39;treeview1&#39;,&#39;tvalue&#39;)\"><img src=\"/NunitWeb/WebResource.axd?d=Me-CdxEHiTTT3lXTDd0I2qLe7WfoYyDfWfVSkV5Suzs1&amp;t=634067491135766272\" alt=\"Collapse text\" style=\"border-width:0;\" /></a></td><td><a href=\"navigateUrl\" target=\"target\" title=\"ToolTip\" id=\"treeview1t0i\" tabindex=\"-1\"><img src=\"imageUrl\" alt=\"ImageToolTip\" style=\"border-width:0;\" /></a></td><td style=\"white-space:nowrap;\"><input type=\"checkbox\" name=\"treeview1n0CheckBox\" id=\"treeview1n0CheckBox\" checked=\"checked\" title=\"ToolTip\" /><a class=\"treeview1_0\" href=\"navigateUrl\" target=\"target\" title=\"ToolTip\" id=\"treeview1t0\">text</a></td>\r\n\t\t</tr>\r\n\t</table><table cellpadding=\"0\" cellspacing=\"0\" style=\"border-width:0;\">\r\n\t\t<tr>\r\n\t\t\t<td><div style=\"width:20px;height:1px\"></div></td><td><img src=\"/NunitWeb/WebResource.axd?d=Me-CdxEHiTTT3lXTDd0I2ilpe6vhhhJjssENmbNkrSY1&amp;t=634067491135766272\" alt=\"\" /></td><td style=\"white-space:nowrap;\"><a class=\"treeview1_0\" href=\"javascript:__doPostBack(&#39;treeview1&#39;,&#39;svalue\\\\childenode&#39;)\" id=\"treeview1t1\">childenode</a></td>\r\n\t\t</tr>\r\n\t</table>\r\n</div><a id=\"treeview1_SkipLink\"></a>";
#else
			string strTarget =
@"<a href=""#treeview1_SkipLink""><img alt=""Skip Navigation Links."" src=""/NunitWeb/WebResource.axd?d=kffkK8wYLPknq-W8AKNdNQ2&amp;t=632883840303269703"" width=""0"" height=""0"" style=""border-width:0px;"" /></a><div id=""treeview1"">
	<table cellpadding=""0"" cellspacing=""0"" style=""border-width:0;"">
		<tr>
			<td><a id=""treeview1n0"" href=""javascript:TreeView_ToggleNode(treeview1_Data,0,treeview1n0,' ',treeview1n0Nodes)""><img src=""/NunitWeb/WebResource.axd?d=edXX1vkoy5lI0CekgaZ5zW7-1Af97Wq_r6fRK7PDqP81&amp;t=632883840303269703"" alt=""Collapse text"" style=""border-width:0;"" /></a></td><td><a href=""navigateUrl"" target=""target"" onclick=""javascript:TreeView_SelectNode(treeview1_Data, this,'treeview1t0');javascript:TreeView_ToggleNode(treeview1_Data,0,treeview1n0,' ',treeview1n0Nodes)"" title=""ToolTip"" id=""treeview1t0i""><img src=""imageUrl"" alt=""ImageToolTip"" style=""border-width:0;"" /></a></td><td style=""white-space:nowrap;""><input type=""checkbox"" name=""treeview1n0CheckBox"" id=""treeview1n0CheckBox"" checked=""checked"" title=""text"" /><a class=""treeview1_0"" href=""navigateUrl"" target=""target"" onclick=""javascript:TreeView_SelectNode(treeview1_Data, this,'treeview1t0');javascript:TreeView_ToggleNode(treeview1_Data,0,treeview1n0,' ',treeview1n0Nodes)"" title=""ToolTip"" id=""treeview1t0"">text</a></td>
		</tr>
	</table>
	<table cellpadding=""0"" cellspacing=""0"" style=""border-width:0;"">
		<tr>
			<td><div style=""width:20px;height:1px""></div></td><td><img src=""/NunitWeb/WebResource.axd?d=edXX1vkoy5lI0CekgaZ5zZhMbc1ZCZv4nlS9J-l53l41&amp;t=632883840303269703"" alt="""" /></td><td style=""white-space:nowrap;""><a class=""treeview1_0"" href=""javascript:__doPostBack('treeview1','svalue\\childenode')"" onclick=""TreeView_SelectNode(treeview1_Data, this,'treeview1t1');"" id=""treeview1t1"">childenode</a></td>
		</tr>
	</table>
</div><a id=""treeview1_SkipLink""></a>";
#endif
			string str = HtmlDiff.GetControlFromPageHtml (t.Run ());
			HtmlDiff.AssertAreEqual (strTarget, str, "Render");
		}
		public static void pageLoadRender (Page page) {
			TreeView tv = new TreeView ();
			tv.EnableClientScript = false;
			tv.ID = "treeview1";
			TreeNode tn = new TreeNode
				("text", "value", "imageUrl", "navigateUrl", "target");
			tn.Checked = true;
			tn.ChildNodes.Add (new TreeNode ("childenode"));
			tn.Expanded = true;
			tn.ImageToolTip = "ImageToolTip";
			tn.PopulateOnDemand = false;
			tn.SelectAction = TreeNodeSelectAction.SelectExpand;
			tn.Selected = true;
			tn.ShowCheckBox = true;
			tn.ToolTip = "ToolTip";
			tv.Nodes.Add (tn);
			tv.DataBind ();
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			page.Form.Controls.Add (lcb);
			page.Form.Controls.Add (tv);
			page.Form.Controls.Add (lce);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Ignore ("Pending more investigation (see FIXME at the top of TreeView.RenderNode)")]
		public void PopulateOnDemand_With_ChildNodes ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (PopulateOnDemand_With_ChildNodes_OnLoad));
			t.Run ();
		}

		protected static void PopulateOnDemand_With_ChildNodes_OnLoad (Page p)
		{
			TreeView tv = new TreeView ();
			TreeNode node = new TreeNode ("text", "value", "imageUrl", "navigateUrl", "target");
			node.PopulateOnDemand = true;
			node.ChildNodes.Add (new TreeNode ("text", "value", "imageUrl", "navigateUrl", "target"));
			tv.Nodes.Add (node);

			p.Form.Controls.Add (tv);
		}
		
		[Test]
		public void TreeNode_ToggleExpandState ()
		{
			TreeNode node = new TreeNode ("node");
			Assert.AreEqual (null, node.Expanded, "TreeNode_ToggleExpandState#1");
			node.ToggleExpandState ();
			Assert.AreEqual (true, node.Expanded, "TreeNode_ToggleExpandState#2");
			node.ToggleExpandState ();
			Assert.AreEqual (false, node.Expanded, "TreeNode_ToggleExpandState#3");
		}

		[Test]
		public void TreeNode_TextValue1 ()
		{
			TreeNode node = new TreeNode ();
			node.Text = "TTT";
			Assert.AreEqual ("TTT", node.Value, "TreeNode_TextValue1#1");
			node.Value = "";
			Assert.AreEqual ("", node.Value, "TreeNode_TextValue1#2");
			node.Value = null;
			Assert.AreEqual ("TTT", node.Value, "TreeNode_TextValue1#3");
		}

		[Test]
		public void TreeNode_TextValue2 ()
		{
			TreeNode node = new TreeNode ();
			node.Value = "VVV";
			Assert.AreEqual ("VVV", node.Text, "TreeNode_TextValue2#1");
			node.Text = "";
			Assert.AreEqual ("", node.Text, "TreeNode_TextValue2#2");
			node.Text = null;
			Assert.AreEqual ("VVV", node.Text, "TreeNode_TextValue2#3");
		}
	}
}


#endif
