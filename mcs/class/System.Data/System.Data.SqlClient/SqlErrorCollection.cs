//
// System.Data.SqlClient.SqlErrorCollection.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002
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
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices;

namespace System.Data.SqlClient {
	[ListBindable (false)]
	[Serializable]
	public sealed class SqlErrorCollection : ICollection, IEnumerable
	{
		#region Fields

		ArrayList list = new ArrayList();

		#endregion // Fields

		#region Constructors

		internal SqlErrorCollection () 
		{
		}

		internal SqlErrorCollection (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state) 
		{
			Add (theClass, lineNumber, message, number, procedure, server, source, state);
		}

		#endregion // Constructors

		#region Properties
                
		public int Count {
			get { return list.Count; }			  
		}

		bool ICollection.IsSynchronized {
			get { return list.IsSynchronized; }
		}

		object ICollection.SyncRoot {
			get { return list.SyncRoot; }
		}

		public SqlError this[int index] {
			get { return (SqlError) list [index]; }
		}

		#endregion

		#region Methods
		
		internal void Add(SqlError error) 
		{
			list.Add (error);
		}

		internal void Add(byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state) 
		{
			SqlError error = new SqlError (theClass, lineNumber, message, number, procedure, server, source, state);
			Add (error);
		}

		public void CopyTo (Array array, int index) 
		{
			list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator() 
		{
			return list.GetEnumerator ();
		}

#if NET_2_0
		public void CopyTo (SqlError[] array, int index)
		{
			list.CopyTo (array, index);
		}

#endif // NET_2_0

		#endregion		
	}
}
