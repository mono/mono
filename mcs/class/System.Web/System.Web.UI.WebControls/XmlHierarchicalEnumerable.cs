//
// System.Web.UI.WebControls.XmlHierarchicalEnumerable
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Xml;

namespace System.Web.UI.WebControls {
	public class XmlHierarchicalEnumerable : IHierarchicalEnumerable {
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
#endif

