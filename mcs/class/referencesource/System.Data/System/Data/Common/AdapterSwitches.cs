//------------------------------------------------------------------------------
// <copyright file="AdapterSwitches.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">markash</owner>
// <owner current="true" primary="false">laled</owner>
//------------------------------------------------------------------------------

#if DEBUG

using System.Diagnostics;

namespace System.Data.Common {

    internal static class AdapterSwitches {

        static private TraceSwitch _dataSchema;

        static internal TraceSwitch DataSchema {
            get {
                TraceSwitch dataSchema = _dataSchema;
                if (null == dataSchema) {
                    _dataSchema = dataSchema = new TraceSwitch("Data.Schema", "Enable tracing for schema actions.");
                }
                return dataSchema;
            }
        }
    }
}
#endif
