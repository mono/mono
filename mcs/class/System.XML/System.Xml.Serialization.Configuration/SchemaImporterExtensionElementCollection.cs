//
// SchemaImporterExtensionElementCollection.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
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

#if NET_2_0 && CONFIGURATION_DEP
using System;
using System.Collections;
using System.Configuration;
using System.Globalization;
using System.Xml;
using System.Text;

namespace System.Xml.Serialization.Configuration
{
	[ConfigurationCollection (typeof (SchemaImporterExtensionElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
	public sealed class SchemaImporterExtensionElementCollection : ConfigurationElementCollection
	{
		public SchemaImporterExtensionElement this [int index] {
			get {
				int i = 0;
				if (index >= this.Count || index < 0)
					throw new ArgumentOutOfRangeException ("index is less than 0 or greater than the count of the collection.");
				foreach (SchemaImporterExtensionElement e in this)
					if (i++ == index)
						return e;
				return null; // should not happen
			}
			set {
				if (index >= this.Count || index < 0)
					throw new ArgumentOutOfRangeException ("index is less than 0 or greater than the count of the collection.");
				RemoveAt (index);
				BaseAdd (index, value);
			}
		}
		public new SchemaImporterExtensionElement this [string name] {
			get {
				foreach (SchemaImporterExtensionElement e in this)
					if (e.Name == name)
						return e;
				return null;
			}
			set {
				Remove (name);
				Add (value);
			}
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new SchemaImporterExtensionElement ();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((SchemaImporterExtensionElement) element).Name;
		}

		public void Add (SchemaImporterExtensionElement element)
		{
			BaseAdd (element);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		public int IndexOf (SchemaImporterExtensionElement element)
		{
			int i = 0;
			foreach (SchemaImporterExtensionElement e in this) {
				if (element.Equals (e))
					return i;
				i++;
			}
			return -1;
		}

		public void Remove (string name)
		{
			foreach (SchemaImporterExtensionElement el in this)
				if (name == el.Name) {
					BaseRemove (el);
					break;
				}
		}

		public void Remove (SchemaImporterExtensionElement element)
		{
			foreach (SchemaImporterExtensionElement el in this)
				if (element.Name == el.Name) {
					BaseRemove (el);
					break;
				}
		}

		public void RemoveAt (int i)
		{
			BaseRemoveAt (i);
		}
	}
}

#endif
