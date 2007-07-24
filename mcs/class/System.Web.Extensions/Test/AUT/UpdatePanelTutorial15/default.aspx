
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected SortedList ProductInfo
    {
        get { return (SortedList)(ViewState["ProductInfo"] ?? new SortedList()); }
        set { ViewState["ProductInfo"] = value; }
    }

    protected void Category1Button_Click(object sender, EventArgs e)
    {
        SqlDataSource1.SelectParameters[0].DefaultValue = "1";
    }

    protected void Category2Button_Click(object sender, EventArgs e)
    {
        SqlDataSource1.SelectParameters[0].DefaultValue = "2";
    }

    protected void DataList1_ItemDataBound(object sender, DataListItemEventArgs e)
    {
        Label label = (Label)e.Item.FindControl("ProductIDLabel");
        Button button = (Button)e.Item.FindControl("Button1");
        TextBox textbox = (TextBox)e.Item.FindControl("TextBox1");
        button.OnClientClick = "GetQuantity(" + label.Text + ",'" + 
            textbox.ClientID + "','" + label.ClientID + "','" + button.ClientID + "')";
        SortedList ProductInfo = this.ProductInfo;
        if (ProductInfo.ContainsKey(label.Text))
        {
            textbox.Text = ProductInfo[label.Text].ToString();
        }        
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (ScriptManager1.IsInAsyncPostBack)
        {
            SortedList ProductInfo = this.ProductInfo;
            foreach (DataListItem d in DataList1.Items)
            {
                Label label = (Label)d.FindControl("ProductIDLabel");
                TextBox textbox = (TextBox)d.FindControl("TextBox1");
                if (textbox.Text.Length > 0)
                {
                    ProductInfo[label.Text] = textbox.Text;
                }
            }
            this.ProductInfo = ProductInfo;
        }
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
                <Services>
                    <asp:ServiceReference Path="ProductQueryService.asmx" />
                </Services>
                <Scripts>
                    <asp:ScriptReference Path="ProductQueryScript.js" />
                </Scripts>
            </asp:ScriptManager>
            <asp:UpdatePanel ID="UpdatePanel1" runat="server" ChildrenAsTriggers="False" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Button ID="Category1Button" runat="server" Text="Category 1" OnClick="Category1Button_Click" />
                    <asp:Button ID="Category2Button" runat="server" OnClick="Category2Button_Click" Text="Category 2" />
                    <asp:DataList ID="DataList1" runat="server" DataKeyField="ProductID" DataSourceID="SqlDataSource1"
                        Width="400px" OnItemDataBound="DataList1_ItemDataBound">
                        <ItemTemplate>
                            ProductName:
                            <asp:Label ID="ProductNameLabel" runat="server" Text='<%# Eval("ProductName") %>'>
                            </asp:Label><br />
                            ProductID:
                            <asp:Label ID="ProductIDLabel" runat="server" Text='<%# Eval("ProductID") %>'></asp:Label><br />
                            <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
                            <asp:Button ID="Button1" runat="server" Text="Get Quantity from Web Service" /><br />
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
