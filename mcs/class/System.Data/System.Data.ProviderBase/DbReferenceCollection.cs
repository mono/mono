//
// System.Data.ProviderBase.DbReferenceCollection
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Data.Common;

namespace System.Data.ProviderBase {
	public abstract class DbReferenceCollection : IEnumerable
	{
		#region Constructors
	
		[MonoTODO]
		protected DbReferenceCollection ()
		{
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public void Add (object value)
		{
			throw new NotImplementedException ();
		}

		public abstract void Add (object value, int tag);

		[MonoTODO]
		protected void AddItem (object value, int tag)
		{
			Add (value, tag);
		}

		[MonoTODO]
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Notify (int message, int tag, object connectionInternal)
		{
			throw new NotImplementedException ();
		}

		protected abstract bool NotifyItem (int message, object value, int tag, object connectionInternal);

		[MonoTODO]
		public void Purge ()
		{
			throw new NotImplementedException ();
		}

		public abstract void Remove (object value);

		[MonoTODO]
		protected void RemoveItem (object value)
		{
			Remove (value);
		}

		#endregion // Methods
	}
}

#endif
