//------------------------------------------------------------------------------
// <copyright file="XmlSchemaValidationException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright> 
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Schema {
    using System;
    using System.IO;
    using System.Text;  
    using System.Resources;
    using System.Runtime.Serialization;
    using System.Diagnostics;
	using System.Security.Permissions;

    /// <include file='doc\XmlSchemaException.uex' path='docs/doc[@for="XmlSchemaException"]/*' />
    [Serializable]
    public class XmlSchemaValidationException : XmlSchemaException {
        
        private Object sourceNodeObject;
               
        /// <include file='doc\XmlSchemaException.uex' path='docs/doc[@for="XmlSchemaException.XmlSchemaException5"]/*' />
        protected XmlSchemaValidationException(SerializationInfo info, StreamingContext context) : base(info, context) {}
            

        /// <include file='doc\XmlSchemaException.uex' path='docs/doc[@for="XmlSchemaException.GetObjectData"]/*' />
        [SecurityPermissionAttribute(SecurityAction.LinkDemand,SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);
        }

        /// <include file='doc\XmlSchemaException.uex' path='docs/doc[@for="XmlSchemaException.XmlSchemaException1"]/*' />
        public XmlSchemaValidationException() : base(null) {
        }

        /// <include file='doc\XmlSchemaException.uex' path='docs/doc[@for="XmlSchemaException.XmlSchemaException2"]/*' />
        public XmlSchemaValidationException(String message) : base (message, ((Exception)null), 0, 0) {
        }
        
        /// <include file='doc\XmlSchemaException.uex' path='docs/doc[@for="XmlSchemaException.XmlSchemaException0"]/*' />
        public XmlSchemaValidationException(String message, Exception innerException) : base (message, innerException, 0, 0) {
        } 

        /// <include file='doc\XmlSchemaException.uex' path='docs/doc[@for="XmlSchemaException.XmlSchemaException3"]/*' />
        public XmlSchemaValidationException(String message, Exception innerException, int lineNumber, int linePosition) : 
            base(message, innerException, lineNumber, linePosition) {
	    }
            
        internal XmlSchemaValidationException(string res, string[] args) : base(res, args, null, null, 0, 0, null) {
        }
        
        internal XmlSchemaValidationException(string res, string arg) : base(res, new string[] { arg }, null, null, 0, 0, null) {
        }

        internal XmlSchemaValidationException(string res, string arg, string sourceUri, int lineNumber, int linePosition) :
            base(res, new string[] { arg }, null, sourceUri, lineNumber, linePosition, null) {
        }

        internal XmlSchemaValidationException(string res, string sourceUri, int lineNumber, int linePosition) :
            base(res, (string[])null, null, sourceUri, lineNumber, linePosition, null) {
        }

        internal XmlSchemaValidationException(string res, string[] args, string sourceUri, int lineNumber, int linePosition) :
            base(res, args, null, sourceUri, lineNumber, linePosition, null) {
        }

        internal XmlSchemaValidationException(string res, string[] args, Exception innerException, string sourceUri, int lineNumber, int linePosition) :
            base(res, args, innerException, sourceUri, lineNumber, linePosition, null) {
        }

        internal XmlSchemaValidationException(string res, string[] args, object sourceNode) :
            base(res, args, null, null,  0, 0, null) {
                this.sourceNodeObject = sourceNode;
        }

        internal XmlSchemaValidationException(string res, string[] args, string sourceUri, object sourceNode) :
            base(res, args, null, sourceUri,  0, 0, null) {
                this.sourceNodeObject = sourceNode;
        }

        internal XmlSchemaValidationException(string res, string[] args, string sourceUri, int lineNumber, int linePosition, XmlSchemaObject source, object sourceNode) :
            base(res, args, null, sourceUri, lineNumber, linePosition, source) {
                this.sourceNodeObject = sourceNode;
        }
        
         /// <include file='doc\XmlSchemaException.uex' path='docs/doc[@for="XmlSchemaException.SourceUri"]/*' />
        public Object SourceObject {
            get { return this.sourceNodeObject; }
        }

        protected internal void SetSourceObject (Object sourceObject){
            this.sourceNodeObject = sourceObject;
        }

    };
} // namespace System.Xml.Schema


