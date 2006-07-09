<%@ Page Language="C#" Theme="Green" %>
<head id="foo" runat="server">
<script runat="server">
protected override void OnInit (EventArgs args)
{
  base.OnInit (args);
}

void Page_Load ()
{
  string s = String.Format ("{0} controls", Header.Controls.Count);
  foreach (Control c in Header.Controls) {
    s = s + String.Format (", {0}", c.GetType());
  }
  fweep.Text = s;
}
</script>
</head>

<form runat="server">
  <asp:Label runat="server" id="label" Text="Label"/>
  <asp:Button runat="server" id="button" Text="Button"/>
  <br/>

<asp:radiobuttonlist runat="server">
<asp:ListItem Value="4" Text="Four"/>
<asp:ListItem Value="5" Text="Five"/>
<asp:ListItem Value="6" Text="Six"/>
</asp:radiobuttonlist>

<asp:label id="fweep" runat="server"></asp:label>
</form>

