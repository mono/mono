<%@ Page Language="C#" Debug="true" %>
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
		ShowMessageBox="False"
		ShowSummary="True">
	</asp:ValidationSummary> 
</form>


<script Language="JavaScript">
    var TestFixture = {
	/* tests for the EnableClientScript=true, summary=true, messagebox=false mode */
        VS_summary_bullet_test: function () {
	    JSUnit_BindElement ("ValidationSummary1");

	    var summary = JSUnit_GetElement();
	    var dropdown1 = JSUnit_GetElement ("DropDownList1");
	    var dropdown2 = JSUnit_GetElement ("DropDownList2");
	    var submit = JSUnit_GetElement ("Button1");

	    summary.setAttribute ("displaymode", "Bulleted");
	    dropdown1.selectedIndex = 0;
	    dropdown2.selectedIndex = 0;

	    JSUnit_Click (submit);
	    
	    Assert.AreEqualCase ("<ul>\n<li>Please select an item in the DropDownList</li>\n<li>Please select an item in the second DropDownList</li></ul>",
				 "JSUnit_GetElement().innerHTML", "validation summary inner html");
	},

        VS_summary_list_test: function () {
	    JSUnit_BindElement ("ValidationSummary1");

	    var summary = JSUnit_GetElement();
	    var dropdown1 = JSUnit_GetElement ("DropDownList1");
	    var dropdown2 = JSUnit_GetElement ("DropDownList2");
	    var submit = JSUnit_GetElement ("Button1");

	    summary.setAttribute ("displaymode", "List");
	    dropdown1.selectedIndex = 0;
	    dropdown2.selectedIndex = 0;

	    JSUnit_Click (submit);

	    Assert.AreEqualCase ("Please select an item in the DropDownList<br>Please select an item in the second DropDownList<br>",
				 "JSUnit_GetElement().innerHTML", "validation summary inner html");
	},

        VS_summary_paragraph_test: function () {
	    JSUnit_BindElement ("ValidationSummary1");

	    var summary = JSUnit_GetElement();
	    var dropdown1 = JSUnit_GetElement ("DropDownList1");
	    var dropdown2 = JSUnit_GetElement ("DropDownList2");
	    var submit = JSUnit_GetElement ("Button1");

	    summary.setAttribute ("displaymode", "SingleParagraph");
	    dropdown1.selectedIndex = 0;
	    dropdown2.selectedIndex = 0;

	    JSUnit_Click (submit);

	    Assert.AreEqualCase ("Please select an item in the DropDownList Please select an item in the second DropDownList <br>",
				 "JSUnit_GetElement().innerHTML", "validation summary inner html");
	}
    };

</script>

</body>
</html>
