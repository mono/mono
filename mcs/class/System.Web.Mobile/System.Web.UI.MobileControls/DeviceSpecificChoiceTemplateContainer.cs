/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : DeviceSpecificChoiceTemplateContainer
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Collections;
using System.Reflection;
using System.Web.UI;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public class DeviceSpecificChoiceTemplateContainer
	{
		private string name;
		private ITemplate template;

		public DeviceSpecificChoiceTemplateContainer()
		{
		}

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		public ITemplate Template
		{
			get
			{
				return template;
			}
			set
			{
				template = value;
			}
		}
	}
}
