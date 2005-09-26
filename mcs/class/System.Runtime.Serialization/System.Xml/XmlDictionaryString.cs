#if NET_2_0
using System;
using System.Collections;

namespace System.Xml
{
	public class XmlDictionaryString
	{
		static XmlDictionaryString empty = new XmlDictionaryString (
			XmlDictionary.EmptyDictionary.Instance,
			String.Empty, 0);

		public static XmlDictionaryString Empty {
			get { return empty; }
		}

		readonly IXmlDictionary dict;
		readonly string value;
		readonly int key;

		public XmlDictionaryString (IXmlDictionary dictionary,
			string value, int key)
		{
			this.dict = dictionary;
			this.value = value;
			this.key = key;
		}

		public IXmlDictionary Dictionary {
			get { return dict; }
		}

		public int Key {
			get { return key; }
		}

		public string Value {
			get { return value; }
		}

		public override string ToString ()
		{
			return value;
		}
	}
}
#endif
