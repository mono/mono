//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Converters
{
    using System;

    internal sealed class SearchableStringConverterAttribute : Attribute
    {
        string converterTypeName;
        public SearchableStringConverterAttribute(Type type)
        {
            this.converterTypeName = type.AssemblyQualifiedName;
        }
        public string ConverterTypeName
        {
            get
            {
                return this.converterTypeName;
            }
        }
    }
}
