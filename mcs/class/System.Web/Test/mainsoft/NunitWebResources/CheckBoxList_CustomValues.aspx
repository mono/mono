<%@ Page Language="C#" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>Value attribute on items</title>
</head>
<body>
<form id="form1" runat="server" method="post">
<%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %>
	<asp:CheckBoxList id="checkBoxList" runat="server">
		<asp:ListItem Value="val1" Selected="true">Text1</asp:ListItem>
		<asp:ListItem Value="val2">Text2</asp:ListItem>
		<asp:ListItem Value="val3" Selected="true">Text3</asp:ListItem>
	</asp:CheckBoxList>
<%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
</form>
</body>
</html>
