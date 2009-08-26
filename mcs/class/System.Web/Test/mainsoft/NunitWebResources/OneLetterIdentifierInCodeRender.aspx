<%@ Page Language="C#"  %>
<script runat="server">
    void DoR ()
    {
		Response.Write ("DoR called");
    }
</script>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html>
<head>
	<title>Bug #400807</title>
</head>
<body>
	
	<form id="form1" runat="server">
	<%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><% 
	System.Action r = new Action (DoR);%>
	<% Response.Write("b");%> 
	
	 <% r(); %><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
	</form>
</body>
</html>
