//
// System.Web.UI.WebControls.XmlHierarchicalDataSourceView
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
	public class XmlHierarchicalDataSourceView : HierarchicalDataSourceView	{
		internal XmlHierarchicalDataSourceView (XmlNodeList nodeList)
		{
			this.nodeList = nodeList;
		}
		
		public override IHierarchicalEnumerable GetHierarchicalList ()
		{
			return new XmlHierarchicalEnumerable (nodeList);
		}
		
		XmlNodeList nodeList;
	}
}
#endif

