/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : DeviceSpecific
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Web.UI;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public class DeviceSpecific : Control
	{
		private DeviceSpecificChoiceCollection choices;
		private TemplateControl closestTemplateControl = null;
		private bool haveSelectedChoice;
		private object owner;
		private DeviceSpecificChoice selectedChoice;

		public DeviceSpecific()
		{
		}
		
		public DeviceSpecificChoiceCollection Choices
		{
			get
			{
				if(this.choices == null)
				{
					choices = new DeviceSpecificChoiceCollection(this);
				}
				return this.choices;
			}
		}
		
		public object Owner
		{
			get
			{
				throw new NotImplementedException();
			}
		}
	}
}
