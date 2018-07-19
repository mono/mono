//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

#pragma warning disable 1634, 1691 // Stops compiler from warning about unknown warnings (for Presharp)

namespace System
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class UriTemplateEquivalenceComparer : IEqualityComparer<UriTemplate>
    {
        static UriTemplateEquivalenceComparer instance;
        internal static UriTemplateEquivalenceComparer Instance
        {
            get
            {
                if (instance == null)
                {
                    // lock-free, fine if we allocate more than one
                    instance = new UriTemplateEquivalenceComparer();
                }
                return instance;
            }
        }

        public bool Equals(UriTemplate x, UriTemplate y)
        {
            if (x == null)
            {
                return y == null;
            }
            return x.IsEquivalentTo(y);
        }
        public int GetHashCode(UriTemplate obj)
        {
            if (obj == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("obj");
            }
#pragma warning disable 56506 // obj.xxx is never null
            // prefer final literal segment (common literal prefixes are common in some scenarios)
            for (int i = obj.segments.Count - 1; i >= 0; --i)
            {
                if (obj.segments[i].Nature == UriTemplatePartType.Literal)
                {
                    return obj.segments[i].GetHashCode();
                }
            }
            return obj.segments.Count + obj.queries.Count;
#pragma warning restore 56506
        }
    }
}
