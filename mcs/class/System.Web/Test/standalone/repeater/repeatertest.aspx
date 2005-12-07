<%@ Page language="c#" src="./repeatertest.aspx.cs"  Inherits="test.SimplePage" AutoEventWireup="false"%>

<html>
<head>
</head>
<body>

<asp:XmlDataSource ID="XmlDataSource" runat="server" DataFile="iran.xml" XPath="IranHistoricalPlaces/Place"/>

</body>
</html>
