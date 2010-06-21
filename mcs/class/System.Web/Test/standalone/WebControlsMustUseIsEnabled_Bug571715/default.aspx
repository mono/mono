<%@ Page Language="C#" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<script runat="server">
	protected void GridView1_RowDataBound (object sender, GridViewRowEventArgs e)
	{
		if (e.Row.RowType == DataControlRowType.DataRow && e.Row.Cells [0].Text == "2")
			e.Row.Cells [2].Enabled = false;
	}
</script>
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
	<title>Bug #571715</title>
</head>
<body>
<form id="form1" runat="server">
	<%= AppDomain.CurrentDomain.GetData ("BEGIN_CODE_MARKER") %><asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" DataSourceID="XmlDataSource1" OnRowDataBound="GridView1_RowDataBound">
		<Columns>
			<asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
			<asp:BoundField DataField="FULLNAME" HeaderText="FULLNAME" SortExpression="FULLNAME" />
			<asp:TemplateField>
				<ItemTemplate><asp:Button CommandName="Delete" CommandArgument="<%# Container.DataItemIndex %>" Text="Delete" ID="DeleteBtn" runat="server" CausesValidation="False" /></ItemTemplate>
				<ItemStyle HorizontalAlign="Left" Width="60px" />
			</asp:TemplateField>
		</Columns>
	</asp:GridView><%= AppDomain.CurrentDomain.GetData ("END_CODE_MARKER") %>
	<asp:XmlDataSource ID="XmlDataSource1" runat="server" DataFile="People.xml"></asp:XmlDataSource>
</form>
</body>
</html>
