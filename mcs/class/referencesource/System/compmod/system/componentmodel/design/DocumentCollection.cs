//------------------------------------------------------------------------------
// <copyright file="DocumentCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using Microsoft.Win32;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>
    ///       Provides a read-only collection of documents.
    ///    </para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    public class DesignerCollection : ICollection {
        private IList designers;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.Design.DesignerCollection'/> class
        ///       that stores an array with a pointer to each <see cref='System.ComponentModel.Design.IDesignerHost'/>
        ///       for each document in the collection.
        ///    </para>
        /// </devdoc>
        public DesignerCollection(IDesignerHost[] designers) {
            if (designers != null) {
                this.designers = new ArrayList(designers);
            }
            else {
                this.designers = new ArrayList();
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.Design.DesignerCollection'/> class
        ///       that stores an array with a pointer to each <see cref='System.ComponentModel.Design.IDesignerHost'/>
        ///       for each document in the collection.
        ///    </para>
        /// </devdoc>
        public DesignerCollection(IList designers) {
            this.designers = designers;
        }

        /// <devdoc>
        ///    <para>Gets or
        ///       sets the number
        ///       of documents in the collection.</para>
        /// </devdoc>
        public int Count {
            get {
                return designers.Count;
            }
        }

        /// <devdoc>
        ///    <para> Gets
        ///       or sets the document at the specified index.</para>
        /// </devdoc>
        public virtual IDesignerHost this[int index] {
            get {
                return (IDesignerHost)designers[index];
            }
        }

        /// <devdoc>
        ///    <para>Creates and retrieves a new enumerator for this collection.</para>
        /// </devdoc>
        public IEnumerator GetEnumerator() {
            return designers.GetEnumerator();
        }

        /// <internalonly/>
        int ICollection.Count {
            get {
                return Count;
            }
        }
      
        /// <internalonly/>
        bool ICollection.IsSynchronized {
            get {
                return false;
            }
        }

        /// <internalonly/>
        object ICollection.SyncRoot {
            get {
                return null;
            }
        }

        /// <internalonly/>
        void ICollection.CopyTo(Array array, int index) {
            designers.CopyTo(array, index);
        }

        /// <internalonly/>
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}

