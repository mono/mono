<%@ Page Language="C#" AutoEventWireup="True" %>
 <html>
 <head>
 </head>

<body>
  <h3>RegularExpressionValidator render tests</h3>

  <form runat="server">
         <asp:TextBox id="TextBox1" 
              runat="server"/>

	 <asp:RegularExpressionValidator id="RE_noattributes"
		ControlToValidate="TextBox1"
		runat="server"/>

 	 <!-- this control should render as a single &nbsp;
	      unfortunately there's no way to test this except
	      through looking at the source. -->

	 <asp:RegularExpressionValidator id="RE_static_downlevel"
                ControlToValidate="TextBox1"
                Display="Static"
                ErrorMessage="Your value doesn't match the expected results"
                EnableClientScript="False" 
                runat="server"/>

	<!-- just a validator with a non-default ValidationExpression -->
	 <asp:RegularExpressionValidator id="RE_validationexpression"
		ControlToValidate="TextBox1"
		ValidationExpression="\d{5}"
		runat="server"/>

	<!-- a dynamic uplevel validator.  -->
	 <asp:RegularExpressionValidator id="RE_dynamic_uplevel"
                ControlToValidate="TextBox1"
                ValidationExpression="\d{5}"
                Display="Dynamic"
                ErrorMessage="Your value doesn't match the expected results"
                runat="server"/>

  </form>


<script Language="JavaScript">

    var TestFixture = {
	RE_noattributes : function () {
	    JSUnit_BindElement ("RE_noattributes");

	    Assert.NotNull ("JSUnit_GetElement ()", "exists");

	    Assert.AttributeHasValue ("TextBox1", "controltovalidate", "controltovalidate");
	    Assert.IsFunction ("JSUnit_GetAttribute ('evaluationfunction')", "evaluationfunction");
	    Assert.AttributeHasValue (null, "validationexpression", "validationexpression");
	    Assert.AreEqualCase ("red", "JSUnit_GetAttribute ('style')['color']", "color style");
	    Assert.AreEqualCase ("hidden", "JSUnit_GetAttribute ('style')['visibility']", "visibility style");
	},

	RE_validationexpression: function () {
	    JSUnit_BindElement ("RE_validationexpression");

	    Assert.AttributeHasValue ("\\d{5}", "validationexpression", "validationexpression");
	},

	RE_static_downlevel: function () {
	    JSUnit_BindElement ("RE_static_downlevel");

	    Assert.IsNull ("JSUnit_GetElement ()", "does not exist");
	},

	RE_dynamic_uplevel: function () {
	    JSUnit_BindElement ("RE_dynamic_uplevel");

	    Assert.NotNull ("JSUnit_GetElement ()", "exists");

	    Assert.AttributeHasValue ("TextBox1", "controltovalidate", "controltovalidate");
	    Assert.IsFunction ("JSUnit_GetAttribute ('evaluationfunction')", "evaluationfunction");
	    Assert.AttributeHasValue ("\\d{5}", "validationexpression", "validationexpression");
	    Assert.AttributeHasValue ("Dynamic", "display", "display");
	    Assert.AttributeHasValue ("Your value doesn't match the expected results", "errormessage", "errormessage");
	    Assert.AreEqualCase ("red", "JSUnit_GetAttribute ('style')['color']", "color style");
	    Assert.AreEqualCase ("none", "JSUnit_GetAttribute ('style')['display']", "display style");
	    Assert.AreEqual ("Your value doesn't match the expected results", "JSUnit_GetElement ().innerHTML", "innerHTML");
	}
    };

</script>

</body>

</html>