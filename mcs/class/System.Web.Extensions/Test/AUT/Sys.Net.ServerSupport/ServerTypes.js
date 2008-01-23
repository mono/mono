// ServerTypes.js

// The Web service default color.
var defaultRgb;

// The page feedback display element.
var displayResult;

// Gets the Web service selection list colors 
// and the default color.
function pageLoad() 
{
    // Get page feedback display element.
    displayResult = 
        document.getElementById("ResultId");
    
    // Assign default values for the generated proxy using
    // the (generated) static proxy.
    //Samples.AspNet.ServerTypes.set_timeout(200);
    Samples.AspNet.ServerTypes.set_defaultUserContext("Default context");
    Samples.AspNet.ServerTypes.set_defaultSucceededCallback(SucceededCallback);
    Samples.AspNet.ServerTypes.set_defaultFailedCallback(FailedCallback);
    
}


// This function shows how to get an 
// enumeration object from the server.
function GetFirstEnumElement()
{
    // Get the first element of the enumeration
    Samples.AspNet.ServerTypes.GetFirstColor();
}

// This function shows how to pass an 
// enumeration value to the server.
function GetSelectedEnumValue()
{
   // Get the value of the selected enumerated
   // element.
   Samples.AspNet.ServerTypes.GetSelectedColor(
    Samples.AspNet.ColorEnum.Blue);
}

// Callback function invoked when the call to 
// the Web service methods succeeds.
function SucceededCallback(result, userContext, methodName)
{ 
    var message;
    switch(methodName)
    {
        case ("GetFirstColor"):
        {
            var firstColor = result;
            message = "First enumerated value: " + firstColor;
            DisplayMessage(message);
            break;
        }
        
        case ("GetSelectedColor"):
        {
            var selectedColor = result;
            message = "Selected enumerated value: " + selectedColor;
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
