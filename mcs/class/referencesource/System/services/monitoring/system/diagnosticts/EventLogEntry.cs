//------------------------------------------------------------------------------
// <copyright file="EventLogEntry.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Diagnostics {
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ComponentModel;
    using System.Diagnostics;
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;
    using System.Security.Permissions;
    using System.IO;
    using System.Globalization;
    using System.Runtime.Versioning;

    /// <devdoc>
    ///    <para>
    ///    <see cref='System.Diagnostics.EventLogEntry'/>
    ///    encapsulates a single record in the NT event log.
    /// </para>
    /// </devdoc>
    [
    ToolboxItem(false),
    DesignTimeVisible(false),
    Serializable,
    ]
    public sealed class EventLogEntry : Component, ISerializable {
        internal byte[] dataBuf;
        internal int bufOffset;
        private EventLogInternal owner;
        private string category;
        private string message;

        // make sure only others in this package can create us
        internal EventLogEntry(byte[] buf, int offset, EventLogInternal log) {
            this.dataBuf = buf;
            this.bufOffset = offset;
            this.owner = log;

            GC.SuppressFinalize(this);
        }

        /// <devdoc>
        /// </devdoc>
        /// <internalonly/>
        private EventLogEntry(SerializationInfo info, StreamingContext context) {
            dataBuf = (byte[])info.GetValue("DataBuffer", typeof(byte[]));
            string logName = info.GetString("LogName");
            string machineName = info.GetString("MachineName");
            owner = new EventLogInternal(logName, machineName, "");
            GC.SuppressFinalize(this);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the name of the computer on which this entry was generated.
        ///
        ///    </para>
        /// </devdoc>
        [MonitoringDescription(SR.LogEntryMachineName)]
        public string MachineName {
            get {
                // first skip over the source name
                int pos = bufOffset + FieldOffsets.RAWDATA;
                while (CharFrom(dataBuf, pos) != '\0')
                    pos += 2;
                pos += 2;
                char ch = CharFrom(dataBuf, pos);
                StringBuilder buf = new StringBuilder();
                while (ch != '\0') {
                    buf.Append(ch);
                    pos += 2;
                    ch = CharFrom(dataBuf, pos);
                }
                return buf.ToString();
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the binary data associated with the entry.
        ///
        ///    </para>
        /// </devdoc>
        [
        MonitoringDescription(SR.LogEntryData)
        ]
        public byte[] Data {
            get {
                int dataLen = IntFrom(dataBuf, bufOffset + FieldOffsets.DATALENGTH);
                byte[] data = new byte[dataLen];
                Array.Copy(dataBuf, bufOffset + IntFrom(dataBuf, bufOffset + FieldOffsets.DATAOFFSET),
                           data, 0, dataLen);
                return data;
            }
        }

        /*
        /// <summary>
        ///    <para>
        ///       Copies the binary data in the <see cref='System.Diagnostics.EventLogEntry.Data'/> member into an
        ///       array.
        ///    </para>
        /// </summary>
        /// <returns>
        ///    <para>
        ///       An array of type <see cref='System.Byte'/>.
        ///    </para>
        /// </returns>
        /// <keyword term=''/>
        public Byte[] getDataBytes() {
            Byte[] data = new Byte[rec.dataLength];
            for (int i = 0; i < data.Length; i++)
                data[i] = new Byte(rec.buf[i]);
            return data;
        }
        */

        /// <devdoc>
        ///    <para>
        ///       Gets the index of this entry in the event
        ///       log.
        ///    </para>
        /// </devdoc>
        [MonitoringDescription(SR.LogEntryIndex)]
        public int Index {
            get {
                return IntFrom(dataBuf, bufOffset + FieldOffsets.RECORDNUMBER);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the text associated with the <see cref='System.Diagnostics.EventLogEntry.CategoryNumber'/> for this entry.
        ///
        ///    </para>
        /// </devdoc>
        [MonitoringDescription(SR.LogEntryCategory)]
        public string Category {
            get {
                if (category == null) {
                    string dllName = GetMessageLibraryNames("CategoryMessageFile");
                    string cat = owner.FormatMessageWrapper(dllName, (uint) CategoryNumber, null);
                    if (cat == null)
                        category = "(" + CategoryNumber.ToString(CultureInfo.CurrentCulture) + ")";
                    else
                        category = cat;
                }

                return category;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the application-specific category number for this entry
        ///
        ///    </para>
        /// </devdoc>
        [
        MonitoringDescription(SR.LogEntryCategoryNumber)
        ]
        public short CategoryNumber {
            get {
                return ShortFrom(dataBuf, bufOffset + FieldOffsets.EVENTCATEGORY);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the application-specific event indentifier of this entry.
        ///
        ///    </para>
        /// </devdoc>
        [
        MonitoringDescription(SR.LogEntryEventID),
        Obsolete("This property has been deprecated.  Please use System.Diagnostics.EventLogEntry.InstanceId instead.  http://go.microsoft.com/fwlink/?linkid=14202")
        ]
        public int EventID {
            get {
                // Apparently the top 2 bits of this number are not
                // always 0. Strip them so the number looks nice to the user.
                // The problem is, if the user were to want to call FormatMessage(),
                // they'd need these two bits.
                return IntFrom(dataBuf, bufOffset + FieldOffsets.EVENTID) & 0x3FFFFFFF;
            }
        }

        /// <devdoc>
        ///    <para>
        ///
        ///       Gets the type
        ///       of this entry.
        ///
        ///    </para>
        /// </devdoc>
        [MonitoringDescription(SR.LogEntryEntryType)]
        public EventLogEntryType EntryType {
            get {
                return(EventLogEntryType) ShortFrom(dataBuf, bufOffset + FieldOffsets.EVENTTYPE);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the localized message corresponding to this event entry.
        ///
        ///    </para>
        /// </devdoc>
        [
        MonitoringDescription(SR.LogEntryMessage),
        Editor("System.ComponentModel.Design.BinaryEditor, " + AssemblyRef.SystemDesign, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing)
        ]
        public string Message {
            get {
                if (message == null) {
                    string dllNames = GetMessageLibraryNames("EventMessageFile");
                    int msgId =   IntFrom(dataBuf, bufOffset + FieldOffsets.EVENTID);
                    string msg = owner.FormatMessageWrapper(dllNames, (uint)msgId, ReplacementStrings);
                    if (msg == null) {
                        StringBuilder msgBuf = new StringBuilder(SR.GetString(SR.MessageNotFormatted, msgId, Source));
                        string[] strings = ReplacementStrings;
                        for (int i = 0; i < strings.Length; i++) {
                            if (i != 0)
                                msgBuf.Append(", ");
                            msgBuf.Append("'");
                            msgBuf.Append(strings[i]);
                            msgBuf.Append("'");
                        }
                        msg = msgBuf.ToString();
                    }
                    else
                        msg = ReplaceMessageParameters( msg, ReplacementStrings );
                    message = msg;
                }

                return message;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the name of the application that generated this event.
        ///    </para>
        /// </devdoc>
        [MonitoringDescription(SR.LogEntrySource)]
        public string Source {
            get {
                StringBuilder buf = new StringBuilder();
                int pos = bufOffset + FieldOffsets.RAWDATA;

                char ch = CharFrom(dataBuf, pos);
                while (ch != '\0') {
                    buf.Append(ch);
                    pos += 2;
                    ch = CharFrom(dataBuf, pos);
                }

                return buf.ToString();
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the replacement strings
        ///       associated with the entry.
        ///
        ///    </para>
        /// </devdoc>
        [MonitoringDescription(SR.LogEntryReplacementStrings)]
        public string[] ReplacementStrings {
            get {
                string[] strings = new string[ShortFrom(dataBuf, bufOffset + FieldOffsets.NUMSTRINGS)];
                int i = 0;
                int bufpos = bufOffset + IntFrom(dataBuf, bufOffset + FieldOffsets.STRINGOFFSET);
                StringBuilder buf = new StringBuilder();
                while (i < strings.Length) {
                    char ch = CharFrom(dataBuf, bufpos);
                    if (ch != '\0')
                        buf.Append(ch);
                    else {
                        strings[i] = buf.ToString();
                        i++;
                        buf = new StringBuilder();
                    }
                    bufpos += 2;
                }
                return strings;
            }
        }

        [
            MonitoringDescription(SR.LogEntryResourceId),
            ComVisible(false)
        ]
        public Int64 InstanceId {
            get {
                return (UInt32)IntFrom(dataBuf, bufOffset + FieldOffsets.EVENTID);
            }
        }

#if false
        internal string StringsBuffer {
            get {
                StringBuilder buf = new StringBuilder();
                int bufpos = bufOffset + IntFrom(dataBuf, bufOffset + FieldOffsets.STRINGOFFSET);
                int i = 0;
                int numStrings = ShortFrom(dataBuf, bufOffset + FieldOffsets.NUMSTRINGS);
                while (i < numStrings) {
                    char ch = CharFrom(dataBuf, bufpos);
                    buf.Append(ch);
                    bufpos += 2;
                    if (ch == '\0')
                        i++;
                }
                return buf.ToString();
            }
        }
#endif

        /// <devdoc>
        ///    <para>
        ///       Gets the time at which this event was generated, in local time.
        ///
        ///    </para>
        /// </devdoc>
        [MonitoringDescription(SR.LogEntryTimeGenerated)]
        public DateTime TimeGenerated {
            get {
                return beginningOfTime.AddSeconds(IntFrom(dataBuf, bufOffset + FieldOffsets.TIMEGENERATED)).ToLocalTime();
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       the time at which this event was written to the log, in local time.
        ///
        ///    </para>
        /// </devdoc>
        [MonitoringDescription(SR.LogEntryTimeWritten)]
        public DateTime TimeWritten {
            get {
                return beginningOfTime.AddSeconds(IntFrom(dataBuf, bufOffset + FieldOffsets.TIMEWRITTEN)).ToLocalTime();
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the name
        ///       of the user responsible for this event.
        ///    </para>
        /// </devdoc>
        [MonitoringDescription(SR.LogEntryUserName)]
        public string UserName {
            get {
                int sidLen = IntFrom(dataBuf, bufOffset + FieldOffsets.USERSIDLENGTH);
                if (sidLen == 0)
                    return null;
                byte[] sid = new byte[sidLen];                                 
                Array.Copy(dataBuf, bufOffset + IntFrom(dataBuf, bufOffset + FieldOffsets.USERSIDOFFSET),
                           sid, 0, sid.Length);
                
                int userNameLen = 256;
                int domainNameLen = 256;
                int sidNameUse = 0;
                StringBuilder bufUserName = new StringBuilder(userNameLen);
                StringBuilder bufDomainName = new StringBuilder(domainNameLen);
                
                StringBuilder retUserName = new StringBuilder();

                if(UnsafeNativeMethods.LookupAccountSid(MachineName, sid, bufUserName, ref userNameLen, bufDomainName, ref domainNameLen, ref sidNameUse) != 0) {
                    retUserName.Append(bufDomainName.ToString());
                    retUserName.Append("\\");
                    retUserName.Append(bufUserName.ToString());
                }
                
                return retUserName.ToString();
            }
        }

        private char CharFrom(byte[] buf, int offset) {
            return(char) ShortFrom(buf, offset);
        }

        /// <devdoc>
        ///    <para>
        ///       Performs a comparison between two event log entries.
        ///
        ///    </para>
        /// </devdoc>
        public bool Equals(EventLogEntry otherEntry) {
            if (otherEntry == null)
                return false;
            int ourLen = IntFrom(dataBuf, bufOffset + FieldOffsets.LENGTH);
            int theirLen = IntFrom(otherEntry.dataBuf, otherEntry.bufOffset + FieldOffsets.LENGTH);
            if (ourLen != theirLen) {
                return false;
            }
            int min = bufOffset;
            int max = bufOffset + ourLen;
            int j = otherEntry.bufOffset;
            for (int i = min; i < max; i++, j++)
                if (dataBuf[i] != otherEntry.dataBuf[j]) {
                    return false;
                }
            return true;
        }

        private int IntFrom(byte[] buf, int offset) {
            // assumes Little Endian byte order.
            return(unchecked((int)0xFF000000) & (buf[offset+3] << 24)) | (0xFF0000 & (buf[offset+2] << 16)) |
            (0xFF00 & (buf[offset+1] << 8)) | (0xFF & (buf[offset]));
        }

        // Replacing parameters '%n' in formated message using 'ParameterMessageFile' registry key.
        internal string ReplaceMessageParameters( String msg,  string[] insertionStrings )   {

            int percentIdx = msg.IndexOf('%');
            if ( percentIdx < 0 )
                return msg;     // no '%' at all

            int startCopyIdx     = 0;        // start idx of last orig msg chars to copy
            int msgLength   = msg.Length;
            StringBuilder buf = new StringBuilder();
            string paramDLLNames = GetMessageLibraryNames("ParameterMessageFile");

            while ( percentIdx >= 0 ) {
                string param = null;

                // Convert numeric string after '%' to paramMsgID number.
                int lasNumIdx =  percentIdx + 1;
                while ( lasNumIdx < msgLength && Char.IsDigit(msg, lasNumIdx) )
                    lasNumIdx++;

                uint paramMsgID = 0; 

                // If we can't parse it, leave the paramMsgID as zero.  We'll skip the replacement and just put
                // the %xxxx into the final message. 
                if (lasNumIdx != percentIdx + 1 ) 
                    UInt32.TryParse( msg.Substring(percentIdx + 1, lasNumIdx - percentIdx - 1), out paramMsgID);

                if ( paramMsgID != 0 )
                    param = owner.FormatMessageWrapper( paramDLLNames, paramMsgID, insertionStrings);

                if ( param != null ) {
                    if ( percentIdx > startCopyIdx )
                        buf.Append(msg, startCopyIdx, percentIdx - startCopyIdx);    // original chars from msg
                    buf.Append(param);
                    startCopyIdx = lasNumIdx;
                }

                percentIdx = msg.IndexOf('%', percentIdx + 1);
            }

            if ( msgLength - startCopyIdx > 0 )
                buf.Append(msg, startCopyIdx, msgLength - startCopyIdx);          // last span of original msg
            return buf.ToString();
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private static RegistryKey GetSourceRegKey(string logName, string source, string machineName) {
            RegistryKey eventKey = null;
            RegistryKey logKey   = null;

            try {
                eventKey = EventLog.GetEventLogRegKey(machineName, false);
                if (eventKey == null)
                    return null;
                if (logName == null)
                    logKey = eventKey.OpenSubKey("Application", /*writable*/false);
                else
                    logKey = eventKey.OpenSubKey(logName, /*writable*/false);
                if (logKey == null)
                    return null;
                return logKey.OpenSubKey(source, /*writable*/false);
            }
            finally {
                if (eventKey != null) eventKey.Close();
                if (logKey != null) logKey.Close();
            }

        }

        // ------------------------------------------------------------------------------
        // Returns DLL names list.
        // libRegKey can be: "EventMessageFile", "CategoryMessageFile", "ParameterMessageFile"
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private string GetMessageLibraryNames(string libRegKey ) {
            // get the value stored in the registry

            string fileName = null;
            RegistryKey regKey = null;
            try {
                regKey = GetSourceRegKey(owner.Log, Source, owner.MachineName);
                if (regKey != null) {
                    fileName = (string)regKey.GetValue(libRegKey);
                }
            }
            finally {
                if (regKey != null)
                    regKey.Close();
            }

            if (fileName == null)
                return null;

            // convert any absolute paths on a remote machine to use the \\MACHINENAME\DRIVELETTER$ shares
            // so we pick up message dlls from the remote machine.
            if (owner.MachineName != ".") {

                string[] fileNames = fileName.Split(';');

                StringBuilder result = new StringBuilder();

                for (int i = 0; i < fileNames.Length; i++) {
                    if (fileNames[i].Length >= 2 && fileNames[i][1] == ':') {
                        result.Append(@"\\");
                        result.Append(owner.MachineName);
                        result.Append(@"\");
                        result.Append(fileNames[i][0]);
                        result.Append("$");
                        result.Append(fileNames[i], 2, fileNames[i].Length - 2);
                        result.Append(';');
                    } 
                }

                if (result.Length == 0) {
                    return null;
                } else {
                    return result.ToString(0, result.Length - 1); // Chop of last ";"
                }
            }
            else {
                return fileName;
            }
        }


        private short ShortFrom(byte[] buf, int offset) {
            // assumes little Endian byte order.
            return(short) ((0xFF00 & (buf[offset+1] << 8)) | (0xFF & buf[offset]));
        }

        /// <internalonly/>
        /// <devdoc>
        /// <para>
        /// Saves an entry as a stream of data.
        /// </para>
        /// </devdoc>
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            int len = IntFrom(dataBuf, bufOffset + FieldOffsets.LENGTH);
            byte[] buf = new byte[len];
            Array.Copy(dataBuf, bufOffset, buf, 0, len);

            info.AddValue("DataBuffer", buf, typeof(byte[]));
            info.AddValue("LogName", owner.Log);
            info.AddValue("MachineName", owner.MachineName);
        }

        /// <devdoc>
        ///     Stores the offsets from the beginning of the record to fields within the record.
        /// </devdoc>
        private static class FieldOffsets {
            /** int */
            internal const int LENGTH = 0;
            /** int */
            internal const int RESERVED = 4;
            /** int */
            internal const int RECORDNUMBER = 8;
            /** int */
            internal const int TIMEGENERATED = 12;
            /** int */
            internal const int TIMEWRITTEN = 16;
            /** int */
            internal const int EVENTID = 20;
            /** short */
            internal const int EVENTTYPE = 24;
            /** short */
            internal const int NUMSTRINGS = 26;
            /** short */
            internal const int EVENTCATEGORY = 28;
            /** short */
            internal const int RESERVEDFLAGS = 30;
            /** int */
            internal const int CLOSINGRECORDNUMBER = 32;
            /** int */
            internal const int STRINGOFFSET = 36;
            /** int */
            internal const int USERSIDLENGTH = 40;
            /** int */
            internal const int USERSIDOFFSET = 44;
            /** int */
            internal const int DATALENGTH = 48;
            /** int */
            internal const int DATAOFFSET = 52;
            /** bytes */
            internal const int RAWDATA = 56;
        }

        // times in the struct are # seconds from midnight 1/1/1970.
        private static readonly DateTime beginningOfTime = new DateTime(1970, 1, 1, 0, 0, 0);

        // offsets in the struct are specified from the beginning, but we have to reference
        // them from the beginning of the array.  This is the size of the members before that.
        private const int OFFSETFIXUP = 4+4+4+4+4+4+2+2+2+2+4+4+4+4+4+4;

    }
}

