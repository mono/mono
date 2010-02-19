<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head><title>Bug #325489 part 1</title></head>
<body>
    <form id="form1" runat="server">
<%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:Button id="button1" runat="server" backcolor="#316AC5" /><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
    </form>
</body>
</html>
