/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : Form
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Web.UI;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public class Form : Panel, IPostBackEventHandler
	{
		public Form()
		{
		}

		public string Action
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		[MonoTODO]
		void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
		{
			throw new NotImplementedException();
		}

		public int CurrentPage
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public bool HasActiveHandler()
		{
			throw new NotImplementedException();
		}
	}
}
