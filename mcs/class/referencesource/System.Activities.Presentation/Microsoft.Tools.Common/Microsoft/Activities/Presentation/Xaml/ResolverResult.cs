//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace Microsoft.Activities.Presentation.Xaml
{
    using System.Collections.Generic;

    internal class ResolverResult
    {
        private static ResolverResult unknown = new ResolverResult(XamlTypeKind.Unknown);
        private static ResolverResult fullySupported = new ResolverResult(XamlTypeKind.FullySupported);

        public ResolverResult(XamlTypeKind kind)
            : this(kind, null)
        {
        }

        public ResolverResult(ICollection<string> newProperties)
            : this(XamlTypeKind.PartialSupported, newProperties)
        {
        }

        private ResolverResult(XamlTypeKind kind, ICollection<string> newProperties)
        {
            SharedFx.Assert(kind != XamlTypeKind.PartialSupported || newProperties != null, "newProperties should not be null when kind is XamlTypeKind.PartialSupported");

            this.Kind = kind;
            this.NewProperties = newProperties;
        }

        public static ResolverResult Unknown
        {
            get { return unknown; }
        }

        public static ResolverResult FullySupported
        {
            get { return fullySupported; }
        }

        public XamlTypeKind Kind { get; private set; }

        public ICollection<string> NewProperties { get; private set; }
    }
}
