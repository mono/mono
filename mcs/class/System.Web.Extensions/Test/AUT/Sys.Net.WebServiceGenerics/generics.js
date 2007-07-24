
// generics.js

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
    Samples.AspNet.TestService.set_defaultUserContext("Default context");
    Samples.AspNet.TestService.set_defaultSucceededCallback(SucceededCallback);
    Samples.AspNet.TestService.set_defaultFailedCallback(FailedCallback);
 }
 
 // Get a generic List.
 function GenericList() 
 {	   
    Samples.AspNet.TestService.GetGenericList(); 
 }
 
 // Get a generic Dictionary.
 function GenericDictionary() 
 {	   
    Samples.AspNet.TestService.GetGenericDictionary(); 
 }
 
 // Get a generic Dictionary of custom types. 
 function GenericCustomTypeDictionary() 
 {	   
    Samples.AspNet.TestService.GetGenericCustomTypeDictionary(); 
 }
 
 
 // Pass a generic dictionary of custom types
 // to Webservice.
 function PassGenericDictionary() 
 {	   
  
    var simple = new Samples.AspNet.SimpleClass2();
    simple.s = "WebService proxy.";
  
    Samples.AspNet.TestService.PassGenericDictionary(
    {"first":simple}); 
 }
 
 // Get an Array.
 function ArrayType() 
 {	   
    Samples.AspNet.TestService.GetArray(); 
 }
 
 
// Callback function invoked when the call to 
// the Web service methods succeeds. 
function SucceededCallback(result, userContext, methodName)
{ 
    var message;
    switch(methodName)
    {
        case ("GetGenericList"):
        {
            var i = 0;
            var message = new Array();
            for(var item in result)
            {    
               message[i] = "List element " + i + ": " + result[item].s;
               i++;
            }
            DisplayMessage(message.toString());
            break;
        }
        
        
        case ("GetGenericDictionary"):
        {
            var i = 0;
            var message = new Array();
            for(var item in result)
            {    
               message[i] = item + ": " + result[item];
               i++;
            }
            DisplayMessage(message.toString());
            break;
        }
        
        case ("GetGenericCustomTypeDictionary"):
        {
            var i = 0;
            var message = new Array();
            for(var item in result)
            {    
               message[i] = item + ": " + result[item].s;
               i++;
            }
            DisplayMessage(message.toString());
            break;
        }
        
        case ("PassGenericDictionary"):
        {
            
            DisplayMessage(result);
          
            break;
        }
        
        case ("GetArray"):
        {
            var i = 0;
            var message = new Array();
            for(var item in result)
            {    
               message[i] = result[item];
               i++;
            }
            DisplayMessage(message.toString());
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
