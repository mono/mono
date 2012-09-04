// 
// System.Xml.Serialization.SchemaImporterExtensionCollection.cs 
//
// Author:
//   Lluis Sanchez Gual (lluis@novell.com)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) Novell, Inc., 2004, 2006
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

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Xml.Serialization.Advanced
{
	public class SchemaImporterExtensionCollection : CollectionBase
	{
		Dictionary<string,SchemaImporterExtension> named_items =
			new Dictionary<string,SchemaImporterExtension> ();

		public SchemaImporterExtensionCollection ()
		{
		}

		public int Add (SchemaImporterExtension extension)
		{
			if (extension == null)
				throw new ArgumentNullException ("extension");
			return List.Add (extension);
		}

		public int Add (string name, Type type)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (type == null)
				throw new ArgumentNullException ("type");
			if (!type.IsSubclassOf (typeof (SchemaImporterExtension)))
				throw new ArgumentException ("The type argument must be subclass of SchemaImporterExtension.");

			SchemaImporterExtension e = (SchemaImporterExtension) Activator.CreateInstance (type);
			if (named_items.ContainsKey (name))
				throw new InvalidOperationException (String.Format ("A SchemaImporterExtension keyed by '{0}' already exists.", name));
			int ret = Add (e);
			named_items.Add (name, e);
			return ret;
		}
	
		public new void Clear ()
		{
			named_items.Clear ();
			List.Clear ();
		}
	
		public bool Contains (SchemaImporterExtension extension)
		{
			if (extension == null)
				throw new ArgumentNullException ("extension");
			foreach (SchemaImporterExtension e in List)
				if (extension.Equals (e))
					return true;
			return false;
		}
		
		public void CopyTo (SchemaImporterExtension [] array, int index)
		{
			List.CopyTo (array, index);
		}
		
		public int IndexOf (SchemaImporterExtension extension)
		{
			if (extension == null)
				throw new ArgumentNullException ("extension");
			int i = 0;
			foreach (SchemaImporterExtension e in List) {
				if (extension.Equals (e)) {
					return i;
				}
				i++;
			}
			return -1;
		}
		
		public void Insert (int index, SchemaImporterExtension extension)
		{
			if (extension == null)
				throw new ArgumentNullException ("extension");
			List.Insert (index, extension);
		}
		
		public void Remove (SchemaImporterExtension extension)
		{
			int idx = IndexOf (extension);
			if (idx >= 0)
				List.RemoveAt (idx);
		}
		
		public void Remove (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (!named_items.ContainsKey (name))
				return;
			SchemaImporterExtension e = named_items [name];
			Remove (e);
			named_items.Remove (name);
		}
		
		public SchemaImporterExtension this [int index] 
		{
			get { return (SchemaImporterExtension) List [index]; }
			set { List [index] = value; }
		}
	}
}

#endif
