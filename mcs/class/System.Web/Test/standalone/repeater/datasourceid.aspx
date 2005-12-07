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
		<asp:Repeater ID="Repeater" runat="server" DataSourceID="XmlDataSource">
		 <HeaderTemplate>
                   <div>
                 </HeaderTemplate>
		 <ItemTemplate>
		    <Strong><%# XPath("@name") %><br /></Strong>
		    <%#XPath("City")%><br />
		    <%#XPath("Antiquity")%><br />
		 </ItemTemplate>
		 <FooterTemplate>
                   </div>
                 </FooterTemplate>
		</asp:Repeater>

		<asp:XmlDataSource ID="XmlDataSource" runat="server" DataFile="iran.xml" XPath="IranHistoricalPlaces/Place"/>
	</form>
</body>
</html>
