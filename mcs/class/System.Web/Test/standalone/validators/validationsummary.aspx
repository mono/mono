<html>
<body>

<form runat="server">
	<asp:DropDownList id="DropDownList1" runat="server"> 
		  <asp:ListItem Value="">Please Choose</asp:ListItem> 
		  <asp:ListItem Value="Bleep">Bleep</asp:ListItem> 
		  <asp:ListItem Value="Blob">Blob</asp:ListItem> 
		  <asp:ListItem Value="Blah">Blah</asp:ListItem> 
	</asp:DropDownList> 

	<asp:RequiredFieldValidator 
		id="RequiredFieldValidator1" 
		runat="server" 
		ErrorMessage="Please select an item in the DropDownList" 
		ControlToValidate="DropDownList1" 
		Display="None">
	</asp:RequiredFieldValidator> 

	<asp:DropDownList id="DropDownList2" runat="server"> 
		  <asp:ListItem Value="">Please Choose A Second Item</asp:ListItem> 
		  <asp:ListItem Value="Boing">Boing</asp:ListItem> 
		  <asp:ListItem Value="Bump">Bump</asp:ListItem> 
		  <asp:ListItem Value="Bing">Bing</asp:ListItem> 
	</asp:DropDownList> 

	<asp:RequiredFieldValidator 
		id="RequiredFieldValidator2" 
		runat="server" 
		ErrorMessage="Please select an item in the second DropDownList" 
		ControlToValidate="DropDownList2" 
		Display="None">
	</asp:RequiredFieldValidator> 

	<asp:Button 
		id="Button1" 
		runat="server" 
		Text="Button">
	</asp:Button> 

	<asp:ValidationSummary 
		id="ValidationSummary1" 
		runat="server" 
		ShowMessageBox="True" 
		ShowSummary="False">
	</asp:ValidationSummary> 
</form>
</body>
</html>

