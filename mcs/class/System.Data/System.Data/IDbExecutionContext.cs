//
// System.Data.IDbExecutionContext.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data {
	public interface IDbExecutionContext
	{
		#region Properties

		IDbConnection Connection { get; set; }
		int ConnectionTimeOut { get; set; }
		IDbTransaction Transaction { get; set; }

		#endregion // Properties

		#region Methods

		IAsyncResult BeginExecuteNonQuery (AsyncCallback callback, object stateObject);
		IAsyncResult BeginExecuteReader (AsyncCallback callback, object stateObject, CommandBehavior behavior);
		IAsyncResult BeginExecuteScalar (AsyncCallback callback, object stateObject);

		void Cancel ();

		int EndExecuteNonQuery (IAsyncResult result);
		IDataReader EndExecuteReader (IAsyncResult result);
		object EndExecuteScalar (IAsyncResult result);

		int ExecuteNonQuery ();
		IDataReader ExecuteReader ();
		IDataReader ExecuteReader (CommandBehavior behavior);
		object ExecuteScalar ();

		#endregion // Methods
	}
}

#endif // NET_1_2
