// 
// System.Web.Configuration.PagesConfiguration
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//

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

using System.Globalization;
using System.Text;

namespace System.Web.Configuration
{
	class PagesConfiguration
	{
		internal bool Buffer = true;
		internal PagesEnableSessionState EnableSessionState = PagesEnableSessionState.True;
		internal bool EnableViewState = true;
		internal bool EnableViewStateMac = true;
		internal bool SmartNavigation = false;
		internal bool AutoEventWireup = true;
		internal bool ValidateRequest = true;
		internal string PageBaseType = "System.Web.UI.Page";
		internal string UserControlBaseType = "System.Web.UI.UserControl";

		internal PagesConfiguration (object p)
		{
			if (!(p is PagesConfiguration))
				return;

			PagesConfiguration parent = (PagesConfiguration) p;
			Buffer = parent.Buffer;
			EnableSessionState = parent.EnableSessionState;
			EnableViewState = parent.EnableViewState;
			EnableViewStateMac = parent.EnableViewStateMac;
			SmartNavigation = parent.SmartNavigation;
			AutoEventWireup = parent.AutoEventWireup;
			ValidateRequest = parent.ValidateRequest;
			PageBaseType = parent.PageBaseType;
			UserControlBaseType = parent.UserControlBaseType;
		}

		static public PagesConfiguration GetInstance (HttpContext context)
		{
			if (context == null)
				context = HttpContext.Current;
			if (context == null)
				return null;
			return context.GetConfig ("system.web/pages") as PagesConfiguration;
		}
	}
}
