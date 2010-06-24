<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:GridView ID="GridView1" runat="server" DataSourceID="ObjectDataSource1" AutoGenerateColumns="false" DataKeyNames="id">
            <Columns>
                <asp:BoundField DataField="id" HeaderText="ID" />
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:TextBox ID="Name" runat="server" Text='<%# Bind("name") %>'></asp:TextBox>
                        <asp:Button ID="b1" runat="server" CommandName="Update" Text="upd" UseSubmitBehavior="false"/>
                    </ItemTemplate>
                    <AlternatingItemTemplate>
                        <asp:TextBox ID="Name" runat="server" Text='<%# Bind("name") %>'></asp:TextBox>
                        <asp:Button ID="b1" runat="server" CommandName="Update" Text="upd" UseSubmitBehavior="false"/>
                    </AlternatingItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="GetAllItems"
            TypeName="MonoTests.System.Web.UI.WebControls.GridViewTest+data" UpdateMethod="UpdateItem">
            <UpdateParameters>
                <asp:Parameter Name="id" Type="Int32" />
                <asp:Parameter Name="name" Type="String" />
            </UpdateParameters>
        </asp:ObjectDataSource>
        <asp:Button ID="Button1" runat="server" Text="Button" UseSubmitBehavior="false" /></div>
    </form>
</body>
</html>
