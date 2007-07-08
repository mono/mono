<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
 <head id="Head1" runat="server">
 
        <style type="text/css">
            body {  font: 11pt Trebuchet MS;
                    font-color: #000000;
                    padding-top: 72px;
                    text-align: center }
  
            .text { font: 8pt Trebuchet MS }
        </style>
  
        <title>Web Service Proxy</title>
        
        
</head>
<body>
    <h2>WebServiceProxy Example</h2>
     
        <form id="form1" runat="server">
            <asp:ScriptManager runat="server" ID="scriptManager">
                <Scripts>
                    <asp:ScriptReference Path="WebServiceProxy.js" />
                </Scripts>
            </asp:ScriptManager>  
           
            <table style="font-size:12px">
                <tr>
                    <td>Select Web Service and Method:</td>
                    <td>
                        <select id="SelectionId"     
                            onchange="OnSelectMethod(); return false;">
                            <option value="WebService.asmx" selected>GetServerTime</option>
                            <option value="WebService.asmx">GetGreetings</option>
                            <option value="WebService.asmx">PostGreetings</option>
                        </select>
                    </td>
                </tr>
            </table> 
         
           <hr />
          
            <!-- Display results. -->
            <p>
                <span  style="background-color:Aqua" id="ResultId"></span>
            </p>
        
        
        </form>
</body>
</html>
