//
// System.Web.UI.WebControls.XmlDataSourceView
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
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
using System.Collections.Specialized;
using System.Text;
using System.Xml;

namespace System.Web.UI.WebControls {
	public sealed class XmlDataSourceView : DataSourceView {
		public XmlDataSourceView (XmlDataSource owner, string name, XmlNodeList nodes)
		{
			// Why do they pass owner?
			this.name = name;
			this.nodes = new ArrayList (nodes.Count);
			
			foreach (XmlNode node in nodes) {
				if (node.NodeType == XmlNodeType.Element)
					this.nodes.Add (node);
			}
		}
		
		[MonoTODO]
		public IEnumerable Select (DataSourceSelectArguments arguments)
		{
			return nodes;
		}
		
		public override string Name { 
			get { return name; }
		}
		
		string name;
		ArrayList nodes;
	
		[MonoTODO]
		protected internal override IEnumerable ExecuteSelect (
						DataSourceSelectArguments arguments)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO ("Extra method to keep things compiling, need to remove later")]
		public override IEnumerable Select ()
		{
			throw new NotImplementedException ();
		}
	}
}
#endif

