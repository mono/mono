using System;

namespace System.Data.Db2Client
{

	public class Db2ClientUtils
	{
		public Db2ClientUtils()
		{

		}
		#region Db2CheckReturn

		public static int Db2CheckReturn(short sqlRet, short handleType, IntPtr handle, string message)
		{
			switch ((long)sqlRet) 
			{
				case Db2Constants.SQL_ERROR:
					if (handleType != 0)
						throw new Db2Exception(handleType, handle, message);
					else
						throw new Db2Exception(message);
				default:
					return 0;
			}
		}
		#endregion
	}
}
