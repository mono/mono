#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.Data;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Data.Linq;
using System.Data.Linq.Mapping;
#if MONO_STRICT
using System.Data.Linq.SqlClient;
#else
using DbLinq.Data.Linq.SqlClient;
#endif
using DbLinq.Vendor;
#if MONO_STRICT
using DataContext = System.Data.Linq.DataContext;
#else
using DataContext = DbLinq.Data.Linq.DataContext;
#endif

namespace DbLinq.Oracle
{
    [Vendor(typeof(OracleProvider))]
#if MONO_STRICT
    internal
#else
    public
#endif
 class OracleVendor : Vendor.Implementation.Vendor
    {
        public override string VendorName { get { return "Oracle"; } }

        protected readonly OracleSqlProvider sqlProvider = new OracleSqlProvider();
        public override ISqlProvider SqlProvider { get { return sqlProvider; } }

        public override bool Ping(DataContext dataContext)
        {
            return dataContext.ExecuteCommand("SELECT 11 FROM DUAL") == 11;
        }

        public override IExecuteResult ExecuteMethodCall(DataContext context, MethodInfo method
                                                                 , params object[] inputValues)
        {
            throw new NotImplementedException();
        }

        // This method workds much better on various environment. But why? Thanks Oracle guys for dry documentation.
        public override string BuildConnectionString(string host, string databaseName, string userName, string password)
        {
            var connectionStringBuilder = new StringBuilder();
            connectionStringBuilder.AppendFormat(
                "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = {0})(PORT = 1521)))(CONNECT_DATA = (SERVER = DEDICATED)))",
                host);
            if (!string.IsNullOrEmpty(userName))
            {
                connectionStringBuilder.AppendFormat("; User Id = {0}", userName);
                if (!string.IsNullOrEmpty(password))
                    connectionStringBuilder.AppendFormat("; Password = {0}", password);
            }
            return connectionStringBuilder.ToString();
        }

        protected override string ConnectionStringDatabase { get { return null; } }
        protected override string ConnectionStringServer { get { return "data source"; } }
    }
}
