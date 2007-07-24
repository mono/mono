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
  
        <title>Simple Web Service</title>
        
            <script type="text/javascript">
  
            // This function calls the Web Service method.  
            function EchoUserInput()
            {
                var echoElem = document.getElementById("EnteredValue");
                Samples.AspNet.SimpleWebService.EchoInput(echoElem.value,
                    SucceededCallback);
            }
  
            // This is the callback function that
            // processes the Web Service return value.
            function SucceededCallback(result)
            {
                var RsltElem = document.getElementById("Results");
                RsltElem.innerHTML = result;
            }
  
        </script>
        
    </head>
    
    <body>
        <form id="Form1" runat="server">
         <asp:ScriptManager runat="server" ID="scriptManager">
                <Services>
                    <asp:ServiceReference path="SimpleWebService.asmx" />
                </Services>
            </asp:ScriptManager>
            <div>
                <h2>Simple Web Service</h2>
                    <p>Calling a simple service that echoes the user's input and 
                        returns the current server time.</p>
                    <input id="EnteredValue" type="text" />
                    <input id="EchoButton" type="button" 
                        value="Echo" onclick="EchoUserInput()" />
            </div>
        </form>
        
        <hr/>
        
        <div>
            <span id="Results"></span>
        </div>   
        
    </body>
    
</html>

