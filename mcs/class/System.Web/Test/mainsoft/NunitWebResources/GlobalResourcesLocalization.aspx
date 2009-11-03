<%@ Page Language="C#" UICulture="fr-FR" Culture="fr-FR" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html>
<head runat="server">
	<title>Default</title>
</head>
<body>
	<form id="form1" runat="server">
		Culture: <%= System.Globalization.CultureInfo.CurrentCulture %>
		UICulture: <%= System.Globalization.CultureInfo.CurrentUICulture %>
		<%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:Button id="button1" runat="server" Text="<%$ Resources:Common,Reload %>" /><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
	</form>
</body>
</html>