//
// System.Data.SqlClient.SqlClientFactory.cs
//
// Author:
//   Sureshkumar T (tsureshkumar@novell.com)
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0
namespace System.Data.SqlClient
{
        using System.Data;
        using System.Data.Common;
        
        public class SqlClientFactory : DbProviderFactory
        {
                #region Fields
                public static SqlClientFactory Instance = null;
                public static object lockStatic = new object ();
                #endregion //Fields

                #region Constructors
                
                private SqlClientFactory ()
                {
                        
                }

                static SqlClientFactory ()
                {
                        lock (lockStatic) 
                        {
                                if (Instance == null)
                                        Instance = new SqlClientFactory ();
                        }
                }
                #endregion //Constructors
                
                #region public overrides
                public override DbCommand CreateCommand ()
                {
                        return (DbCommand) new SqlCommand ();
                }

                public override DbCommandBuilder CreateCommandBuilder ()
                {
                        return (DbCommandBuilder) new SqlCommandBuilder ();
                }

                public override DbConnection CreateConnection ()
                {
                        SqlConnection connection = new SqlConnection ();
                        return (DbConnection) connection;
                }
                
                public override DbDataAdapter CreateDataAdapter ()
                {
                        return (DbDataAdapter) new SqlDataAdapter ();
                }
                
                public override DbDataSourceEnumerator CreateDataSourceEnumerator ()
                {
                        return (DbDataSourceEnumerator) new SqlDataSourceEnumerator ();
                }
                
                public override DbParameter CreateParameter ()
                {
                        return (DbParameter) new SqlParameter ();
                }
                
                public  new DbProviderSupportedClasses SupportedClasses
                {
                        get
                        {
                                return (DbProviderSupportedClasses) (
                                                                     DbProviderSupportedClasses.DbConnection | 
                                                                     DbProviderSupportedClasses.DbCommand | 
                                                                     DbProviderSupportedClasses.DbParameter | 
                                                                     DbProviderSupportedClasses.DbConnectionStringBuilder | 
                                                                     DbProviderSupportedClasses.DbCommandBuilder | 
                                                                     DbProviderSupportedClasses.DbDataAdapter | 
                                                                     DbProviderSupportedClasses.DbDataSourceEnumerator | 
                                                                     DbProviderSupportedClasses.CodeAccessPermission
                                                                     );
                        }
                }

                #endregion // public overrides
        }
}
#endif // NET_2_0
