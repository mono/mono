<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
 "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Make a PostBack Exclusive Example</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        <script type="text/javascript" language="javascript">
        </script>    
        <script type="text/javascript" language="javascript">
            Sys.WebForms.PageRequestManager.getInstance().add_initializeRequest(InitializeRequest);
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
       </script>
    </div>
    </form>
</body>
</html>
