using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;

namespace IBM.Data.DB2
{
	class DB2Environment{
		static IntPtr penvHandle = IntPtr.Zero;
		private static void AllocateEnvironmentHwnd(){
			short sqlRet;
			sqlRet = DB2CLIWrapper.SQLAllocHandle(DB2Constants.SQL_HANDLE_ENV, IntPtr.Zero, ref penvHandle);
			DB2ClientUtils.DB2CheckReturn(sqlRet, 0, IntPtr.Zero, "Unable to allocate Environment handle in DB2Connection.");
		}
		public static IntPtr GetEnvironmentHwnd(){
			if (penvHandle == IntPtr.Zero) AllocateEnvironmentHwnd();
			return penvHandle;
		}	
	}

	internal class DB2InternalConnection{
		bool isOpen = false;
		IntPtr pdbcHandle = IntPtr.Zero;
		private void AllocateConnectionHwnd(){
			short sqlRet;
			sqlRet = DB2CLIWrapper.SQLAllocHandle(DB2Constants.SQL_HANDLE_DBC, DB2Environment.GetEnvironmentHwnd(), ref pdbcHandle);
			DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_ENV, DB2Environment.GetEnvironmentHwnd(), "Unable to allocate database handle in DB2Connection.");
		}
		public IntPtr GetConnectionHwnd(){
			if(pdbcHandle == IntPtr.Zero) AllocateConnectionHwnd();
			return pdbcHandle;
		}
		
		public void Open(string connectionString){
			if (isOpen) return;
			string serverName, userName, authentication, dsn;
			StringBuilder outConnectStr = new StringBuilder(60);
			IntPtr numOutCharsReturned = IntPtr.Zero;
			short sqlRet = 0;
			  
			serverName = userName = authentication = dsn = String.Empty;
			try
			{
				string[] parts = connectionString.Split(new char[]{';'});
				foreach(string part in parts)
				{
					string[] pairs = part.Split(new char[]{'='});
					switch(pairs[0].ToLower())
					{
						case "server":
						case "database":
							serverName = pairs[1];
							break;
						case "uid":
							userName = pairs[1];
							break;
						case "pwd":
							authentication = pairs[1];
							break;
						case "dsn":
							dsn = pairs[1];
							break;
						default:
							break;
					}
				}
			}
			catch(Exception)
			{
				throw new DB2Exception("Bad connection string");
			}
			
			if(dsn != null && dsn != String.Empty)
			{
				sqlRet = DB2CLIWrapper.SQLDriverConnect(pdbcHandle, 0, connectionString,
					connectionString.Length, outConnectStr, 100, numOutCharsReturned, 
					DB2Constants.SQL_DRIVER_COMPLETE);
			}
			else
			{
				sqlRet = DB2CLIWrapper.SQLConnect(pdbcHandle,serverName, (short)serverName.Length, userName, (short)userName.Length, authentication, (short)authentication.Length);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_ENV, DB2Environment.GetEnvironmentHwnd(), "Unable to connect to the database.");
			}
			
			
			isOpen = true;
		}
	}
}