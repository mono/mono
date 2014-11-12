//------------------------------------------------------------------------------
// <copyright file="TextWriterTraceListener.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.Diagnostics {
    using System;
    using System.IO;
    using System.Text;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;
    using System.IO.Ports;
    using Microsoft.Win32;
    using System.Runtime.Versioning;

    /// <devdoc>
    ///    <para>Directs tracing or debugging output to
    ///       a <see cref='T:System.IO.TextWriter'/> or to a <see cref='T:System.IO.Stream'/>,
    ///       such as <see cref='F:System.Console.Out'/> or <see cref='T:System.IO.FileStream'/>.</para>
    /// </devdoc>
    [HostProtection(Synchronization=true)]
    public class TextWriterTraceListener : TraceListener {
        internal TextWriter writer;
        String fileName = null;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Diagnostics.TextWriterTraceListener'/> class with
        /// <see cref='System.IO.TextWriter'/> 
        /// as the output recipient.</para>
        /// </devdoc>
        public TextWriterTraceListener() {
        }
        
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Diagnostics.TextWriterTraceListener'/> class, using the 
        ///    stream as the recipient of the debugging and tracing output.</para>
        /// </devdoc>
        public TextWriterTraceListener(Stream stream) 
            : this(stream, string.Empty) {
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Diagnostics.TextWriterTraceListener'/> class with the 
        ///    specified name and using the stream as the recipient of the debugging and tracing output.</para>
        /// </devdoc>
        public TextWriterTraceListener(Stream stream, string name) 
            : base(name) {
            if (stream == null) throw new ArgumentNullException("stream");
            this.writer = new StreamWriter(stream);                        
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Diagnostics.TextWriterTraceListener'/> class using the 
        ///    specified writer as recipient of the tracing or debugging output.</para>
        /// </devdoc>
        public TextWriterTraceListener(TextWriter writer) 
            : this(writer, string.Empty) {
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Diagnostics.TextWriterTraceListener'/> class with the 
        ///    specified name and using the specified writer as recipient of the tracing or
        ///    debugging
        ///    output.</para>
        /// </devdoc>
        public TextWriterTraceListener(TextWriter writer, string name) 
            : base(name) {
            if (writer == null) throw new ArgumentNullException("writer");
            this.writer = writer;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        public TextWriterTraceListener(string fileName) {
            this.fileName = fileName;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        public TextWriterTraceListener(string fileName, string name) : base(name) {
            this.fileName = fileName;
        }

        /// <devdoc>
        ///    <para> Indicates the text writer that receives the tracing
        ///       or debugging output.</para>
        /// </devdoc>
        public TextWriter Writer {
            get {
                EnsureWriter();
                return writer;
            }

            set {
                writer = value;
            }
        }
        
        /// <devdoc>
        /// <para>Closes the <see cref='System.Diagnostics.TextWriterTraceListener.Writer'/> so that it no longer
        ///    receives tracing or debugging output.</para>
        /// </devdoc>
        public override void Close() {
            if (writer != null) {
                try {
                    writer.Close();
                } catch (ObjectDisposedException) { }
            }

            writer = null;
        }

        /// <internalonly/>
        /// <devdoc>        
        /// </devdoc>
        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                    this.Close();
                }
                else {
                    // clean up resources
                    if (writer != null)
                        try {
                            writer.Close();
                        } catch (ObjectDisposedException) { }
                    writer = null;
                }
            }
            finally {
                base.Dispose(disposing);
            }
        }                

        /// <devdoc>
        /// <para>Flushes the output buffer for the <see cref='System.Diagnostics.TextWriterTraceListener.Writer'/>.</para>
        /// </devdoc>
        public override void Flush() {
            if (!EnsureWriter()) return;
            try {
                writer.Flush();
            } catch (ObjectDisposedException) { }
        }

        /// <devdoc>
        ///    <para>Writes a message 
        ///       to this instance's <see cref='System.Diagnostics.TextWriterTraceListener.Writer'/>.</para>
        /// </devdoc>
        public override void Write(string message) {
            if (!EnsureWriter()) return;   
            if (NeedIndent) WriteIndent();
            try {
                writer.Write(message);
            } catch (ObjectDisposedException) { }
        }

        /// <devdoc>
        ///    <para>Writes a message 
        ///       to this instance's <see cref='System.Diagnostics.TextWriterTraceListener.Writer'/> followed by a line terminator. The
        ///       default line terminator is a carriage return followed by a line feed (\r\n).</para>
        /// </devdoc>
        public override void WriteLine(string message) {
            if (!EnsureWriter()) return;   
            if (NeedIndent) WriteIndent();
            try {
                writer.WriteLine(message);
                NeedIndent = true;
            } catch (ObjectDisposedException) { }
        }

        private static Encoding GetEncodingWithFallback(Encoding encoding)
        {
            // Clone it and set the "?" replacement fallback
            Encoding fallbackEncoding = (Encoding)encoding.Clone();
            fallbackEncoding.EncoderFallback = EncoderFallback.ReplacementFallback;
            fallbackEncoding.DecoderFallback = DecoderFallback.ReplacementFallback;

            return fallbackEncoding;
        }

        // This uses a machine resource, scoped by the fileName variable.
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        internal bool EnsureWriter() {
            bool ret = true;

            if (writer == null) {
                ret = false;
                
                if (fileName == null) 
                    return ret;

                // StreamWriter by default uses UTF8Encoding which will throw on invalid encoding errors.
                // This can cause the internal StreamWriter's state to be irrecoverable. It is bad for tracing 
                // APIs to throw on encoding errors. Instead, we should provide a "?" replacement fallback  
                // encoding to substitute illegal chars. For ex, In case of high surrogate character 
                // D800-DBFF without a following low surrogate character DC00-DFFF
                // NOTE: We also need to use an encoding that does't emit BOM whic is StreamWriter's default
                Encoding noBOMwithFallback = GetEncodingWithFallback(new UTF8Encoding(false));
                

                // To support multiple appdomains/instances tracing to the same file,
                // we will try to open the given file for append but if we encounter 
                // IO errors, we will prefix the file name with a unique GUID value 
                // and try one more time
                string fullPath = Path.GetFullPath(fileName);
                string dirPath = Path.GetDirectoryName(fullPath);
                string fileNameOnly = Path.GetFileName(fullPath);

                for (int i=0; i<2; i++) {
                    try {
                        writer = new StreamWriter(fullPath, true, noBOMwithFallback, 4096);
                        ret = true;
                        break;
                    }
                    catch (IOException ) { 

                        // Should we do this only for ERROR_SHARING_VIOLATION?
                        //if (InternalResources.MakeErrorCodeFromHR(Marshal.GetHRForException(ioexc)) == InternalResources.ERROR_SHARING_VIOLATION) {

                        fileNameOnly = Guid.NewGuid().ToString() + fileNameOnly;
                        fullPath = Path.Combine(dirPath, fileNameOnly);
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

                if (!ret) {
                    // Disable tracing to this listener. Every Write will be nop.
                    // We need to think of a central way to deal with the listener
                    // init errors in the future. The default should be that we eat 
                    // up any errors from listener and optionally notify the user
                    fileName = null;
                }
            }
            return ret;
        }
        
    }
}
