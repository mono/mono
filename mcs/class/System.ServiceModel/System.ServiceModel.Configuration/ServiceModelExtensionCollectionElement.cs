//
// ServiceModelExtensionCollectionElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace System.ServiceModel.Configuration
{
	public abstract class ServiceModelExtensionCollectionElement<TServiceModelExtensionElement> : ConfigurationElement,
		ICollection<TServiceModelExtensionElement>,
		IEnumerable<TServiceModelExtensionElement>,
		IEnumerable
		where TServiceModelExtensionElement : ServiceModelExtensionElement
	{
		internal ServiceModelExtensionCollectionElement ()
		{
		}

		ConfigurationPropertyCollection properties;

		KeyedByTypeCollection<TServiceModelExtensionElement> _list = new KeyedByTypeCollection<TServiceModelExtensionElement> ();
		bool is_modified;

		public virtual void Add (TServiceModelExtensionElement element)
		{
			is_modified = true;
			_list.Add (element);
		}
		
		public virtual bool CanAdd (TServiceModelExtensionElement element) {
			throw new NotImplementedException ();
		}

		public void Clear ()
		{
			is_modified = true;
			_list.Clear ();
		}

		public bool Contains (TServiceModelExtensionElement element)
		{
			return _list.Contains (element);
		}

		public bool ContainsKey (string elementName)
		{
			throw new NotImplementedException ();
		}

		public bool ContainsKey (Type elementType)
		{
			return _list.Contains (elementType);
		}

		public void CopyTo (TServiceModelExtensionElement[] elements,
			int start)
		{
			throw new NotImplementedException ();
		}

		public IEnumerator<TServiceModelExtensionElement> GetEnumerator () {
			for (int i = 0; i < Count; i++)
				yield return this [i];
		}

		public bool Remove (TServiceModelExtensionElement element)
		{
			is_modified = true;
			return _list.Remove (element.GetType ());
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		bool ICollection<TServiceModelExtensionElement>.IsReadOnly {
			get { throw new NotImplementedException (); }
		}

		public TServiceModelExtensionElement this [int index] {
			get {
				return _list [index];
			}
		}

		public TServiceModelExtensionElement this [Type extensionType] {
			get {
				if (_list.Contains (extensionType))
					return _list [extensionType];
				return null;
			}
		}

		protected override void DeserializeElement (XmlReader reader, bool serializeCollectionKey)
		{
			base.DeserializeElement (reader, serializeCollectionKey);
		}

		protected override bool OnDeserializeUnrecognizedElement (string elementName, XmlReader reader) {
			TServiceModelExtensionElement ext= DeserializeExtensionElement (elementName, reader);
			if (ext == null)
				return false;
			Add (ext);
			return true;
		}

		internal virtual TServiceModelExtensionElement DeserializeExtensionElement (string elementName, XmlReader reader) {
			return null;
		}

		public int Count {
			get { return _list.Count; }
		}

		protected override void Reset (ConfigurationElement parentElement)
		{
			base.Reset (parentElement);
			var parent = (ServiceModelExtensionCollectionElement<TServiceModelExtensionElement>) parentElement;

#if true
			foreach (var item in parent)
				Add (item);
#else // FIXME: no way to call Reset() on item (hence disabled now)
			// It is based on ConfigurationElementCollection.Reset()
			for (int n=0; n<parent.Count; n++)
			{
				var parentItem = parent [n];
//				ConfigurationElement item = CreateNewElementInternal (null);
				var item = (TServiceModelExtensionElement) Activator.CreateInstance (parentItem.GetType ()); // FIXME: there is no assureance on default constructor existence.
				item.Reset (parentItem);
				Add (item);
			}
#endif
		}

		protected override bool IsModified ()
		{
			return is_modified;
		}

		protected override void ResetModified ()
		{
			is_modified = false;
		}

		protected void SetIsModified ()
		{
			is_modified = true;
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (properties == null) {
					properties = base.Properties;
				}
				return properties;
			}
		}
	}
}
