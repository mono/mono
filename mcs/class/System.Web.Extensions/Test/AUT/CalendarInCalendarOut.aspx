<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
 "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

	protected void Page_Load (object sender, EventArgs e) {

	}

	protected override void OnInit (EventArgs e) {
		base.OnInit (e);

		Calendar1.TodaysDate = Calendar2.TodaysDate = new DateTime (2007, 07, 01);
	}
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>UpdatePanel Tutorial</title>
    <style type="text/css">
    #UpdatePanel1 { 
      width:300px;
     }
    </style>
    <script language="javascript" type="text/javascript">
    var callCount = 1;
    function myPageLoad()
    {
        window.opener = window;
        document.getElementById("Lable1").innerHTML = "pageLoad called: " + callCount;
    }
    
    function IncrementLoadCount()
    {
        callCount++;
        document.getElementById("Lable1").innerHTML = "pageLoad called: " + callCount;
    }
    </script>
</head>
<body onload="myPageLoad()">
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>
        <span id="Lable1" visible="true">Not Initialized</span>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <fieldset onclick="IncrementLoadCount()">
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