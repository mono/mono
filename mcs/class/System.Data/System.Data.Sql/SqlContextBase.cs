//
// System.Data.Sql.SqlContextBase
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;

namespace System.Data.Sql {
	public abstract class SqlContextBase
	{
		#region Constructors

		[MonoTODO]
		protected SqlContextBase ()
		{
		}

		#endregion // Constructors

		#region Methods

		public abstract ISqlConnection GetConnection ();
		public abstract SqlPipeBase GetPipe ();
		public abstract ISqlResultSet GetReturnResultSet ();
		public abstract ISqlTransaction GetTransaction ();
		public abstract SqlTriggerContextBase GetTriggerContext ();

		#endregion // Methods
	}
}

#endif
