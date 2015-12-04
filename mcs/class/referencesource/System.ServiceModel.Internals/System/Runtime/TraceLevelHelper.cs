//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Diagnostics;

    /// <remarks>
    /// [....] (11/15/10, CSDMain 194940) - Previously, this code first checked that the opcode was set to informational.  If not, it would check
    /// the opcode name for start, stop, suspend, or resume and use that or return Information otherwise.  This does not work well with the latest 
    /// ETW changes where almost every event has a task and opcode.  With the old logic, if an opcode is set on the event with a level such as 
    /// warning or error, the level would be incorrectly reported in diagnostic tracing as informational.  Also, start/stop/suspend/resume events 
    /// have an overloaded meaning in diagnostic tracing that the svctraceviewer would misinterpret.  To keep diagnostic tracing from breaking, this
    /// class now checks for start/stop/suspend/resume opcodes and returns the level if any of those do not match.  Furthermore, any events defined 
    /// that are shared between diagnostics and ETW should not use start/stop/suspend/resume opcodes unless explicitly intended for use in 
    /// diagnostics tracing.
    /// </remarks>
    class TraceLevelHelper
    {
        static TraceEventType[] EtwLevelToTraceEventType = { TraceEventType.Critical, TraceEventType.Critical, TraceEventType.Error,
                TraceEventType.Warning, TraceEventType.Information, TraceEventType.Verbose
            };

        internal static TraceEventType GetTraceEventType(byte level, byte opcode)
        {
            switch (opcode)
            {
                case (byte)TraceEventOpcode.Start:
                    return TraceEventType.Start;
                case (byte)TraceEventOpcode.Stop:
                    return TraceEventType.Stop;
                case (byte)TraceEventOpcode.Suspend:
                    return TraceEventType.Suspend;
                case (byte)TraceEventOpcode.Resume:
                    return TraceEventType.Resume;
                default:
                    return EtwLevelToTraceEventType[(int)level];
            }
        }

        internal static TraceEventType GetTraceEventType(TraceEventLevel level)
        {
            return EtwLevelToTraceEventType[(int)level];
        }

        internal static TraceEventType GetTraceEventType(byte level)
        {
            return EtwLevelToTraceEventType[(int)level];
        }

        internal static string LookupSeverity(TraceEventLevel level, TraceEventOpcode opcode)
        {
            string severity;
            switch (opcode)
            {
                case TraceEventOpcode.Start:
                    severity = "Start";
                    break;
                case TraceEventOpcode.Stop:
                    severity = "Stop";
                    break;
                case TraceEventOpcode.Suspend:
                    severity = "Suspend";
                    break;
                case TraceEventOpcode.Resume:
                    severity = "Resume";
                    break;
                default:
                    switch (level)
                    {
                        case TraceEventLevel.Critical:
                            severity = "Critical";
                            break;
                        case TraceEventLevel.Error:
                            severity = "Error";
                            break;
                        case TraceEventLevel.Warning:
                            severity = "Warning";
                            break;
                        case TraceEventLevel.Informational:
                            severity = "Information";
                            break;
                        case TraceEventLevel.Verbose:
                            severity = "Verbose";
                            break;
                        default:
                            severity = level.ToString();
                            break;
                    }
                    break;
            }
            return severity;
        }
    }
}
