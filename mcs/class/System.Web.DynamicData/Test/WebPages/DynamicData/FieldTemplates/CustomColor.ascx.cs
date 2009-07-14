using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using System.Web.DynamicData;

public partial class CustomColorField : System.Web.DynamicData.FieldTemplateUserControl
{
	public override Control DataControl {
		get { return Literal1; }
	}

	public override string FieldValueString {
		get {
			var color = (Color)FieldValue;
			if (color == null)
				return "Unknown";

			return color.Name;
		}
	}
}
