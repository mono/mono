#region Licence
	/// DB2DriverCS - A DB2 driver for .Net
	/// Copyright 2003 By Christopher Bockner
	/// Released under the terms of the MIT/X11 Licence
	/// Please refer to the Licence.txt file that should be distributed with this package
	/// This software requires that DB2 client software be installed correctly on the machine
	/// (or instance) on which the driver is running.  
#endregion
using System;
using System.Data;

namespace DB2DriverCS
{
	/// <summary>
	/// Summary description for DB2ClientLOB.
	/// Base class for LOB support.  Other LOB classes inherit from this.  My support at first will be limited to 
	/// handling LOB locators, but we'll see where we go...
	/// </summary>
	public class DB2ClientLOB
	{
		internal IntPtr hwndStmt;

		public DB2ClientLOB()
		{
			//
			// TODO: Add constructor logic here
			//
		}
	}
}
