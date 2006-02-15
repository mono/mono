<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="WebControl_Attributes.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.WebControl_Attributes" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>WebControl_Attributes</title>
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
			<cc1:GHTSubTest id="GHTSubTest25" runat="server" Width="104px" Height="56px">
				<asp:Button id="Button1" runat="server" TestAttribute="TestValue" Text="Button"></asp:Button>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest26" runat="server" Width="104px" Height="56px">
				<asp:CheckBox id="CheckBox1" runat="server" TestAttribute="TestValue"></asp:CheckBox>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest28" runat="server" Width="104px" Height="56px">
				<asp:HyperLink id="HyperLink1" runat="server" TestAttribute="TestValue">HyperLink</asp:HyperLink>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest30" runat="server" Width="104px" Height="56px">
				<asp:Image id="Image1" runat="server" TestAttribute="TestValue"></asp:Image>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest32" runat="server" Width="104px" Height="56px">
				<asp:ImageButton id="ImageButton1" runat="server" TestAttribute="TestValue"></asp:ImageButton>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest34" runat="server" Width="104px" Height="56px">
				<asp:Label id="Label1" runat="server" TestAttribute="TestValue">Label</asp:Label>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest36" runat="server" Width="104px" Height="56px">
				<asp:LinkButton id="LinkButton1" runat="server" TestAttribute="TestValue">LinkButton</asp:LinkButton>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest37" runat="server" Width="104px" Height="56px">
				<asp:Panel id="Panel1" runat="server" TestAttribute="TestValue">Panel</asp:Panel>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest38" runat="server" Width="104px" Height="56px">
				<asp:RadioButton id="RadioButton1" runat="server" TestAttribute="TestValue"></asp:RadioButton>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest39" runat="server" Width="104px" Height="56px">
				<asp:TextBox id="TextBox1" runat="server" Width="100px" TestAttribute="TestValue"></asp:TextBox>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest40" runat="server" Width="104px" Height="56px">
				<asp:DropDownList id="DropDownList1" runat="server" TestAttribute="TestValue">
					<asp:ListItem Value="Item1">Item1</asp:ListItem>
					<asp:ListItem Value="Item2">Item2</asp:ListItem>
				</asp:DropDownList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest41" runat="server" Width="104px" Height="56px">
				<asp:ListBox id="ListBox1" runat="server" TestAttribute="TestValue">
					<asp:ListItem Value="Item1">Item1</asp:ListItem>
					<asp:ListItem Value="Item2">Item2</asp:ListItem>
				</asp:ListBox>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest42" runat="server" Width="104px" Height="56px">
				<asp:RadioButtonList id="RadioButtonList1" runat="server" TestAttribute="TestValue">
					<asp:ListItem Value="Item1">Item1</asp:ListItem>
					<asp:ListItem Value="Item2">Item2</asp:ListItem>
				</asp:RadioButtonList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest43" runat="server" Width="104px" Height="56px">
				<asp:CheckBoxList id="CheckBoxList1" runat="server" TestAttribute="TestValue">
					<asp:ListItem Value="Item1">Item1</asp:ListItem>
					<asp:ListItem Value="Item2">Item2</asp:ListItem>
				</asp:CheckBoxList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHT_SubTest44" runat="server" Width="104px" Height="56px">
				<asp:CompareValidator id="CompareValidator1" runat="server" TestAttribute="TestValue" ControlToValidate="TextBox1"
					ErrorMessage="CompareValidator" ControlToCompare="ListBox1" ValueToCompare="aaa"></asp:CompareValidator>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHT_SubTest45" runat="server" Width="104px" Height="56px">
				<asp:CustomValidator id="CustomValidator1" runat="server" TestAttribute="TestValue" ControlToValidate="TextBox1"
					ErrorMessage="CustomValidator"></asp:CustomValidator>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHT_SubTest46" runat="server" Width="104px" Height="56px">
				<asp:RangeValidator id="RangeValidator1" runat="server" TestAttribute="TestValue" ControlToValidate="TextBox1"
					ErrorMessage="RangeValidator" MaximumValue="z" MinimumValue="a"></asp:RangeValidator>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHT_SubTest47" runat="server" Width="104px" Height="56px">
				<asp:RegularExpressionValidator id="RegularExpressionValidator1" runat="server" TestAttribute="TestValue" ControlToValidate="TextBox1"
					ErrorMessage="RegularExpressionValidator" ValidationExpression="(0( \d|\d ))?\d\d \d\d(\d \d| \d\d )\d\d"></asp:RegularExpressionValidator>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHT_SubTest48" runat="server" Width="104px" Height="56px">
				<asp:RequiredFieldValidator id="RequiredFieldValidator1" runat="server" TestAttribute="TestValue" ControlToValidate="TextBox1"
					ErrorMessage="RequiredFieldValidator"></asp:RequiredFieldValidator>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHT_SubTest49" runat="server" Width="104px" Height="56px">
				<asp:ValidationSummary id="ValidationSummary1" runat="server" Height="37px" Width="144px" TestAttribute="TestValue"></asp:ValidationSummary>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest50" runat="server" Width="104px" Height="56px">
				<asp:DataGrid id="DataGrid1" runat="server" TestAttribute="TestValue"></asp:DataGrid>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest51" runat="server" Width="104px" Height="56px">
				<asp:DataGrid id="DataGrid2" runat="server">
					<ItemStyle></ItemStyle>
				</asp:DataGrid>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest52" runat="server" Width="104px" Height="56px">
				<asp:DataList id="DataList1" runat="server" TestAttribute="TestValue">
					<HeaderTemplate>
						<b>
							<tr>
								<td>Id</td>
								<td>Description</td>
							</tr>
						</b>
					</HeaderTemplate>
					<ItemTemplate>
						<tr>
							<td><%#DataBinder.Eval(Container.DataItem, "Id") %></td>
							<td><%#DataBinder.Eval(Container.DataItem, "Description") %></td>
							<p></p>
						</tr>
					</ItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest53" runat="server" Width="104px" Height="56px">
				<asp:DataList id="DataList2" runat="server">
					<HeaderTemplate>
						<b>
							<tr>
								<td>Id</td>
								<td>Description</td>
							</tr>
						</b>
					</HeaderTemplate>
					<ItemTemplate>
						<tr TestAttribute="TestValue">
							<td><%#DataBinder.Eval(Container.DataItem, "Id") %></td>
							<td><%#DataBinder.Eval(Container.DataItem, "Description") %></td>
							<p></p>
						</tr>
					</ItemTemplate>
				</asp:DataList>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest54" runat="server" Width="104px" Height="56px">
				<asp:Table id="Table1" runat="server" TestAttribute="TestValue">
					<asp:TableRow>
						<asp:TableCell Text="Header cell"></asp:TableCell>
					</asp:TableRow>
					<asp:TableRow>
						<asp:TableCell Text="Table cell"></asp:TableCell>
					</asp:TableRow>
				</asp:Table>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest55" runat="server" Width="104px" Height="56px">
				<asp:Table id="Table5" runat="server">
					<asp:TableRow>
						<asp:TableCell Text="Header cell"></asp:TableCell>
					</asp:TableRow>
					<asp:TableRow>
						<asp:TableCell Text="Table cell" TestAttribute="TestValue"></asp:TableCell>
					</asp:TableRow>
				</asp:Table>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest56" runat="server" Width="104px" Height="56px">
				<asp:Table id="Table2" runat="server">
					<asp:TableRow>
						<asp:TableCell Text="Header cell" TestAttribute="TestValue"></asp:TableCell>
					</asp:TableRow>
					<asp:TableRow>
						<asp:TableCell Text="Table cell"></asp:TableCell>
					</asp:TableRow>
				</asp:Table>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest57" runat="server" Width="104px" Height="56px">
				<asp:Table id="Table3" runat="server">
					<asp:TableRow>
						<asp:TableCell Text="Header cell"></asp:TableCell>
					</asp:TableRow>
					<asp:TableRow TestAttribute="TestValue">
						<asp:TableCell Text="Table cell"></asp:TableCell>
					</asp:TableRow>
				</asp:Table>
			</cc1:GHTSubTest></form>
	</body>
</HTML>
