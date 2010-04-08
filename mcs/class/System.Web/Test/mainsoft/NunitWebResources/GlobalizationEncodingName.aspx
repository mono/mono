<%@ Page Language="C#" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>
<%@ Import Namespace="System.Web.Configuration" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
</head>
<body>
<%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><%
        Configuration config = WebConfigurationManager.OpenWebConfiguration("~/");
        GlobalizationSection configSection = (GlobalizationSection)config.GetSection("system.web/globalization");
        configSection.RequestEncoding = Encoding.UTF7;
        if (configSection.RequestEncoding == Encoding.UTF7)
    		Response.Write ("GOOD");
    	else
    		Response.Write ("BAD");
%><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
</body>
</html>
