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

#if NET_2_0

using System.Collections;
using System.Xml;

namespace System.Configuration 
{
	public abstract class ConfigurationElementCollection : ConfigurationElement, ICollection, IEnumerable
	{
		#region Constructors

		[MonoTODO]
		protected ConfigurationElementCollection ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		protected virtual ConfigurationElementCollectionType CollectionType {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual int Count {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected virtual string ElementName {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool EmitClear {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool ICollection.IsSynchronized {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		object ICollection.SyncRoot {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected virtual bool ThrowOnDuplicate {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected virtual void BaseAdd (ConfigurationElement element)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void BaseAdd (ConfigurationElement element, bool throwIfExists)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void BaseAdd (int index, ConfigurationElement element)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal void BaseClear ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal ConfigurationElement BaseGet (int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal ConfigurationElement BaseGet (object key)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal string[] BaseGetAllKeys ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal string BaseGetKey (int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected int BaseIndexOf (ConfigurationElement element)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal bool BaseIsRemoved (object key)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal void BaseRemove (object key)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal void BaseRemoveAt (int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual bool CompareKeys (object key1, object key2)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (ConfigurationElement[] array, int index)
		{
			throw new NotImplementedException ();
		}
		
		protected abstract ConfigurationElement CreateNewElement ();

		[MonoTODO]
		protected virtual ConfigurationElement CreateNewElement (string elementName)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override bool Equals (object compareTo)
		{
			throw new NotImplementedException ();
		}
		

		protected abstract object GetElementKey (ConfigurationElement element);

		[MonoTODO]
		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override bool HandleUnrecognizedElement (string elementName, XmlReader reader)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		void ICollection.CopyTo (Array arr, int index)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual bool IsElementName (string elementName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual bool IsElementRemovable (ConfigurationElement element)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override bool IsModified ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void Reset (ConfigurationElement parentElement, object context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void ResetModified ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override bool Serialize (XmlWriter writer, bool serializeCollectionKey)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void UnMerge (ConfigurationElement sourceElement, ConfigurationElement parentElement, bool serializeCollectionKey, object context, ConfigurationUpdateMode updateMode)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
        }
}

#endif
