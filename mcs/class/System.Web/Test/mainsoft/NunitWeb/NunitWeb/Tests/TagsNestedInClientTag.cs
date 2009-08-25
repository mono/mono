using System;
using System.Data;
using System.Data.Common;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Tests {
	public class TagsNestedInClientTag : System.Web.UI.Page
	{
		protected Literal languageLiteral;
		protected Literal typeLiteral;
		protected Literal srcLiteral;
		protected Literal languageLiteral1;
		protected Literal typeLiteral1;
		protected Literal srcLiteral1;
		
		protected void Page_Load(object sender, EventArgs e)
		{
			languageLiteral.Text = "language=\"javascript\"";
			typeLiteral.Text = "type=\"text/javascript\"";
			srcLiteral.Text = "src=\"/js/test.js\"";
			
			languageLiteral1.Text = "language=\"javascript\"";
			typeLiteral1.Text = "type=\"text/javascript\"";
			srcLiteral1.Text = "src=\"/js/test.js\"";
		}
	}
}

