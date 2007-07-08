<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    protected void ScriptManager1_ResolveScriptReference(object sender, ScriptReferenceEventArgs e)
    {
        if (e.Script.Path.Contains("CustomScript"))
        {
            if (HttpContext.Current.Request.Url.Host.ToLower() == "www.contoso.com")
            {
                e.Script.Path = "http://www.contoso.com/ScriptRepository/CustomScript.js";
            }
        }
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Script Reference Example</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager OnResolveScriptReference="ScriptManager1_ResolveScriptReference" ID="ScriptManager1" runat="server">
        <Scripts>
            <asp:ScriptReference Path="~/scripts/CustomScript.js" />
        </Scripts>
        </asp:ScriptManager>
    </div>
    </form>
</body>
</html>
