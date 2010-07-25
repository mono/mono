
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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace IBM.Data.DB2
{

	internal sealed class DB2OpenConnection : IDisposable
	{
		private IntPtr dbHandle = IntPtr.Zero;

		private DB2ConnectionSettings settings;
		private bool disposed = false;
		internal DateTime poolDisposalTime; // time to live used for disposal of connections in the connection pool
		public bool transactionOpen;
		public bool autoCommit = true;
		private string		databaseProductName;
		private string		databaseVersion;
		private int			majorVersion;
		private int			minorVersion;

		public IntPtr DBHandle
		{
			get { return dbHandle; }
		}
		public string DatabaseProductName
		{
			get { return databaseProductName; }
		}
		public string DatabaseVersion
		{
			get { return databaseVersion; }
		}
		public int MajorVersion
		{
			get { return majorVersion; }
		}
		public int MinorVersion
		{
			get { return minorVersion; }
		}

		public DB2OpenConnection(DB2ConnectionSettings settings, DB2Connection connection)
		{
			this.settings = settings;
			try
			{
				short sqlRet = DB2CLIWrapper.SQLAllocHandle(DB2Constants.SQL_HANDLE_DBC, DB2Environment.Instance.penvHandle, out dbHandle);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_DBC, DB2Environment.Instance.penvHandle, "Unable to allocate database handle in DB2Connection.", connection);

				if(settings.Server.Length > 0)
				{
					StringBuilder outConnectStr = new StringBuilder(60);  // TODO: ????
					short numOutCharsReturned;

					sqlRet = DB2CLIWrapper.SQLDriverConnect(dbHandle, IntPtr.Zero, 
						settings.ConnectionString,	(short)settings.ConnectionString.Length,
						outConnectStr,				(short)outConnectStr.Length, out numOutCharsReturned, 
						DB2Constants.SQL_DRIVER_NOPROMPT /*SQL_DRIVER_COMPLETE*/);
				}
				else
				{
					sqlRet = DB2CLIWrapper.SQLConnect(dbHandle, 
						settings.DatabaseAlias,	(short)settings.DatabaseAlias.Length, 
						settings.UserName,		(short)settings.UserName.Length,
						settings.PassWord,		(short)settings.PassWord.Length);
					DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_DBC, dbHandle, "Unable to connect to the database.", connection);
				}

				if((settings.Pool == null) || (settings.Pool.databaseProductName == null))
				{
					StringBuilder sb = new StringBuilder(256);
					short stringLength;
					sqlRet = DB2CLIWrapper.SQLGetInfo(dbHandle, /*SQL_DBMS_NAME*/17, sb, (short)(sb.Capacity / 2), out stringLength);
					new DB2ErrorCollection(DB2Constants.SQL_HANDLE_DBC, dbHandle).ToString();
					if(sqlRet == 0)
						databaseProductName = sb.ToString(0, Math.Min(sb.Capacity, stringLength / 2));
					sqlRet = DB2CLIWrapper.SQLGetInfo(dbHandle, /*SQL_DBMS_VER*/18, sb, (short)(sb.Capacity / 2), out stringLength);
					if(sqlRet == 0)
					{
						databaseVersion = sb.ToString(0, Math.Min(sb.Capacity, stringLength / 2));
						try
						{
							string[] splitVersion = databaseVersion.Split('.');
							majorVersion = int.Parse(splitVersion[0]);
							minorVersion = int.Parse(splitVersion[1]);
						}
						catch{}
					}
					if(settings.Pool != null)
					{
						settings.Pool.databaseProductName = databaseProductName;
						settings.Pool.databaseVersion	  = databaseVersion;
						settings.Pool.majorVersion		  = majorVersion;
						settings.Pool.minorVersion		  = minorVersion;
					}
				}
				else if(settings.Pool != null)
				{
					if(settings.Pool != null)
					{
						databaseProductName = settings.Pool.databaseProductName;
						databaseVersion		= settings.Pool.databaseVersion;
						majorVersion		= settings.Pool.majorVersion;
						minorVersion		= settings.Pool.minorVersion;
					}
				}
			}
			catch
			{
				if(dbHandle != IntPtr.Zero)
				{
					DB2CLIWrapper.SQLFreeHandle(DB2Constants.SQL_HANDLE_DBC, dbHandle);
					dbHandle = IntPtr.Zero;
				}
				throw;
			}
		}

		public void RollbackDeadTransaction()
		{
			DB2CLIWrapper.SQLEndTran(DB2Constants.SQL_HANDLE_DBC, DBHandle, DB2Constants.SQL_ROLLBACK);
			transactionOpen = false;
		}

		public void Close()
		{
			if(transactionOpen)
				RollbackDeadTransaction();

			if(settings.Pool != null)
			{
				settings.Pool.AddToFreeConnections(this);
			}
			else
			{
				Dispose();
			}
		}

		private void FreeHandles()
		{
			if(dbHandle != IntPtr.Zero)
			{
				short sqlRet = DB2CLIWrapper.SQLDisconnect(dbHandle);
				// Note that SQLDisconnect() automatically drops any statements and
				// descriptors open on the connection.
				sqlRet = DB2CLIWrapper.SQLFreeHandle(DB2Constants.SQL_HANDLE_DBC, dbHandle);

				dbHandle = IntPtr.Zero;
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if(!disposed) 
			{
				if(disposing)
				{
					// dispose managed resources
				}
				FreeHandles();
			}
			disposed = true;
		}


		~DB2OpenConnection()
		{
			if(settings.Pool != null)
			{
				settings.Pool.OpenConnectionFinalized();
			}
			Dispose(false);
		}
		#endregion
	}


}