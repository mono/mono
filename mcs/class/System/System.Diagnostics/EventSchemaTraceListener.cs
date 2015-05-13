namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true), HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
    public class EventSchemaTraceListener : TextWriterTraceListener
    {
        private int _bufferSize;
        private bool _initialized;
        private long _maxFileSize;
        private int _maxNumberOfFiles;
        private System.Diagnostics.TraceLogRetentionOption _retention;
        private const int _retryThreshold = 2;
        private string fileName;
        private readonly object m_lockObject;
        private static readonly string machineName = Environment.MachineName;
        private const int s_defaultPayloadSize = 0x200;
        private const string s_eventHeader = "<Event xmlns=\"http://schemas.microsoft.com/win/2004/08/events/event\"><System><Provider Guid=\"";
        private const string s_optionBufferSize = "bufferSize";
        private const string s_optionLogRetention = "logRetentionOption";
        private const string s_optionMaximumFileSize = "maximumFileSize";
        private const string s_optionMaximumNumberOfFiles = "maximumNumberOfFiles";
        private const string s_userDataHeader = "<System.Diagnostics.UserData xmlns=\"http://schemas.microsoft.com/win/2006/09/System.Diagnostics/UserData/\">";
        private TraceWriter traceWriter;

        public EventSchemaTraceListener(string fileName) : this(fileName, string.Empty)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventSchemaTraceListener(string fileName, string name) : this(fileName, name, 0x8000)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventSchemaTraceListener(string fileName, string name, int bufferSize) : this(fileName, name, bufferSize, System.Diagnostics.TraceLogRetentionOption.SingleFileUnboundedSize)
        {
        }

        public EventSchemaTraceListener(string fileName, string name, int bufferSize, System.Diagnostics.TraceLogRetentionOption logRetentionOption) : this(fileName, name, bufferSize, logRetentionOption, 0x9c4000L)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventSchemaTraceListener(string fileName, string name, int bufferSize, System.Diagnostics.TraceLogRetentionOption logRetentionOption, long maximumFileSize) : this(fileName, name, bufferSize, logRetentionOption, maximumFileSize, 2)
        {
        }

        public EventSchemaTraceListener(string fileName, string name, int bufferSize, System.Diagnostics.TraceLogRetentionOption logRetentionOption, long maximumFileSize, int maximumNumberOfFiles)
        {
            this._bufferSize = 0x8000;
            this._retention = System.Diagnostics.TraceLogRetentionOption.SingleFileUnboundedSize;
            this._maxFileSize = 0x9c4000L;
            this._maxNumberOfFiles = 2;
            this.m_lockObject = new object();
            if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", System.SR.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((logRetentionOption < System.Diagnostics.TraceLogRetentionOption.UnlimitedSequentialFiles) || (logRetentionOption > System.Diagnostics.TraceLogRetentionOption.SingleFileBoundedSize))
            {
                throw new ArgumentOutOfRangeException("logRetentionOption", System.SR.GetString("ArgumentOutOfRange_NeedValidLogRetention"));
            }
            base.Name = name;
            this.fileName = fileName;
            if ((!string.IsNullOrEmpty(this.fileName) && (fileName[0] != Path.DirectorySeparatorChar)) && ((fileName[0] != Path.AltDirectorySeparatorChar) && !Path.IsPathRooted(fileName)))
            {
                this.fileName = Path.Combine(Path.GetDirectoryName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile), this.fileName);
            }
            this._retention = logRetentionOption;
            this._bufferSize = bufferSize;
            this._SetMaxFileSize(maximumFileSize, false);
            this._SetMaxNumberOfFiles(maximumNumberOfFiles, false);
        }

        private static void _InternalBuildRaw(StringBuilder writer, string message)
        {
            writer.Append(message);
        }

        [SecurityCritical]
        private void _InternalWriteRaw(StringBuilder writer)
        {
            if (this.EnsureWriterInternal())
            {
                this.traceWriter.Write(writer.ToString());
            }
        }

        private void _SetMaxFileSize(long maximumFileSize, bool throwOnError)
        {
            switch (this._retention)
            {
                case System.Diagnostics.TraceLogRetentionOption.UnlimitedSequentialFiles:
                case System.Diagnostics.TraceLogRetentionOption.LimitedCircularFiles:
                case System.Diagnostics.TraceLogRetentionOption.LimitedSequentialFiles:
                case System.Diagnostics.TraceLogRetentionOption.SingleFileBoundedSize:
                    if ((maximumFileSize < 0L) && throwOnError)
                    {
                        throw new ArgumentOutOfRangeException("maximumFileSize", System.SR.GetString("ArgumentOutOfRange_NeedNonNegNum"));
                    }
                    if (maximumFileSize < this._bufferSize)
                    {
                        if (throwOnError)
                        {
                            throw new ArgumentOutOfRangeException("maximumFileSize", System.SR.GetString("ArgumentOutOfRange_NeedMaxFileSizeGEBufferSize"));
                        }
                        this._maxFileSize = this._bufferSize;
                    }
                    else
                    {
                        this._maxFileSize = maximumFileSize;
                    }
                    return;

                case System.Diagnostics.TraceLogRetentionOption.SingleFileUnboundedSize:
                    this._maxFileSize = -1L;
                    return;
            }
        }

        private void _SetMaxNumberOfFiles(int maximumNumberOfFiles, bool throwOnError)
        {
            switch (this._retention)
            {
                case System.Diagnostics.TraceLogRetentionOption.UnlimitedSequentialFiles:
                    this._maxNumberOfFiles = -1;
                    return;

                case System.Diagnostics.TraceLogRetentionOption.LimitedCircularFiles:
                    if (maximumNumberOfFiles < 2)
                    {
                        if (throwOnError)
                        {
                            throw new ArgumentOutOfRangeException("maximumNumberOfFiles", System.SR.GetString("ArgumentOutOfRange_NeedValidMaxNumFiles", new object[] { 2 }));
                        }
                        this._maxNumberOfFiles = 2;
                        return;
                    }
                    this._maxNumberOfFiles = maximumNumberOfFiles;
                    return;

                case System.Diagnostics.TraceLogRetentionOption.SingleFileUnboundedSize:
                case System.Diagnostics.TraceLogRetentionOption.SingleFileBoundedSize:
                    this._maxNumberOfFiles = 1;
                    return;

                case System.Diagnostics.TraceLogRetentionOption.LimitedSequentialFiles:
                    if (maximumNumberOfFiles < 1)
                    {
                        if (throwOnError)
                        {
                            throw new ArgumentOutOfRangeException("maximumNumberOfFiles", System.SR.GetString("ArgumentOutOfRange_NeedValidMaxNumFiles", new object[] { 1 }));
                        }
                        this._maxNumberOfFiles = 1;
                        return;
                    }
                    this._maxNumberOfFiles = maximumNumberOfFiles;
                    return;
            }
        }

        private static void BuildEscaped(StringBuilder writer, string str)
        {
            if (str != null)
            {
                int startIndex = 0;
                for (int i = 0; i < str.Length; i++)
                {
                    switch (str[i])
                    {
                        case '\n':
                            _InternalBuildRaw(writer, str.Substring(startIndex, i - startIndex));
                            _InternalBuildRaw(writer, "&#xA;");
                            startIndex = i + 1;
                            break;

                        case '\r':
                            _InternalBuildRaw(writer, str.Substring(startIndex, i - startIndex));
                            _InternalBuildRaw(writer, "&#xD;");
                            startIndex = i + 1;
                            break;

                        case '&':
                            _InternalBuildRaw(writer, str.Substring(startIndex, i - startIndex));
                            _InternalBuildRaw(writer, "&amp;");
                            startIndex = i + 1;
                            break;

                        case '\'':
                            _InternalBuildRaw(writer, str.Substring(startIndex, i - startIndex));
                            _InternalBuildRaw(writer, "&apos;");
                            startIndex = i + 1;
                            break;

                        case '"':
                            _InternalBuildRaw(writer, str.Substring(startIndex, i - startIndex));
                            _InternalBuildRaw(writer, "&quot;");
                            startIndex = i + 1;
                            break;

                        case '<':
                            _InternalBuildRaw(writer, str.Substring(startIndex, i - startIndex));
                            _InternalBuildRaw(writer, "&lt;");
                            startIndex = i + 1;
                            break;

                        case '>':
                            _InternalBuildRaw(writer, str.Substring(startIndex, i - startIndex));
                            _InternalBuildRaw(writer, "&gt;");
                            startIndex = i + 1;
                            break;
                    }
                }
                _InternalBuildRaw(writer, str.Substring(startIndex, str.Length - startIndex));
            }
        }

        private static void BuildFooter(StringBuilder writer, TraceEventType eventType, TraceEventCache eventCache, bool isUserData, TraceOptions opts)
        {
            if (!isUserData)
            {
                _InternalBuildRaw(writer, "</EventData>");
            }
            else
            {
                _InternalBuildRaw(writer, "</UserData>");
            }
            _InternalBuildRaw(writer, "<RenderingInfo Culture=\"en-EN\">");
            switch (eventType)
            {
                case TraceEventType.Verbose:
                    _InternalBuildRaw(writer, "<Level>Verbose</Level>");
                    break;

                case TraceEventType.Start:
                    _InternalBuildRaw(writer, "<Level>Information</Level><Opcode>Start</Opcode>");
                    break;

                case TraceEventType.Critical:
                    _InternalBuildRaw(writer, "<Level>Critical</Level>");
                    break;

                case TraceEventType.Error:
                    _InternalBuildRaw(writer, "<Level>Error</Level>");
                    break;

                case TraceEventType.Warning:
                    _InternalBuildRaw(writer, "<Level>Warning</Level>");
                    break;

                case TraceEventType.Information:
                    _InternalBuildRaw(writer, "<Level>Information</Level>");
                    break;

                case TraceEventType.Stop:
                    _InternalBuildRaw(writer, "<Level>Information</Level><Opcode>Stop</Opcode>");
                    break;

                case TraceEventType.Suspend:
                    _InternalBuildRaw(writer, "<Level>Information</Level><Opcode>Suspend</Opcode>");
                    break;

                case TraceEventType.Resume:
                    _InternalBuildRaw(writer, "<Level>Information</Level><Opcode>Resume</Opcode>");
                    break;

                case TraceEventType.Transfer:
                    _InternalBuildRaw(writer, "<Level>Information</Level><Opcode>Transfer</Opcode>");
                    break;
            }
            _InternalBuildRaw(writer, "</RenderingInfo>");
            if ((eventCache != null) && (((TraceOptions.Callstack | TraceOptions.Timestamp | TraceOptions.LogicalOperationStack) & opts) != TraceOptions.None))
            {
                _InternalBuildRaw(writer, "<System.Diagnostics.ExtendedData xmlns=\"http://schemas.microsoft.com/2006/09/System.Diagnostics/ExtendedData\">");
                if ((TraceOptions.Timestamp & opts) != TraceOptions.None)
                {
                    _InternalBuildRaw(writer, "<Timestamp>");
                    _InternalBuildRaw(writer, eventCache.Timestamp.ToString(CultureInfo.InvariantCulture));
                    _InternalBuildRaw(writer, "</Timestamp>");
                }
                if ((TraceOptions.LogicalOperationStack & opts) != TraceOptions.None)
                {
                    Stack logicalOperationStack = eventCache.LogicalOperationStack;
                    _InternalBuildRaw(writer, "<LogicalOperationStack>");
                    if ((logicalOperationStack != null) && (logicalOperationStack.Count > 0))
                    {
                        foreach (object obj2 in logicalOperationStack)
                        {
                            _InternalBuildRaw(writer, "<LogicalOperation>");
                            BuildEscaped(writer, obj2.ToString());
                            _InternalBuildRaw(writer, "</LogicalOperation>");
                        }
                    }
                    _InternalBuildRaw(writer, "</LogicalOperationStack>");
                }
                if ((TraceOptions.Callstack & opts) != TraceOptions.None)
                {
                    _InternalBuildRaw(writer, "<Callstack>");
                    BuildEscaped(writer, eventCache.Callstack);
                    _InternalBuildRaw(writer, "</Callstack>");
                }
                _InternalBuildRaw(writer, "</System.Diagnostics.ExtendedData>");
            }
            _InternalBuildRaw(writer, "</Event>");
        }

        [SecurityCritical]
        private static void BuildHeader(StringBuilder writer, string source, TraceEventType eventType, int id, TraceEventCache eventCache, string relatedActivityId, bool isUserData, TraceOptions opts)
        {
            _InternalBuildRaw(writer, "<Event xmlns=\"http://schemas.microsoft.com/win/2004/08/events/event\"><System><Provider Guid=\"");
            _InternalBuildRaw(writer, "{00000000-0000-0000-0000-000000000000}");
            _InternalBuildRaw(writer, "\"/><EventID>");
            _InternalBuildRaw(writer, ((id < 0) ? 0 : ((uint) id)).ToString(CultureInfo.InvariantCulture));
            _InternalBuildRaw(writer, "</EventID>");
            _InternalBuildRaw(writer, "<Level>");
            int num = (int) eventType;
            int num2 = num;
            if ((num > 0xff) || (num < 0))
            {
                num = 8;
            }
            _InternalBuildRaw(writer, num.ToString(CultureInfo.InvariantCulture));
            _InternalBuildRaw(writer, "</Level>");
            if (num2 > 0xff)
            {
                num2 /= 0x100;
                _InternalBuildRaw(writer, "<Opcode>");
                _InternalBuildRaw(writer, num2.ToString(CultureInfo.InvariantCulture));
                _InternalBuildRaw(writer, "</Opcode>");
            }
            if ((TraceOptions.DateTime & opts) != TraceOptions.None)
            {
                _InternalBuildRaw(writer, "<TimeCreated SystemTime=\"");
                if (eventCache != null)
                {
                    _InternalBuildRaw(writer, eventCache.DateTime.ToString("o", CultureInfo.InvariantCulture));
                }
                else
                {
                    _InternalBuildRaw(writer, DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
                }
                _InternalBuildRaw(writer, "\"/>");
            }
            _InternalBuildRaw(writer, "<Correlation ActivityID=\"");
            _InternalBuildRaw(writer, Trace.CorrelationManager.ActivityId.ToString("B"));
            if (relatedActivityId != null)
            {
                _InternalBuildRaw(writer, "\" RelatedActivityID=\"");
                _InternalBuildRaw(writer, relatedActivityId);
            }
            _InternalBuildRaw(writer, "\"/>");
            if ((eventCache != null) && (((TraceOptions.ThreadId | TraceOptions.ProcessId) & opts) != TraceOptions.None))
            {
                _InternalBuildRaw(writer, "<Execution ");
                _InternalBuildRaw(writer, "ProcessID=\"");
                _InternalBuildRaw(writer, ((uint) eventCache.ProcessId).ToString(CultureInfo.InvariantCulture));
                _InternalBuildRaw(writer, "\" ");
                _InternalBuildRaw(writer, "ThreadID=\"");
                _InternalBuildRaw(writer, eventCache.ThreadId);
                _InternalBuildRaw(writer, "\"");
                _InternalBuildRaw(writer, "/>");
            }
            _InternalBuildRaw(writer, "<Computer>");
            _InternalBuildRaw(writer, machineName);
            _InternalBuildRaw(writer, "</Computer>");
            _InternalBuildRaw(writer, "</System>");
            if (!isUserData)
            {
                _InternalBuildRaw(writer, "<EventData>");
            }
            else
            {
                _InternalBuildRaw(writer, "<UserData>");
            }
        }

        private static void BuildMessage(StringBuilder writer, string message)
        {
            _InternalBuildRaw(writer, "<Data>");
            BuildEscaped(writer, message);
            _InternalBuildRaw(writer, "</Data>");
        }

        private static void BuildUserData(StringBuilder writer, object data)
        {
            UnescapedXmlDiagnosticData data2 = data as UnescapedXmlDiagnosticData;
            if (data2 == null)
            {
                BuildMessage(writer, data.ToString());
            }
            else
            {
                _InternalBuildRaw(writer, data2.ToString());
            }
        }

        public override void Close()
        {
            try
            {
                if (this.traceWriter != null)
                {
                    this.traceWriter.Flush();
                    this.traceWriter.Close();
                }
            }
            finally
            {
                this.traceWriter = null;
                base.Close();
            }
        }

        [SecurityCritical]
        private bool EnsureWriterInternal()
        {
            if (this.traceWriter == null)
            {
                if (string.IsNullOrEmpty(this.fileName))
                {
                    return false;
                }
                lock (this.m_lockObject)
                {
                    if (this.traceWriter != null)
                    {
                        return true;
                    }
                    string fileName = this.fileName;
                    for (int i = 0; i < 2; i++)
                    {
                        try
                        {
                            this.Init();
                            this.traceWriter = new TraceWriter(fileName, this._bufferSize, this._retention, this._maxFileSize, this._maxNumberOfFiles);
                            break;
                        }
                        catch (IOException)
                        {
                            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(this.fileName);
                            string extension = Path.GetExtension(this.fileName);
                            fileName = fileNameWithoutExtension + Guid.NewGuid().ToString() + extension;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            break;
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                    if (this.traceWriter == null)
                    {
                        this.fileName = null;
                    }
                }
            }
            return (this.traceWriter != null);
        }

        public override void Fail(string message, string detailMessage)
        {
            StringBuilder builder = new StringBuilder(message);
            if (detailMessage != null)
            {
                builder.Append(" ");
                builder.Append(detailMessage);
            }
            this.TraceEvent(null, System.SR.GetString("TraceAsTraceSource"), TraceEventType.Error, 0, builder.ToString());
        }

        [SecurityCritical]
        public override void Flush()
        {
            if (this.EnsureWriterInternal())
            {
                this.traceWriter.Flush();
            }
        }

        protected internal override string[] GetSupportedAttributes()
        {
            return new string[] { "bufferSize", "logRetentionOption", "maximumFileSize", "maximumNumberOfFiles" };
        }

        private void Init()
        {
            if (!this._initialized)
            {
                lock (this.m_lockObject)
                {
                    if (!this._initialized)
                    {
                        try
                        {
                            if (base.Attributes.ContainsKey("bufferSize"))
                            {
                                int num = int.Parse(base.Attributes["bufferSize"], CultureInfo.InvariantCulture);
                                if (num > 0)
                                {
                                    this._bufferSize = num;
                                }
                            }
                            if (base.Attributes.ContainsKey("logRetentionOption"))
                            {
                                string strA = base.Attributes["logRetentionOption"];
                                if (string.Compare(strA, "SingleFileUnboundedSize", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    this._retention = System.Diagnostics.TraceLogRetentionOption.SingleFileUnboundedSize;
                                }
                                else if (string.Compare(strA, "LimitedCircularFiles", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    this._retention = System.Diagnostics.TraceLogRetentionOption.LimitedCircularFiles;
                                }
                                else if (string.Compare(strA, "UnlimitedSequentialFiles", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    this._retention = System.Diagnostics.TraceLogRetentionOption.UnlimitedSequentialFiles;
                                }
                                else if (string.Compare(strA, "SingleFileBoundedSize", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    this._retention = System.Diagnostics.TraceLogRetentionOption.SingleFileBoundedSize;
                                }
                                else if (string.Compare(strA, "LimitedSequentialFiles", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    this._retention = System.Diagnostics.TraceLogRetentionOption.LimitedSequentialFiles;
                                }
                                else
                                {
                                    this._retention = System.Diagnostics.TraceLogRetentionOption.SingleFileUnboundedSize;
                                }
                            }
                            if (base.Attributes.ContainsKey("maximumFileSize"))
                            {
                                long maximumFileSize = long.Parse(base.Attributes["maximumFileSize"], CultureInfo.InvariantCulture);
                                this._SetMaxFileSize(maximumFileSize, false);
                            }
                            if (base.Attributes.ContainsKey("maximumNumberOfFiles"))
                            {
                                int maximumNumberOfFiles = int.Parse(base.Attributes["maximumNumberOfFiles"], CultureInfo.InvariantCulture);
                                this._SetMaxNumberOfFiles(maximumNumberOfFiles, false);
                            }
                        }
                        catch (Exception)
                        {
                        }
                        finally
                        {
                            this._initialized = true;
                        }
                    }
                }
            }
        }

        [SecurityCritical]
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null))
            {
                StringBuilder writer = new StringBuilder(0x200);
                BuildHeader(writer, source, eventType, id, eventCache, null, true, base.TraceOutputOptions);
                if (data != null)
                {
                    _InternalBuildRaw(writer, "<System.Diagnostics.UserData xmlns=\"http://schemas.microsoft.com/win/2006/09/System.Diagnostics/UserData/\">");
                    BuildUserData(writer, data);
                    _InternalBuildRaw(writer, "</System.Diagnostics.UserData>");
                }
                BuildFooter(writer, eventType, eventCache, true, base.TraceOutputOptions);
                this._InternalWriteRaw(writer);
            }
        }

        [SecurityCritical]
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data))
            {
                StringBuilder writer = new StringBuilder(0x200);
                BuildHeader(writer, source, eventType, id, eventCache, null, true, base.TraceOutputOptions);
                if ((data != null) && (data.Length > 0))
                {
                    _InternalBuildRaw(writer, "<System.Diagnostics.UserData xmlns=\"http://schemas.microsoft.com/win/2006/09/System.Diagnostics/UserData/\">");
                    for (int i = 0; i < data.Length; i++)
                    {
                        if (data[i] != null)
                        {
                            BuildUserData(writer, data[i]);
                        }
                    }
                    _InternalBuildRaw(writer, "</System.Diagnostics.UserData>");
                }
                BuildFooter(writer, eventType, eventCache, true, base.TraceOutputOptions);
                this._InternalWriteRaw(writer);
            }
        }

        [SecurityCritical]
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, message, null, null, null))
            {
                StringBuilder writer = new StringBuilder(0x200);
                BuildHeader(writer, source, eventType, id, eventCache, null, false, base.TraceOutputOptions);
                BuildMessage(writer, message);
                BuildFooter(writer, eventType, eventCache, false, base.TraceOutputOptions);
                this._InternalWriteRaw(writer);
            }
        }

        [SecurityCritical]
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, format, args, null, null))
            {
                string str;
                StringBuilder writer = new StringBuilder(0x200);
                BuildHeader(writer, source, eventType, id, eventCache, null, false, base.TraceOutputOptions);
                if (args != null)
                {
                    str = string.Format(CultureInfo.InvariantCulture, format, args);
                }
                else
                {
                    str = format;
                }
                BuildMessage(writer, str);
                BuildFooter(writer, eventType, eventCache, false, base.TraceOutputOptions);
                this._InternalWriteRaw(writer);
            }
        }

        [SecurityCritical]
        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            StringBuilder writer = new StringBuilder(0x200);
            BuildHeader(writer, source, TraceEventType.Transfer, id, eventCache, relatedActivityId.ToString("B"), false, base.TraceOutputOptions);
            BuildMessage(writer, message);
            BuildFooter(writer, TraceEventType.Transfer, eventCache, false, base.TraceOutputOptions);
            this._InternalWriteRaw(writer);
        }

        public override void Write(string message)
        {
            this.WriteLine(message);
        }

        public override void WriteLine(string message)
        {
            this.TraceEvent(null, System.SR.GetString("TraceAsTraceSource"), TraceEventType.Information, 0, message);
        }

        public int BufferSize
        {
            get
            {
                this.Init();
                return this._bufferSize;
            }
        }

        public override bool IsThreadSafe
        {
            get
            {
                return true;
            }
        }

        public long MaximumFileSize
        {
            get
            {
                this.Init();
                return this._maxFileSize;
            }
        }

        public int MaximumNumberOfFiles
        {
            get
            {
                this.Init();
                return this._maxNumberOfFiles;
            }
        }

        public System.Diagnostics.TraceLogRetentionOption TraceLogRetentionOption
        {
            get
            {
                this.Init();
                return this._retention;
            }
        }

        public new TextWriter Writer
        {
            [SecurityCritical]
            get
            {
                this.EnsureWriter();
                return this.traceWriter;
            }
            set
            {
                throw new NotSupportedException(System.SR.GetString("NotSupported_SetTextWriter"));
            }
        }

        private sealed class TraceWriter : TextWriter
        {
            private System.Text.Encoding encNoBOMwithFallback;
            private object m_lockObject;
            private Stream stream;

            [SecurityCritical]
            internal TraceWriter(string _fileName, int bufferSize, TraceLogRetentionOption retention, long maxFileSize, int maxNumberOfFiles) : base(CultureInfo.InvariantCulture)
            {
                this.m_lockObject = new object();
                this.stream = new LogStream(_fileName, bufferSize, (LogRetentionOption) retention, maxFileSize, maxNumberOfFiles);
            }

            protected override void Dispose(bool disposing)
            {
                try
                {
                    if (disposing)
                    {
                        this.stream.Close();
                    }
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }

            public override void Flush()
            {
                this.stream.Flush();
            }

            private static System.Text.Encoding GetEncodingWithFallback(System.Text.Encoding encoding)
            {
                System.Text.Encoding encoding2 = (System.Text.Encoding) encoding.Clone();
                encoding2.EncoderFallback = EncoderFallback.ReplacementFallback;
                encoding2.DecoderFallback = DecoderFallback.ReplacementFallback;
                return encoding2;
            }

            public override void Write(string value)
            {
                try
                {
                    byte[] bytes = this.Encoding.GetBytes(value);
                    this.stream.Write(bytes, 0, bytes.Length);
                }
                catch (Exception)
                {
                    if (this.stream is BufferedStream2)
                    {
                        ((BufferedStream2) this.stream).DiscardBuffer();
                    }
                }
            }

            public override System.Text.Encoding Encoding
            {
                get
                {
                    if (this.encNoBOMwithFallback == null)
                    {
                        lock (this.m_lockObject)
                        {
                            if (this.encNoBOMwithFallback == null)
                            {
                                this.encNoBOMwithFallback = GetEncodingWithFallback(new UTF8Encoding(false));
                            }
                        }
                    }
                    return this.encNoBOMwithFallback;
                }
            }
        }
    }
}

