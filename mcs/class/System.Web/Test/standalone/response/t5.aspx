<%@ Page language="c#" debug="true" %>
<html>
<script runat="server">
	void Page_Load (Object sender,EventArgs e) 
	{
		try {
			HttpResponse.RemoveOutputCacheItem (null);
			throw new Exception ("#01");
		} catch (ArgumentNullException) {}

		HttpResponse.RemoveOutputCacheItem ("");
		HttpResponse.RemoveOutputCacheItem ("/");
		try {
			HttpResponse.RemoveOutputCacheItem ("a");
			throw new Exception ("#02");
		} catch (ArgumentException) {}

		HttpResponse.RemoveOutputCacheItem ("/../hola");
		Response.Clear ();
		Response.Write ("OK");
		Response.End ();
	}
</script>
<body>
</body>
</html>

