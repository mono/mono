
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected void ErrorProcessClick_Handler(object sender, EventArgs e)
    {
        // This handler demonstrates an error condition. In this example
        // the server error gets intercepted on the client and an alert is shown. 
        throw new ArgumentException();
    }
    protected void SuccessProcessClick_Handler(object sender, EventArgs e)
    {
        // This handler demonstrates no server side exception.
        UpdatePanelMessage.Text = "The asynchronous postback completed successfully.";
    }

</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>PageRequestManager endRequestEventArgs Example</title>
    <style type="text/css">
    body {
        font-family: Tahoma;
    }
    div.AlertStyle{
    position: absolute; width: 90%; top: 0%;
    visibility: hidden; z-index: 99; background-color: #ffff99;
    font-size: larger;  font-family: Tahoma; 
    border-right: navy thin solid; border-top: navy thin solid; 
    border-left: navy thin solid; border-bottom: navy thin solid;    
    }
	</style>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager runat="server" ID="ScriptManager1"/>

            <script type="text/javascript" language="javascript">
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
                var divElem = 'AlertDiv';
                var messageElem = 'UpdatePanelMessage';
                function ToggleAlertDiv(visString)
                {
                     var adiv = $get(divElem);
                     adiv.style.visibility = visString;
                     
                }
                function ClearErrorState() {
                     $get(messageElem).innerHTML = '';
                     ToggleAlertDiv('hidden');
                     
                }
                function EndRequestHandler(sender, args)
                {
                   if (args.get_error() != null)
                   {
                       var errorName = args.get_error().name;
                       if (errorName.length > 0 && errorName.indexOf('ServerErrorException') >= 0)
                       {
                          args.set_errorHandled(true);
                          ToggleAlertDiv('visible');
                          $get(messageElem).innerHTML = 'The panel did not update successfully.';
                       }
                   }
                   
                }
            </script>

            <asp:UpdatePanel ID="UpdatePanel1" UpdateMode="Conditional" runat="Server">
                <ContentTemplate>
                    <asp:Panel ID="Panel1" runat="server" GroupingText="Update Panel">
                        <asp:Label ID="UpdatePanelMessage" runat="server" />
                        <br />
                        Last update:
                        <%= DateTime.Now.ToString() %>
                        .
                        <br />
                        <asp:Button runat="server" ID="Button1" Text="Submit Successful Async Postback"
                            OnClick="SuccessProcessClick_Handler" OnClientClick="ClearErrorState()" />
                        <asp:Button runat="server" ID="Button2" Text="Submit Async Postback With Error"
                            OnClick="ErrorProcessClick_Handler" OnClientClick="ClearErrorState()" />
                        <br />
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
            <div id="AlertDiv" class="AlertStyle" style="">
                <div id="AlertMessage" style="float: left">
                    There was a problem processing the last request.
                </div>
                <div id="AlertLinks" style="float: right">
                    <a title="Hide this alert." href="#" onclick="ClearErrorState()">
                        close</a> | <a title="Send an email notifying the Web site owner."
                            href="mailto:someone@example.com" onclick="ToggleAlertDiv('hidden', 'AlertDiv')">
                            notify</a></div>
            </div>
        </div>
    </form>
</body>
</html>
