/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls.Adapters
 * Class     : ControlAdapter
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.Collections.Specialized;
using System.Web.Mobile;
using System.Web.UI;
using System.Web.UI.MobileControls;

namespace System.Web.UI.MobileControls.Adapters
{
	public abstract class ControlAdapter : IControlAdapter
	{
		private MobileControl control = null;

		protected static readonly int BackLabel = 0x00;
		protected static readonly int GoLabel   = 0x01;
		protected static readonly int OKLabel   = 0x02;
		protected static readonly int MoreLabel = 0x03;
		protected static readonly int OptionsLabel = 0x04;
		protected static readonly int NextLabel = 0x05;
		protected static readonly int PreviousLabel = 0x06;
		protected static readonly int LinkLabel = 0x07;
		protected static readonly int CallLabel = 0x08;

		private static string[] labelIDs = {
			"ControlAdapter_Back",
			"ControlAdapter_Go",
			"ControlAdapter_OK",
			"ControlAdapter_More",
			"ControlAdapter_Options",
			"ControlAdapter_Next",
			"ControlAdapter_Previous",
			"ControlAdapter_Link",
			"ControlAdapter_Call"
		};

		private string[] defaultLabels = null;

		public ControlAdapter()
		{
		}

		public MobileCapabilities Device
		{
			get
			{
				if(Page != null)
				{
					return (MobileCapabilities) Page.Request.Browser;
				}
				return null;
			}
		}

		public MobileControl Control
		{
			get
			{
				return this.control;
			}
			set
			{
				this.control = value;
			}
		}

		public int ItemWeight
		{
			get
			{
				return ControlPager.UseDefaultWeight;
			}
		}

		public MobilePage Page
		{
			get
			{
				if(Control != null)
					return Control.MobilePage;
				return null;
			}
		}

		public int VisibleWeight
		{
			get
			{
				return ControlPager.UseDefaultWeight;
			}
		}

		public void CreateTemplatedUI(bool doDataBind)
		{
			Control.CreateDefaultTemplatedUI(doDataBind);
		}

		public bool HandlePostBackEvent(string eventArguments)
		{
			return false;
		}

		public void LoadAdapterState(object state)
		{
			return;
		}

		public bool LoadPostData(string postKey,
		               NameValueCollection postCollection,
		               object privateControlData, out bool dataChanged)
		{
			dataChanged = false;
			return false;
		}

		public void OnInit(EventArgs e)
		{
			return;
		}

		public void OnLoad(EventArgs e)
		{
			return;
		}

		public void OnPreRender(EventArgs e)
		{
			return;
		}

		public void OnUnload(EventArgs e)
		{
			return;
		}

		public void Render(HtmlTextWriter writer)
		{
			RenderChildren(writer);
		}

		public object SaveAdapterState()
		{
			return null;
		}

		protected void RenderChildren(HtmlTextWriter writer)
		{
			if(Control != null && Control.HasControls())
			{
				foreach(Control cCtrl in Control.Controls)
				{
					cCtrl.RenderControl(writer);
				}
				if(Control.Controls.GetEnumerator() is IDisposable)
				{
					((IDisposable)Control.Controls.GetEnumerator()).Dispose();
				}
			}
		}

		protected int CalculateOptimumPageWeight(int defaultPageWeight)
		{
			int defWt = defaultPageWeight;
			if(Device != null)
			{
				string httpDefWt = Device[Constants.OptimumPageWeightParameter];
				if(httpDefWt != null)
				{
					try
					{
						defWt = Convert.ToInt32(httpDefWt);
					} catch { }
				}
			}
			return -1;
		}
	}
}
