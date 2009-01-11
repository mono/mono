//
// DirectoryAttributeCollection.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.
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

namespace System.DirectoryServices.Protocols
{
	[MonoTODO]
	public class DirectoryAttributeCollection : CollectionBase
	{
		internal DirectoryAttributeCollection (DirectoryAttribute [] attributes)
		{
			list = new ArrayList (attributes);
		}

		ArrayList list;

		public DirectoryAttribute this [int index] {
			get { return (DirectoryAttribute) list [index]; }
			set { list [index] = value; }
		}

		public int Add (DirectoryAttribute attribute)
		{
			return list.Add (attribute);
		}

		public void AddRange (DirectoryAttribute [] attributes)
		{
			list.AddRange (attributes);
		}

		public void AddRange (DirectoryAttributeCollection attributeCollection)
		{
			list.Add (attributeCollection.list);
		}

		public bool Contains (DirectoryAttribute value)
		{
			return list.Contains (value);
		}

		public void CopyTo (DirectoryAttribute [] array, int index)
		{
			list.CopyTo (array, index);
		}

		public int IndexOf (DirectoryAttribute value)
		{
			return list.IndexOf (value);
		}

		public void Insert (int index, DirectoryAttribute value)
		{
			list.Insert (index, value);
		}

		[MonoTODO ("verify")]
		protected override void OnValidate (object value)
		{
			if (!(value is DirectoryAttribute))
				throw new ArgumentException ("value must be a DirectoryAttribute");
		}

		public void Remove (DirectoryAttribute value)
		{
			list.Remove (value);
		}
	}
}
