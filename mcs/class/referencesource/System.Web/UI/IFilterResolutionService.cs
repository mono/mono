//------------------------------------------------------------------------------
// <copyright file="IFilterResolutionService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    public interface IFilterResolutionService {


        /// <devdoc>
        /// Returns true if the specified filter applies to the current filter
        /// </devdoc>
        bool EvaluateFilter(string filterName);


        /// <devdoc>
        /// Returns 1 if filter1 is a parent of filter2.
        /// Returns -1 if filter2 is a parent of filter1.
        /// Returns 0 if there is no parent-child relationship between filter1 and filter2.
        /// </devdoc>
        int CompareFilters(string filter1, string filter2);

    }
}
