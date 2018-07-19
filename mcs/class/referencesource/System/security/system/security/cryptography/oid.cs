// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

//
// Oid.cs
//

namespace System.Security.Cryptography {
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security.Cryptography.X509Certificates;

    // Values taken from wincrypt.h
    public enum OidGroup {
        All                     = 0,
        HashAlgorithm           = 1,
        EncryptionAlgorithm     = 2,
        PublicKeyAlgorithm      = 3,
        SignatureAlgorithm      = 4,
        Attribute               = 5,
        ExtensionOrAttribute    = 6,
        EnhancedKeyUsage        = 7,
        Policy                  = 8,
        Template                = 9,
        KeyDerivationFunction   = 10
    }
    
    public sealed class Oid {
        private string m_value = null;
        private string m_friendlyName = null;
        private OidGroup m_group = OidGroup.All;
        
        public Oid() { }

        public Oid(string oid) : this(oid, OidGroup.All, true)
        {
        }
        
        internal Oid(string oid, OidGroup group, bool lookupFriendlyName)
        {
            if (lookupFriendlyName)
            {
                // If we were passed the friendly name, retrieve the value string.
                string oidValue = X509Utils.FindOidInfoWithFallback(CAPI.CRYPT_OID_INFO_NAME_KEY, oid, group);
                if (oidValue == null)
                    oidValue = oid;
                this.Value = oidValue;
            }
            else
            {
                this.Value = oid;
            }

            m_group = group;
        }

        public Oid(string value, string friendlyName) {
            m_value = value;
            m_friendlyName = friendlyName;
        }

        public Oid(Oid oid) {
            if (oid == null)
                throw new ArgumentNullException("oid");
            m_value = oid.m_value;
            m_friendlyName = oid.m_friendlyName;
            m_group = oid.m_group;
        }

        private Oid(string value, string friendlyName, OidGroup group) {
            Debug.Assert(value != null);
            Debug.Assert(friendlyName != null);

            m_value = value;
            m_friendlyName = friendlyName;
            m_group = group;
        }

        public static Oid FromFriendlyName(string friendlyName, OidGroup group) {
            if (friendlyName == null) {
                throw new ArgumentNullException("friendlyName");
            }

            string oidValue = X509Utils.FindOidInfo(CAPI.CRYPT_OID_INFO_NAME_KEY, friendlyName, group);
            if (oidValue == null) {
                throw new CryptographicException(SR.GetString(SR.Cryptography_Oid_InvalidValue));
            }

            return new Oid(oidValue, friendlyName, group);
        }

        public static Oid FromOidValue(string oidValue, OidGroup group) {
            if (oidValue == null) {
                throw new ArgumentNullException("oidValue");
            }

            string friendlyName = X509Utils.FindOidInfo(CAPI.CRYPT_OID_INFO_OID_KEY, oidValue, group);
            if (friendlyName == null) {
                throw new CryptographicException(SR.GetString(SR.Cryptography_Oid_InvalidValue));
            }

            return new Oid(oidValue, friendlyName, group);
        }

        public string Value {
            get { return m_value; }
            set { m_value = value; }
        }

        public string FriendlyName {
            get {
                if(m_friendlyName == null && m_value != null)
                    m_friendlyName = X509Utils.FindOidInfoWithFallback(CAPI.CRYPT_OID_INFO_OID_KEY, m_value, m_group);
                
                return m_friendlyName;
            }
            set {
                m_friendlyName = value;
                // If we can find the matching OID value, then update it as well
                if (m_friendlyName != null) {
                    // If FindOidInfo fails, we return a null string
                    string oidValue = X509Utils.FindOidInfoWithFallback(CAPI.CRYPT_OID_INFO_NAME_KEY, m_friendlyName, m_group);
                    if (oidValue != null)
                        m_value = oidValue;
                }
            }
        }
    }

    public sealed class OidCollection : ICollection {
        private ArrayList m_list;

        public OidCollection() {
            m_list = new ArrayList();
        }

        public int Add(Oid oid) {
            return m_list.Add(oid);
        }

        public Oid this[int index] {
            get {
                return m_list[index] as Oid;
            }
        }

        // Indexer using an OID friendly name or value.
        public Oid this[string oid] {
            get {
                // If we were passed the friendly name, retrieve the value string.
                string oidValue = X509Utils.FindOidInfoWithFallback(CAPI.CRYPT_OID_INFO_NAME_KEY, oid, OidGroup.All);
                if (oidValue == null)
                    oidValue = oid;
                foreach (Oid entry in m_list) {
                    if (entry.Value == oidValue)
                        return entry;
                }
                return null;
            }
        }

        public int Count {
            get {
                return m_list.Count;
            }
        }

        public OidEnumerator GetEnumerator() {
            return new OidEnumerator(this);
        }

        /// <internalonly/>
        IEnumerator IEnumerable.GetEnumerator() {
            return new OidEnumerator(this);
        }

        /// <internalonly/>
        void ICollection.CopyTo(Array array, int index) {
            if (array == null)
                throw new ArgumentNullException("array");
            if (array.Rank != 1)
                throw new ArgumentException(SR.GetString(SR.Arg_RankMultiDimNotSupported));
            if (index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException("index", SR.GetString(SR.ArgumentOutOfRange_Index));
            if (index + this.Count > array.Length)
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidOffLen));

            for (int i=0; i < this.Count; i++) {
                array.SetValue(this[i], index);
                index++;
            }
        }

        public void CopyTo(Oid[] array, int index) {
            ((ICollection)this).CopyTo(array, index);
        }

        public bool IsSynchronized {
            get {
                return false;
            }
        }

        public Object SyncRoot {
            get {
                return this;
            }
        }
    }

    public sealed class OidEnumerator : IEnumerator {
        private OidCollection m_oids;
        private int m_current;

        private OidEnumerator() {}
        internal OidEnumerator(OidCollection oids) {
            m_oids = oids;
            m_current = -1;
        }
                
        public Oid Current {
            get {
                return m_oids[m_current];
            }
        }

        /// <internalonly/>
        Object IEnumerator.Current {
            get {
                return (Object) m_oids[m_current];
            }
        }
        
        public bool MoveNext() {
            if (m_current == ((int) m_oids.Count - 1))
                return false;
            m_current++;
            return true;
        }

        public void Reset() {
            m_current = -1;
        }
    }
}
