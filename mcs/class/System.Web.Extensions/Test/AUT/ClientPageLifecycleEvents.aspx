<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <fieldset>
                    <legend>Update Panel1</legend>
                    Last Update: <div id="LastUpdate"><%= System.DateTime.Now.ToString() %></div>
                    <br />
                    <asp:Button ID="Button1" Text="Update" runat="server" />
                </fieldset>
            </ContentTemplate>
        </asp:UpdatePanel>
        <div id="div1">No Events</div>
        <input type="button" value="Add Handlers" onclick="AddHandlers()" />
        <input type="button" value="Remove Handlers" onclick="RemoveHandlers()" />
        <input type="button" value="Clear" onclick="ClearDiv1()" />
        
        <script language="javascript" type="text/javascript">
        
        function AddHandlers()
        {
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.add_initializeRequest(InitializeRequestHandler);
            prm.add_beginRequest(BeginRequestHandler);
            prm.add_pageLoading(PageLoadingHandler);
            prm.add_pageLoaded(PageLoadedHandler);
            prm.add_endRequest(EndRequestHandler);
        }
        
        function RemoveHandlers()
        {
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.remove_initializeRequest(InitializeRequestHandler);
            prm.remove_beginRequest(BeginRequestHandler);
            prm.remove_pageLoading(PageLoadingHandler);
            prm.remove_pageLoaded(PageLoadedHandler);
            prm.remove_endRequest(EndRequestHandler);
        }
        
        function ClearDiv1()
        {
            $get("div1").innerHTML = "No Events";
        }
        
        function InitializeRequestHandler(sender, args)
        {
            if ($get("div1").innerHTML == "No Events"){
                $get("div1").innerHTML = "";
            }
            $get("div1").innerHTML = $get("div1").innerHTML + "InitializeRequest:";
        }
        
        function BeginRequestHandler(sender, args)
        {
            $get("div1").innerHTML = $get("div1").innerHTML + "BeginRequest:";
        }

        function PageLoadingHandler(sender, args)
        {
            $get("div1").innerHTML = $get("div1").innerHTML + "PageLoading:";
        }

        function PageLoadedHandler(sender, args)
        {
            $get("div1").innerHTML = $get("div1").innerHTML + "PageLoaded:";
        }

        function EndRequestHandler(sender, args)
        {
            $get("div1").innerHTML = $get("div1").innerHTML + "EndRequest:";
        }

        </script>
    </div>
    </form>
</body>
</html>
