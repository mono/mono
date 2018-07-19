//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    public enum QueuedDeliveryRequirementsMode
    {
        Allowed,
        Required,
        NotAllowed,
    }

    static class QueuedDeliveryRequirementsModeHelper
    {
        static public bool IsDefined(QueuedDeliveryRequirementsMode x)
        {
            return
                x == QueuedDeliveryRequirementsMode.Allowed ||
                x == QueuedDeliveryRequirementsMode.Required ||
                x == QueuedDeliveryRequirementsMode.NotAllowed ||
                false;
        }
    }
}
