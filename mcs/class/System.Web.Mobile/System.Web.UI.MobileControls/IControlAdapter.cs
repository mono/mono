/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : IControlAdapter
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Collections.Specialized;
using System.Web.UI;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public interface IControlAdapter
	{
		MobileControl Control       { get; set; }
		int           ItemWeight    { get; }
		MobilePage    Page          { get; }
		int           VisibleWeight { get; }

		void   CreateTemplatedUI(bool doDataBind);
		bool   HandlePostBackEvent(string eventArguments);
		void   LoadAdapterState(object state);
		bool   LoadPostData(string postKey, NameValueCollection postCollection,
		                    object privateControlData, out bool dataChanged);
		void   OnInit(EventArgs e);
		// Strange! Docs read "public virtual void OnLoad(...);"
		void   OnLoad(EventArgs e);
		void   OnPreRender(EventArgs e);
		void   OnUnload(EventArgs e);
		// Strange! Docs read "Render(...);"
		void   Render(HtmlTextWriter writer);
		object SaveAdapterState();
	}
}
