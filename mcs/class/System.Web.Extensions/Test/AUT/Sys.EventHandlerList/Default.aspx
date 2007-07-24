<%@ Page Language="C#" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>EventHandlerList Example</title>
</head>
<body>
<form id="form1" runat="server">
    <div id="ResultDisplay"></div>

        <asp:ScriptManager runat="server" ID="ScriptManager01">
            <scripts>
               <asp:ScriptReference Path="HoverButton.js" />
            </scripts>
        </asp:ScriptManager>

        <script type="text/javascript">
            var app = Sys.Application;
            // Add the handler function to the pageLoad event.
            app.add_load(applicationLoadHandler);

            function applicationLoadHandler(sender, args) {
                $create(
                    Demo.HoverButton, 
                    {element: {style: {borderWidth: "2px"}}},
                    // Bind the start function to the click event.
                    {click: start},
                    null,
                    $get('Button1')
                    );
            }

            function start(sender, args) {
               alert("The start function handled the HoverButton click event.");
            }
        </script>

        <button type="button" id="Button1" value="HoverButton">
            HoverButton
        </button>
</form>
</body>
</html>
