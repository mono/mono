namespace System.Workflow.Runtime
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Holds trace sources for the runtime and associated modules
    /// </summary>
    internal static class WorkflowTrace
    {
        static TraceSource runtime;
        static TraceSource tracking;
        static TraceSource host;

        /// <summary>
        /// Tracesource for the core runtime
        /// </summary>
        internal static TraceSource Runtime
        {
            get { return runtime; }
        }

        /// <summary>
        /// Tracesource for tracking
        /// </summary>
        internal static TraceSource Tracking
        {
            get { return tracking; }
        }

        /// <summary>
        /// Tracesource for the host
        /// </summary>
        internal static TraceSource Host
        {
            get { return host; }
        }

        /// <summary>
        /// Statically set up trace sources
        /// 
        /// To enable logging to a file, add lines like the following to your app config file.
        /*
            <system.diagnostics>
                <switches>
                    <add name="System.Workflow LogToFile" value="1" />
                </switches>
            </system.diagnostics>
        */
        /// To enable tracing to default trace listeners, add lines like the following
        /*
            <system.diagnostics>
                <switches>
                    <add name="System.Workflow LogToTraceListener" value="1" />
                </switches>
            </system.diagnostics>
        */
        /// </summary>
        static WorkflowTrace()
        {
            runtime = new TraceSource("System.Workflow.Runtime");
            runtime.Switch = new SourceSwitch("System.Workflow.Runtime", SourceLevels.Off.ToString());
            // we'll use ID of 1 for the scheduler, 0 for rest of runtime

            tracking = new TraceSource("System.Workflow.Runtime.Tracking");
            tracking.Switch = new SourceSwitch("System.Workflow.Runtime.Tracking", SourceLevels.Off.ToString());

            host = new TraceSource("System.Workflow.Runtime.Hosting");
            host.Switch = new SourceSwitch("System.Workflow.Runtime.Hosting", SourceLevels.Off.ToString());



            BooleanSwitch logToFile = new BooleanSwitch("System.Workflow LogToFile", "Log traces to file");
            if (logToFile.Enabled)
            {
                TextWriterTraceListener fileLog = new TextWriterTraceListener("WorkflowTrace.log");
                // add to global Listeners list
                Trace.Listeners.Add(fileLog);
                // don't add to tracking (which probably has its own log)
                runtime.Listeners.Add(fileLog);
                host.Listeners.Add(fileLog);
            }

            BooleanSwitch traceToDefault = new BooleanSwitch("System.Workflow LogToTraceListeners", "Trace to listeners in Trace.Listeners", "0");
            if (traceToDefault.Enabled)
            {
                foreach (TraceListener listener in Trace.Listeners)
                {
                    if (!(listener is DefaultTraceListener))
                    {
                        runtime.Listeners.Add(listener);
                        tracking.Listeners.Add(listener);
                        host.Listeners.Add(listener);
                    }
                }
            }
        }
    }
}
