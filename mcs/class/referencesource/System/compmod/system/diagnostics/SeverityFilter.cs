//------------------------------------------------------------------------------
// <copyright file="SeverityFilter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace System.Diagnostics {
    public class EventTypeFilter : TraceFilter {
        private SourceLevels level;
        
        public EventTypeFilter(SourceLevels level) {
            this.level = level;
        }

        public override bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, 
                                         object[] args, object data1, object[] data) {
                                         
             return ((int) eventType & (int) level) != 0;
        }

        public SourceLevels EventType {
            get {
                return level;
            }
            set {
                level = value;
            }
        }
    }
}
