<%@ Page language="c#" AutoEventWireup="true"%>

<html>
<head>
<script runat="server">

	
	void Page_Load (object s, EventArgs e)
	{            
		HttpCookie cookie = HttpContext.Current.Request.Cookies ["blah"];
		if (cookie != null)
			return;

		HttpContext.Current.Response.Cookies ["blah"].Expires = DateTime.Now.AddMinutes(15);
		HttpContext.Current.Response.Cookies ["blah"].Value = Guid.NewGuid ().ToString ();
	}

</script>

</head>
<body>
</body>
</html>
