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

    using Microsoft.InfoCards.Diagnostics;
    using IDT = Microsoft.InfoCards.Diagnostics.InfoCardTrace;

    //
    // Summary:
    //  This is the managed representation of the native POLICY_ELEMENT struct.  This internal version
    //  knows how to Marshal itself and manages the native memory associated with a marshaled POLICY_ELEMENT.
    //
    internal class InternalPolicyElement : IDisposable
    {
        CardSpacePolicyElement m_element;

        NativePolicyElement m_nativeElement;

        IntPtr m_nativePtr;



        //
        // Parameters:
        //  target     - The target of the token being described.
        //  parameters - describes the type of token required by the target.
        //
        public InternalPolicyElement(CardSpacePolicyElement element)
        {
            m_nativePtr = IntPtr.Zero;
            if (null == element.Target)
            {
                throw IDT.ThrowHelperArgumentNull("PolicyElement.Target");
            }

            m_element = element;
        }

        public static int Size
        {
            get
            {
                return Marshal.SizeOf(typeof(NativePolicyElement));
            }
        }

        //
        // Summary:
        //  Marshals the PolicyElement to it's native format.
        //
        // Parameters:
        //  ptr  - A pointer to native memory in which to place the native format of the PolicyElement.  Must be
        //         a buffer atleast as large as this.Size.
        //
        public void DoMarshal(IntPtr ptr)
        {
            string target = m_element.Target.OuterXml;
            string issuer = "";

            IDT.DebugAssert(IntPtr.Zero == m_nativePtr, "Pointer already assigned");

            m_nativePtr = ptr;
            if (m_element.Issuer != null)
            {
                issuer = m_element.Issuer.OuterXml;
            }
            string tokenParameters = string.Empty;
            if (null != m_element.Parameters)
            {
                tokenParameters = CardSpaceSelector.XmlToString(m_element.Parameters);
            }

            m_nativeElement.targetEndpointAddress = target;
            m_nativeElement.issuerEndpointAddress = issuer;
            m_nativeElement.issuedTokenParameters = tokenParameters;
            m_nativeElement.policyNoticeLink = null != m_element.PolicyNoticeLink ? m_element.PolicyNoticeLink.ToString() : null;
            m_nativeElement.policyNoticeVersion = m_element.PolicyNoticeVersion;
            m_nativeElement.isManagedCardProvider = m_element.IsManagedIssuer;

            Marshal.StructureToPtr(m_nativeElement, ptr, false);

            return;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~InternalPolicyElement()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {

            if (IntPtr.Zero != m_nativePtr)
            {
                Marshal.DestroyStructure(m_nativePtr, typeof(NativePolicyElement));
                m_nativePtr = IntPtr.Zero;
            }
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

        }
    }
}
