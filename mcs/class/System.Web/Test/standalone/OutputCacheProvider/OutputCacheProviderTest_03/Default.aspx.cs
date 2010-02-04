using System;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default : System.Web.UI.Page
{
	protected void Page_Load (object sender, EventArgs e)
	{
		var sb = new StringBuilder ();
		string name = OutputCache.DefaultProviderName;
		sb.AppendFormat ("Default provider name: {0}\n", name);
		
		name = ApplicationInstance.GetOutputCacheProviderName (Context);
		sb.AppendFormat ("Default context: {0}\n", name);

		output.InnerText = sb.ToString ();
	}
}