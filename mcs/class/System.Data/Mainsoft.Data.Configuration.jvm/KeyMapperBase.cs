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
using System.IO;

namespace Mainsoft.Data.Configuration
{
	public class ConnectionStringDictionary: IConnectionStringDictionary
	{
		#region Fields

		private readonly IDictionary _dictionary;
		private readonly NameValueCollection _mapping;
		private readonly NameValueCollection _actualKeys;
		const string DataDirectoryPlaceholder = "|DataDirectory|";

		#endregion // Fields

		#region Constructors

		public ConnectionStringDictionary(string connectionString, NameValueCollection defaultMapping)
		{
			_actualKeys = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
			_dictionary = Parse (connectionString);
			_mapping = defaultMapping;
		}

		#endregion // Constructors

		#region Methods

		public string GetConnectionStringKey (string key)
		{
			string cached = _actualKeys [key];
			if (cached != null)
				return cached;

			if (_mapping != null)
			for(int i = 0, c = _mapping.Keys.Count; i < c; i++) {
				if (string.Compare(key, _mapping.Keys[i], StringComparison.OrdinalIgnoreCase) == 0) {
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

		public object GetValue (string key)
		{
			return _dictionary [key];
		}

		public static IDictionary Parse (string connectionString)
		{
			IDictionary userParameters = new Hashtable (StringComparer.OrdinalIgnoreCase);

			if (connectionString == null || connectionString.Length == 0) {
				return userParameters;
			}
			//connectionString += ";";

			if (connectionString.IndexOf (DataDirectoryPlaceholder, StringComparison.Ordinal) >= 0) {
				string dataDirectory = (string) AppDomain.CurrentDomain.GetData ("DataDirectory");
				if (dataDirectory != null && dataDirectory.Length > 0) {
					char lastChar = dataDirectory [dataDirectory.Length - 1];
					if (lastChar != Path.DirectorySeparatorChar &&
						lastChar != Path.AltDirectorySeparatorChar)
						dataDirectory += '/';
				}
				connectionString = connectionString.Replace (DataDirectoryPlaceholder, dataDirectory);
			}

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

		public virtual bool IsFixedSize
		{
			get { return _dictionary.IsFixedSize; }
		}

		public virtual bool IsReadOnly
		{
			get { return _dictionary.IsReadOnly; }
		}

		public virtual ICollection Keys
		{
			get { 
				return _dictionary.Keys; 
			}
		}

		public virtual object this [object key] {
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

		public virtual ICollection Values
		{
			get { return _dictionary.Values; }
		}

		public virtual void Add (object key, object value)
		{
			_dictionary.Add ((string)key, (string)value);
		}

		public virtual void Clear ()
		{
			_dictionary.Clear ();
		}

		public virtual bool Contains (object key)
		{
			return _dictionary.Contains (key);
		}

		public virtual IDictionaryEnumerator GetEnumerator ()
		{
			return _dictionary.GetEnumerator ();
		}

		public virtual void Remove (object key)
		{
			_dictionary.Remove ((string)key);
		}

		#endregion // IDictionary Members

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator () 
		{
			return this.GetEnumerator();
		}

		#endregion // IEnumerable Members

		#region ICollection Members

		public virtual bool IsSynchronized 
		{
			get { return ((ICollection)_dictionary).IsSynchronized; }
		}

		public virtual int Count 
		{
			get { return _dictionary.Count;	}
		}

		public virtual void CopyTo (Array array, int index) 
		{
			_dictionary.CopyTo (array, index);
		}

		public virtual object SyncRoot 
		{
			get {return ((ICollection)_dictionary).SyncRoot; }
		}

		#endregion // ICollection Members
	}
}
