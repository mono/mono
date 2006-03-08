<%@ Page Language="C#" %>
<script runat="server">
protected override void OnPreInit (EventArgs e)
{
  string t = Context.Request["theme"];
  if (t == null)
    t = "MyTheme";
  Console.WriteLine ("setting theme");
  Theme = t;
  base.OnPreInit (e);
}
</script>

<form runat="server">
  <asp:Button runat="server" id="button1" Text="Button"/>
  <asp:Button runat="server" id="button2" Text="Button"/>
  <asp:Button runat="server" id="button3" Text="Button"/>
</form>

