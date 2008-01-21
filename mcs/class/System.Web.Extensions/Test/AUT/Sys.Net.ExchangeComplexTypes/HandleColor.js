// HandlerColor.js
 
 // The Web service default color.
 var defaultRgb;
 
 // The page feedback display element.
 var displayResult;
 
// Gets the selection list colors and 
// the default color from the Web service.
function GetServerColors()  
{
  // Gets the default color.
    Sys.Net.ExchangeComplexTypes.HandleColor.GetDefaultColor(
        SucceededCallback, FailedCallback);  
        
    // Get selection list colors.
    Sys.Net.ExchangeComplexTypes.HandleColor.GetColorList(
        SucceededCallback, FailedCallback);
        
      
}

// This function passes the color selected
// by the user (client) to the Web service.
function OnChangeDefaultColor(comboObject)  
{
    // Create an instance 
    // of the server color object.
    var color = 
        new Sys.Net.ExchangeComplexTypes.ColorObject();

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
    Sys.Net.ExchangeComplexTypes.HandleColor.ChangeDefaultColor(
        color, SucceededCallback, FailedCallback);  
}

// This is the callback function that processes 
// the complex type returned by the Web service.
function SucceededCallback(result, userContext, methodName)
{ 
    switch(methodName)
    {
        case ("GetColorList"):
        {
            // Get the select object.
            var selectObject = document.getElementById("ColorSelectID");
            var i = 0;
      
            // Iterate through the dictionary to populate 
            // the selection list.
            for(var item in result)
            {        
                var option = new Option(result[item], item);
                selectObject.options[i]=option;
   
                // Set the default selection.        
                if (item == defaultRgb)
                    selectObject.options[i].selected = true;
               i++;
            }
            $get("ok").value="ok";
            break;
        }
        default:
        {
            // Get the server default color and its current time.
            // Read the values returned by the
            // Web service.
            var message = result.message;
            defaultRgb = result.rgb;
           
            var timeStamp = result.timeStamp;
            
            // Transform the rgb array into a string.
            var serverColor = defaultRgb[0]+ defaultRgb[1] + defaultRgb[2];

            // Display the result.
            displayResult.style.color = "yellow";
            displayResult.style.fontWeight = "bold";
            if (document.all) 
                displayResult.innerText = message + " " + timeStamp;
            else
               // Firefox
               displayResult.textContent = message + " " + timeStamp;
               
            displayResult.style.backgroundColor = "#" + serverColor;
            break;
        }
    }            
}
    
// Callback function invoked on failure 
// of the Web service methods.
function FailedCallback(error, userContext, methodName) 
{
    if(error !== null) 
    {
        displayResult.innerHTML = "An error occurred: " + 
            error.get_message();
    }
}

// Gets the Web service selection list colors 
// and the default color.
function pageLoad() 
{
    // Get page feedback display element.
    displayResult = 
        document.getElementById("ResultId");
    // Get the server's selection list colors and 
    // the default color.
    GetServerColors();
}
   
    
if (typeof(Sys) !== "undefined") Sys.Application.notifyScriptLoaded();
