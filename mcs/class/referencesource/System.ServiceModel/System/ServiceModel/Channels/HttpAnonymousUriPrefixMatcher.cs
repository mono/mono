//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel.Description;
    using System.ServiceModel;

    class HttpAnonymousUriPrefixMatcher : IAnonymousUriPrefixMatcher
    {
        UriPrefixTable<Uri> anonymousUriPrefixes;

        internal HttpAnonymousUriPrefixMatcher()
        {
        }

        internal HttpAnonymousUriPrefixMatcher(HttpAnonymousUriPrefixMatcher objectToClone)
            : this()
        {
            if (objectToClone.anonymousUriPrefixes != null)
            {
                this.anonymousUriPrefixes = new UriPrefixTable<Uri>(objectToClone.anonymousUriPrefixes);
            }
        }

        public void Register(Uri anonymousUriPrefix)
        {
            if (anonymousUriPrefix == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("anonymousUriPrefix");
            }

            if (!anonymousUriPrefix.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("anonymousUriPrefix", SR.GetString(SR.UriMustBeAbsolute));
            }

            if (this.anonymousUriPrefixes == null)
            {
                this.anonymousUriPrefixes = new UriPrefixTable<Uri>(true);
            }

            if (!this.anonymousUriPrefixes.IsRegistered(new BaseUriWithWildcard(anonymousUriPrefix, HostNameComparisonMode.Exact)))
            {
                this.anonymousUriPrefixes.RegisterUri(anonymousUriPrefix, HostNameComparisonMode.Exact, anonymousUriPrefix);
            }
        }

        internal bool IsAnonymousUri(Uri to)
        {
            Fx.Assert(to == null || to.IsAbsoluteUri, SR.GetString(SR.UriMustBeAbsolute));

            if (this.anonymousUriPrefixes == null)
            {
                return false;
            }

            Uri returnValue;
            return this.anonymousUriPrefixes.TryLookupUri(to, HostNameComparisonMode.Exact, out returnValue);
        }
    }
}
