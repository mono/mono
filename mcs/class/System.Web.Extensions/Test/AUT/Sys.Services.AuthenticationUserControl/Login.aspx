<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<%@ Register TagPrefix="Samples" 
    TagName="AjaxLogin" Src="LoginControl.ascx" %>


<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <style type="text/css">
        body {  font: 11pt Trebuchet MS;
                font-color: #000000;
                padding-top: 72px;
                text-align: center }
        .text { font: 8pt Trebuchet MS }
    </style>
    <title>Login User Control</title>
  
</head>
<body>
    <h2>Login User Control</h2>
    
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager runat="server" ID="ScriptManagerId"/>
            
            <Samples:AjaxLogin ID="LoginId" runat="server" />
      
            <span style="font-weight:normal; font-size:medium; color:Black">
                Please, use one of the following [username, password] 
                combinations:<br />
                [user1, u$er1] <br/>
                [user2, u$er2] <br/> 
                [user3, u$er3]   
            </span>   
        </div>
       
    </form>
    <div>
        <a href="secured/Default.aspx" target="_top2" >Attempt to access a page 
         that requires authenticated users.</a>
    </div>
</body>
</html>
