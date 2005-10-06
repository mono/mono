<%@ Page Language="C#" AutoEventWireup="True" %>
 <html>
 <head>
	<script runat="server">
	void ButtonClick(Object sender, EventArgs e)
	{
	    if (Page.IsValid)
	    {
	        Label1.Text="Page is valid.";
	    }
	    else
	    {
	        Label1.Text="Page is not valid!!";
	    }
	}
	</script>
 </head>

<body>
  <h3>RangeValidator render tests</h3>

  <form runat="server">
        <asp:TextBox id="TextBox1" runat="server"/>

	<asp:RangeValidator id="RA_disabled"
                ControlToValidate="TextBox1"
                MinimumValue="1"
                MaximumValue="10"
	        Type="Integer"
		Enabled="False"
                ErrorMessage="Your value isn't within min/max"
		EnableClientScript="False"
                runat="server"/>

	<asp:Label id="Label1" runat="server"/>

	<asp:Button id="Submit" Text="Submit" OnClick="ButtonClick" runat="server"/>
  </form>


<script Language="JavaScript">
    var TestFixture = {
	RA_disabled_success_pre: function () {
	    JSUnit_BindElement ("RA_disabled");
	    JSUnit_TestCausesPageLoad ();

	    var textbox = JSUnit_GetElement ("TextBox1");
	    var submit = JSUnit_GetElement ("Submit");

	    textbox.value = "14";

	    JSUnit_Click (submit)
	},

	RA_disabled_success_post: function () {
	    JSUnit_BindElement ("Label1");

	    Assert.AreEqual ("Page is valid.", "JSUnit_GetElement ().innerHTML", "innerHTML");
	}

    };

</script>

</body>

</html>