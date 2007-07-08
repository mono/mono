
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
   <title>Passing user context or method name</title>
        
     
    </head>
    
    <body>
        <form id="Form1" runat="server">
        
            <asp:ScriptManager runat="server" ID="scriptManager">
                <Services>
                    <asp:ServiceReference Path="WebService.asmx" />
                </Services>
                
                <Scripts>
                    <asp:ScriptReference Path="WebServiceMultipleCallers.js" />
                </Scripts>
                
            </asp:ScriptManager>
            <div>
                <h2>Passing User Context or Method Name</h2>
               
                <table>
                    <tr align="left">
                        <td>Passing user context:</td>
                        <td>
                            <button id="Button1" 
                                onclick="AddWithContext(10, 20, 'user context information'); return false;">User Context</button>
                        </td>
                    </tr>
                    <tr align="left">
                      <td>Passing method:</td>
                      <td>   
                        <button id="Button7" 
                            onclick="AddWithMethod(10, 30); return false;">Method Name</button>
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
