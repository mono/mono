using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default : System.Web.UI.Page
{
	protected void Page_Load (object sender, EventArgs e)
	{
		StringBuilder sb = new StringBuilder ();
		TraverseChildren (sb, sitemap, String.Empty);

		log.InnerText = sb.ToString ();
	}

	void TraverseChildren (StringBuilder sb, Control top, string indent)
	{
		foreach (Control c in top.Controls) {
			sb.AppendFormat ("\t{0} [{1}]\r\n", c.GetType (), c.UniqueID);
			TraverseChildren (sb, c, indent + "\t");
		}
	}
}