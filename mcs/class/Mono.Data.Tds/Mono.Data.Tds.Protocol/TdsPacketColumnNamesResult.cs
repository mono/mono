//
// Mono.Data.TdsClient.Internal.TdsPacketColumnNamesResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;
using System.Data;

namespace Mono.Data.TdsClient.Internal {
	internal class TdsPacketColumnNamesResult : TdsPacketResult, IEnumerable
	{
		#region Fields

		ArrayList list;

		#endregion // Fields

		#region Constructors

		public TdsPacketColumnNamesResult ()
			: base (TdsPacketSubType.ColumnNameToken)
		{
			this.list = new ArrayList ();
		}

		#endregion // Constructors

		#region Properties

		public string this[int index] {
			get { return (string) list[index]; }
			set { list[index] = value; }
		}

		#endregion // Properties

		#region Methods

		public int Add (string columnName)
		{
			return list.Add (columnName);
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		#endregion // Methods
	}
}
