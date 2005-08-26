<%@ OutputCache Duration="60" VaryByParam="none" %>
<%@ Page language="C#"%>

<html>
<script runat=server>
        static int count = 0;

	void Page_Load (object o, EventArgs e)
	{
		lbl1.Text = "Count:   " + count++;
	}
</script>
<head>
<title>Output Cache Test</title>
</head>
<body>
<form runat="server">

        The count should stay the same until a minute has passed.
        <asp:Label id="lbl1" runat="server" />
        <br>
        <asp:Button id="btn1" Text="Refresh Page" runat="server" />
</form>
</body>
</html>

