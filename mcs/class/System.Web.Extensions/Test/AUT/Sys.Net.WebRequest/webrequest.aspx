<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">


<html xmlns="http://www.w3.org/1999/xhtml" >
    <head id="Head1" runat="server">
        
        <title> Using WebRequest </title>
       
        <style type="text/css">
            body {  font: 11pt Trebuchet MS;
                    font-color: #000000;
                    padding-top: 72px;
                    text-align: center }
  
            .text { font: 8pt Trebuchet MS }
        </style>

   </head>

   <body>
     
    <h2>Using WebRequest</h2>
    
        <form id="form1" runat="server">
            <asp:ScriptManager runat="server" ID="ScriptManagerId">
                <Scripts>
                    <asp:ScriptReference Path="WebRequest.js" />
                </Scripts>
            </asp:ScriptManager>
        </form>
     
        <table>
            <tr align="left">
                <td>Make GET Request:</td>
                <td>
                    <button id="Button1"  
                        onclick="GetWebRequest()">GET</button>
                </td>
            </tr>
            <tr align="left">  
                <td>Request Body:</td>
                <td>
                    <button id="Button2"  
                        onclick="PostWebRequest()">Body</button>
                </td>
                
            </tr>
            <tr align="left">
                <td>Request Timeout:</td>
                <td>
                    <button id="Button3"  
                        onclick="WebRequestTimeout()">Timeout</button>
                </td>
            </tr> 
            <tr align="left">
                <td>Request Completed Handler:</td>
                <td>
                    <button id="Button4"  
                        onclick="WebRequestCompleted()">Completed Handler</button>
                </td>
            </tr>
            
            <tr align="left">
                <td>Resolved Url:</td>
                <td>
                    <button id="Button5"  
                        onclick="GetWebRequestResolvedUrl()">Resolved Url</button>
                </td>
                
            </tr>
            
            <tr align="left">
                <td>Request Executor:</td>
                <td>
                    <button id="Button6"  
                        onclick="WebRequestExecutor()">Executor</button>
                </td>
                
            </tr>
            
            <tr align="left">
                <td>Request Header:</td>
                <td>
                    <button id="Button7"  
                        onclick="WebRequestHeader()">Header</button>
                </td>
                
            </tr>
            
        </table>
        
     
        <hr />
       
        <div id="ResultId" style="background-color:Aqua;"></div>


   
    </body>
    
</html>
