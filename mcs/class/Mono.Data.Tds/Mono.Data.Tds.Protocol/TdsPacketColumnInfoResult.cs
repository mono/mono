//
// Mono.Data.Tds.Protocol.TdsPacketColumnInfoResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using Mono.Data.Tds.Protocol;
using System.Collections;

namespace Mono.Data.Tds.Protocol {
	public class TdsPacketColumnInfoResult : TdsPacketResult, IEnumerable
	{
		#region Fields

		ArrayList list;
		
		#endregion // Fields

		#region Constructors

		public TdsPacketColumnInfoResult ()
			: base (TdsPacketSubType.ColumnInfo)
		{
			list = new ArrayList ();
		}

		#endregion // Constructors

		#region Properties

		public TdsSchemaInfo this [int index] {
			get { return (TdsSchemaInfo) list[index]; }
			set { list[index] = value; }
		}

		#endregion // Properties

		#region Methods

		public int Add (TdsSchemaInfo schema)
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
