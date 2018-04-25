//------------------------------------------------------------------------------
// <copyright file="HttpApplicationStateWrapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Runtime.CompilerServices;

    [SuppressMessage("Microsoft.Security", "CA2126:TypeLinkDemandsRequireInheritanceDemands", Justification="Workaround for FxCop Bug")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix",
        Justification = "This type is an abstraction for HttpApplicationState.")]
    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class HttpApplicationStateWrapper : HttpApplicationStateBase {

        private HttpApplicationState _application;

        public HttpApplicationStateWrapper(HttpApplicationState httpApplicationState) {
            if (httpApplicationState == null) {
                throw new ArgumentNullException("httpApplicationState");
            }
            _application = httpApplicationState;
        }

        public override string[] AllKeys {
            get {
                return _application.AllKeys;
            }
        }

        public override HttpApplicationStateBase Contents {
            get {
                return this;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override int Count {
            get {
                return _application.Count;
            }
        }

        public override bool IsSynchronized {
            get {
                return ((ICollection)_application).IsSynchronized;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override NameObjectCollectionBase.KeysCollection Keys {
            get {
                return _application.Keys;
            }
        }

        public override object SyncRoot {
            get {
                return ((ICollection)_application).SyncRoot;
            }
        }

        public override object this[int index] {
            get {
                return _application[index];
            }
        }

        public override object this[string name] {
            get {
                return _application[name];
            }
            set {
                _application[name] = value;
            }
        }

        public override HttpStaticObjectsCollectionBase StaticObjects {
            get {
                // method returns an empty collection rather than null
                return new HttpStaticObjectsCollectionWrapper(_application.StaticObjects);
            }
        }

        public override void Add(string name, object value) {
            _application.Add(name, value);
        }

        public override void Clear() {
            _application.Clear();
        }

        public override void CopyTo(Array array, int index) {
            ((ICollection)_application).CopyTo(array, index);
        }

        public override object Get(int index) {
            return _application.Get(index);
        }

        public override object Get(string name) {
            return _application.Get(name);
        }

        public override IEnumerator GetEnumerator() {
            return ((IEnumerable)_application).GetEnumerator();
        }

        public override string GetKey(int index) {
            return _application.GetKey(index);
        }

        [SuppressMessage("Microsoft.Security", "CA2114:MethodSecurityShouldBeASupersetOfType", Justification = "Workaround for FxCop Bug")]
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            _application.GetObjectData(info, context);
        }

        public override void Lock() {
            _application.Lock();
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override void OnDeserialization(object sender) {
            _application.OnDeserialization(sender);
        }

        public override void Remove(string name) {
            _application.Remove(name);
        }

        public override void RemoveAll() {
            _application.RemoveAll();
        }

        public override void RemoveAt(int index) {
            _application.RemoveAt(index);
        }

        public override void Set(string name, object value) {
            _application.Set(name, value);
        }

        public override void UnLock() {
            _application.UnLock();
        }

    }
}
