// ConnectingEndPoints.js

var resultElement;

function pageLoad()
{
    resultElement = $get("ResultId");
}

// This function performs a GET Web request.
function GetWebRequest()
{
    alert("Performing Get Web request.");

    // Instantiate a WebRequest.
    var wRequest = new Sys.Net.WebRequest();
    
    // Set the request URL.      
    wRequest.set_url("getTarget.htm");
    alert("Target Url: getTarget.htm");

    // Set the request verb.
    wRequest.set_httpVerb("GET");
           
    // Set the request callback function.
    wRequest.add_completed(OnWebRequestCompleted);
 
    // Clear the results area.
    resultElement.innerHTML = "";

    // Execute the request.
    wRequest.invoke();  
}

// This function performs a POST Web request.
function PostWebRequest()
{
    alert("Performing Post Web request.");
    
    // Instantiate a WebRequest.
    var wRequest = new Sys.Net.WebRequest();
    
    // Set the request URL.      
    wRequest.set_url("postTarget.aspx");
    alert("Target Url: postTarget.aspx");

    // Set the request verb.
    wRequest.set_httpVerb("POST");
   
    // Set the request handler.
    wRequest.add_completed(OnWebRequestCompleted);
 
    // Set the body for he POST.
    var requestBody = 
        "Message=Hello! Do you hear me?";
    wRequest.set_body(requestBody);
    wRequest.get_headers()["Content-Length"] = 
        requestBody.length;
   
    // Clear the results area.
   resultElement.innerHTML = "";
     
    // Execute the request.
    wRequest.invoke();              
}


// This callback function processes the 
// request return values. It is called asynchronously 
// by the current executor.
function OnWebRequestCompleted(executor, eventArgs) 
{    
    if(executor.get_responseAvailable()) 
    {
        // Clear the previous results. 
       
       resultElement.innerHTML = "";
   
        // Display Web request status. 
       resultElement.innerHTML +=
          "Status: [" + executor.get_statusCode() + " " + 
                    executor.get_statusText() + "]" + "<br/>";
        
        // Display Web request headers.
       resultElement.innerHTML += 
            "Headers: ";
            
       resultElement.innerHTML += 
            executor.getAllResponseHeaders() + "<br/>";

        // Display Web request body.
       resultElement.innerHTML += 
            "Body:";
           
      if(document.all)
        resultElement.innerText += 
           executor.get_responseData();
      else
        resultElement.textContent += 
           executor.get_responseData();
    }

}
if (typeof(Sys) !== "undefined") Sys.Application.notifyScriptLoaded();
