<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DataList_AlternatingItemStyle.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DataList_AlternatingItemStyle" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DataList_AlternatingItemStyle</title>
		<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
		<meta content="Visual Basic .NET 7.1" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<script LANGUAGE="JavaScript">
        function ScriptTest()
        {
            var theform;
		    if (window.navigator.appName.toLowerCase().indexOf("netscape") > -1) {
			    theform = document.forms["Form1"];
		    }
		    else {
			    theform = document.Form1;
		    }
        }
		</script>
	</HEAD>
	<body>
		<form id="Form1" method="post" runat="server">
			<cc1:GHTSubTest id="GHTSubTest1" runat="server" Height="96px" Width="144px" Description="Default style">
				<asp:DataList id="DataList1" runat="server">
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>&nbsp;
			<cc1:GHTSubTest id="GHTSubTest2" runat="server" Width="144px" Height="96px" Description="Set legal style ">
				<asp:DataList id="DataList2" runat="server">
					<AlternatingItemStyle Font-Size="Medium" Font-Names="Batang" HorizontalAlign="Left" BorderWidth="2px"
						ForeColor="Yellow" BorderStyle="Solid" BorderColor="Navy" VerticalAlign="Top" BackColor="Red"></AlternatingItemStyle>
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest3" runat="server" Width="144px" Height="96px" Description="Overrides ItemStyle">
				<asp:DataList id="DataList3" runat="server">
					<AlternatingItemStyle Font-Size="Medium" Font-Names="Batang" HorizontalAlign="Left" BorderWidth="2px"
						ForeColor="Yellow" BorderStyle="Solid" BorderColor="Navy" VerticalAlign="Top" BackColor="Red"></AlternatingItemStyle>
					<ItemStyle Font-Names="Miriam Fixed" HorizontalAlign="Right" BorderWidth="1px" ForeColor="Purple"
						BorderStyle="Dotted" BorderColor="Lime" VerticalAlign="Bottom" BackColor="#FFFF80"></ItemStyle>
					<ItemTemplate>
						<%#Container.DataItem%>
					</ItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
		</form>
	</body>
</HTML>
