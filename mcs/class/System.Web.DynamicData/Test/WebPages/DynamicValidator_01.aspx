<%@ Page Language="C#" AutoEventWireup="true" CodeFile="DynamicValidator_01.aspx.cs" Inherits="DynamicValidator_01" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:TextBox runat="server" ID="textBox1" />
        <test:PokerDynamicValidator runat="server" ControlToValidate="textBox1" ID="dynamicValidator1" />
    </div>
    </form>
</body>
</html>
