
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

		public override void LoadAdapterState(object state)
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

		protected virtual void RenderAsHiddenInputField(HtmlMobileTextWriter writer)
		{
		}

		[MonoTODO]
		protected void RenderBeginLink(HtmlMobileTextWriter writer,
		                               string target)
		{
			bool isHTTP = false;
			if(PageAdapter.PersistCookielessData)
			{
				if(target.StartsWith("http:") || target.StartsWith("https:"))
				{
					throw new NotImplementedException();
				}
			}
		}

		[MonoTODO]
		protected void RenderEndLink(HtmlMobileTextWriter writer)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected void RenderPostBackEventAsAnchor(HtmlMobileTextWriter writer,
		                             string argument, string linkText)
		{
			throw new NotImplementedException();
		}

		protected void RenderPostBackEventAsAttribute(HtmlMobileTextWriter writer,
		                             string name, string value)
		{
			writer.Write(" " + name + "=\"");
			RenderPostBackEventReference(writer, value);
			writer.Write("\" ");
		}

		protected void RenderPostBackEventReference(HtmlMobileTextWriter writer,
		                                            string argument)
		{
			PageAdapter.RenderPostBackEvent(writer, Control.UniqueID, argument);
		}

		public override object SaveAdapterState()
		{
			int uiMode = SecondaryUIMode;
			object retVal = null;
			if(uiMode != NotSecondaryUI)
				retVal = uiMode;
			return retVal;
		}
	}
}
