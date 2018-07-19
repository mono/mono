//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Xml;
    using System.Collections.Generic;
    using System.ServiceModel.Security;
    using System.Globalization;

    sealed class RequestSecurityTokenResponseCollection : BodyWriter
    {
        IEnumerable<RequestSecurityTokenResponse> rstrCollection;
        SecurityStandardsManager standardsManager;

        public RequestSecurityTokenResponseCollection(IEnumerable<RequestSecurityTokenResponse> rstrCollection)
            : this(rstrCollection, SecurityStandardsManager.DefaultInstance)
        { }

        public RequestSecurityTokenResponseCollection(IEnumerable<RequestSecurityTokenResponse> rstrCollection, SecurityStandardsManager standardsManager)
            : base(true)
        {
            if (rstrCollection == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstrCollection");
            int index = 0;
            foreach (RequestSecurityTokenResponse rstr in rstrCollection)
            {
                if (rstr == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(String.Format(CultureInfo.InvariantCulture, "rstrCollection[{0}]", index));
                ++index;
            }
            this.rstrCollection = rstrCollection;
            if (standardsManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("standardsManager"));
            }
            this.standardsManager = standardsManager;
        }

        public IEnumerable<RequestSecurityTokenResponse> RstrCollection
        {
            get
            {
                return this.rstrCollection;
            }
        }

        public void WriteTo(XmlWriter writer)
        {
            this.standardsManager.TrustDriver.WriteRequestSecurityTokenResponseCollection(this, writer);
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            WriteTo(writer);
        }
    }
}
