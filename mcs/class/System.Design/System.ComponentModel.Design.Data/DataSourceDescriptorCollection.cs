//
// System.ComponentModel.Design.Data.DataSourceDescriptor
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2007 Novell, Inc.
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

namespace System.ComponentModel.Design.Data
{
	public class DataSourceDescriptorCollection : CollectionBase
	{
		public DataSourceDescriptorCollection ()
		{
		}

		[MonoTODO]
		public DataSourceDescriptor this [int index] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public int Add (DataSourceDescriptor value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains (DataSourceDescriptor value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (DataSourceDescriptor [] array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int IndexOf (DataSourceDescriptor value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Insert (int index, DataSourceDescriptor value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove (DataSourceDescriptor value)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
