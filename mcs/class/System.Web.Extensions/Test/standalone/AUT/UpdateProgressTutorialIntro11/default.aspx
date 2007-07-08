<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
 "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    protected void Button1_Click(object sender, EventArgs e)
    {
        System.Threading.Thread.Sleep(3000);
        Label1.Text = DateTime.Now.ToString();
    }
    protected void Panel1Trigger_Click(object sender, EventArgs e)
    {
        System.Threading.Thread.Sleep(3000);
        Label1.Text = DateTime.Now.ToString() + " - trigger";
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>UpdateProgress Tutorial</title>
    <style type="text/css">
    #UpdatePanel1 {
      width:200px; height:100px;
      border: 1px solid gray;
    }
    #UpdateProgress1 {
      width:200px; background-color: #FFC080;
      bottom: 0%; left: 0px; position: absolute;
    }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        <script language="javascript" type="text/javascript">
        <!-- 
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        function CancelAsyncPostBack() {
            if (prm.get_isInAsyncPostBack()) {
              prm.abortPostBack();
            }
        }
        prm.add_initializeRequest(InitializeRequest);
        prm.add_endRequest(EndRequest);
        var postBackElement;
        function InitializeRequest(sender, args) {
            if (prm.get_isInAsyncPostBack()) {
                args.set_cancel(true);
            }
            postBackElement = args.get_postBackElement();
            if (postBackElement.id == 'Panel1Trigger') {
                $get('UpdateProgress1').style.display = 'block';                
            }
        }
        function EndRequest(sender, args) {
            if (postBackElement.id == 'Panel1Trigger') {
                $get('UpdateProgress1').style.display = 'none';
            }
        }
        // -->
        </script>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <asp:Label ID="Label1" runat="server" Text="Panel rendered."></asp:Label><br />
                <asp:Button ID="Button1" runat="server" Text="refresh" OnClick="Button1_Click" />
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="Panel1Trigger" />
            </Triggers>
        </asp:UpdatePanel>
        <asp:Button ID="Panel1Trigger" runat="server" Text="Trigger" OnClick="Panel1Trigger_Click" />
        <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
            <ProgressTemplate>
                Processing...
                <input id="Button2" 
                       type="button" 
                       value="cancel"
                       onclick="CancelAsyncPostBack()" />
            </ProgressTemplate>
        </asp:UpdateProgress>
    
    </div>
    </form>
</body>
</html>
