#region Licence
	/// DB2DriverCS Test Code - A DB2 driver test for .Net
	/// Copyright 2003 By Christopher Bockner
	/// Released under the terms of the MIT/X11 Licence
	/// Please refer to the Licence.txt file that should be distributed with this package
	/// This software requires that DB2 client software be installed correctly on the machine
	/// (or instance) on which the driver is running.  
	/// 
#endregion

using System;
using System.Data;
using DB2ClientCS;

namespace TestDB2Conn
{
	/// <summary>
	/// Code to test DB2DriverCS.
	/// </summary>
	class Class1
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			try 
			{

				DB2ClientConnection DB2Conn = new DB2ClientConnection();
				DB2Conn.Open();
				IDbCommand command = new DB2ClientCommand("select * from employee", DB2Conn);
				command.ExecuteNonQuery();
				IDataReader dr = command.ExecuteReader();
				dr.Read();
				dr.Read();
				string dt = dr.GetString(1);
				string s = dr.GetString(5);
				DateTime t = dr.GetDateTime(6);
				DB2Conn.Close();
			}
			catch(DB2ClientException e) 
			{
				System.Console.Write(e.Message);
			}
		}
	}
}
