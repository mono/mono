<%@ Page Language="C#" Debug="true" %>
<script runat="server">
	void Command (object sender, CommandEventArgs e) 
	{
		if (e.CommandName == "ONE" && e.CommandArgument.Equals ("TWO"))
			label.Text = "PASS";
	}

	void Click (object sender, EventArgs e)
	{
		label2.Text = "PASS";
	}
</script>

<html>
<body>
	<p>Command event
	<asp:Label runat="server" id="label" />
	<p>Click event
	<asp:Label runat="server" id="label2" />
	<form runat="server">
	      <asp:LinkButton id="lb1" Text="Push me!" CommandName="ONE" CommandArgument="TWO" OnCommand="Command" OnClick="Click" runat="server"/> 
	</form>

<script Language="JavaScript">
    var TestFixture = {
	LB_click2_pre: function () {
	    JSUnit_TestCausesPageLoad ();

	    var label1 = JSUnit_GetElement ("label");
	    var label2 = JSUnit_GetElement ("label2");
	    var linkbutton = JSUnit_GetElement ("lb1");

	    Assert.AreEqual ("", "JSUnit_GetElement ('label').innerHTML", "label1 inner html");
	    Assert.AreEqual ("", "JSUnit_GetElement ('label2').innerHTML", "label2 inner html");

	    JSUnit_Click (linkbutton);
	},


	LB_click2_post: function () {

	    Assert.AreEqual ("PASS", "JSUnit_GetElement ('label').innerHTML", "label1 inner html");
	    Assert.AreEqual ("PASS", "JSUnit_GetElement ('label2').innerHTML", "label2 inner html");
	}
    };

</script>

</body>
</html>
