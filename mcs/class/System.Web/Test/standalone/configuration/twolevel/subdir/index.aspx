<%@ Page Language="C#" %>
<%@ Import namespace="System.Web.Configuration" %>

<script runat="server">

void Page_Load ()
{
	string value = "";
	System.Configuration.Configuration c = WebConfigurationManager.OpenWebConfiguration("/toshok/configuration/twolevel");
	if (c == null)
		value = "c == null";
	else if (c.AppSettings == null)
		value = "c.AppSettings == null";
	else if (c.AppSettings.Settings == null)
		value = "c.AppSettings.Settings == null";
	else if (c.AppSettings.Settings["testSetting"] == null)
		value = "c.AppSettings.Settings[testSetting] == null";
	else if (c.AppSettings.Settings["testSetting"].Value == null)
		value = "c.AppSettings.Settings[testSetting].Value == null";
	else
		value = c.AppSettings.Settings["testSetting"].Value;
	lbl.Text = value + String.Format (" (c.FilePath = {0})", c.FilePath);
	lbl2.Text = WebConfigurationManager.AppSettings["testSetting"];

//	NameValueCollection col = (NameValueCollection)WebConfigurationManager.GetSection ("appSettings");
//	lbl3.Text = col["testSetting"];
}

</script>

<asp:Label id="lbl" runat="server" /> <br />
<asp:Label id="lbl2" runat="server" /> <br />
<asp:Label id="lbl3" runat="server" /> <br />

