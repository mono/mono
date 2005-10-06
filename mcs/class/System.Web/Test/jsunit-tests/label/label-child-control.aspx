<%@ Page Language="C#" AutoEventWireup="True" %>
<html>
<body>
       <form id="myform" runat="server">
       <asp:label id="mylabel" runat="server">asdfasdf <asp:checkbox id="mycheckbox" runat="server" /> asdfasd</asp:label>
       </form>

	<script Language="JavaScript">
	    var TestFixture = {

		verify_child_control: function() {
		    Assert.NotNull ("JSUnit_GetElement ('mycheckbox')", "child control exists");
		},

		verify_parent_control: function() {
		    Assert.AreEqual ("mylabel", "JSUnit_GetElement ('mycheckbox').parentNode.getAttribute('id')", "parent control id");
		}
	    };
	</script>
</body>
</html>
