//
// System.Data.Common.TdsDataColumn.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;

namespace Mono.Data.Tds.Protocol {
	public class TdsDataColumn : Hashtable
	{
		#region Constructors

		public TdsDataColumn ()
		{
			SetDefaultValues ();
		}

		#endregion // Constructors

		#region Methods

		private void SetDefaultValues ()
		{
			Add ("AllowDBNull", true);
			Add ("ColumnOrdinal", 0);
			Add ("IsAutoIncrement", false);
			Add ("IsIdentity", false);
			Add ("IsReadOnly", false);
			Add ("IsRowVersion", false);
		}

		#endregion // Methods
	}
}
