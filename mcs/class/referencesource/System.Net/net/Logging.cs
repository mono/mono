//------------------------------------------------------------------------------
// <copyright file="Logging.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Net
{
    using System.Collections;
    using System.IO;
    using System.Threading;
    using System.Diagnostics;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;
    using System.Globalization;
    using Microsoft.Win32;
    using System.Text;
    internal static class Logging
    {
        private static P2PTraceSource s_P2PTraceSource;
        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="AppDomain.add_ProcessExit(System.EventHandler):System.Void" />
        // <SatisfiesLinkDemand Name="AppDomain.add_DomainUnload(System.EventHandler):System.Void" />
        // <ReferencesCritical Name="Method: ProcessExitEventHandler(Object, EventArgs):Void" Ring="2" />
        // <ReferencesCritical Name="Method: DomainUnloadEventHandler(Object, EventArgs):Void" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        static Logging()
        {
            s_P2PTraceSource = new P2PTraceSource();
            if (s_P2PTraceSource.Switch.ShouldTrace(TraceEventType.Critical))
            {
                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.ProcessExit += new EventHandler(ProcessExitEventHandler);
                //currentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionEventHandler);
                currentDomain.DomainUnload += new EventHandler(DomainUnloadEventHandler);
            }
        }
        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: Close():Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private static void DomainUnloadEventHandler(object sender, EventArgs e)
        {
            Close();
        }

        /*
        private static void UnhandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs e)
        {
            s_P2PTraceSource.TraceEvent(TraceEventType.Critical, UNHANDLED_EXCEPTION_EVENT_ID, "Unhandled Exception {0}", e.ExceptionObject);
        }
        */
        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: Close():Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private static void ProcessExitEventHandler(object sender, EventArgs e)
        {
            Close();
        }
        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="TraceSource.Close():System.Void" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private static void Close()
        {
            s_P2PTraceSource.Close();
        }
        //private Logging() {}
        internal static P2PTraceSource P2PTraceSource
        {
            get
            {
                return s_P2PTraceSource;
            }
        }

        internal static void Enter(TraceSource source, string method)
        {
            if(source.Switch.ShouldTrace(TraceEventType.Verbose))
            {
                source.TraceEvent(TraceEventType.Verbose, 0, "Entering --> " + method);
            }
        }
        internal static void Leave(TraceSource source, string message)
        {
            if (source.Switch.ShouldTrace(TraceEventType.Verbose))
            {
                source.TraceEvent(TraceEventType.Verbose, 0, "Leaving <-- " + message);
            }
        }

        /*
        internal static void Enter(TraceSource source, string type, object obj, string method)
        {

        }
        internal static void Enter(TraceSource source, string type, object obj, string method, params object[] args)
        {

        }
         * */

        internal static void DumpData(TraceSource source, TraceEventType eventType, int maxDataSize, byte[] buffer, int offset, int length)
        {
            if (buffer == null ||
                    buffer.Length == 0 ||
                    offset > buffer.Length)
            {
                return;
            }
            if (length > maxDataSize)
            {
                source.TraceEvent(eventType, 0, "dumping {0} of {1} bytes", maxDataSize, length);
                length = maxDataSize;
            }
            if ((length < 0) || (length > buffer.Length - offset))
            {
                length = buffer.Length - offset;
            }
            do
            {
                int n = Math.Min(length, 16);
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format(CultureInfo.CurrentCulture, "{0:X8} : ", offset));
                for (int i = 0; i < n; ++i)
                {
                    sb.Append(String.Format(CultureInfo.CurrentCulture, "{0:X2}", buffer[offset + i]) + ((i == 7) ? '-' : ' '));
                }
                for (int i = n; i < 16; ++i)
                {
                    sb.Append("   ");
                }
                sb.Append(": ");
                for (int i = 0; i < n; ++i)
                {
                    sb.Append(((buffer[offset + i] < 0x20) || (buffer[offset + i] > 0x7e))
                                ? '.'
                                : (char)(buffer[offset + i]));
                }
                source.TraceEvent(eventType, 0, sb.ToString());
                offset += n;
                length -= n;
            } while (length > 0);

        }

    }

    internal class P2PTraceSource : TraceSource
    {
        private const string P2PTraceSourceName = "System.Net.PeerToPeer";
        private const int DefaultMaxDataSize = 1024;
        private const string AttributeNameMaxDataSize = "maxdatasize";
        private static readonly string[] P2PTraceSourceSupportedAttributes = new string[] { AttributeNameMaxDataSize };
        private readonly int m_maxDataSize = DefaultMaxDataSize;

        internal P2PTraceSource() :base(P2PTraceSourceName)
        {
            if (Attributes.ContainsKey(AttributeNameMaxDataSize))
            {
                try{
                    m_maxDataSize = Int32.Parse(Attributes[AttributeNameMaxDataSize], NumberFormatInfo.InvariantInfo);
                }
                catch (Exception exception) {
                    if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {
                        throw;
                    }
                }
            }
        }
        protected  override string[] GetSupportedAttributes()
        {
            return P2PTraceSourceSupportedAttributes;
        }

        internal int MaxDataSize
        {
            get
            {
                return m_maxDataSize;
            }
        }
    }
 

}
