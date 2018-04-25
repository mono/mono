//------------------------------------------------------------------------------
// <copyright file="HttpStaticObjectsCollectionWrapper.cs" company="Microsoft">
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
    public class HttpStaticObjectsCollectionWrapper : HttpStaticObjectsCollectionBase {

        private HttpStaticObjectsCollection _collection;

        public HttpStaticObjectsCollectionWrapper(HttpStaticObjectsCollection httpStaticObjectsCollection) {
            if (httpStaticObjectsCollection == null) {
                throw new ArgumentNullException("httpStaticObjectsCollection");
            }
            _collection = httpStaticObjectsCollection;
        }

        public override int Count {
            get {
                return _collection.Count;
            }
        }

        public override bool IsReadOnly {
            get {
                return _collection.IsReadOnly;
            }
        }

        public override bool IsSynchronized {
            get {
                return _collection.IsSynchronized;
            }
        }

        public override object this[string name] {
            get {
                return _collection[name];
            }
        }

        public override bool NeverAccessed {
            get {
                return _collection.NeverAccessed;
            }
        }

        public override object SyncRoot {
            get {
                return _collection.SyncRoot;
            }
        }

        public override void CopyTo(Array array, int index) {
            _collection.CopyTo(array, index);
        }

        public override IEnumerator GetEnumerator() {
            return _collection.GetEnumerator();
        }

        public override object GetObject(string name) {
            return _collection.GetObject(name);
        }

        public override void Serialize(BinaryWriter writer) {
            _collection.Serialize(writer);
        }

    }
}
