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

    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldBeSpelledCorrectly, Justification = "Upn is an acronym")]
    [MarkupExtensionReturnType(typeof(UpnEndpointIdentity))]
    public class UpnEndpointIdentityExtension : MarkupExtension
    {
        public UpnEndpointIdentityExtension()
        {
        }

        public UpnEndpointIdentityExtension(UpnEndpointIdentity identity)
        {
            if (identity == null)
            {
                throw FxTrace.Exception.ArgumentNull("identity");
            }
            Fx.Assert(identity.IdentityClaim.Resource is string, "UpnEndpointIdentity claim resource is not string");
            this.UpnName = (string)identity.IdentityClaim.Resource;
        }

        [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldBeSpelledCorrectly, Justification = "Upn is an acronym")]
        public string UpnName
        { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new UpnEndpointIdentity(this.UpnName);
        }
    }
}
