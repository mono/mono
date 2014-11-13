//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.ComponentModel;

    public enum SecurityTokenAttachmentMode
    {
        Signed,
        Endorsing,
        SignedEndorsing,
        SignedEncrypted
    }

    static class SecurityTokenAttachmentModeHelper
    {
        internal static bool IsDefined(SecurityTokenAttachmentMode value)
        {
            return value == SecurityTokenAttachmentMode.Endorsing
                || value == SecurityTokenAttachmentMode.Signed
                || value == SecurityTokenAttachmentMode.SignedEncrypted
                || value == SecurityTokenAttachmentMode.SignedEndorsing;
        }

        internal static void Validate(SecurityTokenAttachmentMode value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value,
                    typeof(SecurityTokenAttachmentMode)));
            }
        }

        internal static void Categorize(SecurityTokenAttachmentMode value,
            out bool isBasic, out bool isSignedButNotBasic, out ReceiveSecurityHeaderBindingModes mode)
        {

            SecurityTokenAttachmentModeHelper.Validate(value);

            switch (value)
            {
                case SecurityTokenAttachmentMode.Endorsing:
                    isBasic = false;
                    isSignedButNotBasic = false;
                    mode = ReceiveSecurityHeaderBindingModes.Endorsing;
                    break;
                case SecurityTokenAttachmentMode.Signed:
                    isBasic = false;
                    isSignedButNotBasic = true;
                    mode = ReceiveSecurityHeaderBindingModes.Signed;
                    break;
                case SecurityTokenAttachmentMode.SignedEncrypted:
                    isBasic = true;
                    isSignedButNotBasic = false;
                    mode = ReceiveSecurityHeaderBindingModes.Basic;
                    break;
                case SecurityTokenAttachmentMode.SignedEndorsing:
                    isBasic = false;
                    isSignedButNotBasic = true;
                    mode = ReceiveSecurityHeaderBindingModes.SignedEndorsing;
                    break;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
            }
        }
    }
}
