//
// System.Configuration.ConfigHelper
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System.Collections;
using System.Collections.Specialized;
using System.Xml;

namespace System.Configuration
{
	class ConfigHelper
	{
		class CollectionWrapper
		{
			IDictionary dict;
			NameValueCollection collection;
			bool isDict;

			public CollectionWrapper (IDictionary dict)
			{
				this.dict = dict;
				isDict = true;
			}

			public CollectionWrapper (NameValueCollection collection)
			{
				this.collection = collection;
				isDict = false;
			}

			public void Remove (string s)
			{
				if (isDict)
					dict.Remove (s);
				else
					collection.Remove (s);
			}

			public void Clear ()
			{
				if (isDict)
					dict.Clear ();
				else
					collection.Clear ();
			}

			public string this [string key]
			{
				set {
					if (isDict)
						dict [key] = value;
					else
						collection [key] = value;
				}
			}

			public object UnWrap ()
			{
				if (isDict)
					return dict;
				else
					return collection;
			}
		}
		
		internal static IDictionary GetDictionary (IDictionary prev,
							   XmlNode region,
							   string nameAtt,
							   string valueAtt)
		{
			Hashtable hash;
			if (prev == null)
				hash = new Hashtable (CaseInsensitiveHashCodeProvider.Default,
						      CaseInsensitiveComparer.Default);
			else {
				Hashtable aux = (Hashtable) prev;
				hash = (Hashtable) aux.Clone ();
			}

			CollectionWrapper result = new CollectionWrapper (hash);
			result = GoGetThem (result, region, nameAtt, valueAtt);
			if (result == null)
				return null;

			return result.UnWrap () as IDictionary;
		}

		internal static NameValueCollection GetNameValueCollection (NameValueCollection prev,
									    XmlNode region,
									    string nameAtt,
									    string valueAtt)
		{
			NameValueCollection coll =
					new NameValueCollection (CaseInsensitiveHashCodeProvider.Default,
								 CaseInsensitiveComparer.Default);

			if (prev != null)
				coll.Add (prev);

			CollectionWrapper result = new CollectionWrapper (coll);
			result = GoGetThem (result, region, nameAtt, valueAtt);
			if (result == null)
				return null;

			return result.UnWrap () as NameValueCollection;
		}

		private static CollectionWrapper GoGetThem (CollectionWrapper result,
							    XmlNode region,
							    string nameAtt,
							    string valueAtt)
		{
			if (region.Attributes != null && region.Attributes.Count != 0)
				throw new ConfigurationException ("Unknown attribute", region);

			XmlNode keyNode;
			XmlNode valueNode;
			XmlNodeList childs = region.ChildNodes;
			foreach (XmlNode node in childs) {
				XmlNodeType ntype = node.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;

				if (ntype != XmlNodeType.Element)
					throw new ConfigurationException ("Only XmlElement allowed", node);
					
				string nodeName = node.Name;
				if (nodeName == "clear") {
					if (node.Attributes != null && node.Attributes.Count != 0)
						throw new ConfigurationException ("Unknown attribute", node);

					result.Clear ();
				} else if (nodeName == "remove") {
					keyNode = null;
					if (node.Attributes != null)
						keyNode = node.Attributes.RemoveNamedItem (nameAtt);

					if (keyNode == null)
						throw new ConfigurationException ("Required attribute not found",
										  node);
					if (keyNode.Value == String.Empty)
						throw new ConfigurationException ("Required attribute is empty",
										  node);

					if (node.Attributes.Count != 0)
						throw new ConfigurationException ("Unknown attribute", node);
					
					result.Remove (keyNode.Value);
				} else if (nodeName == "add") {
					keyNode = null;
					if (node.Attributes != null)
						keyNode = node.Attributes.RemoveNamedItem (nameAtt);

					if (keyNode == null)
						throw new ConfigurationException ("Required attribute not found",
										  node);
					if (keyNode.Value == String.Empty)
						throw new ConfigurationException ("Required attribute is empty",
										  node);

					valueNode = node.Attributes.RemoveNamedItem (valueAtt);
					if (valueNode == null)
						throw new ConfigurationException ("Required attribute not found",
										  node);

					if (node.Attributes.Count != 0)
						throw new ConfigurationException ("Unknown attribute", node);

					result [keyNode.Value] = valueNode.Value;
				} else {
					throw new ConfigurationException ("Unknown element", node);
				}
			}

			return result;
		}
	}

}
