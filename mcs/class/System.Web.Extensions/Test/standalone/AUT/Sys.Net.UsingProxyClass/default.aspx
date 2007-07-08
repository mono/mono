<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">


<html xmlns="http://www.w3.org/1999/xhtml" >
    <head id="Head1" runat="server">
        
        <title>Using Generated Web Service Proxy Class</title>
        <style type="text/css">
            body {  font: 11pt Trebuchet MS;
                    font-color: #000000;
                    padding-top: 72px;
                    text-align: center }
           .text { font: 10pt Trebuchet MS; text-align: center }
        </style>    
    </head>
    
    <body>
     
        <h2>Using Generated Web Service Proxy Class</h2>
     
        <form id="form1" runat="server">

            <asp:ScriptManager runat="server" ID="scriptManager">
                <Services>
                    <asp:ServiceReference Path="UsingProxyClass.asmx" />
                </Services>
                <Scripts>
                    <asp:ScriptReference Path="UsingProxyClass.js" />
                </Scripts>
            </asp:ScriptManager>  
          
        </form>
            
  
        <center>
            <table style="font-size:12px;" >
            
                 <tr align="left">
                    <td class="text">Get Server Object:</td>
                    <td>
                     <button id="Button3"  
                        onclick="GetDefaultColor()">Get Default Color</button>
                    </td>
                </tr>
                
                 <tr align="left">
                    <td class="text">Pass Server Object:</td>
                    <td>
                     <button id="Button4"  
                        onclick="SetColor()">Set Color</button>
                    </td>
                </tr>
                
            </table> 
        </center>
       
        <hr />
          
        <!-- Display current color object. -->
        <span id="ResultId"></span>
          
    </body>
    
</html>
