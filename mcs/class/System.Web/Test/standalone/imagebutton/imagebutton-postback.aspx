<%@ Page Language="C#" %>
<script runat="server">
	void btnclick (object s, ImageClickEventArgs ea)
	{
		txt.Text = "PASS";
	}
</script>

<html>
<body>
	Click the button and a message will appear: <asp:label id="txt" runat="server" />
	<form runat="server">
	      <asp:imagebutton imageurl="http://www.novell.com/common/img/hdr_logo_pf.gif" alternatetext="click here" onclick="btnclick" runat="server"/> 
	</form>
</body>
</html>
