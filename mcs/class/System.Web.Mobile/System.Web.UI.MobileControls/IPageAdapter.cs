/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : IPageAdapter
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Web.UI;
using System.Web;

namespace System.Web.UI.MobileControls
{
	public interface IPageAdapter : IControlAdapter
	{
		IList       CacheVaryByHeaders       { get; }
		IDictionary CookielessDataDictionary { get; set; }
		int         OptimumPageWeight        { get; }
		new MobilePage  Page                 { get; set; }
		bool        PersistCookielessData    { get; set; }

		HtmlTextWriter      CreateTextWriter(TextWriter writer);
		NameValueCollection DeterminePostBackMode(HttpRequest request,
		                         string postEventSourceID,
		                         string postEventArgumentID,
		                         NameValueCollection baseCollection);
		bool                HandleError(Exception e, HtmlTextWriter writer);
		bool                HandlePagePostBackEvent(string eventSource,
		                                            string eventArgument);
	}
}
