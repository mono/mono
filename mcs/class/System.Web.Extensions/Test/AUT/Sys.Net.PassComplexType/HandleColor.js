// HandlerColor.js

// It gets the default color
// from the Web service.
function OnDefaultColor()  
{
    // Call the Web service method to get 
    // the default color.
    Sys.Net.PassComplexType.HandleColor.GetDefaultColor(
        OnSucceeded);  
     
}

// This function passes the color selected
// by the user to the Web service.
function OnChangeDefaultColor(comboObject)  
{

    // Create an instance 
    // of the server color object.
    var color = 
        new Sys.Net.PassComplexType.ColorObject();

    // Get the user's selected color.
    var selectionIndex = 
        comboObject.selectedIndex;
    var selectedColor = 
        comboObject.options[selectionIndex].text;    

    // Get the related RGB color value.
    var selectionValue = 
        comboObject.value;
    // Transform it into an array.
    var colorArray = selectionValue.split(",");

    // Assign the new values to 
    // the server color object.
    color.message = "The new default color is " + selectedColor + ".";
    color.rgb = colorArray;

    // Call the Web service method to change the color.
    Sys.Net.PassComplexType.HandleColor.ChangeDefaultColor(
        color, OnSucceeded);  
}

// This is the callback function that 
// processes the complex type returned  
// by the Web service.
function OnSucceeded(result)
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
    
// This function is called at page load.
// It obtains the Web service default
// color.
function pageLoad() 
{
    // Get the Web service default color.
    OnDefaultColor();
}
   
    
if (typeof(Sys) !== "undefined") Sys.Application.notifyScriptLoaded();
