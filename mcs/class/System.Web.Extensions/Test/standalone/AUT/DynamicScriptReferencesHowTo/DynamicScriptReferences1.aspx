<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
    "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
    
<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {
        // If there is a ScriptManager on the page, use it.
        // If not, throw an exception.
        ScriptManager Smgr = ScriptManager.GetCurrent(Page);
        if (Smgr == null) throw new Exception("ScriptManager not found.");
        
        ScriptReference SRef = new ScriptReference();
        
        //// To set ScriptMode for all scripts on the page...
        Smgr.ScriptMode = ScriptMode.Release;
        SRef.Path = "~/DynamicScriptReferencesHowTo/Scripts/Scripts.js";
        Smgr.Scripts.Add(SRef);
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Dynamic Script References</title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:scriptmanager ID="ScriptManager1" runat="server" />
        <div>
        </div>
    </form>
</body>
</html>
