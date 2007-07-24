<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">


<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Authentication Service</title>
    <style type="text/css">
        body {  font: 11pt Trebuchet MS;
                font-color: #000000;
                padding-top: 72px;
                text-align: center }

        .text { font: 8pt Trebuchet MS }
    </style>
</head>
<body>
      <form id="form1" runat="server">
        
        <asp:ScriptManager runat="server" ID="ScriptManagerId">
            <Scripts>
                <asp:ScriptReference Path="Authentication.js" />
            </Scripts>
        </asp:ScriptManager>
 
        <h2>Authentication Service</h2>
        
            <span id="loggedin" 
                style="visibility:hidden; color:Green; font-weight:bold; font-size:large" 
                visible="false"><b>You are logged in! </b>
            </span> 
            
            <span id="notloggedin" 
                style="visibility:visible;color:Red; font-weight:bold; font-size:large">
                You are logged out!    
                <br /> <br />
                <span style="font-weight:normal; font-size:medium; color:Black">
                    Please, use one of the following [username, password] 
                    combinations:<br />
	                [user1, u$er1] <br/>
	                [user2, u$er2] <br/> 
	                [user3, u$er3]   
	            </span>   
            </span>
            
        
        <br /><br />
        <div>
        <table>
            <tr id="NameId"  style="visibility:visible;">
                <td style="background-color:Yellow; font-weight:bold; color:Red">
                    User Name:
                </td>
                <td>
                    <input type="text" id="username"/>
                </td> 
            </tr>
            <tr id="PwdId"  style="visibility:visible;">
               <td style="background-color:Yellow; font-weight:bold; color:Red">
                    Password:
                </td>
                <td>
                    <input type="password" id="password" />
                </td> 
            </tr>   
            <tr>
                <td colspan="2" align="center">
                    <button id="ButtonLogin"   
                        style="background-color:Aqua;"
                        onclick="OnClickLogin(); return false;">Login</button>
                    <button id="ButtonLogout"   
                        style="background-color:Aqua; visibility:hidden;"
                        onclick="OnClickLogout(); return false;">Logout</button>
                </td> 
            </tr>          
        </table>
        <br />
        <br />
        <a href="secured/Default.aspx" target="_top2" >
            Attempt to access a page 
            that requires authenticated users.</a>
        <br />
        <br />
        <!-- <a href="CreateNewUser.aspx"><b>Create a new user.</b></a>
        -->        
        </div>
       
   </form>
   
   <hr />
   
   <div id="FeedBackID" style="visibility:visible" />
   
    
</body>
</html>
