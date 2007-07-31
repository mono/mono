// XmlHttpExecutor.js

var resultElementId;

function pageLoad()
{
    resultElementId = $get("ResultId");
}

// This function aborts a Web request.
function AbortWebRequest()
{
    // Create the WebRequest object.
    wRequest =  new Sys.Net.WebRequest();
    
    // Set the request Url.  
    wRequest.set_url("getTarget.aspx");
    
    // Clear the results area.
    resultElementId.innerHTML = "";
    
    // Set the Completed event handler, 
    // for processing return data
    wRequest.add_completed(OnCompleted);
   
    // Make the request.
    wRequest.invoke();
    
    // Get the current executor.
    var executor = wRequest.get_executor();
     
    // Abort the request.
    executor.abort();
  
    // Check if the executor is aborted.
    var execAborted = 
        executor.get_aborted();
  
    //alert("Executor aborted: " + execAborted);  
    $get("alert2").value = "Executor aborted: " + execAborted;
}

// This function executes a Web request.
function ExecuteWebRequest()
{
    // Create the WebRequest object.
    wRequest =  new Sys.Net.WebRequest();

    // Set the request Url.  
    wRequest.set_url("getTarget.htm");
  
    
    // Set the Completed event handler 
    // for processing return data
    wRequest.add_completed(OnCompleted);
    
      // Clear the results area.
    resultElementId.innerHTML = "";

  
    // To use executeRequest you must instantiate the
    // executor, assign it to the Web request instance,
    // then call the executeRequest function.
    // Note: Normally to make a Web request you use
    // the invoke method of the WebRequest instance.
    var executor = new Sys.Net.XMLHttpExecutor();
    wRequest.set_executor(executor); 
    executor.executeRequest();
    
    var started = executor.get_started();
  
    //alert("Executor started: " + started);
    $get("alert2").value = "Executor started: " + started;
}


// This is the event handler called after 
// the Web request returns.
function OnCompleted(executor, eventArgs) 
{
   
    if(executor.get_responseAvailable()) 
   
    {

        // Get the Web request instance.
        var webReq = executor.get_webRequest();
        // Display request Url.
        //alert(webReq.get_url());
        $get("alert1").value = webReq.get_url();

       // Clear the previous results. 
       resultElementId.innerHTML = "";

     
       // Display the Web request status. 
       resultElementId.innerHTML +=
          "Request Status: [" + executor.get_statusCode() + " " + 
                    executor.get_statusText() + "]" + "<br/>";
     
        // Display the Web request headers.
        resultElementId.innerHTML += "Headers: <br/>";
        
        
       
        // Get all the headers.    
        resultElementId.innerHTML += 
        "All Request Headers: " +
            executor.getAllResponseHeaders() + "<br/>"; 
       
        // Get a specific header.
        resultElementId.innerHTML += 
        "Content-Type Header: " +
            executor.getResponseHeader("Content-Type") + 
            "<br/>";       
       
        // Display Web request body.
        resultElementId.innerHTML += "Body: <br/>";
        
        if (document.all)
            resultElementId.innerText += 
                executor.get_responseData();
        else
            // Firefox 
            resultElementId.textContent += 
                executor.get_responseData();
   
    }
    else
    {
        if (executor.get_timedOut())
            alert("Timed Out");
        else
            if (executor.get_aborted()) {
                //alert("Aborted");
                $get("alert1").value = "Aborted";
			}
    }

}


// This is the event handler called after 
// the Web request returns. It is designed
// for Web requests that return XML.
function OnSucceededXml(executor, eventArgs) 
{
    if (executor.get_responseAvailable()) 
    {
       
        if (document.all)
            resultElementId.innerText += "First node: " + 
                executor.get_xml().documentElement.nodeName;
        else
            // Firefox 
            resultElementId.textContent += "First node: " + 
                executor.get_xml().documentElement.nodeName;

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

// This function executes a Web request
// to get XML data.
function GetXml()
{
    // Create the WebRequest object.
    wRequest =  new Sys.Net.WebRequest();

    // Set the request Url.  
    wRequest.set_url("getTarget.xml");
  
    // Set the Completed event handler 
    // for processing return data.
    wRequest.add_completed(OnSucceededXml);
    
    // Clear the results area.
   if (document.all)
        resultElementId.innerText = "";
    else
        // Firefox 
        resultElementId.textContent = "";
   
    // Invoke the Web request.
    wRequest.invoke();
}

