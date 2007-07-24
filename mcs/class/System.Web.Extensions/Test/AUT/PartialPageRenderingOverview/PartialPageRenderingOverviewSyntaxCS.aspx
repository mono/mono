<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    
    protected void Button1_Click(object sender, EventArgs e)
    {
        Label1.Text = "Test";
    }

    protected override void OnPreRender(EventArgs e)
    {
        if (IsPostBack)
        {
            Label2.Text = "Posted Back";
            Label3.Text = "Static text changed";    
        }
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Partial-Page Rendering Server-Side Syntax</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
      <asp:ScriptManager ID="ScriptManager1" runat="server" />
      <asp:UpdatePanel ID="UpdatePanel1" runat="server">
          <ContentTemplate>
              <asp:Button ID="Button1" runat="server" Text="Button" OnClick="Button1_Click"  /> <br />
              <asp:Label ID="Label1" runat="server"></asp:Label> <br />
              <asp:Label ID="Label2" runat="server"></asp:Label> <br />
          </ContentTemplate>
      </asp:UpdatePanel>
      <asp:Label ID="Label3" runat="server">Text from static page area</asp:Label>
    </div>
    </form>
</body>
</html>
