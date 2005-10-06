<%@ Page Language="C#" AutoEventWireup="True" %>
<html>
<head>
</head>
<body>
  <form id="myform" runat="server">
    <asp:checkbox id="mytxtbox" runat="server" />
    <asp:label id="mylabel" associatedcontrolid="mytxtbox" runat="server">Hello!</asp:label>
  </form>


  <script Language="JavaScript">
    var TestFixture = {

      	verify_controls: function() {
	    Assert.NotNull ("JSUnit_GetElement ('mytxtbox')", "assoc control exists");
	    Assert.NotNull ("JSUnit_GetElement ('mylabel')", "label control exists");
	},

	verify_assoc_id: function() {
	    JSUnit_BindElement ("mylabel");

	    JSUnit_ExpectFailure ("IE fails this test because for some reason it doesn't expose the 'for' attribute in their dom?..  ugh.");
	    Assert.AttributeHasValue ("mytxtbox", "for", "assoc_id exists and is correct");
	}
	
    };
  </script>

</body>
</html>
