<%@ Page Language="C#" AutoEventWireup="True" %>
 <html>
 <head>
 </head>

<body>
  <h3>RangeValidator render tests</h3>

  <form runat="server">
        <asp:TextBox id="TextBox1" 
              runat="server"/>

	<asp:RangeValidator id="RA_noattributes"
		ControlToValidate="TextBox1"
		runat="server"/>

 	<!-- this control should render as a single &nbsp;
	      unfortunately there's no way to test this except
	      through looking at the source. -->

	<asp:RangeValidator id="RA_static_downlevel"
                ControlToValidate="TextBox1"
                Display="Static"
                ErrorMessage="Your value isn't within min/max"
                EnableClientScript="False" 
                runat="server"/>

	<!-- just a validator with a non-default Min/Max values -->
	<asp:RangeValidator id="RA_minmax"
		ControlToValidate="TextBox1"
	        Type="Integer"
		MinimumValue="1"
		MaximumValue="10"
		runat="server"/>

	<!-- a dynamic uplevel validator.  -->
	<asp:RangeValidator id="RA_dynamic_uplevel"
                ControlToValidate="TextBox1"
	        Type="Integer"
                MinimumValue="1"
                MaximumValue="10"
                Display="Dynamic"
                ErrorMessage="Your value isn't within min/max"
                runat="server"/>

	<asp:Label id="Label1"
           runat="server"/>
  </form>


<script Language="JavaScript">
    var TestFixture = {
	RA_noattributes : function () {
	    JSUnit_BindElement ("RA_noattributes");

	    Assert.NotNull ("JSUnit_GetElement ()", "exists");

	    Assert.AttributeHasValue ("TextBox1", "controltovalidate", "controltovalidate");
	    Assert.IsFunction ("JSUnit_GetAttribute ('evaluationfunction')", "evaluationfunction");
	    Assert.AttributeHasValue ("", "minimumvalue", "minimumvalue");
	    Assert.AttributeHasValue ("", "maximumvalue", "maximumvalue");
	    Assert.AreEqualCase ("red", "JSUnit_GetAttribute ('style')['color']", "color style");
	    Assert.AreEqualCase ("hidden", "JSUnit_GetAttribute ('style')['visibility']", "visibility style");
	},

	RA_minmax: function () {
	    JSUnit_BindElement ("RA_minmax");

	    Assert.AttributeHasValue ("1", "minimumvalue", "minimumvalue");
	    Assert.AttributeHasValue ("10", "maximumvalue", "maximumvalue");
	},

	RA_static_downlevel: function () {
	    JSUnit_BindElement ("RA_static_downlevel");

	    Assert.IsNull ("JSUnit_GetElement ()", "does not exist");
	},

	RA_dynamic_uplevel: function () {
	    JSUnit_BindElement ("RA_dynamic_uplevel");

	    Assert.NotNull ("JSUnit_GetElement ()", "exists");

	    Assert.AttributeHasValue ("TextBox1", "controltovalidate", "controltovalidate");
	    Assert.IsFunction ("JSUnit_GetAttribute ('evaluationfunction')", "evaluationfunction");
	    Assert.AttributeHasValue ("1", "minimumvalue", "minimumvalue");
	    Assert.AttributeHasValue ("10", "maximumvalue", "maximumvalue");
	    Assert.AttributeHasValue ("Dynamic", "display", "display");
	    Assert.AttributeHasValue ("Your value isn't within min/max", "errormessage", "errormessage");
	    Assert.AreEqualCase ("red", "JSUnit_GetAttribute ('style')['color']", "color style");
	    Assert.AreEqualCase ("none", "JSUnit_GetAttribute ('style')['display']", "display style");
	    Assert.AreEqual ("Your value isn't within min/max", "JSUnit_GetElement ().innerHTML", "innerHTML");
	}
    };

</script>

</body>

</html>