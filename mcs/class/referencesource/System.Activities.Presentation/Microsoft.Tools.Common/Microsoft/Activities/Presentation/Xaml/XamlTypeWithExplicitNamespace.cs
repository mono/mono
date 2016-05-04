// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities.Presentation.Xaml
{
    using System.Collections.Generic;
    using System.Xaml;

    class XamlTypeWithExplicitNamespace : XamlType
    {
        List<string> explicitNamespaces;

        public XamlTypeWithExplicitNamespace(XamlType wrapped, IEnumerable<string> explicitNamespaces) :
            base(wrapped.UnderlyingType, wrapped.SchemaContext)
        {
            this.explicitNamespaces = new List<string>(explicitNamespaces);
        }

        public override IList<string> GetXamlNamespaces()
        {
            return this.explicitNamespaces;
        }
    }
}
