//
// Mono.Data.SybaseClient.SybaseErrorCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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
using System.Data;
using System.Runtime.InteropServices;

namespace Mono.Data.SybaseClient {
	[MonoTODO]
	public sealed class SybaseErrorCollection : ICollection, IEnumerable
	{
		#region Fields

		ArrayList list = new ArrayList();

		#endregion // Fields

		#region Constructors

		internal SybaseErrorCollection () {
		}

		internal SybaseErrorCollection (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state) 
		{
			Add (theClass, lineNumber, message, number, procedure, server, source, state);
		}

		#endregion // Constructors

		#region Properties
                
		public int Count {
			get { return list.Count; }			  
		}

		bool ICollection.IsSynchronized {
			get { throw new NotImplementedException (); }			  
		}

		object ICollection.SyncRoot {
			get { throw new NotImplementedException (); }			  
		}

		public SybaseError this[int index] 
		{
			get { return (SybaseError) list[index]; }
		}

		#endregion // Properties

		#region Methods
		
		internal void Add(SybaseError error) 
		{
			list.Add(error);
		}

		internal void Add(byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state) 
		{
			SybaseError error = new SybaseError(theClass, lineNumber, message, number, procedure, server, source, state);
			Add(error);
		}

		public void CopyTo (Array array, int index) 
		{
			list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
