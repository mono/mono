using System;

namespace IBM.Data.DB2
{

	public class DB2ClientUtils
	{
		public DB2ClientUtils()
		{

		}
		#region DB2CheckReturn

		public static int DB2CheckReturn(short sqlRet, short handleType, IntPtr handle, string message)
		{
			switch ((long)sqlRet) 
			{
				case DB2Constants.SQL_ERROR:
					if (handleType != 0)
						throw new DB2Exception(handleType, handle, message);
					else
						throw new DB2Exception(message);
				default:
					return 0;
			}
		}
		#endregion
	}
}
