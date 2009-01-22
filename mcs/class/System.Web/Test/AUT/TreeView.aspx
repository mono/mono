<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected void TreeView1_TreeNodePopulate (object sender, TreeNodeEventArgs e) {
		if (e.Node.Depth >= 3)
			return;
        
        for (int i = 1; i <= 2; i++) {
            TreeNode node = new TreeNode (e.Node.Text + "_" + (e.Node.Depth + 1) + "-" + i);
            node.PopulateOnDemand = true;
            e.Node.ChildNodes.Add (node);
        }
    }

</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>TreeView Test Page</title>
</head>
<body dir="ltr">
    <form id="form1" runat="server">
    <div>
		&nbsp;<asp:Button ID="Button1" runat="server" Text="Do PostBack" /><br />
        <br />
        <asp:TreeView ID="TreeView1" runat="server" OnTreeNodePopulate="TreeView1_TreeNodePopulate" ExpandDepth="0">
            <Nodes>
                <asp:TreeNode Text="pop.on.dem" Value="pop.on.dem-value" PopulateOnDemand="True"></asp:TreeNode>
            </Nodes>
             <ParentNodeStyle BackColor="#FFE0C0" Font-Bold="False" />
            <LevelStyles>
                <asp:TreeNodeStyle BorderColor="Red" BorderStyle="Dotted" BorderWidth="1px" Font-Underline="False" />
                <asp:TreeNodeStyle BorderColor="Red" BorderStyle="Solid" BorderWidth="1px" Font-Underline="False" />
                <asp:TreeNodeStyle BorderColor="Red" BorderStyle="Double" BorderWidth="3px" Font-Underline="False" ForeColor="Green" BackColor="#FFFFC0" />
            </LevelStyles>
            <SelectedNodeStyle Font-Underline="True" />
            <RootNodeStyle BackColor="#E0E0E0" Font-Bold="True" />
            <LeafNodeStyle Font-Italic="True" ForeColor="#0000C0" BackColor="#C0C0FF" />
            <NodeStyle BackColor="White" ForeColor="DarkBlue" Font-Names="Tahoma" Font-Size="8pt" HorizontalPadding="5px" VerticalPadding="0px" NodeSpacing="3px" />
            <HoverNodeStyle ForeColor="White" BackColor="Silver" BorderColor="#000040" />
       </asp:TreeView>
        &nbsp;&nbsp;&nbsp;
        <br />
    </div>
    </form>
</body>
</html>
