<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="Repeater_DataBind_.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.Repeater_DataBind_" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>Repeater_DataBind_</title>
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
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Width="553px" Height="96px">
					<asp:Repeater id="Repeater1" runat="server">
						<ItemTemplate>
							<div><%#Container.DataItem%></div>
							<br>
						</ItemTemplate>
					</asp:Repeater>
				</cc1:GHTSubTest>
				<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="96px" Width="553px">
					<asp:Repeater id="Repeater2" runat="server">
						<ItemTemplate>
							<div><%#Container.DataItem%></div>
							<br>
						</ItemTemplate>
					</asp:Repeater>
				</cc1:GHTSubTest>
				<cc1:GHTSubTest id="GHTSubTest3" runat="server" Height="96px" Width="553px">
					<asp:Repeater id="Repeater3" runat="server">
						<ItemTemplate>
							<div><%#DataBinder.Eval(Container.DataItem, "number")%></div>
							<div><%#DataBinder.Eval(Container.DataItem, "number_up")%></div>
							<div><%#DataBinder.Eval(Container.DataItem, "number_number")%></div>
							<br>
						</ItemTemplate>
					</asp:Repeater>
				</cc1:GHTSubTest>
				<cc1:GHTSubTest id="GHTSubTest4" runat="server" Height="96px" Width="553px">
					<asp:Repeater id="Repeater4" runat="server">
						<ItemTemplate>
							<div><%#DataBinder.Eval(Container.DataItem, "id")%></div>
							<div><%#DataBinder.Eval(Container.DataItem, "name")%></div>
							<div><%#DataBinder.Eval(Container.DataItem, "company")%></div>
							<br>
						</ItemTemplate>
					</asp:Repeater>
				</cc1:GHTSubTest>&nbsp;
				<cc1:GHTSubTest id="GHTSubTest5" runat="server" Height="96px" Width="553px">
					<asp:Repeater id="Repeater5" runat="server">
						<ItemTemplate>
							<div><%#DataBinder.Eval(Container.DataItem, "id")%></div>
							<div><%#DataBinder.Eval(Container.DataItem, "name")%></div>
							<div><%#DataBinder.Eval(Container.DataItem, "company")%></div>
							<br>
						</ItemTemplate>
					</asp:Repeater>
				</cc1:GHTSubTest>
				<br>
				<br>
				<cc1:GHTSubTest id="GHTSubTest6" runat="server" Height="96px" Width="553px">
					<asp:Repeater id="Repeater6" runat="server">
						<ItemTemplate>
							<div><%#DataBinder.Eval(Container.DataItem, "color")%></div>
							<div><%#DataBinder.Eval(Container.DataItem, "colorObj")%></div>
							<br>
						</ItemTemplate>
					</asp:Repeater>
				</cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest7" runat="server" Height="96px" Width="553px">
					<asp:Repeater id="Repeater7" runat="server"></asp:Repeater>
				</cc1:GHTSubTest></P>
		</form>
	</body>
</HTML>
