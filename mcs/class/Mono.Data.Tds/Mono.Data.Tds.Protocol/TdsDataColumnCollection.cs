//
// Mono.Data.Tds.Protocol.TdsDataColumnCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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
			schema["ColumnOrdinal"] = index;
			return index;
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		#endregion // Methods
	}
}
