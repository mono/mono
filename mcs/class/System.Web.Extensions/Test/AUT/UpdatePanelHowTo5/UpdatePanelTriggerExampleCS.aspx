<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
    "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    protected void Search_Click(object sender, EventArgs e)
    {
        SqlDataSource1.SelectParameters["SearchTerm"].DefaultValue = 
            SearchField.Text;
        Label1.Text = "Searching for '" + SearchField.Text + "'";
    }

</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Declarative Trigger Example</title>
</head>
<body>
    <form id="form1" runat="server"
          defaultbutton="SearchButton" defaultfocus="SearchField">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server" />
            <asp:TextBox ID="SearchField" runat="server"></asp:TextBox>
            <asp:Button ID="SearchButton" Text="Submit" OnClick="Search_Click"
                runat="server" />
            <hr />
            <asp:UpdatePanel ID="UpdatePanel1" UpdateMode="Conditional" 
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
        </div>
    </form>
</body>
</html>
