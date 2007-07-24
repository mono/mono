<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>UpdatePanel Trigger Declarative Syntax</title>
    <style type="text/css">
    body {
        font-family: Lucida Sans Unicode;
        font-size: 10pt;
    }
    button {
        font-family: tahoma;
        font-size: 8pt;
    }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <asp:Button ID="Button1" 
                Text="Refresh Panel"
                runat="server" />
    <asp:ScriptManager ID="ScriptManager1" 
                       runat="server" />
    <asp:UpdatePanel ID="UpdatePanel1" 
                     UpdateMode="Conditional"
                     runat="server">
                     <Triggers>
                       <asp:AsyncPostBackTrigger ControlID="Button1" />
                     </Triggers>
                     <ContentTemplate>
                     <fieldset>
                     <legend>UpdatePanel content</legend>
                     <%=DateTime.Now.ToString() %>
                     </fieldset>
                     </ContentTemplate>
    </asp:UpdatePanel>

    </div>
    </form>
</body>
</html>
