<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>
<%@ Register Assembly="App_Code" Namespace="MonoTests.Controls" TagPrefix="test" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
<%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><test:EnumConverterTextBox runat="server" ID="test" Values="FlagOne"/><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
    </div>
    </form>
</body>
</html>
