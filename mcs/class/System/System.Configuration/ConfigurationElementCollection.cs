//
// System.Configuration.ConfigurationElementCollection.cs
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004

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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

#if NET_2_0 && XML_DEP

using System.Collections;
using System.Xml;

namespace System.Configuration 
{
	public abstract class ConfigurationElementCollection : ConfigurationElement, ICollection, IEnumerable
	{
		ArrayList list = new ArrayList ();
		ArrayList removed;
		bool emitClear;
		bool modified;
		
		#region Constructors

		protected ConfigurationElementCollection ()
		{
		}

		#endregion // Constructors

		#region Properties
		
		protected virtual ConfigurationElementCollectionType CollectionType {
			get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
		}

		public virtual int Count {
			get { return list.Count; }
		}

		protected virtual string ElementName {
			get { return string.Empty; }
		}

		public bool EmitClear {
			get { return emitClear; }
			set { emitClear = value; }
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		object ICollection.SyncRoot {
			get { return this; }
		}

		protected virtual bool ThrowOnDuplicate {
			get { return true; }
		}

		#endregion // Properties

		#region Methods

		protected virtual void BaseAdd (ConfigurationElement element)
		{
			BaseAdd (element, ThrowOnDuplicate);
		}

		protected virtual void BaseAdd (ConfigurationElement element, bool throwIfExists)
		{
//			if (throwIfExists && BaseIndexOf (element) != -1)
//				throw new ConfigurationException ("Duplicate element in collection");
			list.Add (element);
			modified = true;
		}

		protected virtual void BaseAdd (int index, ConfigurationElement element)
		{
//			if (ThrowOnDuplicate && BaseIndexOf (element) != -1)
//				throw new ConfigurationException ("Duplicate element in collection");
			list.Insert (index, element);
			modified = true;
		}

		protected internal void BaseClear ()
		{
			list.Clear ();
			modified = true;
		}

		protected internal ConfigurationElement BaseGet (int index)
		{
			return (ConfigurationElement) list [index];
		}

		protected internal ConfigurationElement BaseGet (object key)
		{
			int index = IndexOfKey (key);
			if (index != -1) return (ConfigurationElement) list [index];
			else return null;
		}

		protected internal string[] BaseGetAllKeys ()
		{
			string[] keys = new string [list.Count];
			for (int n=0; n<list.Count; n++)
				keys [n] = BaseGetKey (n);
			return keys;
		}

		protected internal string BaseGetKey (int index)
		{
			return GetElementKey ((ConfigurationElement) list[index]).ToString ();
		}

		protected int BaseIndexOf (ConfigurationElement element)
		{
			return list.IndexOf (element);
		}
		
		int IndexOfKey (object key)
		{
			for (int n=0; n<list.Count; n++) {
				if (CompareKeys (GetElementKey ((ConfigurationElement) list[n]), key))
					return n;
			}
			return -1;
		}

		[MonoTODO]
		protected internal bool BaseIsRemoved (object key)
		{
			return false;
		}

		protected internal void BaseRemove (object key)
		{
			int index = IndexOfKey (key);
			if (index != -1) {
				BaseRemoveAt (index);
				modified = true;
			}
		}

		protected internal void BaseRemoveAt (int index)
		{
			ConfigurationElement elem = (ConfigurationElement) list [index];
			if (!IsElementRemovable (elem))
				throw new ConfigurationException ("Element can't be removed from element collection");
			list.RemoveAt (index);
			modified = true;
		}

		protected virtual bool CompareKeys (object key1, object key2)
		{
			return object.Equals (key1, key2);
		}

		public void CopyTo (ConfigurationElement[] array, int index)
		{
			list.CopyTo (array, index);
		}
		
		protected abstract ConfigurationElement CreateNewElement ();

		protected virtual ConfigurationElement CreateNewElement (string elementName)
		{
			return CreateNewElement ();
		}
		
		public override bool Equals (object compareTo)
		{
			ConfigurationElementCollection other = compareTo as ConfigurationElementCollection;
			if (other == null) return false;
			if (GetType() != other.GetType()) return false;
			if (Count != other.Count) return false;
			
			for (int n=0; n<Count; n++) {
				if (!BaseGet (n).Equals (other.BaseGet (n)))
					return false;
			}
			return true;
		}

		protected abstract object GetElementKey (ConfigurationElement element);

		public override int GetHashCode ()
		{
			int code = 0;
			for (int n=0; n<Count; n++)
				code += BaseGet (n).GetHashCode ();
			return code;
		}
		
		void ICollection.CopyTo (Array arr, int index)
		{
			list.CopyTo (arr, index);
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		[MonoTODO ("Do something with this")]
		protected virtual bool IsElementName (string elementName)
		{
			return false;
		}

		protected virtual bool IsElementRemovable (ConfigurationElement element)
		{
			return true;
		}

		protected internal override bool IsModified ()
		{
			return modified;
		}

		internal override bool HasValues ()
		{
			return list.Count > 0;
		}

		[MonoTODO ("parentItem.GetType().Name ??")]
		protected internal override void Reset (ConfigurationElement parentElement, object context)
		{
			ConfigurationElementCollection parent = (ConfigurationElementCollection) parentElement;
			for (int n=0; n<parent.Count; n++)
			{
				ConfigurationElement parentItem = parent.BaseGet (n);
				ConfigurationElement item = CreateNewElement (parentItem.GetType().Name);
				item.Reset (parentItem, context);
				BaseAdd (item);
			}
			modified = false;
		}

		protected internal override void ResetModified ()
		{
			modified = false;
		}

		[MonoTODO ("Support for BasicMap. Return value.")]
		protected internal override bool Serialize (XmlWriter writer, bool serializeCollectionKey)
		{
			if (serializeCollectionKey) {
				return base.Serialize (writer, serializeCollectionKey);
			}
			
			if (emitClear)
				writer.WriteElementString ("clear","");
			
			if (removed != null)
				for (int n=0; n<removed.Count; n++) {
					writer.WriteStartElement ("remove");
					((ConfigurationElement)removed[n]).Serialize (writer, true);
					writer.WriteEndElement ();
				}
			
			for (int n=0; n<list.Count; n++) {
				ConfigurationElement elem = (ConfigurationElement) list [n];
				elem.SerializeToXmlElement (writer, "add");
			}
			return true;
		}

		protected override bool HandleUnrecognizedElement (string elementName, XmlReader reader)
		{
			if (elementName == "clear") {
				BaseClear ();
				emitClear = true;
				modified = false;
				return true;
			}
			else if (elementName == "remove") {
				ConfigurationElement elem = CreateNewElement ();
				elem.Deserialize (reader, true);
				BaseRemove (GetElementKey (elem));
				modified = false;
				return true;
			}
			else if (elementName == "add") {
				ConfigurationElement elem = CreateNewElement ();
				elem.Deserialize (reader, false);
				BaseAdd (elem);
				modified = false;
				return true;
			}
			
			return false;
		}
		
		[MonoTODO ("CreateNewElement?, serializeCollectionKey?")]
		protected internal override void UnMerge (ConfigurationElement sourceElement, ConfigurationElement parentElement, bool serializeCollectionKey, object context, ConfigurationUpdateMode updateMode)
		{
			ConfigurationElementCollection source = (ConfigurationElementCollection) sourceElement;
			ConfigurationElementCollection parent = (ConfigurationElementCollection) parentElement;
			
			for (int n=0; n<source.Count; n++) {
				ConfigurationElement sitem = source.BaseGet (n);
				object key = source.GetElementKey (sitem);
				ConfigurationElement pitem = parent.BaseGet (key) as ConfigurationElement;
				if (pitem != null && updateMode != ConfigurationUpdateMode.Full) {
					ConfigurationElement nitem = CreateNewElement ();
					nitem.UnMerge (sitem, pitem, serializeCollectionKey, context, ConfigurationUpdateMode.Minimal);
					if (nitem.HasValues ())
						BaseAdd (nitem);
				}
				else
					BaseAdd (sitem);
			}
			
			if (updateMode == ConfigurationUpdateMode.Full)
				EmitClear = true;
			else {
				for (int n=0; n<parent.Count; n++) {
					ConfigurationElement pitem = parent.BaseGet (n);
					object key = parent.GetElementKey (pitem);
					if (source.IndexOfKey (key) == -1) {
						if (removed == null) removed = new ArrayList ();
						removed.Add (pitem);
					}
				}
			}
		}

		#endregion // Methods
        }
}

#endif
