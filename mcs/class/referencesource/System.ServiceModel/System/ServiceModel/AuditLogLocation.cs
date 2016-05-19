//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.ServiceModel.Security;
    using System.ComponentModel;

    public enum AuditLogLocation
    {
        Default,
        Application,
        Security,
    }

    static class AuditLogLocationHelper
    {
        public static bool IsDefined(AuditLogLocation auditLogLocation)
        {
            if (auditLogLocation == AuditLogLocation.Security && !SecurityAuditHelper.IsSecurityAuditSupported)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new PlatformNotSupportedException(SR.GetString(SR.SecurityAuditPlatformNotSupported)));

            return auditLogLocation == AuditLogLocation.Default
                || auditLogLocation == AuditLogLocation.Application
                || auditLogLocation == AuditLogLocation.Security;
        }

        public static void Validate(AuditLogLocation value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value,
                    typeof(AuditLogLocation)));
            }
        }
    }
}
