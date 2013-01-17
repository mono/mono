//
// System.Configuration.ConfigurationElementCollection.cs
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
// 	Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (C) Tim Coleman, 2004
// Copyright (c) 2012 Xamarin Inc. (http://www.xamarin.com)

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

#if NET_2_0

using System.Collections;
using System.Diagnostics;
using System.Xml;

namespace System.Configuration 
{
	[DebuggerDisplayAttribute ("Count = {Count}")]
	public abstract partial class ConfigurationElementCollection : ConfigurationElement, ICollection, IEnumerable
	{
		ArrayList list = new ArrayList ();
		ArrayList removed;
		ArrayList inherited;
		bool emitClear;
		bool modified;
		IComparer comparer;
		int inheritedLimitIndex;
		
		string addElementName = "add";
		string clearElementName = "clear";
		string removeElementName = "remove";
		
		#region Constructors

		protected ConfigurationElementCollection ()
		{
		}

		protected ConfigurationElementCollection (IComparer comparer)
		{
			this.comparer = comparer;
		}

		internal override void InitFromProperty (PropertyInformation propertyInfo)
		{
			ConfigurationCollectionAttribute colat = propertyInfo.Property.CollectionAttribute;
	
			if (colat == null)
				colat = Attribute.GetCustomAttribute (propertyInfo.Type, typeof (ConfigurationCollectionAttribute)) as ConfigurationCollectionAttribute;

			if (colat != null) {
				addElementName = colat.AddItemName;
				clearElementName = colat.ClearItemsName;
				removeElementName = colat.RemoveItemName;
			}
			base.InitFromProperty (propertyInfo);
		}
		
		#endregion // Constructors

		#region Properties
		
		public virtual ConfigurationElementCollectionType CollectionType {
			get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
		}
		
		bool IsBasic {
			get {
				return CollectionType == ConfigurationElementCollectionType.BasicMap ||
						CollectionType == ConfigurationElementCollectionType.BasicMapAlternate;
			}
		}
		
		bool IsAlternate {
			get {
				return CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate ||
						CollectionType == ConfigurationElementCollectionType.BasicMapAlternate;
			}
		}

		public int Count {
			get { return list.Count; }
		}

		protected virtual string ElementName {
			get { return string.Empty; }
		}

		public bool EmitClear {
			get { return emitClear; }
			set { emitClear = value; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public object SyncRoot {
			get { return this; }
		}

		protected virtual bool ThrowOnDuplicate {
			get {
				if (CollectionType != ConfigurationElementCollectionType.AddRemoveClearMap &&
				    CollectionType != ConfigurationElementCollectionType.AddRemoveClearMapAlternate)
					return false;
				
				return true;
			}
		}
		
		protected internal string AddElementName {
			get { return addElementName; }
			set { addElementName = value; }
		}

		protected internal string ClearElementName {
			get { return clearElementName; }
			set { clearElementName = value; }
		}

		protected internal string RemoveElementName {
			get { return removeElementName; }
			set { removeElementName = value; }
		}

		#endregion // Properties

		#region Methods

		protected virtual void BaseAdd (ConfigurationElement element)
		{
			BaseAdd (element, ThrowOnDuplicate);
		}

		protected void BaseAdd (ConfigurationElement element, bool throwIfExists)
		{
			if (IsReadOnly ())
				throw new ConfigurationErrorsException ("Collection is read only.");
			
			if (IsAlternate) {
				list.Insert (inheritedLimitIndex, element);
				inheritedLimitIndex++;
			}
			else {
				int old_index = IndexOfKey (GetElementKey (element));
				if (old_index >= 0) {
					if (element.Equals (list [old_index]))
						return;
					if (throwIfExists)
						throw new ConfigurationErrorsException ("Duplicate element in collection");
					list.RemoveAt (old_index);
				}
				list.Add (element);
			}

			modified = true;
		}

		protected virtual void BaseAdd (int index, ConfigurationElement element)
		{
			if (ThrowOnDuplicate && BaseIndexOf (element) != -1)
				throw new ConfigurationErrorsException ("Duplicate element in collection");
			if (IsReadOnly ())
				throw new ConfigurationErrorsException ("Collection is read only.");
			
			if (IsAlternate && (index > inheritedLimitIndex))
				throw new ConfigurationErrorsException ("Can't insert new elements below the inherited elements.");
			if (!IsAlternate && (index <= inheritedLimitIndex))
				throw new ConfigurationErrorsException ("Can't insert new elements above the inherited elements.");
			
			list.Insert (index, element);
			modified = true;
		}

		protected internal void BaseClear ()
		{
			if (IsReadOnly ())
				throw new ConfigurationErrorsException ("Collection is read only.");
				
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

		protected internal object[] BaseGetAllKeys ()
		{
			object[] keys = new object [list.Count];
			for (int n=0; n<list.Count; n++)
				keys [n] = BaseGetKey (n);
			return keys;
		}

		protected internal object BaseGetKey (int index)
		{
			if (index < 0 || index >= list.Count)
				throw new ConfigurationErrorsException (String.Format ("Index {0} is out of range", index));

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

		protected internal bool BaseIsRemoved (object key)
		{
			if (removed == null)
				return false;
			foreach (ConfigurationElement elem in removed) {
				if (CompareKeys (GetElementKey (elem), key))
					return true;
			}
			return false;
		}

		protected internal void BaseRemove (object key)
		{
			if (IsReadOnly ())
				throw new ConfigurationErrorsException ("Collection is read only.");
				
			int index = IndexOfKey (key);
			if (index != -1) {
				BaseRemoveAt (index);
				modified = true;
			}
		}

		protected internal void BaseRemoveAt (int index)
		{
			if (IsReadOnly ())
				throw new ConfigurationErrorsException ("Collection is read only.");
				
			ConfigurationElement elem = (ConfigurationElement) list [index];
			if (!IsElementRemovable (elem))
				throw new ConfigurationErrorsException ("Element can't be removed from element collection.");
			
			if (inherited != null && inherited.Contains (elem))
				throw new ConfigurationErrorsException ("Inherited items can't be removed.");
			
			list.RemoveAt (index);
			
			if (IsAlternate) {
				if (inheritedLimitIndex > 0)
					inheritedLimitIndex--;
			}

			modified = true;
		}

		bool CompareKeys (object key1, object key2)
		{
			if (comparer != null)
				return comparer.Compare (key1, key2) == 0;
			else
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
		
		ConfigurationElement CreateNewElementInternal (string elementName)
		{
			ConfigurationElement elem;
			if (elementName == null)
				elem = CreateNewElement ();
			else
				elem = CreateNewElement (elementName);
			elem.Init ();
			return elem;
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
		
		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		protected virtual bool IsElementName (string elementName)
		{
			return false;
		}

		protected virtual bool IsElementRemovable (ConfigurationElement element)
		{
			return !IsReadOnly ();
		}

		protected internal override bool IsModified ()
		{
			if (modified)
				return true;

			for (int n=0; n<list.Count; n++) {
				ConfigurationElement elem = (ConfigurationElement) list [n];
				if (!elem.IsModified ())
					continue;
				modified = true;
				break;
			}

			return modified;
		}

		[MonoTODO]
		public override bool IsReadOnly ()
		{
			return base.IsReadOnly ();
		}

		internal override void PrepareSave (ConfigurationElement parentElement, ConfigurationSaveMode mode)
		{
			var parent = (ConfigurationElementCollection)parentElement;
			base.PrepareSave (parentElement, mode);

			for (int n=0; n<list.Count; n++) {
				ConfigurationElement elem = (ConfigurationElement) list [n];
				object key = GetElementKey (elem);
				ConfigurationElement pitem = parent != null ? parent.BaseGet (key) as ConfigurationElement : null;

				elem.PrepareSave (pitem, mode);
			}
		}

		internal override bool HasValues (ConfigurationElement parentElement, ConfigurationSaveMode mode)
		{
			var parent = (ConfigurationElementCollection)parentElement;

			if (mode == ConfigurationSaveMode.Full)
				return list.Count > 0;

			for (int n=0; n<list.Count; n++) {
				ConfigurationElement elem = (ConfigurationElement) list [n];
				object key = GetElementKey (elem);
				ConfigurationElement pitem = parent != null ? parent.BaseGet (key) as ConfigurationElement : null;

				if (elem.HasValues (pitem, mode))
					return true;
			}

			return false;
		}

		protected internal override void Reset (ConfigurationElement parentElement)
		{
			bool basic = IsBasic;
				
			ConfigurationElementCollection parent = (ConfigurationElementCollection) parentElement;
			for (int n=0; n<parent.Count; n++)
			{
				ConfigurationElement parentItem = parent.BaseGet (n);
				ConfigurationElement item = CreateNewElementInternal (null);
				item.Reset (parentItem);
				BaseAdd (item);
				
				if (basic) {
					if (inherited == null)
						inherited = new ArrayList ();
					inherited.Add (item);
				}
			}
			if (IsAlternate)
				inheritedLimitIndex = 0;
			else
				inheritedLimitIndex = Count - 1;
			modified = false;
		}

		protected internal override void ResetModified ()
		{
			modified = false;
			for (int n=0; n<list.Count; n++) {
				ConfigurationElement elem = (ConfigurationElement) list [n];
				elem.ResetModified ();
			}
		}

		[MonoTODO]
		protected internal override void SetReadOnly ()
		{
			base.SetReadOnly ();
		}

		protected internal override bool SerializeElement (XmlWriter writer, bool serializeCollectionKey)
		{
			if (serializeCollectionKey) {
				return base.SerializeElement (writer, serializeCollectionKey);
			}
			
			bool wroteData = false;
			
			if (IsBasic)
			{
				for (int n=0; n<list.Count; n++) {
					ConfigurationElement elem = (ConfigurationElement) list [n];
					if (ElementName != string.Empty)
						wroteData = elem.SerializeToXmlElement (writer, ElementName) || wroteData;
					else
						wroteData = elem.SerializeElement (writer, false) || wroteData;
				}
			}
			else
			{
				if (emitClear) {
					writer.WriteElementString (clearElementName, "");
					wroteData = true;
				}
				
				if (removed != null) {
					for (int n=0; n<removed.Count; n++) {
						writer.WriteStartElement (removeElementName);
						((ConfigurationElement)removed[n]).SerializeElement (writer, true);
						writer.WriteEndElement ();
					}
					wroteData = wroteData || removed.Count > 0;
				}
				
				for (int n=0; n<list.Count; n++) {
					ConfigurationElement elem = (ConfigurationElement) list [n];
					elem.SerializeToXmlElement (writer, addElementName);
				}
				
				wroteData = wroteData || list.Count > 0;
			}
			return wroteData;
		}

		protected override bool OnDeserializeUnrecognizedElement (string elementName, XmlReader reader)
		{
			if (IsBasic)
			{
				ConfigurationElement elem = null;
				
				if (elementName == ElementName)
					elem = CreateNewElementInternal (null);
				if (IsElementName (elementName))
					elem = CreateNewElementInternal (elementName);

				if (elem != null) {
					elem.DeserializeElement (reader, false);
					BaseAdd (elem);
					modified = false;
					return true;
				}
			}
			else {
				if (elementName == clearElementName) {
					reader.MoveToContent ();
					if (reader.MoveToNextAttribute ())
						throw new ConfigurationErrorsException ("Unrecognized attribute '" + reader.LocalName + "'.");
					reader.MoveToElement ();
					reader.Skip ();
					BaseClear ();
					emitClear = true;
					modified = false;
					return true;
				}
				else if (elementName == removeElementName) {
					ConfigurationElement elem = CreateNewElementInternal (null);
					ConfigurationRemoveElement removeElem = new ConfigurationRemoveElement (elem, this);
					removeElem.DeserializeElement (reader, true);
					BaseRemove (removeElem.KeyValue);
					modified = false;
					return true;
				}
				else if (elementName == addElementName) {
					ConfigurationElement elem = CreateNewElementInternal (null);
					elem.DeserializeElement (reader, false);
					BaseAdd (elem);
					modified = false;
					return true;
				}
			}
			
			return false;
		}
		
		protected internal override void Unmerge (ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode updateMode)
		{
			ConfigurationElementCollection source = (ConfigurationElementCollection) sourceElement;
			ConfigurationElementCollection parent = (ConfigurationElementCollection) parentElement;
			
			for (int n=0; n<source.Count; n++) {
				ConfigurationElement sitem = source.BaseGet (n);
				object key = source.GetElementKey (sitem);
				ConfigurationElement pitem = parent != null ? parent.BaseGet (key) as ConfigurationElement : null;
				ConfigurationElement nitem = CreateNewElementInternal (null);
				if (pitem != null && updateMode != ConfigurationSaveMode.Full) {
					nitem.Unmerge (sitem, pitem, updateMode);
					if (nitem.HasValues (pitem, updateMode))
						BaseAdd (nitem);
				} else {
					nitem.Unmerge (sitem, null, ConfigurationSaveMode.Full);
					BaseAdd (nitem);
				}
			}
			
			if (updateMode == ConfigurationSaveMode.Full)
				EmitClear = true;
			else if (parent != null) {
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
