//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.IdentityModel.Selectors
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;

    //
    // For common
    //
    using Microsoft.InfoCards;

    //
    // Summary:
    //  This class wraps and manages the lifetime of an array of PolicyElements that are to be Marshaled to 
    //  native memory.
    //
    internal class PolicyChain : IDisposable
    {
        HGlobalSafeHandle m_nativeChain;

        InternalPolicyElement[] m_chain;

        public int Length
        {
            get { return m_chain.Length; }
        }

        public PolicyChain(CardSpacePolicyElement[] elements)
        {
            int length = elements.Length;

            m_chain = new InternalPolicyElement[length];

            for (int i = 0; i < length; i++)
            {
                m_chain[i] = new InternalPolicyElement(elements[i]);
            }
        }

        public SafeHandle DoMarshal()
        {
            if (null == m_nativeChain)
            {
                int elementSize = InternalPolicyElement.Size;
                int chainLength = m_chain.Length;


                m_nativeChain = HGlobalSafeHandle.Construct(chainLength * elementSize);

                IntPtr pos = m_nativeChain.DangerousGetHandle();

                foreach (InternalPolicyElement element in m_chain)
                {
                    element.DoMarshal(pos);
                    unsafe
                    {
                        //
                        // All this just to do pos += elementSize
                        //
                        pos = new IntPtr((long)(((ulong)pos.ToPointer()) + (ulong)elementSize));
                    }
                }
            }

            return m_nativeChain;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~PolicyChain()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            if (null != m_chain)
            {
                foreach (InternalPolicyElement element in m_chain)
                {
                    if (null != element)
                    {
                        element.Dispose();
                    }
                }
                m_chain = null;
            }

            if (null != m_nativeChain)
            {
                m_nativeChain.Dispose();
            }
        }
    }
}
