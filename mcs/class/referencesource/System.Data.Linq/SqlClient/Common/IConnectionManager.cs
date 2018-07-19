using System;
using System.Data;
using System.Data.Common;

namespace System.Data.Linq.SqlClient {

    internal interface IConnectionManager {
        DbConnection UseConnection(IConnectionUser user);
        void ReleaseConnection(IConnectionUser user);
    }

    internal interface IConnectionUser {
        void CompleteUse();
    }
}
