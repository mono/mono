// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  StringWriter
** 
** <OWNER>[....]</OWNER>
**
** Purpose: For writing text to a string
**
**
===========================================================*/

using System;
using System.Runtime;
using System.Text;
using System.Globalization;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Security.Permissions;
#if FEATURE_ASYNC_IO
using System.Threading.Tasks;
#endif

namespace System.IO {
    // This class implements a text writer that writes to a string buffer and allows
    // the resulting sequence of characters to be presented as a string.
    //
    [Serializable]
    [ComVisible(true)]
    public class StringWriter : TextWriter
    {
        private static volatile UnicodeEncoding m_encoding=null;

        private StringBuilder _sb;
        private bool _isOpen;

        // Constructs a new StringWriter. A new StringBuilder is automatically
        // created and associated with the new StringWriter.
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        public StringWriter() 
            : this(new StringBuilder(), CultureInfo.CurrentCulture)
        {
        }

#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        public StringWriter(IFormatProvider formatProvider) 
            : this(new StringBuilder(), formatProvider) {
        }
    
        // Constructs a new StringWriter that writes to the given StringBuilder.
        // 
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        public StringWriter(StringBuilder sb) : this(sb, CultureInfo.CurrentCulture) {
        }

        public StringWriter(StringBuilder sb, IFormatProvider formatProvider) : base(formatProvider) {
            if (sb==null)
                throw new ArgumentNullException("sb", Environment.GetResourceString("ArgumentNull_Buffer"));
            Contract.EndContractBlock();
            _sb = sb;
            _isOpen = true;
        }

#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        public override void Close()
        {
            Dispose(true);
        }

#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        protected override void Dispose(bool disposing)
        {
            // Do not destroy _sb, so that we can extract this after we are
            // done writing (similar to MemoryStream's GetBuffer & ToArray methods)
            _isOpen = false;
            base.Dispose(disposing);
        }


        public override Encoding Encoding {
#if !FEATURE_CORECLR
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
            get { 
                if (m_encoding==null) {
                    m_encoding = new UnicodeEncoding(false, false);
                }
                return m_encoding; 
            }
        }

        // Returns the underlying StringBuilder. This is either the StringBuilder
        // that was passed to the constructor, or the StringBuilder that was
        // automatically created.
        //
        public virtual StringBuilder GetStringBuilder() {
            return _sb;
        }
    
        // Writes a character to the underlying string buffer.
        //
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        public override void Write(char value) {
            if (!_isOpen)
                __Error.WriterClosed();
            _sb.Append(value);
        }
    
        // Writes a range of a character array to the underlying string buffer.
        // This method will write count characters of data into this
        // StringWriter from the buffer character array starting at position
        // index.
        //
        public override void Write(char[] buffer, int index, int count) {
            if (buffer==null)
                throw new ArgumentNullException("buffer", Environment.GetResourceString("ArgumentNull_Buffer"));
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            if (buffer.Length - index < count)
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            Contract.EndContractBlock();

            if (!_isOpen)
                __Error.WriterClosed();

            _sb.Append(buffer, index, count);
        }
    
        // Writes a string to the underlying string buffer. If the given string is
        // null, nothing is written.
        //
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        public override void Write(String value) {
            if (!_isOpen)
                __Error.WriterClosed();
            if (value != null) _sb.Append(value);
        }


#if FEATURE_ASYNC_IO
        #region Task based Async APIs
        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public override Task WriteAsync(char value)
        {
            Write(value);
            return Task.CompletedTask;
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public override Task WriteAsync(String value)
        {
            Write(value);
            return Task.CompletedTask;
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            Write(buffer, index, count);
            return Task.CompletedTask;
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public override Task WriteLineAsync(char value)
        {
            WriteLine(value);
            return Task.CompletedTask;
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public override Task WriteLineAsync(String value)
        {
            WriteLine(value);
            return Task.CompletedTask;
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public override Task WriteLineAsync(char[] buffer, int index, int count)
        {
            WriteLine(buffer, index, count);
            return Task.CompletedTask;
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public override Task FlushAsync()
        {
            return Task.CompletedTask;
        }
        #endregion
#endif //FEATURE_ASYNC_IO

        // Returns a string containing the characters written to this TextWriter
        // so far.
        //
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        public override String ToString() {
            return _sb.ToString();
        }
    }
}
