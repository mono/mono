<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Samples</title>
</head>
<body>
    <form id="form1" runat="server">
       <asp:ScriptManager runat="server" ID="ScriptManager1">
       </asp:ScriptManager>
       
       <script type="text/javascript">
            function buildAString(title){
                var headTagStart = "<head>";
                var headTagEnd = "</head>";
                var titleTagStart = "<title>";
                var titleTagEnd = "</title>";

                var sb = new Sys.StringBuilder(this._headTagStart);
                sb.append(titleTagEnd);
                sb.append(title);
                sb.append(titleTagEnd);
                sb.append(headTagEnd);
                // Displays: "The result: <head><title>A Title</title></head>"
                // alert("The result" + sb.toString()); 
                result = sb.toString();
            }
            
            var title = "A Title";
            var result;
            buildAString(title);
            
        </script>
    </form>
</body>
</html>

