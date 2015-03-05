//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Diagnostics
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;

    static class TraceCode
    {
        // IdentityModel TraceCodes
        public const int IdentityModel = 0xC0000;
        public const int AuthorizationContextCreated = TraceCode.IdentityModel | 0X0002;
        public const int AuthorizationPolicyEvaluated = TraceCode.IdentityModel | 0X0003;
        public const int ServiceBindingCheck = TraceCode.IdentityModel | 0X0004;
        public const int ChannelBindingCheck = TraceCode.IdentityModel | 0x0005;
        public const int Diagnostics = TraceCode.IdentityModel | 0x0006;
    }

    static class TraceUtility
    {
        static Dictionary<int, string> traceCodes = new Dictionary<int, string>( 5 )
        {
            { TraceCode.IdentityModel, "IdentityModel" },
            { TraceCode.AuthorizationContextCreated, "AuthorizationContextCreated" },
            { TraceCode.AuthorizationPolicyEvaluated, "AuthorizationPolicyEvaluated" },
            { TraceCode.ServiceBindingCheck, "ServiceBindingCheck" },
            { TraceCode.ChannelBindingCheck, "ChannelBindingCheck" },
            { TraceCode.Diagnostics, "Diagnostics" }
        };

        internal static void TraceEvent( TraceEventType severity, int traceCode, string traceDescription )
        {
            TraceEvent( severity, traceCode, traceDescription, null, null, null );
        }

        // These methods require a TraceRecord to be allocated, so we want them to show up on profiles if the caller didn't avoid
        // allocating the TraceRecord by using ShouldTrace.
        [MethodImpl( MethodImplOptions.NoInlining )]
        internal static void TraceEvent( TraceEventType severity, int traceCode, string traceDescription, TraceRecord extendedData, object source, Exception exception )
        {
		// FIXME: implement
        }

        internal static void TraceString( TraceEventType eventType, string formatString, params object[] args )
        {
            if ( DiagnosticUtility.ShouldTrace( eventType ) )
            {
                if ( null != args && args.Length > 0 )
                {
                    TraceEvent( eventType, TraceCode.IdentityModel, String.Format( CultureInfo.InvariantCulture, formatString, args ) );
                }
                else
                {
                    TraceEvent( eventType, TraceCode.IdentityModel, formatString );
                }
            }
        }
    }
}
