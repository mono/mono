//
// System.Data.Sql.SqlTriggerContextBase
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;
using System.Data.SqlTypes;

namespace System.Data.Sql {
	public abstract class SqlTriggerContextBase
	{
		#region Constructors

		protected SqlTriggerContextBase ()
		{
		}

		#endregion // Constructors

		#region Properties

		public abstract bool[] ColumnsUpdated { get; }
		public abstract SqlChars EventData { get; }
		public abstract TriggerAction TriggerAction { get; }

		#endregion // Properties
	}
}

#endif
