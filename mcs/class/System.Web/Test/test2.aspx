<%@ Page Language="C#" %>
<html>
<head>
<script language="C#" runat="server">
void Page_Load(Object Sender, EventArgs e) {
          if (!IsPostBack) {
             ArrayList values = new ArrayList();

             values.Add(new PositionData("Microsoft", "Msft"));
             values.Add(new PositionData("Intel", "Intc"));
             values.Add(new PositionData("Dell", "Dell"));

             Repeater1.DataSource = values;
             Repeater1.DataBind();

             Repeater2.DataSource = values;
             Repeater2.DataBind();
             Response.Write(Repeater1.Controls[0].ClientID);
             Response.Write("<br>");
             Response.Write(Repeater1.Controls[0].UniqueID);
             Response.Write("<br>");
          }
       }

       public class PositionData {

          private string name;
          private string ticker;

          public PositionData(string name, string ticker) {
             this.name = name;
             this.ticker = ticker;
          }

          public string Name {
             get {
                return name;
             }
          }

          public string Ticker {
             get {
                return ticker;
             }
          }
       }

    </script>

</head>
<body>

<h3><font face="Verdana">Repeater Example</font></h3>

<form runat=server>

<b>Repeater1:</b>

<p>

<asp:Repeater id=Repeater1 runat="server">
<HeaderTemplate>
<table border=1>
<tr>
<td><b>Company</b></td>
<td><b>Symbol</b></td>
</tr>
</HeaderTemplate>

<ItemTemplate>
<tr>
<td> <%# DataBinder.Eval(Container.DataItem, "Name") %> <asp:label id="test"></td>
<td> <%# DataBinder.Eval(Container.DataItem, "Ticker") %> </td>
</tr>
</ItemTemplate>

<FooterTemplate>
</table>
</FooterTemplate>

</asp:Repeater>
<p>

<b>Repeater2:</b>
<p>
<asp:Repeater id=Repeater2 runat="server">

<HeaderTemplate>
Company data:
          </HeaderTemplate>

<ItemTemplate>
<%# DataBinder.Eval(Container.DataItem, "Name") %> (<%# DataBinder.Eval(Container.DataItem, "Ticker") %>)
          </ItemTemplate>

<SeparatorTemplate>, </SeparatorTemplate>
</asp:Repeater>
</form>
</body>
</html>

