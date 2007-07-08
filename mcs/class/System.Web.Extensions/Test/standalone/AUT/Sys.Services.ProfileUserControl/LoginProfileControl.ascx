<%@ Control Language="C#" ClassName="LoginProfileControl" %>
<%@Import Namespace="System.Web.UI" %>

<script language="C#" runat="server">
    protected void Page_(object sender, EventArgs e)
    {
        ScriptManager sm = ScriptManager.GetCurrent(this.Page);

        if (sm == null)
        {
            throw new InvalidOperationException(
                "Script Manager not defined. Create one in the page where you use this control.");

        }
    }
    
</script>

   
<script type="text/javascript"> 
   // Profile.js

    // The OnClickLogin function is called when 
    // the user clicks the Login button. 
    // It calls the AuthenticationService.login to
    // authenticates the user.
    function OnClickLogin()
    {
	    Sys.Services.AuthenticationService.login(
	        document.form1.userId.value,
	        document.form1.userPwd.value,false,null,null,
	        OnLoginComplete, OnAuthenticationFailed,
	        "User context information.");
    }

    // The OnClickLogout function is called when 
    // the user clicks the Logout button. 
    // It logs out the current authenticated user.
    function OnClickLogout()
    {
	    Sys.Services.AuthenticationService.logout(
	        null, OnLogoutComplete, OnAuthenticationFailed,null);
    }


    function OnLogoutComplete(result, 
        userContext, methodName)
    {
	    // Code that performs logout 
	    // housekeeping goes here.			
    }		

    // This function is called after the user is
    // authenticated. It loads the user's profile.
    // This is the callback function called 
    // if the authentication completed successfully.
    function OnLoginComplete(validCredentials, 
        userContext, methodName)
    {
	    if(validCredentials == true)
	    {
		    DisplayInformation("Welcome " + document.form1.userId.value);
    			
		    LoadProfile();
    		
		    // Hide or make visible page display elements.
		    GetElementById("loginId").style.display = "none";
	        GetElementById("setProfileButton").style.display ="block";
		    GetElementById("logoutId").style.display = "block";
    	
	    }
	    else
	    {
		    DisplayInformation("Could not login");
	    }
    }

    // This is the callback function called 
    // if the authentication failed.
    function OnAuthenticationFailed(error_object, 
        userContext, methodName)
    {	
        DisplayInformation("Authentication failed with this error: " +
	        error_object.get_message());
    }


    // Loads the profile of the current
    // authenticated user.
    function LoadProfile()
    {
	    Sys.Services.ProfileService.load(null, 
	        OnLoadCompleted, OnProfileFailed, null);
    }

    // Saves the new profile
    // information entered by the user.
    function SaveProfile()
    {

	    Sys.Services.ProfileService.properties.Backgroundcolor = 
	        GetElementById("bgcolor").value;
	        // document.getElementById('bgcolor').value;
	    Sys.Services.ProfileService.properties.Foregroundcolor =
	        GetElementById("fgcolor").value; 
	       // document.getElementById('fgcolor').value;
	    Sys.Services.ProfileService.save(null, 
	        OnSaveCompleted, OnProfileFailed, null);
    }

    // Reads the profile information and displays it.
    function OnLoadCompleted(numProperties, userContext, methodName)
    {
	    document.bgColor = 
	        Sys.Services.ProfileService.properties.Backgroundcolor;

        document.fgColor =   
	        Sys.Services.ProfileService.properties.Foregroundcolor;			
    }

    // This is the callback function called 
    // if the profile was saved successfully.
    function OnSaveCompleted(numProperties, userContext, methodName)
    {
	    LoadProfile();
	    // Hide the area that contains 
	    // the controls to set the profile properties.
        SetProfileControlsVisibility("none");
    }

    // This is the callback function called 
    // if the profile load or save operations failed.
    function OnProfileFailed(error_object, userContext, methodName)
    {
	    alert("Profile service failed with message: " + 
	            error_object.get_message());
    }


    // Utility functions.

    // This function sets the visibilty for the
    // area containing the page elements for settings
    // profiles.
    function SetProfileControlsVisibility(currentVisibility)
    {
        GetElementById("setProfileProps").style.display = 
            currentVisibility; 
    }

    // Utility function to display user's information.
    function DisplayInformation(text)
    {
        
	    document.getElementById('feedBackId').innerHTML = 
	    "<br/>"+ text;
    }

       
    function GetElementById(elementId)
    {
        var element = document.getElementById(elementId);
        return element;
    }
        
    if (typeof(Sys) !== "undefined") Sys.Application.notifyScriptLoaded();

</script>

<script language="C#" runat="server">
    protected void Page_PreLoad(object sender, EventArgs e)
    {
        ScriptManager sm = ScriptManager.GetCurrent(this.Page);

        if (sm == null)
        {
            // throw new InvalidOperationException("sm is null");
            sm = new ScriptManager();
            Page.Controls.Add(sm);
           
        }
    }

</script>

<center>

    <!-- Login form -->
    <div id="loginId" style="display:block;">
		<table id="loginForm">
			<tr>
			    <td style="background-color:Yellow; font-weight:bold; color:Red">User Name:</td>
				<td><input type="text" 
				    id="userId" name="userId" value=""/></td>
			</tr>
			
			<tr>
				<td style="background-color:Yellow; font-weight:bold; color:Red">Password:</td>
				<td><input type="password" 
				    id="userPwd" name="userPwd" value="" /></td>
			</tr>
			
			<tr>
				<td align="center" colspan="2">
				    <input type="button" 
				    id="login" name="login" value="Login" 
				    onclick="OnClickLogin()" /></td>
			</tr>
		</table>				
	</div>
		
	<!-- Set profile button -->
	<div id="setProfileButton" style="display:none">
		<input type="button" 
		value="Set Profile Properties" 
		onclick="SetProfileControlsVisibility('block')"/> 
	</div>
		
    <!-- User feedback -->
	<div id="feedBackId" style="display:block"></div>
		
    <br />
		
	<!-- Logout button -->
    <input id="logoutId" type="button" 
        value="Logout"  style="display:none" 
    onclick="OnClickLogout()" />
		
		
	<!-- Set profile properties form -->
	<div id="setProfileProps" style="display:none" >
		<table>
			<tr>
				<td align="left">Foreground Color</td>
				<td align="left"><input type="text" id="fgcolor" 
				name="fgcolor" value=""/></td>
			</tr>
			
			<tr>
				<td align="left">Background Color</td>
				<td align="left"><input type="text" id="bgcolor" 
				    name="bgcolor" value="" /></td>
			</tr>
			
			<tr>
				<td align="center" colspan="2"><input type="button" 
				id="saveProf" name="saveProf" 
				value="Save Profile Properties" 
				onclick="SaveProfile();" /></td>
			</tr>
		</table>		
	</div>	    
	
</center>

<hr />

<div id="FeedBackID" style="display:none" />
   