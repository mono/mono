#if NET_2_0
using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Xml
{
	public class XmlDictionary : IXmlDictionary
	{
		internal class EmptyDictionary : XmlDictionary
		{
			public static readonly EmptyDictionary Instance =
				new EmptyDictionary ();

			public EmptyDictionary ()
				: base (1)
			{
			}
		}

		static XmlDictionary empty = new XmlDictionary (true);

		public static XmlDictionary Empty {
			get { return empty; }
		}

		readonly bool is_readonly;
		Dictionary<string, XmlDictionaryString> dict;
		List<XmlDictionaryString> list;

		public XmlDictionary ()
		{
			dict = new Dictionary<string, XmlDictionaryString> ();
			list = new List<XmlDictionaryString> ();
		}

		public XmlDictionary (int capacity)
		{
			dict = new Dictionary<string, XmlDictionaryString> (capacity);
			list = new List<XmlDictionaryString> (capacity);
		}

		// for static empty.
		private XmlDictionary (bool isReadOnly)
			: this (1)
		{
			is_readonly = isReadOnly;
		}

		public virtual XmlDictionaryString Add (string value)
		{
			if (is_readonly)
				throw new InvalidOperationException ();
			XmlDictionaryString ret;
			if (dict.TryGetValue (value, out ret))
				return ret;
			ret = new XmlDictionaryString (this, value, dict.Count);
			dict.Add (value, ret);
			list.Add (ret);
			return ret;
		}

		public bool TryLookup (int key, out XmlDictionaryString result)
		{
			if (key < 0 || dict.Count >= key) {
				result = null;
				return false;
			}
			result = list [key];
			return true;
		}

		public bool TryLookup (string value, out XmlDictionaryString result)
		{
			if (value == null)
				throw new ArgumentNullException ();
			result = dict [value];
			return result != null;
		}

		public bool TryLookup (XmlDictionaryString value,
			out XmlDictionaryString result)
		{
			if (value == null)
				throw new ArgumentNullException ();
			if (value.Dictionary != this) {
				result = null;
				return false;
			}
			for (int i = 0; i < list.Count; i++) {
				if (object.ReferenceEquals (list [i], value)) {
					result = value;
					return true;
				}
			}
			result = null;
			return false;
		}
	}
}
#endif
