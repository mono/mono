<%@ Page Language="C#" %>
<html>
  <head>
    <title>HtmlForm test</title>
  </head>
  <body>
    <form defaultfocus="entry" defaultbutton="Button"  id="form1" runat="server">
      <asp:TextBox id="firstone" Text="this won't have focus" runat="server"/>
      <asp:TextBox id="entry" Text="this will" runat="server"/>
      <asp:LinkButton name="Button" id="Button" Text="Hi There" runat="server"/>
    </form>
  </body>
</html>

