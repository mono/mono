//
// System.Data.Common.TdsSchemaInfo.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;

namespace Mono.Data.Tds.Protocol {
	public class TdsSchemaInfo : Hashtable
	{
		#region Fields

		Hashtable table;

		#endregion // Fields

		#region Constructors

		public TdsSchemaInfo ()
			: base ()
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
