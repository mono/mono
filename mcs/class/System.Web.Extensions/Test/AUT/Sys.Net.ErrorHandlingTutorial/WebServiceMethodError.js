// WebServiceMethodError.js

// This function can cause a divide by zero error.  
function Div(a, b)
{
    Sys.Net.ErrorHandlingTutorial.WebService.Div(a, b, 
        SucceededCallback, FailedCallback);
}

// This is the failed callback function.
function FailedCallback(error)
{
    var stackTrace = error.get_stackTrace();
    var message = error.get_message();
    var statusCode = error.get_statusCode();
    var exceptionType = error.get_exceptionType();
    var timedout = error.get_timedOut();
   
    // Display the error.    
    var RsltElem = 
        document.getElementById("Results");
    RsltElem.innerHTML = 
        "Stack Trace: " +  stackTrace + "<br/>" +
        "Service Error: " + message + "<br/>" +
        "Status Code: " + statusCode + "<br/>" +
        "Exception Type: " + exceptionType + "<br/>" +
        "Timedout: " + timedout;
}

// This is the succeeded callback function.
function SucceededCallback(result)
{
    // Display the result.
    var RsltElem = document.getElementById("Results");
    RsltElem.innerHTML = result;
}

if (typeof(Sys) !== "undefined") Sys.Application.notifyScriptLoaded();
