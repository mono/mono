//------------------------------------------------------------------------------
// <copyright file="ConfigurationException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Configuration.Internal;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;
    using System.Collections;
    using System.Runtime.Versioning;

    // A config exception can contain a filename (of a config file)
    // and a line number (of the location in the file in which a problem was
    // encountered).
    // 
    // Section handlers should throw this exception (or subclasses)
    // together with filename and line nubmer information where possible.
    [Serializable]
    public class ConfigurationException : SystemException {
        private const string    HTTP_PREFIX = "http:";

        private string          _filename; 
        private int             _line;     

        void Init(string filename, int line) {
            HResult = HResults.Configuration;
            _filename = filename;
            _line = line;
        }

        // Default ctor is required for serialization.
        protected ConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context) { 
            Init(info.GetString("filename"), info.GetInt32("line"));
        }

        [Obsolete("This class is obsolete, to create a new exception create a System." +
                  "Configuration!System.Configuration.ConfigurationErrorsException")]
        public ConfigurationException() : 
                this(null, null, null, 0) {}

        [Obsolete("This class is obsolete, to create a new exception create a System." +
                  "Configuration!System.Configuration.ConfigurationErrorsException")]
        public ConfigurationException(string message) : 
                this(message, null, null, 0) {}

        [Obsolete("This class is obsolete, to create a new exception create a System." +
                  "Configuration!System.Configuration.ConfigurationErrorsException")]
        public ConfigurationException(string message, Exception inner) : 
                this(message, inner, null, 0) {}


        [Obsolete("This class is obsolete, to create a new exception create a System." +
                  "Configuration!System.Configuration.ConfigurationErrorsException")]
        public ConfigurationException(string message, XmlNode node) : 
                this(message, null, GetUnsafeXmlNodeFilename(node), GetXmlNodeLineNumber(node)) {}

        [Obsolete("This class is obsolete, to create a new exception create a System." +
                  "Configuration!System.Configuration.ConfigurationErrorsException")]
        public ConfigurationException(string message, Exception inner, XmlNode node) : 
                this(message, inner, GetUnsafeXmlNodeFilename(node), GetXmlNodeLineNumber(node)) {}


        [Obsolete("This class is obsolete, to create a new exception create a System." +
                  "Configuration!System.Configuration.ConfigurationErrorsException")]
        public ConfigurationException(string message, string filename, int line) :
                this(message, null, filename, line) {}

        [Obsolete("This class is obsolete, to create a new exception create a System." +
                  "Configuration!System.Configuration.ConfigurationErrorsException")]
        public ConfigurationException(string message, Exception inner, string filename, int line) : base(message, inner) {
            Init(filename, line);
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue("filename", _filename);
            info.AddValue("line", _line);
        }

        // The message includes the file/line number information.  
        // To get the message without the extra information, use BareMessage.
        public override string Message {
            get {
                string file = Filename;
                if (!string.IsNullOrEmpty(file)) {
                    if (Line != 0) {
                        return BareMessage + " (" + file + " line " + Line.ToString(CultureInfo.InvariantCulture) + ")";
                    }
                    else {
                        return BareMessage + " (" + file + ")";
                    }
                }
                else if (Line != 0) {
                    return BareMessage + " (line " + Line.ToString("G", CultureInfo.InvariantCulture) + ")";
                }
                else {
                    return BareMessage;
                }
            }
        }

        public virtual string BareMessage {
            get {
                return base.Message;
            }
        }

        public virtual string Filename {
            [ResourceExposure(ResourceScope.Machine)]
            [ResourceConsumption(ResourceScope.Machine)]
            get {
                return SafeFilename(_filename);
            }
        }

        public virtual int Line {
            get {
                return _line;
            }
        }

        [Obsolete("This class is obsolete, use System.Configuration!System.Configuration." +
                  "ConfigurationErrorsException.GetFilename instead")]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static string GetXmlNodeFilename(XmlNode node) {
            return SafeFilename(GetUnsafeXmlNodeFilename(node));
        }

        [Obsolete("This class is obsolete, use System.Configuration!System.Configuration." +
                  "ConfigurationErrorsException.GetLinenumber instead")]
        public static int GetXmlNodeLineNumber(XmlNode node) {
            IConfigErrorInfo configNode = node as IConfigErrorInfo;

            if (configNode != null) {
                return configNode.LineNumber;
            }
            return 0;
        }

        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private static string FullPathWithAssert(string filename) {
            string fullPath = null;

            try {
                fullPath = Path.GetFullPath(filename);
            }
            catch {
            }

            return fullPath;
        }

        // 
        // Internal Helper to strip a full path to just filename.ext when caller 
        // does not have path discovery to the path (used for sane error handling).
        //
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static string SafeFilename(string filename) {
            if (string.IsNullOrEmpty(filename)) {
                return filename;
            }

            // configuration file can be an http URL in IE
            if (filename.StartsWith(HTTP_PREFIX, StringComparison.OrdinalIgnoreCase)) {
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
                    string fullPath = FullPathWithAssert(filename);
                    filename = Path.GetFileName(fullPath);
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

        private static string GetUnsafeXmlNodeFilename(XmlNode node) {
            IConfigErrorInfo configNode = node as IConfigErrorInfo;

            if (configNode != null) {
                return configNode.Filename;
            }

            return string.Empty;
        }
    }
}
