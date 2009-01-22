<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

	protected void Button1_Click (object sender, EventArgs e) {
		DataGrid1.SelectedIndex++;
	}
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div><%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %>
        <asp:DataGrid ID="DataGrid1" runat="server" DataSourceID="ObjectDataSource1" DataKeyField="id" AllowPaging="True" AllowSorting="True" CellPadding="4" ForeColor="#333333" GridLines="None" PageSize="5" ShowFooter="True">
            <FooterStyle BackColor="#990000" Font-Bold="True" ForeColor="White" />
            <SelectedItemStyle BackColor="#FFCC66" Font-Bold="True" ForeColor="Navy" />
            <PagerStyle BackColor="#FFCC66" ForeColor="#333333" HorizontalAlign="Center" NextPageText="Next"
                PrevPageText="Previous" />
            <AlternatingItemStyle BackColor="White" />
            <ItemStyle BackColor="#FFFBD6" ForeColor="#333333" />
            <HeaderStyle BackColor="#990000" Font-Bold="True" ForeColor="White" />
            <Columns>
                <asp:BoundColumn ReadOnly="True" DataField="ID" HeaderText="ID"></asp:BoundColumn>
                <asp:BoundColumn DataField="Name" HeaderText="Name"></asp:BoundColumn>
                <asp:EditCommandColumn CancelText="Cancel" EditText="Edit" UpdateText="Update"></asp:EditCommandColumn>
                <asp:ButtonColumn CommandName="Delete" Text="Delete"></asp:ButtonColumn>
            </Columns>
            <EditItemStyle BackColor="Green" Font-Bold="False" Font-Italic="False" Font-Overline="False"
                Font-Strikeout="False" Font-Underline="False" />
        </asp:DataGrid><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="GetAllItems" SelectCountMethod="GetCount" MaximumRowsParameterName="maxRows"
            TypeName="MonoTests.System.Web.UI.WebControls.DataGridTest+MyDataSource" UpdateMethod="UpdateItem" StartRowIndexParameterName="startIndex" OldValuesParameterFormatString="original_{0}" EnablePaging="False">
            <UpdateParameters>
                <asp:Parameter Name="id" Type="Int32" />
                <asp:Parameter Name="name" Type="String" />
            </UpdateParameters>
        </asp:ObjectDataSource>
        <asp:Button ID="Button1" runat="server" Text="Button" UseSubmitBehavior="false" OnClick="Button1_Click" /></div>
    </form>
</body>
</html>
