<%@ Page Language="C#" %>
<html>
<script runat=server>
	void Page_Load ()
	{
		if (!IsPostBack)
			return;

		HttpFileCollection Files = Request.Files;
		string [] names = Files.AllKeys;
		for (int i = 0; i < names.Length; i++) {
			Files [i].SaveAs ("FILE" + i);
		}
	}
</script>
<title>HtmlInputFile</title>
<body>
This should save the file you upload in the server as 'FILE0'. <br/>
<form id="myForm" name="myform" method="post" runat="server">
Pick a file:
<input id="myFile" type="file" runat="server"> 
<br>
<asp:Button id="btn" Text="Go send it!" runat="server" />
<asp:TextBox Columns="2" MaxLength="3" Text="1" runat="server"/>
</form>
</body>
</html>

