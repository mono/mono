
<%@ Page Language="C#" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected void Button1_Click(object sender, EventArgs e)
    {
        System.Threading.Thread.Sleep(5000);
        Label1.Text = "Last update from server " + DateTime.Now.ToString();        
    }

    protected void Button2_Click(object sender, EventArgs e)
    {
        System.Threading.Thread.Sleep(2000);
        Label2.Text = "Laster update from server " + DateTime.Now.ToString();        
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>PageRequestManager Multiple Asynchronous Postbacks Example</title>
    <style type="text/css">
    body {
        font-family: Tahoma;
    }
	div.MessageStyle
	{
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
            <asp:ScriptManager ID="ScriptManager1" runat="server" />
            <script type="text/javascript" language="javascript">
                Sys.WebForms.PageRequestManager.getInstance().add_initializeRequest(InitializeRequest);
                var divElem = 'AlertDiv';
                var messageElem = 'AlertMessage';
                var exclusivePostBackElement = 'Button1';
                var exclusiveFlag;
                function InitializeRequest(sender, args)
                {
                  var prm = Sys.WebForms.PageRequestManager.getInstance();
                  if (prm.get_isInAsyncPostBack())
                  {
                    if (typeof exclusiveFlag != 'undefined' & exclusiveFlag )
                    {
                        args.set_cancel(true);
                        // Set UI elements to inform users that 
                        // new asynchronous postback was canceled.
                       ActivateAlertDiv('visible', 'A previous postback is still executing. The new postback has been canceled.');
                       setTimeout("ActivateAlertDiv('hidden','')", 1500);                        
                    }
                  }
                  else
                  {
                    exclusiveFlag = false;
                    if (args.get_postBackElement().id == exclusivePostBackElement)
                    {
                      exclusiveFlag = true;
                    }
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
            <asp:UpdatePanel  ID="UpdatePanel1" UpdateMode="Conditional" runat="Server" >
                <ContentTemplate>
                <fieldset>
                <legend> 
                UpdatePanel 1
                </legend>
                The submit button in this panel invokes a long running process or critical process
                on the server that we want to make sure does not get overridden by another
                asynchronous postback. Multiple clicks of the submit button in this panel
                or panel 2 are intercepted and canceled until the response from any existing
                postback from this panel is finished.<br />
                <asp:Label ID="Label1" runat="server"></asp:Label><br />
                <asp:Button ID="Button1" runat="server" Text="Submit" OnClick="Button1_Click" />                
                </fieldset>
                </ContentTemplate>
            </asp:UpdatePanel>
            <hr />
            <asp:UpdatePanel  ID="UpdatePanel2" UpdateMode="Conditional" runat="Server" >
                <ContentTemplate>
                <fieldset>
                <legend> 
                UpdatePanel 2
                </legend>
                Clicking the submit button in this panel while an existing postback from
                panel 1 is still processing causes the panel 2 postback to be canceled. Clicking
                the submit button in this panel multiple times results in the default behavior,
                that is, last postback wins.<br />
                <asp:Label ID="Label2" runat="server"></asp:Label><br />
                <asp:Button ID="Button2" runat="server" Text="Submit" OnClick="Button2_Click" />
                </fieldset>
                </ContentTemplate>
            </asp:UpdatePanel>
           <div id="AlertDiv" class="MessageStyle">
           <span id="AlertMessage"></span>
           </div>

        </div>
    </form>
</body>
</html>
