//
// System.Data.Odbc.OdbcFactory
//
// Author:
//   Sureshkumar T <tsureshkumar@novell.com>
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#region Using Directives
using System;
using System.Data;
using System.Data.Common;
using System.Data.ProviderBase;
#endregion // Using Directives

namespace System.Data.Odbc 
{
        
        public sealed class OdbcFactory : DbProviderFactory
        {
                #region Fields
                public static readonly OdbcFactory Instance;
                #endregion //Fields

                #region Constructors
                /// <remarks>
                /// public static variable Instance should hold the the singleton instance 
                /// based on the knowledge that custom factories without this instance variable
                /// ms.net  throws exception 
                /// <pre>
                /// System.InvalidOperationException: The requested .Net Framework Data 
                ///             Provider's implementation does not have an Instance field 
                ///             of a System.Data.Common.DbProviderFactory derived type.
                ///     at System.Data.Common.DbProviderFactories.GetFactory(DataRow providerRow)
                ///     at System.Data.Common.DbProviderFactories.GetFactory(String providerInvariantName)
                /// </pre>
                /// </remarks>
                static OdbcFactory() 
                {
                        lock (typeof (OdbcFactory)) 
                                {
                                        if (Instance == null)
                                                Instance = new OdbcFactory ();
                                }
                        
                }
                
                private OdbcFactory()
                {
                        
                }
                
                #endregion //Constructors

                #region Properties
                public override DbProviderSupportedClasses SupportedClasses { 
                        get {                                
                                return (DbProviderSupportedClasses) (
                                                                     DbProviderSupportedClasses.DbConnection | 
                                                                     DbProviderSupportedClasses.DbCommand | 
                                                                     DbProviderSupportedClasses.DbParameter | 
                                                                     //DbProviderSupportedClasses.DbConnectionStringBuilder | 
                                                                     DbProviderSupportedClasses.DbCommandBuilder | 
                                                                     DbProviderSupportedClasses.DbDataAdapter //| 
                                                                     //DbProviderSupportedClasses.DbDataSourceEnumerator | 
                                                                     //DbProviderSupportedClasses.CodeAccessPermission
                                                                     );
                        }
                }
                
                #endregion //Properties

                #region Methods
                public override DbConnection CreateConnection()
                {
                        OdbcConnectionFactory connFactory = OdbcConnectionFactory.GetSingleton (this);
                        return  new OdbcConnection (connFactory);
                }
                
                public override DbCommand CreateCommand()
                {
                        return new OdbcCommand ()  as DbCommand;
                }
                
                public override DbCommandBuilder CreateCommandBuilder()
                {
                        return new OdbcCommandBuilder () as DbCommandBuilder;
                }
                
                
//                 public override DbConnectionStringBuilder CreateConnectionStringBuilder()
//                 {
//                         throw new NotImplementedException ();
//                 }
                
                public override DbDataAdapter CreateDataAdapter()
                {
                        return new OdbcDataAdapter () as DbDataAdapter;
                }
                
                public override DbParameter CreateParameter()
                {
                        return new OdbcParameter () as DbParameter;
                }
                
//                 public override CodeAccessPermission CreatePermission(PermissionState state)
//                 {
//                         throw new NotImplementedException ();
//                 }
                
                #endregion //Methods

        }
 

}
#endif // NET_2_0
