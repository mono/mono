<%@ Page Language="C#"  AutoEventWireup="true"%>

<%@ Import Namespace="System.Web.Script.Serialization" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">


<html xmlns="http://www.w3.org/1999/xhtml" >
    <head id="Head1" runat="server">
        
        <title>Client Deserialization Server Serialization</title>
        <style type="text/css" >
            body 
            {
                font-family: Verdana, Arial, Helvetica, sans-serif;
                font-size: 80%;
                width: 100%;
            }
        </style>
        
        <!-- Client script performing deserialization -->
        <script type="text/javascript">
            // Color object constructor.
            // It functions also as the class definition for the object.
            function color() {
                // Define string property.
                this.message="client default color is Black";
                // Define an array property.
                this.rgb=['00', '00', '00'];
                // Define the a class member.
            }
             
            function OnClick_ClientDeserialize()  
            {
              
                // Create the color object to use 
                // for client deserialization.
                var jsonObject = new color();
             
                // Get the JSON string (serialized object) 
                // stored by the server.
                //var jsonString =  
               //     document.getElementById('<%= ServerResultID.ClientID %>').innerText;             
                
                var jsonString;
                 
                if (document.all)
                    jsonString = 
                        document.getElementById('<%= ServerResultID.ClientID %>').innerText;                    
                else     
                   jsonString = 
                        document.getElementById('<%= ServerResultID.ClientID %>').textContent;
                
                if (jsonString == "")
                {
                    alert("blank");
                    document.getElementById("ClientResultID").innerText = 
                        "Please, perform client serialization first.";
                     document.getElementById("ClientResultID").style.backgroundColor = 
                        "red";
                }
                else
                {
                    // Deserialize the JSON string 
                    // into the related client object.
                    eval("jsonObject = " + jsonString);
                
                    var message = jsonObject.message;
                    var rgb = jsonObject.rgb;
                
                    var serverColor = "";
                    serverColor = 
                        serverColor.concat(rgb[0], rgb[1], rgb[2]);
               
                    if (document.all)
                        document.getElementById("ClientResultID").innerText = message + rgb;
                    else
                        document.getElementById("ClientResultID").textContent = message + rgb;
                 
                    document.getElementById("ClientResultID").style.backgroundColor = "#" + serverColor;
                }
             
            }     
            
       </script>
        
        
        

        <!-- Sever script performing serialization -->
        <script   runat="server">

         // Define the object to serialize.
         class ColorObject
         {
             public string message = "Server default color is Black ";
             public string[] rgb = new string[] { "00", "00", "00" };
         }

         
        public void OnClick_ServerSerialize(object sender, EventArgs e)
        {

            // Instantiate a serializer.
            System.Web.Script.Serialization.JavaScriptSerializer serializer = 
            new JavaScriptSerializer();
            
            // Perform serialization.
            ColorObject co = new ColorObject();
         
            string serializedServerObject = serializer.Serialize(co);

            ServerResultID.Text = serializedServerObject;
                        
        }
        </script>

    </head>
    
    <body>
     
        <h2>Client Deserialization Server Serialization</h2>
     
        <!-- Add the script manager -->
        <form id="form1" runat="server">
            <asp:ScriptManager runat="server" ID="ScriptManager1"/>
  
            
            <h3>Server</h3>
            <!-- Perform server serialization -->
            <asp:Button ID="ServerButtonID" OnClick="OnClick_ServerSerialize" 
                Text="Server Serialize" runat="server" />
          
            <p>
                <span style="background-color:Yellow">Server serialization:</span>
                <asp:Label ID="ServerResultID" runat="server" />
            </p>
            
            <hr />         
              
            <h2>Client</h2>
            <!-- Perform client deserialization -->
            <button id="ClientButtonID" 
                onclick="OnClick_ClientDeserialize(); return false;">Client Deserialize</button>
         
            <!-- Store client deserialization and preserve it on postback so 
                the server can read it and perform its deserialization -->
            <p>
                <span style="background-color:Yellow">Client deserialization:</span>
                <span id="ClientResultID" 
                style="background-color:Aqua; font-weight:bold; color:Yellow" ></span>
            </p>
            
      
           <hr />
        
       
        
        </form>
    

    </body>
    
</html>
