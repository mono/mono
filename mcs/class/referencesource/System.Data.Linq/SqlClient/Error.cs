using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace System.Data.Linq.SqlClient {
    /// <summary>
    /// DLinq-providerbase-specific custom exception factory.
    /// </summary>
    internal partial class Error {
        /// <summary>
        /// Exception thrown when a query cannot execute against a particular SQL server version.
        /// </summary>
        static internal Exception ExpressionNotSupportedForSqlServerVersion(Collection<string> reasons) {
            StringBuilder exceptionMessage = new StringBuilder(Strings.CannotTranslateExpressionToSql);
            foreach (string reason in reasons) {
                exceptionMessage.AppendLine(reason);                    
            }
            return new NotSupportedException(exceptionMessage.ToString());
        }
    }

}
