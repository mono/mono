// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;

namespace System.ComponentModel.Composition.Extensibility
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class CustomImportManyAttribute : ImportManyAttribute
    {
        public CustomImportManyAttribute()
        {
        }

        public CustomImportManyAttribute(Type type)
            : base(type)
        {
        }
    }
}
