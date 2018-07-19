//
// System.Configuration.ConfigHelper
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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
#if (XML_DEP)
using System.Xml;
#endif

#pragma warning disable 618

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
#if (XML_DEP)
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

		internal static ConfigNameValueCollection GetNameValueCollection (NameValueCollection prev,
									    XmlNode region,
									    string nameAtt,
									    string valueAtt)
		{
			ConfigNameValueCollection coll =
					new ConfigNameValueCollection (CaseInsensitiveHashCodeProvider.Default,
								 CaseInsensitiveComparer.Default);

			if (prev != null)
				coll.Add (prev);

			CollectionWrapper result = new CollectionWrapper (coll);
			result = GoGetThem (result, region, nameAtt, valueAtt);
			if (result == null)
				return null;

			return result.UnWrap () as ConfigNameValueCollection;
		}

		private static CollectionWrapper GoGetThem (CollectionWrapper result,
							    XmlNode region,
							    string nameAtt,
							    string valueAtt)
		{
			if (region.Attributes != null && region.Attributes.Count != 0) {
				if (region.Attributes.Count != 1 || region.Attributes[0].Name != "xmlns") {
					throw new ConfigurationException ("Unknown attribute", region);
				}
			}

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
#endif
	}
	
	internal class ConfigNameValueCollection: NameValueCollection
	{
		bool modified;
		
		public ConfigNameValueCollection ()
		{
		}
		
		public ConfigNameValueCollection (ConfigNameValueCollection col)
		: base (col.Count, col)
		{
		}
		
		public ConfigNameValueCollection (IHashCodeProvider hashProvider, IComparer comparer)
			: base(hashProvider, comparer)
		{
		}
		
		public void ResetModified ()
		{
			modified = false;
		}
		
		public bool IsModified {
			get { return modified; }
		}
		
		public override void Set (string name, string value)
		{
			base.Set (name, value);
			modified = true;
		}
	}

}
