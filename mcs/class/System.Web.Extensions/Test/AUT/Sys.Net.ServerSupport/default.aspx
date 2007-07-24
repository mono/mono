<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">


<html xmlns="http://www.w3.org/1999/xhtml" >
    <head id="Head1" runat="server">
        
        <title>Using Generated Proxy Types</title>
        <style type="text/css">
            body {  font: 11pt Trebuchet MS;
                    font-color: #000000;
                    padding-top: 72px;
                    text-align: center }
           .text { font: 10pt Trebuchet MS; text-align: center }
        </style>    
    </head>
    
    <body>
     
        <h2>Using Generated Proxy Types</h2>
     
        <form id="form1" runat="server">

            <asp:ScriptManager runat="server" ID="scriptManager">
                <Services>
                    <asp:ServiceReference Path="ServerTypes.asmx" />
                </Services>
                <Scripts>
                    <asp:ScriptReference Path="ServerTypes.js" />
                </Scripts>
            </asp:ScriptManager>  
          
        </form>
            
  
        <center>
            <table style="font-size:12px;" >
            
                <tr align="left">
                    <td class="text">Get Enum:</td>
                    <td>
                        <button id="Button2"  
                            onclick="GetSelectedEnumValue()">Get Enum Value</button>
                    </td>
                </tr>
                
               <tr align="left">
                    <td class="text">Pass Enum:</td>
                    <td>
                     <button id="Button1"  
                        onclick="GetFirstEnumElement()">First Enum</button>
                    </td>
                </tr>
               
            </table> 
        </center>
       
        <hr />
          
        <!-- Display current color object. -->
        <span id="ResultId"></span>
          
    </body>
    
</html>
