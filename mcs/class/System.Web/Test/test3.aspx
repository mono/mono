<%@ Page Debug="true" %>
<html>
<script runat=server language="vb">

Sub AnchorBtn_Click(Source As Object, E as EventArgs)
          Message.InnerText = Message.InnerHtml
       End Sub

    </script>

<body>
<form method=post runat=server>

<a OnServerClick="AnchorBtn_Click" runat=server> Click here at your peril.</a>

<h1>
<span id="Message" runat=server><span id="Message2" runat=server>narf</span></span>
</h1>

</form>
</body>
</html>

