<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">


<html xmlns="http://www.w3.org/1999/xhtml" >
    <head id="Head1" runat="server">
        <title>Connecting HTTP End Points Sample</title>
         <style type="text/css">
            body {  font: 11pt Trebuchet MS;
                    font-color: #000000;
                    padding-top: 72px;
                    text-align: center }
  
            .text { font: 8pt Trebuchet MS }
        </style>
  
    </head>

    <body>
    
        <h2>Connecting HTTP End Points Example</h2>

        <!-- Add the script manager -->
        <form id="form1" runat="server">
            <asp:ScriptManager runat="server" ID="ScriptManagerId">
                <Scripts>
                    <asp:ScriptReference Path="ConnectingEndPoints.js" />
                </Scripts>
            </asp:ScriptManager>
        </form>
         
        <table>
        
            <tr align="left">
                <td>Make GET Request:</td>
                <td>
                    <button id="Button1"  
                        onclick="GetWebRequest()"
                        type="button">GET Request</button>
                </td>
            </tr>
            <tr align="left">
                <td>Make POST Request:</td>
                <td>
                    <button id="Button2"  
                        onclick="PostWebRequest()"
                        type="button">POST Request</button>
                </td>
            </tr>
        
        </table>
        
        <hr />
                 
        <div id="ResultId" style="background-color:Aqua" />
        
    </body>

</html>

