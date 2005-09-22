using System;
using System.Collections;

namespace System.Xml
{
	public class XmlDictionaryString
	{
		IXmlDictionary dict;
		string value;
		int key;

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

		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
	}
}
