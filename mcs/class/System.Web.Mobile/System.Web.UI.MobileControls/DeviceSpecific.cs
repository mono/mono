
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
 * Class     : DeviceSpecific
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Collections;
using System.Web.UI;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public class DeviceSpecific : Control
	{
		private DeviceSpecificChoiceCollection choices;
		private TemplateControl closestTemplateControl = null;
		private bool haveSelectedChoice = false;
		private object owner;
		private DeviceSpecificChoice selectedChoice;

		public DeviceSpecific()
		{
		}

		internal void SetOwner(object owner)
		{
			this.owner = owner;
		}

		internal void SetDesignerChoice(DeviceSpecificChoice choice)
		{
			this.selectedChoice = choice;
			this.haveSelectedChoice = true;
		}

		internal void ApplyProperties()
		{
			if(SelectedChoice != null)
			{
				SelectedChoice.ApplyProperties();
			}
		}

		public new event EventHandler DataBinding
		{
			add
			{
				base.DataBinding += value;
			}
			remove
			{
				base.DataBinding -= value;
			}
		}

		public new event EventHandler Disposed
		{
			add
			{
				base.Disposed += value;
			}
			remove
			{
				base.Disposed -= value;
			}
		}

		public new event EventHandler Init
		{
			add
			{
				base.Init += value;
			}
			remove
			{
				base.Init -= value;
			}
		}

		public new event EventHandler Load
		{
			add
			{
				base.Load += value;
			}
			remove
			{
				base.Load -= value;
			}
		}

		public new event EventHandler PreRender
		{
			add
			{
				base.PreRender += value;
			}
			remove
			{
				base.PreRender -= value;
			}
		}

		public new event EventHandler Unload
		{
			add
			{
				base.Unload += value;
			}
			remove
			{
				base.Unload -= value;
			}
		}

		public DeviceSpecificChoiceCollection Choices
		{
			get
			{
				if(choices == null)
				{
					choices = new DeviceSpecificChoiceCollection(this);
				}
				return choices;
			}
		}

		public TemplateControl ClosestTemplateControl
		{
			get
			{
				if(closestTemplateControl == null)
				{
					MobileControl ctrl = null;
					if(Owner is System.Web.UI.MobileControls.Style)
					{
						ctrl = ((System.Web.UI.MobileControls.Style)Owner).Control;
					} else
					{
						ctrl = (MobileControl) Owner;
					}
					closestTemplateControl = ctrl.FindClosestTemplateControl();
				}
				return closestTemplateControl;
			}
		}

		public override bool EnableViewState
		{
			get
			{
				return base.EnableViewState;
			}
		}

		public bool HasTemplates
		{
			get
			{
				if(SelectedChoice != null)
				{
					return SelectedChoice.HasTemplates;
				}
				return false;
			}
		}

		public MobilePage MobilePage
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public object Owner
		{
			get
			{
				return this.owner;
			}
		}

		public DeviceSpecificChoice SelectedChoice
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override bool Visible
		{
			get
			{
				return base.Visible;
			}
		}
		
		protected override void AddParsedSubObject(object obj)
		{
			if(obj is DeviceSpecificChoice)
			{
				DeviceSpecificChoice dsc = (DeviceSpecificChoice)obj;
				Choices.Add(dsc);
			}
		}
		
		public ITemplate GetTemplate(string templateName)
		{
			ITemplate retVal = null;
			if(SelectedChoice != null)
			{
				retVal = (ITemplate) SelectedChoice.Templates[templateName];
			}
			return retVal;
		}
	}
}
