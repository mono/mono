<%@ Page Language="C#" Debug="true" %>
<%@ Import namespace="System.Collections" %>
<%@ Import namespace="System.Text" %>
<html>
<script runat="server">
	void Page_Load ()
	{
		ar1.AdvertisementFile = null;
	}
</script>
<body>
Setting AdvertisementFile to null in Page_Load. Should display a broken img.
It used to crash.
<br>
<form runat="server">
<asp:adrotator runat="server" id="ar1" />
</form>
</body>
</html>

