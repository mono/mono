<%@ Page Language="C#" %>
<%@ Register Namespace="Samples.CS" TagPrefix="sample" Assembly="SystemWebExtensionsAUT" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>ASP.NET AJAX Control Sample</title>
    <style type="text/css">
    .LowLight
    {
        background-color:#EEEEEE;
    }
    
    .HighLight
    {
        background-color:Ivory;
    }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        <div>
            <table border="0" cellpadding="2">
              <tr>
                <td><asp:Label runat="server" ID="Label1" AssociatedControlID="TextBox1">Name</asp:Label></td>
                <td><sample:SampleTextBox ID="TextBox1" runat="server" NoHighlightCssClass="LowLight" HighlightCssClass="HighLight" /></td>
              </tr>
              <tr>
                <td><asp:Label runat="server" ID="Label2" AssociatedControlID="TextBox2">Phone</asp:Label></td>
                <td><sample:SampleTextBox ID="TextBox2" runat="server" NoHighlightCssClass="LowLight" HighlightCssClass="HighLight" /></td>
              </tr>
              <tr>
                <td><asp:Label runat="server" ID="Label3" AssociatedControlID="TextBox3">E-mail</asp:Label></td>
                <td><sample:SampleTextBox ID="TextBox3" runat="server" NoHighlightCssClass="LowLight" HighlightCssClass="HighLight" /></td>
              </tr>
            </table>
            
            <asp:Button runat="server" ID="Button1" Text="Submit Form" />
        </div>
    </form>
</body>
</html>
