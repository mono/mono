//
// SystemWebTestShim/Adapters.cs
//
// Author:
//   Raja R Harinath (harinath@hurrynot.org)
//
// (C) 2009 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System.Collections;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SystemWebTestShim {
	using Orig = System.Web.UI.Adapters;

	public class PageAdapter : Orig.PageAdapter {
		public PageAdapter (Page p) : base (p) {}
	}
}

namespace SystemWebTestShim {
	using Orig = System.Web.UI.WebControls.Adapters;

	public class DataBoundControlAdapter : Orig.DataBoundControlAdapter {
		public DataBoundControlAdapter (DataBoundControl c) : base (c) {}

		public new void PerformDataBinding (IEnumerable data)
		{
			base.PerformDataBinding (data);
		}
	}

	public class HierarchicalDataBoundControlAdapter : Orig.HierarchicalDataBoundControlAdapter {
		public HierarchicalDataBoundControlAdapter (HierarchicalDataBoundControl h) : base (h) {}

		public new void PerformDataBinding ()
		{
			base.PerformDataBinding ();
		}
	}

	public class WebControlAdapter : Orig.WebControlAdapter {
		public WebControlAdapter (WebControl c) : base (c) {}
	}

	public class MenuAdapter : Orig.MenuAdapter {
		public MenuAdapter (Menu c) : base (c) {}
	}

	public class HideDisabledControlAdapter : Orig.HideDisabledControlAdapter {
		public HideDisabledControlAdapter (WebControl c) : base (c) {}

		public new void Render (HtmlTextWriter w)
		{
			base.Render (w);
		}
	}
}

#endif
