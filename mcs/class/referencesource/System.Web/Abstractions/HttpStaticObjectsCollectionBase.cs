//------------------------------------------------------------------------------
// <copyright file="HttpStaticObjectsCollectionBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix",
        Justification = "This type is an abstraction for HttpStaticObjectsCollection.")]
    public abstract class HttpStaticObjectsCollectionBase : ICollection, IEnumerable {

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public virtual int Count {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsReadOnly {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public virtual bool IsSynchronized {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual object this[string name] {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool NeverAccessed {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public virtual object SyncRoot {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public virtual void CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public virtual IEnumerator GetEnumerator() {
            throw new NotImplementedException();
        }

        public virtual object GetObject(string name) {
            throw new NotImplementedException();
        }

        public virtual void Serialize(BinaryWriter writer) {
            throw new NotImplementedException();
        }

    }
}
