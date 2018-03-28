//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.ComponentModel;

    public enum AuditLevel
    {
        None = 0,
        Success = 0x1,
        Failure = 0x2,
        SuccessOrFailure = Success | Failure,
    }

    static class AuditLevelHelper
    {
        public static bool IsDefined(AuditLevel auditLevel)
        {
            return auditLevel == AuditLevel.None
                || auditLevel == AuditLevel.Success
                || auditLevel == AuditLevel.Failure
                || auditLevel == AuditLevel.SuccessOrFailure;
        }

        public static void Validate(AuditLevel value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value,
                    typeof(AuditLevel)));
            }
        }

    }
}
