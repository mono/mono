//
// DirectoryAttribute.cs
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
	public class DirectoryAttribute : CollectionBase
	{
		public DirectoryAttribute ()
		{
		}

		public DirectoryAttribute (string name, byte [] value)
		{
			Name = name;
			Add (value);
		}

		public DirectoryAttribute (string name, params object [] values)
		{
			Name = name;
			AddRange (values);
		}

		public DirectoryAttribute (string name, string value)
		{
			Name = name;
			Add (value);
		}

		public DirectoryAttribute (string name, Uri value)
		{
			Name = name;
			Add (value);
		}

		public object this [int index] {
			get { return List [index]; }
			set { List [index] = value; }
		}

		public string Name { get; set; }

		[MonoTODO]
		public int Add (byte [] value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Add (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Add (Uri value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AddRange (object [] values)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (object [] array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object [] GetValues (Type valuesType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int IndexOf (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Insert (int index, byte [] value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Insert (int index, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Insert (int index, Uri value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnValidate (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove (object value)
		{
			throw new NotImplementedException ();
		}
	}
}
