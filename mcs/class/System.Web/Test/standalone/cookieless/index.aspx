<%@ language="C#" %>
<html>
<script runat=server>
        void Clicked (object o, EventArgs e)
        {
                Console.WriteLine (HttpContext.Current.Server.MapPath
("site.config"));
		//Server.Transfer ("/other/index2.aspx");
		//Server.Transfer ("/index2.aspx");
		Server.Transfer ("/index 2.aspx");
        }
</script>
<head>
<title>Button</title>
</head>
<body>
<form runat="server">
<%= String.Format ("FilePath: {0} {1}", Context.Request.FilePath, GetType ().Name) %>
<%= String.Format ("CurrentExecutionFilePath: {0}", Context.Request.CurrentExecutionFilePath, GetType ().Name) %>
<br>
<asp:Button id="btn"
     Text="Submit"
     OnClick="Clicked"
     runat="server"/>
</form>
</body>
</html>
