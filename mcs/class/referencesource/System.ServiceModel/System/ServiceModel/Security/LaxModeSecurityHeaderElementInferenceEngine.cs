//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    using SignedXml = System.IdentityModel.SignedXml;
    using StandardSignedInfo = System.IdentityModel.StandardSignedInfo;

    class LaxModeSecurityHeaderElementInferenceEngine : SecurityHeaderElementInferenceEngine
    {
        static LaxModeSecurityHeaderElementInferenceEngine instance = new LaxModeSecurityHeaderElementInferenceEngine();

        protected LaxModeSecurityHeaderElementInferenceEngine() { }

        internal static LaxModeSecurityHeaderElementInferenceEngine Instance
        {
            get { return instance; }
        }

        public override void ExecuteProcessingPasses(ReceiveSecurityHeader securityHeader, XmlDictionaryReader reader)
        {
            // pass 1
            securityHeader.ExecuteReadingPass(reader);

            // pass 1.5
            securityHeader.ExecuteDerivedKeyTokenStubPass(false);

            // pass 2
            securityHeader.ExecuteSubheaderDecryptionPass();

            // pass 2.5
            securityHeader.ExecuteDerivedKeyTokenStubPass(true);

            // layout-specific inferences
            MarkElements(securityHeader.ElementManager, securityHeader.RequireMessageProtection);

            // pass 3
            securityHeader.ExecuteSignatureEncryptionProcessingPass();
        }

        public override void MarkElements(ReceiveSecurityHeaderElementManager elementManager, bool messageSecurityMode)
        {
            bool primarySignatureFound = false;
            for (int position = 0; position < elementManager.Count; position++)
            {
                ReceiveSecurityHeaderEntry entry;
                elementManager.GetElementEntry(position, out entry);
                if (entry.elementCategory == ReceiveSecurityHeaderElementCategory.Signature)
                {
                    if (!messageSecurityMode)
                    {
                        elementManager.SetBindingMode(position, ReceiveSecurityHeaderBindingModes.Endorsing);
                        continue;
                    }
                    SignedXml signedXml = (SignedXml)entry.element;
                    StandardSignedInfo signedInfo = (StandardSignedInfo)signedXml.Signature.SignedInfo;
                    bool targetsSignature = false;
                    if (signedInfo.ReferenceCount == 1)
                    {
                        string uri = signedInfo[0].Uri;
                        string id;
                        if (uri != null && uri.Length > 1 && uri[0] == '#')
                        {
                            id = uri.Substring(1);
                        }
                        else
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new MessageSecurityException(SR.GetString(SR.UnableToResolveReferenceUriForSignature, uri)));
                        }
                        for (int j = 0; j < elementManager.Count; j++)
                        {
                            ReceiveSecurityHeaderEntry inner;
                            elementManager.GetElementEntry(j, out inner);
                            if (j != position && inner.elementCategory == ReceiveSecurityHeaderElementCategory.Signature && inner.id == id)
                            {
                                targetsSignature = true;
                                break;
                            }
                        }
                    }
                    if (targetsSignature)
                    {
                        elementManager.SetBindingMode(position, ReceiveSecurityHeaderBindingModes.Endorsing);
                        continue;
                    }
                    else
                    {
                        if (primarySignatureFound)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.AtMostOnePrimarySignatureInReceiveSecurityHeader)));
                        }
                        primarySignatureFound = true;
                        elementManager.SetBindingMode(position, ReceiveSecurityHeaderBindingModes.Primary);
                        continue;
                    }
                }
            }
        }
    }
}
