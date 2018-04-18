//------------------------------------------------------------------------------
// <copyright file="XmlSchemaException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                 
//------------------------------------------------------------------------------

namespace System.Xml.Schema {
    using System;
    using System.IO;
    using System.Text;  
    using System.Resources;
    using System.Runtime.Serialization;
    using System.Globalization;
    using System.Diagnostics;
    using System.Security.Permissions;

    /// <include file='doc\XmlSchemaException.uex' path='docs/doc[@for="XmlSchemaException"]/*' />
    [Serializable]
    public class XmlSchemaException : SystemException {
        string res;
        string[] args;
        string sourceUri;
        int lineNumber;
        int linePosition;

        [NonSerialized]
        XmlSchemaObject sourceSchemaObject;

        // message != null for V1 exceptions deserialized in Whidbey
        // message == null for V2 or higher exceptions; the exception message is stored on the base class (Exception._message)
        string message;

        /// <include file='doc\XmlSchemaException.uex' path='docs/doc[@for="XmlSchemaException.XmlSchemaException5"]/*' />
        protected XmlSchemaException(SerializationInfo info, StreamingContext context) : base(info, context) {
            res                = (string)         info.GetValue("res"  , typeof(string));
            args               = (string[])       info.GetValue("args", typeof(string[]));
            sourceUri          = (string)         info.GetValue("sourceUri", typeof(string));
            lineNumber         = (int)            info.GetValue("lineNumber", typeof(int));
            linePosition       = (int)            info.GetValue("linePosition", typeof(int));

            // deserialize optional members
            string version = null;
            foreach ( SerializationEntry e in info ) {
                if ( e.Name == "version" ) {
                    version = (string)e.Value;
                }
            }

            if ( version == null ) {
                // deserializing V1 exception
                message = CreateMessage( res, args );
            }
            else {
                // deserializing V2 or higher exception -> exception message is serialized by the base class (Exception._message)
                message = null;
            }
        }


        /// <include file='doc\XmlSchemaException.uex' path='docs/doc[@for="XmlSchemaException.GetObjectData"]/*' />
        [SecurityPermissionAttribute(SecurityAction.LinkDemand,SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue("res",                res);
            info.AddValue("args",               args);
            info.AddValue("sourceUri",          sourceUri);
            info.AddValue("lineNumber",         lineNumber);
            info.AddValue("linePosition",       linePosition);
            info.AddValue("version",            "2.0");
        }

        
        /// <include file='doc\XmlSchemaException.uex' path='docs/doc[@for="XmlSchemaException.XmlSchemaException1"]/*' />
        public XmlSchemaException() : this(null) {
        }

        /// <include file='doc\XmlSchemaException.uex' path='docs/doc[@for="XmlSchemaException.XmlSchemaException2"]/*' />
        public XmlSchemaException(String message) : this (message, ((Exception)null), 0, 0) {
#if DEBUG
            Debug.Assert(message == null || !message.StartsWith("Sch_", StringComparison.Ordinal), "Do not pass a resource here!");
#endif
        }
        
        /// <include file='doc\XmlSchemaException.uex' path='docs/doc[@for="XmlSchemaException.XmlSchemaException0"]/*' />
        public XmlSchemaException(String message, Exception innerException) : this (message, innerException, 0, 0) {
        } 

        /// <include file='doc\XmlSchemaException.uex' path='docs/doc[@for="XmlSchemaException.XmlSchemaException3"]/*' />
        public XmlSchemaException(String message, Exception innerException, int lineNumber, int linePosition) : 
            this((message == null ? Res.Sch_DefaultException : Res.Xml_UserException), new string[] { message }, innerException, null, lineNumber, linePosition, null ) {
	    }

        internal XmlSchemaException(string res, string[] args) :
            this(res, args, null, null, 0, 0, null) {}
        
        internal XmlSchemaException(string res, string arg) :
            this(res, new string[] { arg }, null, null, 0, 0, null) {}

        internal XmlSchemaException(string res, string arg, string sourceUri, int lineNumber, int linePosition) :
            this(res, new string[] { arg }, null, sourceUri, lineNumber, linePosition, null) {}

        internal XmlSchemaException(string res, string sourceUri, int lineNumber, int linePosition) :
            this(res, (string[])null, null, sourceUri, lineNumber, linePosition, null) {}

        internal XmlSchemaException(string res, string[] args, string sourceUri, int lineNumber, int linePosition) :
            this(res, args, null, sourceUri, lineNumber, linePosition, null) {}

        internal XmlSchemaException(string res, XmlSchemaObject source) :
            this(res, (string[])null, source) {}

        internal XmlSchemaException(string res, string arg, XmlSchemaObject source) :
            this(res, new string[] { arg }, source) {}

        internal XmlSchemaException(string res, string[] args, XmlSchemaObject source) :
            this(res, args, null, source.SourceUri,  source.LineNumber, source.LinePosition, source) {}

        internal XmlSchemaException(string res, string[] args, Exception innerException, string sourceUri, int lineNumber, int linePosition, XmlSchemaObject source) :
            base (CreateMessage(res, args), innerException) {

            HResult = HResults.XmlSchema;
            this.res = res;
            this.args = args;
            this.sourceUri = sourceUri;
            this.lineNumber = lineNumber;
            this.linePosition = linePosition;
            this.sourceSchemaObject = source;
        }

        internal static string CreateMessage(string res, string[] args) {
            try {
                return Res.GetString(res, args);
            }
            catch ( MissingManifestResourceException ) {
                return "UNKNOWN("+res+")";
            }
        }
        
        internal string GetRes {
            get {
	            return res;
            }
        }

        internal string[] Args {
            get {
	            return args; 
            }
        }
        /// <include file='doc\XmlSchemaException.uex' path='docs/doc[@for="XmlSchemaException.SourceUri"]/*' />
        public string SourceUri {
            get { return this.sourceUri; }
        }

        /// <include file='doc\XmlSchemaException.uex' path='docs/doc[@for="XmlSchemaException.LineNumber"]/*' />
        public int LineNumber {
            get { return this.lineNumber; }
        }

        /// <include file='doc\XmlSchemaException.uex' path='docs/doc[@for="XmlSchemaException.LinePosition"]/*' />
        public int LinePosition {
            get { return this.linePosition; }
        }
        
        /// <include file='doc\XmlSchemaException.uex' path='docs/doc[@for="XmlSchemaException.SourceObject"]/*' />
        public XmlSchemaObject SourceSchemaObject {
            get { return this.sourceSchemaObject; }
        }
        
        /*internal static XmlSchemaException Create(string res) { //Since internal overload with res string will clash with public constructor that takes in a message
            return new XmlSchemaException(res, (string[])null, null, null, 0, 0, null);
        }*/

        internal void SetSource(string sourceUri, int lineNumber, int linePosition) {
            this.sourceUri = sourceUri;
            this.lineNumber = lineNumber;
            this.linePosition = linePosition;
        }

        internal void SetSchemaObject(XmlSchemaObject source) {
            this.sourceSchemaObject = source;
        }

        internal void SetSource(XmlSchemaObject source) {
            this.sourceSchemaObject = source;
            this.sourceUri = source.SourceUri;
            this.lineNumber = source.LineNumber;
            this.linePosition = source.LinePosition;
        }

        internal void SetResourceId(string resourceId) {
            this.res = resourceId;
        }

        public override string Message {
            get {
                return (message == null) ? base.Message : message;
            }
        }
    };
} // namespace System.Xml.Schema


