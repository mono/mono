//
// Mono.Data.TdsClient.Internal.TdsPacketColumnInfoResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;
using System.Data;

namespace Mono.Data.TdsClient.Internal {
	internal class TdsPacketColumnInfoResult : TdsPacketResult, IEnumerable
	{
		#region Fields

		ArrayList list;
		
		#endregion // Fields

		#region Constructors

		public TdsPacketColumnInfoResult ()
			: base (TdsPacketSubType.ColumnNameToken)
		{
			list = new ArrayList ();
		}

		#endregion // Constructors

		#region Properties

		public TdsColumnSchema this [int index] {
			get { return (TdsColumnSchema) list[index]; }
			set { list[index] = value; }
		}

		#endregion // Properties

		#region Methods

		public int Add (TdsColumnSchema schema)
		{
			int index;
			index = list.Add (schema);
			schema.ColumnOrdinal = index;
			return index;
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		#endregion // Methods
	}
}
