<%@ Page Language="C#" %>

<%@ Import Namespace="System.Web.Script.Serialization" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">


<html xmlns="http://www.w3.org/1999/xhtml" >
    <head id="Head1" runat="server">
        
        <title>Passing Complex Type</title>
        <style type="text/css" >
            body 
            {
                font-family: Verdana, Arial, Helvetica, sans-serif;
                font-size: 80%;
                width: 100%;
            }
        </style>
         
    </head>
    
    <body>
     
        <h2>Passing Complex Type</h2>
     
        <form id="form1" runat="server">

            <asp:ScriptManager runat="server" ID="scriptManager">
                <Services>
                    <asp:ServiceReference Path="HandleColor.asmx" />
                </Services>
                <Scripts>
                    <asp:ScriptReference Path="HandleColor.js" />
                </Scripts>
            </asp:ScriptManager>  
           
            <table style="font-size:12px">
                <tr>
                    <td>Change Color:</td>
                    <td>
                        <select id="ColorSelectID"   
                            onchange="OnChangeDefaultColor(this);" runat="server">
                            <option value="00,00,FF" SELECTED>Blue</option>
                            <option value="FF,00,00">Red</option>
                            <option value="00,FF,00">Green</option>
                        </select>
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

