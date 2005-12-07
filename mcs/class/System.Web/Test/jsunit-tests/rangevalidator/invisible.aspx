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
        <asp:TextBox id="TextBox1" 
              runat="server"/>

	<!-- an invisible validator.  -->
	<asp:RangeValidator id="RA_invisible"
                ControlToValidate="TextBox1"
                MinimumValue="1"
                MaximumValue="10"
	        Type="Integer"
                Display="Dynamic"
		Visible="False"
                ErrorMessage="Your value isn't within min/max"
                runat="server"/>

	<asp:Label id="Label1"
           runat="server"/>

	<asp:Button id="Submit" Text="Submit" OnClick="ButtonClick" runat="server"/>
  </form>


<script Language="JavaScript">
    var TestFixture = {
	RA_invisible_success_pre: function () {
	    JSUnit_BindElement ("RA_invisible");

	    Assert.IsNull ("JSUnit_GetElement()", "invisible validator doesn't exist on page");

	    JSUnit_TestCausesPageLoad ();

	    var textbox = JSUnit_GetElement ("TextBox1");
	    var submit = JSUnit_GetElement ("Submit");

	    textbox.value = "14";

	    JSUnit_Click (submit)
	},

	RA_invisible_success_post: function () {
	    JSUnit_BindElement ("Label1");

	    Assert.AreEqual ("Page is valid.", "JSUnit_GetElement ().innerHTML", "innerHTML");
	}

    };

</script>

</body>

</html>