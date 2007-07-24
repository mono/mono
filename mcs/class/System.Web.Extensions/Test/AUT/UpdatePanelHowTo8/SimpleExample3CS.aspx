<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
 "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Handling the pageLoaded Event</title>
    <style type="text/css">
    body {
        font-family: Tahoma;
    }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <asp:ScriptManager ID="ScriptManager1" runat="server" />
    <script type="text/javascript" language="Javascript">
        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(PageLoaded);
        function PageLoaded(sender, args)
        {
            var updatedPanels = args.get_panelsUpdated();
            for (i=0; i < updatedPanels.length; i++) 
            {
                // Call a custom animation and pass the DOM
                // element representing the panel which is
                // represented by updatedPanels[i]
            }
        }
    </script>
    </div>
    </form>
</body>
</html>
