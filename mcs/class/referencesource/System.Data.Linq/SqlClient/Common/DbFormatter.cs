using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Linq.SqlClient {
    internal abstract class DbFormatter {
        internal abstract string Format(SqlNode node, bool isDebug);
        internal abstract string Format(SqlNode node);
    }
}
