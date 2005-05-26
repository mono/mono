
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections;

namespace IBM.Data.DB2
{

	internal sealed class DB2Environment : IDisposable
	{
		volatile static DB2Environment environment;
		static readonly object lockobj = new object ();
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
					lock(lockobj)
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