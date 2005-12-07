<%@ Page Language="C#" AutoEventWireup="True" %>
<%@ Import Namespace="System.Data" %>

<html>
<head>
<script runat="server">
	void Page_Load (object s, EventArgs e)
	{
		if (IsPostBack)
			return;
	}
</script>
</head>
<body>
	<form runat="server">
		<select runat="server" DataSourceID="XmlDataSource" DataTextField="name"> </select>

		<asp:XmlDataSource ID="XmlDataSource" runat="server" DataFile="iran.xml" XPath="IranHistoricalPlaces/Place"/>
	</form>
</body>
</html>
