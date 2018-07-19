//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Diagnostics
{
    using System.ComponentModel;

    public enum PerformanceCounterScope
    {
        Off = 0,     // +
        ServiceOnly = 1,
        All = 2,
        Default = 3, // *
    }

    static class PerformanceCounterScopeHelper
    {
        internal static bool IsDefined(PerformanceCounterScope value)
        {
            return
                value == PerformanceCounterScope.Off
                || value == PerformanceCounterScope.Default
                || value == PerformanceCounterScope.ServiceOnly
                || value == PerformanceCounterScope.All;
        }

        public static void Validate(PerformanceCounterScope value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value,
                    typeof(PerformanceCounterScope)));
            }
        }
    }
}
