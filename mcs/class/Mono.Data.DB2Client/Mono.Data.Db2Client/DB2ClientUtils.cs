#region Licence
	/// DB2DriverCS - A DB2 driver for .Net
	/// Copyright 2003 By Christopher Bockner
	/// Released under the terms of the MIT/X11 Licence
	/// Please refer to the Licence.txt file that should be distributed with this package
	/// This software requires that DB2 client software be installed correctly on the machine
	/// (or instance) on which the driver is running.  
#endregion
using System;

namespace DB2ClientCS
{
	/// <summary>
	/// Utility functions class, i.e. functions like SQL return code error checking
	/// </summary>
	public class DB2ClientUtils
	{
		public DB2ClientUtils()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		#region DB2CheckReturn
		/// <summary>
		/// Check the return value from a Call Level Interface call and respond to errors (by tossing exception)
		/// </summary>
		/// <param name="sqlRet"><The return value>
		/// <param name="handleType"><Type of handle, 0 means we don't care, so don't try to check the handle for any messages placed there by DB2>
		/// <param name="handle"><The handle in question, don't care if handletype is 0>
		/// <param name="message"><Message we may throw>
		/// <returns></returns>
		public int DB2CheckReturn(short sqlRet, short handleType, IntPtr handle, string message)
		{
			switch ((long)sqlRet) 
			{
				case DB2ClientConstants.SQL_ERROR:
					if (handleType != 0)
						throw new DB2ClientException(handleType, handle, message);
					else
						throw new DB2ClientException(message);
				default:
					return 0;
			}
		}
		#endregion
	}
}
