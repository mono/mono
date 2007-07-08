<%@ Page Language="C#" MasterPageFile="NorthwindTradersMasterPageCS.master" Title="Content Page" %>
<script runat="server">

    protected void Search_Click(object sender, EventArgs e)
    {
        SqlDataSource1.SelectParameters["SearchTerm"].DefaultValue =
            Server.HtmlEncode(SearchField.Text);
        Label1.Text = "Searching for '" +
            Server.HtmlEncode(SearchField.Text) + "'";
    }
</script>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
            <asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server" />
            <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                Product Search: <asp:TextBox ID="SearchField" runat="server"></asp:TextBox>
                <asp:Button ID="SearchButton" Text="Submit" OnClick="Search_Click"
                    runat="server" />            
            </ContentTemplate>
            </asp:UpdatePanel>
            <asp:UpdatePanel ID="UpdatePanel1" UpdateMode="Always" 
                             runat="server">
                <Triggers>
                <asp:AsyncPostBackTrigger ControlID="SearchButton" />
                </Triggers>
                <ContentTemplate>
                    <asp:Label ID="Label1" runat="server"/>
                    <br />
                    <asp:GridView ID="GridView1" runat="server" AllowPaging="True"
                        AllowSorting="True" DataSourceID="SqlDataSource1">
                        <EmptyDataTemplate>
                        No results to display.
                        </EmptyDataTemplate>
                    </asp:GridView>
                    <asp:SqlDataSource ID="SqlDataSource1" runat="server" 
                        ConnectionString="<%$ ConnectionStrings:NorthwindConnectionString %>"
                        SelectCommand="SELECT [ProductName], [UnitsInStock] FROM 
                        [Alphabetical list of products] WHERE ([ProductName] LIKE 
                        '%' + @SearchTerm + '%')">
                        <SelectParameters>
                            <asp:Parameter Name="SearchTerm" Type="String" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </ContentTemplate>
            </asp:UpdatePanel>
</asp:Content>
