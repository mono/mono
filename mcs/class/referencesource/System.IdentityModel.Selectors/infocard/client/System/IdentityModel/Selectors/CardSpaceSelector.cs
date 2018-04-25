//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
//
// Presharp uses the c# pragma mechanism to supress its warnings.
// These are not recognised by the base compiler so we need to explictly
// disable the following warnings. See http://winweb/cse/Tools/PREsharp/userguide/default.asp 
// for details. 
//
#pragma warning disable 1634, 1691      // unknown message, unknown pragma

namespace System.IdentityModel.Selectors
{

    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.IdentityModel.Claims;
    using System.Text;
    using System.Xml;
    using System.IdentityModel.Tokens;
    using System.ServiceProcess;
    using System.Globalization;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.CompilerServices;
    using Microsoft.InfoCards.Diagnostics;
    using Microsoft.Win32;
    using IDT = Microsoft.InfoCards.Diagnostics.InfoCardTrace;


    //
    // For common & resources
    //
    using Microsoft.InfoCards;

    //
    // Summary
    //  This structure is the native version of the GenericXmlSecurityToken
    //
    // Remark
    //   When adding new fields to this structure, add pointers to the end to make 
    //   sure that alignment is done correctly
    //
    [StructLayout(LayoutKind.Sequential)]
    struct RpcGenericXmlToken
    {

        public Int64 createDate;              // Date the token was created on
        public Int64 expiryDate;              // Date the token will expire on
        [MarshalAs(UnmanagedType.LPWStr)]
        public string xmlToken;               // Token
        [MarshalAs(UnmanagedType.LPWStr)]
        public string internalTokenReference; // Internal Token reference
        [MarshalAs(UnmanagedType.LPWStr)]
        public string externalTokenReference; // External Token reference

    }

    //
    // Summary
    //  This class implements the client API for the Infocard system
    //
    public static class CardSpaceSelector
    {
        static CardSpaceShim s_cardSpaceShim = new CardSpaceShim();

        //
        // The default quotas we apply to incoming xml messages
        //
        private static XmlDictionaryReaderQuotas DefaultQuotas = new XmlDictionaryReaderQuotas();

        //
        // Used by infocard.exe as well.
        //
        internal const int MaxPolicyChainLength = 50;

        static CardSpaceSelector()
        {
            //
            // Quotas for xml readers
            //
            DefaultQuotas.MaxDepth = 32;               // max depth of elements
            DefaultQuotas.MaxStringContentLength = 8192;             // maximum string read
            DefaultQuotas.MaxArrayLength = 20 * 1024 * 1024; // maximum byte array 
            DefaultQuotas.MaxBytesPerRead = 4096;             // max start element tag
            DefaultQuotas.MaxNameTableCharCount = 16384;            // max size of name table

        }

        // Summary
        //  Request a security token from the infocard system
        //
        // Parameters
        //  endPoint                    -  The token recipient end point.
        //  policy                      -  Policy stating the requirements for the token.
        //  requiredRemoteTokenIssuer   -  The returned token should be issued by this 
        //                                 specific issuer.
        //
        public static GenericXmlSecurityToken GetToken(XmlElement endpoint,
                                                IEnumerable<XmlElement> policy,
                                                XmlElement requiredRemoteTokenIssuer,
                                                SecurityTokenSerializer tokenSerializer)
        {
            if (null == endpoint)
            {
                throw IDT.ThrowHelperArgumentNull("endpoint");
            }

            if (null == policy)
            {
                throw IDT.ThrowHelperArgumentNull("policy");
            }

            if (null == tokenSerializer)
            {
                throw IDT.ThrowHelperArgumentNull("tokenSerializer");
            }

            Collection<XmlElement> policyCollection = new Collection<XmlElement>();

            foreach (XmlElement element in policy)
            {
                policyCollection.Add(element);
            }

            return GetToken(new CardSpacePolicyElement[] { new CardSpacePolicyElement(endpoint, requiredRemoteTokenIssuer, policyCollection, null, 0, false) }, tokenSerializer);
        }


        // Summary
        //  Request a security token from the infocard system
        //
        // Parameters
        //  policyChain  - an array of PolicyElements that describe the federated security chain that the client
        //                 needs a final token to unwind.
        //
        public static GenericXmlSecurityToken GetToken(CardSpacePolicyElement[] policyChain, SecurityTokenSerializer tokenSerializer)
        {
            IDT.TraceDebug("ICARDCLIENT: GetToken called with a policy chain of length {0}", policyChain.Length);

            InfoCardProofToken proofToken = null;
            InternalRefCountedHandle nativeCryptoHandle = null;
            GenericXmlSecurityToken token = null;
            RpcGenericXmlToken infocardToken = new RpcGenericXmlToken();
            SafeTokenHandle nativeToken = null;
            Int32 result = 0;

            if (null == policyChain || 0 == policyChain.Length)
            {
                throw IDT.ThrowHelperArgumentNull("policyChain");
            }
            if (null == tokenSerializer)
            {
                throw IDT.ThrowHelperArgumentNull("tokenSerializer");
            }

            if (null == tokenSerializer)
            {
                throw IDT.ThrowHelperArgumentNull("tokenSerializer");
            }

            try
            {


                RuntimeHelpers.PrepareConstrainedRegions();
                bool mustRelease = false;
                try
                {
                }
                finally
                {
                    //
                    // The PolicyChain class will do the marshalling and native buffer management for us.
                    //
                    try
                    {
                        using (PolicyChain tmpChain = new PolicyChain(policyChain))
                        {

                            IDT.TraceDebug("ICARDCLIENT: PInvoking the native GetToken call");

                            result = GetShim().m_csShimGetToken(
                                                        tmpChain.Length,
                                                        tmpChain.DoMarshal(),
                                                        out nativeToken,
                                                        out nativeCryptoHandle);


                        }

                        if (0 == result)
                        {
                            IDT.TraceDebug("ICARDCLIENT: The PInvoke of GetToken succeeded");
                            nativeToken.DangerousAddRef(ref mustRelease);

                            infocardToken = (RpcGenericXmlToken)Marshal.PtrToStructure(
                                                                          nativeToken.DangerousGetHandle(),
                                                                          typeof(RpcGenericXmlToken));
                        }
                    }
                    finally
                    {
                        if (mustRelease)
                        {
                            nativeToken.DangerousRelease();
                        }
                    }

                }
                if (0 == result)
                {
                    using (ProofTokenCryptoHandle crypto =
                        (ProofTokenCryptoHandle)CryptoHandle.Create(nativeCryptoHandle))
                    {
                        proofToken = crypto.CreateProofToken();
                    }

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(infocardToken.xmlToken);
                    SecurityKeyIdentifierClause internalTokenReference = null;
                    if (null != infocardToken.internalTokenReference)
                    {
                        internalTokenReference = tokenSerializer.ReadKeyIdentifierClause(
                                           CreateReaderWithQuotas(infocardToken.internalTokenReference));
                    }
                    SecurityKeyIdentifierClause externalTokenReference = null;
                    if (null != infocardToken.externalTokenReference)
                    {

                        externalTokenReference = tokenSerializer.ReadKeyIdentifierClause(
                                CreateReaderWithQuotas(infocardToken.externalTokenReference));
                    }
                    IDT.TraceDebug("ICARDCLIENT: Constructing a new GenericXmlSecurityToken");
                    token = new GenericXmlSecurityToken(
                                             xmlDoc.DocumentElement,
                                             proofToken,
                                             DateTime.FromFileTimeUtc(infocardToken.createDate),
                                             DateTime.FromFileTimeUtc(infocardToken.expiryDate),
                                             internalTokenReference,
                                             externalTokenReference,
                                             null);
                }
                else
                {
                    IDT.TraceDebug("ICARDCLIENT: The PInvoke of GetToken failed with a return code of {0}", result);

                    //
                    // Convert the HRESULTS to exceptions
                    //
                    ExceptionHelper.ThrowIfCardSpaceException((int)result);
                    throw IDT.ThrowHelperError(new CardSpaceException(SR.GetString(SR.ClientAPIInfocardError)));
                }
            }
            catch
            {
                if (null != nativeCryptoHandle)
                {
                    nativeCryptoHandle.Dispose();
                }

                if (null != proofToken)
                {
                    proofToken.Dispose();
                }
                throw;
            }
            finally
            {
                if (null != nativeToken)
                {
                    nativeToken.Dispose();
                }
            }

            return token;
        }

        //
        // Summary
        //  Start the management user interface
        //
        public static void Manage()
        {
            Int32 result = CardSpaceSelector.GetShim().m_csShimManageCardSpace();

            //
            // Convert HRESULTS to errors
            //
            if (0 != result)
            {
                //
                // Convert the HRESULTS to exceptions
                //
                ExceptionHelper.ThrowIfCardSpaceException((int)result);
                throw IDT.ThrowHelperError(new CardSpaceException(SR.GetString(SR.ClientAPIInfocardError)));
            }

        }

        //
        // Summary
        //  Start the import card user interface
        //
        public static void Import(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                throw IDT.ThrowHelperArgumentNull("fileName");
            }


            IDT.TraceDebug("Import Infocard has been called");
            Int32 result = CardSpaceSelector.GetShim().m_csShimImportInformationCard(fileName);

            //
            // Convert HRESULTS to errors
            //
            if (0 != result)
            {
                //
                // Convert the HRESULTS to exceptions
                //
                ExceptionHelper.ThrowIfCardSpaceException((int)result);
                throw IDT.ThrowHelperError(new CardSpaceException(SR.GetString(SR.ClientAPIInfocardError)));
            }

        }

        internal static CardSpaceShim GetShim()
        {
            s_cardSpaceShim.InitializeIfNecessary();
            return s_cardSpaceShim;

        }

        //
        // Summary
        //  Convert the XML data to a string
        //
        // Parameter
        //  xml - The xml data to be converted into a string
        //
        // Returns
        //   A string format of the XML
        //
        internal static string XmlToString(IEnumerable<XmlElement> xml)
        {
            StringBuilder builder = new StringBuilder();

            foreach (XmlElement element in xml)
            {
                if (null == element)
                {
                    throw IDT.ThrowHelperError(new ArgumentException(SR.GetString(SR.ClientAPIInvalidPolicy)));
                }
                builder.Append(element.OuterXml);
            }

            return builder.ToString();
        }
        private static XmlDictionaryReader CreateReaderWithQuotas(string root)
        {
            UTF8Encoding utf8 = new UTF8Encoding();
            byte[] rootbytes = utf8.GetBytes(root);
            return XmlDictionaryReader.CreateTextReader(
                rootbytes, 0, rootbytes.GetLength(0), null, DefaultQuotas, null);
        }




    }
}
