//------------------------------------------------------------------------------
// <copyright file="SqlEventSource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">mihailsm</owner>
//------------------------------------------------------------------------------

namespace System.Data
{
    using System;
    using System.Diagnostics.Tracing;

    [EventSource(Name = SqlEventSource.EventSourceName)]
    internal sealed class SqlEventSource : EventSource
    {
        internal const string EventSourceName = "Microsoft-AdoNet-SystemData";

        /// <summary>
        /// Defines EventId for BeginExecute (Reader, Scalar, NonQuery, XmlReader).
        /// </summary>
        private const int BeginExecuteEventId = 1;

        /// <summary>
        /// Defines EventId for EndExecute (Reader, Scalar, NonQuery, XmlReader).
        /// </summary>
        private const int EndExecuteEventId = 2;

        // Defines the singleton instance for the Resources ETW provider
        internal static readonly SqlEventSource Log = new SqlEventSource();


        /// <summary>
        /// Keyword definitions.  These represent logical groups of events that can be turned on and off independently
        /// Often each task has a keyword, but where tasks are determined by subsystem, keywords are determined by
        /// usefulness to end users to filter.  Generally users don't mind extra events if they are not high volume
        /// so grouping low volume events together in a single keywords is OK (users can post-filter by task if desired)
        /// <remarks>The visibility of the enum has to be public, otherwise there will be an ArgumentException on calling related WriteEvent method.</remarks>
        /// </summary>
        public static class Keywords
        {
            public const EventKeywords SqlClient = (EventKeywords)0x0001; // This is bit 0
        }

        public static class Tasks // this name is important for EventSource
        {
            /// <summary>Task that tracks sql command execute.</summary>
            public const EventTask ExecuteCommand = (EventTask)1;
        }

        private SqlEventSource() 
        {
        }

        // unfortunately these are not marked as Start/Stop opcodes.  The reason is that we dont want them to participate in 
        // the EventSource activity IDs (because they currently don't use tasks and this simply confuses the logic) and 
        // because of versioning requirements we don't have ActivityOptions capability (because mscorlib and System.Data version 
        // at different rates)  Sigh...
        [Event(SqlEventSource.BeginExecuteEventId, Keywords = Keywords.SqlClient)]
        public void BeginExecute(int objectId, string dataSource, string database, string commandText)
        {
            // we do not use unsafe code for better performance optization here because optimized helpers make the code unsafe where that would not be the case otherwise. 
            // This introduces the question of partial trust, which is complex in the SQL case (there are a lot of scenarios and SQL has special security support).   
            WriteEvent(SqlEventSource.BeginExecuteEventId, objectId, dataSource, database, commandText);
        }

        // unfortunately these are not marked as Start/Stop opcodes.  The reason is that we dont want them to participate in 
        // the EventSource activity IDs (because they currently don't use tasks and this simply confuses the logic) and 
        // because of versioning requirements we don't have ActivityOptions capability (because mscorlib and System.Data version 
        // at different rates)  Sigh...
        [Event(SqlEventSource.EndExecuteEventId, Keywords = Keywords.SqlClient)]
        public void EndExecute(int objectId, int compositeState, int sqlExceptionNumber)
        {
            WriteEvent(SqlEventSource.EndExecuteEventId, objectId, compositeState, sqlExceptionNumber);
        }
    }
}
