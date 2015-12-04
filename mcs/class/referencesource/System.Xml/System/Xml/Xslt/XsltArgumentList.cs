//------------------------------------------------------------------------------
// <copyright file="XsltArgumentList.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

using System.Collections;
using System.Security.Permissions;

namespace System.Xml.Xsl {

    public abstract class XsltMessageEncounteredEventArgs : EventArgs {
        public abstract string Message { get; }
    }

    public delegate void XsltMessageEncounteredEventHandler(object sender, XsltMessageEncounteredEventArgs e);

    public class XsltArgumentList {
        private Hashtable parameters = new Hashtable();
        private Hashtable extensions = new Hashtable();

        // Used for reporting xsl:message's during execution
        internal XsltMessageEncounteredEventHandler xsltMessageEncountered = null;

        public XsltArgumentList() {}

        public object GetParam(string name, string namespaceUri) {
            return this.parameters[new XmlQualifiedName(name, namespaceUri)];
        }

        public object GetExtensionObject(string namespaceUri) {
            return this.extensions[namespaceUri];
        }

        public void AddParam(string name, string namespaceUri, object parameter) {
            CheckArgumentNull(name        , "name"        );
            CheckArgumentNull(namespaceUri, "namespaceUri");
            CheckArgumentNull(parameter   , "parameter"   );

            XmlQualifiedName qname = new XmlQualifiedName(name, namespaceUri);
            qname.Verify();
            this.parameters.Add(qname, parameter);
        }

        public void AddExtensionObject(string namespaceUri, object extension) {
            CheckArgumentNull(namespaceUri, "namespaceUri");
            CheckArgumentNull(extension   , "extension"   );
            this.extensions.Add(namespaceUri, extension);
        }

        public object RemoveParam(string name, string namespaceUri) {
            XmlQualifiedName qname = new XmlQualifiedName(name, namespaceUri);
            object parameter = this.parameters[qname];
            this.parameters.Remove(qname);
            return parameter;
        }

        public object RemoveExtensionObject(string namespaceUri) {
            object extension = this.extensions[namespaceUri];
            this.extensions.Remove(namespaceUri);
            return extension;
        }

        public event XsltMessageEncounteredEventHandler XsltMessageEncountered {
            add {
                xsltMessageEncountered += value;
            }
            remove {
                xsltMessageEncountered -= value;
            }
        }

        public void Clear() {
            this.parameters.Clear();
            this.extensions.Clear();
            xsltMessageEncountered = null;
        }

        private static void CheckArgumentNull(object param, string paramName) {
            if (param == null) {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
