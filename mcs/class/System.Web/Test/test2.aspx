<%@ page language="c#"%>
<html>
<head>
<title>Test</title>
<%Response.Write("Test");%>

<script runat="server" language="c#">
  void Unload(Object sender, EventArgs e) {
    Response.Write("1");
  }
  void Disposed(Object sender, EventArgs e) {
    Response.Write("2");
  }
  void click(Object sender, EventArgs e) {
    Response.Write("3");
  }
</script>

<asp:label id="testing" runat="server"/>

<form action="test.aspx" method="post" runat="server">
<a OnServerClick="click" runat="server">test</a>
</form>
