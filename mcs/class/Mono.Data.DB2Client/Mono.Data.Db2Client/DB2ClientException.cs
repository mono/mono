#region Licence
	/// DB2DriverCS - A DB2 driver for .Net
	/// Copyright 2003 By Christopher Bockner
	/// Released under the terms of the MIT/X11 Licence
	/// Please refer to the Licence.txt file that should be distributed with this package
	/// This software requires that DB2 client software be installed correctly on the machine
	/// (or instance) on which the driver is running.  
#endregion
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DB2ClientCS
{
	/// <summary>
	/// Exception class.  We will throw our own exception in the case of SQL_ERROR returns
	/// and call SQLGetDiagRec to get the error info.
	/// </summary>
	public class DB2ClientException : Exception
	{
		internal string message;
		public DB2ClientException(string Message)
		{	
			this.message = Message;
		}
		unsafe public DB2ClientException(short sqlHandleType, IntPtr sqlHandle, string Message)
		{	
			StringBuilder sqlState = new StringBuilder(50);
			StringBuilder errorMessage = new StringBuilder(1025);

			int sqlReturn;
			short recNum=1;
			short bufLength = 1025;

			IntPtr textLengthPtr = IntPtr.Zero;
			IntPtr nativeErrorPtr = IntPtr.Zero;

			sqlReturn = DB2ClientPrototypes.SQLGetDiagRec(sqlHandleType, sqlHandle, recNum, sqlState, ref nativeErrorPtr, errorMessage, bufLength, ref textLengthPtr);
			this.message = Message + "\n" + sqlState.ToString() + " " + errorMessage.ToString()+"\n";
			//See if there are more errors to retrieve and get them.
			while (sqlReturn != DB2ClientConstants.SQL_NO_DATA  && sqlReturn > 0)
			{
				recNum++;
				sqlReturn = DB2ClientPrototypes.SQLGetDiagRec(sqlHandleType, sqlHandle, recNum, sqlState, ref nativeErrorPtr, errorMessage, bufLength, ref textLengthPtr);
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
