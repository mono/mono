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

namespace System.Web.UI.WebControls
{
	public sealed class XmlDataSourceView : DataSourceView
	{
		ArrayList nodes;
		XmlDataSource owner;

		public XmlDataSourceView (XmlDataSource owner, string name)
			: base (owner, name)
		{
			this.owner = owner;
		}
		
		public IEnumerable Select (DataSourceSelectArguments arguments)
		{
			return ExecuteSelect (arguments);
		}
		
		void DoXPathSelect ()
		{
			XmlNodeList selected_nodes = owner.GetXmlDocument ().SelectNodes (owner.XPath != "" ? owner.XPath : "/*/*");

			nodes = new ArrayList (selected_nodes.Count);
			
			foreach (XmlNode node in selected_nodes) {
				if (node.NodeType == XmlNodeType.Element)
					nodes.Add (node);
			}
		}

		protected internal override IEnumerable ExecuteSelect (DataSourceSelectArguments arguments)
		{
			if (nodes == null)
				DoXPathSelect();

			ArrayList list = new ArrayList ();
			int max = arguments.StartRowIndex + (arguments.MaximumRows > 0 ? arguments.MaximumRows : nodes.Count);
			if (max > nodes.Count) max = nodes.Count;

			for (int n = arguments.StartRowIndex; n < max; n++)
				list.Add (new XmlDataSourceNodeDescriptor ((XmlElement) nodes [n]));
				
			if (arguments.RetrieveTotalRowCount)
				arguments.TotalRowCount = nodes.Count;

			return list;
		}		
	}
}
#endif

