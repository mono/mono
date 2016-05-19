// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

//
// X509ChainElement.cs
//

namespace System.Security.Cryptography.X509Certificates {
    using System.Collections;
    using System.Runtime.InteropServices;

    public class X509ChainElement {
        private X509Certificate2 m_certificate;
        private X509ChainStatus[] m_chainStatus;
        private string m_description;

        private X509ChainElement () {}

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        internal unsafe X509ChainElement (IntPtr pChainElement) {
            CAPI.CERT_CHAIN_ELEMENT chainElement = new CAPI.CERT_CHAIN_ELEMENT(Marshal.SizeOf(typeof(CAPI.CERT_CHAIN_ELEMENT)));
            uint cbSize = (uint) Marshal.ReadInt32(pChainElement);
            if (cbSize > Marshal.SizeOf(chainElement))
                cbSize = (uint) Marshal.SizeOf(chainElement);
            X509Utils.memcpy(pChainElement, new IntPtr(&chainElement), cbSize);

            m_certificate = new X509Certificate2(chainElement.pCertContext);
            if (chainElement.pwszExtendedErrorInfo == IntPtr.Zero)
                m_description = String.Empty;
            else
                m_description = Marshal.PtrToStringUni(chainElement.pwszExtendedErrorInfo);

            // We give the user a reference to the array since we'll never access it.
            if (chainElement.dwErrorStatus == 0)
                m_chainStatus = new X509ChainStatus[0]; // empty array
            else
                m_chainStatus = X509Chain.GetChainStatusInformation(chainElement.dwErrorStatus);
        }

        public X509Certificate2 Certificate {
            get {
                return m_certificate;
            }
        }

        public X509ChainStatus[] ChainElementStatus {
            get {
                return m_chainStatus;
            }
        }

        public string Information {
            get {
                return m_description;
            }
        }
    }

    public sealed class X509ChainElementCollection : ICollection {
        private X509ChainElement[] m_elements;

        internal X509ChainElementCollection () {
            m_elements = new X509ChainElement[0];
        }

#if FEATURE_CORESYSTEM
        [SecuritySafeCritical]
#endif
        internal unsafe X509ChainElementCollection (IntPtr pSimpleChain) {
            CAPI.CERT_SIMPLE_CHAIN simpleChain = new CAPI.CERT_SIMPLE_CHAIN(Marshal.SizeOf(typeof(CAPI.CERT_SIMPLE_CHAIN)));
            uint cbSize = (uint) Marshal.ReadInt32(pSimpleChain);
            if (cbSize > Marshal.SizeOf(simpleChain))
                cbSize = (uint) Marshal.SizeOf(simpleChain);
            X509Utils.memcpy(pSimpleChain, new IntPtr(&simpleChain), cbSize);
            m_elements = new X509ChainElement[simpleChain.cElement];
            for (int index = 0; index < m_elements.Length; index++) {
                m_elements[index] = new X509ChainElement(Marshal.ReadIntPtr(new IntPtr((long) simpleChain.rgpElement + index * Marshal.SizeOf(typeof(IntPtr)))));
            }
        }

        public X509ChainElement this[int index] {
            get {
                if (index < 0)
                    throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_EnumNotStarted));
                if (index >= m_elements.Length)
                    throw new ArgumentOutOfRangeException("index", SR.GetString(SR.ArgumentOutOfRange_Index));
                return m_elements[index];
            }
        }

        public int Count {
            get {
                return m_elements.Length;
            }
        }

        public X509ChainElementEnumerator GetEnumerator() {
            return new X509ChainElementEnumerator(this);
        }

        /// <internalonly/>
        IEnumerator IEnumerable.GetEnumerator() {
            return new X509ChainElementEnumerator(this);
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

        public void CopyTo(X509ChainElement[] array, int index) {
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

    public sealed class X509ChainElementEnumerator : IEnumerator {
        private X509ChainElementCollection m_chainElements;
        private int m_current;

        private X509ChainElementEnumerator () {}
        internal X509ChainElementEnumerator (X509ChainElementCollection chainElements) {
            m_chainElements = chainElements;
            m_current = -1;
        }

        public X509ChainElement Current {
            get {
                return (X509ChainElement) m_chainElements[m_current];
            }
        }

        /// <internalonly/>
        Object IEnumerator.Current {
            get {
                return (Object) m_chainElements[m_current];
            }
        }
        
        public bool MoveNext() {
            if (m_current == ((int) m_chainElements.Count - 1))
                return false;
            m_current++;
            return true;
        }

        public void Reset() {
            m_current = -1;
        }
    }
}
