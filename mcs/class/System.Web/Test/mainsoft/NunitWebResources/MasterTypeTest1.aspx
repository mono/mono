<%@ Page Language="C#" AutoEventWireup="true" %>
<%@ MasterType VirtualPath="~/MyDerived.master" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
    <script runat="server">
        protected override void OnLoad (EventArgs e)
        {
            myderived_master master = new myderived_master ();
            base.OnLoad (e);
            Response.Write(master.MasterTypeMethod ());
        }
    
    </script>

</head>
<body>
    <form id="form1" runat="server">
    <div>
    
    </div>
    </form>
</body>
</html>
