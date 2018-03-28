//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Runtime.Diagnostics;
    using System.Xml;
    
    class PerformanceCounterTraceRecord : TraceRecord
    {
        string categoryName;
        string perfCounterName;
        string instanceName;

        internal PerformanceCounterTraceRecord(string perfCounterName) 
            : this(null, perfCounterName, null)
        {
        }

        internal PerformanceCounterTraceRecord(string categoryName, string perfCounterName) 
            : this(categoryName, perfCounterName, null)
        {
        }

        internal PerformanceCounterTraceRecord(string categoryName, string perfCounterName, string instanceName)
        {
            this.categoryName = categoryName;
            this.perfCounterName = perfCounterName;
            this.instanceName = instanceName;
        }

        internal override string EventId { get { return BuildEventId("PerformanceCounter"); } }

        internal override void WriteTo(XmlWriter writer)
        {
            if (!String.IsNullOrEmpty(this.categoryName))
            {
                writer.WriteElementString("PerformanceCategoryName", this.categoryName);
            }

            writer.WriteElementString("PerformanceCounterName", this.perfCounterName);


            if (!String.IsNullOrEmpty(this.instanceName))
            {
                writer.WriteElementString("InstanceName", this.instanceName);
            }
        }
    }
}
