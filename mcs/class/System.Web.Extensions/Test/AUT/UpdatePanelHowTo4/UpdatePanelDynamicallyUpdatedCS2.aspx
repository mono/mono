<!-- This is a helper file to grab snippets from -->
<!-- This is not meant to be run -->
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>UpdatePanel Update Method Example</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="TheScriptManager" 
                               runat="server" />
            <asp:UpdatePanel ID="UpdatePanel1" 
                             UpdateMode="Conditional"
                             runat="server">
                <ContentTemplate>

                </ContentTemplate>
            </asp:UpdatePanel>
            <br />
        </div>
    </form>
</body>
</html>
