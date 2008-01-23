// UsingProxyClass.js

// The Web service default color.
var defaultRgb;

// The proxy class instance.
var proxyInstance;

// The page feedback display element.
var displayResult;

// This function intializes the global variables and
// assigns default values to the generated proxy.
function pageLoad() 
{
    // Get page feedback display element.
    displayResult = 
        document.getElementById("ResultId");
    
    // Assign default values to the generated proxy.
    //Sys.Net.UsingProxyClass.UsingProxyClass.set_timeout(200);
    Sys.Net.UsingProxyClass.UsingProxyClass.set_defaultUserContext("Default context");
    Sys.Net.UsingProxyClass.UsingProxyClass.set_defaultSucceededCallback(SucceededCallback);
    Sys.Net.UsingProxyClass.UsingProxyClass.set_defaultFailedCallback(FailedCallback);
}

// This function shows how to get 
// a server object.
function GetDefaultColor()
{
    // Gets the default color obiect.
    Sys.Net.UsingProxyClass.UsingProxyClass.GetDefaultColor();  
        
}

// This function shows how to instantiate
// the proxy class to assign its default values.
function SetColor()
{
    // Instantiate a color object.
    var color = 
        new Sys.Net.UsingProxyClass.ColorObject();

    // Define a color array (blue).
    var colorArray = new Array("00", "00", "FF");

    // Assign the new values to the server color object.
    color.message = "The new color is Blue";
    color.rgb = colorArray;
    
   
    // Assign default values for the generated proxy using
    // a proxy instance.
    proxyInstance = new  Sys.Net.UsingProxyClass.UsingProxyClass();
    proxyInstance.set_timeout(1000);
    proxyInstance.set_defaultUserContext("New context");
    proxyInstance.set_defaultSucceededCallback(SucceededCallback);
    proxyInstance.set_defaultFailedCallback(FailedCallback);
  
    // Set the default color object.
    proxyInstance.SetColor(color);  
}

// Callback function invoked when the call to 
// the Web service methods succeeds.
function SucceededCallback(result, userContext, methodName)
{ 
    var message;
    switch(methodName)
    {
        case ("GetDefaultColor"):
        case ("SetColor"):
        {
            // Get the server default color.
            message = result.message;
            defaultRgb = result.rgb;
        
            
            // Transform the rgb array into a string.
            var serverColor = defaultRgb[0]+ defaultRgb[1] + defaultRgb[2];
            
            // Display the result.
            displayResult.style.color = "yellow";
            displayResult.style.fontWeight = "bold";
            displayResult.style.backgroundColor = "#" + serverColor;
            DisplayMessage(message);
            break;
        }
        default:
        {
            DisplayMessage("Method unknown");
        }
    }       
}

// Callback function invoked when the call to 
// the Web service methods fails.
function FailedCallback(error, userContext, methodName) 
{
    if(error !== null) 
    {
        displayResult.innerHTML = "An error occurred: " + 
            error.get_message();
    }
}

function DisplayMessage(message)
{
    if (document.all) 
        displayResult.innerText = message;
    else
        // Firefox
        displayResult.textContent = message;    
}

if (typeof(Sys) !== "undefined") Sys.Application.notifyScriptLoaded();
