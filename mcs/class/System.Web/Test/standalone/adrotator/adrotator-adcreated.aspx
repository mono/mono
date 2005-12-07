<%@ Page Language="C#" Debug="true" %>
<%@ Import namespace="System.Collections" %>
<%@ Import namespace="System.Text" %>
<html>
<script runat="server">
	void Create1 (object sender, AdCreatedEventArgs args)
	{
		label1.Text = GetArgsData (args);
	}

	void Create2 (object sender, AdCreatedEventArgs args)
	{
		label2.Text = GetArgsData (args);
	}

	string GetArgsData (AdCreatedEventArgs args)
	{
		StringBuilder sb = new StringBuilder ();
		sb.AppendFormat ("ImageUrl: {0}<br>", args.ImageUrl);
		sb.AppendFormat ("NavigateUrl: {0}<br>", args.NavigateUrl);
		sb.AppendFormat ("AlternateText: {0}<br>", args.AlternateText);
		sb.Append ("AdProperties:<br><div style='margin-left: 80px;'>");
		foreach (DictionaryEntry entry in args.AdProperties)
			sb.AppendFormat (" {0}: {1}<br>", entry.Key, entry.Value);
		sb.Append ("</div>");
		return sb.ToString ();
	}

</script>
<body>
Testing 2 source files, 2 ad rotators. One does not change 'cause it uses a filter.
It will also show the properties passed to the AdCreated event.<br>
<form runat="server">
<asp:button Text="Click me" runat="server" />
<hr>
This should rotate:<br>
<asp:adrotator runat="server" id="ar1"  AdvertisementFile="ads.xml"
					OnAdCreated="Create1" />
<br>
<asp:Label id="label1" runat="server" />
<hr>
This should always be novell:<br>
<asp:adrotator runat="server" id="ar2"  KeywordFilter="novell"
					AdvertisementFile="adsplus.xml"
					OnAdCreated="Create2" />
<br>
<asp:Label id="label2" runat="server" />
</form>
</body>
</html>

