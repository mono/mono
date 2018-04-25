//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    sealed class StrictModeSecurityHeaderElementInferenceEngine : SecurityHeaderElementInferenceEngine
    {
        static StrictModeSecurityHeaderElementInferenceEngine instance = new StrictModeSecurityHeaderElementInferenceEngine();

        StrictModeSecurityHeaderElementInferenceEngine() { }

        internal static StrictModeSecurityHeaderElementInferenceEngine Instance
        {
            get { return instance; }
        }

        public override void ExecuteProcessingPasses(ReceiveSecurityHeader securityHeader, XmlDictionaryReader reader)
        {
            securityHeader.ExecuteFullPass(reader);
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
                    if (!messageSecurityMode || primarySignatureFound)
                    {
                        elementManager.SetBindingMode(position, ReceiveSecurityHeaderBindingModes.Endorsing);
                    }
                    else
                    {
                        elementManager.SetBindingMode(position, ReceiveSecurityHeaderBindingModes.Primary);
                        primarySignatureFound = true;
                    }
                }
            }
        }
    }
}
