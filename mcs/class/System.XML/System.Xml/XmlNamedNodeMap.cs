//
// System.Xml.XmlNamedNodeMap
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//   Duncan Mak  (duncan@ximian.com)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System;
using System.Collections;

namespace System.Xml
{
	public class XmlNamedNodeMap : IEnumerable
	{
		private XmlNode parent;
		private ArrayList nodeList;

		internal XmlNamedNodeMap (XmlNode parent)
		{
			this.parent = parent;
			nodeList = new ArrayList ();
		}

		public virtual int Count {
			get { return nodeList.Count; }
		}

		[MonoTODO]
		public virtual IEnumerator GetEnumerator () 
		{
			throw new NotImplementedException ();
		}

		public virtual XmlNode GetNamedItem (string name)
		{
			foreach (XmlNode node in nodeList) {
				if (node.Name == name)
					return node;
			}
			return null;
		}

		[MonoTODO]
		public virtual XmlNode GetNamedItem (string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
		}
		
		public virtual XmlNode Item (int index)
		{
			if (index < 0 || index > nodeList.Count)
				return null;
			else
				return (XmlNode) nodeList [index];
		}

		[MonoTODO]
		public virtual XmlNode RemoveNamedItem (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlNode RemoveNamedItem (string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlNode SetNamedItem (XmlNode node)
		{
			nodeList.Add (node);
			return node;
		}
	}
}
