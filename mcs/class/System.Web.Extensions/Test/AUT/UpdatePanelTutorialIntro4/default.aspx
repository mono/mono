<%@ Page Language="C#" %>


<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected void Button1_Click(object sender, EventArgs e)
    {
        Label1.Text = "Panel refreshed at " +
            DateTime.Now.ToString();
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>UpdatePanel Tutorial</title>
    <style type="text/css">
    #UpdatePanel1, #UpdatePanel2 { 
      width:300px; height:100px;
     }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager id="ScriptManager1" runat="server">
        </asp:ScriptManager>
        <asp:UpdatePanel id="UpdatePanel1" runat="server">
            <ContentTemplate>
                <fieldset>
                <legend>UpdatePanel1</legend>
                <asp:Label ID="Label1" runat="server" Text="Panel Created"></asp:Label><br />
                <asp:Button ID="Button1" runat="server" Text="Refresh Panel 1" OnClick="Button1_Click" />
                </fieldset>
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:UpdatePanel ID="UpdatePanel2" runat="server">
            <ContentTemplate>
                <fieldset>
                <legend>UpdatePanel2</legend>
                <asp:Calendar ID="Calendar1" runat="server"></asp:Calendar>
                </fieldset>
            </ContentTemplate>
        </asp:UpdatePanel>
    
    </div>
    </form>
</body>
</html>
