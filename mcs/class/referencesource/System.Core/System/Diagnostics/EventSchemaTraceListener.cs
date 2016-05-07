//------------------------------------------------------------------------------
// <copyright file="EventSchemaTraceListener.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.Win32;
using System.Text;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Threading;
using System.Security.Permissions;
using System.Runtime.Versioning;

namespace System.Diagnostics {

[HostProtection(Synchronization=true)]
[System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
public class EventSchemaTraceListener : TextWriterTraceListener {    
    private const string s_optionBufferSize = "bufferSize";
    private const string s_optionLogRetention = "logRetentionOption";
    private const string s_optionMaximumFileSize = "maximumFileSize";
    private const string s_optionMaximumNumberOfFiles = "maximumNumberOfFiles";

    private const string s_userDataHeader = "<System.Diagnostics.UserData xmlns=\"http://schemas.microsoft.com/win/2006/09/System.Diagnostics/UserData/\">";
    private const string s_eventHeader = "<Event xmlns=\"http://schemas.microsoft.com/win/2004/08/events/event\"><System><Provider Guid=\"";
    private const int s_defaultPayloadSize = 512;
    private const int _retryThreshold = 2;      // Threshold to retry creating TraceWriter on failure
    
    private static readonly string machineName = Environment.MachineName; 
    private TraceWriter traceWriter;
    
    private string fileName;
    private bool _initialized;
    
    // Retention policy
    private int _bufferSize = LogStream.DefaultBufferSize; 
    private TraceLogRetentionOption _retention = (TraceLogRetentionOption)LogStream.DefaultRetention;
    private long _maxFileSize = LogStream.DefaultFileSize;
    private int _maxNumberOfFiles = LogStream.DefaultNumberOfFiles;

    private readonly object m_lockObject = new Object();


    [ResourceExposure(ResourceScope.Machine)]
    [ResourceConsumption(ResourceScope.Machine)]
    public EventSchemaTraceListener(string fileName) 
        : this(fileName, String.Empty) {
    }

    [ResourceExposure(ResourceScope.Machine)]
    [ResourceConsumption(ResourceScope.Machine)]
    public EventSchemaTraceListener(string fileName, string name) 
        :this(fileName, name, LogStream.DefaultBufferSize){ 
    }

    [ResourceExposure(ResourceScope.Machine)]
    [ResourceConsumption(ResourceScope.Machine)]
    public EventSchemaTraceListener(String fileName, String name, int bufferSize) 
        : this(fileName, name, bufferSize, (TraceLogRetentionOption)LogStream.DefaultRetention) { 
    }

    [ResourceExposure(ResourceScope.Machine)]
    [ResourceConsumption(ResourceScope.Machine)]
    public EventSchemaTraceListener(String fileName, String name, int bufferSize, TraceLogRetentionOption logRetentionOption) 
        : this(fileName, name, bufferSize, logRetentionOption, LogStream.DefaultFileSize) { 
    }

    [ResourceExposure(ResourceScope.Machine)]
    [ResourceConsumption(ResourceScope.Machine)]
    public EventSchemaTraceListener(String fileName, String name, int bufferSize, TraceLogRetentionOption logRetentionOption, long maximumFileSize) 
        : this(fileName, name, bufferSize, logRetentionOption, maximumFileSize, LogStream.DefaultNumberOfFiles) { 
    }

    [ResourceExposure(ResourceScope.Machine)]
    [ResourceConsumption(ResourceScope.Machine)]
    public EventSchemaTraceListener(String fileName, String name, int bufferSize, TraceLogRetentionOption logRetentionOption, long maximumFileSize, int maximumNumberOfFiles) { 
        if (bufferSize < 0) 
            throw new ArgumentOutOfRangeException(s_optionBufferSize, SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNum));
        if (logRetentionOption < TraceLogRetentionOption.UnlimitedSequentialFiles || logRetentionOption > TraceLogRetentionOption.SingleFileBoundedSize)  
            throw new ArgumentOutOfRangeException(s_optionLogRetention, SR.GetString(SR.ArgumentOutOfRange_NeedValidLogRetention));

        base.Name = name;
        this.fileName = fileName; 

        if (!String.IsNullOrEmpty(this.fileName) && (fileName[0] != Path.DirectorySeparatorChar) && (fileName[0] != Path.AltDirectorySeparatorChar) && !Path.IsPathRooted(fileName)) {
		this.fileName = Path.Combine(Path.GetDirectoryName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile), this.fileName);
        }

        this._retention = logRetentionOption;
        this._bufferSize = bufferSize;
        _SetMaxFileSize(maximumFileSize, false);
        _SetMaxNumberOfFiles(maximumNumberOfFiles, false);
    }

    // Hide base class version
    new public TextWriter Writer {
        [System.Security.SecurityCritical]
        get {
            EnsureWriter();
            return traceWriter;
        }
        
        set {
            throw new NotSupportedException(SR.GetString(SR.NotSupported_SetTextWriter));
        }
    }

    public override bool IsThreadSafe 
    {
        get { return true; }
    }

    public int BufferSize {
        get { 
            Init();
            return _bufferSize; 
        }
    }

    public TraceLogRetentionOption TraceLogRetentionOption {
        get { 
            Init();
            return _retention; 
        }
    }

    public long MaximumFileSize {
        get { 
            Init();
            return _maxFileSize; 
        }
    }

    public int MaximumNumberOfFiles {
        get { 
            Init();
            return _maxNumberOfFiles; 
        }
    }

    public override void Close() {
        try {
            if (traceWriter != null) {
                traceWriter.Flush();
                traceWriter.Close();
            }
        }
        finally {
            traceWriter = null;
            //fileName = null;  // It is more useful to allow the listener to be reopened upon subsequent write
            base.Close();       // This should be No-op
        }
    }

    [System.Security.SecurityCritical]
    public override void Flush() {
        if (!EnsureWriter()) return;
        traceWriter.Flush();
    }

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

    [System.Security.SecurityCritical]
    public override void TraceEvent(TraceEventCache eventCache, String source, TraceEventType eventType, int id, string format, params object[] args) {
        if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, format, args, null, null)) 
            return;

        StringBuilder writer = new StringBuilder(s_defaultPayloadSize);
        BuildHeader(writer, source, eventType, id, eventCache, null, false, this.TraceOutputOptions);

        string message;
        if (args != null)
            message = String.Format(CultureInfo.InvariantCulture, format, args);
        else
            message = format;
        
        BuildMessage(writer, message);
        
        BuildFooter(writer, eventType, eventCache, false, this.TraceOutputOptions);
        _InternalWriteRaw(writer);
    }

    [System.Security.SecurityCritical]
    public override void TraceEvent(TraceEventCache eventCache, String source, TraceEventType eventType, int id, string message) {
        if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, message, null, null, null)) 
            return;

        StringBuilder writer = new StringBuilder(s_defaultPayloadSize);
        BuildHeader(writer, source, eventType, id, eventCache, null, false, this.TraceOutputOptions);
        BuildMessage(writer, message);
        BuildFooter(writer, eventType, eventCache, false, this.TraceOutputOptions);
        _InternalWriteRaw(writer);
    }

    [System.Security.SecurityCritical]
    public override void TraceData(TraceEventCache eventCache, String source, TraceEventType eventType, int id, object data) {
        if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null)) 
            return;

        StringBuilder writer = new StringBuilder(s_defaultPayloadSize);
        BuildHeader(writer, source, eventType, id, eventCache, null, true, this.TraceOutputOptions);
        
        // No validation of user provided data. No explicit namespace scope. The data should identify the XML schema by itself. 
        if (data != null) {
            _InternalBuildRaw(writer, s_userDataHeader);
            BuildUserData(writer, data);
            _InternalBuildRaw(writer, "</System.Diagnostics.UserData>");
        }

        BuildFooter(writer, eventType, eventCache, true, this.TraceOutputOptions);
        _InternalWriteRaw(writer);
    }

    [System.Security.SecurityCritical]
    public override void TraceData(TraceEventCache eventCache, String source, TraceEventType eventType, int id, params object[] data) {
        if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data)) 
            return;

        StringBuilder writer = new StringBuilder(s_defaultPayloadSize);
        BuildHeader(writer, source, eventType, id, eventCache, null, true, this.TraceOutputOptions);

        // No validation of user provided data. No explicit namespace scope. The data should identify the XML schema by itself. 
        if ((data != null) && (data.Length > 0)) {
            _InternalBuildRaw(writer, s_userDataHeader);
            for (int i=0; i<data.Length; i++) {
                if (data[i] != null)
                    BuildUserData(writer, data[i]);
            }
            _InternalBuildRaw(writer, "</System.Diagnostics.UserData>");
        }
        
        BuildFooter(writer, eventType, eventCache, true, this.TraceOutputOptions);
        _InternalWriteRaw(writer);
    }

    [System.Security.SecurityCritical]
    public override void TraceTransfer(TraceEventCache eventCache, String source, int id, string message, Guid relatedActivityId) {
        StringBuilder writer = new StringBuilder(s_defaultPayloadSize);
        BuildHeader(writer, source, TraceEventType.Transfer, id, eventCache, relatedActivityId.ToString("B"), false, this.TraceOutputOptions);
        BuildMessage(writer, message);
        BuildFooter(writer, TraceEventType.Transfer, eventCache, false, this.TraceOutputOptions);
        _InternalWriteRaw(writer);
    }

    private static void BuildMessage(StringBuilder writer, string message) {
        _InternalBuildRaw(writer, "<Data>");
        BuildEscaped(writer, message);
        _InternalBuildRaw(writer, "</Data>");
    }

    // Writes the system properties
    [System.Security.SecurityCritical]
    private static void BuildHeader(StringBuilder writer, String source, TraceEventType eventType, int id, TraceEventCache eventCache, string relatedActivityId, bool isUserData, TraceOptions opts) {
        _InternalBuildRaw(writer, s_eventHeader);
        
        // Ideally, we want to enable provider guid at the TraceSource level 
        // We can't blindly use the source param here as we need to a valid guid!
        //_InternalBuildRaw(writer, source);
        
        // For now, trace empty guid for provider id
        _InternalBuildRaw(writer, "{00000000-0000-0000-0000-000000000000}");
        
        _InternalBuildRaw(writer, "\"/><EventID>");
        _InternalBuildRaw(writer, ((uint)((id <0)?0:id)).ToString(CultureInfo.InvariantCulture));
        _InternalBuildRaw(writer, "</EventID>");

        _InternalBuildRaw(writer, "<Level>");
        int sev = (int)eventType;
        int op = sev;
        // Treat overflow conditions as Information
        // Logical operation events (>255) such as Start/Stop will fall into this bucket
        if ((sev > 255) || (sev < 0))
            sev = 0x08;
        _InternalBuildRaw(writer, sev.ToString(CultureInfo.InvariantCulture));
        _InternalBuildRaw(writer, "</Level>");
        
        // Logical operation events (>255) such as Start/Stop will be morphed into a byte value
        if (op > 255) {
            op /= 256;
            _InternalBuildRaw(writer, "<Opcode>");
            _InternalBuildRaw(writer, op.ToString(CultureInfo.InvariantCulture));
            _InternalBuildRaw(writer, "</Opcode>");
        }

        if ((TraceOptions.DateTime & opts) != 0) {
            _InternalBuildRaw(writer, "<TimeCreated SystemTime=\"");
            if (eventCache != null)
                _InternalBuildRaw(writer, eventCache.DateTime.ToString("o", CultureInfo.InvariantCulture));
            else
                _InternalBuildRaw(writer, DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            _InternalBuildRaw(writer, "\"/>");
        }

        // Currently correlation is always traced, we could optimize this further 
        _InternalBuildRaw(writer, "<Correlation ActivityID=\"");
        // ActivityId is typed as GUID by Correlation manager but most tracing usage typically calls ToString which is costly!
        _InternalBuildRaw(writer, Trace.CorrelationManager.ActivityId.ToString("B")); 
        if (relatedActivityId != null) {
            _InternalBuildRaw(writer, "\" RelatedActivityID=\"");
            _InternalBuildRaw(writer, relatedActivityId);
        }
        _InternalBuildRaw(writer, "\"/>");


        // Currently not tracing ProcessName as there is no place for it in the SystemProperties
        // Should we bother adding this to our own section?
        if (eventCache != null && ((TraceOptions.ProcessId | TraceOptions.ThreadId)  & opts) != 0) {
            _InternalBuildRaw(writer, "<Execution ");
            _InternalBuildRaw(writer, "ProcessID=\"");
            // Review ProcessId as string can be cached!
            _InternalBuildRaw(writer, ((uint)eventCache.ProcessId).ToString(CultureInfo.InvariantCulture));
            _InternalBuildRaw(writer, "\" ");
            _InternalBuildRaw(writer, "ThreadID=\"");
            _InternalBuildRaw(writer, eventCache.ThreadId);
            _InternalBuildRaw(writer, "\"");
            _InternalBuildRaw(writer, "/>");
        }

        //_InternalBuildRaw(writer, "<Channel/>");

        _InternalBuildRaw(writer, "<Computer>");
        _InternalBuildRaw(writer, machineName);
        _InternalBuildRaw(writer, "</Computer>");


        _InternalBuildRaw(writer, "</System>");

        if (!isUserData) {
            _InternalBuildRaw(writer, "<EventData>");
        }
        else {
            _InternalBuildRaw(writer, "<UserData>");
        }
    }

    private static void BuildFooter(StringBuilder writer, TraceEventType eventType, TraceEventCache eventCache, bool isUserData, TraceOptions opts) {
        if (!isUserData) {
            _InternalBuildRaw(writer, "</EventData>");
        }
        else {
            _InternalBuildRaw(writer, "</UserData>");
        }

        // Provide English resource string for EventType  
        _InternalBuildRaw(writer, "<RenderingInfo Culture=\"en-EN\">");

        //Avoid Enum.ToString which uses reflection
        switch (eventType) {
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
        case TraceEventType.Verbose:
            _InternalBuildRaw(writer, "<Level>Verbose</Level>");
            break;
        
        case TraceEventType.Start:
            _InternalBuildRaw(writer, "<Level>Information</Level><Opcode>Start</Opcode>");
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
        default:
            break;
        }
        _InternalBuildRaw(writer, "</RenderingInfo>");

        // Custom System.Diagnostics information as its own schema

        if (eventCache != null && ((TraceOptions.LogicalOperationStack | TraceOptions.Callstack | TraceOptions.Timestamp)  & opts) != 0) {
            _InternalBuildRaw(writer, "<System.Diagnostics.ExtendedData xmlns=\"http://schemas.microsoft.com/2006/09/System.Diagnostics/ExtendedData\">");
        
            if ((TraceOptions.Timestamp & opts) != 0) {
                _InternalBuildRaw(writer, "<Timestamp>");
                _InternalBuildRaw(writer, eventCache.Timestamp.ToString(CultureInfo.InvariantCulture));
                _InternalBuildRaw(writer, "</Timestamp>");
            }

            if ((TraceOptions.LogicalOperationStack & opts) != 0) {
                Stack stk = eventCache.LogicalOperationStack as Stack;
                _InternalBuildRaw(writer, "<LogicalOperationStack>");
                if ((stk != null) && (stk.Count > 0)) {
                    foreach (object obj in stk) {
                        _InternalBuildRaw(writer, "<LogicalOperation>");
                        BuildEscaped(writer, obj.ToString());
                        _InternalBuildRaw(writer, "</LogicalOperation>");
                    }
                }
                _InternalBuildRaw(writer, "</LogicalOperationStack>");
            }

            if ((TraceOptions.Callstack & opts) != 0) {
                _InternalBuildRaw(writer, "<Callstack>");
                BuildEscaped(writer, eventCache.Callstack);
                _InternalBuildRaw(writer, "</Callstack>");
            }

            _InternalBuildRaw(writer, "</System.Diagnostics.ExtendedData>");
        }

        _InternalBuildRaw(writer, "</Event>");
    }

    private static void BuildEscaped(StringBuilder writer, string str) {
        if (str == null)
            return;
        
        int lastIndex = 0;
        for (int i=0; i<str.Length; i++) {
            switch(str[i]) {
                case '&':
                    _InternalBuildRaw(writer, str.Substring(lastIndex, i-lastIndex));
                    _InternalBuildRaw(writer, "&amp;");
                    lastIndex = i +1;
                    break;
                case '<':
                    _InternalBuildRaw(writer, str.Substring(lastIndex, i-lastIndex));
                    _InternalBuildRaw(writer, "&lt;");
                    lastIndex = i +1;
                    break;
                case '>':
                    _InternalBuildRaw(writer, str.Substring(lastIndex, i-lastIndex));
                    _InternalBuildRaw(writer, "&gt;");
                    lastIndex = i +1;
                    break;
                case '"':
                    _InternalBuildRaw(writer, str.Substring(lastIndex, i-lastIndex));
                    _InternalBuildRaw(writer, "&quot;");
                    lastIndex = i +1;
                    break;
                case '\'':
                    _InternalBuildRaw(writer, str.Substring(lastIndex, i-lastIndex));
                    _InternalBuildRaw(writer, "&apos;");
                    lastIndex = i +1;
                    break;
                case (char)0xD:
                    _InternalBuildRaw(writer, str.Substring(lastIndex, i-lastIndex));
                    _InternalBuildRaw(writer, "&#xD;");
                    lastIndex = i +1;
                    break;
                case (char)0xA:
                    _InternalBuildRaw(writer, str.Substring(lastIndex, i-lastIndex));
                    _InternalBuildRaw(writer, "&#xA;");
                    lastIndex = i +1;
                    break;
            }
        }
        _InternalBuildRaw(writer, str.Substring(lastIndex, str.Length-lastIndex));
    }

    // Special case UnescapedXmlDiagnosticData items to write out XML blob unescaped
    private static void BuildUserData(StringBuilder writer, object data) {
        UnescapedXmlDiagnosticData xmlBlob = data as UnescapedXmlDiagnosticData;

        if(xmlBlob == null) {
            BuildMessage(writer, data.ToString());
        }
        else {
            _InternalBuildRaw(writer, xmlBlob.ToString());
        }
    }
    
    private static void _InternalBuildRaw(StringBuilder writer, string message) {
        writer.Append(message);
    }

    [System.Security.SecurityCritical]
    private void _InternalWriteRaw(StringBuilder writer) {
        if (!EnsureWriter()) return;   

        // NeedIndent is nop
        traceWriter.Write(writer.ToString());
    }

    protected override string[] GetSupportedAttributes() {
        return new String[]{s_optionBufferSize, s_optionLogRetention, s_optionMaximumFileSize, s_optionMaximumNumberOfFiles};
    }

    private void Init() {
        if (!_initialized) {
            // We could use Interlocked but this one time overhead is probably not a concern
            lock (m_lockObject) {    
                if (!_initialized) {
                    try {
                        if (Attributes.ContainsKey(s_optionBufferSize)) {
                            int bufferSize = Int32.Parse(Attributes[s_optionBufferSize], CultureInfo.InvariantCulture);
                            if (bufferSize > 0) 
                                _bufferSize = bufferSize;
                        }

                        if (Attributes.ContainsKey(s_optionLogRetention)) {

                            // Enum.Parse is costly!
                            string retOption = Attributes[s_optionLogRetention];
                            
                            if (String.Compare(retOption, "SingleFileUnboundedSize", StringComparison.OrdinalIgnoreCase) == 0)
                                _retention = TraceLogRetentionOption.SingleFileUnboundedSize;
                            else if (String.Compare(retOption, "LimitedCircularFiles", StringComparison.OrdinalIgnoreCase) == 0)
                                _retention = TraceLogRetentionOption.LimitedCircularFiles;
                            else if (String.Compare(retOption, "UnlimitedSequentialFiles", StringComparison.OrdinalIgnoreCase) == 0)
                                _retention = TraceLogRetentionOption.UnlimitedSequentialFiles;
                            else if (String.Compare(retOption, "SingleFileBoundedSize", StringComparison.OrdinalIgnoreCase) == 0)
                                _retention = TraceLogRetentionOption.SingleFileBoundedSize;
                            else if (String.Compare(retOption, "LimitedSequentialFiles", StringComparison.OrdinalIgnoreCase) == 0)
                                _retention = TraceLogRetentionOption.LimitedSequentialFiles;
                            else {   
                                _retention = TraceLogRetentionOption.SingleFileUnboundedSize;
                            }
                        }

                        if (Attributes.ContainsKey(s_optionMaximumFileSize)) {
                            long maxFileSize = Int64.Parse(Attributes[s_optionMaximumFileSize], CultureInfo.InvariantCulture);
                            _SetMaxFileSize(maxFileSize, false);
                        }

                        if (Attributes.ContainsKey(s_optionMaximumNumberOfFiles)) {
                            int maxNumberOfFiles = Int32.Parse(Attributes[s_optionMaximumNumberOfFiles], CultureInfo.InvariantCulture);
                            _SetMaxNumberOfFiles(maxNumberOfFiles, false);
                        }
                    }
                    catch(Exception) {
                        // Avoid trhowing errors from populating config values, let the defaults stand
                        Debug.Assert(false, "Exception while populating config values for EventSchemaTraceListener!");
                    }
                    finally {
                        _initialized = true;
                    }
                }
            }
        }
    }

    private void _SetMaxFileSize(long maximumFileSize, bool throwOnError) {
        switch (this._retention) {
        case TraceLogRetentionOption.SingleFileUnboundedSize:
            this._maxFileSize = -1;
            break;
        case TraceLogRetentionOption.SingleFileBoundedSize:
        case TraceLogRetentionOption.UnlimitedSequentialFiles:
        case TraceLogRetentionOption.LimitedSequentialFiles:
        case TraceLogRetentionOption.LimitedCircularFiles:
            if ((maximumFileSize < 0) && throwOnError)
                throw new ArgumentOutOfRangeException(s_optionMaximumFileSize, SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNum));
            
            if (maximumFileSize < this._bufferSize) { 
                if (throwOnError) {
                    throw new ArgumentOutOfRangeException(s_optionMaximumFileSize, SR.GetString(SR.ArgumentOutOfRange_NeedMaxFileSizeGEBufferSize));
                }
                else {
                    this._maxFileSize = this._bufferSize;
                }
            }
            else 
                this._maxFileSize = maximumFileSize;
            break;
        }
    }

    private void _SetMaxNumberOfFiles(int maximumNumberOfFiles, bool throwOnError) {
        switch (this._retention) {
        case TraceLogRetentionOption.SingleFileUnboundedSize:
        case TraceLogRetentionOption.SingleFileBoundedSize:
            this._maxNumberOfFiles = 1;
            break;
        
        case TraceLogRetentionOption.UnlimitedSequentialFiles:
            this._maxNumberOfFiles = -1;
            break;
        
        case TraceLogRetentionOption.LimitedSequentialFiles:
            if (maximumNumberOfFiles < 1) {
                if (throwOnError) {
                    throw new ArgumentOutOfRangeException(s_optionMaximumNumberOfFiles, SR.GetString(SR.ArgumentOutOfRange_NeedValidMaxNumFiles, 1));
                }
                this._maxNumberOfFiles = 1;
            }
            else {
                this._maxNumberOfFiles = maximumNumberOfFiles;
            }
            break;

        case TraceLogRetentionOption.LimitedCircularFiles:
            if (maximumNumberOfFiles < 2) {
                if (throwOnError) {
                    throw new ArgumentOutOfRangeException(s_optionMaximumNumberOfFiles, SR.GetString(SR.ArgumentOutOfRange_NeedValidMaxNumFiles, 2));
                }
                this._maxNumberOfFiles = 2;
            }
            else {
                this._maxNumberOfFiles = maximumNumberOfFiles;
            }
            break;
        }
    }

    // This uses a machine resource, scoped by the fileName variable.
    [ResourceExposure(ResourceScope.None)]
    [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
    [System.Security.SecurityCritical]
    private bool EnsureWriter() {
        if (traceWriter == null) {
            if (String.IsNullOrEmpty(fileName)) 
                return false;

            // We could use Interlocked but this one time overhead is probably not a concern
            lock (m_lockObject) {
                if (traceWriter != null)
                    return true;

                // To support multiple appdomains/instances tracing to the same file,
                // we will try to open the given file for append but if we encounter 
                // IO errors, we will suffix the file name with a unique GUID value 
                // and try one more time
                string path = fileName;

                for (int i=0; i<_retryThreshold; i++) {
                    try {
                        Init();
                        traceWriter = new TraceWriter(path, _bufferSize, _retention, _maxFileSize, _maxNumberOfFiles);
                        break;
                    }
                    catch (IOException ) { 
                        // Should we do this only for ERROR_SHARING_VIOLATION?
                        //if (UnsafeNativeMethods.MakeErrorCodeFromHR(Marshal.GetHRForException(ioexc)) != InternalResources.ERROR_SHARING_VIOLATION) break;

                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                        string fileExt = Path.GetExtension(fileName);

                        path = fileNameWithoutExt + Guid.NewGuid().ToString() + fileExt;
                        continue;
                    }
                    catch (UnauthorizedAccessException ) { 
                        //ERROR_ACCESS_DENIED, mostly ACL issues
                        break;
                    }
                    catch (Exception ) {
                        break;
                    }
                }

                // Disable tracing to this listener. Every Write will be nop.
                // We need to think of a central way to deal with the listener
                // init errors in the future. The default should be that we eat 
                // up any errors from listener and optionally notify the user
                if (traceWriter == null) 
                    fileName = null;
            }
        }
        return traceWriter != null;
    }

    private sealed class TraceWriter : TextWriter
    {
        Encoding encNoBOMwithFallback;
        Stream stream;

        private object m_lockObject = new object();

        [System.Security.SecurityCritical]
        internal TraceWriter(string _fileName, int bufferSize, TraceLogRetentionOption retention, long maxFileSize, int maxNumberOfFiles): base(CultureInfo.InvariantCulture) {
            stream = new LogStream(_fileName, bufferSize, (LogRetentionOption)retention, maxFileSize, maxNumberOfFiles);
        }
    
        // This is defined in TWTL as well, we should look into refactoring/relayering at a later time
        private static Encoding GetEncodingWithFallback(Encoding encoding)
        {
            // Clone it and set the "?" replacement fallback
            Encoding fallbackEncoding = (Encoding)encoding.Clone();
            fallbackEncoding.EncoderFallback = EncoderFallback.ReplacementFallback;
            fallbackEncoding.DecoderFallback = DecoderFallback.ReplacementFallback;
        
            return fallbackEncoding;
        }

        public override Encoding Encoding {
            get 
            { 
                if (encNoBOMwithFallback == null) {  
                    lock (m_lockObject) {
                        if (encNoBOMwithFallback == null) {  
                            // It is bad for tracing APIs to throw on encoding errors. Instead, we should 
                            // provide a "?" replacement fallback encoding to substitute illegal chars. 
                            // For ex, In case of high surrogate character D800-DBFF without a following 
                            // low surrogate character DC00-DFFF. We also need to use an encoding that 
                            // does't emit BOM whics is StreamWriter's default (for compatibility)
                            encNoBOMwithFallback = GetEncodingWithFallback(new UTF8Encoding(false));
                        }
                    }
                }

                return encNoBOMwithFallback;  
            }
        }

        public override void Write(String value) {
            try {
                byte[] buffer = Encoding.GetBytes(value);
                stream.Write(buffer, 0, buffer.Length);
            } 
            catch (Exception) {
                
                Debug.Assert(false, "UnExpected exc! Possible encoding error or failure on write! DATA LOSS!!!");  
                
                if (stream is BufferedStream2) {
                    ((BufferedStream2)stream).DiscardBuffer();
                }
            }

        }

        public override void Flush() {
            stream.Flush();    
        }
        
        protected override void Dispose(bool disposing) {
            try {
                if (disposing) 
                    stream.Close();  
            }
            finally {
                base.Dispose(disposing); // Essentially no-op
            }
        }
    }
}

}
