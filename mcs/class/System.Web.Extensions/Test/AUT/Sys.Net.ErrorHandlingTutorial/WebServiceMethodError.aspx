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
  
        <title>Handling Web Service Error</title>
     
        
    </head>
    
    <body>
        <form id="Form1" runat="server">
        
            <asp:ScriptManager runat="server" ID="scriptManagerId">
            
                <Scripts>
                    <asp:ScriptReference Path="WebServiceMethodError.js" />
                </Scripts>
                <Services>
                    <asp:ServiceReference  Path="WebService.asmx" />
                </Services>
                
            </asp:ScriptManager>
            <div>
                <h2>Handling Web Service Error</h2>
               
                <table>
                    <tr align="left">
                        <td>Method with error:</td>
                        <td>
                           <!-- Cause divide by zero failure. --> 
                            <button id="Button1" 
                                onclick="Div(10, 0); return false;">Div Error</button>
                        </td>
                    </tr>
                
                </table>
         
            </div>
        </form>
        
        <hr/>
        
        <div>
            <span id="Results"></span>
        </div>   
        
    </body>
    
</html>
