//------------------------------------------------------------------------------
// <copyright file="CodeChartHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security.AntiXss.CodeCharts {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    internal static class CodeChartHelper {
        internal static IEnumerable<int> GetRange(int min, int max, Func<int, bool> exclusionFilter) {
            Debug.Assert(min <= max);

            var range = Enumerable.Range(min, (max - min + 1));
            if (exclusionFilter != null) {
                range = range.Where(i => !exclusionFilter(i));
            }
            return range;
        }

        internal static IEnumerable<int> GetRange(int min, int max) {
            return GetRange(min, max, null);
        }
    }
}
