
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected void ProcessClick_Handler(object sender, EventArgs e)
    {
        System.Threading.Thread.Sleep(2000);
    }

</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>PageRequestManager beginRequest Example</title>
    <style type="text/css">
    body {
        font-family: Tahoma;
    }
    div.AlertStyle
    {
      background-color: #FFC080;
      top: 95%;
      left: 1%;
      height: 20px;
      width: 270px;
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
                Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(BeginRequestHandler);
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
                function BeginRequestHandler(sender, args)
                {
                     var elem = args.get_postBackElement();
                     ActivateAlertDiv('visible', 'AlertDiv', elem.value + ' processing...');
                }
                function EndRequestHandler(sender, args)
                {
                     ActivateAlertDiv('hidden', 'AlertDiv', '');
                }
                function ActivateAlertDiv(visstring, elem, msg)
                {
                     var adiv = $get(elem);
                     adiv.style.visibility = visstring;
                     adiv.innerHTML = msg;                     
                }
            </script>

            <asp:UpdatePanel ID="UpdatePanel1" UpdateMode="Conditional" runat="Server">
                <ContentTemplate>
                    <asp:Panel ID="Panel1" runat="server" GroupingText="Update Panel">
                        Last update:
                        <%= DateTime.Now.ToString()%>.
                        <br />
                        <asp:Button runat="server" ID="Button1" Text="Process 1" OnClick="ProcessClick_Handler" />
                        <asp:Button runat="server" ID="Button2" Text="Process 2" OnClick="ProcessClick_Handler" />
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
            <div id="AlertDiv" class="AlertStyle">
            </div>
        </div>
    </form>
</body>
</html>
