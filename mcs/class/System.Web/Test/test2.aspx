<%@ page language="c#" debug="true"%>
<html>
<head>
<title>Test</title>
<%Response.Write("Test");%>

<script runat="server" language="c#">
  void click(Object sender, EventArgs e) {
    if(Object.Events != null)
      Response.Write("1");
    else
      Response.Write("2");
  }
</script>

<asp:label id="testing" runat="server"/>

<form action="test.aspx" method="post" runat="server">
<a OnServerClick="click" runat="server">test</a>
</form>
