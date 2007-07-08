<%@ Page Language="C#" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<%@ Register TagPrefix="Samples" 
    TagName="AjaxProfile" Src="LoginProfileControl.ascx" %>


<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">

    <title>Profile User Control</title>
    
    <style type="text/css">
        body {  font: 11pt Trebuchet MS;
                font-color: #000000;
                padding-top: 72px;
                text-align: center }

        .text { font: 8pt Trebuchet MS }
    </style>
   
    <script language="C#" runat="server">
        
        // Add the ScriptManager to the page programmatically.
        protected void Page_PreInit(object sender, EventArgs e)
        {
            ScriptManager sm = ScriptManager.GetCurrent(this.Page);
            if (sm == null)
            {
                sm = new ScriptManager();
                sm.ID = "ScriptManagerId";
                form1.Controls.Add(sm);
            }
        }
    </script>
</head>
<body>
	<h2>Profile User Control</h2>
    
    <form id="form1" runat="server">
        <div>
            <Samples:AjaxProfile ID="ProfileId" runat="server" />
        </div>
    </form>
  
    <span style="font-weight:normal; font-size:medium; color:Black">
        Please, use one of the following [username, password] 
        combinations:<br />
        [user1, u$er1] <br/>
        [user2, u$er2] <br/> 
        [user3, u$er3]   
    </span>   
		    
</body>

</html>
