
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
