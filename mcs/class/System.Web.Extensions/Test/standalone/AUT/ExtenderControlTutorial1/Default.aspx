<%@ Page Language="C#"  %>
<%@ Register Namespace="Samples.CS" TagPrefix="sample" Assembly="SystemWebExtensionsAUT" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>ASP.NET AJAX Behavior Sample</title>
    <style type="text/css">
    .LowLight
    {
        background-color:#EEEEEE;
    }
    
    .HighLight
    {
        background-color:Ivory;
    }
    .LowLightButton
    {
        font-weight:normal;
        width:100px;
    }
    
    .HighLightButton
    {
        font-weight:bold;
        width:100px;
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
                <td><asp:TextBox ID="TextBox1" runat="server" /></td>
              </tr>
              <tr>
                <td><asp:Label runat="server" ID="Label2" AssociatedControlID="TextBox2">Phone</asp:Label></td>
                <td><asp:TextBox ID="TextBox2" runat="server" /></td>
              </tr>
              <tr>
                <td><asp:Label runat="server" ID="Label3" AssociatedControlID="TextBox3">E-mail</asp:Label></td>
                <td><asp:TextBox ID="TextBox3" runat="server" /></td>
              </tr>
            </table>
            
            <asp:Button runat="server" ID="Button1" Text="Submit Form" />
            
            <sample:FocusExtender ID="FocusExtender1" runat="server"
                                  NoHighlightCssClass="LowLight"
                                  HighlightCssClass="HighLight"
                                  TargetControlID="TextBox1" />
            <sample:FocusExtender ID="FocusExtender2" runat="server"
                                  NoHighlightCssClass="LowLight"
                                  HighlightCssClass="HighLight"
                                  TargetControlID="TextBox2" />
            <sample:FocusExtender ID="FocusExtender3" runat="server"
                                  NoHighlightCssClass="LowLight"
                                  HighlightCssClass="HighLight"
                                  TargetControlID="TextBox3" />
            <sample:FocusExtender ID="FocusExtender4" runat="server"
                                  NoHighlightCssClass="LowLightButton"
                                  HighlightCssClass="HighLightButton"
                                  TargetControlID="Button1" />
        </div>

    </form>
</body>
</html>
