//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Activation
{
    using System.ComponentModel;

    public enum AspNetCompatibilityRequirementsMode
    {
        NotAllowed,
        Allowed,
        Required,
    }

    static class AspNetCompatibilityRequirementsModeHelper
    {
        static public bool IsDefined(AspNetCompatibilityRequirementsMode x)
        {
            return
                x == AspNetCompatibilityRequirementsMode.NotAllowed ||
                x == AspNetCompatibilityRequirementsMode.Allowed ||
                x == AspNetCompatibilityRequirementsMode.Required ||
                false;
        }

        public static void Validate(AspNetCompatibilityRequirementsMode value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value,
                    typeof(AspNetCompatibilityRequirementsMode)));
            }
        }
    }
}
