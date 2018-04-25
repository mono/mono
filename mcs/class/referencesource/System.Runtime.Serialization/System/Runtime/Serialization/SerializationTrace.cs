//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization
{
    using System.Diagnostics;
    using System.Security;

#if USE_REFEMIT
    public static class SerializationTrace
#else
    static class SerializationTrace
#endif
    {
        [Fx.Tag.SecurityNote(Critical = "Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        static TraceSource codeGen;

        internal static SourceSwitch CodeGenerationSwitch
        {
            get
            {
                return CodeGenerationTraceSource.Switch;
            }
        }

        internal static void WriteInstruction(int lineNumber, string instruction)
        {
            CodeGenerationTraceSource.TraceInformation("{0:00000}: {1}", lineNumber, instruction);
        }

#if USE_REFEMIT
        public static void TraceInstruction(string instruction)
#else
        internal static void TraceInstruction(string instruction)
#endif
        {
            CodeGenerationTraceSource.TraceEvent(TraceEventType.Verbose, 0, instruction);
        }

        static TraceSource CodeGenerationTraceSource
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical codeGen field.",
                Safe = "Get-only properties only needs to be protected for write; initialized in getter if null.")]
            [SecuritySafeCritical]
            get
            {
                if (codeGen == null)
                    codeGen = new TraceSource("System.Runtime.Serialization.CodeGeneration");
                return codeGen;
            }
        }
    }
}


