//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Configuration
{
    using System;
    using System.Configuration;

    class TimeSpanOrInfiniteValidator : TimeSpanValidator
    {
        public TimeSpanOrInfiniteValidator(TimeSpan minValue, TimeSpan maxValue)
            : base(minValue, maxValue)
        {
        }

        public override void Validate(object value)
        {
            if (value.GetType() == typeof(TimeSpan) && (TimeSpan)value == TimeSpan.MaxValue)
            {
                return; // we're good
            }

            base.Validate(value);
        }
    }
}
