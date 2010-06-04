using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class search : System.Web.UI.Page
{
	protected void Page_Load (object sender, EventArgs e)
	{
		string searchterm = Page.RouteData.Values ["searchterm"] as string;
		label1.Text = searchterm;

		var sb = new StringBuilder ();
		RunTest (sb, "Missing key", "SearchTermd", typeof (Label), "Text");
		RunTest (sb, "Missing property", "SearchTerm", typeof (Label), "NoSuchPropertyHere");
		RunTest (sb, "No converter", "SearchTerm", typeof (Image), "Font");
		RunTest (sb, "Valid conversion to target", "SearchTerm", typeof (Image), "Enabled");
		RunTest (sb, "Invalid conversion to target", "SearchTerm", typeof (HotSpot), "TabIndex");
		RunTest (sb, "Complex type converter", "SearchTerm", typeof (Style), "BackColor");
		RunTest (sb, "Null controlType", "SearchTerm", null, "Text");
		RunTest (sb, "Null propertyName", "SearchTerm", typeof (Label), null);
		RunTest (sb, "Empty propertyName", "SearchTerm", typeof (Label), String.Empty);
		RunTest (sb, "Non-string value", "intValue", typeof (Label), "Text");
		RunTest (sb, "Non-string value", "boolValue", typeof (Label), "Text");
		RunTest (sb, "Non-string value", "doubleValue", typeof (Label), "Text");

		testLog.InnerText = sb.ToString ();
	}

	void RunTest (StringBuilder sb, string title, string key, Type controlType, string propertyName)
	{
		sb.AppendFormat (".: {0} (key: '{1}')\n", title, key);
		try {
			object val = RouteValueExpressionBuilder.GetRouteValue (this, key, controlType, propertyName);
			if (val != null)
				sb.AppendFormat ("\tReturned value of type '{0}': {1}\n", val.GetType (), val.ToString ());
			else
				sb.AppendFormat ("\tReturned null.\n");
		} catch (Exception ex) {
			sb.AppendFormat ("\tException '{0}' caught\n", ex.GetType ());
		}
	}
}