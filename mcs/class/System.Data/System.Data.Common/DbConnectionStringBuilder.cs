//
// System.Data.Common.DbConnectionStringBuilder.cs
//
// Author:
//   Sureshkumar T (tsureshkumar@novell.com)
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0
using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.Common
{

        public class DbConnectionStringBuilder : IDictionary, ICollection, IEnumerable, ICustomTypeDescriptor
        {
                #region Fields
                Dictionary<string, object> _dictionary = null;
		bool useOdbcRules;
                #endregion Fields

                #region Constructors
                public DbConnectionStringBuilder ()
                {
                        Init ();
                }

                public DbConnectionStringBuilder (bool useOdbcRules)
                {
			// TODO: if true, quote values using curly braces instead of double-quotes
			//       the default is false
                        //this.useOdbcRules = useOdbcRules; 
			throw new NotImplementedException ();
                }

                private void Init ()
                {
                        _dictionary = new Dictionary <string, object> (StringComparer.InvariantCultureIgnoreCase);
                }

                #endregion // Constructors

                #region Properties
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		[DesignOnly (true)]
                public bool BrowsableConnectionString
                {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

		[RefreshProperties (RefreshProperties.All)]
                public string ConnectionString
                {
                        get
                        {
                                IDictionary<string, object> dictionary = (IDictionary <string, object>) _dictionary;
                                string conn = "";
				string parm = "";
                                foreach (string key in dictionary.Keys) {
					string val = dictionary [key].ToString (); 		
					bool dquoteFound = (val.IndexOf ("\"") > -1);
					bool squoteFound = (val.IndexOf ("\'") > -1);
					bool semicolonFound = (val.IndexOf (";") > -1);
					bool equalFound = (val.IndexOf ("=") > -1);
					bool braceFound = (val.IndexOf ("{") > -1 || val.IndexOf ("}") > -1);
					if (dquoteFound && squoteFound)
						parm = "\"" + val.Replace ("\"", "\"\"") + "\"";
					else if (squoteFound || braceFound || equalFound || semicolonFound)
						parm = "\"" + val + "\"";
					else if (dquoteFound)
						parm = "\'" + val + "\'";
					else
						parm = val;
						
                                        conn += key + "=" + parm + ";";
                                }
                                conn = conn.TrimEnd (';');
                                return conn;
                        }
                        set { 
				Clear ();
				if (value == null)
					return;
				if (value.Trim ().Length == 0)
					return;

				string connectionString = value + ";";
			
				bool inQuote = false;
				bool inDQuote = false;
				bool inName = true;
				int inParen = 0;
				int inBraces = 0;

				string name = String.Empty;
				string val = String.Empty;
				StringBuilder sb = new StringBuilder ();

				for (int i = 0; i < connectionString.Length; i += 1) {
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
						else if (peek.Equals (c)) {
							sb.Append (c);
							i += 1;
						}
						else
							inQuote = !inQuote;
						break;
					case '"':
						if (inQuote)
							sb.Append (c);
						else if (peek.Equals (c)) {
							sb.Append (c);
							i += 1;
						}
						else
							inDQuote = !inDQuote;
						break;
					case '(':
						inParen++;
						sb.Append (c);
						break;
					case ')':
						inParen--;
						sb.Append (c);
						break;
					case '{':
						inBraces++;
						sb.Append (c);
						break;
					case '}':
						inBraces--;
						sb.Append (c);
						break;
					case ';':
						if (inDQuote || inQuote)
							sb.Append (c);
						else {
							if (name != String.Empty && name != null) {
								val = sb.ToString ();
								name = name.ToLower ().Trim ();
								this [name] = val;
							}
							else if (sb.Length != 0)
								throw new ArgumentException ("Format of initialization string does not conform to specifications");
							inName = true;
							name = String.Empty;
							value = String.Empty;
							sb = new StringBuilder ();
						}
						break;
					case '=':
						if (inDQuote || inQuote || !inName || inParen > 0 || inBraces > 0)
							sb.Append (c);
						else if (peek.Equals (c)) {
							sb.Append (c);
							i += 1;
						}
						else {
							name = sb.ToString ();
							sb = new StringBuilder ();
							inName = false;
						}
						break;
					case ' ':
						if (inQuote || inDQuote)
							sb.Append (c);
						else if (sb.Length > 0 && !peek.Equals (';'))
							sb.Append (c);
						break;
					default:
						sb.Append (c);
						break;
					}
				}
			}
                }

		[Browsable (false)]
                public virtual int Count
                {
                        get { return _dictionary.Count; }
                }

		[Browsable (false)]
                public virtual bool IsFixedSize
                {
                        get { return false; }
                }

		[Browsable (false)]
                public bool IsReadOnly
                {
                        get { throw new NotImplementedException (); }
                }

		[Browsable (false)]
                public virtual object this [string keyword]
                {
                        get
                        {
                                if (ContainsKey (keyword))
                                        return _dictionary [keyword];
                                else
                                        throw new ArgumentException ("Keyword does not exist");
                        }
                        set { Add (keyword, value); }
                }

		[Browsable (false)]
                public virtual ICollection Keys
                {
                        get { 
				string [] keys = new string [_dictionary.Keys.Count];
				((ICollection<string>) _dictionary.Keys).CopyTo (keys, 0);
				ReadOnlyCollection<string> keyColl = new ReadOnlyCollection<string> (keys);
				return keyColl; 
			}
                }

                bool ICollection.IsSynchronized
                {
                        get { throw new NotImplementedException (); }
                }

                object ICollection.SyncRoot
                {
                        get { throw new NotImplementedException (); }
                }

                object IDictionary.this [object keyword]
                {
                        get { return this [(string) keyword]; }
                        set { this [(string) keyword] = value; }
                }

		[Browsable (false)]
                public virtual ICollection Values
                {
                        get { 
				object [] values = new object [_dictionary.Values.Count];
				((ICollection<object>) _dictionary.Values).CopyTo (values, 0);
				ReadOnlyCollection<object> valuesColl = new ReadOnlyCollection<object> (values);
				return valuesColl; 
			}
                }

                #endregion // Properties


                #region Methods


                public void Add (string keyword, object value)
                {
			if (keyword == null || keyword.Trim () == "")
				throw new ArgumentException ("Keyword should not be emtpy");
			if (value == null)
				throw new ArgumentException ("Value should not be null");
                        if (ContainsKey (keyword)) {
                                _dictionary [keyword] = value;
                        } else {
                                _dictionary.Add (keyword, value);
                        }

                }

		[MonoLimitation("useOdbcRules set to true is not supported")]
		public static void AppendKeyValuePair (StringBuilder builder, string keyword, string value,
						       bool useOdbcRules)
		{
			if (useOdbcRules == false) {
				AppendKeyValuePair (builder, keyword, value);
			} else {
				throw new NotImplementedException ();
			}
		}

                public static void AppendKeyValuePair (StringBuilder builder, string keyword, string value)
                {
                        if (builder.Length > 0) {
                                char lastChar = builder [builder.Length];
                                if (lastChar != ';' && lastChar != ' ')
                                        builder.Append (';');
                                else if (lastChar == ' ' && !builder.ToString ().Trim ().EndsWith (";"))
                                        builder.Append (';');
                        }
                        builder.AppendFormat ("{0}={1}", keyword, value);
                }

                public virtual void Clear ()
                {
                        _dictionary.Clear ();
                }

                public virtual bool ContainsKey (string keyword)
                {
			if (keyword == null)
				throw new ArgumentNullException ("Invalid argument", keyword);
                        return _dictionary.ContainsKey (keyword);
                }

                public virtual bool EquivalentTo (DbConnectionStringBuilder connectionStringBuilder)
                {
                        bool ret = true;
                        try {
                                if (Count != connectionStringBuilder.Count)
                                        ret = false;
                                else {
                                        foreach (string key in Keys) {
                                                if (!this [key].Equals (connectionStringBuilder [key])) {
                                                        ret = false;
                                                        break;
                                                }
                                        }
                                }
                        } catch (ArgumentException) {
                                ret = false;
                        }
                        return ret;
                }

		[MonoTODO]
		protected virtual void GetProperties (Hashtable propertyDescriptors)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal void ClearPropertyDescriptors ()
		{
			throw new NotImplementedException ();
		}

                public virtual bool Remove (string keyword)
                {
                        return _dictionary.Remove (keyword);
                }

                public virtual bool ShouldSerialize (string keyword)
                {
                        throw new NotImplementedException ();
                }

                void ICollection.CopyTo (Array array, int index)
                {
			if (array == null)
				throw new ArgumentNullException ("array");
			KeyValuePair<string, object> [] arr = array as KeyValuePair<string, object> [];
			if (arr == null)
				throw new ArgumentException ("Target array type is not compatible with the type of items in the collection");
			((ICollection<KeyValuePair<string, object>>) _dictionary).CopyTo (arr, index);
                }

                void IDictionary.Add (object keyword, object value)
                {
                        this.Add ((string) keyword, value);
                }

                bool IDictionary.Contains (object keyword)
                {
                        return ContainsKey ((string) keyword);
                }

                IDictionaryEnumerator IDictionary.GetEnumerator ()
                {
                        return (IDictionaryEnumerator) _dictionary.GetEnumerator ();
                }

                void IDictionary.Remove (object keyword)
                {
                        Remove ((string) keyword);
                }

                IEnumerator IEnumerable.GetEnumerator ()
                {
                        return (IEnumerator) _dictionary.GetEnumerator ();
                }

                private static object _staticAttributeCollection = null;
                AttributeCollection ICustomTypeDescriptor.GetAttributes ()
                {
                        object value = _staticAttributeCollection;
                        if (value == null) {
                                CLSCompliantAttribute clsAttr = new CLSCompliantAttribute (true);
                                DefaultMemberAttribute defMemAttr = new DefaultMemberAttribute ("Item");
                                Attribute [] attrs = {clsAttr, defMemAttr};
                                value = new AttributeCollection (attrs);
                        }
                        System.Threading.Interlocked.CompareExchange (ref _staticAttributeCollection, value, null);
                        return _staticAttributeCollection as AttributeCollection;
                }

                string ICustomTypeDescriptor.GetClassName ()
                {
                        return this.GetType ().ToString ();
                }

                string ICustomTypeDescriptor.GetComponentName ()
                {
                        return null;
                }

                TypeConverter ICustomTypeDescriptor.GetConverter ()
                {
                        return new CollectionConverter ();
                }

                EventDescriptor ICustomTypeDescriptor.GetDefaultEvent ()
                {
                        return null;
                }

                PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty ()
                {
                        return null;
                }

                object ICustomTypeDescriptor.GetEditor (Type editorBaseType)
                {
                        return null;
                }

                EventDescriptorCollection ICustomTypeDescriptor.GetEvents ()
                {
                        return EventDescriptorCollection.Empty;
                }

                EventDescriptorCollection ICustomTypeDescriptor.GetEvents (Attribute [] attributes)
                {
                        return EventDescriptorCollection.Empty;
                }

                PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties ()
                {
                        return PropertyDescriptorCollection.Empty;
                }

                PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties (Attribute [] attributes)
                {
                        return PropertyDescriptorCollection.Empty;
                }

                object ICustomTypeDescriptor.GetPropertyOwner (PropertyDescriptor pd)
                {
                        throw new NotImplementedException ();
                }

                public override string ToString ()
                {
                        return ConnectionString;
                }

                public virtual bool TryGetValue (string keyword, out object value)
                {
                        // FIXME : not sure, difference between this [keyword] and this method
                        bool found = ContainsKey (keyword);
                        if (found)
                                value = this [keyword];
                        else
                                value = null;
                        return found;
                }

                #endregion // Public Methods
        }
}
#endif // NET_2_0 using
