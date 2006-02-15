<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="WebControl_Style.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.WebControl_Style" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>WebControl_Style</title>
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
			<cc1:GHTSubTest id="GHTSubTest24" runat="server" Height="56px" Width="104px">
				<asp:Button id="Button2" style="COLOR: firebrick" runat="server" Width="120" Height="40" Text="Button"></asp:Button>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest25" runat="server" Height="56px" Width="104px">
				<asp:CheckBox id="Checkbox2" style="COLOR: firebrick" runat="server" Width="120" Height="40"></asp:CheckBox>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest26" runat="server" Height="56px" Width="104px">
				<asp:HyperLink id="Hyperlink2" style="COLOR: firebrick" runat="server" Width="120" Height="40">HyperLink</asp:HyperLink>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest27" runat="server" Height="56px" Width="104px">
				<asp:Image id="Image2" style="COLOR: firebrick" runat="server" Width="120" Height="40"></asp:Image>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest28" runat="server" Height="56px" Width="104px">
				<asp:ImageButton id="Imagebutton2" style="COLOR: firebrick" runat="server" Width="120" Height="40"></asp:ImageButton>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest29" runat="server" Height="56px" Width="104px">
				<asp:Label id="Label2" style="COLOR: firebrick" runat="server" Width="120" Height="40">Label</asp:Label>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest30" runat="server" Height="56px" Width="104px">
				<asp:LinkButton id="Linkbutton2" style="COLOR: firebrick" runat="server" Width="120" Height="40">LinkButton</asp:LinkButton>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest31" runat="server" Height="56px" Width="104px">
				<asp:Panel id="Panel2" style="COLOR: firebrick" runat="server" Width="120" Height="40">Panel</asp:Panel>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest32" runat="server" Height="56px" Width="104px">
				<asp:RadioButton id="Radiobutton2" style="COLOR: firebrick" runat="server" Width="120" Height="40"></asp:RadioButton>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest33" runat="server" Height="56px" Width="104px">
				<asp:TextBox id="Textbox2" style="COLOR: firebrick" runat="server" Width="120" Height="40"></asp:TextBox>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest34" runat="server" Height="56px" Width="104px">
				<asp:DropDownList id="Dropdownlist2" style="COLOR: firebrick" runat="server" Width="120" Height="40">
					<asp:ListItem Value="Item1">Item1</asp:ListItem>
					<asp:ListItem Value="Item2">Item2</asp:ListItem>
				</asp:DropDownList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest35" runat="server" Height="56px" Width="104px">
				<asp:ListBox id="Listbox2" style="COLOR: firebrick" runat="server" Width="120" Height="40">
					<asp:ListItem Value="Item1">Item1</asp:ListItem>
					<asp:ListItem Value="Item2">Item2</asp:ListItem>
				</asp:ListBox>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest36" runat="server" Height="56px" Width="104px">
				<asp:RadioButtonList id="Radiobuttonlist2" style="COLOR: firebrick" runat="server" Width="120" Height="40">
					<asp:ListItem Value="Item1">Item1</asp:ListItem>
					<asp:ListItem Value="Item2">Item2</asp:ListItem>
				</asp:RadioButtonList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest37" runat="server" Height="56px" Width="104px">
				<asp:CheckBoxList id="Checkboxlist2" style="COLOR: firebrick" runat="server" Width="120" Height="40">
					<asp:ListItem Value="Item1">Item1</asp:ListItem>
					<asp:ListItem Value="Item2">Item2</asp:ListItem>
				</asp:CheckBoxList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest38" runat="server" Height="56px" Width="104px">
				<%--<asp:CompareValidator id="Comparevalidator2" runat="server" Width="120" Height="40" ControlToValidate="Listbox2"
					ErrorMessage="CompareValidator" ControlToCompare="Dropdownlist2" ValueToCompare="aaa"></asp:CompareValidator>--%>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest39" runat="server" Height="56px" Width="104px">
				<%--<asp:CustomValidator id="Customvalidator2" runat="server" Width="120" Height="40" ControlToValidate="Dropdownlist2"
					ErrorMessage="CustomValidator"></asp:CustomValidator>--%>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest40" runat="server" Height="56px" Width="104px">
				<%--<asp:RangeValidator id="Rangevalidator2" runat="server" Width="120" Height="40" ControlToValidate="Textbox2"
					ErrorMessage="RangeValidator" MaximumValue="z" MinimumValue="a"></asp:RangeValidator>--%>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest41" runat="server" Height="56px" Width="104px">
				<%--<asp:RegularExpressionValidator id="Regularexpressionvalidator2" runat="server" Width="120" Height="40" ControlToValidate="Textbox2"
					ErrorMessage="RegularExpressionValidator" ValidationExpression="(0( \d|\d ))?\d\d \d\d(\d \d| \d\d )\d\d"></asp:RegularExpressionValidator>--%>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest42" runat="server" Height="56px" Width="104px">
				<%--<asp:RequiredFieldValidator id="Requiredfieldvalidator2" runat="server" Width="120" Height="40" ControlToValidate="Textbox2"
					ErrorMessage="RequiredFieldValidator"></asp:RequiredFieldValidator>--%>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest43" runat="server" Height="56px" Width="104px">
				<%--<asp:ValidationSummary id="Validationsummary2" runat="server" Width="120" Height="40"></asp:ValidationSummary>--%>
			</cc1:GHTSubTest>;
			<cc1:GHTSubTest id="GHTSubTest44" runat="server" Height="56px" Width="104px">
				<asp:DataGrid id="Datagrid3" style="COLOR: firebrick" runat="server" Width="120" Height="40"></asp:DataGrid>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest45" runat="server" Height="56px" Width="104px">
				<asp:DataGrid id="Datagrid4" style="COLOR: firebrick" runat="server" Width="120" Height="40"></asp:DataGrid>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest46" runat="server" Height="56px" Width="104px">
				<asp:DataList id="Datalist3" style="COLOR: firebrick" runat="server" Width="120" Height="40">
					<HeaderTemplate>
						<b>
							<tr>
								<td>Items</td>
							</tr>
						</b>
					</HeaderTemplate>
					<ItemTemplate>
						<tr>
							<td><%#Container.DataItem%></td>
							<p></p>
						</tr>
					</ItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest47" runat="server" Height="56px" Width="104px">
				<asp:DataList id="Datalist4" style="COLOR: firebrick" runat="server" Width="120" Height="40">
					<HeaderTemplate>
						<b>
							<tr>
								<td>items</td>
							</tr>
						</b>
					</HeaderTemplate>
					<ItemTemplate>
						<tr>
							<td><%#Container.DataItem%></td>
							<p></p>
						</tr>
					</ItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest48" runat="server" Width="104px" Height="56px">
				<asp:Table id="Table4" style="COLOR: firebrick" runat="server" Width="120" Height="40">
					<asp:TableRow>
						<asp:TableCell Text="Header cell"></asp:TableCell>
					</asp:TableRow>
					<asp:TableRow>
						<asp:TableCell Text="Table cell"></asp:TableCell>
					</asp:TableRow>
				</asp:Table>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest49" runat="server" Height="56px" Width="104px">
				<asp:Table id="Table6" style="COLOR: firebrick" runat="server" Width="120" Height="40">
					<asp:TableRow>
						<asp:TableCell Text="Header cell"></asp:TableCell>
					</asp:TableRow>
					<asp:TableRow>
						<asp:TableCell Text="Table cell"></asp:TableCell>
					</asp:TableRow>
				</asp:Table>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest50" runat="server" Height="56px" Width="104px">
				<asp:Table id="Table7" style="COLOR: firebrick" runat="server" Width="120" Height="40">
					<asp:TableRow>
						<asp:TableCell Text="Header cell"></asp:TableCell>
					</asp:TableRow>
					<asp:TableRow>
						<asp:TableCell Text="Table cell"></asp:TableCell>
					</asp:TableRow>
				</asp:Table>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest51" runat="server" Height="56px" Width="104px">
				<asp:Table id="Table8" style="COLOR: firebrick" runat="server" Width="120" Height="40">
					<asp:TableRow>
						<asp:TableCell Text="Header cell"></asp:TableCell>
					</asp:TableRow>
					<asp:TableRow>
						<asp:TableCell Text="Table cell"></asp:TableCell>
					</asp:TableRow>
				</asp:Table>
			</cc1:GHTSubTest></form>
	</body>
</HTML>
