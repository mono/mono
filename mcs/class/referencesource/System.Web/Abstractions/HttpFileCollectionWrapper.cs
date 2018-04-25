//------------------------------------------------------------------------------
// <copyright file="HttpFileCollectionWrapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    [SuppressMessage("Microsoft.Security", "CA2126:TypeLinkDemandsRequireInheritanceDemands", Justification = "Workaround for FxCop Bug")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix",
        Justification = "This type is an abstraction for HttpFileCollection.")]
    public class HttpFileCollectionWrapper : HttpFileCollectionBase {

        private HttpFileCollection _collection;

        public HttpFileCollectionWrapper(HttpFileCollection httpFileCollection) {
            if (httpFileCollection == null) {
                throw new ArgumentNullException("httpFileCollection");
            }
            _collection = httpFileCollection;
        }

        public override string[] AllKeys {
            get {
                return _collection.AllKeys;
            }
        }

        public override int Count {
            get {
                return ((ICollection)_collection).Count;
            }
        }

        public override bool IsSynchronized {
            get {
                return ((ICollection)_collection).IsSynchronized;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override NameObjectCollectionBase.KeysCollection Keys {
            get {
                return _collection.Keys;
            }
        }

        public override object SyncRoot {
            get {
                return ((ICollection)_collection).SyncRoot;
            }
        }

        public override HttpPostedFileBase this[string name] {
            get {
                HttpPostedFile file = _collection[name];
                return (file != null) ? new HttpPostedFileWrapper(file) : null;
            }
        }

        public override HttpPostedFileBase this[int index] {
            get {
                HttpPostedFile file = _collection[index];
                return (file != null) ? new HttpPostedFileWrapper(file) : null;
            }
        }

        public override void CopyTo(Array dest, int index) {
            _collection.CopyTo(dest, index);
        }

        public override HttpPostedFileBase Get(int index) {
            HttpPostedFile file = _collection.Get(index);
            return (file != null) ? new HttpPostedFileWrapper(file) : null;
        }

        public override HttpPostedFileBase Get(string name) {
            HttpPostedFile file = _collection.Get(name);
            return (file != null) ? new HttpPostedFileWrapper(file) : null;
        }

        public override IList<HttpPostedFileBase> GetMultiple(string name) {
            ICollection<HttpPostedFile> files = _collection.GetMultiple(name);
            Debug.Assert(files != null);
            return files.Select(f => (HttpPostedFileBase)new HttpPostedFileWrapper(f)).ToList().AsReadOnly();
        }

        public override IEnumerator GetEnumerator() {
            return ((IEnumerable)_collection).GetEnumerator();
        }

        public override string GetKey(int index) {
            return _collection.GetKey(index);
        }

        [SuppressMessage("Microsoft.Security", "CA2114:MethodSecurityShouldBeASupersetOfType", Justification = "Workaround for FxCop Bug")]
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            _collection.GetObjectData(info, context);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override void OnDeserialization(object sender) {
            _collection.OnDeserialization(sender);
        }

    }
}
