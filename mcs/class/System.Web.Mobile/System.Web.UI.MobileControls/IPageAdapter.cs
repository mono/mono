
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
