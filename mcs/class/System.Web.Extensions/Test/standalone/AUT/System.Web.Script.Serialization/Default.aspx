<%@ Page Language="C#" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    JavaScriptSerializer serializer;
    protected void Page_Load(object sender, EventArgs e)
    {
        serializer = new JavaScriptSerializer();
        
        // Register the custom converter.
        serializer.RegisterConverters(new JavaScriptConverter[] { 
            new System.Web.Script.Serialization.CS.ListItemCollectionConverter() });
        this.SetFocus(TextBox1);
    }
    
    protected void saveButton_Click(object sender, EventArgs e)
    {
        // Save the current state of the ListBox control.
        SavedState.Text = serializer.Serialize(ListBox1.Items);
        recoverButton.Enabled = true;
        Message.Text = "State saved";
    }

    protected void recoverButton_Click(object sender, EventArgs e)
    {        
        //Recover the saved items of the ListBox control.
        ListItemCollection recoveredList = serializer.Deserialize<ListItemCollection>(SavedState.Text);
        ListItem[] newListItemArray = new ListItem[recoveredList.Count];
        recoveredList.CopyTo(newListItemArray, 0);
        ListBox1.Items.Clear();
        ListBox1.Items.AddRange(newListItemArray);
        Message.Text = "Last saved state recovered.";
    }

    protected void clearButton_Click(object sender, EventArgs e)
    {
        // Remove all items from the ListBox control.
        ListBox1.Items.Clear();
        Message.Text = "All items removed";
    }

    protected void ContactsGrid_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Get the currently selected row using the SelectedRow property.
        GridViewRow row = ContactsGrid.SelectedRow;

        // Get the ID of item selected.
        string itemId = ContactsGrid.DataKeys[row.RowIndex].Value.ToString();                
        ListItem newItem = new ListItem(row.Cells[4].Text, itemId);
        
        // Check if the item already exists in the ListBox control.
        if (!ListBox1.Items.Contains(newItem))
        {
            // Add the item to the ListBox control.
            ListBox1.Items.Add(newItem);
            Message.Text = "Item added";
        }
        else
            Message.Text = "Item already exists";
    }

    protected void ContactsGrid_PageIndexChanged(object sender, EventArgs e)
    {
        //Reset the selected index.
        ContactsGrid.SelectedIndex = -1;
    }

    protected void searchButton_Click(object sender, EventArgs e)
    {
        //Reset indexes.
        ContactsGrid.SelectedIndex = -1;
        ContactsGrid.PageIndex = 0;
        
        //Set focus on the TextBox control.
        ScriptManager1.SetFocus(TextBox1);
    }

    protected void CheckBox1_CheckedChanged(object sender, EventArgs e)
    {
        // Show/hide the saved state string.        
        SavedState.Visible = CheckBox1.Checked;
        StateLabel.Visible = CheckBox1.Checked;
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Save/Recover state</title>
    <style type="text/css">
        body {  font: 11pt Trebuchet MS;
                font-color: #000000;
                padding-top: 72px;
                text-align: center }

        .text { font: 8pt Trebuchet MS }
    </style>
</head>
<body>    
    <form id="form1" runat="server" defaultbutton="searchButton" defaultfocus="TextBox1">
        <h3>
            <span style="text-decoration: underline">
                                    Contacts Selection</span><br />
        </h3>
        <asp:ScriptManager runat="server" ID="ScriptManager1" />
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                Type contact's first name:
                <asp:TextBox ID="TextBox1" runat="server" />
                <asp:Button ID="searchButton" runat="server" Text="Search" OnClick="searchButton_Click" />&nbsp;
                <br />
                <br />
                <table border="0" width="100%">
                    <tr>
                        <td style="width:50%" valign="top" align="center">
                            <b>Search results:</b><br />
                            <asp:GridView ID="ContactsGrid" runat="server" AutoGenerateColumns="False"
                                CellPadding="4" DataKeyNames="ContactID" DataSourceID="SqlDataSource1"
                                OnSelectedIndexChanged="ContactsGrid_SelectedIndexChanged" ForeColor="#333333" GridLines="None" AllowPaging="True" PageSize="7" OnPageIndexChanged="ContactsGrid_PageIndexChanged">
                                <Columns>
                                    <asp:CommandField ShowSelectButton="True" ButtonType="Button" />
                                    <asp:BoundField DataField="ContactID" HeaderText="ContactID" SortExpression="ContactID" Visible="False" />
                                    <asp:BoundField DataField="FirstName" HeaderText="FirstName" SortExpression="FirstName" />
                                    <asp:BoundField DataField="LastName" HeaderText="LastName" SortExpression="LastName" />
                                    <asp:BoundField DataField="EmailAddress" HeaderText="EmailAddress" SortExpression="EmailAddress" />
                                </Columns>
                                <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                                <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                                <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
                                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                                <EditRowStyle BackColor="#999999" />
                                <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
                                <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                                <EmptyDataTemplate>No data found.</EmptyDataTemplate>
                            </asp:GridView>
                            <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:AdventureWorksConnectionString %>"
                                SelectCommand="SELECT ContactID, FirstName, LastName, EmailAddress FROM Person.Contact WHERE (UPPER(FirstName) = UPPER(@FIRSTNAME))" >
                                <SelectParameters>
                                    <asp:ControlParameter Name="FIRSTNAME" ControlId="TextBox1" Type="String" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </td>
                        <td valign="top">
                            <b>Contacts list:</b><br />
                        <asp:ListBox ID="ListBox1" runat="server" Height="200px" Width="214px" /><br />
                        <asp:Button ID="saveButton" runat="server" Text="Save state" OnClick="saveButton_Click" ToolTip="Save the current state of the list" />
                        <asp:Button ID="recoverButton" runat="server" Text="Recover saved state" OnClick="recoverButton_Click" Enabled="false" ToolTip="Recover the last saved state" />
                        <asp:Button ID="clearButton" runat="server" Text="Clear" OnClick="clearButton_Click" ToolTip="Remove all items from the list" /><br />
                            <br />
                            <asp:CheckBox ID="CheckBox1" runat="server" Checked="True" OnCheckedChanged="CheckBox1_CheckedChanged"
                                Text="Show saved state" AutoPostBack="True" /></td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <br />
                            <br />
                            &nbsp;&nbsp;
                            <hr />
                            Message: <asp:Label ID="Message" runat="server" ForeColor="SteelBlue" />&nbsp;&nbsp;<br />
                            <asp:Label ID="StateLabel" runat="server" Text="State:"></asp:Label>
                            <asp:Label ID="SavedState" runat="server"/><br />                        
                        </td>
                    </tr>
                </table>
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:UpdateProgress ID="UpdateProgress1" runat="server">
            <ProgressTemplate>
                <asp:Image ID="Image1" runat="server" ImageUrl="..\images\spinner.gif" />&nbsp;Processing...
            </ProgressTemplate>
        </asp:UpdateProgress>
    </form>
</body>
</html>
