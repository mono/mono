
<%@ Page Language="C#" %>

<%@ Import Namespace="System.Web.Script.Serialization" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">


<html xmlns="http://www.w3.org/1999/xhtml" >
    <head id="Head1" runat="server">
        
        <title>Exchanging Complex Types</title>
        <style type="text/css">
            body {  font: 11pt Trebuchet MS;
                    font-color: #000000;
                    padding-top: 72px;
                    text-align: center }
  
            .text { font: 8pt Trebuchet MS }
        </style>

    </head>
    
    <body>
     
        <h2>Exchanging Complex Types</h2>
     
        <form id="form1" runat="server">

            <asp:ScriptManager runat="server" ID="scriptManager">
                <Services>
                    <asp:ServiceReference Path="HandleColor.asmx" />
                </Services>
                <Scripts>
                    <asp:ScriptReference Path="HandleColor.js" />
                </Scripts>
            </asp:ScriptManager>  
          
        </form>
            
  
        <center>
            <table style="font-size:12px;" >
                <tr align="center">
                    <td class="text">Change Color:</td>
                    <td>
                        <select id="ColorSelectID"   
                            onchange="OnChangeDefaultColor(this);" runat="server">
                        </select>
                    </td>
                </tr>
            </table> 
        </center>
       
        <hr />
          
        <!-- Display current color object. -->
        <span style="background-color:Yellow">Color:</span>
        <span id="ResultId"></span>
          <br /><input id="ok" />
    </body>
    
</html>
