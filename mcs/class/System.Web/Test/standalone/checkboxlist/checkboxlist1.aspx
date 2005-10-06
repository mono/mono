<%@ Page Language="C#" %>
<html>
<head>
<title>CheckBoxList</title>
</head>
<body>

<script runat="server">

void SelectedIndexChanged (object sender, EventArgs e)
{
        label.Text = "Index Has Changed";
}

</script>


<form runat="server">

<p>
I have AutoPostBack enabled.
<p>

<asp:CheckBoxList id="l2"
RepeatLayout="table" AutoPostBack="true"
OnSelectedIndexChanged="SelectedIndexChanged" runat="server">
<asp:ListItem>One</asp:ListItem>
<asp:ListItem>Two</asp:ListItem>
<asp:ListItem>Three</asp:ListItem>
<asp:ListItem>Five</asp:ListItem>
</asp:CheckBoxList>


<asp:Button id="button1" runat="server" Text="Button" />
<br>
<asp:Label id="label" runat="server" />


</form>
</body>
</html>

