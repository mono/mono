using System;
using System.Collections;

namespace IBM.Data.DB2
{

	internal sealed class DB2Environment : IDisposable
	{
		private static DB2Environment environment;
		internal Hashtable connectionPools;
		internal IntPtr penvHandle = IntPtr.Zero;

		private DB2Environment()
		{
			connectionPools = Hashtable.Synchronized(new Hashtable());

			//			short sqlRet = DB2CLIWrapper.SQLAllocHandle(DB2Constants.SQL_HANDLE_ENV, IntPtr.Zero, ref penvHandle);
			short sqlRet = DB2CLIWrapper.Initialize(ref penvHandle);
			DB2ClientUtils.DB2CheckReturn(sqlRet, 0, IntPtr.Zero, "Unable to allocate Environment handle.", null);

			// SQLSetEnvAttr( hEnv=0:1, fAttribute=SQL_ATTR_APP_TYPE 2473, vParam=4, cbParam=0 )	// 4=ADO.NET apptype????
			// SQLSetEnvAttr( hEnv=0:1, fAttribute=SQL_ATTR_OUTPUT_NTS 10001, vParam=0, cbParam=0 ) // strings not 0-terminated
		}

		public static DB2Environment Instance
		{
			get
			{
				if(environment == null)
				{
					lock(typeof(DB2Environment))
					{
						if(environment == null)
						{
							environment = new DB2Environment();
						}
					}
				}
				return environment;
			}
		}
		#region IDisposable Members

		bool disposed;
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Dispose(bool disposing)
		{
			if(disposed)
			{
				DB2CLIWrapper.SQLFreeHandle(DB2Constants.SQL_HANDLE_ENV, penvHandle);
				environment = null;
			}
			disposed = true;
		}

		~DB2Environment()
		{
			Dispose(false);
		}

		#endregion
	}

}