
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
 "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    private void ChangeQuantity(object sender, int delta)
    {
        Label quantityLabel = (Label)((Button)sender).FindControl("QuantityLabel");
        int currentQuantity = Int32.Parse(quantityLabel.Text);
        currentQuantity = Math.Max(0, currentQuantity + delta);
        quantityLabel.Text = currentQuantity.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private void OnDecreaseQuantity(object sender, EventArgs e)
    {
        ChangeQuantity(sender, -1);
    }

    private void OnIncreaseQuantity(object sender, EventArgs e)
    {
        ChangeQuantity(sender, 1);
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("Beverage order:<br/>");
        foreach (GridViewRow row in GridView1.Rows)
        {
            if (row.RowType == DataControlRowType.DataRow)
            {
                Label quantityLabel = (Label)row.FindControl("QuantityLabel");
                int currentQuantity = Int32.Parse(quantityLabel.Text);
                sb.Append(row.Cells[0].Text + " : " + currentQuantity + "<br/>");
            }
        }
        SummaryLabel.Text = sb.ToString();

    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>UpdatePanel Inside GridView Template Example </title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" 
                               runat="server" />
            <asp:GridView ID="GridView1" 
                          AutoGenerateColumns="False"
                          DataSourceID="SqlDataSource1"
                          runat="server">
                <Columns>
                    <asp:BoundField DataField="ProductName" 
                                    HeaderText="ProductName" />
                    <asp:BoundField DataField="UnitPrice"
                                    HeaderText="UnitPrice" />
                    <asp:TemplateField HeaderText="Quantity">
                        <ItemTemplate>
                            <asp:UpdatePanel ID="QuantityUpdatePanel"
                                             UpdateMode="Conditional"
                                             runat="server" >
                                <ContentTemplate>
                                    <asp:Label ID="QuantityLabel"
                                               Text="0"
                                               runat="server" />
                                    <asp:Button ID="DecreaseQuantity"
                                                Text="-"
                                                OnClick="OnDecreaseQuantity"
                                                runat="server" />
                                    <asp:Button ID="IncreaseQuantity" 
                                                Text="+"
                                                OnClick="OnIncreaseQuantity"
                                                runat="server" />
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>            
            <asp:UpdatePanel ID="SummaryUpdatePanel" 
                             UpdateMode="Conditional"
                             runat="server">
                <ContentTemplate>
                    <asp:Button ID="Button1"
                                OnClick="Button1_Click"
                                Text="Get Summary"
                                runat="server" />
                    <br />
                    <asp:Label ID="SummaryLabel"
                               runat="server">
                    </asp:Label>
                </ContentTemplate>
            </asp:UpdatePanel>
            <asp:SqlDataSource ID="SqlDataSource1"
                               ConnectionString="<%$ ConnectionStrings:NorthwindConnectionString %>"
                               SelectCommand="SELECT [ProductName], [UnitPrice] FROM 
                               [Alphabetical list of products] WHERE ([CategoryName] 
                               LIKE '%' + @CategoryName + '%')"
                               runat="server">
                <SelectParameters>
                    <asp:Parameter DefaultValue="Beverages"
                                   Name="CategoryName"
                                   Type="String" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
    </form>
</body>
</html>
