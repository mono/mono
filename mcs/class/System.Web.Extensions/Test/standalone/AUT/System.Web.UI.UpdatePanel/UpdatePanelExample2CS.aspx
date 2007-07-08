<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
    {
        SqlDataSource2.SelectParameters["OrderID"].DefaultValue = 
            GridView1.SelectedDataKey.Value.ToString();
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>UpdatePanel Example</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1"
                               runat="server" />
            <asp:UpdatePanel ID="OrdersPanel"
                             UpdateMode="Conditional"
                             runat="server">
                <ContentTemplate>
                    <asp:GridView ID="GridView1" 
                                  AllowPaging="True"
                                  AllowSorting="True"
                                  Caption="Orders"
                                  DataKeyNames="OrderID"
                                  DataSourceID="SqlDataSource1"
                                  OnSelectedIndexChanged="GridView1_SelectedIndexChanged"
                                  runat="server" >
                    <Columns>
                    <asp:CommandField ShowSelectButton="True"></asp:CommandField>
                    </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="SqlDataSource1"
                                       runat="server"
                                       ConnectionString="<%$ ConnectionStrings:NorthwindConnectionString %>"
                                       SelectCommand="SELECT [OrderID], [CustomerID], [EmployeeID], [OrderDate] FROM [Orders]">
                    </asp:SqlDataSource>
                </ContentTemplate>
            </asp:UpdatePanel>
            <asp:UpdatePanel ID="OrderDetailsPanel"
                             UpdateMode="Always"
                             runat="server">
                <ContentTemplate>
                    <asp:DetailsView ID="DetailsView1"
                                     Caption="Order Details"
                                     DataKeyNames="OrderID,ProductID"
                                     DataSourceID="SqlDataSource2"
                                     runat="server">
                        <EmptyDataTemplate>
                        <i>Select a row from the Orders table.</i>
                        </EmptyDataTemplate>
                    </asp:DetailsView>
                    <asp:SqlDataSource ID="SqlDataSource2"
                                       ConnectionString="<%$ ConnectionStrings:NorthwindConnectionString %>"
                                       SelectCommand="SELECT [OrderID], [ProductID], [UnitPrice], [Quantity], [Discount] FROM [Order Details] WHERE ([OrderID] = @OrderID)"
                                       runat="server">
                        <SelectParameters>
                            <asp:Parameter Name="OrderID"
                                           Type="Int32" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </form>
</body>
</html>
