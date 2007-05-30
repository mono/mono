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
using System.ComponentModel;
using System.Collections.Generic;

using System.Data;
using System.Data.Common;

namespace System.Data.Common
{

        public class DbConnectionStringBuilder : IDictionary, ICollection, IEnumerable, ICustomTypeDescriptor
        {
                #region Fields
                Dictionary<string, object> _dictionary = null;
                #endregion Fields

                #region Constructors
                public DbConnectionStringBuilder ()
                {
                        Init ();
                }

                public DbConnectionStringBuilder (bool useFirstKeyValue)
                {
                        throw new NotImplementedException ();
                }

                private void Init ()
                {
                        _dictionary = new Dictionary<string, object> ();
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
                                foreach (string key in dictionary.Keys) {
                                        conn += key + "=" + dictionary [key].ToString () + ";";
                                }
                                conn = conn.TrimEnd (';');
                                return conn;
                        }
                        set { 
				if (value == null)
					throw new ArgumentNullException ("ConnectionString cannot be null");
				
				string [] parameters = value.Split (new char [] {';'});
				foreach (string args in parameters) {
					string [] arg = args.Split (new char [] {'='}, 2);
					if (arg.Length == 2) {
						string key = arg [0].Trim ().ToUpper ();
						string val = arg [1].Trim ();
						this [key] = val;
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
                                        throw new ArgumentException ();
                        }
                        set { Add (keyword, value); }
                }

		[Browsable (false)]
                public virtual ICollection Keys
                {
                        get { return _dictionary.Keys; }
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
                        get { return _dictionary.Values; }
                }

                #endregion // Properties


                #region Methods


                public void Add (string keyword, object value)
                {
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
                        } catch (ArgumentException e) {
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
