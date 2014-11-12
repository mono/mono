// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
// HashMembershipCondition.cs
//
// Implementation of membership condition for hashes of assemblies.
//

namespace System.Security.Policy {
    using System.Collections;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Util;
    using System.Security.Permissions;
    using System.Threading;
    using System.Globalization;
    using System.Diagnostics.Contracts;

    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public sealed class HashMembershipCondition : ISerializable, IDeserializationCallback, IMembershipCondition, IReportMatchMembershipCondition {
        private byte[] m_value = null;
        private HashAlgorithm m_hashAlg = null;
        private SecurityElement m_element = null;

        private object s_InternalSyncObject = null;
        private object InternalSyncObject {
            get {
                if (s_InternalSyncObject == null) {
                    Object o = new Object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, o, null);
                }
                return s_InternalSyncObject;
            }
        }

        internal HashMembershipCondition() {}
        private HashMembershipCondition (SerializationInfo info, StreamingContext context) {
            m_value = (byte[]) info.GetValue("HashValue", typeof(byte[]));
            string hashAlgorithm = (string) info.GetValue("HashAlgorithm", typeof(string));
            if (hashAlgorithm != null)
                m_hashAlg = HashAlgorithm.Create(hashAlgorithm);
            else
                m_hashAlg = new SHA1Managed();
        }

        public HashMembershipCondition(HashAlgorithm hashAlg, byte[] value) {
            if (value == null)
                throw new ArgumentNullException("value");
            if (hashAlg == null)
                throw new ArgumentNullException("hashAlg");
            Contract.EndContractBlock();

            m_value = new byte[value.Length];
            Array.Copy(value, m_value, value.Length);
            m_hashAlg = hashAlg;
        }

        /// <internalonly/>
        [System.Security.SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("HashValue", this.HashValue);
            info.AddValue("HashAlgorithm", this.HashAlgorithm.ToString());
        }

        /// <internalonly/>
        void IDeserializationCallback.OnDeserialization (Object sender) {}

        public HashAlgorithm HashAlgorithm {
            set {
                if (value == null)
                    throw new ArgumentNullException("HashAlgorithm");
                Contract.EndContractBlock();
                m_hashAlg = value;
            }
            get {
                if (m_hashAlg == null && m_element != null)
                    ParseHashAlgorithm();
                return m_hashAlg;
            }
        }

        public byte[] HashValue {
            set {
                if (value == null)
                    throw new ArgumentNullException("value");
                Contract.EndContractBlock();
                m_value = new byte[value.Length];
                Array.Copy(value, m_value, value.Length);
            }
            get {
                if (m_value == null && m_element != null)
                    ParseHashValue();
                if (m_value == null)
                    return null;

                byte[] value = new byte[m_value.Length];
                Array.Copy(m_value, value, m_value.Length);
                return value;
            }
        }

        public bool Check(Evidence evidence) {
            object usedEvidence = null;
            return (this as IReportMatchMembershipCondition).Check(evidence, out usedEvidence);
        }

        bool IReportMatchMembershipCondition.Check(Evidence evidence, out object usedEvidence) {
            usedEvidence = null;

            if (evidence == null)
                return false;

            Hash hash = evidence.GetHostEvidence<Hash>();
            if (hash != null) {
                if (m_value == null && m_element != null)
                    ParseHashValue();

                if (m_hashAlg == null && m_element != null)
                    ParseHashAlgorithm();

                byte[] asmHash = null;
                lock (InternalSyncObject) {
                    asmHash = hash.GenerateHash(m_hashAlg);
                }

                if (asmHash != null && CompareArrays(asmHash, m_value)) {
                    usedEvidence = hash;
                    return true;
                }
            }

            return false;
        }

        public IMembershipCondition Copy() {
            if (m_value == null && m_element != null)
                ParseHashValue();

            if (m_hashAlg == null && m_element != null)
                ParseHashAlgorithm();

            return new HashMembershipCondition(m_hashAlg, m_value);
        }

        public SecurityElement ToXml() {
            return ToXml(null);
        }

        public void FromXml(SecurityElement e) {
            FromXml(e, null);
        }

        public SecurityElement ToXml(PolicyLevel level) {
            if (m_value == null && m_element != null)
                ParseHashValue();

            if (m_hashAlg == null && m_element != null)
                ParseHashAlgorithm();

            SecurityElement root = new SecurityElement("IMembershipCondition");
            XMLUtil.AddClassAttribute(root, this.GetType(), "System.Security.Policy.HashMembershipCondition");
            // If you hit this assert then most likely you are trying to change the name of this class. 
            // This is ok as long as you change the hard coded string above and change the assert below.
            Contract.Assert(this.GetType().FullName.Equals("System.Security.Policy.HashMembershipCondition"), "Class name changed!");

            root.AddAttribute("version", "1");
            if (m_value != null)
                root.AddAttribute(s_tagHashValue, Hex.EncodeHexString(HashValue));
            if (m_hashAlg != null)
                root.AddAttribute(s_tagHashAlgorithm, HashAlgorithm.GetType().FullName);
            return root;
        }

        public void FromXml(SecurityElement e, PolicyLevel level) {
            if (e == null)
                throw new ArgumentNullException("e");

            if (!e.Tag.Equals("IMembershipCondition"))
                throw new ArgumentException(Environment.GetResourceString("Argument_MembershipConditionElement"));
            Contract.EndContractBlock();

            lock (InternalSyncObject) {
                m_element = e;
                m_value = null;
                m_hashAlg = null;
            }
        }

        public override bool Equals(Object o) {
            HashMembershipCondition that = (o as HashMembershipCondition);
            if (that != null) {
                if (this.m_hashAlg == null && this.m_element != null)
                    this.ParseHashAlgorithm();
                if (that.m_hashAlg == null && that.m_element != null)
                    that.ParseHashAlgorithm();

                if (this.m_hashAlg != null && that.m_hashAlg != null &&
                    this.m_hashAlg.GetType() == that.m_hashAlg.GetType()) {
                    if (this.m_value == null && this.m_element != null)
                        this.ParseHashValue();
                    if (that.m_value == null && that.m_element != null)
                        that.ParseHashValue();

                    if (this.m_value.Length != that.m_value.Length)
                        return false;

                    for (int i = 0; i < m_value.Length; i++) {
                        if (this.m_value[i] != that.m_value[i])
                            return false;
                    }
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode() {
            if (this.m_hashAlg == null && this.m_element != null)
                this.ParseHashAlgorithm();

            int accumulator = this.m_hashAlg != null ? this.m_hashAlg.GetType().GetHashCode() : 0;
            if (this.m_value == null && this.m_element != null)
                this.ParseHashValue();

            accumulator = accumulator ^ GetByteArrayHashCode(this.m_value);
            return accumulator;
        }

        public override string ToString() {
            if (m_hashAlg == null)
                ParseHashAlgorithm();

            return Environment.GetResourceString("Hash_ToString", m_hashAlg.GetType().AssemblyQualifiedName, Hex.EncodeHexString(HashValue));
        }

        private const string s_tagHashValue = "HashValue";
        private const string s_tagHashAlgorithm = "HashAlgorithm";

        private void ParseHashValue() {
            lock (InternalSyncObject) {
                if (m_element == null)
                    return;

                string elHash = m_element.Attribute(s_tagHashValue);
                if (elHash != null)
                    m_value = Hex.DecodeHexString(elHash);
                else
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidXMLElement", s_tagHashValue, this.GetType().FullName));

                if (m_value != null && m_hashAlg != null) {
                    m_element = null;
                }
            }
        }

        private void ParseHashAlgorithm() {
            lock (InternalSyncObject) {
                if (m_element == null)
                    return;

                string elHashAlg = m_element.Attribute(s_tagHashAlgorithm);
                if (elHashAlg != null)
                    m_hashAlg = HashAlgorithm.Create(elHashAlg);
                else
                    m_hashAlg = new SHA1Managed();

                if (m_value != null && m_hashAlg != null)
                    m_element = null;
            }
        }

        private static bool CompareArrays(byte[] first, byte[] second) {
            if (first.Length != second.Length)
                return false;

            int count = first.Length;
            for (int i = 0; i < count; ++i) {
                if (first[i] != second[i])
                    return false;
            }

            return true;
        }

        private static int GetByteArrayHashCode(byte[] baData) {
            if (baData == null)
                return 0;

            int accumulator = 0;
            for (int i = 0; i < baData.Length; ++i) {
                accumulator = (accumulator << 8) ^ (int)baData[i] ^ (accumulator >> 24);
            }
            return accumulator;
        }
    }
}
