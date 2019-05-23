<%@ Page Language="C#" AutoEventWireup="True" %>
<script runat="server">
	void Page_Load (object sender, EventArgs e)
	{
		a1.HRef = "http://www.example.com";
		a2.HRef = "http://www.example.org";
		a3.HRef = ":@\\";

		a2.Name = "Name";
		a3.Title = "Title";
		a4.Target = "_top";

		a5.ServerClick += new System.EventHandler (Click);
		a5.ServerClick += new System.EventHandler (Click2);
	}

	void Click (object sender, EventArgs e)
	{
		Message.InnerHtml = "Programative Click";
	}

	void Click2 (object sender, EventArgs e)
	{
		Message2.InnerHtml = "too :-)";
	}

	void HtmlAnchor_Click (object sender, EventArgs e)
	{
		Message.InnerHtml = "Declarative Click";
		Message2.InnerHtml = String.Empty;
	}
</script>
<html>
<head>
<title>Just a HtmlAnchor</title>
</head>
<body>
<a id="a1" target="_blank" runat="server">Go mono!</a><hr>
<a id="a2" target="_parent" runat="server">Go mono!</a><hr>
<a id="a3" target="_self" runat="server">Go mono!</a><hr>
<a id="a4" href="http://www.example.com#4" target="mono" runat="server">Go mono!</a><hr>
<form runat="server">
<a id="a5" href="oops" title="t" runat="server" OnServerClick="Click">Click1</a><hr>
<a id="a6" runat="server" OnServerClick="HtmlAnchor_Click">Click2</a><hr>
<span id="Message" runat="server">no click</span><br>
<span id="Message2" runat="server"></span>
</form>
</body>
</html>

