//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Hosting;
    using System.Runtime;
    using System.Xaml;
    using Microsoft.Activities.Presentation.Xaml;

    internal class MultiTargetingXamlSchemaContext : XamlSchemaContext
    {
        private MultiTargetingSupportService multiTargetingService;

        public MultiTargetingXamlSchemaContext(MultiTargetingSupportService multiTargetingService)
        {
            Fx.Assert(multiTargetingService != null, "multiTargetingService should not be null");

            this.multiTargetingService = multiTargetingService;
        }

        protected override XamlType GetXamlType(string xamlNamespace, string name, params XamlType[] typeArguments)
        {
            XamlType xamlType = base.GetXamlType(xamlNamespace, name, typeArguments);

            if (xamlType == null || xamlType.UnderlyingType == null)
            {
                return xamlType;
            }

            ResolverResult resolverResult = MultiTargetingTypeResolver.Resolve(this.multiTargetingService, xamlType.UnderlyingType);
            return MultiTargetingTypeResolver.GetXamlType(resolverResult, xamlType);
        }
    }
}
