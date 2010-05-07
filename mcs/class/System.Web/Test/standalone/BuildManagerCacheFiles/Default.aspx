<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>Create:</div>
    <div>
    CodeGenDir: <span runat="server" id="codeGenCreate" /><br />
    File path: <span runat="server" id="filePathCreate" /><br />
    </div>

    <div>Read:</div>
    <div>
    CodeGenDir: <span runat="server" id="codeGenRead" /><br />
    File path: <span runat="server" id="filePathRead" /><br />
    </div>

    <div>Log:</div>
    <pre runat="server" id="log" />
    </form>
</body>
</html>
