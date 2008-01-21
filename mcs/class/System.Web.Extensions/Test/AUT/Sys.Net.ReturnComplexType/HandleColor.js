/* 
 * Client script *
 */

// It gets the default color
// from the Web service.
function GetDefaultColor()  
{
    // Call the Web service method to get 
    // the default color.
    Sys.Net.ReturnComplexType.HandleColor.GetDefaultColor(
        SucceededCallback);  
     
}

// This is the callback function that 
// processes the complex type returned  
// by the Web service.
function SucceededCallback(result)
{
   
    // Read the values returned by the
    // Web service.
    var message = result.message;
    var rgb = result.rgb;
    var timeStamp = result.timeStamp;
    
    // Transform the rgb array into a string.
    var serverColor = rgb[0]+ rgb[1] + rgb[2];

    // Display the result.
    var displayResult = 
        document.getElementById("ResultId");
    displayResult.style.color = "yellow";
    displayResult.style.fontWeight = "bold";
    if (document.all) 
        displayResult.innerText = message + " " + timeStamp;
	else
	   // Firefox
	   displayResult.textContent = message + " " + timeStamp;
    displayResult.style.backgroundColor = "#" + serverColor;

}

if (typeof(Sys) !== "undefined") Sys.Application.notifyScriptLoaded();
