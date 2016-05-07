//------------------------------------------------------------------------------
// <copyright file="ConfigurationErrorsException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Configuration.Internal;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;
    using System.Runtime.Versioning;
    using System.Diagnostics.CodeAnalysis;

    [Serializable]
    public class ConfigurationErrorsException : ConfigurationException {

        // Constants
        private const string HTTP_PREFIX                     = "http:";
        private const string SERIALIZATION_PARAM_FILENAME    = "firstFilename";
        private const string SERIALIZATION_PARAM_LINE        = "firstLine";
        private const string SERIALIZATION_PARAM_ERROR_COUNT = "count";
        private const string SERIALIZATION_PARAM_ERROR_DATA  = "_errors";
        private const string SERIALIZATION_PARAM_ERROR_TYPE  = "_errors_type";

        private string                      _firstFilename; 
        private int                         _firstLine;     
        private ConfigurationException[]    _errors;

        void Init(string filename, int line) {
            HResult = HResults.Configuration;

            // BaseConfigurationRecord.cs uses -1 as uninitialized line number.
            if (line == -1) {
                line = 0;
            }
            
            _firstFilename = filename;
            _firstLine = line;
        }

        // The ConfigurationException class is obsolete, but we still need to derive from it and call the base ctor, so we
        // just disable the obsoletion warning.
#pragma warning disable 0618
        public ConfigurationErrorsException(string message, Exception inner, string filename, int line) : base(message, inner) {
#pragma warning restore 0618
            Init(filename, line);
        }

        public ConfigurationErrorsException() : 
                this(null, null, null, 0) {}

        public ConfigurationErrorsException(string message) : 
                this(message, null, null, 0) {}

        public ConfigurationErrorsException(string message, Exception inner) : 
                this(message, inner, null, 0) {}

        public ConfigurationErrorsException(string message, string filename, int line) :
                this(message, null, filename, line) {}

        public ConfigurationErrorsException(string message, XmlNode node) : 
                this(message, null, GetUnsafeFilename(node), GetLineNumber(node)) {}

        public ConfigurationErrorsException(string message, Exception inner, XmlNode node) : 
                this(message, inner, GetUnsafeFilename(node), GetLineNumber(node)) {}

        public ConfigurationErrorsException(string message, XmlReader reader) : 
                this(message, null, GetUnsafeFilename(reader), GetLineNumber(reader)) {}

        public ConfigurationErrorsException(string message, Exception inner, XmlReader reader) : 
                this(message, inner, GetUnsafeFilename(reader), GetLineNumber(reader)) {}

        internal ConfigurationErrorsException(string message, IConfigErrorInfo errorInfo) :
                this(message, null, GetUnsafeConfigErrorInfoFilename(errorInfo), GetConfigErrorInfoLineNumber(errorInfo)) {}

        internal ConfigurationErrorsException(string message, Exception inner, IConfigErrorInfo errorInfo) :
                this(message, inner, GetUnsafeConfigErrorInfoFilename(errorInfo), GetConfigErrorInfoLineNumber(errorInfo)) {}

        internal ConfigurationErrorsException(ConfigurationException e) :
                this(GetBareMessage(e), GetInnerException(e), GetUnsafeFilename(e), GetLineNumber(e)) {}


        [ResourceExposure(ResourceScope.None)]
        internal ConfigurationErrorsException(ICollection<ConfigurationException> coll) :
                this(GetFirstException(coll)) {

            if (coll.Count > 1) {
                _errors = new ConfigurationException[coll.Count];
                coll.CopyTo(_errors, 0);
            }
        }

        internal ConfigurationErrorsException(ArrayList coll) :
                this((ConfigurationException) (coll.Count > 0 ? coll[0] : null)) {

            if (coll.Count > 1) {
                _errors = new ConfigurationException[coll.Count];
                coll.CopyTo(_errors, 0);
                
                foreach (object error in _errors) {
                    // force an illegal typecast exception if the object is not
                    // of the right type
                    ConfigurationException exception = (ConfigurationException) error;
                }
            }
        }

        static private ConfigurationException GetFirstException(ICollection<ConfigurationException> coll) {
            foreach (ConfigurationException e in coll) {
                return e;
            }

            return null;
        }

        static private string GetBareMessage(ConfigurationException e) {
            if (e != null) {
                return e.BareMessage;
            }

            return null;
        }

        static private Exception GetInnerException(ConfigurationException e) {
            if (e != null) {
                return e.InnerException;
            }

            return null;
        }

        //
        // We assert PathDiscovery so that we get the full filename when calling ConfigurationException.Filename
        //
        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "Our Filename property getter demands the appropriate permissions.")]
        static private string GetUnsafeFilename(ConfigurationException e) {
            if (e != null) {
                return e.Filename;
            }

            return null;
        }

        static private int GetLineNumber(ConfigurationException e) {
            if (e != null) {
                return e.Line;
            }

            return 0;
        }


        // Serialization methods
        protected ConfigurationErrorsException(SerializationInfo info, StreamingContext context) : 
                base(info, context) { 

            string  firstFilename;
            int     firstLine;
            int     count;
            string  numPrefix;
            string  currentType;
            Type    currentExceptionType;

            // Retrieve out members
            firstFilename = info.GetString(SERIALIZATION_PARAM_FILENAME);
            firstLine     = info.GetInt32(SERIALIZATION_PARAM_LINE);
            
            Init(firstFilename, firstLine);

            // Retrieve errors for _errors object
            count = info.GetInt32(SERIALIZATION_PARAM_ERROR_COUNT);
            
            if (count != 0) {
                _errors = new ConfigurationException[count];
                
                for (int i = 0; i < count; i++) {
                    numPrefix = i.ToString(CultureInfo.InvariantCulture);

                    currentType = info.GetString(numPrefix + SERIALIZATION_PARAM_ERROR_TYPE);
                    currentExceptionType = Type.GetType(currentType, true);

                    // Only allow our exception types
                    if ( ( currentExceptionType != typeof( ConfigurationException ) ) &&
                         ( currentExceptionType != typeof( ConfigurationErrorsException ) ) ) {
                        throw ExceptionUtil.UnexpectedError( "ConfigurationErrorsException" );
                    }

                    _errors[i] = (ConfigurationException) 
                                    info.GetValue(numPrefix + SERIALIZATION_PARAM_ERROR_DATA,
                                                  currentExceptionType);
                }
            }
        }

        [SecurityPermissionAttribute(SecurityAction.Demand,SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            int    subErrors = 0;
            string numPrefix;

            // call base implementation
            base.GetObjectData(info, context);

            // Serialize our members
            info.AddValue(SERIALIZATION_PARAM_FILENAME, Filename);
            info.AddValue(SERIALIZATION_PARAM_LINE,     Line);

            // Serialize rest of errors, along with count
            // (since first error duplicates this error, only worry if
            //  there is more than one)
            if ((_errors        != null) &&
                (_errors.Length >  1   )){
                subErrors = _errors.Length;
                
                for (int i = 0; i < _errors.Length; i++) {
                    numPrefix = i.ToString(CultureInfo.InvariantCulture);

                    info.AddValue(numPrefix + SERIALIZATION_PARAM_ERROR_DATA, 
                                  _errors[i]);
                    info.AddValue(numPrefix + SERIALIZATION_PARAM_ERROR_TYPE, 
                                  _errors[i].GetType());
                }
            }    

            info.AddValue(SERIALIZATION_PARAM_ERROR_COUNT, subErrors);
        }

        // The message includes the file/line number information.  
        // To get the message without the extra information, use BareMessage.
        public override string Message {
            get {
                string file = Filename;
                if (!string.IsNullOrEmpty(file)) {
                    if (Line != 0) {
                        return BareMessage + " (" + file + " line " + Line.ToString(CultureInfo.CurrentCulture) + ")";
                    }
                    else {
                        return BareMessage + " (" + file + ")";
                    }
                }
                else if (Line != 0) {
                    return BareMessage + " (line " + Line.ToString("G", CultureInfo.CurrentCulture) + ")";
                }
                else {
                    return BareMessage;
                }
            }
        }

        public override string BareMessage {
            get {
                return base.BareMessage;
            }
        }

        public override string Filename {
            get {
                return SafeFilename(_firstFilename);
            }
        }

        public override int Line {
            get {
                return _firstLine;
            }
        }

        public ICollection Errors {
            get {
                if (_errors != null) {
                    return _errors;
                }
                else {
                    ConfigurationErrorsException e = new ConfigurationErrorsException(BareMessage, base.InnerException, _firstFilename, _firstLine);
                    ConfigurationException[] a = new ConfigurationException[] {e};
                    return a;
                }
            }
        }

        internal ICollection<ConfigurationException> ErrorsGeneric {
            get {
                return (ICollection<ConfigurationException>) this.Errors;
            }
        }

        // 
        // Get file and linenumber from an XML Node in a DOM
        //
        public static int GetLineNumber(XmlNode node) {
            return GetConfigErrorInfoLineNumber(node as IConfigErrorInfo);
        }

        public static string GetFilename(XmlNode node) {
            return SafeFilename(GetUnsafeFilename(node));
        }

        private static string GetUnsafeFilename(XmlNode node) {
            return GetUnsafeConfigErrorInfoFilename(node as IConfigErrorInfo);
        }

        // 
        // Get file and linenumber from an XML Reader
        //
        public static int GetLineNumber(XmlReader reader) {
            return GetConfigErrorInfoLineNumber(reader as IConfigErrorInfo);
        }

        public static string GetFilename(XmlReader reader) {
            return SafeFilename(GetUnsafeFilename(reader));
        }

        private static string GetUnsafeFilename(XmlReader reader) {
            return GetUnsafeConfigErrorInfoFilename(reader as IConfigErrorInfo);
        }

        // 
        // Get file and linenumber from an IConfigErrorInfo
        //
        private static int GetConfigErrorInfoLineNumber(IConfigErrorInfo errorInfo) {
            if (errorInfo != null) {
                return errorInfo.LineNumber;
            }
            else {
                return 0;
            }
        }

        private static string GetUnsafeConfigErrorInfoFilename(IConfigErrorInfo errorInfo) {
            if (errorInfo != null) {
                return errorInfo.Filename;
            }
            else {
                return null;
            }
        }

        [FileIOPermission(SecurityAction.Assert, AllFiles = FileIOPermissionAccess.PathDiscovery)]
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "This method simply extracts the filename, which isn't sensitive information.")]
        private static string ExtractFileNameWithAssert(string filename) {
            // This method can throw; callers should wrap in try / catch.

            string fullPath = Path.GetFullPath(filename);
            return Path.GetFileName(fullPath);
        }

        // 
        // Internal Helper to strip a full path to just filename.ext when caller 
        // does not have path discovery to the path (used for sane error handling).
        //
        internal static string SafeFilename(string filename) {
            if (string.IsNullOrEmpty(filename)) {
                return filename;
            }

            // configuration file can be an http URL in IE
            if (StringUtil.StartsWithIgnoreCase(filename, HTTP_PREFIX)) {
                return filename;
            }

            //
            // If it is a relative path, return it as is. 
            // This could happen if the exception was constructed from the serialization constructor,
            // and the caller did not have PathDiscoveryPermission for the file.
            //
            try {
                if (!Path.IsPathRooted(filename)) {
                    return filename;
                }
            }
            catch {
                return null;
            }

            try {
                // Confirm that it is a full path.
                // GetFullPath will also Demand PathDiscovery for the resulting path
                string fullPath = Path.GetFullPath(filename);
            } 
            catch (SecurityException) {
                // Get just the name of the file without the directory part.
                try {
                    filename = ExtractFileNameWithAssert(filename);
                }
                catch {
                    filename = null;
                }
            }
            catch {
                filename = null;
            }

            return filename;
        }

        // 
        // Internal Helper to always strip a full path to just filename.ext.
        //
        internal static string AlwaysSafeFilename(string filename) {
            if (string.IsNullOrEmpty(filename)) {
                return filename;
            }

            // configuration file can be an http URL in IE
            if (StringUtil.StartsWithIgnoreCase(filename, HTTP_PREFIX)) {
                return filename;
            }

            //
            // If it is a relative path, return it as is. 
            // This could happen if the exception was constructed from the serialization constructor,
            // and the caller did not have PathDiscoveryPermission for the file.
            //
            try {
                if (!Path.IsPathRooted(filename)) {
                    return filename;
                }
            }
            catch {
                return null;
            }

            // Get just the name of the file without the directory part.
            try {
                filename = ExtractFileNameWithAssert(filename);
            }
            catch {
                filename = null;
            }

            return filename;
        }

    }
}
