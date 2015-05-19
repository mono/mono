//
// System.Web.UI.WebControls.XmlHierarchicalEnumerable
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

using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Xml;

namespace System.Web.UI.WebControls {
	internal class XmlHierarchicalEnumerable : IHierarchicalEnumerable {
		internal XmlHierarchicalEnumerable (XmlNodeList nodeList)
		{
			this.nodeList = nodeList;
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			ArrayList ret = new ArrayList (nodeList.Count);
			
			foreach (XmlNode node in nodeList) {
				if (node.NodeType == XmlNodeType.Element)
					ret.Add (new XmlHierarchyData (node));
			}
			
			return ret.GetEnumerator ();
		}
		
		IHierarchyData IHierarchicalEnumerable.GetHierarchyData (object enumeratedItem)
		{
			return (IHierarchyData) enumeratedItem;
		}
		
		XmlNodeList nodeList;
	
	}
}

