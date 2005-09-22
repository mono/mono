#if NET_2_0
using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Xml
{
	public class XmlDictionary : IXmlDictionary
	{
		static XmlDictionary empty = new XmlDictionary (true);

		public static XmlDictionary Empty {
			get { return empty; }
		}

		readonly bool is_readonly;
		Dictionary<string, XmlDictionaryString> dict;

		public XmlDictionary ()
		{
			dict = new Dictionary<string, XmlDictionaryString> ();
		}

		public XmlDictionary (int capacity)
		{
			dict = new Dictionary<string, XmlDictionaryString> (capacity);
		}

		// for static empty.
		private XmlDictionary (bool isReadOnly)
			: this ()
		{
			is_readonly = isReadOnly;
		}

		public virtual XmlDictionaryString Add (string value)
		{
			if (is_readonly)
				throw new InvalidOperationException ();
			throw new NotImplementedException ();
		}

		public bool TryLookup (int key, out XmlDictionaryString result)
		{
			throw new NotImplementedException ();
		}

		public bool TryLookup (string value, out XmlDictionaryString result)
		{
			throw new NotImplementedException ();
		}

		public bool TryLookup (XmlDictionaryString value,
			out XmlDictionaryString result)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
