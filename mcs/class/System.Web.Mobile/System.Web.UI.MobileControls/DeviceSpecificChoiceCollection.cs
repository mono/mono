
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
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
