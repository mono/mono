//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.Text;

    class WebEncodingValidator : ConfigurationValidatorBase
    {
        public override bool CanValidate(Type type)
        {
            return type == typeof(Encoding);
        }

        public override void Validate(object value)
        {
            Encoding encoding = value as Encoding;
            if ((encoding == null) ||
                // utf-8 case. EncodingConverter generates TextEncoderDefaults.Encoding for utf-8, different from System.Text.Encoding.UTF8
                ((encoding.WebName != Encoding.UTF8.WebName) &&
                (encoding.WebName != Encoding.Unicode.WebName) &&
                (encoding.WebName != Encoding.BigEndianUnicode.WebName)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR2.GetString(SR2.JsonEncodingNotSupported));
            }
        }
    }
}
