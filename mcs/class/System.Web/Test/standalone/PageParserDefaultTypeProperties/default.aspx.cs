using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PageParserDefaultTypeProperties
{
	public partial class _default : MyPage
	{
		protected void Page_Load (object sender, EventArgs e)
		{
			var sb = new StringBuilder ();
			foreach (string s in PreStartMethods.Info)
				sb.AppendLine (s);

			log.InnerText = sb.ToString ();
		}
	}
}