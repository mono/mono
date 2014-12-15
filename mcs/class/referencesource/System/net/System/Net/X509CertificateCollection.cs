// ------------------------------------------------------------------------------
// <copyright file="X509CertificateCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright> 
// ------------------------------------------------------------------------------
// 
namespace System.Security.Cryptography.X509Certificates {
    using System;
    using System.Collections;
    
    
    [Serializable()]
    public class X509CertificateCollection : CollectionBase {
        
        public X509CertificateCollection() {
        }
        
        public X509CertificateCollection(X509CertificateCollection value) {
            this.AddRange(value);
        }
        
        public X509CertificateCollection(X509Certificate[] value) {
            this.AddRange(value);
        }
        
        public X509Certificate this[int index] {
            get {
                return ((X509Certificate)(List[index]));
            }
            set {
                List[index] = value;
            }
        }
        
        public int Add(X509Certificate value) {
            return List.Add(value);
        }
        
        public void AddRange(X509Certificate[] value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; (i < value.Length); i = (i + 1)) {
                this.Add(value[i]);
            }
        }
        
        public void AddRange(X509CertificateCollection value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; (i < value.Count); i = (i + 1)) {
                this.Add(value[i]);
            }
        }
        
        public bool Contains(X509Certificate value) {
            foreach (X509Certificate cert in List) {
                if (cert.Equals(value)) {
                    return true;
                }
            }
            return false;
        }
        
        public void CopyTo(X509Certificate[] array, int index) {
            List.CopyTo(array, index);
        }
        
        public int IndexOf(X509Certificate value) {
            return List.IndexOf(value);
        }
        
        public void Insert(int index, X509Certificate value) {
            List.Insert(index, value);
        }
        
        public new X509CertificateEnumerator GetEnumerator() {
            return new X509CertificateEnumerator(this);
        }
        
        public void Remove(X509Certificate value) {
            List.Remove(value);
        }

        public override int GetHashCode() {
            int hashCode = 0;

            foreach (X509Certificate cert in this) {                
                hashCode += cert.GetHashCode();  
            }

            return hashCode;
        }
        
        public class X509CertificateEnumerator : object, IEnumerator {
            
            private IEnumerator baseEnumerator;
            
            private IEnumerable temp;
            
            public X509CertificateEnumerator(X509CertificateCollection mappings) {
                this.temp = ((IEnumerable)(mappings));
                this.baseEnumerator = temp.GetEnumerator();
            }
            
            public X509Certificate Current {
                get {
                    return ((X509Certificate)(baseEnumerator.Current));
                }
            }
            
            /// <internalonly/>
            object IEnumerator.Current {
                get {
                    return baseEnumerator.Current;
                }
            }
            
            public bool MoveNext() {
                return baseEnumerator.MoveNext();
            }
            
            /// <internalonly/>
            bool IEnumerator.MoveNext() {
                return baseEnumerator.MoveNext();
            }
            
            public void Reset() {
                baseEnumerator.Reset();
            }
            
            /// <internalonly/>
            void IEnumerator.Reset() {
                baseEnumerator.Reset();
            }
        }
    }
}
