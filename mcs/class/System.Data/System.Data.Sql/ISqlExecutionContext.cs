//
// System.Data.Sql.ISqlExecutionContext
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Xml;

namespace System.Data.Sql {
	public interface ISqlExecutionContext : IDbExecutionContext, IDisposable, ISqlGetTypedData, IGetTypedData, ISqlSetTypedData
	{
		#region Properties

		ISqlConnection Connection { get; }
		SqlNotificationRequest Notification { get; set; }
		ISqlTransaction Transaction { get; set; }

		#endregion // Properties

		#region Methods

		ISqlReader ExecutePageReader (CommandBehavior behavior, int startRow, int pageSize);
		ISqlReader ExecuteReader ();
		ISqlReader ExecuteReader (CommandBehavior behavior);
		ISqlResultSet ExecuteResultSet (ResultSetOptions options);
		ISqlResultSet ExecuteResultSet (ResultSetOptions options, string cursorname);
		ISqlRecord ExecuteRow ();
		object ExecuteSqlScalar ();
		XmlReader ExecuteXmlReader ();

		#endregion // Methods
	}
}

#endif
