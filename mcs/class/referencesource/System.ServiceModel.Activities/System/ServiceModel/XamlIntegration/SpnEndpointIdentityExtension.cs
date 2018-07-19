//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.XamlIntegration
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Windows.Markup;
    using System.ServiceModel.Activities;

    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldBeSpelledCorrectly, Justification = "Spn is an acronym")]
    [MarkupExtensionReturnType(typeof(SpnEndpointIdentity))]
    public class SpnEndpointIdentityExtension : MarkupExtension
    {
        public SpnEndpointIdentityExtension()
        {
        }

        public SpnEndpointIdentityExtension(SpnEndpointIdentity identity)
        {
            if (identity == null)
            {
                throw FxTrace.Exception.ArgumentNull("identity");
            }
            Fx.Assert(identity.IdentityClaim.Resource is string, "SpnEndpointIdentity claim resource is not string");
            this.SpnName = (string)identity.IdentityClaim.Resource;
        }

        [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldBeSpelledCorrectly, Justification = "Spn is an acronym")]
        public string SpnName
        {
            get;
            set;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new SpnEndpointIdentity(this.SpnName);
        }
    }
}
