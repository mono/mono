//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace Microsoft.Activities.Presentation.Xaml
{
    internal enum XamlTypeKind
    {
        Unknown, // I don't understand this type
        PartialSupported, // I understand this type if you would like to remove some new properties
        FullySupported, // I understand this type
    }
}
