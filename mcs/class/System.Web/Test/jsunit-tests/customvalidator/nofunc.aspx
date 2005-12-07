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
  <h3>test for bug #76087</h3>

  <form runat="server">
        <asp:TextBox id="TextBox1" 
              runat="server"/>

	<!-- a dynamic uplevel validator.  -->
	<asp:CustomValidator id="CV_dynamic_uplevel"
                ControlToValidate="TextBox1"
		Display="dynamic"
		runat="server"/>*</asp:CustomValidator>

	<asp:Label id="Label1"
           runat="server"/>

	<asp:Button id="Submit" Text="Submit" OnClick="ButtonClick" runat="server"/>
  </form>


<script Language="JavaScript">
    var TestFixture = {
	CV_nofunc_pre: function () {
	    JSUnit_TestCausesPageLoad ();

	    var submit = JSUnit_GetElement ("Submit");

	    JSUnit_Click (submit)
	},

	CV_nofunc_post: function () {
	    JSUnit_BindElement ("Label1");

	    Assert.AreEqual ("Page is valid.", "JSUnit_GetElement ().innerHTML", "innerHTML");
	}

    };

</script>

</body>

</html>