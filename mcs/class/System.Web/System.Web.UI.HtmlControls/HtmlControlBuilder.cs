//
// System.Web.UI.HtmlControls.HtmlControlBuilder
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System.Web.UI;

namespace System.Web.UI.HtmlControls
{
	class HtmlControlBuilder : ControlBuilder
	{
		public override bool HasBody ()
		{
			return false;
		}
	}
}

