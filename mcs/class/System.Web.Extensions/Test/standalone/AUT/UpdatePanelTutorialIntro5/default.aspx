<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
 "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">


<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>UpdatePanel Tutorial</title>
    <style type="text/css">
    #UpdatePanel2  {
      position: relative;
      margin: 2% 5% 2% 5%;
    }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager id="ScriptManager1" runat="server">
        </asp:ScriptManager>
        <asp:UpdatePanel id="UpdatePanel1" UpdateMode="Conditional" runat="server">
            <ContentTemplate>
                <fieldset>
                <legend>Parent UpdatePanel</legend>
                Last refresh <%=DateTime.Now.ToString() %> <br />
                <asp:Button ID="Button1" runat="server" Text="Refresh Outer Panel" />
                <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                    <ContentTemplate>
                        <fieldset>
                        <legend>Nested UpdatePanel</legend>
                         Last refresh <%=DateTime.Now.ToString() %> <br />
                        <asp:Calendar ID="Calendar1" runat="server"></asp:Calendar>
                        </fieldset>
                    </ContentTemplate>
                </asp:UpdatePanel>
                </fieldset>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    </form>
</body>
</html>
