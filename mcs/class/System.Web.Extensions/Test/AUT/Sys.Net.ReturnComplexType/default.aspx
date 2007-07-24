
<%@ Page Language="C#" %>


<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">


<html xmlns="http://www.w3.org/1999/xhtml" >
    <head id="Head1" runat="server">
        
        <title>Receiving Complex Type</title>
        
        <style type="text/css">
            body {  font: 11pt Trebuchet MS;
                    font-color: #000000;
                    padding-top: 72px;
                    text-align: center }
  
            .text { font: 8pt Trebuchet MS }
        </style>

       
    </head>
    
    <body>
     
        <h2>Receiving Complex Type</h2>
     
        <form id="form1" runat="server">

            <asp:ScriptManager runat="server" ID="scriptManager">
                <Services>
                    <asp:ServiceReference  Path="HandleColor.asmx" />
                </Services>
                <Scripts>
                    <asp:ScriptReference Path="HandleColor.js" />
                </Scripts>
            </asp:ScriptManager>  
           
            <table style="font-size:12px">
                <tr>
                    <td>Web Service Default Color:</td>
                    <td>
                     <button id="Button1" 
                        onclick="GetDefaultColor(); return false;">Get Default Color</button>
                    </td>
                </tr>
            </table> 
         
           <hr />
          
            <!-- Display current color object. -->
            <p>
                <span style="background-color:Yellow">Color:</span>
                <span id="ResultId"></span>
            </p>
        
        
        </form>
            
    </body>
    
</html>


