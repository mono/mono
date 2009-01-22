<%@ Page Language="C#" AutoEventWireup="true"   %>
<%@ Import Namespace="MonoTests.SystemWeb.Framework" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Untitled Page</title>
    <script runat="server">
        public override void Validate (string validationGroup)
        {
            Response.Write ("Validate_validation_group" + " " + validationGroup); 
        }

        public override void Validate ()
        {
            Response.Write ("Validate");
        }
</script>

</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:TextBox ID="TextBox1" CausesValidation="true" ValidationGroup="MyValidationGroup" AutoPostBack="true" runat="server"></asp:TextBox>
    </div>
    </form>
</body>
</html>
