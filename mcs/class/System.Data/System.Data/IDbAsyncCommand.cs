//
// System.Data.IDbAsyncCommand.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data {
	public interface IDbAsyncCommand
	{
		#region Methods

		IAsyncResult BeginExecuteNonQuery (AsyncCallback callback, object stateObject);
		IAsyncResult BeginExecuteReader (AsyncCallback callback, object stateObject, CommandBehavior behavior);
		int EndExecuteNonQuery (IAsyncResult result);
		IDataReader EndExecuteReader (IAsyncResult result);

		#endregion // Methods
	}
}

#endif // NET_1_2
