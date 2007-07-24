<%@ Page Language="C#" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    protected void Button1_Click(object sender, EventArgs e)
    {
        System.Threading.Thread.Sleep(4000);
        Label1.Text = "Last update from server " + DateTime.Now.ToString();        
    }

    protected void Button2_Click(object sender, EventArgs e)
    {
        System.Threading.Thread.Sleep(1000);
        Label2.Text = "Last update from server " + DateTime.Now.ToString();        
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Postback Precedence Example</title>
    <style type="text/css">
    body {
        font-family: Tahoma;
    }
    #UpdatePanel1, #UpdatePanel2 {
      width: 400px;
      height: 100px;
      border: solid 1px gray;
    }
	div.MessageStyle {
      background-color: #FFC080;
      top: 95%;
      left: 1%;
      height: 20px;
      width: 600px;
      position: absolute;
      visibility: hidden;
    }
	</style>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server">
            <Scripts>
            <asp:ScriptReference Path="PostBackPrecedence.js" />
            </Scripts>
            </asp:ScriptManager>
            <asp:UpdatePanel  ID="UpdatePanel1" UpdateMode="Conditional" runat="Server" >
                <ContentTemplate>
                <strong>UpdatePanel 1</strong><br />

                This postback takes precedence.<br />
                <asp:Label ID="Label1" runat="server">Panel initially rendered.</asp:Label><br />
                <asp:Button ID="Button1" runat="server" Text="Button" OnClick="Button1_Click" />&nbsp;
                <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
                <ProgressTemplate>
                Panel1 updating...
                </ProgressTemplate>
                </asp:UpdateProgress>
                </ContentTemplate>
            </asp:UpdatePanel>
            <asp:UpdatePanel  ID="UpdatePanel2" UpdateMode="Conditional" runat="Server" >
                <ContentTemplate>
                <strong>UpdatePanel 2</strong><br />
                <asp:Label ID="Label2" runat="server">Panel initially rendered.</asp:Label><br />
                <asp:Button ID="Button2" runat="server" Text="Button" OnClick="Button2_Click" />
                <asp:UpdateProgress ID="UpdateProgress2" runat="server" AssociatedUpdatePanelID="UpdatePanel2">
                <ProgressTemplate>
                Panel2 updating...
                </ProgressTemplate>
                </asp:UpdateProgress>
                </ContentTemplate>
            </asp:UpdatePanel>
           <div id="AlertDiv" class="MessageStyle">
           <span id="AlertMessage"></span>
           </div>
        </div>
    </form>
</body>
</html>
