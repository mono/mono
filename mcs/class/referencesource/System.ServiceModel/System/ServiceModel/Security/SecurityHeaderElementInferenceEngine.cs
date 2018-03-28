//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.ServiceModel.Security.Tokens;
    using System.ServiceModel.Channels;
    using System.Xml;

    abstract class SecurityHeaderElementInferenceEngine
    {
        public abstract void ExecuteProcessingPasses(ReceiveSecurityHeader securityHeader, XmlDictionaryReader reader);

        public abstract void MarkElements(ReceiveSecurityHeaderElementManager elementManager, bool messageSecurityMode);

        public static SecurityHeaderElementInferenceEngine GetInferenceEngine(SecurityHeaderLayout layout)
        {
            SecurityHeaderLayoutHelper.Validate(layout);

            switch (layout)
            {
                case SecurityHeaderLayout.Strict:
                    return StrictModeSecurityHeaderElementInferenceEngine.Instance;
                case SecurityHeaderLayout.Lax:
                    return LaxModeSecurityHeaderElementInferenceEngine.Instance;
                case SecurityHeaderLayout.LaxTimestampFirst:
                    return LaxTimestampFirstModeSecurityHeaderElementInferenceEngine.Instance;
                case SecurityHeaderLayout.LaxTimestampLast:
                    return LaxTimestampLastModeSecurityHeaderElementInferenceEngine.Instance;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("layout"));
            }
        }
    }
}
