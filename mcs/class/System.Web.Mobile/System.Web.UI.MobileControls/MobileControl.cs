
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
 * Class     : MobileControl
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Drawing;
using System.Collections;
using System.Web.UI;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public abstract class MobileControl : Control//, IAttributeAccessor
	{
		private Style style;
		private IControlAdapter adapter;

		private bool enablePagination;

		//public abstract string GetAttribute(string key);
		//public abstract void   SetAttribute(string key, string value);

		protected MobileControl()
		{
		}

		public IControlAdapter Adapter
		{
			get
			{
				IControlAdapter retVal = null;
				if(adapter != null)
					retVal = adapter;
				else if(MobilePage != null)
					retVal = MobilePage.GetControlAdapter(this);
				return retVal;
			}
		}

		public Alignment Alignment
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

		public virtual Color BackColor
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

		public virtual bool BreakAfter
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

		public DeviceSpecific DeviceSpecific
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

		public int FirstPage
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

		public virtual FontInfo Font
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public virtual Color ForeColor
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

		public Form Form
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public virtual bool IsTemplated
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int LastPage
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

		public MobilePage MobilePage
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public virtual Style Style
		{
			get
			{
				if(this.style == null)
				{
					this.style = this.CreateStyle();
				}
				return this.style;
			}
		}

		public virtual string StyleReference
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

		public virtual int VisibleWeight
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public virtual Wrapping Wrapping
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

		protected string InnerText
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

		protected virtual bool PaginateChildren
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

		public virtual void AddLinkedForms(IList linkedForms)
		{
			throw new NotImplementedException();
		}

		public virtual void CreateDefaultTemplatedUI(bool doDataBind)
		{
			throw new NotImplementedException();
		}

		public virtual void EnsureTemplatedUI()
		{
			throw new NotImplementedException();
		}

		public virtual ITemplate GetTemplate(string templateName)
		{
			throw new NotImplementedException();
		}

		public bool IsVisibleOnPage(int pageNumber)
		{
			throw new NotImplementedException();
		}

		public virtual void PaginateRecursive(ControlPager pager)
		{
			throw new NotImplementedException();
		}

		public Form ResolveFormReference(string name)
		{
			throw new NotImplementedException();
		}

		protected virtual Style CreateStyle()
		{
			throw new NotImplementedException();
		}

		protected virtual void CreatedTempaltedUI(bool doDataBind)
		{
			throw new NotImplementedException();
		}

		protected virtual bool isFormSubmitControl()
		{
			throw new NotImplementedException();
		}

		protected virtual void LoadPrivateViewState(object state)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnPageChange(int oldIndex, int newIndex)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnRender(HtmlTextWriter writer)
		{
			throw new NotImplementedException();
		}

		protected virtual object SavePrivateViewState()
		{
			throw new NotImplementedException();
		}

		protected virtual void CreateTemplatedUI(bool doDataBind)
		{
			throw new NotImplementedException();
		}

		internal static void SetControlPageRecursive(Control ctrl, int page)
		{
			throw new NotImplementedException();
		}

		internal bool EnablePagination
		{
			get
			{
				return enablePagination;
			}
			set
			{
				enablePagination = value;
			}
		}

		internal TemplateControl FindClosestTemplateControl()
		{
			throw new NotImplementedException();
		}
	}
}
