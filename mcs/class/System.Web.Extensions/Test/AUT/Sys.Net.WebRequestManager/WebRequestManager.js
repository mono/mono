// WebRequestManager.js

var displayElement;

function pageLoad()
{
    displayElement = $get("ResultId");
}

// Adds invokingRequest and completedRequest
// handlers, and performs a Web request. 
function MakeWebRequest() 
{
    // Clear the previous results. 
    displayElement.innerHTML = "";
    
    // Instantiate a Web request.
    wRequest =  new Sys.Net.WebRequest();

    // Set the handler to process the Web request.
    Sys.Net.WebRequestManager.add_completedRequest(On_WebRequestCompleted);

    alert("Added On_WebRequestCompleted handler.");
   
    // Set the handler to call before the Web request
    // is executed.
    Sys.Net.WebRequestManager.add_invokingRequest(On_InvokingRequest);   

    
    alert("Added On_InvokingRequest handler.");
      
    // Set the request Url.  
    wRequest.set_url("getTarget.htm");
   
    // Execute the request.
    // Notice that you do not use the executeRequest method of
    // the WebRequestManager which is intended for internal 
    // use only as in: Sys.Net.WebRequestManager.executeRequest(wRequest).
    // The correct way to execute a request is the following:
    // wRequest.invoke();

    Sys.Net.WebRequestManager.executeRequest(wRequest);
}

// Removes the event handlers that were previusly added. 
function RemoveDefaultHandlers() 
{
    // Clear the previous results. 
    displayElement.innerHTML = "";
    

    Sys.Net.WebRequestManager.remove_completedRequest(On_WebRequestCompleted);

    
    alert("Removed On_WebRequestCompleted handler.");
    
    Sys.Net.WebRequestManager.remove_invokingRequest(On_InvokingRequest); 
  
    alert("Removed On_InvokingRequest handler.");
}


// Gets and sets the default executor.
function DefaultExecutor()
{
    // Clear the previous results. 
    displayElement.innerHTML = "";
    
    // Get system default executor type.
    var sysDefaultExecutor = 
        Sys.Net.WebRequestManager.get_defaultExecutorType();
    alert("Get default executor:" + sysDefaultExecutor);
    
    
    // Modify the default executor type.
    Sys.Net.WebRequestManager.set_defaultExecutorType(
    "Sys.Net.CustomExecutor");
    
    var customDefaultExecutor = 
        Sys.Net.WebRequestManager.get_defaultExecutorType();
        
    alert("Set default executor: " + customDefaultExecutor);
    
    // Set the executor back to the system default. This is 
    // to allow the WebRequest script to run.
    executor = "Sys.Net.XMLHttpExecutor";
    Sys.Net.WebRequestManager.set_defaultExecutorType(
    sysDefaultExecutor);    
} 

// Gets and sets the default timeout.
function DefaultTimeout()
{
    // Clear the previous results. 
    displayElement.innerHTML = "";

    // Get system default timeout.
    var sysDefaultTimeout = 
        Sys.Net.WebRequestManager.get_defaultTimeout();
    
    alert("Get default timeout: " + sysDefaultTimeout);
    
    
    // Set custom default timeout.
    Sys.Net.WebRequestManager.set_defaultTimeout(100);
    
    var customDefaultTimeout = 
        Sys.Net.WebRequestManager.get_defaultTimeout();
        
    alert("Set default timeout: " + customDefaultTimeout);
    
   
    // Set the timeout back to the system default. 
    Sys.Net.WebRequestManager.set_defaultTimeout(
    sysDefaultTimeout);    
}

// The On_InvokingRequest can be used to perform
// processing prior to the Web request being executed. 
function On_InvokingRequest(executor, eventArgs)
{
    alert("Executing OnInvokingRequest handler, before the Web request.");
    
    // Add custom code to perform processing prior
    // to the request being executed or to abort the 
    // request.
    alert("The current executor is: " + 
        executor.get_defaultExecutorType());
    
    // Use the eventArgs of type
    // NetworkRequestEventArgs to access the 
    // current WebRequest instance.
    var currentRequest = eventArgs.get_webRequest();
    var requestUrl = currentRequest.getResolvedUrl();           
    alert("Current request URL: " + requestUrl);
}

// The On_WebRequestComplete occurs after the
// Web request has returned, and can be used to
// get error status, process returned data, etc...
function On_WebRequestCompleted(executor, eventArgs) 
{
    if(executor.get_responseAvailable()) 
    {

        // Clear the previous results. 
        displayElement.innerHTML = "";
        
        // Display Web request status.                 
        displayElement.innerHTML  +=
          "Status: [" + executor.get_statusCode() + " " + 
                    executor.get_statusText() + "]" + "<br/>";
        
        // Display Web request headers.
        displayElement.innerHTML  += 
            "Headers: ";
        displayElement.innerHTML  += 
            executor.getAllResponseHeaders() + "<br/>";

        // Display Web request body.
        displayElement.innerHTML += 
            "Body: ";
       
        displayElement.innerHTML  += 
           executor.get_responseData();
    }
}

if (typeof(Sys) !== "undefined") Sys.Application.notifyScriptLoaded();



