<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<%@ Page Language="C#" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>
<%@ Register Assembly="App_Code" Namespace="TestNamedHolders" TagPrefix="tnh" %>
<script runat="server">
	protected override void OnLoad (EventArgs e)
	{
		base.OnLoad (e);
		LinkButton lb = new LinkButton();
		lb.ID = "lb";
		lb.Text = "Click me!";
		lb.Click += delegate {
			lb.Text = "Woot! I got clicked!";
		};
		this.container.Controls.Add(lb);
	}
</script>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Default</title>
</head>
<body>
	<form id="form1" runat="server">
	<div>
		<%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><tnh:MyContainer id="container" runat="server">
		</tnh:MyContainer><hr/><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
	</div>
	</form>
</body>
</html>