//
// System.Xml.XmlNamedNodeMap
//
// Author:
//   Jason Diamond (jason@injektilo.org)
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

		[MonoTODO]
		public virtual int Count {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual IEnumerator GetEnumerator () 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
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

		[MonoTODO]
		public virtual XmlNode Item (int index)
		{
			throw new NotImplementedException ();
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
