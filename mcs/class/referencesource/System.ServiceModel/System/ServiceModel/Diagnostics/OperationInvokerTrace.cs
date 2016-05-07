//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.ServiceModel.Channels;
    using System.Diagnostics;
    using System.Reflection;

#if USE_REFEMIT
    public static class OperationInvokerTrace
#else
    static class OperationInvokerTrace
#endif
    {
        static TraceSource codeGenSource;
        static MethodInfo traceInstructionMethod;

        internal static SourceSwitch CodeGenerationSwitch
        {
            get { return CodeGenerationTraceSource.Switch; }
        }

        internal static void WriteInstruction(int lineNumber, string instruction)
        {
            CodeGenerationTraceSource.TraceInformation("{0:00000}: {1}", lineNumber, instruction);
        }

        internal static MethodInfo TraceInstructionMethod
        {
            get
            {
                if (traceInstructionMethod == null)
                    traceInstructionMethod = typeof(OperationInvokerTrace).GetMethod("TraceInstruction", BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                return traceInstructionMethod;
            }
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
            get
            {
                if (codeGenSource == null)
                    codeGenSource = new TraceSource("System.ServiceModel.OperationInvoker.CodeGeneration");
                return codeGenSource;
            }
        }
    }
}

