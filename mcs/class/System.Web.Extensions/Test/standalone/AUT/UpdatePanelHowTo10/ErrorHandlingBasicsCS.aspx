<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected void ErrorProcessClick_Handler(object sender, EventArgs e)
    {
        // This handler demonstrates an error condition. In this example
        // the server error gets intercepted on the client and an alert is shown.
        Exception exc = new ArgumentException();
        exc.Data["GUID"] = Guid.NewGuid().ToString().Replace("-"," - "); 
        throw exc;
    }
    protected void SuccessProcessClick_Handler(object sender, EventArgs e)
    {
        // This handler demonstrates no server side exception.
        UpdatePanelMessage.Text = "The asynchronous postback completed successfully.";
    }

    protected void ScriptManager1_AsyncPostBackError(object sender, AsyncPostBackErrorEventArgs e)
    {
        if (e.Exception.Data["GUID"] != null)
        {
            ScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message +
                " When reporting this error use the following ID: " +
                e.Exception.Data["GUID"].ToString();
        }
        else
        {
            ScriptManager1.AsyncPostBackErrorMessage =
                "The server could not process the request.";
        }
            
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>PageRequestManager Error Example</title>
    <style type="text/css">
    body {
        font-family: Tahoma;
    }
    #AlertDiv{
    left: 40%; top: 40%;
    position: absolute; width: 200px;
    padding: 12px; 
    border: #000000 1px solid;
    background-color: white; 
    text-align: left;
    visibility: hidden;
    z-index: 99;
    }
    #AlertButtons{
    position: absolute;
    right: 5%;
    bottom: 5%;
    }
	</style>
</head>
<body id="bodytag">
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" 
                               OnAsyncPostBackError="ScriptManager1_AsyncPostBackError"
                               runat="server"  />
            <script type="text/javascript" language="javascript">
            </script>
            <script type="text/javascript" language="javascript">
                var divElem = 'AlertDiv';
                var messageElem = 'AlertMessage';
                var bodyTag = 'bodytag';
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
                function ToggleAlertDiv(visString)
                {
                     if (visString == 'hidden')
                     {
                         $get(bodyTag).style.backgroundColor = 'white';                         
                     }
                     else
                     {
                         $get(bodyTag).style.backgroundColor = 'gray';                         
                        
                     }
                     var adiv = $get(divElem);
                     adiv.style.visibility = visString;
                     
                }
                function ClearErrorState() {
                     $get(messageElem).innerHTML = '';
                     ToggleAlertDiv('hidden');                     
                }
                function EndRequestExampleHandler(sender, args)
                {
                   if (args.get_error() != undefined)
                   {
                       var errorMessage = args.get_error().message;
                       args.set_errorHandled(true);
                       ToggleAlertDiv('visible');
                       $get(messageElem).innerHTML = errorMessage;
                   }
                }
                function EndRequestHandler(sender, args)
                {
                   if (args.get_error() != undefined)
                   {
                       var errorMessage = args.get_error().message;
                       args.set_errorHandled(true);
                       // Provide other custom script actions here.
                       alert(errorMessage);
                   }
                }
            </script>
            <asp:UpdatePanel runat="Server" UpdateMode="Conditional" ID="UpdatePanel1">
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
            <div id="AlertDiv">
                <div id="AlertMessage">
                </div>
                <br />
                <div id="AlertButtons" >
                    <input id="OKButton" type="button" value="OK" runat="server" onclick="ClearErrorState()" />
                </div>
           </div>
        </div>
    </form>
</body>
</html>
