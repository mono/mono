//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RequiredArgumentAttribute : Attribute
    {
        public RequiredArgumentAttribute()
            : base()
        {
        }

        public override object TypeId
        {
            get
            {
                return this;
            }
        }
    }
}
