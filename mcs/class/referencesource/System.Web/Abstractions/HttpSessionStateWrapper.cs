//------------------------------------------------------------------------------
// <copyright file="HttpSessionStateWrapper.cs" company="Microsoft">
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
        Justification = "This type name needs to match the ASP.NET 2.0 type name.")]
    public class HttpSessionStateWrapper : HttpSessionStateBase {
        private readonly HttpSessionState _session;

        public HttpSessionStateWrapper(HttpSessionState httpSessionState) {
            if (httpSessionState == null) {
                throw new ArgumentNullException("httpSessionState");
            }
            _session = httpSessionState;
        }

        public override int CodePage {
            get {
                return _session.CodePage;
            }
            set {
                _session.CodePage = value;
            }
        }

        public override HttpSessionStateBase Contents {
            get {
                return this;
            }
        }

        public override HttpCookieMode CookieMode {
            get {
                return _session.CookieMode;
            }
        }

        public override bool IsCookieless {
            get {
                return _session.IsCookieless;
            }
        }

        public override bool IsNewSession {
            get {
                return _session.IsNewSession;
            }
        }

        public override bool IsReadOnly {
            get {
                return _session.IsReadOnly;
            }
        }

        public override NameObjectCollectionBase.KeysCollection Keys {
            get {
                return _session.Keys;
            }
        }

        public override int LCID {
            get {
                return _session.LCID;
            }
            set {
                _session.LCID = value;
            }
        }

        public override SessionStateMode Mode {
            get {
                return _session.Mode;
            }
        }

        public override string SessionID {
            get {
                return _session.SessionID;
            }
        }

        public override HttpStaticObjectsCollectionBase StaticObjects {
            get {
                // method returns an empty collection rather than null
                return new HttpStaticObjectsCollectionWrapper(_session.StaticObjects);
            }
        }

        public override int Timeout {
            get {
                return _session.Timeout;
            }
            set {
                _session.Timeout = value;
            }
        }

        public override object this[int index] {
            get {
                return _session[index];
            }
            set {
                _session[index] = value;
            }
        }

        public override object this[string name] {
            get {
                return _session[name];
            }
            set {
                _session[name] = value;
            }
        }

        public override void Abandon() {
            _session.Abandon();
        }

        public override void Add(string name, object value) {
            _session.Add(name, value);
        }

        public override void Clear() {
            _session.Clear();
        }

        public override void Remove(string name) {
            _session.Remove(name);
        }

        public override void RemoveAll() {
            _session.RemoveAll();
        }

        public override void RemoveAt(int index) {
            _session.RemoveAt(index);
        }

        #region ICollection Members
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override void CopyTo(Array array, int index) {
            _session.CopyTo(array, index);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override int Count {
            get {
                return _session.Count;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override bool IsSynchronized {
            get {
                return _session.IsSynchronized;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override object SyncRoot {
            get {
                return _session.SyncRoot;
            }
        }
        #endregion

        #region IEnumerable Members
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override IEnumerator GetEnumerator() {
            return _session.GetEnumerator();
        }
        #endregion
    }
}
