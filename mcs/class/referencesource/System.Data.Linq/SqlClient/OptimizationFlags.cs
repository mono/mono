using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Linq.SqlClient {
    /// <summary>
    /// Flags that control the optimization of SQL produced.
    /// Only optimization flags should go here because QA will be looking
    /// here to see what optimizations they need to test.
    /// </summary>
    [Flags]
    internal enum OptimizationFlags {
        None = 0,
        SimplifyCaseStatements = 1,
        OptimizeLinkExpansions = 2,
        All = SimplifyCaseStatements | OptimizeLinkExpansions
    }
}
