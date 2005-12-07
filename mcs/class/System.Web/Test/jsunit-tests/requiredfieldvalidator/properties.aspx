<%@ Page Language="C#" AutoEventWireup="True" %>
 <html>
 <head>
 </head>

<body>
  <h3>RequiredFieldValidator render tests</h3>

  <form runat="server">
         <asp:TextBox id="TextBox1" 
              runat="server"/>

	 <asp:RequiredFieldValidator id="RF_noattributes"
		ControlToValidate="TextBox1"
		runat="server"/>

 	 <!-- this control should render as a single &nbsp;
	      unfortunately there's no way to test this except
	      through looking at the source. -->

	 <asp:RequiredFieldValidator id="RF_static_downlevel"
                ControlToValidate="TextBox1"
                InitialValue=""
                Display="Static"
                ErrorMessage="You must supply a value"
                EnableClientScript="False" 
                runat="server"/>

	<!-- just a validator with a non-default initial value -->
	 <asp:RequiredFieldValidator id="RF_initialvalue"
		ControlToValidate="TextBox1"
		InitialValue="hi there"
		runat="server"/>

	<!-- a dynamic uplevel validator.  -->
	 <asp:RequiredFieldValidator id="RF_dynamic_uplevel"
                ControlToValidate="TextBox1"
                InitialValue=""
                Display="Dynamic"
                ErrorMessage="You must supply a value"
                runat="server"/>

  </form>

<script Language="JavaScript">

    var TestFixture = {
	RF_noattributes : function () {
	    JSUnit_BindElement ("RF_noattributes");

	    Assert.NotNull ("JSUnit_GetElement ()", "exists");

	    Assert.AttributeHasValue ("TextBox1", "controltovalidate", "controltovalidate");
	    Assert.IsFunction ("JSUnit_GetAttribute ('evaluationfunction')", "evaluationfunction");
	    Assert.AttributeHasValue ("", "initialvalue", "initialvalue");
	    Assert.AreEqualCase ("red", "JSUnit_GetAttribute ('style')['color']", "color style");
	    Assert.AreEqualCase ("hidden", "JSUnit_GetAttribute ('style')['visibility']", "visibility style");
	},

	RF_initialvalue: function () {
	    JSUnit_BindElement ("RF_initialvalue");

	    Assert.AttributeHasValue ("hi there", "initialvalue", "initialvalue");
	},

	RF_static_downlevel: function () {
	    JSUnit_BindElement ("RF_static_downlevel");

	    Assert.IsNull ("JSUnit_GetElement ()", "does not exist");
	},

	RF_dynamic_uplevel: function () {
	    JSUnit_BindElement ("RF_dynamic_uplevel");

	    Assert.NotNull ("JSUnit_GetElement ()", "exists");

	    Assert.AttributeHasValue ("TextBox1", "controltovalidate", "controltovalidate");
	    Assert.IsFunction ("JSUnit_GetAttribute ('evaluationfunction')", "evaluationfunction");
	    Assert.AttributeHasValue ("", "initialvalue", "initialvalue");
	    Assert.AttributeHasValue ("Dynamic", "display", "display");
	    Assert.AttributeHasValue ("You must supply a value", "errormessage", "errormessage");
	    Assert.AreEqualCase ("red", "JSUnit_GetAttribute ('style')['color']", "color style");
	    Assert.AreEqualCase ("none", "JSUnit_GetAttribute ('style')['display']", "display style");
	    Assert.AreEqual ("You must supply a value", "JSUnit_GetElement ().innerHTML", "innerHTML");
	}
    };

</script>

</body>

</html>