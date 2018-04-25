//
// System.Web.UI.WebControls.SiteMapDataSourceView
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Sanjay Gupta (gsanjay@novell.com)
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2003 Ben Maurer
// (C) 2004 Novell, Inc. (http://www.novell.com)
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

using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class SiteMapDataSourceView : DataSourceView
	{
		SiteMapNodeCollection collection;
		
		public SiteMapDataSourceView (SiteMapDataSource owner, string name, SiteMapNode node)
		: this (owner, name, new SiteMapNodeCollection (node))
		{
		}
		
		public SiteMapDataSourceView (SiteMapDataSource owner, string name, SiteMapNodeCollection collection)
		: base (owner, name)
		{
			this.collection = collection;
		}
		
		public IEnumerable Select (DataSourceSelectArguments arguments)
		{
			return ExecuteSelect (arguments);
		}

		protected internal override IEnumerable ExecuteSelect (DataSourceSelectArguments arguments)
		{
			arguments.RaiseUnsupportedCapabilitiesError (this);
			return collection;
		}

		protected override void OnDataSourceViewChanged (EventArgs e)
		{
			base.OnDataSourceViewChanged (e);
		}

	}
}

