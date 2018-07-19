//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    sealed class LaxTimestampFirstModeSecurityHeaderElementInferenceEngine : LaxModeSecurityHeaderElementInferenceEngine
    {
        static LaxTimestampFirstModeSecurityHeaderElementInferenceEngine instance = new LaxTimestampFirstModeSecurityHeaderElementInferenceEngine();

        LaxTimestampFirstModeSecurityHeaderElementInferenceEngine() { }

        internal new static LaxTimestampFirstModeSecurityHeaderElementInferenceEngine Instance
        {
            get { return instance; }
        }

        public override void MarkElements(ReceiveSecurityHeaderElementManager elementManager, bool messageSecurityMode)
        {
            for (int position = 1; position < elementManager.Count; position++)
            {
                if (elementManager.GetElementCategory(position) == ReceiveSecurityHeaderElementCategory.Timestamp)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TimestampMustOccurFirstInSecurityHeaderLayout)));
                }
            }
            base.MarkElements(elementManager, messageSecurityMode);
        }
    }
}
