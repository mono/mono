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
using System.Data.Common;

namespace Mono.Data.TdsClient.Internal {
	internal class TdsPacketColumnInfoResult : TdsPacketResult, IEnumerable
	{
		#region Fields

		ArrayList list;
		ArrayList columnTypes;
		
		#endregion // Fields

		#region Constructors

		public TdsPacketColumnInfoResult ()
			: base (TdsPacketSubType.ColumnNameToken)
		{
			list = new ArrayList ();
			columnTypes = new ArrayList ();
		}

		#endregion // Constructors

		#region Properties

		public SchemaInfo[] Schema {
			get { return (SchemaInfo[]) list.ToArray (typeof (SchemaInfo)); }
		}

		public SchemaInfo this [int index] {
			get { return (SchemaInfo) list[index]; }
			set { list[index] = value; }
		}

		public ArrayList ColumnTypes {
			get { return columnTypes; }
		}

		#endregion // Properties

		#region Methods

		public int Add (SchemaInfo schema)
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

		public int Add (TdsColumnType columnType)
		{
			return columnTypes.Add (columnType);
		}

		#endregion // Methods
	}
}
