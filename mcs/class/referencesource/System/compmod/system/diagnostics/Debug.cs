//------------------------------------------------------------------------------
// <copyright file="Debug.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
#define DEBUG
namespace System.Diagnostics {
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.Security.Permissions;
    using System.Globalization;

    /// <devdoc>
    ///    <para>Provides a set of properties and
    ///       methods
    ///       for debugging code.</para>
    /// </devdoc>
    public static class Debug { 

#if !SILVERLIGHT
        /// <devdoc>
        ///    <para>Gets
        ///       the collection of listeners that is monitoring the debug
        ///       output.</para>
        /// </devdoc>
        public static TraceListenerCollection Listeners { 
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            [HostProtection(SharedState=true)]
            get {
                return TraceInternal.Listeners;
            }
        }          

        /// <devdoc>
        /// <para>Gets or sets a value indicating whether <see cref='System.Diagnostics.Debug.Flush'/> should be called on the
        /// <see cref='System.Diagnostics.Debug.Listeners'/>
        /// after every write.</para>
        /// </devdoc>
        public static bool AutoFlush { 
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get {
                return TraceInternal.AutoFlush;
            }

            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            set {
                TraceInternal.AutoFlush = value;
            }
        }
        
        /// <devdoc>
        ///    <para>Gets or sets
        ///       the indent level.</para>
        /// </devdoc>
        public static int IndentLevel {
            get { return TraceInternal.IndentLevel; }

            set { TraceInternal.IndentLevel = value; }
        }

        /// <devdoc>
        ///    <para>Gets or sets the number of spaces in an indent.</para>
        /// </devdoc>
        public static int IndentSize {
            get { return TraceInternal.IndentSize; }
            
            set { TraceInternal.IndentSize = value; }
        }        
        
        /// <devdoc>
        ///    <para>Clears the output buffer, and causes buffered data to
        ///       be written to the <see cref='System.Diagnostics.Debug.Listeners'/>.</para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]          
        public static void Flush() {
            TraceInternal.Flush();
        }

        /// <devdoc>
        ///    <para>Clears the output buffer, and then closes the <see cref='System.Diagnostics.Debug.Listeners'/> so that they no longer receive
        ///       debugging output.</para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]
        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static void Close() {
            TraceInternal.Close();
        }

        /// <devdoc>
        /// <para>Checks for a condition, and outputs the callstack if the condition is <see langword='false'/>.</para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]  
        public static void Assert(bool condition) {
            TraceInternal.Assert(condition);
        }

        /// <devdoc>
        ///    <para>Checks for a condition, and displays a message if the condition is
        ///    <see langword='false'/>. </para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]  
        public static void Assert(bool condition, string message) {
            TraceInternal.Assert(condition, message);
        }

        /// <devdoc>
        ///    <para>Checks for a condition, and displays both the specified messages if the condition
        ///       is <see langword='false'/>. </para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]  
        public static void Assert(bool condition, string message, string detailMessage) {
            TraceInternal.Assert(condition, message, detailMessage);
        }

        /// <devdoc>
        ///    <para>Checks for a condition, and displays both the specified messages if the condition
        ///       is <see langword='false'/>. </para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Assert(bool condition, string message, string detailMessageFormat, params Object[] args) {
            TraceInternal.Assert(condition, message, String.Format(CultureInfo.InvariantCulture, detailMessageFormat, args));
        }

        /// <devdoc>
        ///    <para>Emits or displays a message for an assertion that always fails.</para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]        
        public static void Fail(string message) {
            TraceInternal.Fail(message);
        }        

        /// <devdoc>
        ///    <para>Emits or displays both messages for an assertion that always fails.</para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]        
        public static void Fail(string message, string detailMessage) {
            TraceInternal.Fail(message, detailMessage);
        }        

        [System.Diagnostics.Conditional("DEBUG")]        
        public static void Print(string message) {
            TraceInternal.WriteLine(message);
        }
        
        [System.Diagnostics.Conditional("DEBUG")]        
        public static void Print(string format, params object[] args) {
            TraceInternal.WriteLine(String.Format(CultureInfo.InvariantCulture, format, args));
        }
        
        /// <devdoc>
        ///    <para>Writes a message to the trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection.</para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]        
        public static void Write(string message) {
            TraceInternal.Write(message);
        }

        /// <devdoc>
        ///    <para>Writes the name of the value 
        ///       parameter to the trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection.</para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Write(object value) {
            TraceInternal.Write(value);
        }

        /// <devdoc>
        ///    <para>Writes a category name and message 
        ///       to the trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection.</para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Write(string message, string category) {
            TraceInternal.Write(message, category);
        }

        /// <devdoc>
        ///    <para>Writes a category name and the name of the value parameter to the trace
        ///       listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection.</para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Write(object value, string category) {
            TraceInternal.Write(value, category);
        }

        /// <devdoc>
        ///    <para>Writes a message followed by a line terminator to the trace listeners in the
        ///    <see cref='System.Diagnostics.Debug.Listeners'/> collection. The default line terminator 
        ///       is a carriage return followed by a line feed (\r\n).</para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]        
        public static void WriteLine(string message) {
            TraceInternal.WriteLine(message);
        }

        /// <devdoc>
        ///    <para>Writes the name of the value 
        ///       parameter followed by a line terminator to the
        ///       trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection. The default line
        ///       terminator is a carriage return followed by a line feed (\r\n).</para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLine(object value) {
            TraceInternal.WriteLine(value);
        }

        /// <devdoc>
        ///    <para>Writes a category name and message followed by a line terminator to the trace
        ///       listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection. The default line
        ///       terminator is a carriage return followed by a line feed (\r\n).</para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLine(string message, string category) {
            TraceInternal.WriteLine(message, category);
        }

        /// <devdoc>
        ///    <para>Writes a category name and the name of the value 
        ///       parameter followed by a line
        ///       terminator to the trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection. The
        ///       default line terminator is a carriage return followed by a line feed (\r\n).</para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLine(object value, string category) {
            TraceInternal.WriteLine(value, category);
        }

        /// <devdoc>
        ///    <para>Writes a category name and the name of the value 
        ///       parameter followed by a line
        ///       terminator to the trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection. The
        ///       default line terminator is a carriage return followed by a line feed (\r\n).</para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLine(string format, params object[] args) {
            TraceInternal.WriteLine(String.Format(CultureInfo.InvariantCulture, format, args));
        }

        /// <devdoc>
        /// <para>Writes a message to the trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection 
        ///    if a condition is
        /// <see langword='true'/>. </para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]        
        public static void WriteIf(bool condition, string message) {
            TraceInternal.WriteIf(condition, message);
        }

        /// <devdoc>
        ///    <para>Writes the name of the value 
        ///       parameter to the trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/>
        ///       collection if a condition is
        ///    <see langword='true'/>. </para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteIf(bool condition, object value) {
            TraceInternal.WriteIf(condition, value);
        }

        /// <devdoc>
        ///    <para>Writes a category name and message 
        ///       to the trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/>
        ///       collection if a condition is
        ///    <see langword='true'/>. </para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteIf(bool condition, string message, string category) {
            TraceInternal.WriteIf(condition, message, category);
        }

        /// <devdoc>
        ///    <para>Writes a category name and the name of the value 
        ///       parameter to the trace
        ///       listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection if a condition is
        ///    <see langword='true'/>. </para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteIf(bool condition, object value, string category) {
            TraceInternal.WriteIf(condition, value, category);
        }

        /// <devdoc>
        ///    <para>Writes a message followed by a line terminator to the trace listeners in the
        ///    <see cref='System.Diagnostics.Debug.Listeners'/> collection if a condition is 
        ///    <see langword='true'/>. The default line terminator is a carriage return followed 
        ///       by a line feed (\r\n).</para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]        
        public static void WriteLineIf(bool condition, string message) {
            TraceInternal.WriteLineIf(condition, message);
        }

        /// <devdoc>
        ///    <para>Writes the name of the value 
        ///       parameter followed by a line terminator to the
        ///       trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection if a condition is
        ///    <see langword='true'/>. The default line terminator is a carriage return followed 
        ///       by a line feed (\r\n).</para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLineIf(bool condition, object value) {
            TraceInternal.WriteLineIf(condition, value);
        }

        /// <devdoc>
        ///    <para>Writes a category name and message
        ///       followed by a line terminator to the trace
        ///       listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection if a condition is
        ///    <see langword='true'/>. The default line terminator is a carriage return followed 
        ///       by a line feed (\r\n).</para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLineIf(bool condition, string message, string category) {
            TraceInternal.WriteLineIf(condition, message, category);
        }
        
        /// <devdoc>
        ///    <para>Writes a category name and the name of the value parameter followed by a line
        ///       terminator to the trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection
        ///       if a condition is <see langword='true'/>. The default line terminator is a carriage
        ///       return followed by a line feed (\r\n).</para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLineIf(bool condition, object value, string category) {
            TraceInternal.WriteLineIf(condition, value, category);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Indent() {
            TraceInternal.Indent();
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Unindent() {
            TraceInternal.Unindent();
        }

#else
        static readonly object s_ForLock = new Object();

        // This is the number of characters that OutputDebugString chunks at.
        const int internalWriteSize = 4091;

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Assert(bool condition) {
            Assert(condition, String.Empty, String.Empty);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Assert(bool condition, string message) {
            Assert(condition, message, String.Empty);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Security.SecuritySafeCritical]
        public static void Assert(bool condition, string message, string detailMessage) {
            if (!condition) {
                StackTrace stack = new StackTrace(true);
                int userStackFrameIndex = 0;
                string stackTrace;

                try {
                    stackTrace = StackTraceToString(stack, userStackFrameIndex, stack.FrameCount - 1);
                } catch {
                    stackTrace = "";
                }

                WriteAssert(stackTrace, message, detailMessage);
                AssertWrapper.ShowAssert(stackTrace, stack.GetFrame(userStackFrameIndex), message, detailMessage);
            }
        }

        // Given a stack trace and start and end frame indexes, construct a
        // callstack that contains method, file and line number information.
        [System.Security.SecuritySafeCritical]
        private static string StackTraceToString(StackTrace trace, int startFrameIndex, int endFrameIndex) {
            StringBuilder sb = new StringBuilder(512);

            for (int i = startFrameIndex; i <= endFrameIndex; i++) {
                StackFrame frame = trace.GetFrame(i);
                MethodBase method = frame.GetMethod();
                sb.Append(Environment.NewLine);
                sb.Append("    at ");
                if (method.ReflectedType != null) {
                    sb.Append(method.ReflectedType.Name);
                } else {
                    // This is for global methods and this is what shows up in windbg. 
                    sb.Append("<Module>");
                }
                sb.Append(".");
                sb.Append(method.Name);
                sb.Append("(");
                ParameterInfo[] parameters = method.GetParameters();
                for (int j = 0; j < parameters.Length; j++) {
                    ParameterInfo parameter = parameters[j];
                    if (j > 0)
                        sb.Append(", ");
                    sb.Append(parameter.ParameterType.Name);
                    sb.Append(" ");
                    sb.Append(parameter.Name);
                }
                sb.Append(")  ");
                sb.Append(frame.GetFileName());
                int line = frame.GetFileLineNumber();
                if (line > 0) {
                    sb.Append("(");
                    sb.Append(line.ToString(CultureInfo.InvariantCulture));
                    sb.Append(")");
                }
            }
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }

        private static void WriteAssert(string stackTrace, string message, string detailMessage) {
            string assertMessage = SR.GetString(SR.DebugAssertBanner) + Environment.NewLine
                                            + SR.GetString(SR.DebugAssertShortMessage) + Environment.NewLine
                                            + message + Environment.NewLine
                                            + SR.GetString(SR.DebugAssertLongMessage) + Environment.NewLine +
                                            detailMessage + Environment.NewLine
                                            + stackTrace;
            WriteLine(assertMessage);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Assert(bool condition, string message, string detailMessageFormat, params object[] args) {
            Assert(condition, message, String.Format(detailMessageFormat, args));
        }

        [System.Diagnostics.Conditional("DEBUG")]       
        public static void WriteLine(string message) {
            message = message + "\r\n"; // Use Windows end line on *all* Platforms

            // We don't want output from multiple threads to be interleaved.
            lock (s_ForLock) {
                // really huge messages mess up both VS and dbmon, so we chop it up into 
                // reasonable chunks if it's too big
                if (message == null || message.Length <= internalWriteSize) {
                    internalWrite(message);
                } else {
                    int offset;
                    for (offset = 0; offset < message.Length - internalWriteSize; offset += internalWriteSize) {
                        internalWrite(message.Substring(offset, internalWriteSize));
                    }
                    internalWrite(message.Substring(offset));
                }
            }

        }

        [System.Security.SecuritySafeCritical]
        private static void internalWrite(string message) {
            if (Debugger.IsLogging()) {
                Debugger.Log(0, null, message);
#if !FEATURE_PAL
            } else {
                if (message == null)
                    Microsoft.Win32.SafeNativeMethods.OutputDebugString(String.Empty);
                else
                    Microsoft.Win32.SafeNativeMethods.OutputDebugString(message);
#endif //!FEATURE_PAL
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLine(object value) {
            WriteLine((value == null) ? String.Empty : value.ToString());
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLine(string format, params object[] args) {
            WriteLine(String.Format(null, format, args));
        }

#if FEATURE_NETCORE
        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLineIf(bool condition, string message)
        {
            if(condition)
            {
                WriteLine(message);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        // This is used by our compression code.
        internal static void WriteLineIf(bool condition, string message, string category)
        {
            if (condition)
            {
                WriteLine(message);
            }
        }
#endif // FEATURE_NETCORE

#endif // SILVERLIGHT

    }
}
