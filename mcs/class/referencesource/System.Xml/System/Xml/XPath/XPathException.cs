//------------------------------------------------------------------------------
// <copyright file="XPathException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml.XPath {
    using System;
    using System.IO;
    using System.Resources;
    using System.Runtime.Serialization;
    using System.Diagnostics;
    using System.Security.Permissions;

    // Represents the exception that is thrown when there is error processing an
    // XPath expression.
    [Serializable]
    public class XPathException : SystemException {
        // we need to keep this members for V1 serialization compatibility
        string   res;
        string[] args;

        // message != null for V1 & V2 exceptions deserialized in Whidbey
        // message == null for created V2 exceptions; the exception message is stored in Exception._message
        string   message;
                                                                                                
        protected XPathException(SerializationInfo info, StreamingContext context) : base(info, context) {
            res  = (string  ) info.GetValue("res" , typeof(string  ));
            args = (string[]) info.GetValue("args", typeof(string[]));

            // deserialize optional members
            string version = null;
            foreach ( SerializationEntry e in info ) {
                if ( e.Name == "version" ) {
                    version = (string)e.Value;
                }
            }

            if (version == null) {
                // deserializing V1 exception
                message = CreateMessage(res, args);
            }
            else {
                // deserializing V2 or higher exception -> exception message is serialized by the base class (Exception._message)
                message = null;
            }
        }

        [SecurityPermissionAttribute(SecurityAction.LinkDemand,SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue("res" , res );
            info.AddValue("args", args);
            info.AddValue("version", "2.0");
        }

        public XPathException() : this (string.Empty, (Exception) null) {}

        public XPathException(string message) : this (message, (Exception) null) {}

        public XPathException(string message, Exception innerException) : 
            this(Res.Xml_UserException, new string[] { message }, innerException) {
        }

        internal static XPathException Create(string res) {
            return new XPathException(res, (string[])null);
        }

        internal static XPathException Create(string res, string arg) {
            return new XPathException(res, new string[] { arg });
        }
            
        internal static XPathException Create(string res, string arg, string arg2) {
            return new XPathException(res, new string[] { arg, arg2 });
        }
            
        internal static XPathException Create(string res, string arg, Exception innerException) {
            return new XPathException(res, new string[] { arg }, innerException);
        }
            
        private XPathException(string res, string[] args) :
            this(res, args, null) {
        }

        private XPathException(string res, string[] args, Exception inner) :
            base(CreateMessage(res, args), inner) {
            HResult = HResults.XmlXPath;
            this.res = res;
            this.args = args;
        }

        private static string CreateMessage(string res, string[] args) { 
            try {
                string message = Res.GetString(res, args);
                if (message == null)
                    message = "UNKNOWN("+res+")";
                return message;
            }
            catch ( MissingManifestResourceException ) {
                return "UNKNOWN("+res+")";
            }
        }

        public override string Message {
            get {
                return (message == null) ? base.Message : message;
            }
        }
    }
}

