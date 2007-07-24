<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">


<html xmlns="http://www.w3.org/1999/xhtml" >
    <head id="Head1" runat="server">
        
        <title> WebRequestManager  Example </title>
        
        <style type="text/css">
            body {  font: 11pt Trebuchet MS;
                    font-color: #000000;
                    padding-top: 72px;
                    text-align: center }
  
            .text { font: 8pt Trebuchet MS }
        </style>
    

        
     </head>

   <body>
     
     
    <h2>WebRequestManager Example</h2>
    
        <form id="form1" runat="server">
            <asp:ScriptManager runat="server" ID="scriptManagerId">
                <Scripts>
                    <asp:ScriptReference Path="WebRequestManager.js" />
                </Scripts>
            </asp:ScriptManager>
        </form>
             
      
        <table>
            <tr align="left">
                <td>Make a Web request:</td>
                <td>
                    <button id="Button1" 
                        title="adds and remove handlers, too"  
                        onclick="MakeWebRequest(); return false;">Web Request</button>
                </td>
            </tr>
          
            <tr align="left">
                <td>Set, get default executor:</td>
                <td>
                    <button id="Button2"  
                        onclick="DefaultExecutor(); return false;">Executor</button>
                </td>
           </tr>
          
           <tr align="left">
                <td>Set, get default timeout:</td>
                <td>
                    <button id="Button3" 
                        onclick="DefaultTimeout(); return false;">Timeout</button>
                </td>
           </tr>
           
        </table>
     
        <hr />
       
        <div id="ResultId" style="background-color:Aqua;"></div>


    </body>
    
</html>
