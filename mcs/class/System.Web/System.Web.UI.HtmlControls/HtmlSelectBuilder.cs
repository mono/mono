//
// System.Web.UI.HtmlControls.HtmlSelectBuilder
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web.UI.HtmlControls
{
	class HtmlSelectBuilder : ControlBuilder
	{
		public override bool AllowWhitespaceLiterals () 
		{
			return false;
		}

		public override Type GetChildControlType (string tagName, IDictionary attribs) 
		{
			if (System.String.Compare (tagName, "option", true) != 0)
				return null;

			return typeof (ListItem);
		}
	}
}

