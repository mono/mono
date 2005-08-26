<%@ Page language="c#" AutoEventWireup="true"%>

<html>
<head>
<script runat="server">

	void Page_Load (object o, EventArgs e)
	{           
		string s = "";

		foreach (string key in HttpContext.Current.Request.Params.AllKeys) {
			string[] vals = HttpContext.Current.Request.Params.GetValues (key);

			s += key + ": ";
		        foreach (string val in vals) {
				s += val + ",";
			}
			s += "<br/>\n";
		}

		foo.Text = s;

		HttpContext.Current.Response.Cookies ["blah"].Expires = DateTime.Now.AddMinutes(15);
		HttpContext.Current.Response.Cookies ["blah"].Value = Guid.NewGuid ().ToString ();
	}

</script>

</head>
<body>
<asp:Label id="foo" runat="server"/>
</body>
</html>
