//
// System.Web.UI.WebControls.XmlDataSourceView
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
		
		public override IEnumerable Select ()
		{
			return nodes;
		}
		
		public override string Name { 
			get { return name; }
		}
		
		string name;
		ArrayList nodes;
	
	}
}
#endif

