
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected void Calendar1_SelectionChanged(object sender, EventArgs e)
    {
        TextBox1.Text = Calendar1.SelectedDate.ToShortDateString();
        Label1.Text = "";
    }
    protected void Button1_Click(object sender, EventArgs e)
    {
        Label1.Text = "Tickets are available as of " + DateTime.Now.ToString() + ".";
    }

</script>
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>UpdatePanel with Validators Example</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server" />
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                Select a date below or enter a date:
                <asp:TextBox ID="TextBox1" runat="server" Width="70px"></asp:TextBox><br />
                <asp:Calendar ID="Calendar1" runat="server" OnSelectionChanged="Calendar1_SelectionChanged" />
                <br />
                Specify number of tickets (1-10):
                <asp:TextBox ID="TextBox2" runat="server" Width="40px"></asp:TextBox><br />
                <asp:Button ID="Button1" runat="server" OnClientClick="ClearLastMessage('Label1')" Text="Check Availability" OnClick="Button1_Click" />
                <br />
                <br />
                <asp:Label ID="Label1" runat="server"></asp:Label>
                <br />
            </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </form>
</body>
</html>

