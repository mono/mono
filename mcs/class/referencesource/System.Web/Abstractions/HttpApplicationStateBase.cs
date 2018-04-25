//------------------------------------------------------------------------------
// <copyright file="HttpApplicationStateBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix",
        Justification = "This type is an abstraction for HttpApplicationState.")]
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable",
        Justification = "The abstraction is not meant to be serialized.")]
    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public abstract class HttpApplicationStateBase : NameObjectCollectionBase, ICollection {

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
            Justification = "Matches HttpApplicationState class")]
        public virtual string[] AllKeys {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual HttpApplicationStateBase Contents {
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

        public virtual object this[int index] {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual object this[string name] {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual HttpStaticObjectsCollectionBase StaticObjects {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual void Add(string name, object value) {
            throw new NotImplementedException();
        }

        public virtual void Clear() {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public virtual void CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get",
            Justification = "Matches HttpApplicationState class")]
        public virtual object Get(int index) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get",
            Justification = "Matches HttpApplicationState class")]
        public virtual object Get(string name) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override IEnumerator GetEnumerator() {
            throw new NotImplementedException();
        }

        public virtual string GetKey(int index) {
            throw new NotImplementedException();
        }

        public virtual void Lock() {
            throw new NotImplementedException();
        }

        public virtual void Remove(string name) {
            throw new NotImplementedException();
        }

        public virtual void RemoveAll() {
            throw new NotImplementedException();
        }

        public virtual void RemoveAt(int index) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Set",
            Justification = "Matches HttpApplicationState class")]
        public virtual void Set(string name, object value) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Un",
            Justification = "Matches HttpApplicationState class")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "UnLock",
            Justification = "Matched HttpApplicationState class")]
        [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="UnLock",
            Justification = "Matched HttpApplicationState class")]
        public virtual void UnLock() {
            throw new NotImplementedException();
        }

    }
}
