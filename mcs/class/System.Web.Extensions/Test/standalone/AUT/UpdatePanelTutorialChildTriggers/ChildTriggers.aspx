<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>UpdatePanel Example</title>
    <script runat="server">
        protected void ProductsUpdatePanel_Load(object sender, EventArgs e)
        {
            CategoryTimeLabel.Text = DateTime.Now.ToString();
        }
        
        protected void CategoriesRepeater_ItemCommand(object sender, RepeaterCommandEventArgs e)
        {
            SubcategoriesDataSource.SelectParameters["CategoryID"].DefaultValue = e.CommandArgument.ToString();
            SubcategoriesRepeater.DataBind();
            ProductsDataSource.SelectParameters["SubcategoryID"].DefaultValue = "0";
            ProductsGridView.DataBind();
        }

        protected void SubcategoriesRepeater_ItemCommand(object sender, RepeaterCommandEventArgs e)
        {
            ProductsDataSource.SelectParameters["SubcategoryID"].DefaultValue = e.CommandArgument.ToString();
            ProductsGridView.DataBind();
        }

        protected void RefreshCategoriesButton_Click(object sender, EventArgs e)
        {
            SubcategoriesDataSource.SelectParameters["CategoryID"].DefaultValue = "0";
            SubcategoriesRepeater.DataBind();
            ProductsDataSource.SelectParameters["SubcategoryID"].DefaultValue = "0";
            ProductsGridView.DataBind();
        }
</script>
</head>
<body>
  <form id="form1" runat="server">
  <div>
    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>
    <table>
      <tr>
        <td valign="top" style="width:120px">
          <asp:UpdatePanel ID="ProductsUpdatePanel" runat="server" 
                           ChildrenAsTriggers="False" 
                           OnLoad="ProductsUpdatePanel_Load" UpdateMode="Conditional">
            <ContentTemplate>
              <asp:Repeater ID="CategoryRepeater" runat="server" DataSourceID="CategoriesDataSource"
                            OnItemCommand="CategoriesRepeater_ItemCommand">
                <ItemTemplate>
                  <asp:LinkButton runat="server" ID="SelectCategoryButton" 
                                  Text='<%# Eval("Name") %>'
                                  CommandName="SelectCategory"
                                  CommandArgument='<%#Eval("ProductCategoryID") %>' /><br />
                </ItemTemplate>
              </asp:Repeater>
              <asp:SqlDataSource ID="CategoriesDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:AdventureWorksConnectionString %>"
                  SelectCommand="SELECT Name, ProductCategoryID FROM Production.ProductCategory ORDER BY ProductCategoryID">
              </asp:SqlDataSource>
              <br />
              <asp:Label ID="CategoryTimeLabel" runat="server" Text="Label" Font-Size="Smaller"></asp:Label>
            </ContentTemplate>
            <Triggers>
              <asp:AsyncPostBackTrigger ControlID="RefreshCategoriesButton" EventName="Click" />
            </Triggers>
          </asp:UpdatePanel>
          <asp:LinkButton runat="Server" ID="RefreshCategoriesButton" Text="Refresh Category List"
                          OnClick="RefreshCategoriesButton_Click" Font-Size="smaller" />
        </td>
        <td valign="top">
          <asp:UpdatePanel ID="SubcategoriesUpdatePanel" runat="server">
            <ContentTemplate>
              <asp:Repeater ID="SubcategoriesRepeater" runat="server" DataSourceID="SubcategoriesDataSource"
                            OnItemCommand="SubcategoriesRepeater_ItemCommand" >
                <ItemTemplate>
                  <asp:LinkButton runat="server" ID="SelectSubcategoryButton" 
                                  Text='<%# Eval("Name") %>'
                                  CommandName="SelectSubcategory"
                                  CommandArgument='<%#Eval("ProductSubcategoryID") %>' /><br />
                </ItemTemplate>
              </asp:Repeater>
              <asp:SqlDataSource ID="SubcategoriesDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:AdventureWorksConnectionString %>"
                  SelectCommand="SELECT Name, ProductSubcategoryID FROM Production.ProductSubcategory 
                                 WHERE Production.ProductSubcategory.ProductCategoryID = @CategoryID 
                                 ORDER BY Name">
                 <SelectParameters>
                   <asp:Parameter Name="CategoryID" DefaultValue="0" />
                 </SelectParameters>
              </asp:SqlDataSource>
            </ContentTemplate>
          </asp:UpdatePanel>
        </td>
        <td valign="top">
          <asp:UpdatePanel ID="ProductUpdatePanel" runat="server">
            <ContentTemplate>
              <asp:GridView ID="ProductsGridView" runat="server" AllowPaging="True" AutoGenerateColumns="False" BackColor="White" BorderColor="#E7E7FF" BorderStyle="None" BorderWidth="1px" CellPadding="3" DataKeyNames="ProductID" DataSourceID="ProductsDataSource" GridLines="Horizontal">
                <FooterStyle BackColor="#B5C7DE" ForeColor="#4A3C8C" />
                <Columns>
                  <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" >
                    <ItemStyle Width="240px" />
                  </asp:BoundField>
                </Columns>
                <RowStyle BackColor="#E7E7FF" ForeColor="#4A3C8C" />
                <SelectedRowStyle BackColor="#738A9C" Font-Bold="True" ForeColor="#F7F7F7" />
                <PagerStyle BackColor="#E7E7FF" ForeColor="#4A3C8C" HorizontalAlign="Right" />
                <HeaderStyle BackColor="#4A3C8C" Font-Bold="True" ForeColor="#F7F7F7" />
                <AlternatingRowStyle BackColor="#F7F7F7" />
              </asp:GridView>
              <asp:SqlDataSource ID="ProductsDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:AdventureWorksConnectionString %>"
                  SelectCommand="SELECT ProductID, Name, ProductSubcategoryID FROM Production.Product WHERE (ProductSubcategoryID = @SubcategoryID) ORDER BY Name">
                <SelectParameters>
                  <asp:Parameter DefaultValue="0" Name="SubcategoryID" />
                </SelectParameters>
              </asp:SqlDataSource>
            </ContentTemplate>
          </asp:UpdatePanel>
        </td>
      </tr>
    </table>
  </div>
  </form>
</body>
</html>
