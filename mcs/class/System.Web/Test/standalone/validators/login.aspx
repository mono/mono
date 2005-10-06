<%@ Page Language="C#" EnableViewState="false" %>
<%@ Import Namespace="System.Web.Security" %>
<html>
<script language="C#" runat=server>
	void Login_Click (object sender, EventArgs e)
	{
		Console.WriteLine("In Login_Click");
        if (IsPostBack == true && Page.IsValid)
        {
			Console.WriteLine("Before RedirectFromLoginPage: Name=" + name.Value);
			FormsAuthentication.RedirectFromLoginPage(name.Value, PersistCookie.Checked);
		}
		else
			Console.WriteLine("No IsPostBack or Page not valid");
	}
</script>
<head>
<title>Login</title>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
</head>
<body>
<form method="POST" action="login.aspx" runat="server">
<asp:ValidationSummary runat="server" HeaderText="Please fix the following errors:" />
Name: <input type="text" id="name" runat=server/></td><td><ASP:RequiredFieldValidator ControlToValidate="name"
			 Display="Static" ErrorMessage="Please enter a name" runat=server>*</ASP:RequiredFieldValidator>
<p />
Password:<asp:textbox id="password" textmode="password" runat="server" />
<p />
<ASP:CheckBox id=PersistCookie runat="server" checked="true" /> activate autologin
<p />
<asp:button text="Login" OnClick="Login_Click" runat=server/>
</form>
</body>
</html>
