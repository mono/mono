<%@ Page Language="C#" CodeFile="default.aspx.cs" Inherits="testwebemailcontrols.Default" %>

<%@ Register Src="test.ascx" TagName="test" TagPrefix="tester"  %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html>
<head runat="server">
	<title>Default</title>
</head>
<body>
	<form id="form1" runat="server">
	<!--START--><%= AppDomain.CurrentDomain.GetData ("BEGIN_CODE_MARKER") %><tester:test id="testertest" runat="server" stringSlam="string">
		<slam State="4444" Text="snap test snap"></slam>
		<stringBuilderSlam Length="0"/>
		<dateTimeSlam/>
		<intSlam/>
	</tester:test><%= AppDomain.CurrentDomain.GetData ("END_CODE_MARKER") %><!--END-->
		<asp:Button id="button1" runat="server" Text="Click me!" OnClick="button1Clicked" />
	</form>
</body>
</html>
