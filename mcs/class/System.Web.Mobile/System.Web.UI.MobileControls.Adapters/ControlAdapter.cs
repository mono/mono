
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

		public System.Web.UI.MobileControls.Style Style
		{
			get
			{
				return Control.Style;
			}
		}

		public virtual MobileControl Control
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

		public virtual bool HandlePostBackEvent(string eventArguments)
		{
			return false;
		}

		public virtual void LoadAdapterState(object state)
		{
		}

		public virtual bool LoadPostData(string postKey,
		               NameValueCollection postCollection,
		               object privateControlData, out bool dataChanged)
		{
			dataChanged = false;
			return false;
		}

		public virtual void OnInit(EventArgs e)
		{
			return;
		}

		public virtual void OnLoad(EventArgs e)
		{
			return;
		}

		public virtual void OnPreRender(EventArgs e)
		{
			return;
		}

		public virtual void OnUnload(EventArgs e)
		{
			return;
		}

		public virtual void Render(HtmlTextWriter writer)
		{
			RenderChildren(writer);
		}

		public virtual object SaveAdapterState()
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

		protected string GetDefaultLabel(int labelID)
		{
			if(labelID < 0 || labelID >= labelIDs.Length)
			{
				// FIXME
				throw new ArgumentException("ControlAdapter" +
				                            "_InvalidDefaultLabel");
			}
			string retVal = null;
			if(Page != null)
			{
				ControlAdapter ca = (ControlAdapter)Page.Adapter;
				if(defaultLabels == null)
				{
					defaultLabels = new string[labelIDs.Length];
					Array.Copy(labelIDs, defaultLabels, labelIDs.Length);
				}
				retVal = defaultLabels[labelID];
			}
			if(retVal == null)
			{
				retVal = labelIDs[labelID];
			}
			return retVal;
		}
	}
}
