<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">


<html xmlns="http://www.w3.org/1999/xhtml" >
    <head id="Head1" runat="server">
        
        <title>Using Generics Proxy Types</title>
        <style type="text/css">
            body {  font: 11pt Trebuchet MS;
                    font-color: #000000;
                    padding-top: 72px;
                    text-align: center }
           .text { font: 10pt Trebuchet MS; text-align: center }
        </style>    
    </head>
    
    <body>
     
        <h2>Using Generics Proxy Types</h2>
     
     
        <form id="form1" runat="server">

            <asp:ScriptManager runat="server" ID="scriptManager">
                <Services>
                    <asp:ServiceReference Path="WebService.asmx" />
                </Services>
                <Scripts>
                    <asp:ScriptReference Path="generics.js" />
                </Scripts>
            </asp:ScriptManager>  
          
        </form>
            
  
        <center>
            <table style="font-size:12px;" >
            
                <tr align="left">
                    <td class="text">Generic List:</td>
                    <td>
                        <button id="Button1"  
                            onclick="GenericList()">Get List</button>
                    </td>
                </tr>
                
               <tr align="left">
                    <td class="text">Generic Dictionary:</td>
                    <td>
                     <button id="Button2"  
                        onclick="GenericDictionary()">Get Dictionary</button>
                    </td>
                </tr>
                
                 <tr align="left">
                    <td class="text">Generic Custom Type Dictionary:</td>
                    <td>
                     <button id="Button3"  
                        onclick="GenericCustomTypeDictionary()">Get Dictionary</button>
                    </td>
                </tr>
                
                  
                <tr align="left">
                    <td class="text">Generic Dictionary:</td>
                    <td>
                     <button id="Button5"  
                        onclick="PassGenericDictionary()">Pass Dictionary</button>
                    </td>
                </tr>
                    
                 <tr align="left">
                    <td class="text">Array:</td>
                    <td>
                     <button id="Button4"  
                        onclick="ArrayType()">Get Array</button>
                    </td>
                </tr>
               
            </table> 
        </center>
       
        <hr />
          
        <!-- Display current color object. -->
        <span id="ResultId"></span>
          
    </body>
    
</html>
