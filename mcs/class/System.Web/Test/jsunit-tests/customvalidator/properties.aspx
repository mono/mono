<%@ Page Language="C#" AutoEventWireup="True" %>
<body>
  <h3>CustomValidator render tests</h3>

  <form runat="server">
        <asp:TextBox id="TextBox1" 
              runat="server"/>

	<asp:CustomValidator id="CV_noattributes"
		ControlToValidate="TextBox1"
		runat="server"/>

 	<!-- this control should render as a single &nbsp;
	      unfortunately there's no way to test this except
	      through looking at the source, so we just test that the
	      control doesn't show up in the resulting html. -->

	<asp:CustomValidator id="CV_static_downlevel"
                ControlToValidate="TextBox1"
                Display="Static"
                ErrorMessage="Your value isn't within min/max"
                EnableClientScript="False" 
                runat="server"/>

	<!-- a dynamic uplevel validator.  -->
	<asp:CustomValidator id="CV_dynamic_uplevel"
                ControlToValidate="TextBox1"
		Display="dynamic"
		ClientValidationFunction="myClientValidationFunction"
                ErrorMessage="Your value isn't 'Chris Toshok'"
		runat="server"/>

	<asp:Button id="submit" text="Submit"/>
  </form>

<script Language="JavaScript">

    var TestFixture = {
	CV_noattributes : function () {
	    JSUnit_BindElement ("CV_noattributes");

	    Assert.NotNull ("JSUnit_GetElement ()", "exists");

	    Assert.AttributeHasValue ("TextBox1", "controltovalidate", "controltovalidate");
	    Assert.IsNull ("JSUnit_GetAttribute ('clientvalidationfunction')", "clientvalidationfunction");
	    Assert.IsFunction ("JSUnit_GetAttribute ('evaluationfunction')", "evaluationfunction");
	    Assert.AreEqualCase ("red", "JSUnit_GetAttribute ('style')['color']", "color style");
	    Assert.AreEqualCase ("hidden", "JSUnit_GetAttribute ('style')['visibility']", "visibility style");
	},

	CV_static_downlevel: function () {
	    JSUnit_BindElement ("CV_static_downlevel");

	    Assert.IsNull ("JSUnit_GetElement ()", "does not exist");
	},
	CV_dynamic_uplevel: function () {
	    JSUnit_BindElement ("CV_dynamic_uplevel");

	    Assert.NotNull ("JSUnit_GetElement ()", "exists");

	    Assert.AttributeHasValue ("TextBox1", "controltovalidate", "controltovalidate");
	    Assert.AreEqual ("myClientValidationFunction", "JSUnit_GetAttribute ('clientvalidationfunction')", "clientvalidationfunction");
	    Assert.IsFunction ("JSUnit_GetAttribute ('evaluationfunction')", "evaluationfunction");
	    Assert.AttributeHasValue ("Dynamic", "display", "display");
	    Assert.AttributeHasValue ("Your value isn't 'Chris Toshok'", "errormessage", "errormessage");
	    Assert.AreEqualCase ("red", "JSUnit_GetAttribute ('style')['color']", "color style");
	    Assert.AreEqualCase ("none", "JSUnit_GetAttribute ('style')['display']", "display style");
	    Assert.AreEqual ("Your value isn't 'Chris Toshok'", "JSUnit_GetElement ().innerHTML", "innerHTML");
	}
    };

</script>

</body>

</html>