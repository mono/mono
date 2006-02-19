//
// System.Data.OleDb.OleDbConnection
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2006 Mainsoft Corporation (http://www.mainsoft.com)
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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Globalization;

namespace Mainsoft.Data.Configuration
{
	public class ConnectionStringDictionary: IConnectionStringDictionary
	{
		#region Fields

		private readonly IDictionary _dictionary;
		private readonly NameValueCollection _mapping;
		private readonly NameValueCollection _actualKeys;

		#endregion // Fields

		#region Constructors

		public ConnectionStringDictionary(string connectionString, NameValueCollection defaultMapping)
		{
			_actualKeys = new NameValueCollection();
			_dictionary = Parse (connectionString);
			_mapping = defaultMapping;
		}

		#endregion // Constructors

		#region Properties

		public object this [object key]
		{
			get { 

				if (!(key is String))
					throw new ArgumentException("key is not string");

				string skey = (string)key;

				skey = GetConnectionStringKey(skey);
				if (skey == null)
					return null;

				return _dictionary[skey];
			}

			set {
				if (!(key is String))
					throw new ArgumentException("key is not string");

				string skey = (string)key;

				skey = GetConnectionStringKey(skey);
				if (skey == null)
					skey = (string)key;

				_dictionary[skey] = value;
			}
		}
		#endregion // Properties

		#region Methods

		public string GetConnectionStringKey (string key)
		{
			string cached = _actualKeys [key];
			if (cached != null)
				return cached;

			if (_mapping != null)
			for(int i = 0, c = _mapping.Keys.Count; i < c; i++) {
				if (string.Compare(key, _mapping.Keys[i], true,
					CultureInfo.InvariantCulture) == 0) {
					string[] values = _mapping.GetValues(i);
					for(int j = 0; j < values.Length; j++) {
						string actualKey = values[j];
						if (_dictionary.Contains (actualKey)) {
							_actualKeys.Add (key, actualKey);
							return actualKey;
						}
					}
				}
			}

			if (_dictionary.Contains(key))
				return key;

			return null;
		}

		public static IDictionary Parse (string connectionString)
		{
			IDictionary userParameters = CollectionsUtil.CreateCaseInsensitiveHashtable();

			if (connectionString == null || connectionString.Length == 0) {
				return userParameters;
			}
			//connectionString += ";";

			bool inQuote = false;
			bool inDQuote = false;
			bool inName = true;

			string name = String.Empty;
			string value = String.Empty;
			StringBuilder sb = new StringBuilder (connectionString.Length);

			for (int i = 0; i < connectionString.Length; i ++) {
				char c = connectionString [i];
				char peek;
				if (i == connectionString.Length - 1)
					peek = '\0';
				else
					peek = connectionString [i + 1];

				switch (c) {
					case '\'':
						if (inDQuote)
							sb.Append (c);
						else if (peek == c) {
							sb.Append(c);
							i ++;
						}
						else
							inQuote = !inQuote;
						break;
					case '"':
						if (inQuote)
							sb.Append(c);
						else if (peek == c) {
							sb.Append(c);
							i ++;
						}
						else
							inDQuote = !inDQuote;
						break;
					case ';':
						if (inDQuote || inQuote)
							sb.Append(c);
						else {
							if (name != null && name.Length > 0) {
								value = sb.ToString();
								userParameters [name.Trim()] = value.Trim();
							}
							inName = true;
							name = String.Empty;
							value = String.Empty;
							sb.Length = 0;
						}
						break;
					case '=':
						if (inDQuote || inQuote || !inName)
							sb.Append (c);
						else if (peek == c) {
							sb.Append (c);
							i += 1;
						}
						else {
							name = sb.ToString();
							sb.Length = 0;
							inName = false;
						}
						break;
					case ' ':
						if (inQuote || inDQuote)
							sb.Append(c);
						else if (sb.Length > 0 && peek != ';')
							sb.Append(c);
						break;
					default:
						sb.Append(c);
						break;
				}
			}

			if (inDQuote || inQuote)
				throw new ArgumentException("connectionString");

			if (name != null && name.Length > 0) {
				value = sb.ToString();
				userParameters [name.Trim()] = value.Trim();
			}

			return userParameters;
		}


		#endregion // Methods

		#region IDictionary Members

		public bool IsFixedSize
		{
			get { return _dictionary.IsFixedSize; }
		}

		public bool IsReadOnly
		{
			get { return _dictionary.IsReadOnly; }
		}

		public ICollection Keys
		{
			get { 
				return _dictionary.Keys; 
			}
		}

		public ICollection Values
		{
			get { return _dictionary.Values; }
		}

		public void Add (object key, object value)
		{
			_dictionary.Add ((string)key, (string)value);
		}

		public void Clear ()
		{
			_dictionary.Clear ();
		}

		public bool Contains (object key)
		{
			return _dictionary.Contains (key);
		}

		public IDictionaryEnumerator GetEnumerator ()
		{
			return _dictionary.GetEnumerator ();
		}

		public void Remove (object key)
		{
			_dictionary.Remove ((string)key);
		}

		#endregion // IDictionary Members

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator () 
		{
			return _dictionary.GetEnumerator ();
		}

		#endregion // IEnumerable Members

		#region ICollection Members

		public bool IsSynchronized 
		{
			get { return ((ICollection)_dictionary).IsSynchronized; }
		}

		public int Count 
		{
			get { return _dictionary.Count;	}
		}

		public void CopyTo (Array array, int index) 
		{
			_dictionary.CopyTo (array, index);
		}

		public object SyncRoot 
		{
			get {return ((ICollection)_dictionary).SyncRoot; }
		}

		#endregion // ICollection Members
	}
}
