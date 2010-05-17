using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using RegisterBuildProvider.Test;

namespace RegisterBuildProvider
{
	public partial class _default : System.Web.UI.Page
	{
		protected void Page_Load (object sender, EventArgs e)
		{
			var sb = new StringBuilder ();

			foreach (string s in Log.Data)
				sb.AppendLine (s);

			log.InnerText = sb.ToString ();
			
			AppDomain.CurrentDomain.SetData ("TestRunData", Log.Data);
		}
	}
}