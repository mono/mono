//------------------------------------------------------------------------------
// <copyright file="SourceFilter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;

namespace System.Diagnostics {
    public class SourceFilter : TraceFilter {
        private string src;
        
        public SourceFilter(string source) {
            Source = source;
        }

        public override bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, 
                                         object[] args, object data1, object[] data) {
             if (source == null)
                throw new ArgumentNullException("source");
             
             return String.Equals(src, source);
        }

        public String Source {
            get { 
                return src; 
            }
            set {
                if (value == null)
                   throw new ArgumentNullException("source");
                src = value;
            }
        }
    }
}
                
