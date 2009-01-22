<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:LoginView ID="LoginView1" runat="server">
        <LoggedInTemplate>
          <br />
          You are logged in as
          <asp:LoginName ID="LoginName1" runat="server" />
          .<br />
          <br />
          <asp:LoginStatus ID="LoginStatus1" runat="server" />
        </LoggedInTemplate>
        <AnonymousTemplate>
          <strong>You are not logged in. Please Login.<br />
          </strong>
          <br />
          <asp:Login ID="Login1" DestinationPageUrl="~/LoginViewTest1.aspx" runat="server" MembershipProvider="FakeProvider" BackColor="#EFF3FB" BorderColor="#B5C7DE" BorderPadding="4"
            BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" ForeColor="#333333">
            <TitleTextStyle BackColor="#507CD1" Font-Bold="True" Font-Size="0.9em" 
               ForeColor="White" />
            <LoginButtonStyle BackColor="White" BorderColor="#507CD1" BorderStyle="Solid" BorderWidth="1px"
              Font-Names="Verdana" ForeColor="#284E98" />
            <InstructionTextStyle Font-Italic="True" ForeColor="Black" />
          </asp:Login>
        </AnonymousTemplate>
      </asp:LoginView>
    </div>
    </form>
</body>
</html>
