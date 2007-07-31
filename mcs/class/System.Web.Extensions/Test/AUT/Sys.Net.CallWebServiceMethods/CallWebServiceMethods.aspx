<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

  

<html xmlns="http://www.w3.org/1999/xhtml">
  
    <head id="Head1" runat="server">
 
        <style type="text/css">
            body {  font: 11pt Trebuchet MS;
                    font-color: #000000;
                    padding-top: 72px;
                    text-align: center }
  
            .text { font: 8pt Trebuchet MS }
        </style>
  
        <title>Calling Web Methods</title>
    
    </head>
    
    <body>
        <form id="Form1" runat="server">
        
            <asp:ScriptManager runat="server" ID="scriptManagerId">
                <Scripts>
                    <asp:ScriptReference Path="CallWebServiceMethods.js" />
                </Scripts>
                <Services>
                    <asp:ServiceReference  Path="WebService.asmx" />
                </Services>
                
            </asp:ScriptManager>
            
            <div>
                <h2>Calling Web Methods</h2>
                 
               <table>
                    <tr align="left">
                        <td>Method that does not return a value:</td>
                        <td>
                            <!-- Getting no retun value from 
                            the Web service. --> 
                            <button id="Button1"  
                                onclick="GetNoReturn(); return false;">No Return</button>
                        </td>
                    </tr>
                    
                    <tr align="left">
                        <td>Method that returns a value:</td>
                        <td>
                            <!-- Getting a retun value from 
                            the Web service. --> 
                            <button id="Button2" 
                                onclick="GetTime(); return false;">Server Time</button>
                        </td>
                   </tr>
                   
                   <tr align="left">
                        <td>Method that takes parameters:</td>
                        <td>
                            <!-- Passing simple parameter types to 
                            the Web service. --> 
                            <button id="Button3" 
                                onclick="Add(20, 30); return false;">Add</button>
                        </td>
                       
                    </tr>
                   
                    <tr align="left">
                        <td>Method that returns XML data:</td>
                        <td>   
                             <!-- Get Xml. --> 
                            <button id="Button4" 
                                onclick="GetXmlDocument(); return false;">Get Xml</button>
                        </td>
                    </tr>
                    <tr align="left">
                        <td>Method that uses GET:</td>
                        <td>   
                             <!-- Making a GET Web request. --> 
                            <button id="Button5" 
                                onclick="MakeGetRequest(); return false;">Make GET Request</button>
                        </td>
                    </tr>
                    
                </table>
         
            </div>
        </form>
        
        <hr/>
        
        <div>
            <span id="ResultId"></span>
        </div>   
        
    </body>
    
</html>
