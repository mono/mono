using System;
using System.Runtime.InteropServices;
using System.Text;

namespace IBM.Data.DB2
{

	public class DB2Exception : Exception
	{
		internal string message;
		public DB2Exception(string Message)
		{	
			this.message = Message;
		}
		public DB2Exception(short sqlHandleType, IntPtr sqlHandle, string Message)
		{	
			StringBuilder sqlState = new StringBuilder(50);
			StringBuilder errorMessage = new StringBuilder(1025);

			int sqlReturn;
			short recNum=1;
			short bufLength = 1025;

			IntPtr textLengthPtr = IntPtr.Zero;
			IntPtr nativeErrorPtr = IntPtr.Zero;

			sqlReturn = DB2CLIWrapper.SQLGetDiagRec(sqlHandleType, sqlHandle, recNum, sqlState, ref nativeErrorPtr, errorMessage, bufLength, ref textLengthPtr);
			this.message = Message + "\n" + sqlState.ToString() + " " + errorMessage.ToString()+"\n";
			//See if there are more errors to retrieve and get them.
			while (sqlReturn != DB2Constants.SQL_NO_DATA  && sqlReturn > 0)
			{
				recNum++;
				sqlReturn = DB2CLIWrapper.SQLGetDiagRec(sqlHandleType, sqlHandle, recNum, sqlState, ref nativeErrorPtr, errorMessage, bufLength, ref textLengthPtr);
				this.message += "\n" + sqlState.ToString() + " " + errorMessage.ToString()+"\n";
			}

		}
		public override string Message
		{
			get
			{
				return this.message;
			}
		}
	}
}
