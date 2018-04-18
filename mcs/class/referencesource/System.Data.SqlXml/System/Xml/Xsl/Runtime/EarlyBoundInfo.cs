//------------------------------------------------------------------------------
// <copyright file="EarlyBoundInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="false">Microsoft</owner>
//------------------------------------------------------------------------------

using System.Diagnostics;
using System.Reflection;

namespace System.Xml.Xsl.Runtime {

    /// <summary>
    /// This class contains information about early bound function objects.
    /// </summary>
    internal sealed class EarlyBoundInfo {
        private string namespaceUri;            // Namespace Uri mapped to these early bound functions
        private ConstructorInfo constrInfo;     // Constructor for the early bound function object

        public EarlyBoundInfo(string namespaceUri, Type ebType) {
            Debug.Assert(namespaceUri != null && ebType != null);

            // Get the default constructor
            this.namespaceUri = namespaceUri;
            this.constrInfo = ebType.GetConstructor(Type.EmptyTypes);
            Debug.Assert(this.constrInfo != null, "The early bound object type " + ebType.FullName + " must have a public default constructor");
        }

        /// <summary>
        /// Get the Namespace Uri mapped to these early bound functions.
        /// </summary>
        public string NamespaceUri { get { return this.namespaceUri; } }

        /// <summary>
        /// Return the Clr Type of the early bound object.
        /// </summary>
        public Type EarlyBoundType { get { return this.constrInfo.DeclaringType; } }

        /// <summary>
        /// Create an instance of the early bound object.
        /// </summary>
        public object CreateObject() { return this.constrInfo.Invoke(new object[] {}); }

        /// <summary>
        /// Override Equals method so that EarlyBoundInfo to implement value comparison.
        /// </summary>
        public override bool Equals(object obj) {
            EarlyBoundInfo info = obj as EarlyBoundInfo;
            if (info == null)
                return false;

            return this.namespaceUri == info.namespaceUri && this.constrInfo == info.constrInfo;
        }

        /// <summary>
        /// Override GetHashCode since Equals is overriden.
        /// </summary>
        public override int GetHashCode() {
            return this.namespaceUri.GetHashCode();
        }
    }
}

