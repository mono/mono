
<%@ Page Language="C#"  AutoEventWireup="true"%>

<%@ Import Namespace="System.Web.Script.Serialization" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">


<html xmlns="http://www.w3.org/1999/xhtml" >
    <head id="Head1" runat="server">
        
        <title>Client Serialization Server Deserialization</title>
        <style type="text/css" >
            body 
            {
                font-family: Verdana, Arial, Helvetica, sans-serif;
                font-size: 80%;
                width: 100%;
            }
        </style>
        
        
        <!-- Client script performing serialization -->
        <script type="text/JavaScript">
        
            // Color object constructor.
            // It is also the class definittion for the object.
            function color() {
                // Define string property.
                this.message="client default color is Black";
                // Define an array property.
                this.rgb=['00', '00', '00'];
                
                // Define the a class member.
                this.setColor = setColor;
            }


            // It changes the color value stored in the object.
            function setColor(message, rgbValue)
            {
                // Build an array from the comma 
                // separated string parameter.
                var rgbValueArray = rgbValue.split(",");
                // Assign the message.
                this.message=message;
                // Assign the rgb value.
                this.rgb=rgbValueArray;
            }


            function OnClick_ClientSerialize()  
            {
               
                // Create the color object to use 
                // for client serialization.
                var jsonObject = new color();
             
                // Serialize the object to obtain 
                // the related JSON string.
                var jsonString =  
                    Sys.Serialization.JavaScriptSerializer.serialize(jsonObject);
               
                
                if (document.all)
                {
                    // Display the client's serialization.
                    document.getElementById('<%= ClientResultID.ClientID %>').innerText = 
                        jsonString;
                    // Store the serialized object for the server.
                    document.getElementById('<%= InputID.ClientID %>').innerText = 
                        jsonString;
                }
                else
                {    // Firefox
            
                    document.getElementById('<%= ClientResultID.ClientID %>').textContent = 
                        jsonString;
                    // Store the serialized object for the server.
                    document.getElementById('<%= InputID.ClientID %>').value = 
                        jsonString;
                }
                
                
            }
            
             // It changes the color 
            // and stores it for the server.
            function OnColorSelected()
            {
                
                var selectionValue = 
                    document.getElementById("ColorSelectID").value;
               
                var selectionIndex = 
                    document.getElementById("ColorSelectID").selectedIndex;
                var selectedColor = 
                    document.getElementById("ColorSelectID").options[selectionIndex].text;
              
                // Create a color object.
                var myColor = new color();
                
                // Assign the new selected value.
                var message = "client color is " + selectedColor;
                var rgb = selectionValue;
                
                // Assign the selected value to the color object.
                myColor.setColor(message, rgb);
                
                // Serialize the color object to obtain 
                // the related JSON string.
                var jsonString = 
                    Sys.Serialization.JavaScriptSerializer.serialize(myColor);
                
                if (document.all)
                {
                    // Display the client's serialization.
                    document.getElementById('<%= ClientResultID.ClientID %>').innerText = 
                        jsonString;
                    // Store the serialized object for the server.
                    document.getElementById('<%= InputID.ClientID %>').innerText = 
                        jsonString;
                }
                else
                {    // Firefox
             
                    document.getElementById('<%= ClientResultID.ClientID %>').textContent = 
                        jsonString;
                    // Store the serialized object for the server.
                    document.getElementById('<%= InputID.ClientID %>').value = 
                        jsonString;
                }
                
                        
            }

        </script>
        
        <!-- Sever script performing deserialization -->
        <script   runat="server">

             // Define the color object to serialize.
             class ColorObject
             {
                 public string message = "server color";
                 public string[] rgb = 
                     new string[] { "00", "00", "00" };

                 public ColorObject()
                 {
                     this.message = "Server color is black.";
                     this.rgb = new string[] { "00", "00", "00" };
                 }
             }

             
            public void OnClick_ServerDeSerialize(object sender, 
                EventArgs e)
            {

                // Instantiate a serializer.
                System.Web.Script.Serialization.JavaScriptSerializer serializer = 
                new JavaScriptSerializer();
                
                
                // Get the client serialized object (JSON string).
                string clientSerializedObject = InputID.Value;
                ClientResultID.InnerText = InputID.Value;
              
                // Perform deserialization and 
                // get back the color object.
                ColorObject co = 
                    serializer.Deserialize<ColorObject>(clientSerializedObject);

                if (co == null)
                {

                    ServerResultID.InnerText = 
                        "Please, perform client serialization first.";
                    ServerResultID.Style.Value = 
                        "color:red;" + " font-weight:bold;";

                }
                else
                {
                    string rgb = co.rgb[0] + co.rgb[1] + co.rgb[2];
                    ServerResultID.InnerText = co.message + ": " + rgb;
                    ServerResultID.Style.Value = 
                        "color:yellow;" + " font-weight:bold;" +
                        " background-color:#" + rgb;
                }
                
            }
        </script>
           
    </head>
    
    <body>
     
    
        <!-- Add the script manager -->
        <form id="form1" runat="server">
            <asp:ScriptManager runat="server" ID="ScriptManager1"/>
  
        <h2>Client Serialization Server Deserialization</h2>
    
        <h3>Client</h3>
        
        <!-- Perform client serialization -->
        <span class="yellow_marker">Serialize client default color:</span>
        <button id="ClientButtonID" 
            onclick="OnClick_ClientSerialize(); return false;">Default Color</button>
         
        <!-- Store client serialization and preserve it 
        on postback so the server can read it and 
        perform its deserialization -->
        <input 
            id="InputID" 
            type="text"  
            style="width:300px; visibility:hidden" 
            runat="server"/>
        
        <p>
            <span class="yellow_marker">Serialize new client color:</span>
            <select id="ColorSelectID" OnChange="OnColorSelected(); return false; " runat="server" >
                <option value="00,00,00">Black</option>
                <option value="FF,00,00">Red</option>
                <option value="00,FF,00">Green</option>
                <option value="00,00,FF">Blue</option>
            </select>
        </p>
        
        <span 
            style="background-color:Yellow">Client serialization:</span>
            <span id="ClientResultID" runat="server" enableviewstate="true"> </span>
        
       
        
        <hr />
        
        <h3>Server</h3>
           
        <!-- Perform server deserialization -->
        <asp:Button ID="ServerButtonID" OnClick="OnClick_ServerDeSerialize" 
            Text="Server Deserialize" runat="server" />
            
        <p>
            <span style="background-color:Yellow">Server deserialization:</span>
            <span id="ServerResultID" style="background-color:Aqua;" runat="server"/>
        </p>
            
        
        </form>    
    
        <hr />
    

   
    </body>
    
</html>

