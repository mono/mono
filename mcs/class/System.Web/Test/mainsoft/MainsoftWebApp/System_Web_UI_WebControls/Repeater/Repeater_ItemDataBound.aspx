<%@ Page Language="c#" AutoEventWireup="false" Codebehind="Repeater_ItemDataBound.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.Repeater_ItemDataBound" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>Repeater_ItemDataBound</title>
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
			<P>
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Height="72px" Width="553px">
					<asp:Repeater id="Repeater1" runat="server">
						<ItemTemplate>
							<asp:HyperLink id="hl_name" NavigateUrl="http://www.example.com/" Runat="server">
								<%# Container.DataItem %>
							</asp:HyperLink>
						</ItemTemplate>
					</asp:Repeater>
				</cc1:GHTSubTest></P>
			<P>
				<asp:HyperLink id="HyperLink1" runat="server">HyperLink</asp:HyperLink></P>
		</form>
	</body>
</HTML>
