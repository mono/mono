// Authentication.js

var usernameEntry;
var passwordEntry;
var username;
var password;
var textLoggedIn;
var textNotLoggedIn;
var buttonLogin;  
var buttonLogout; 

function pageLoad()
{
 	usernameEntry = $get("NameId");
	passwordEntry = $get("PwdId");
	username = $get("username");
	password = $get("password");
	textLoggedIn = $get("loggedin");
	textNotLoggedIn = $get("notloggedin");
	buttonLogin = $get("ButtonLogin");  
	buttonLogout = $get("ButtonLogout"); 
}            

// This function sets and gets the default
// login completed callback function.
function SetDefaultLoginCompletedCallBack()
{
    // Set the default callback function.
    Sys.Services.AuthenticationService.set_defaultLoginCompletedCallback(OnLoginCompleted);
   
    // Get the default callback function.
    var callBack =     
        Sys.Services.AuthenticationService.get_defaultLoginCompletedCallback();
}

// This function sets and gets the default
// logout completed callback function.
function SetDefaultLogoutCompletedCallBack()
{
    // Set the default callback function.
    Sys.Services.AuthenticationService.set_defaultLogoutCompletedCallback(OnLogoutCompleted);
   
    // Get the default callback function.
    var callBack =     
        Sys.Services.AuthenticationService.get_defaultLogoutCompletedCallback();
}

// This function sets and gets the default
// failed callback function.
function SetDefaultFailedCallBack()
{
    // Set the default callback function.
    Sys.Services.AuthenticationService.set_defaultFailedCallback(OnFailed);
   
    // Get the default callback function.
    var callBack =     
        Sys.Services.AuthenticationService.get_defaultFailedCallback();
}

// This function calls the login method of the
// authentication service to verify 
// the credentials entered by the user.
// If the credentials are authenticated, the
// authentication service issues a forms 
// authentication cookie. 
function OnClickLogin() 
{   
    // Set the default callback functions.
    SetDefaultLoginCompletedCallBack();
    SetDefaultLogoutCompletedCallBack();
    SetDefaultFailedCallBack();
   
    // Call the authetication service to authenticate
    // the credentials entered by the user.
    Sys.Services.AuthenticationService.login(username.value, 
        password.value, false,null,null,null,null,"User Context");
}

// This function calls the logout method of the
// authentication service to clear the forms 
// authentication cookie.
function OnClickLogout() 
{  
   // Clear the forms authentication cookie. 
   Sys.Services.AuthenticationService.logout(null, 
        null, null, null); 
} 

// This is the callback function called 
// if the authentication fails.      
function OnFailed(error, 
    userContext, methodName)
{			
    // Display feedback message.
	DisplayInformation("error:message = " + 
	    error.get_message());
	DisplayInformation("error:timedOut = " + 
	    error.get_timedOut());
	DisplayInformation("error:statusCode = " + 
	    error.get_statusCode());			
}


// The callback function called 
// if the authentication completed successfully.
function OnLoginCompleted(validCredentials, 
    userContext, methodName)
{
	
    // Clear the user password.
    password.value = "";
    
    // On success there will be a forms 
    // authentication cookie in the browser.
    if (validCredentials == true) 
    {
        
        // Clear the user name.
        username.value = "";
     
        // Hide login fields.
        buttonLogin.style.visibility = "hidden";
        usernameEntry.style.visibility = "hidden";
        passwordEntry.style.visibility = "hidden";
        textNotLoggedIn.style.visibility = "hidden";  
   
        // Display logout fields.
        buttonLogout.style.visibility = "visible";
        textLoggedIn.style.visibility = "visible";
        
        // Clear the feedback area.
        DisplayInformation(""); 
    }
    else 
    {
        textLoggedIn.style.visibility = "hidden";
        textNotLoggedIn.style.visibility = "visible";
        DisplayInformation(
            "Login Credentials Invalid. Could not login"); 
    }
}

// This is the callback function called 
// if the user logged out successfully.
function OnLogoutCompleted(result) 
{
    // Display login fields.
    usernameEntry.style.visibility = "visible";
    passwordEntry.style.visibility = "visible";
    textNotLoggedIn.style.visibility = "visible";  
    buttonLogin.style.visibility = "visible";
   
    // Hide logout fields.
    buttonLogout.style.visibility = "hidden";
    textLoggedIn.style.visibility = "hidden";
}                   

// This function displays feedback
// information for the user.    
function DisplayInformation(text)
{
    document.getElementById("FeedBackID").innerHTML = 
        "<br/>" + text;

    // Display authentication service information.
    
    
	var userLoggedIn =
	    Sys.Services.AuthenticationService.get_isLoggedIn();
	
    var authServiceTimeout =       
        Sys.Services.AuthenticationService.get_timeout();
   
    var userLoggedInfo = 
        "<br/> User logged in:                 " + userLoggedIn;
        
    var timeOutInfo = 
        "<br/> Authentication service timeout: " + authServiceTimeout;
        
    document.getElementById("FeedBackID").innerHTML = 
        userLoggedInfo + timeOutInfo; 
}

if (typeof(Sys) !== "undefined") Sys.Application.notifyScriptLoaded();
