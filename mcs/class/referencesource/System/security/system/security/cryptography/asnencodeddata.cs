// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

//
// AsnEncodedData.cs
//

namespace System.Security.Cryptography {
    using System.Collections;
    using System.Globalization;
    using System.Runtime.InteropServices;

    public class AsnEncodedData {
        internal Oid m_oid = null;
        internal byte[] m_rawData = null;

        internal AsnEncodedData (Oid oid) {
            m_oid = oid;
        }

        internal AsnEncodedData (string oid, CAPI.CRYPTOAPI_BLOB encodedBlob) : this(oid, CAPI.BlobToByteArray(encodedBlob)) {}
        internal AsnEncodedData (Oid oid, CAPI.CRYPTOAPI_BLOB encodedBlob) : this(oid, CAPI.BlobToByteArray(encodedBlob)) {}

        protected AsnEncodedData () {}

        public AsnEncodedData (byte[] rawData) {
            Reset(null, rawData);
        }

        public AsnEncodedData (string oid, byte[] rawData) {
            Reset(new Oid(oid), rawData);
        }

        public AsnEncodedData (Oid oid, byte[] rawData) {
            Reset(oid, rawData);
        }

        public AsnEncodedData (AsnEncodedData asnEncodedData) {
            if (asnEncodedData == null)
                throw new ArgumentNullException("asnEncodedData");
            Reset(asnEncodedData.m_oid, asnEncodedData.m_rawData);
        }

        public Oid Oid {
            get {
                return m_oid;
            }
            set {
                if (value == null)
                    m_oid = null;
                else
                    m_oid = new Oid(value);
            }
        }

        public byte[] RawData {
            get {
                return m_rawData;
            }
            set {
                if (value == null)
                    throw new ArgumentNullException("value");
                m_rawData = (byte[]) value.Clone();
            }
        }

        public virtual void CopyFrom (AsnEncodedData asnEncodedData) {
            if (asnEncodedData == null)
                throw new ArgumentNullException("asnEncodedData");
            Reset(asnEncodedData.m_oid, asnEncodedData.m_rawData);
        }

        public virtual string Format (bool multiLine) {
            // Return empty string if no data to format.
            if (m_rawData == null || m_rawData.Length == 0)
                return String.Empty;

            // If OID is not present, then we can force CryptFormatObject 
            // to use hex formatting by providing an empty OID string.
            string oidValue = String.Empty;
            if (m_oid != null && m_oid.Value != null)
                oidValue = m_oid.Value;

            return CAPI.CryptFormatObject(CAPI.X509_ASN_ENCODING, 
                                          multiLine ? CAPI.CRYPT_FORMAT_STR_MULTI_LINE : 0,
                                          oidValue,
                                          m_rawData);
        }

        private void Reset (Oid oid, byte[] rawData) {
            this.Oid = oid;
            this.RawData = rawData;
        }
    }

    public sealed class AsnEncodedDataCollection : ICollection {
        private ArrayList m_list = null;
        private Oid m_oid = null;

        public AsnEncodedDataCollection () {
            m_list = new ArrayList();
            m_oid = null;
        }

        public AsnEncodedDataCollection(AsnEncodedData asnEncodedData) : this() {
            m_list.Add(asnEncodedData);
        }

        public int Add (AsnEncodedData asnEncodedData) {
            if (asnEncodedData == null)
                throw new ArgumentNullException("asnEncodedData");

            //
            // If m_oid is not null, then OIDs must match.
            //
            if (m_oid != null)  {
                string szOid1 = m_oid.Value;
                string szOid2 = asnEncodedData.Oid.Value;

                if (szOid1 != null && szOid2 != null) {
                    // Both are not null, so make sure OIDs match.
                    if (String.Compare(szOid1, szOid2, StringComparison.OrdinalIgnoreCase) != 0)
                        throw new CryptographicException(SR.GetString(SR.Cryptography_Asn_MismatchedOidInCollection));
                }
                else if (szOid1 != null || szOid2 != null) {
                    // Can't be matching, since only one of them is null.
                    throw new CryptographicException(SR.GetString(SR.Cryptography_Asn_MismatchedOidInCollection));
                }
            }

            return m_list.Add(asnEncodedData);
        }

        public void Remove (AsnEncodedData asnEncodedData) {
            if (asnEncodedData == null)
                throw new ArgumentNullException("asnEncodedData");
            m_list.Remove(asnEncodedData);
        }

        public AsnEncodedData this[int index] {
            get {
                return (AsnEncodedData) m_list[index];
            }
        }

        public int Count {
            get {
                return m_list.Count;
            }
        }

        public AsnEncodedDataEnumerator GetEnumerator() {
            return new AsnEncodedDataEnumerator(this);
        }

        /// <internalonly/>
        IEnumerator IEnumerable.GetEnumerator() {
            return new AsnEncodedDataEnumerator(this);
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

        public void CopyTo(AsnEncodedData[] array, int index) {
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

    public sealed class AsnEncodedDataEnumerator : IEnumerator {
        private AsnEncodedDataCollection m_asnEncodedDatas;
        private int m_current;

        private AsnEncodedDataEnumerator() {}
        internal AsnEncodedDataEnumerator(AsnEncodedDataCollection asnEncodedDatas) {
            m_asnEncodedDatas = asnEncodedDatas;
            m_current = -1;
        }

        public AsnEncodedData Current {
            get {
                return m_asnEncodedDatas[m_current];
            }
        }

        /// <internalonly/>
        Object IEnumerator.Current {
            get {
                return (Object) m_asnEncodedDatas[m_current];
            }
        }

        public bool MoveNext() {
            if (m_current == ((int) m_asnEncodedDatas.Count - 1))
                return false;
            m_current++;
            return true;
        }

        public void Reset() {
            m_current = -1;
        }
    }
}
