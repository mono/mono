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

	<!-- a dynamic uplevel validator.  -->
	<asp:RangeValidator id="RA_dynamic_uplevel"
                ControlToValidate="TextBox1"
                MinimumValue="1"
                MaximumValue="10"
	        Type="Integer"
                Display="Dynamic"
                ErrorMessage="Your value isn't within min/max"
                runat="server"/>

	<asp:Label id="Label1"
           runat="server"/>

	<asp:Button id="Submit" Text="Submit" OnClick="ButtonClick" runat="server"/>
  </form>


<script Language="JavaScript">
    var TestFixture = {
	RA_dynamic_failure: function () {
	    JSUnit_BindElement ("RA_dynamic_uplevel");

	    var textbox = JSUnit_GetElement ("TextBox1");
	    var submit = JSUnit_GetElement ("Submit");

	    textbox.value = "14";

	    /* this doesn't cause a page load so we're fine */
	    JSUnit_Click(submit);

	    Assert.AreEqualCase ("inline", "JSUnit_GetAttribute ('style')['display']", "display style");
	    Assert.AreEqual ("Your value isn't within min/max", "JSUnit_GetElement ().innerHTML", "innerHTML");
	},

	RA_dynamic_success_pre: function () {
	    JSUnit_BindElement ("RA_dynamic_uplevel");
	    JSUnit_TestCausesPageLoad ();

	    var textbox = JSUnit_GetElement ("TextBox1");
	    var submit = JSUnit_GetElement ("Submit");

	    textbox.value = "8";

	    JSUnit_Click (submit)
	},

	RA_dynamic_success_post: function () {
	    JSUnit_BindElement ("Label1");

	    Assert.AreEqual ("Page is valid.", "JSUnit_GetElement ().innerHTML", "innerHTML");
	}

    };

</script>

</body>

</html>