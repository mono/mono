<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {
        ScriptManager1.RegisterAsyncPostBackControl(Button1);
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        Label1.Text = "Panel refreshed at " + DateTime.Now.ToString();
    }
    protected void Button2_Click(object sender, EventArgs e)
    {
        Label1.Text = "Page refreshed.";
    }
</script>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>ScriptManager RegisterAsyncPostBackControl Example</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server"/>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
        <fieldset>
        <legend>Update Panel</legend>
        <asp:Label ID="Label1" runat="server">Initial postback occurred.</asp:Label>
        </fieldset>
        </ContentTemplate>
        </asp:UpdatePanel>
        <asp:Button ID="Button1" runat="server" Text="Update Panel" OnClick="Button1_Click" />
        <asp:Button ID="Button2" runat="server" Text="Refresh Page" OnClick="Button2_Click" />
        </div>
    </form>
</body>
</html>
