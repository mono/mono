/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : DeviceSpecificChoiceCollection
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
	public class DeviceSpecificChoiceCollection
	             : ArrayListCollectionBase
	{
		private DeviceSpecific owner;

		internal DeviceSpecificChoiceCollection(DeviceSpecific owner)
		{
			this.owner = owner;
		}

		public DeviceSpecificChoice this[int index]
		{
			get
			{
				return (DeviceSpecificChoice)base.Items[index];
			}
		}

		public ArrayList All
		{
			get
			{
				return base.Items;
			}
		}

		public void Add(DeviceSpecificChoice choice)
		{
			AddAt(-1, choice);
		}

		public void AddAt(int index, DeviceSpecificChoice choice)
		{
			choice.Owner = owner;
			if(index == -1)
				Items.Add(choice);
			else
				Items.Insert(index, choice);
		}

		public void Clear()
		{
			Items.Clear();
		}
	}
}
