<%@ Page Language="C#" AutoEventWireup="true" %>
<%@ MasterType TypeName="MonoTests.System.Web.UI.WebControls.PokerMasterPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
    <script runat="server">
        protected override void OnLoad (EventArgs e)
        {
            base.OnLoad (e);
            MonoTests.System.Web.UI.WebControls.PokerMasterPage master = new MonoTests.System.Web.UI.WebControls.PokerMasterPage ();
            Response.Write (master.MasterMethod ());
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
