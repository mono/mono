//------------------------------------------------------------------------------
// <copyright file="XmlWriterTraceListener.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Security.Permissions;
using System.Runtime.Versioning;

namespace System.Diagnostics {
    [HostProtection(Synchronization=true)]
    public class XmlWriterTraceListener : TextWriterTraceListener {
        private const string fixedHeader = "<E2ETraceEvent xmlns=\"http://schemas.microsoft.com/2004/06/E2ETraceEvent\">" + 
                                           "<System xmlns=\"http://schemas.microsoft.com/2004/06/windows/eventlog/system\">";
        private readonly string machineName = Environment.MachineName;
        private StringBuilder strBldr = null;
        private XmlTextWriter xmlBlobWriter = null;
        
        // Previously we had a bug where TraceTransfer did not respect the filter set on this listener.  We're fixing this
        // bug, but only for cases where the filter was set via config.  In the next side by side release, we'll remove
        // this and always respect the filter for TraceTransfer events.
        internal bool shouldRespectFilterOnTraceTransfer;

        public XmlWriterTraceListener(Stream stream) : base(stream){    }
        public XmlWriterTraceListener(Stream stream, string name) : base(stream, name){ }
        public XmlWriterTraceListener(TextWriter writer) : base(writer){    }
        public XmlWriterTraceListener(TextWriter writer, string name) : base(writer, name){    }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public XmlWriterTraceListener(string filename) : base(filename){    }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public XmlWriterTraceListener(string filename, string name) : base(filename, name){ }

        public override void Write(string message) {
            this.WriteLine(message);
        }

        public override void WriteLine(string message) {
            this.TraceEvent(null, SR.GetString(SR.TraceAsTraceSource), TraceEventType.Information, 0, message);
        }

        public override void Fail(string message, string detailMessage) {
            StringBuilder failMessage = new StringBuilder(message);
            if (detailMessage != null) {
                failMessage.Append(" ");
                failMessage.Append(detailMessage);
            }

            this.TraceEvent(null, SR.GetString(SR.TraceAsTraceSource), TraceEventType.Error, 0, failMessage.ToString());
        }

        public override void TraceEvent(TraceEventCache eventCache, String source, TraceEventType eventType, int id, string format, params object[] args) {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, format, args)) 
                return;

            WriteHeader(source, eventType, id, eventCache);

            string message;
            if (args != null)
                message = String.Format(CultureInfo.InvariantCulture, format, args);
            else
                message = format;
            
            WriteEscaped(message);
            
            WriteFooter(eventCache);
        }

        public override void TraceEvent(TraceEventCache eventCache, String source, TraceEventType eventType, int id, string message) {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, message)) 
                return;

            WriteHeader(source, eventType, id, eventCache);
            WriteEscaped(message);
            WriteFooter(eventCache);
        }

        public override void TraceData(TraceEventCache eventCache, String source, TraceEventType eventType, int id, object data) {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data)) 
                return;

            WriteHeader(source, eventType, id, eventCache);
            
            InternalWrite("<TraceData>");
            if (data != null) {
                InternalWrite("<DataItem>");
                WriteData(data);
                InternalWrite("</DataItem>");
            }
            InternalWrite("</TraceData>");

            WriteFooter(eventCache);
        }

        public override void TraceData(TraceEventCache eventCache, String source, TraceEventType eventType, int id, params object[] data) {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data)) 
                return;

            WriteHeader(source, eventType, id, eventCache);
            InternalWrite("<TraceData>");
            if (data != null) {
                for (int i=0; i<data.Length; i++) {
                    InternalWrite("<DataItem>");
                    if (data[i] != null)
                        WriteData(data[i]);
                    InternalWrite("</DataItem>");
                }
            }
            InternalWrite("</TraceData>");

            WriteFooter(eventCache);
        }

        // Special case XPathNavigator dataitems to write out XML blob unescaped
        private void WriteData(object data) {
            XPathNavigator xmlBlob = data as XPathNavigator;

            if(xmlBlob == null)
                WriteEscaped(data.ToString());
            else {
                if (strBldr == null) {
                    strBldr = new StringBuilder();
                    xmlBlobWriter = new XmlTextWriter(new StringWriter(strBldr, CultureInfo.CurrentCulture));
                }
                else 
                    strBldr.Length = 0;

                try {
                    // Rewind the blob to point to the root, this is needed to support multiple XMLTL in one TraceData call
                    xmlBlob.MoveToRoot();
                    xmlBlobWriter.WriteNode(xmlBlob, false);
                    InternalWrite(strBldr.ToString());
                }
                catch (Exception) { // We probably only care about XmlException for ill-formed XML though 
                    InternalWrite(data.ToString());
                }
            }
        }

        public override void Close() {
            base.Close();
            if (xmlBlobWriter != null) 
                xmlBlobWriter.Close();
            xmlBlobWriter = null;
            strBldr = null;
        }

        public override void TraceTransfer(TraceEventCache eventCache, String source, int id, string message, Guid relatedActivityId) {
            if (shouldRespectFilterOnTraceTransfer && (Filter != null && !Filter.ShouldTrace(eventCache, source, TraceEventType.Transfer, id, message)))
                return;

            WriteHeader(source, TraceEventType.Transfer, id, eventCache, relatedActivityId);
            WriteEscaped(message);
            WriteFooter(eventCache);
        }

        private void WriteHeader(String source, TraceEventType eventType, int id, TraceEventCache eventCache, Guid relatedActivityId) {
            WriteStartHeader(source, eventType, id, eventCache);
            InternalWrite("\" RelatedActivityID=\"");
            InternalWrite(relatedActivityId.ToString("B"));
            WriteEndHeader(eventCache);
        }
        
        private void WriteHeader(String source, TraceEventType eventType, int id, TraceEventCache eventCache) {
            WriteStartHeader(source, eventType, id, eventCache);
            WriteEndHeader(eventCache);
        }
        
        private void WriteStartHeader(String source, TraceEventType eventType, int id, TraceEventCache eventCache) {
            InternalWrite(fixedHeader);

            InternalWrite("<EventID>");
            InternalWrite(((uint)id).ToString(CultureInfo.InvariantCulture));
            InternalWrite("</EventID>");

            InternalWrite("<Type>3</Type>");

            InternalWrite("<SubType Name=\"");
            InternalWrite(eventType.ToString());
            InternalWrite("\">0</SubType>");

            InternalWrite("<Level>");
            int sev = (int)eventType;
            if (sev > 255)
                sev = 255;
            if (sev < 0)
                sev = 0;
            InternalWrite(sev.ToString(CultureInfo.InvariantCulture));
            InternalWrite("</Level>");
            
            InternalWrite("<TimeCreated SystemTime=\"");
            if (eventCache != null)
                InternalWrite(eventCache.DateTime.ToString("o", CultureInfo.InvariantCulture));
            else
                InternalWrite(DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
            InternalWrite("\" />");

            InternalWrite("<Source Name=\"");
            WriteEscaped(source);
            InternalWrite("\" />");

            InternalWrite("<Correlation ActivityID=\"");
            if (eventCache != null)
                InternalWrite(eventCache.ActivityId.ToString("B"));
            else
                InternalWrite(Guid.Empty.ToString("B"));
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        private void WriteEndHeader(TraceEventCache eventCache) {
            InternalWrite("\" />");

            InternalWrite("<Execution ProcessName=\"");
            InternalWrite(TraceEventCache.GetProcessName());
            InternalWrite("\" ProcessID=\"");
            InternalWrite(((uint)TraceEventCache.GetProcessId()).ToString(CultureInfo.InvariantCulture));
            InternalWrite("\" ThreadID=\"");
            if (eventCache != null)
                WriteEscaped(eventCache.ThreadId.ToString(CultureInfo.InvariantCulture));
            else
                WriteEscaped(TraceEventCache.GetThreadId().ToString(CultureInfo.InvariantCulture));
            InternalWrite("\" />");
                
            InternalWrite("<Channel/>");

            InternalWrite("<Computer>");
            InternalWrite(machineName);
            InternalWrite("</Computer>");

            InternalWrite("</System>");

            InternalWrite("<ApplicationData>");
        }

        private void WriteFooter(TraceEventCache eventCache) {
            bool writeLogicalOps = IsEnabled(TraceOptions.LogicalOperationStack);
            bool writeCallstack = IsEnabled(TraceOptions.Callstack);
            
            if (eventCache != null && (writeLogicalOps || writeCallstack)) {
                InternalWrite("<System.Diagnostics xmlns=\"http://schemas.microsoft.com/2004/08/System.Diagnostics\">");
            
                if (writeLogicalOps) {
                    InternalWrite("<LogicalOperationStack>");
                    
                    Stack s = eventCache.LogicalOperationStack as Stack;

                    if (s != null) {
                        foreach (object correlationId in s) {
                            InternalWrite("<LogicalOperation>");
                            WriteEscaped(correlationId.ToString());
                            InternalWrite("</LogicalOperation>");
                        }
                    }
                    InternalWrite("</LogicalOperationStack>");
                }

                InternalWrite("<Timestamp>");
                InternalWrite(eventCache.Timestamp.ToString(CultureInfo.InvariantCulture));
                InternalWrite("</Timestamp>");

                if (writeCallstack) {
                    InternalWrite("<Callstack>");
                    WriteEscaped(eventCache.Callstack);
                    InternalWrite("</Callstack>");
                }

                InternalWrite("</System.Diagnostics>");
            }

            InternalWrite("</ApplicationData></E2ETraceEvent>");
        }

        private void WriteEscaped(string str) {
            if (str == null)
                return;
            
            int lastIndex = 0;
            for (int i=0; i<str.Length; i++) {
                switch(str[i]) {
                    case '&':
                        InternalWrite(str.Substring(lastIndex, i-lastIndex));
                        InternalWrite("&amp;");
                        lastIndex = i +1;
                        break;
                    case '<':
                        InternalWrite(str.Substring(lastIndex, i-lastIndex));
                        InternalWrite("&lt;");
                        lastIndex = i +1;
                        break;
                    case '>':
                        InternalWrite(str.Substring(lastIndex, i-lastIndex));
                        InternalWrite("&gt;");
                        lastIndex = i +1;
                        break;
                    case '"':
                        InternalWrite(str.Substring(lastIndex, i-lastIndex));
                        InternalWrite("&quot;");
                        lastIndex = i +1;
                        break;
                    case '\'':
                        InternalWrite(str.Substring(lastIndex, i-lastIndex));
                        InternalWrite("&apos;");
                        lastIndex = i +1;
                        break;
                    case (char)0xD:
                        InternalWrite(str.Substring(lastIndex, i-lastIndex));
                        InternalWrite("&#xD;");
                        lastIndex = i +1;
                        break;
                    case (char)0xA:
                        InternalWrite(str.Substring(lastIndex, i-lastIndex));
                        InternalWrite("&#xA;");
                        lastIndex = i +1;
                        break;
                }
            }
            InternalWrite(str.Substring(lastIndex, str.Length-lastIndex));
        }

        private void InternalWrite(string message) {
            if (!EnsureWriter()) return;   
            // NeedIndent is nop
            writer.Write(message);
        }
    }
}
