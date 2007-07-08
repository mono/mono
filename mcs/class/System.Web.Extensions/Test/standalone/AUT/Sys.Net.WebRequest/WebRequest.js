// WebRequest.js

var getPage;
var postPage;
var displayElement;

function pageLoad()
{
    getPage = "getTarget.htm";
    postPage = "postTarget.aspx";
    displayElement = $get("ResultId");
}

// This function performs a GET Web request 
// to retrieve  information from the Url specified in 
// the query string. 
function GetWebRequest()
{
    alert("Performing Get Web request.");
    
    // Instantiate the WebRequest object.
    var wRequest =  new Sys.Net.WebRequest();
    
    // Set the request Url.  
    wRequest.set_url(getPage);  
    
    // Set the request verb.
    wRequest.set_httpVerb("GET");
          
    // Set user's context
    wRequest.set_userContext("user's context");
            
    // Set the web request completed event handler,
    // for processing return data.
    wRequest.add_completed(OnWebRequestCompleted);
       
      
    // Clear the results page element.
    displayElement.innerHTML = "";
    
    // Execute the request.
    wRequest.invoke();  
       
}

// This function performs a POST Web request
// to upload information to the resource 
// identified by the Url. 
function PostWebRequest()
{
    // Instantiate the WebRequest object.
    var wRequest =  new Sys.Net.WebRequest();

    // Set the request Url.  
    wRequest.set_url(postPage); 
     
    // Set the request verb.
    wRequest.set_httpVerb("POST");
    
    var body = "Message=Hello! Do you hear me?"
    wRequest.set_body(body);
    wRequest.get_headers()["Content-Length"] = body.length;
   
     
    // Set the web request completed event handler,
    // for processing return data.
    wRequest.add_completed(OnWebRequestCompleted);
       
    // Clear the results page element.
    displayElement.innerHTML = "";
      
    // Execute the request.
    wRequest.invoke();  
}

// This function adds and removes the 
// Web request completed event handler.
function WebRequestCompleted()
{
    // Instantiate the WebRequest.
    var wRequest =  new Sys.Net.WebRequest();
  
    // Set the request Url.  
    wRequest.set_url(getPage);  
           
    // Set the web request completed event handler,
    // for processing return data.
    wRequest.add_completed(OnWebRequestCompleted);   
    alert("Added Web request completed handler");
 
    // Remove the web request completed event handler.
    // Comment the following two lines if you want to
    // use the handler.
    wRequest.remove_completed(OnWebRequestCompleted); 
    alert("Removed handler; the Web request return is not processed.");
    
    // Execute the request.
    wRequest.invoke();  
}

// This function gets the resolved Url 
// of the Web request instance.
function GetWebRequestResolvedUrl()
{
    // Instantiate the WebRequest.
    var wRequest =  new Sys.Net.WebRequest();
    
    // Set the request Url.  
    wRequest.set_url(getPage);  
           
    // Get the web request completed event handler.
    var resUrl = wRequest.getResolvedUrl();   
    alert("Resolved Url: " + resUrl);
   
    // Set the web request completed event handler,
    // for processing return data.
    wRequest.add_completed(OnWebRequestCompleted); 
    
    // Execute the request.
    wRequest.invoke();  
  
}


// This function gets and sets the 
// Web request time out.
function WebRequestTimeout()
{    
    // Instantiate the WebRequest.
    var wRequest =  new Sys.Net.WebRequest();
    
    // Set the request Url.  
    wRequest.set_url(getPage);  
           
    var defaultTimeout =  
        wRequest.get_timeout();
        
    // Set request timeout to 100 msec.
    wRequest.set_timeout(100);
    
    var newTimeout = 
        wRequest.get_timeout();
    
    alert("Default timeout: " + defaultTimeout);
    alert("New timeout: " + newTimeout);
     
    // Set the web request completed event handler,
    // for processing return data.
    wRequest.add_completed(OnWebRequestCompleted);   
    
    // Execute the request.
    wRequest.invoke();     
}


// This function sets the Web request
// executor, replacing the default one.
function WebRequestExecutor()
{    
    // Instantiate the WebRequest.
    var wRequest =  new Sys.Net.WebRequest();
    
    // Create the executor. In this case it is an
    // XMLHttpExecutor, equivalent to the default
    // executor. But, you can create a custom one.
    var executor = new Sys.Net.XMLHttpExecutor();
   
    // Set the executor, replacing the default one. 
    // In this case the executor is equivalent to the
    // default one.
    wRequest.set_executor(executor); 
    
    // Get the current executor       
    var executor =  
        wRequest.get_executor();
        
    alert("Response availabe: " + executor.get_responseAvailable())
}

 // This function sets an HTTP header for
 // the Web request.
 function WebRequestHeader() 
 {
       // Instantiate the WebRequest object.
    var wRequest =  new Sys.Net.WebRequest();
    
    // Set the request Url.  
    wRequest.set_url(postPage); 
    
    // Set the request verb.
    wRequest.set_httpVerb("POST");
   
    var body = "Message=Hello! Do you hear me?"
    wRequest.set_body(body);
    
    // Set the value of the HTTP header's "Content-Length".
    wRequest.get_headers()["Content-Length"] = body.length;
   
    // Set the web request completed event handler,
    // for processing return data.
    wRequest.add_completed(OnWebRequestCompletedHeader);
       
    // Clear the results page element.
    displayElement.innerHTML = "";
      
    // Execute the request.
    wRequest.invoke();  
}

// This the handler for the Web request completed event
// that is used to display return data.
function OnWebRequestCompleted(executor, eventArgs) 
{
    if(executor.get_responseAvailable()) 
    {
        
        // Clear the previous results. 
        displayElement.innerHTML = "";
   
        // Display Web request status.                  
        DisplayWebRequestStatus(executor);
  
        // Display Web request headers.                  
        DisplayWebRequestHeaders(executor);
        
        // Display Web request body.                  
        DisplayWebRequestBody(executor);
    
    }
    else
    {
        if (executor.get_timedOut())
            alert("Timed Out");
        else
            if (executor.get_aborted())
                alert("Aborted");
    }
}


// This the handler for the Web request completed event
// that is used to display header information.
function OnWebRequestCompletedHeader(executor, eventArgs) 
{
    if(executor.get_responseAvailable()) 
    {
        
        // Clear the previous results. 
         displayElement.innerHTML = "";
  
        // Display Web request headers.                  
        DisplayWebRequestHeaders(executor);
        
    }
    else
    {
    
        if (executor.get_timedOut())
            alert("Timed Out");
       
        else
       
            if (executor.get_aborted())
                alert("Aborted");
       
    }
}
 
// This function is used to display the Web request status.
function DisplayWebRequestStatus(executor)
{
     displayElement.innerHTML +=
     "Status: [" + 
     executor.get_statusCode() + " " + 
     executor.get_statusText() + "]" + "<br/>"
}

// This function is used to display Web request HTTP headers.
function DisplayWebRequestHeaders(executor)
{
    displayElement.innerHTML += 
        "Headers: ";
    displayElement.innerHTML += 
        executor.getAllResponseHeaders() + "<br/>";
 }

// This function is used to display the Web request body.
function DisplayWebRequestBody(executor)
{   
     displayElement.innerHTML += 
        "Body: ";
    if (document.all)
         displayElement.innerText += 
            executor.get_responseData();
    else
        // Firefox
         displayElement.textContent += 
            executor.get_responseData();
}

// This function is used to display the Web request message.
function DisplayInformation(message)
{
    // Clear the previous results.
     displayElement.innerHTML = "";
    // Display information.
    displayElement.innerHTML = "<br/>" + message;
}
   
if (typeof(Sys) !== "undefined") Sys.Application.notifyScriptLoaded();





