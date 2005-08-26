<%@ Page Language="C#" AutoEventWireup="True" %>
<script runat="server">
        void Page_Load (object o, EventArgs e)
        {
                if (!Page.IsPostBack)
                        lbl.Text = "This text should not change.";
        }
</script>

<html>
<body>
        <form id="myform" runat="server">
                <asp:label id="lbl" runat="server"><asp:label id='fail' runat="server">FAIL!</asp:label></asp:label>
                <asp:button id="button" runat="server" Text="Click Me!"/>
        </form>
</body>


<script Language="JavaScript">
    var TestFixture = {

	postback_pre: function() {
	    JSUnit_BindElement ("lbl");

	    Assert.IsNull ("JSUnit_GetElement('fail')", "label's child should not exist");
	    Assert.AreEqual ("This text should not change.", "JSUnit_GetElement('lbl').innerHTML", "label's html matches");

	    var button = JSUnit_GetElement ("button");

	    JSUnit_TestCausesPageLoad ();
	    JSUnit_Click (button);
	},

	/* ... the page load happens between these two tests */
	postback_post: function() {
	    Assert.IsNull ("JSUnit_GetElement('fail')", "label's child should not exist");
	    Assert.AreEqual ("This text should not change.", "JSUnit_GetElement('lbl').innerHTML", "label's html matches");
	}
    };
</script>

</html>
