<%@ Page Language="C#" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Bug #600415</title>
</head>
<body>
	<form id="form1" runat="server">
<%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:CheckBoxList id="checkBoxList" runat="server">
<asp:ListItem Selected="true">Item 1</asp:ListItem>
<asp:ListItem>Item 2</asp:ListItem>
<asp:ListItem Selected="true">Item 3</asp:ListItem>
<asp:ListItem>Item 4</asp:ListItem>
</asp:CheckBoxList><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
<asp:Button id="cmdClick" runat="server" Text="Ok" />
	</form>
</body>
</html>