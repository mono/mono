// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;

namespace System.ComponentModel.Composition.Extensibility
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class CustomImportAttribute : ImportAttribute
    {
        public CustomImportAttribute()
        {
        }

        public CustomImportAttribute(Type type)
            : base(type)
        {
        }
    }
}
