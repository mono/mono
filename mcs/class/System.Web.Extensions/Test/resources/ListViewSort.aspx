<%@ Page Language="C#" AutoEventWireUp="true" %>
<%@ Register Assembly="System.Web.Extensions_test_net_2_0" Namespace="Tests.System.Web.UI.WebControls" TagPrefix="tc" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
  <head id="Head1" runat="server"><title>Untitled Page</title></head>
  <body>
    <form id="form1" runat="server">
      <div><%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %>
	<tc:ListViewPoker runat="server">
	  
	</tc:ListView>
	<%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
      </div>
    </form>
  </body>
</html>
