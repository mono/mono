<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>
<script runat="server">
	public virtual void button1Clicked (object sender, EventArgs args)
	{
		button1.Text = "You clicked me";
	}
		
	protected void button1_Init (object sender, System.EventArgs e)
	{
		results.Text += " Init: button1.";
	}
		
	protected void sqlDs1_Init (object sender, System.EventArgs e)
	{
		results.Text += " Init: sqlDataSource1";
	}
</script>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Bug #572781</title>
</head>
<body>
	<form id="form1" runat="server">
<%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:Literal runat="server" id="results"/><asp:Button id="button1" runat="server" Text="Click me!" OnClick="button1Clicked" OnInit="button1_Init" /><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
		<asp:SqlDataSource id="sqlDataSource1" runat="server" OnInit="sqlDs1_Init" />
	</form>
</body>
</html>