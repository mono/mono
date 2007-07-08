<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
 "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Client Event Example</title>
    <style type="text/css">
    #OuterPanel { width: 600px; height: 200px; border: 2px solid blue; }
    #NestedPanel { width: 596px; height: 60px; border: 2px solid green; 
                   margin-left:5 px; margin-right:5px; margin-bottom:5px;}
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        <Scripts>
           <asp:ScriptReference Path="ClientEventTest.js" />
        </Scripts>
        </asp:ScriptManager>
        <asp:UpdatePanel ID="OuterPanel" UpdateMode="Conditional" runat="server">
        <ContentTemplate>
            Postbacks from inside the outer panel and inner panel are
            asynchronous postbacks. PRM = Sys.WebForms.PageRequestManager. APP = Sys.Application.

            <br /><br />
            <asp:Button ID="OPButton1" Text="Outer Panel Button" runat="server" />
            Last updated on
            <%= DateTime.Now.ToString() %>
            <br /><br />

            <asp:UpdatePanel ID="NestedPanel" UpdateMode="Conditional" runat="server">
            <ContentTemplate>
                <asp:Button ID="NPButton1" Text="Nested Panel 1 Button" runat="server" />
                Last updated on
                <%= DateTime.Now.ToString() %>
                <br />
            </ContentTemplate>
            </asp:UpdatePanel>
        </ContentTemplate>
        </asp:UpdatePanel>

        <input type="button" onclick="Clear();" value="Clear" />

        <asp:Button ID="FullPostBack" runat="server" Text="Full Postback" />
        <a href="http://www.microsoft.com">Test Window Unload</a>
        <br />
        <span id="ClientEvents"></span>    
    </div>
    </form>
</body>
</html>
