//
// System.Data.OleDb.OleDbConnectionFactory
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//

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
using System.Data;
using System.Data.Common;
using System.Data.ProviderBase;
using System.Reflection;
using System.Collections;

namespace System.Data.OleDb
{
	internal class OleDbConnectionFactory
	{
		private static Object LockObj = new Object();
		private static Hashtable dataSourceCache = new Hashtable();
		private static Type OracleConnectionCacheImplType = Type.GetType("oracle.jdbc.pool.OracleConnectionCacheImpl");
																		  
		private static MethodInfo setUrlMethod = OracleConnectionCacheImplType.GetMethod("setURL", BindingFlags.Public | BindingFlags.Instance);
		private static MethodInfo setUserMethod = OracleConnectionCacheImplType.GetMethod("setUser", BindingFlags.Public | BindingFlags.Instance);
		private static MethodInfo setPasswordMethod = OracleConnectionCacheImplType.GetMethod("setPassword", BindingFlags.Public | BindingFlags.Instance);
		private static MethodInfo getActiveSizeMethod = OracleConnectionCacheImplType.GetMethod("getActiveSize", BindingFlags.Public | BindingFlags.Instance);
		private static MethodInfo closeMethod = OracleConnectionCacheImplType.GetMethod("close", BindingFlags.Public | BindingFlags.Instance);
		private static MethodInfo setMaxLimitMethod = OracleConnectionCacheImplType.GetMethod("setMaxLimit", BindingFlags.Public | BindingFlags.Instance);
		private static MethodInfo setMinLimitMethod = OracleConnectionCacheImplType.GetMethod("setMinLimit", BindingFlags.Public | BindingFlags.Instance);
		private static long timestamp = DateTime.Now.Ticks;
		private static int MINUTES_TIMEOUT = 60;

		private static DbStringManager StringManager = new DbStringManager("System.Data.System.Data.ProviderBase.jvm.OleDbStrings");

		internal static java.sql.Connection GetConnection(OleDbConnection.PROVIDER_TYPE providerType,String url,String user,String password,int timeout)
		{
			CacheKey key = new CacheKey(url,user,password);
			Object dataSourceObj;
			lock(LockObj) 
			{
				if (TimeIsOver())
					Clear();

				dataSourceObj = dataSourceCache[key];
				if (dataSourceObj == null) 
				{
					switch(providerType) 
					{
						case OleDbConnection.PROVIDER_TYPE.MSDAORA :					
					
							dataSourceObj = Activator.CreateInstance(OracleConnectionCacheImplType);
					
							Object[] setUrlArgs = new Object[1] { url };
							Object[] setUserArgs = new Object[1] { user };
							Object[] setPasswordArgs = new Object[1] { password };

							setUrlMethod.Invoke(dataSourceObj,setUrlArgs);
							setUserMethod.Invoke(dataSourceObj,setUserArgs);
							setPasswordMethod.Invoke(dataSourceObj,setPasswordArgs);

							int maxLimit = Convert.ToInt32(StringManager.GetString("ORA_CONNECTION_POOLING_MAX_LIMIT","-1"));
							int minLimit = Convert.ToInt32(StringManager.GetString("ORA_CONNECTION_POOLING_MIN_LIMIT","-1"));

							if(minLimit != -1)
								setMinLimitMethod.Invoke(dataSourceObj,new Object[1] { minLimit } );

							if(maxLimit != -1)
								setMaxLimitMethod.Invoke(dataSourceObj,new Object[1] { maxLimit } );

							break;
					}
					dataSourceCache.Add(key,dataSourceObj);
				}
			}

			java.sql.Connection conn;
			lock(dataSourceObj) {
				if (((javax.sql.DataSource)dataSourceObj).getLoginTimeout() != timeout) {
					((javax.sql.DataSource)dataSourceObj).setLoginTimeout(timeout);
				}
				conn = ((javax.sql.DataSource)dataSourceObj).getConnection();
			}
			return conn;
		}

		private static bool TimeIsOver() 
		{
				return (DateTime.Now.Ticks - timestamp > MINUTES_TIMEOUT * TimeSpan.TicksPerMinute);
		}

		private static void Clear() 
		{
			ArrayList closedDataSources = new ArrayList();

			// collect all the datasources with no connections in use
			foreach(DictionaryEntry e in dataSourceCache)
			{
				Object dataSourceObj = e.Value;
				Object dataSourceObjKey = e.Key;

				if (getActiveSizeMethod.Invoke(dataSourceObj,new object[0]).Equals(0)) {
					closeMethod.Invoke(dataSourceObj,new object[0]);
					closedDataSources.Add(dataSourceObjKey);
				}
			}

			// close and remove all data sources with no connections in use
			foreach(Object dataSourceObjKey in closedDataSources) {
				dataSourceCache.Remove(dataSourceObjKey);
			}	
		}
	}

	// Key for data sources cache
	// data sources mapped by jdbc url, user and password
	internal class CacheKey
    {
        private String url;
        private String user;
        private String password;

        public CacheKey(String url, String user, String password)
        {
            this.url = url;
            this.user = user;
            this.password = password;
        }

        public bool equals(Object o)
        {
            if (this == o) return true;
            if (!(o is CacheKey)) return false;

            CacheKey cacheKey = (CacheKey) o;

            if (!password.Equals(cacheKey.password)) return false;
            if (!url.Equals(cacheKey.url)) return false;
            if (!user.Equals(cacheKey.user)) return false;

            return true;
        }

        public int hashCode()
        {
            int result;
            result = url.GetHashCode();
            result = 29 * result + user.GetHashCode();
            result = 29 * result + password.GetHashCode();
            return result;
        }
    }
}
