/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls.Adapters
 * Class     : HtmlControlAdapter
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls.Adapters
{
	public class HtmlControlAdapter : ControlAdapter
	{
		protected static readonly int NotSecondaryUI = -1;

		[MonoTODO("Whould_like_to_keep_it_FFFFFFFF")]
		internal  const int NotSecondaryUIInitial = 0x7FFFFFFF;

		private static string[] multimediaAttrs = {
			"src",
			"soundstart",
			"loop",
			"volume",
			"vibration",
			"viblength"
		};

		public HtmlControlAdapter()
		{
		}

		protected HtmlFormAdapter FormAdapter
		{
			get
			{
				return (HtmlFormAdapter)Control.Form.Adapter;
			}
		}

		protected HtmlPageAdapter PageAdapter
		{
			get
			{
				return (HtmlPageAdapter)Page.Adapter;
			}
		}

		[MonoTODO]
		protected int SecondaryUIMode
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

		public virtual bool RequiresFormTag
		{
			get
			{
				return false;
			}
		}

		[MonoTODO]
		private void AddAttributePrivate(HtmlMobileTextWriter writer,
		                                 string attribute)
		{
			//string val = Control.GetAttribute(attribute);
			string val = String.Empty;
			if(val != null && val.Length > 0)
			{
				writer.WriteAttribute(attribute, val);
			}
		}

		protected virtual void AddAccesskey(HtmlMobileTextWriter writer)
		{
			if(Device.SupportsAccesskeyAttribute)
			{
				AddAttributePrivate(writer, "accesskey");
			}
		}

		protected virtual void AddAttributes(HtmlMobileTextWriter writer)
		{
		}

		protected virtual void AddJPhoneMultiMediaAttributes(
		                         HtmlMobileTextWriter writer)
		{
			if(Device.SupportsJPhoneMultiMediaAttributes)
			{
				foreach(string cAttrib in multimediaAttrs)
				{
					AddAttributePrivate(writer, cAttrib);
				}
			}
		}

		protected void ExitSecondaryUIMode()
		{
			this.SecondaryUIMode = NotSecondaryUI;
		}

		public virtual void LoadAdapterState(object state)
		{
			if(state != null && state is int)
			{
				this.SecondaryUIMode = (int)state;
			}
		}
		
		public virtual void Render(HtmlMobileTextWriter writer)
		{
			base.RenderChildren(writer);
		}
		
		public override void Render(HtmlTextWriter writer)
		{
			if(writer is HtmlMobileTextWriter)
			{
				Render((HtmlMobileTextWriter)writer);
			}
		}
	}
}
