using System;

namespace IBM.Data.DB2
{

	public class DB2ClientUtils
	{

		#region DB2CheckReturn

		public static void DB2CheckReturn(short sqlRet, short handleType, IntPtr handle, string message, DB2Connection connection)
		{
			switch (sqlRet) 
			{
				case DB2Constants.SQL_SUCCESS_WITH_INFO:
					if(connection != null)
					{
						connection.OnInfoMessage(handleType, handle);
					}
					goto case DB2Constants.SQL_SUCCESS;
				case DB2Constants.SQL_SUCCESS:
				case DB2Constants.SQL_NO_DATA:
					return;

				case DB2Constants.SQL_INVALID_HANDLE:
					throw new ArgumentException("Invalid handle");

				default:
				case DB2Constants.SQL_ERROR:
					throw new DB2Exception(handleType, handle, message);
			}
		}
		public static void DB2CheckReturn(short sqlRet, short handleType, IntPtr handle, string message)
		{
			DB2CheckReturn(sqlRet, handleType, handle, message, null);
		}
		#endregion
	}
}
