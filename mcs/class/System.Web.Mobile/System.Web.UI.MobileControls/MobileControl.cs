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
	public abstract class MobileControl : Control
	{
		private Style style;
		private IControlAdapter adapter;

		private bool enablePagination;

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
