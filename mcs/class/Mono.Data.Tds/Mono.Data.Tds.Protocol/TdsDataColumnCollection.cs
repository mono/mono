//
// Mono.Data.Tds.Protocol.TdsDataColumnCollection.cs
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

using Mono.Data.Tds.Protocol;
using System.Collections;

namespace Mono.Data.Tds.Protocol {
	public class TdsDataColumnCollection : IEnumerable
	{
		#region Fields

		ArrayList list;
		
		#endregion // Fields

		#region Constructors

		public TdsDataColumnCollection ()
		{
			list = new ArrayList ();
		}

		#endregion // Constructors

		#region Properties

		public TdsDataColumn this [int index] {
			get { return (TdsDataColumn) list[index]; }
			set { list[index] = value; }
		}

		public int Count {
			get { return list.Count; }
		}

		#endregion // Properties

		#region Methods

		public int Add (TdsDataColumn schema)
		{
			int index;
			index = list.Add (schema);
#if NET_2_0
			schema.ColumnOrdinal = index;
#else
			schema["ColumnOrdinal"] = index;
#endif
			return index;
		}

		public void Add (TdsDataColumnCollection columns)
		{
			foreach (TdsDataColumn col in columns)
				Add (col);
		}
		
		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public void Clear ()
		{
			list.Clear ();
		}
		
		#endregion // Methods
	}
}
