//------------------------------------------------------------------------------
// <copyright file="HttpSessionStateBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System.Collections;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.SessionState;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix",
        Justification = "This is consistent with the type it abstracts in System.Web.dll.")]
    public abstract class HttpSessionStateBase : ICollection, IEnumerable {
        public virtual int CodePage {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual HttpSessionStateBase Contents {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual HttpCookieMode CookieMode {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsCookieless {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsNewSession {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsReadOnly {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual NameObjectCollectionBase.KeysCollection Keys {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased",
            Justification = "This is consistent with the type it abstracts in System.Web.dll.")]
        public virtual int LCID {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual SessionStateMode Mode {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID",
            Justification = "This is consistent with the type it abstracts in System.Web.dll.")]
        public virtual string SessionID {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual HttpStaticObjectsCollectionBase StaticObjects {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual int Timeout {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual object this[int index] {
            get {
                throw new NotImplementedException();
            }
            set {
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

        public virtual void Abandon() {
            throw new NotImplementedException();
        }

        public virtual void Add(string name, object value) {
            throw new NotImplementedException();
        }

        public virtual void Clear() {
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

        #region ICollection Members
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public virtual void CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public virtual int Count {
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
        #endregion

        #region IEnumerable Members
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public virtual IEnumerator GetEnumerator() {
            throw new NotImplementedException();
        }
        #endregion
    }
}
