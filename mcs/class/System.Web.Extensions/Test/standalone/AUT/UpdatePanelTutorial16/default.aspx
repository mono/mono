<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>UpdatePanel Animation Example</title>
    <style type="text/css">
    #UpdatePanel1 {
      width: 300px;
      height: 100px;
      border:solid 1px gray;
    }    
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        <Scripts>
        <asp:ScriptReference Path="UpdatePanelAnimation.js" />
        </Scripts>
        </asp:ScriptManager>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <%=DateTime.Now.ToString() %>
                <asp:Button ID="AnimateButton1" runat="server" Text="Refresh with Animation" />
                <asp:Button ID="Button2" runat="server" Text="Refresh with no Animation" />
            </ContentTemplate>
        </asp:UpdatePanel>
    
    </div>
    </form>
</body>
</html>
