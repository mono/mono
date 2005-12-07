<%@ Page language="c#" AutoEventWireup="false" %>
<script runat="server" language="C#">
	private void Test_OnClick(object Sender, EventArgs e)
	{
		Validate();
	}
	
	private void CustomValidator_ServerValidate(object Sender, ServerValidateEventArgs e)
	{
		e.IsValid = false;
	}
</script>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" > 

<html>
  <head>
    <title>Mono CustomValidator Bug</title>
  </head>
  <body>
	
    <form id="Form1" runat="server">
		
		<asp:validationsummary runat="server" headertext="Validation errors should only display here"/>

		<br>
		<br>
		After post back, Error Message should not display here: <asp:customvalidator runat="server" display="None" errormessage="Validator is not valid!" onservervalidate="CustomValidator_ServerValidate"/>
		<br>
		<br>
		<asp:button runat="server" causesvalidation="True" Text="Test" onclick="Test_OnClick"/>
     </form>
	
  </body>

</html>

