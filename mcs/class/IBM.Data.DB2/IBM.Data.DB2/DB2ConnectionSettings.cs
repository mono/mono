
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
using System.Runtime.InteropServices;
using System.Text;

namespace IBM.Data.DB2
{
	internal sealed class DB2ConnectionSettings
	{
		private string	 connectionString;
		private string	 userName = "";
		private string	 passWord = "";
		private string	 databaseAlias = "";
		private string	 server = "";
		private bool	 pooling = true;
		private TimeSpan connectTimeout = new TimeSpan(0, 0, 15);
		private TimeSpan connectionLifeTime = new TimeSpan(0, 0, 15);	// 15 seconds
		private int		 connectionPoolSizeMin = 0;
		private int		 connectionPoolSizeMax = -1;	// no maximum

		private DB2ConnectionPool pool;

		private DB2ConnectionSettings(string connectionString)
		{
			this.connectionString = connectionString;
			this.Parse();
		}

		public static DB2ConnectionSettings GetConnectionSettings(string connectionString)
		{
			DB2ConnectionPool pool = DB2ConnectionPool.FindConnectionPool(connectionString);
			if(pool != null)
			{
				return pool.ConnectionSettings;
			}
			DB2ConnectionSettings settings = new DB2ConnectionSettings(connectionString);
			if(settings.Pooling)
			{
				settings.pool = DB2ConnectionPool.GetConnectionPool(settings);
			}
			return settings;
		}

		public DB2OpenConnection GetRealOpenConnection(DB2Connection connection)
		{
			if(pool != null)
			{
				return pool.GetOpenConnection(connection);
			}
			else
			{
				return new DB2OpenConnection(this, connection);
			}
		}

		public DB2ConnectionPool Pool
		{
			get { return pool; }
		}

		public string ConnectionString
		{
			get { return connectionString; }
		}

		public string UserName
		{
			get { return userName; }
		}
		
		public string PassWord
		{
			get { return passWord; }
		}

		/// <summary>
		/// database alias (for cataloged database)
		/// </summary>
		public string DatabaseAlias
		{
			get { return databaseAlias; }
		}

		/// <summary>
		/// server name with optional port number for direct connection (<server name/ip address>[:<port>])
		/// </summary>
		public string Server
		{
			get { return server; }
		}

		public TimeSpan ConnectTimeout
		{
			get { return connectTimeout; }
		}

		/// <summary>
		/// Connection pooling yes/no
		/// </summary>
		public bool Pooling
		{
			get { return pooling; }
		}

		public int ConnectionPoolSizeMin
		{
			get { return connectionPoolSizeMin; }
		}

		public int ConnectionPoolSizeMax
		{
			get { return connectionPoolSizeMin; }
		}

		public TimeSpan ConnectionLifeTime
		{
			get { return connectionLifeTime; }
		}


		/// <summary>
		/// parsed according to IBM DB2 .NET data provider help
		/// </summary>
		public void Parse()
		{
			string[] parts = connectionString.Split(new char[]{';'});
			foreach(string part in parts)
			{
				string[] pairs = part.Split(new char[]{'='});
				switch(pairs[0].ToLower())
				{
					case "database":
					case "dsn":
						databaseAlias = pairs[1];
						break;
					case "uid":
					case "user id":
						userName = pairs[1];
						break;
					case "pwd":
					case "password":
						passWord = pairs[1];
						break;
					case "server":
						server = pairs[1];
						break;
					case "pooling":
						pooling = (pairs[1].ToLower() == "true") || (pairs[1]== "1");
						break;
					case "connect timeout":
					case "timeout":
					case "connection timeout":
						connectTimeout = new TimeSpan(0, 0, int.Parse(pairs[1]));
						break;
					case "min pool size":
						connectionPoolSizeMin = int.Parse(pairs[1]);
						break;
					case "max pool size":
						connectionPoolSizeMax = int.Parse(pairs[1]);
						break;
					case "connection lifetime":
						connectionLifeTime = new TimeSpan(0, 0, int.Parse(pairs[1]));
						break;
				}
			}
			if(connectionLifeTime.Ticks <= 0)
			{
				pooling = false;
			}
		}
	
		public override int GetHashCode()
		{
			return connectionString.GetHashCode ();
		}
	
		public override bool Equals(object obj)
		{
			return connectionString.Equals (obj);
		}
	}
}
