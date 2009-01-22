<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>
<%@ Import Namespace="MonoTests.SystemWeb.Framework" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
    <script runat="server">
        public override void Validate ()
        {
            base.Validate ();
            if (WebTest.CurrentTest.UserData == null) {
                ArrayList list = new ArrayList ();
                list.Add ("Validate");
                WebTest.CurrentTest.UserData = list;
            }
            else {
                ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
                if (list == null)
                    throw new NullReferenceException ();
                list.Add ("Validate");
                WebTest.CurrentTest.UserData = list;
            }
        }

        public override void Validate (string validationGroup)
        {
            base.Validate (validationGroup);
            if (WebTest.CurrentTest.UserData == null) {
                ArrayList list = new ArrayList ();
                list.Add ("Validate_WithGroup");
                WebTest.CurrentTest.UserData = list;
            }
            else {
                ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
                if (list == null)
                    throw new NullReferenceException ();
                list.Add ("Validate_WithGroup");
                WebTest.CurrentTest.UserData = list;
            }
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:TextBox ID="TextBox1" ValidationGroup="valid" runat="server"></asp:TextBox>
        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ValidationGroup="valid"  ControlToValidate="TextBox1" runat="server" ErrorMessage="RequiredFieldValidatorMessage"></asp:RequiredFieldValidator>
    </div>
        <input id="Submit1" type="submit" value="submit" />
        <asp:Button ID="Button1" ValidationGroup="valid" runat="server"
            Text="Button" />
    </form>
</body>
</html>
