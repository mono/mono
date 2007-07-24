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
		GetElementById("loginId").style.visibility = "hidden";
	    GetElementById("setProfProps").style.visibility = "visible";
		GetElementById("logoutId").style.visibility = "visible";
	
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
    SetProfileControlsVisibility("hidden");
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
    GetElementById("setProfileProps").style.visibility = 
        currentVisibility; 
}

// Utility function to display user's information.
function DisplayInformation(text)
{
	document.getElementById('placeHolder').innerHTML += 
	"<br/>"+ text;
}

   
function GetElementById(elementId)
{
    var element = document.getElementById(elementId);
    return element;
}
    
if (typeof(Sys) !== "undefined") Sys.Application.notifyScriptLoaded();


