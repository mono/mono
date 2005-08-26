<%@ Page Language="C#" AutoEventWireup="True" %>
<%@ Import Namespace="System.Data" %>

<html>
<head>
<script runat="server">
	void Page_Load (object s, EventArgs e)
	{
		if (IsPostBack)
			return;
		
		rep.DataBind ();		
	}

	void Clicked (object o, EventArgs e)
	{
		lbl1.Text = "Pass";
	}	
</script>
</head>
<body>
	<form runat="server">

		<asp:label runat="server" id="lbl1" />
		<asp:Repeater id="rep" runat="server">
			<HeaderTemplate>
				<h1>FAIL</h1>
			</HeaderTemplate>
			<FooterTemplate>
				<h2>FAIL</h2>
			</FooterTemplate>
		</asp:Repeater>

		<asp:button runat="server" Text="Click" id="x" onclick="Clicked" />
	</form>
</body>
</html>
