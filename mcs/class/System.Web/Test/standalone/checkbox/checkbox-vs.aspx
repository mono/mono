<%@ Page Language="C#" Debug="true" %>
<html>
  <head>
    <title>CheckBox viewstates</title>
    <script runat="server">
	void Page_Load ()
	{
		label.Text = String.Format ("Version: {0}<br>", Environment.Version);

		// Viewstate
		MyCheckBox myc = new MyCheckBox ();
		label.Text += "<hr>";
		label.Text += myc.GetVSItems ();
		myc.AutoPostBack = true;
		myc.Checked = true;
		myc.Text = "wibble";
		myc.TextAlign = TextAlign.Left;
		label.Text += "<hr>";
		label.Text += myc.GetVSItems ();
		myc.TextAlign = TextAlign.Right;
		label.Text += "<hr>";
		label.Text += myc.GetVSItems ();
		myc.Checked = false;
		label.Text += "<hr>";
		label.Text += myc.GetVSItems ();
		myc.AutoPostBack = false;
		label.Text += "<hr>";
		label.Text += myc.GetVSItems ();
		myc.Text = null;
		label.Text += "<hr>";
		label.Text += myc.GetVSItems ();
		panel.Controls.Add (myc);

	}

	class MyCheckBox : CheckBox {
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
  </head>
  <body>
    This test shows default property values.
    <br>
    <form runat="server">
      <asp:Label runat="server" id="label" />
      <hr>
      <asp:Panel runat="server" id="panel" />
      <hr>
    </form>
  </body>
</html>

