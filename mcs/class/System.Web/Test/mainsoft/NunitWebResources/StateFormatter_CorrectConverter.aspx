<%@ Page Language="C#" Debug="true"  AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>
<%@ Register Namespace="Samples.AspNet.CS.Controls" Assembly="System.Web_test" TagPrefix="aspSample" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<script runat="server">
  void Button_Click(object sender, EventArgs e)
  {
    Book1.Author.FirstName = "Bob";
    Book1.Author.LastName = "Kelly";
    Book1.Title = "Contoso Stories";
    Book1.Price = 39.95M;
    Button1.Visible = false;
  }  
</script>
<html xmlns="http://www.w3.org/1999/xhtml">
  <head id="Head1" runat="server">
    <title>Bug #545979</title>
  </head>
  <body>
    <form id="Form1" runat="server">
      <aspSample:Book ID="Book1" Runat="server"  
        Title="Tailspin Toys Stories" CurrencySymbol="$" 
        BackColor="#FFE0C0" Font-Names="Tahoma" 
        Price="16" BookType="Fiction">
        <Author FirstName="Judy" LastName="Lew" />
      </aspSample:Book>
      <br />
      <asp:Button ID="Button1" OnClick="Button_Click" 
        Runat="server" Text="Change" />
      <asp:Button ID="Button2" Runat="server" Text="Refresh" />
      <br />
      <br />
      <asp:HyperLink ID="Hyperlink1" NavigateUrl="BookTest.aspx" 
        Runat="server">
        Reload Page</asp:HyperLink>
    </form>
  </body>
</html>
