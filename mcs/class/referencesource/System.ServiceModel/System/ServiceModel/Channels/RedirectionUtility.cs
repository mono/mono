//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    static class RedirectionUtility
    {
        public static bool IsNamespaceAndValueMatch(string value1, string namespace1, string value2, string namespace2)
        {
            bool result = false;

            if (IsNamespaceMatch(namespace1, namespace2) == true)
            {
                result = string.Equals(value1, value2, StringComparison.Ordinal);
            }

            return result;
        }

        //allows NULL values
        public static bool IsNamespaceMatch(string namespace1, string namespace2)
        {
            bool namespacesMatch = false;
            if (namespace1 == null && namespace2 == null)
            {
                namespacesMatch = true;
            }
            else if (namespace1 == null || namespace2 == null)
            {
                //one of them is null
                namespacesMatch = false;
            }
            else if (string.Equals(namespace1, namespace2, StringComparison.Ordinal))
            {
                //both are non-null and match
                namespacesMatch = true;
            }

            return namespacesMatch;
        }

        public static int ComputeHashCode(string value, string ns)
        {
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }

            string hashString = value + value.GetHashCode().ToString(Globalization.CultureInfo.InvariantCulture);

            if (!string.IsNullOrEmpty(ns))
            {
                hashString += ns;
            }

            return hashString.GetHashCode();
        }

    }
}
