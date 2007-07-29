<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">


<html xmlns="http://www.w3.org/1999/xhtml" >
    <head id="Head1" runat="server">
        
       <title> Using XmlHttpExecutor </title>
       
       <style type="text/css">
            body {  font: 11pt Trebuchet MS;
                    font-color: #000000;
                    padding-top: 72px;
                    text-align: center }
  
            .text { font: 8pt Trebuchet MS }
        </style>
    
    </head>

   <body>
    
        <h2>Using XmlHttpExecutor</h2>
        
   
        <form id="form1" runat="server">
            <asp:ScriptManager runat="server" ID="scriptManagerId">
                <Scripts>
                    <asp:ScriptReference Path="XmlHttpExecutor.js" />
                </Scripts>
            </asp:ScriptManager>
        </form>
           
        
        <table>
            <tr align="left">
                <td>Abort a Web request:</td>
                <td>
                    <button id="Button1"  
                        onclick="AbortWebRequest()">Abort</button>
                </td>
            </tr>
            
            <tr align="left">
                <td>Execute a Web request:</td>
                <td>
                    <button id="Button2"  title="also gets headers, body" 
                        onclick="ExecuteWebRequest()">Execute</button>
                </td>
           </tr>
          
           <tr align="left">
                <td>Get Xml:</td>
                <td>
                    <button id="Button3" 
                        onclick="GetXml()">Xml</button>
                </td>
           </tr>
        </table>
     
      
     
        <hr />
       
        <div id="ResultId" style="background-color:Aqua;"></div>
        
        <input id="alert1" />
        <input id="alert2" />
   
    </body>
    
</html>


