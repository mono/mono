<%@ Page Language="C#" %>


<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected void Button1_Click(object sender, EventArgs e)
    {
        Label1.Text = "Panel refreshed at " +
            DateTime.Now.ToString();
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>UpdatePanel Tutorial</title>
    <style type="text/css">
    #UpdatePanel1, #UpdatePanel2 { 
      width:300px; height:100px;
     }
    </style>
    <script language="javascript" type="text/javascript">
    var p1UpdateCount = 1;
    var p2UpdateCount = 1;
    function UpdateSpan1()
    {
        document.getElementById("span1").innerHTML = "Panel1: " + p1UpdateCount + ", Panel2: " + p2UpdateCount;
    }
    </script>
</head>
<body onload="UpdateSpan1()">
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager id="ScriptManager1" runat="server">
        </asp:ScriptManager>
        <span id="span1">Not Initialized</span>
        <asp:UpdatePanel id="UpdatePanel1" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <fieldset>
                <legend>UpdatePanel1</legend>
                <asp:Label ID="Label1" runat="server" Text="Panel Created"></asp:Label><br />
                <asp:Button ID="Button1" runat="server" Text="Refresh Panel 1" OnClick="Button1_Click" />
                </fieldset>
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <fieldset>
                <legend>UpdatePanel2</legend>
                <asp:Calendar ID="Calendar1" runat="server"></asp:Calendar>
                </fieldset>
            </ContentTemplate>
        </asp:UpdatePanel>
        
        <script language="javascript" type="text/javascript">

        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(pageLoaded);

        function pageLoaded(sender, args) 
        {
            var updatedPanels = args.get_panelsUpdated();
            for (i=0; i < updatedPanels.length; i++) {
                if (updatedPanels[i].id == "UpdatePanel1"){
                    p1UpdateCount++;
                } else if (updatedPanels[i].id == "UpdatePanel2"){
                    p2UpdateCount++;
                }           
            }
            UpdateSpan1();
        }
        </script>
    </div>
    </form>
</body>
</html>