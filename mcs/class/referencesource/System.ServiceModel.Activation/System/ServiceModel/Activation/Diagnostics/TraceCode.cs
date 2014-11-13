//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Activation.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Diagnostics;    
    using System.Collections.Generic;
    
    static class TraceCode
    {
        // WebHost trace codes
        public const int Activation = 0X90000;
        public const int WebHostFailedToCompile = TraceCode.Activation | 0X0001;
        public const int WebHostServiceActivated = TraceCode.Activation | 0X0002;
        public const int WebHostFailedToActivateService = TraceCode.Activation | 0X0003;
        public const int WebHostCompilation = TraceCode.Activation | 0X0004;
        public const int WebHostDebugRequest = TraceCode.Activation | 0X0005;
        public const int WebHostProtocolMisconfigured = TraceCode.Activation | 0X0006;
        public const int WebHostServiceCloseFailed = TraceCode.Activation | 0X0007;
        public const int WebHostNoCBTSupport = TraceCode.Activation | 0X0008;
    }    
}
