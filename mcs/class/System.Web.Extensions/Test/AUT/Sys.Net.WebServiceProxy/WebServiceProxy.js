// WebServiceProxy.js

var webMethod;
var webServicePath;

// This function shows how to use the 
// WebServiceProxy.invoke method without passing
// parameters.
function GetTime()
{
    Sys.Net.WebServiceProxy.invoke(webServicePath, 
        webMethod, false,{}, OnSucceeded, 
        OnFailed,"User Context",30000);
            
}

// This function shows how to use the 
// invoke method passing
// parameters and using the GET verb.
// The dictionary field names must match the 
// related Web service method parameter names.
function GetGreetings() 
{
    Sys.Net.WebServiceProxy.invoke(webServicePath, 
        webMethod, true,
        {"greeting":"Have a nice day", "name":" to You (via GET)!"},
        OnSucceeded,OnFailed, "User Context",30000);

}

// This function shows how to use the 
// invoke method passing parameters and using the POST verb.
// The dictionary field names must match the 
// related Web service method parameter names.
function PostGreetings() 
{
    Sys.Net.WebServiceProxy.invoke(webServicePath, 
        webMethod, false,
        {"greeting":"Have a nice day", "name":" to You (via POST)!"},
        OnSucceeded,OnFailed, "User Context",30000);

}

// This is the callback function invoked 
// if the Web service succeeded.
function OnSucceeded(result, eventArgs)
{
  
    // Display the result.
    var RsltElem = 
        document.getElementById("ResultId");
    RsltElem.innerHTML = result;
  
}


// This is the callback function invoked 
// if the Web service failed.
function OnFailed(error)
{
    // Display the error.    
    var RsltElem = 
        document.getElementById("ResultId");
    RsltElem.innerHTML = 
    "Service Error: " + error.get_message();
}



// This function process the user's selection.
function OnSelectMethod()  
{
    // Get the user's selected method.
    var selectionIndex = 
        document.getElementById("SelectionId").selectedIndex;
    webMethod = 
        document.getElementById("SelectionId").options[selectionIndex].text;

    // Get the related Web service path.
    webServicePath = 
        document.getElementById("SelectionId").value;
 

   // Call selected Web service method.
   switch (webMethod)
   {
    case "GetServerTime":
        GetTime();
   
        break;
        
    case "GetGreetings":
        GetGreetings();
        break;
        
    case "PostGreetings":
        PostGreetings();
        break;
        
    default:
        alert("default");
   }
}

if (typeof(Sys) !== "undefined") Sys.Application.notifyScriptLoaded();
