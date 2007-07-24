<%@ Page Language="C#" %>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">

    <title>Using Profile Service</title>
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
                <asp:ScriptReference Path="Profile.js" />
            </Scripts>
        </asp:ScriptManager>
 
	
	
		<div id="loginId" style="visibility:visible">
			<table id="loginForm">
				<tr>
					<td>UserId</td>
					<td><input type="text" 
					    id="userId" name="userId" value=""/></td>
				</tr>
				
				<tr>
					<td>Password</td>
					<td><input type="password" 
					    id="userPwd" name="userPwd" value="" /></td>
				</tr>
				
				<tr>
					<td><input type="button" 
					    id="login" name="login" value="Login" 
					        onclick="OnClickLogin()" /></td>
				</tr>
			</table>				
		
		    <p>
		        Please use one of the following [username, password] combinations:<br />
		        [user1, u$er1] <br/>
		        [user2, u$er2] <br/> 
		        [user3, u$er3] 
		    </p>
		    
		</div>
		
		<div id="setProfProps" style="visibility:hidden">
			<input type="button" 
			value="Set Profile Properties" 
			onclick="SetProfileControlsVisibility('visible')"/> 
		</div>
		
		<div id="placeHolder" style="visibility:visible" />
		
		<br />
		
		
	    <input id="logoutId" type="button" 
	        value="Logout"  style="visibility:hidden"
	    onclick="OnClickLogout()" />
		
		
		
		<div id="setProfileProps" style="visibility:hidden">
			<table>
				<tr>
					<td>Foreground Color</td>
					<td><input type="text" id="fgcolor" 
					name="fgcolor" value=""/></td>
				</tr>
				
				<tr>
					<td>Background Color</td>
					<td><input type="text" id="bgcolor" 
					    name="bgcolor" value="" /></td>
				</tr>
				
				<tr>
					<td><input type="button" 
					id="saveProf" name="saveProf" 
					value="Save Profile Properties" 
					onclick="SaveProfile();" /></td>
				</tr>
			</table>		
		</div>	    
	
    </form>
  
</body>

</html>
