<%@ Page Language="C#" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>
<html><head><title>Change password</title></head>
<body>
  <form runat="server">
    Test: <asp:Label runat="server" id="test"/><br/>
    <asp:ChangePassword ID="ChangePassword1" runat="server">
      <ChangePasswordTemplate>
	<table>
	  <tr>
	    <td>
	      <asp:TextBox runat="server" id="CurrentPassword" TextMode="Password"/>
	      <asp:RequiredFieldValidator runat="server" id="text1required" ControlToValidate="CurrentPassword"/>
	    </td>
	  </tr>

	  <tr>
	    <td>
	      <asp:TextBox runat="server" id="NewPassword" TextMode="Password"/>
	      <asp:RequiredFieldValidator runat="server" id="text2required" ControlToValidate="NewPassword"/>
	    </td>
	  </tr>
	</table>
      </ChangePasswordTemplate>
    </asp:ChangePassword>
  </form>
</body>
</html>
