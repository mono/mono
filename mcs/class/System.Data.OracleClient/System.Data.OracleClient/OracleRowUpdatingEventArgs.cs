//
// OracleRowUpdatingEventArgs.cs
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Author: Tim Coleman <tim@timcoleman.com>
//
// Parts derived from System.Data.SqlClient.SqlRowUpdatingEventArgs
// Authors:
//      Rodrigo Moya (rodrigo@ximian.com)
//      Daniel Morgan (danmorg@sc.rr.com)
//      Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002-2003
//
// Licensed under the MIT/X11 License.

using System;
using System.Data;
using System.Data.Common;

namespace System.Data.OracleClient
{
	public sealed class OracleRowUpdatingEventArgs : RowUpdatingEventArgs
	{
		#region Constructors

		public OracleRowUpdatingEventArgs (DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (row, command, statementType, tableMapping)
		{
		}

		#endregion // Constructors

		#region Properties

#if NET_2_0
		protected override IDbCommand BaseCommand {
			get {
				return base.BaseCommand;
			}
			set {
				base.BaseCommand = value;
			}
		}
#endif

		public new OracleCommand Command {
			get { return (OracleCommand) base.Command; }
			set { base.Command = value; }
		}

		#endregion // Properties
	}
}
