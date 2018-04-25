//------------------------------------------------------------------------------
// <copyright file="HttpException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Exception thrown by ASP.NET managed runtime
 * 
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web {
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Security;
    using System.Globalization;
    using System.CodeDom.Compiler;
    using System.Security.Permissions;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.Util;


    /// <devdoc>
    ///    <para> Enables ASP.NET
    ///       to send exception information.</para>
    /// </devdoc>
    [Serializable]
    public class HttpException : ExternalException {
        private const int FACILITY_WIN32 = 7;

        private int _httpCode;
        private ErrorFormatter _errorFormatter;
        private int _webEventCode = WebEventCodes.UndefinedEventCode;

        // N.B. The last error code can be lost if we were to 
        // call UnsafeNativeMethods.GetLastError from this function
        // and it were not yet jitted.
        internal static int HResultFromLastError(int lastError) {
            int hr;

            if (lastError < 0) {
                hr = lastError;
            }
            else {
                hr = (int)(((uint)lastError & 0x0000FFFF) | (FACILITY_WIN32 << 16) | 0x80000000);
            }

            return hr;
        }


        /// <devdoc>
        ///    <para>Creates a new Exception based on the previous Exception. </para>
        /// </devdoc>
        public static HttpException CreateFromLastError(String message) {
            return new HttpException(message, HResultFromLastError(Marshal.GetLastWin32Error()));
        }


        /// <devdoc>
        /// <para> Default constructor.</para>
        /// </devdoc>
        public HttpException() {}


        /// <devdoc>
        ///    <para>
        ///       Construct an exception using error message.
        ///    </para>
        /// </devdoc>
        public HttpException(String message)

        : base(message) {
        }

        internal HttpException(String message, Exception innerException, int code)
        
        : base(message, innerException) {
            _webEventCode = code;
        }
        

        /// <devdoc>
        ///    <para>Construct an exception using error message and hr.</para>
        /// </devdoc>
        public HttpException(String message, int hr)

        : base(message) {
            HResult = hr;
        }


        /// <devdoc>
        ///    <para>Construct an exception using error message, HTTP code, 
        ///       and innerException
        ///       .</para>
        /// </devdoc>
        public HttpException(String message, Exception innerException)

        : base(message, innerException) {
        }


        /// <devdoc>
        ///    <para>Construct an exception using HTTP error code, error message, 
        ///       and innerException
        ///       .</para>
        /// </devdoc>
        public HttpException(int httpCode, String message, Exception innerException)

        : base(message, innerException) {
            _httpCode = httpCode;
        }


        /// <devdoc>
        ///    <para>Construct an
        ///       exception using HTTP error code and error message.</para>
        /// </devdoc>
        public HttpException(int httpCode, String message)

        : base(message) {
            _httpCode = httpCode;
        }


        /// <devdoc>
        ///    <para> Construct an exception
        ///       using HTTP error code, error message, and hr.</para>
        /// </devdoc>
        public HttpException(int httpCode, String message, int hr)

        : base(message) {
            HResult = hr;
            _httpCode = httpCode;
        }


        /// <devdoc>
        ///    <para> Contructor used for derialization.</para>
        /// </devdoc>
        protected HttpException(SerializationInfo info, StreamingContext context)
           :base(info, context) {
           _httpCode = info.GetInt32("_httpCode");
        }


        /// <devdoc>
        ///    <para>Serialize the object.</para>
        /// </devdoc>
        //[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter=true)]
        [SuppressMessage("Microsoft.Security", "CA2110:SecureGetObjectDataOverrides", Justification = "Base class has demand")]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_httpCode", _httpCode);
        }

        /*
         * If we have an Http code (non-zero), return it.  Otherwise, return
         * the inner exception's code.  If there isn't one, return 500.
         */

        /// <devdoc>
        ///    <para>HTTP return code to send back to client. If there is a 
        ///       non-zero Http code, it is returned. Otherwise, the System.HttpException.innerException
        ///       code is returned. If
        ///       there isn't an inner exception, error code 500 is returned.</para>
        /// </devdoc>
        public int GetHttpCode() {
            return GetHttpCodeForException(this);
        }

        internal void SetFormatter(ErrorFormatter errorFormatter) {
            _errorFormatter = errorFormatter;
        }

        internal static int GetHttpCodeForException(Exception e) {

            if (e is HttpException) {
                HttpException he = (HttpException)e;

                // If the HttpException specifies an HTTP code, use it
                if (he._httpCode > 0)
                    return he._httpCode;
            }
/*
404 conversion is done in HttpAplpication.MapHttpHandler

            else if (e is FileNotFoundException || e is DirectoryNotFoundException)
            {
                code = 404;
            }
*/
            else if (e is UnauthorizedAccessException) {
                return 401;
            }
            else if (e is PathTooLongException) {
                return 414;
            }

            // If there is an inner exception, try to get the code from it
            if (e.InnerException != null)
                return GetHttpCodeForException(e.InnerException);

            // If all else fails, use 500
            return 500;
        }

        /*
         * Return the formatter associated with this exception
         */
        internal static ErrorFormatter GetErrorFormatter(Exception e) {
            Exception inner = e.InnerException;
            ErrorFormatter nestedFormatter = null;

            // First, see if the inner exception has a formatter
            if (inner != null) {
                nestedFormatter = GetErrorFormatter(inner);
                if (nestedFormatter != null)
                    return nestedFormatter;

                if (inner is ConfigurationException)
                {
                    ConfigurationException ce = inner as ConfigurationException;
                    if (ce != null && ce.Filename != null)
                        nestedFormatter = new ConfigErrorFormatter((ConfigurationException)inner);
                }
                else if (inner is SecurityException)
                    nestedFormatter = new SecurityErrorFormatter(inner);
            }

            // If it does, return it rather than our own
            if (nestedFormatter != null)
                return nestedFormatter;

            HttpException httpExc = e as HttpException;
            if (httpExc != null)
                return httpExc._errorFormatter;

            return null;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string GetHtmlErrorMessage() {
            ErrorFormatter formatter = GetErrorFormatter(this);
            if (formatter == null) return null;
            return formatter.GetHtmlErrorMessage();
        }

        public int WebEventCode {
            get { return _webEventCode; }
            internal set { _webEventCode = value; }
        }
    }


    /// <devdoc>
    ///    <para> Exception thrown when a generic error occurs.</para>
    /// </devdoc>
    [Serializable]
    public sealed class HttpUnhandledException : HttpException {


        public HttpUnhandledException() {}


        public HttpUnhandledException(String message)

        : base(message) {
        }


        public HttpUnhandledException(string message, Exception innerException)
        : base(message, innerException) {
           
            SetFormatter(new UnhandledErrorFormatter(innerException, message, null));
        }


        /// <internalonly/>
        internal HttpUnhandledException(string message, string postMessage, Exception innerException)
        : base(message, innerException) {
           
            SetFormatter(new UnhandledErrorFormatter(innerException, message, postMessage));
        }

        private HttpUnhandledException(SerializationInfo info, StreamingContext context)
           :base(info, context) {
        }
    }


    /// <devdoc>
    ///    <para> Exception thrown when a compilation error occurs.</para>
    /// </devdoc>
    [Serializable]
    public sealed class HttpCompileException : HttpException {

        private CompilerResults _results;
        private string _sourceCode;


        public HttpCompileException() {
        }


        public HttpCompileException(string message) : base(message) {
        }


        public HttpCompileException(String message, Exception innerException) : base(message, innerException) {
        }


        public HttpCompileException(CompilerResults results, string sourceCode) {
            _results = results;
            _sourceCode = sourceCode;

            SetFormatter(new DynamicCompileErrorFormatter(this));
        }

        private HttpCompileException(SerializationInfo info, StreamingContext context)
           :base(info, context) {
           _results = (CompilerResults) info.GetValue("_results", typeof(CompilerResults));
           _sourceCode = info.GetString("_sourceCode");
        }

        // Determines whether the compile exception should be cached
        private bool _dontCache;
        internal bool DontCache {
            get { return _dontCache; }
            set { _dontCache = value; }
        }

        // The virtualpath depdencies for current buildresult.
        private ICollection _virtualPathDependencies;
        internal ICollection VirtualPathDependencies {
            get { return _virtualPathDependencies; }
            set { _virtualPathDependencies = value; }
        }


        /// <devdoc>
        ///    <para>Serialize the object.</para>
        /// </devdoc>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_results", _results);
            info.AddValue("_sourceCode", _sourceCode);
        }

        private const string compileErrorFormat = "{0}({1}): error {2}: {3}";


        /// <devdoc>
        ///    <para> The first compilation error.</para>
        /// </devdoc>
        public override string Message {
            get {
                // Return the first compile error as the exception message
                CompilerError e = FirstCompileError;

                if (e == null)
                    return base.Message;

                string message = String.Format(CultureInfo.CurrentCulture, compileErrorFormat,
                    e.FileName, e.Line, e.ErrorNumber, e.ErrorText);
                
                return message;
            }
        }


        /// <devdoc>
        ///    <para> The CompilerResults object describing the compile error.</para>
        /// </devdoc>
        public CompilerResults Results {
            [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.High)]
            get { 
                return _results;
            } 
        }

        internal CompilerResults ResultsWithoutDemand {
            get {
                return _results;
            }
        }

        /// <devdoc>
        ///    <para> The source code that was compiled.</para>
        /// </devdoc>
        public string SourceCode {
            [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.High)]
            get { 
                return _sourceCode; 
            }
        }

        internal string SourceCodeWithoutDemand {
            get {
                return _sourceCode;
            }
        }

        // Return the first compile error, or null if there isn't one
        internal CompilerError FirstCompileError {
            get {
                if (_results == null || !_results.Errors.HasErrors)
                    return null;

                CompilerError e = null;

                foreach (CompilerError error in _results.Errors) {
                    
                    // Ignore warnings
                    if (error.IsWarning) continue;

                    // If we found an error that's not in the generated code, use it
                    if (HttpRuntime.CodegenDirInternal != null && error.FileName != null &&
                        !StringUtil.StringStartsWith(error.FileName, HttpRuntime.CodegenDirInternal)) {
                        e = error;
                        break;
                    }

                    // The current error is in the generated code.  Keep track of
                    // it if it's the first one, but keep on looking in case we find another
                    // one that's not in the generated code (ASURT 62600)
                    if (e == null)
                        e = error;
                }

                return e;
            }
        }
    }


    /// <devdoc>
    ///    <para> Exception thrown when a parse error occurs.</para>
    /// </devdoc>
    [Serializable]
    public sealed class HttpParseException : HttpException {

        private VirtualPath _virtualPath;
        private int _line;
        private ParserErrorCollection _parserErrors;


        public HttpParseException() {
        }


        public HttpParseException(string message) : base(message) {
        }


        public HttpParseException(String message, Exception innerException) : base(message, innerException) {
        }


        public HttpParseException(string message, Exception innerException, string virtualPath,
            string sourceCode, int line) : this(message, innerException,
                System.Web.VirtualPath.CreateAllowNull(virtualPath), sourceCode, line) {}

        internal HttpParseException(string message, Exception innerException, VirtualPath virtualPath,
            string sourceCode, int line)
            : base(message, innerException) {

            _virtualPath = virtualPath;
            _line = line;

            string formatterMessage;
            if (innerException != null)
                formatterMessage = innerException.Message;
            else
                formatterMessage = message;

            SetFormatter(new ParseErrorFormatter(this, System.Web.VirtualPath.GetVirtualPathString(virtualPath), sourceCode,
                line, formatterMessage));
        }

        private HttpParseException(SerializationInfo info, StreamingContext context)
           :base(info, context) {
            _virtualPath = (VirtualPath)info.GetValue("_virtualPath", typeof(VirtualPath));
           _line = info.GetInt32("_line");
           _parserErrors = (ParserErrorCollection)info.GetValue("_parserErrors", typeof(ParserErrorCollection));
       }


        /// <devdoc>
        ///    <para>Serialize the object.</para>
        /// </devdoc>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_virtualPath", _virtualPath);
            info.AddValue("_line", _line);
            info.AddValue("_parserErrors", _parserErrors);
        }


        /// <devdoc>
        ///    <para> The physical path to source file that has the error.</para>
        /// </devdoc>
        public string FileName {
            get {
                string physicalPath = _virtualPath.MapPathInternal();

                if (physicalPath == null)
                    return null;

                // Demand path discovery before returning the path (ASURT 123798)
                InternalSecurityPermissions.PathDiscovery(physicalPath).Demand();
                return physicalPath;
            } 
        }


        /// <devdoc>
        ///    <para> The virtual path to source file that has the error.</para>
        /// </devdoc>
        public string VirtualPath {
            get {
                return System.Web.VirtualPath.GetVirtualPathString(_virtualPath);
            } 
        }

        internal VirtualPath VirtualPathObject {
            get {
                return _virtualPath;
            }
        }

        /// <devdoc>
        ///    <para> The CompilerResults object describing the compile error.</para>
        /// </devdoc>
        public int Line {
            get { return _line;} 
        }

        // The set of parser errors
        public ParserErrorCollection ParserErrors {
            get {
                if (_parserErrors == null) {
                    _parserErrors = new ParserErrorCollection();
                    ParserError thisError = new ParserError(Message, _virtualPath, _line);
                    _parserErrors.Add(thisError);
                }

                return _parserErrors;
            }
        }
    }


    /// <devdoc>
    ///    <para> Exception thrown when a potentially unsafe input string is detected (ASURT 122278)</para>
    /// </devdoc>
    [Serializable]
    public sealed class HttpRequestValidationException : HttpException {


        public HttpRequestValidationException() {
        }


        public HttpRequestValidationException(string message) : base(message) {

            SetFormatter(new UnhandledErrorFormatter(
                this, SR.GetString(SR.Dangerous_input_detected_descr), null));
        }


        public HttpRequestValidationException(String message, Exception innerException) : base(message, innerException) {
        }

        private HttpRequestValidationException(SerializationInfo info, StreamingContext context)
           :base(info, context) {
        }
    }

    [Serializable]
    public sealed class ParserError {
        private int _line;
        private VirtualPath _virtualPath;
        private string _errorText;
        private Exception _exception;

        public ParserError() {
        }

        public ParserError(string errorText, string virtualPath, int line)
            : this(errorText, System.Web.VirtualPath.CreateAllowNull(virtualPath), line) { 
        }

        internal ParserError(string errorText, VirtualPath virtualPath, int line) {
            _virtualPath = virtualPath;
            _line = line;
            _errorText = errorText;
        }

        // The original exception that introduces the Parser Error
        internal Exception Exception {
            get { return _exception; }
            set { _exception = value; }
        }

        // The virtualPath where the parser error occurs.
        public string VirtualPath {
            get { return System.Web.VirtualPath.GetVirtualPathString(_virtualPath); }
            set { _virtualPath = System.Web.VirtualPath.Create(value); }
        }

        // The description error text of the error.
        public string ErrorText {
            get { return _errorText; }
            set { _errorText = value; }
        }

        // The line where the parser error occurs.
        public int Line {
            get { return _line; }
            set { _line = value; }
        }
    }

    [Serializable]
    public sealed class ParserErrorCollection : CollectionBase {
        public ParserErrorCollection() {
        }

        public ParserErrorCollection(ParserError[] value) {
            this.AddRange(value);
        }

        public ParserError this[int index] {
            get { return ((ParserError)List[index]); }
            set { List[index] = value; }
        }

        public int Add(ParserError value) {
            return List.Add(value);
        }

        public void AddRange(ParserError[] value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            for (int i = 0; i < value.Length; i++) {
                this.Add(value[i]);
            }
        }

        public void AddRange(ParserErrorCollection value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            foreach(ParserError parserError in value) {
                this.Add(parserError);
            }
        }

        public bool Contains(ParserError value) {
            return List.Contains(value);
        }

        public void CopyTo(ParserError[] array, int index) {
            List.CopyTo(array, index);
        }

        public int IndexOf(ParserError value) {
            return List.IndexOf(value);
        }

        public void Insert(int index, ParserError value) {
            List.Insert(index, value);
        }

        public void Remove(ParserError value) {
            List.Remove(value);
        }
    }
}
