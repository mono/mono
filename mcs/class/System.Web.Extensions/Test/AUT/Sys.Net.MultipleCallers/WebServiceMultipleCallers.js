// WebServiceMultipleCallers.js

// This function calls a Web service method 
// passing simple type parameters and the user context.  
function AddWithContext(a,  b, userContext)
{
    Sys.Net.MultipleCallers.WebService.Add(a, b, 
    SucceededCallbackWithContext, FailedCallback, userContext, null);
}


// This function calls the Web service method 
// passing the method name.  
function AddWithMethod(a,  b)
{
    Sys.Net.MultipleCallers.WebService.Add(a, b, 
    SucceededCallbackWithContext, FailedCallback);
}

// This is the callback function called if the
// Web service succeeded. It accepts the result
// object, the user context, and the method name as
// parameters.
function SucceededCallbackWithContext(result, userContext, methodName)
{
    // It holds feedback message.
	var output = "";
	
	// Page element to display the feedback message.    
    var RsltElem = 
        document.getElementById("Results");
    
    if (userContext)
    {
        output += "The user context is : " + userContext + "<br/>";
        RsltElem.innerHTML =  output;
        return;
 	}
   
	if (methodName) 
  	  output += "The method name is : " + methodName + "<br/>";
  	  
  	RsltElem.innerHTML =  output;
 	
}

// This is the callback function called if the
// Web service failed. It accepts the error object
// as a parameter.
function FailedCallback(error)
{
    // Display the error.    
    var RsltElem = 
        document.getElementById("Results");
    RsltElem.innerHTML = 
        "Service Error: " + error.get_message();
}

if (typeof(Sys) !== "undefined") Sys.Application.notifyScriptLoaded();
