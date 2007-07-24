<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
 "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    protected void Button1_Click(object sender, EventArgs e)
    {
        // Introducing delay for demonstration.
        System.Threading.Thread.Sleep(3000);
        Label1.Text = "Page refreshed at " +
            DateTime.Now.ToString();       
    }

    protected void Button2_Click(object sender, EventArgs e)
    {
        // Introducing delay for demonstration.
        System.Threading.Thread.Sleep(3000);
        Label2.Text = "Page refreshed at " +
            DateTime.Now.ToString();       

    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>UpdateProgress Tutorial</title>
    <style type="text/css">
    #UpdatePanel1, #UpdatePanel2 { 
      width:300px; height:100px;
     }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <fieldset>
                <legend>UpdatePanel1</legend>
                <asp:Label ID="Label1" runat="server" Text="Panel initially rendered."></asp:Label>
                <br />
                <asp:Button ID="Button1" runat="server" Text="Button" OnClick="Button1_Click" />
                <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
                    <ProgressTemplate>
                        Panel1 updating...
                    </ProgressTemplate>
                </asp:UpdateProgress>
                </fieldset>
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:UpdatePanel ID="UpdatePanel2" runat="server">
            <ContentTemplate>
                <fieldset>
                <legend>UpdatePanel2</legend>
                <asp:Label ID="Label2" runat="server" Text="Panel initially rendered."></asp:Label>
                <br />
                <asp:Button ID="Button2" runat="server" Text="Button" OnClick="Button2_Click" />
                <asp:UpdateProgress ID="UpdateProgress2" runat="server" AssociatedUpdatePanelID="UpdatePanel2">
                    <ProgressTemplate>
                        Panel2 updating....
                    </ProgressTemplate>
                </asp:UpdateProgress>
                </fieldset>
            </ContentTemplate>
        </asp:UpdatePanel>
    
    </div>
    </form>
</body>
</html>
