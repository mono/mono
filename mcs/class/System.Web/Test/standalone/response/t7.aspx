<%@ Page language="c#" debug="true"%>
<html>
<script runat="server">
	void Page_Load (Object sender,EventArgs e) 
	{
		Context.RewritePath ("rewrite_next.aspx?hola=pepe");        
		string vpath = HttpRuntime.AppDomainAppVirtualPath;
		if (vpath.EndsWith ("/"))
			vpath = vpath.Substring (0, vpath.Length - 1);

		/* This one fails with IIS on /test but works with xsp on '/'
		if (Request.FilePath != vpath + "/rewrite_next.aspx")
			throw new Exception ("#01" + " " + vpath + " " + Request.FilePath);
		*/

		if (Request.QueryString ["hola"] != "pepe")
			throw new Exception ("#02");


		Context.RewritePath ("rewrite_xxx.aspx");        
		if (Request.FilePath != vpath + "/rewrite_xxx.aspx")
			throw new Exception ("#03");

		// QueryString preserved
		if (Request.QueryString ["hola"] != "pepe")
			throw new Exception ("#04");

		Context.RewritePath ("rewrite_xx1.aspx", null, null);        
		if (Request.FilePath != vpath + "/rewrite_xx1.aspx")
			throw new Exception ("#05");

		// QueryString preserved
		if (Request.QueryString ["hola"] != "pepe")
			throw new Exception ("#06");

		Context.RewritePath ("rewrite_xx2.aspx", "", "");        
		if (Request.FilePath != vpath + "/rewrite_xx2.aspx")
			throw new Exception ("#07");

		// QueryString preserved
		if (Request.QueryString.Count > 0)
			throw new Exception ("#08");

		Response.Clear ();
		Response.Write ("OK");
		Response.End ();
	}
</script>
<body>
The test went OK.
</body>
</html>

