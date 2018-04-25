//------------------------------------------------------------------------------
// <copyright file="HttpFileCollectionBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix",
        Justification = "This type is an abstraction for HttpFileCollectionBase.")]
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable",
        Justification = "The abstraction is not meant to be serialized.")]
    public abstract class HttpFileCollectionBase : NameObjectCollectionBase, ICollection {

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
            Justification = "Matches HttpFileCollectionBase class")]
        public virtual string[] AllKeys {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override int Count {
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

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public virtual object SyncRoot {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual HttpPostedFileBase this[string name] {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual HttpPostedFileBase this[int index] {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dest",
            Justification = "Matches HttpFileCollectionBase class")]
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public virtual void CopyTo(Array dest, int index) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get",
            Justification = "Matches HttpFileCollection class")]
        public virtual HttpPostedFileBase Get(int index) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get",
            Justification = "Matches HttpFileCollection class")]
        public virtual HttpPostedFileBase Get(string name) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get",
            Justification = "Matches HttpFileCollection class")]
        public virtual IList<HttpPostedFileBase> GetMultiple(string name) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override IEnumerator GetEnumerator() {
            throw new NotImplementedException();
        }

        public virtual string GetKey(int index) {
            throw new NotImplementedException();
        }

    }
}
