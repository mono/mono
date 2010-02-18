<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>#Bug578770</title>
</head>
<body>
<form id="form1" runat="server">
<%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:CheckBoxList  ID="test" runat="server" /><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
</form>
</body>
</html>


