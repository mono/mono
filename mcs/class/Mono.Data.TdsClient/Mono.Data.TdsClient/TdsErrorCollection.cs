//
// Mono.Data.TdsClient.TdsErrorCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
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

using Mono.Data.Tds.Protocol;
using System;
using System.Collections;

namespace Mono.Data.TdsClient {
        public class TdsErrorCollection : ICollection, IEnumerable
	{
		#region Fields

		ArrayList list = new ArrayList ();

		#endregion // Fields

		#region Constructors

		internal TdsErrorCollection ()
		{
		}

		internal TdsErrorCollection (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state)
		{
			Add (theClass, lineNumber, message, number, procedure, server, source, state);
		}

		#endregion // Constructors

		#region Properties

		public int Count {
			get { return list.Count; }
		}

		public TdsError this [int index] {
			get { return (TdsError) list[index]; }
		}

		bool ICollection.IsSynchronized {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		object ICollection.SyncRoot {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		internal void Add (TdsError error)
		{
			list.Add (error);
		}

		internal void Add (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state)
		{
			Add (new TdsError (theClass, lineNumber, message, number, procedure, server, source, state));
		}

		[MonoTODO]
		public void CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		#endregion // Methods
	}
}
