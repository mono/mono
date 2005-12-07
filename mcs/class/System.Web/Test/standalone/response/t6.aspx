<%@ Page language="c#" debug="true"%>
<html>
<script runat="server">
	void Page_Load (Object sender,EventArgs e) 
	{
		string browser = Context.ApplicationInstance.GetVaryByCustomString (Context, "browser");
		if (browser == null)
			throw new Exception ("B1");

		try {
			browser = Context.ApplicationInstance.GetVaryByCustomString (null, null);
			throw new Exception ("B2");
		} catch (NullReferenceException) {
		}

		browser = Context.ApplicationInstance.GetVaryByCustomString (Context, "custom");
		if (browser != null)
			throw new Exception ("B3");

		Response.Clear ();
		Response.Write ("OK");
		Response.End ();
	}
</script>
<body>
</body>
</html>
