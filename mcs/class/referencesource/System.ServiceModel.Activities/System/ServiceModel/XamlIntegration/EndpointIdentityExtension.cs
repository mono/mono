//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IdentityModel.Claims;
    using System.Runtime;
    using System.ServiceModel.Activities;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(EndpointIdentity))]
    public class EndpointIdentityExtension : MarkupExtension
    {
        public EndpointIdentityExtension()
        {
        }

        public EndpointIdentityExtension(EndpointIdentity identity)
        {
            if (identity == null)
            {
                throw FxTrace.Exception.ArgumentNull("identity");
            }
            this.ClaimType = identity.IdentityClaim.ClaimType;
            this.ClaimRight = identity.IdentityClaim.Right;
            this.ClaimResource = identity.IdentityClaim.Resource;
        }

        public string ClaimType
        { get; set; }

        public string ClaimRight
        { get; set; }

        public object ClaimResource
        { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            Claim claim = new Claim(this.ClaimType, this.ClaimResource, this.ClaimRight);
            return EndpointIdentity.CreateIdentity(claim);
        }
    }
}
