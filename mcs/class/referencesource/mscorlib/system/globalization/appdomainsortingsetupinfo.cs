// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;

namespace System.Globalization {
    internal sealed class AppDomainSortingSetupInfo {

        internal IntPtr _pfnIsNLSDefinedString;
        internal IntPtr _pfnCompareStringEx;
        internal IntPtr _pfnLCMapStringEx;
        internal IntPtr _pfnFindNLSStringEx;
        internal IntPtr _pfnCompareStringOrdinal;
        internal IntPtr _pfnGetNLSVersionEx;
        // _pfnFindStringOrdinal is used as a fast path for
        // String.IndexOf and String.LastIndexOf OrdinalIngoreCase
        internal IntPtr _pfnFindStringOrdinal;
        internal bool _useV2LegacySorting;
        internal bool _useV4LegacySorting;

        internal AppDomainSortingSetupInfo() {

        }

        internal AppDomainSortingSetupInfo(AppDomainSortingSetupInfo copy) {
            _useV2LegacySorting = copy._useV2LegacySorting;
            _useV4LegacySorting = copy._useV4LegacySorting;
            _pfnIsNLSDefinedString = copy._pfnIsNLSDefinedString;
            _pfnCompareStringEx = copy._pfnCompareStringEx; 
            _pfnLCMapStringEx = copy._pfnLCMapStringEx;
            _pfnFindNLSStringEx = copy._pfnFindNLSStringEx;
            _pfnFindStringOrdinal = copy._pfnFindStringOrdinal;
            _pfnCompareStringOrdinal = copy._pfnCompareStringOrdinal;
            _pfnGetNLSVersionEx = copy._pfnGetNLSVersionEx;
        }
    }
}
