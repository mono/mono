<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DataList_EditItemIndex.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DataList_EditItemIndex" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DataList_EditItemIndex</title>
		<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
		<meta content="Visual Basic .NET 7.1" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<script language="JavaScript">
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
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Width="232px" Height="112px" Description="Not set">
					<asp:DataList id="DataList1" runat="server" Width="136px" Height="112px">
						<ItemTemplate>
							Regular item.
						</ItemTemplate>
						<EditItemTemplate>
							Selected for editing!
						</EditItemTemplate>
					</asp:DataList>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest2" runat="server" Width="232px" Height="112px" Description="Set to legal value">
					<asp:DataList id="DataList2" runat="server" Width="144px" Height="120px" EditItemIndex="2">
						<ItemTemplate>
							Regular item.
						</ItemTemplate>
						<EditItemTemplate>
							Selected for editing!
						</EditItemTemplate>
					</asp:DataList>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest3" runat="server" Width="232px" Height="112px" Description="Higher then the number of items.">
					<asp:DataList id="DataList3" runat="server" Width="136px" Height="122px" EditItemIndex="10">
						<ItemTemplate>
							Regular item.
						</ItemTemplate>
						<EditItemTemplate>
							Selected for editing!
						</EditItemTemplate>
					</asp:DataList>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest4" runat="server" Width="232px" Height="112px" Description="Illegal value - less then -1">
					<asp:DataList id="DataList4" runat="server" Width="168px" Height="130px">
						<ItemTemplate>
							Regular item.
						</ItemTemplate>
						<EditItemTemplate>
							Selected for editing!
						</EditItemTemplate>
					</asp:DataList>
				</cc1:GHTSubTest></P>
			<P>&nbsp;</P>
		</form>
	</body>
</HTML>
