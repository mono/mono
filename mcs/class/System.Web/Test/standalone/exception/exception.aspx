<%@ Page language="c#" AutoEventWireup="false" %>
<script runat="server">

	protected override void OnLoad( EventArgs args )
	{
		base.OnLoad(args);
		// System.Theading.Thread.Abort (); // this won't call OnError.
		throw new Exception();
	}

	protected override void OnError(EventArgs e) 
	{
		base.OnError(e);
		HttpContext.Current.Response.Redirect( "error.aspx" );
	}

</script>
<html>
	<head>
		<title>Mono Bugs</title>
	</head>
	<body id="body">
		<form method="post" runat="server" id="form">
		</form>
	</body>
</html>
