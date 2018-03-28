//------------------------------------------------------------------------------
// <copyright file="MembershipProviderCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security {

    using System;
    using System.Configuration.Provider;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    // This has no hosting permission demands because of DevDiv Bugs 31461: ClientAppSvcs: ASP.net Provider support
    [TypeForwardedFrom("System.Web, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public sealed class MembershipProviderCollection : ProviderCollection {

        public override void Add(ProviderBase provider) {
            if (provider == null) {
                throw new ArgumentNullException("provider");
            }

            if (!(provider is MembershipProvider)) {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, ApplicationServicesStrings.Provider_must_implement_type, typeof(MembershipProvider).ToString()), "provider");
            }

            base.Add(provider);
        }

        new public MembershipProvider this[string name] {
            get {
                return (MembershipProvider)base[name];
            }
        }

        public void CopyTo(MembershipProvider[] array, int index) {
            base.CopyTo(array, index);
        }
    }
}
