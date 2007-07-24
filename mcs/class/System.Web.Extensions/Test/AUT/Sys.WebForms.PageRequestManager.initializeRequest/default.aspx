<%@ Page Language="C#" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected void ButtonClick_Handler(object sender, EventArgs e)
    {
        System.Threading.Thread.Sleep(3000);
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>PageRequestManager initializeRequest Example</title>
    <style type="text/css">
    body {
        font-family: Tahoma;
    }
    a  {
        text-decoration: none;
    }
    a:hover {
        text-decoration: underline;
    }
    div.UpdatePanelStyle{
       width: 300px;
       height: 300px;
    }
    div.AlertStyle {
      font-size: smaller;
      background-color: #FFC080;
      height: 20px;
      visibility: hidden;
    }
    div.Container {
      display: inline;
      float: left;
      width: 330px;
      height: 300px;
    }
	</style>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server" />
            <script type="text/javascript" language="javascript">
                var divElem = 'AlertDiv';
                var messageElem = 'AlertMessage';
                Sys.WebForms.PageRequestManager.getInstance().add_initializeRequest(CheckStatus);
                function CheckStatus(sender, args)
                {
                  var prm = Sys.WebForms.PageRequestManager.getInstance();
                  if (prm.get_isInAsyncPostBack() & args.get_postBackElement().id == 'CancelRefresh') {
                     prm.abortPostBack();
                  }
                  else if (prm.get_isInAsyncPostBack() & args.get_postBackElement().id == 'RefreshButton') {
                     args.set_cancel(true);
                     ActivateAlertDiv('visible', 'Still working on previous request.');
                 }
                  else if (!prm.get_isInAsyncPostBack() & args.get_postBackElement().id == 'RefreshButton') {
                     ActivateAlertDiv('visible', 'Processing....');
                  }
                }
                function ActivateAlertDiv(visString, msg)
                {
                     var adiv = $get(divElem);
                     var aspan = $get(messageElem);
                     adiv.style.visibility = visString;
                     aspan.innerHTML = msg;
                }
            </script>
            <div class="Container" >
            <asp:UpdatePanel  ID="UpdatePanel1" UpdateMode="Conditional" runat="Server"  >
                <ContentTemplate>
                    <asp:Panel ID="Panel1" runat="server" GroupingText="UpdatePanel" 
                    CssClass="UpdatePanelStyle">
                        Last update: 
                        <%=DateTime.Now.ToString() %>.
                        <asp:Button runat="server" ID="RefreshButton" Text="Refresh"
                            OnClick="ButtonClick_Handler" />
                        <div id="AlertDiv" class="AlertStyle">
                        <span id="AlertMessage"></span> &nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:LinkButton ID="CancelRefresh" runat="server">cancel</asp:LinkButton>
                        </div>                        
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
            </div>
        </div>
    </form>
</body>
</html>
