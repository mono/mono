<%@ Page Language="C#" Debug="true" %>
<html>
<script runat="server">
// Output:
//Version: 1.1.4322.2032
//Default AdvertisementFile -> '' (null? False)
//Default Target -> '_top' (null? False)
//Default KeywordFilter -> '' (null? False)
//Type of Controls -> 'System.Web.UI.EmptyControlCollection'
//Number of Controls before adding literal -> '0'
//Number of Controls -> '0'
// <hr>
//Count: 0
// <hr>
//Count: 1
//Target: pepe

	void Page_Load ()
	{
		label.Text = String.Format ("Version: {0}<br>", Environment.Version);
		AdRotator ar = new AdRotator ();
		// Empty
		label.Text += String.Format ("Default AdvertisementFile -> '{0}' (null? {1})", ar.AdvertisementFile, ar.AdvertisementFile == null);
		label.Text += "<br>";
		// "_top"
		label.Text += String.Format ("Default Target -> '{0}' (null? {1})", ar.Target, ar.Target == null);
		label.Text += "<br>";
		// Empty
		label.Text += String.Format ("Default KeywordFilter -> '{0}' (null? {1})", ar.KeywordFilter, ar.KeywordFilter == null);

		label.Text += "<br>";
		// EmptyControlCollection (on 2.0 is a ControlCollection)
		label.Text += String.Format ("Type of Controls -> '{0}'<br>", ar.Controls.GetType ());
		// 0
		label.Text += String.Format ("Number of Controls before adding literal -> '{0}'<br>", ar.Controls.Count);
		// Next line throws in 1.1, works fine under 2.0
		//ar.Controls.Add (new LiteralControl ("Hi there"));
		label.Text += String.Format ("Number of Controls -> '{0}'", ar.Controls.Count);
		//panel.Controls.Add (ar);

		// Viewstate
		MyRotator myr = new MyRotator ();
		label.Text += "<hr>";
		label.Text += myr.GetVSItems ();
		myr.Target = "pepe";
		myr.AdvertisementFile = "ads.xml";
		//myr.KeywordFilter = "filterthis";
		label.Text += "<hr>";
		label.Text += myr.GetVSItems ();
		panel.Controls.Add (myr);

	}

	class MyRotator : AdRotator {
		public string GetVSItems ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("Count: {0}<br>", ViewState.Count);
			foreach (string o in ViewState.Keys) {
				sb.AppendFormat ("{0}: {1}<br>", o, ViewState [o]);
			}

			return sb.ToString ();
		}
	}

</script>
<body>
This test shows default property values.
<br>
<asp:Label runat="server" id="label" />
<hr>
<asp:Panel runat="server" id="panel" />
<hr>
</body>
</html>

