//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Xaml
{
    using System.Xaml;

    // Class that takes a non-public type and acts as if it were public.
    // Allows us to save ErrorActivitys without making that type public.
    class ShimAsPublicXamlType : XamlType
    {
        public ShimAsPublicXamlType(Type type, XamlSchemaContext schemaContext) :
            base(type, schemaContext)
        {
        }

        protected override bool LookupIsPublic()
        {
            return true;
        }
    }
}
