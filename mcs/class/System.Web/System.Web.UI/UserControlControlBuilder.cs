//
// System.Web.UI.UserControlControlBuilder
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

namespace System.Web.UI
{
	public class UserControlControlBuilder : ControlBuilder
	{
		public override bool NeedsTagInnerText ()
		{
			return false;
		}

		[MonoTODO]
		public override void SetTagInnerText (string text)
		{
			// Do something with the text
		}
	}
}

