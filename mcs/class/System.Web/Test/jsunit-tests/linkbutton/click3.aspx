<%@ Page Language="C#" Debug="true" %>
<script runat="server">
	void Click (object sender, EventArgs e)
	{
		label2.Text = button.Text;
		button.Text = "HEY";
	}
</script>

<html>
<body>
	<p>should be blank after you click.
	<asp:Label runat="server" id="label2" />
	

	<p>Should say ABCD EFG HIJK before you click. Should say "HEY" after you click<p>
	<form runat="server">
	      <asp:LinkButton id="button" OnClick="Click" runat="server">
			ABCD <asp:Label runat="server" id="label3" Text="EFG"/> HIJK
	      </asp:LinkButton> 

	      <asp:LinkButton id="button2" OnClick="Click" runat="server"> button2 </asp:LinkButton> 
	</form>


<script Language="JavaScript">
    var TestFixture = {
	LB_click3_pre: function () {
	    JSUnit_BindElement ("button");
	    JSUnit_TestCausesPageLoad ();

	    var linkbutton = JSUnit_GetElement ();

	    /* IE fails this one because it capitalizes SPAN (which
	    AreEqualCase fixes), but it also leaves out the quotes
	    around "label3".. but only in the innerHTML property, not
	    in the actual html.  weird. */
	    JSUnit_ExpectFailure ("IE fails to quote the id attribute properly");
	    Assert.AreEqualCase ("ABCD <span id=\"label3\">EFG</span> HIJK", "JSUnit_GetElement ().innerHTML", "linkbutton inner html");

	    Assert.AreEqual ("", "JSUnit_GetElement ('label2').innerHTML", "label2 inner html");

	    JSUnit_Click (linkbutton);
	},


	LB_click3_post: function () {
	    JSUnit_BindElement ("button");

	    JSUnit_TestCausesPageLoad ();

	    Assert.AreEqual ("HEY", "JSUnit_GetElement ('button').innerHTML", "linkbutton inner html");
	    Assert.AreEqual ("", "JSUnit_GetElement ('label2').innerHTML", "label2 inner html");

	    var linkbutton = JSUnit_GetElement ();

	    JSUnit_Click (linkbutton);
	},

	LB_click3_postpost: function () {
	    JSUnit_BindElement ("button");

	    Trace.debug("made it to LB_click3_postpost");

	    Assert.AreEqual ("HEY", "JSUnit_GetElement ().innerHTML", "linkbutton inner html");
	    Assert.AreEqual ("HEY", "JSUnit_GetElement ('label2').innerHTML", "label2 inner html");
	}
    };

</script>

</body>
</html>
