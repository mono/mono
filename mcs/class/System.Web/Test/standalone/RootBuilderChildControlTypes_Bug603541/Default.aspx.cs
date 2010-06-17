using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
	    StringBuilder sb = new StringBuilder ();

	    sb.AppendFormat ("a: {0}\n", anchor.GetType ());
	    sb.AppendFormat ("button: {0}\n", button.GetType ());
	    sb.AppendFormat ("img: {0}\n", img.GetType ());
	    sb.AppendFormat ("link: {0}\n", link.GetType ());
	    sb.AppendFormat ("meta: {0}\n", meta.GetType ());
	    sb.AppendFormat ("select: {0}\n", testSelect.GetType ());
	    sb.AppendFormat ("table: {0}\n", table.GetType ());
	    sb.AppendFormat ("td: {0}\n", td.GetType ());
	    sb.AppendFormat ("tr: {0}\n", tr.GetType ());
	    sb.AppendFormat ("th: {0}\n", th.GetType ());
	    sb.AppendFormat ("textarea: {0}\n", textarea.GetType ());

	    sb.AppendFormat ("inputButton: {0}\n", inputButton.GetType ());
	    sb.AppendFormat ("inputSubmit: {0}\n", inputSubmit.GetType ());
	    sb.AppendFormat ("inputReset: {0}\n", inputReset.GetType ());
	    sb.AppendFormat ("inputCheckbox: {0}\n", inputCheckbox.GetType ());
	    sb.AppendFormat ("inputFile: {0}\n", inputFile.GetType ());
	    sb.AppendFormat ("inputHidden: {0}\n", inputHidden.GetType ());
	    sb.AppendFormat ("inputImage: {0}\n", inputImage.GetType ());
	    sb.AppendFormat ("inputRadio: {0}\n", inputRadio.GetType ());
	    sb.AppendFormat ("inputText: {0}\n", inputText.GetType ());
	    sb.AppendFormat ("inputPassword: {0}\n", inputPassword.GetType ());

	    log.InnerText = sb.ToString ();
    }
}