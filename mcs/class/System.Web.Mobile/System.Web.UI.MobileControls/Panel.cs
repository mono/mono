
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
 * Class     : Panel
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
	public class Panel : MobileControl, ITemplateable
	{
		private Panel deviceSpecificContent;
		private bool  paginationStateChanged = false;

		public Panel()
		{
		}

		public override bool BreakAfter
		{
			get
			{
				return base.BreakAfter;
			}
			set
			{
				base.BreakAfter = value;
			}
		}

		protected override bool PaginateChildren
		{
			get
			{
				return Paginate;
			}
		}

		public virtual bool Paginate
		{
			get
			{
				object o = ViewState["Paginate"];
				if(o != null)
					return (bool)o;
				return false;
			}
			set
			{
				bool oldPaginate = Paginate;
				ViewState["Paginate"] = value;
				if(IsTrackingViewState)
				{
					PaginationStateChanged = true;
					if(oldPaginate && !value)
						MobileControl.SetControlPageRecursive(this, 1);
				}
			}
		}

		public Panel Content
		{
			get
			{
				return deviceSpecificContent;
			}
		}

		internal bool PaginationStateChanged
		{
			get
			{
				return paginationStateChanged;
			}
			set
			{
				paginationStateChanged = value;
			}
		}

		public override void AddLinkedForms(IList linkedForms)
		{
			try
			{
				foreach(Control current in Controls)
				{
					if(current is MobileControl)
						((MobileControl)current).AddLinkedForms(linkedForms);
				}
			} finally
			{
				if(linkedForms.GetEnumerator() is IDisposable)
					((IDisposable)linkedForms.GetEnumerator()).Dispose();
			}
		}

		public override void CreateDefaultTemplatedUI(bool doDataBind)
		{
			ITemplate contentTmpl = GetTemplate(Constants.ContentTemplateTag);
			if(contentTmpl != null)
			{
				deviceSpecificContent = new TemplateContainer();
				contentTmpl.InstantiateIn(this);
				Controls.AddAt(0, deviceSpecificContent);
			}
		}

		public override void PaginateRecursive(ControlPager pager)
		{
			if(EnablePagination)
			{
				if(Paginate && Content != null)
				{
					Content.Paginate = true;
					Content.PaginateRecursive(pager);
					FirstPage = Content.FirstPage;
					LastPage  = pager.PageCount;
					base.PaginateRecursive(pager);
				}
			}
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			if(IsTemplated)
			{
				ClearChildViewState();
				CreateTemplatedUI(false);
				ChildControlsCreated = true;
			}
		}
	}
}
