/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : ObjectList
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.Web.UI;

namespace System.Web.UI.MobileControls
{
	public class ObjectList // : PagedControl, INamingContainer,
	                        // ITemplateable, IPostBackEventHander
	{
		public ObjectList()
		{
		}

		public IObjectListFieldCollection AllFields
		{
			get
			{
				throw new NotImplementedException();
			}
		}
	}
}
