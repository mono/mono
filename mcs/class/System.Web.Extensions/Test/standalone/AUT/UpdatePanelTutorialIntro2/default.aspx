<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
 "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>UpdatePanel Tutorial</title>
    <style type="text/css">
    #UpdatePanel1 { 
      width:300px;
     }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <fieldset>
                <legend>UpdatePanel</legend>
                <asp:Calendar ID="Calendar1" runat="server"></asp:Calendar>
                </fieldset>
            </ContentTemplate>
        </asp:UpdatePanel>
        <br />
        <asp:Calendar ID="Calendar2" runat="server"></asp:Calendar>    
    </div>
    </form>
</body>
</html>
