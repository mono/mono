<%@ Page Language="C#" CodeFile="login.aspx.cs" inherits="MyLoginPage" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Bug #601727</title>
</head>
<body>
	<form runat="server" id="form1">
		<asp:Login ID="loginControl" runat="server" LoginButtonType="Button" 
			Orientation="Vertical" CssClass="fieldlabel" RememberMeSet="false" 
			TitleText="Login" OnAuthenticate="loginControl_Authenticate"/>
	</form>
</body>
</html>
