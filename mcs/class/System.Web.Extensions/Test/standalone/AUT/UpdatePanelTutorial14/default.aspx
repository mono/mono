
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected void Category1Button_Click(object sender, EventArgs e)
    {
        SqlDataSource1.SelectParameters[0].DefaultValue = "1";
    }

    protected void Category2Button_Click(object sender, EventArgs e)
    {
        SqlDataSource1.SelectParameters[0].DefaultValue = "2";
    }

</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Products Display</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server">
            </asp:ScriptManager>
            <asp:UpdatePanel ID="UpdatePanel1" runat="server" ChildrenAsTriggers="False" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Button ID="Category1Button" runat="server" Text="Category 1" OnClick="Category1Button_Click" />
                    <asp:Button ID="Category2Button" runat="server" OnClick="Category2Button_Click" Text="Category 2" />
                    <asp:DataList ID="DataList1" runat="server" DataKeyField="ProductID" DataSourceID="SqlDataSource1"
                        Width="231px">
                        <ItemTemplate>
                            ProductName:
                            <asp:Label ID="ProductNameLabel" runat="server" Text='<%# Eval("ProductName") %>'>
                            </asp:Label><br />
                            ProductID:
                            <asp:Label ID="ProductIDLabel" runat="server" Text='<%# Eval("ProductID") %>'></asp:Label><br />
                            <br />
                        </ItemTemplate>
                    </asp:DataList>
                    <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:NorthwindConnectionString %>"
                        SelectCommand="SELECT [ProductName], [ProductID] FROM [Alphabetical list of products] WHERE ([CategoryID] = @CategoryID)">
                        <SelectParameters>
                            <asp:Parameter DefaultValue="1" Name="CategoryID" Type="Int32" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="Category1Button" />
                    <asp:AsyncPostBackTrigger ControlID="Category2Button" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </form>
</body>
</html>
