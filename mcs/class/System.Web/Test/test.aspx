<%@ page language="c#"%>
<html>
<head>
<title>Test</title>
<%Response.Write("Test");%>

<script runat="server" language="c#">
  void SubmitBtn_Click(Object sender, EventArgs e) {
    Response.Write("Hi");
  }
</script>

<form action="test.aspx" method="post" runat="server">
<asp:button text="Click Me" OnClick="SubmitBtn_Click" runat="server"/>
</form>

<!-- output

<html>
<head>
<title>Test</title>
Test<form name="ctrl0" method="post" action="test.aspx" id="ctrl0">
<input type="hidden" name="__VIEWSTATE" value="dDwtMTc0MDc5ODg1Mzs7Pg==" />

<input type="submit" name="ctrl1" value="Click Me" />
</form>


-->