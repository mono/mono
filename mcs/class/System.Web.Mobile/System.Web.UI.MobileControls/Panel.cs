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
