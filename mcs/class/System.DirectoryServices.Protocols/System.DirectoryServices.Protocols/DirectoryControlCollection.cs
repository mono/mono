//
// DirectoryControlCollection.cs
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
	public class DirectoryControlCollection : CollectionBase
	{
		public DirectoryControlCollection ()
		{
		}

		public DirectoryControl this [int index] {
			get { return (DirectoryControl) List [index]; }
			set { List [index] = value; }
		}

		public int Add (DirectoryControl control)
		{
			return List.Add (control);
		}

		public void AddRange (DirectoryControl [] controls)
		{
			foreach (var c in controls)
				List.Add (c);
		}

		public void AddRange (DirectoryControlCollection controlCollection)
		{
			foreach (var c in controlCollection)
				List.Add (c);
		}

		public bool Contains (DirectoryControl value)
		{
			return List.Contains (value);
		}

		public void CopyTo (DirectoryControl [] array, int index)
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (DirectoryControl value)
		{
			return List.IndexOf (value);
		}

		public void Insert (int index, DirectoryControl value)
		{
			List.Insert (index, value);
		}

		[MonoTODO ("verify")]
		protected override void OnValidate (object value)
		{
			if (!(value is DirectoryControl))
				throw new ArgumentException ("value must be a DirectoryControl");
		}

		public void Remove (DirectoryControl value)
		{
			List.Remove (value);
		}
	}
}
