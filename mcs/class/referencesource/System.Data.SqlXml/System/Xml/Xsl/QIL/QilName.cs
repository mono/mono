//------------------------------------------------------------------------------
// <copyright file="QilName.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------
using System;
using System.Diagnostics;

namespace System.Xml.Xsl.Qil {

    /// <summary>
    /// View over a Qil name literal.
    /// </summary>
    /// <remarks>
    /// Don't construct QIL nodes directly; instead, use the <see cref="QilFactory">QilFactory</see>.
    /// </remarks>
    internal class QilName : QilLiteral {
        private string local;
        private string uri;
        private string prefix;


        //-----------------------------------------------
        // Constructor
        //-----------------------------------------------

        /// <summary>
        /// Construct a new node
        /// </summary>
        public QilName(QilNodeType nodeType, string local, string uri, string prefix) : base(nodeType, null) {
            LocalName = local;
            NamespaceUri = uri;
            Prefix = prefix;
            Value = this;
        }


        //-----------------------------------------------
        // QilName methods
        //-----------------------------------------------

        public string LocalName {
            get { return this.local; }
            set { this.local = value; }
        }

        public string NamespaceUri {
            get { return this.uri; }
            set { this.uri = value; }
        }

        public string Prefix {
            get { return this.prefix; }
            set { this.prefix = value; }
        }

        /// <summary>
        /// Build the qualified name in the form prefix:local
        /// </summary>
        public string QualifiedName {
            get {
                if (this.prefix.Length == 0) {
                    return this.local;
                }
                else {
                    return this.prefix + ':' + this.local;
                }
            }
        }

        /// <summary>
        /// Override GetHashCode() so that the QilName can be used as a key in the hashtable.
        /// </summary>
        /// <remarks>Does not compare their prefixes (if any).</remarks>
        public override int GetHashCode() {
            return this.local.GetHashCode();
        }

        /// <summary>
        /// Override Equals() so that the QilName can be used as a key in the hashtable.
        /// </summary>
        /// <remarks>Does not compare their prefixes (if any).</remarks>
        public override bool Equals(object other) {
            QilName name = other as QilName;
            if (name == null)
                return false;

            return this.local == name.local && this.uri == name.uri;
        }

        /// <summary>
        /// Implement operator == to prevent exidental referential comarison 
        /// </summary>
        /// <remarks>Does not compare their prefixes (if any).</remarks>
        public static bool operator ==(QilName a, QilName b) {
            if ((object)a == (object)b) {
                return true;
            }
            if ((object)a == null || (object)b == null) {
                return false;
            }
            return a.local == b.local && a.uri == b.uri;
        }

        /// <summary>
        /// Implement operator != to prevent exidental referential comarison 
        /// </summary>
        /// <remarks>Does not compare their prefixes (if any).</remarks>
        public static bool operator !=(QilName a, QilName b) {
            return !(a == b);
        }

        /// <summary>
        /// Return the QilName in this format: "{namespace}prefix:local-name".
        /// If the namespace is empty, return the QilName in this truncated format: "local-name".
        /// If the prefix is empty, return the QilName in this truncated format: "{namespace}local-name".
        /// </summary>
        public override string ToString() {
            if (prefix.Length == 0) {
                if (uri.Length == 0)
                    return local;

                return string.Concat("{", uri, "}", local);
            }

            return string.Concat("{", uri, "}", prefix, ":", local);
        }
    }
}
