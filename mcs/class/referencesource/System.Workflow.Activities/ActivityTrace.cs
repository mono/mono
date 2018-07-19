namespace System.Workflow.Activities
{
    using System;
    using System.Diagnostics;


    internal static class WorkflowActivityTrace
    {
        static TraceSource activity;
        static TraceSource rules;

        internal static TraceSource Activity
        {
            get { return activity; }
        }

        internal static TraceSource Rules
        {
            get { return rules; }
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
        static WorkflowActivityTrace()
        {
            activity = new TraceSource("System.Workflow.Activities");
            activity.Switch = new SourceSwitch("System.Workflow.Activities", SourceLevels.Off.ToString());

            rules = new TraceSource("System.Workflow.Activities.Rules");
            rules.Switch = new SourceSwitch("System.Workflow.Activities.Rules", SourceLevels.Off.ToString());

            foreach (TraceListener listener in Trace.Listeners)
            {
                if (!(listener is DefaultTraceListener))
                {
                    activity.Listeners.Add(listener);
                    rules.Listeners.Add(listener);
                }
            }
        }
    }
}
