<%@ Page Language="C#" %>
<%@ Import namespace="System.Web.Configuration" %>

<script runat="server">

void Page_Load ()
{
	string value = "";
	System.Configuration.Configuration c = WebConfigurationManager.OpenWebConfiguration("/toshok/configuration/twolevel");
	lbl.Text = String.Format ("{0} (c.FilePath = {1}", c.AppSettings.Settings["testSetting"].Value, c.FilePath);
	lbl2.Text = WebConfigurationManager.AppSettings["testSetting"];

	object s = WebConfigurationManager.GetSection ("appSettings");
	if (s is NameValueCollection) {
		NameValueCollection col = (NameValueCollection)s;
		value = String.Format ("{0} (section type = NameValueCollection)", col["testSetting"]);
	}
	else if (s is AppSettingsSection) {
		AppSettingsSection sect = (AppSettingsSection)s;
		value = String.Format ("{0} (section type = AppSettingsSection)", sect.Settings["testSetting"].Value);
	}
	lbl3.Text = value;

	s = WebConfigurationManager.GetSection ("appSettings", "/toshok/configuration/twolevel");
	if (s is NameValueCollection) {
		NameValueCollection col = (NameValueCollection)s;
		value = String.Format ("{0} (section type = NameValueCollection)", col["testSetting"]);
	}
	else if (s is AppSettingsSection) {
		AppSettingsSection sect = (AppSettingsSection)s;
		value = String.Format ("{0} (section type = AppSettingsSection)", sect.Settings["testSetting"].Value);
	}
	lbl4.Text = value;
}

</script>

<table>
<tr><td>WebConfigurationManager.OpenWebConfiguration <td><asp:Label id="lbl" runat="server" /></tr>
<tr><td>WebConfigurationManager.AppSettings <td><asp:Label id="lbl2" runat="server" /> </tr>
<tr><td>WebConfigurationManager.GetSection(string) <td><asp:Label id="lbl3" runat="server" /> </tr>
<tr><td>WebConfigurationManager.GetSection(string,string) <td><asp:Label id="lbl4" runat="server" /> </tr>
</table>

