<%@ Page Language="C#" %>
<%@ Import namespace="System.Web.Configuration" %>

<script runat="server">

void Page_Load ()
{
	lbl.Text = WebConfigurationManager.AppSettings["testSetting"];
}

</script>

<asp:Label id="lbl" runat="server" />
