<%@ Page Language="C#" Theme="Green" %>
<script runat="server">
void Page_Init ()
{
//  throw new Exception ();
}
</script>

<form runat="server">
  <asp:Label runat="server" id="label" Text="Label"/>
  <asp:Button runat="server" id="button" Text="Button"/>
  <br/>

<asp:radiobuttonlist runat="server">
<asp:ListItem Value="4" Text="Four"/>
<asp:ListItem Value="5" Text="Five"/>
<asp:ListItem Value="6" Text="Six"/>
</asp:radiobuttonlist>

</form>

