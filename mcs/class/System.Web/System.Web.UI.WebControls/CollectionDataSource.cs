//
// System.Web.UI.WebControls.CollectionDataSource
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005-2010 Novell, Inc.
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
using System.Xml;
using System.Xml.Xsl;
using System.ComponentModel;
using System.IO;

namespace System.Web.UI.WebControls
{
	internal class CollectionDataSource : IDataSource
	{
		static readonly string[] names = new string [0];
		IEnumerable collection;
		
		public CollectionDataSource (IEnumerable collection)
		{
			this.collection = collection;
		}
		
		public event EventHandler DataSourceChanged {
			add {}
			remove {}
		}
		
		public DataSourceView GetView (string viewName)
		{
			return new CollectionDataSourceView (this, viewName, collection);
		}
		
		public ICollection GetViewNames ()
		{
			return names;
		}
	}
	
	internal class CollectionDataSourceView: DataSourceView
	{
		IEnumerable collection;

		public CollectionDataSourceView (IDataSource owner, string viewName, IEnumerable collection)
		: base (owner, viewName)
		{
			this.collection = collection;
		}
		
		protected internal override IEnumerable ExecuteSelect (DataSourceSelectArguments arguments)
		{
			return collection;
		}
	}
}


