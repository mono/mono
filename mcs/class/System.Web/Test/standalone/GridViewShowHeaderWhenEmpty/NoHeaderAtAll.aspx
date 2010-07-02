<%@ Page Language="C#" AutoEventWireup="true" CodeFile="NoHeaderAtAll.aspx.cs" Inherits="NoHeaderAtAll" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <%= AppDomain.CurrentDomain.GetData ("BEGIN_CODE_MARKER") %><asp:GridView runat="server" ID="GridView1" AutoGenerateColumns="true" ShowHeader="false" ShowHeaderWhenEmpty="true" OnRowCreated="GridView1_RowCreated"/><pre runat="server" id="log" /><%= AppDomain.CurrentDomain.GetData ("END_CODE_MARKER") %>
    </div>
    </form>
</body>
</html>
