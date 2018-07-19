//-----------------------------------------------------------------------
// <copyright file="UniqueId.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel
{
    using System;
    using System.Globalization;
    using System.IdentityModel;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Generates unique IDs.
    /// </summary>
    internal static class UniqueId
    {
        private const int RandomSaltSize = 16;

        // We use UUIDs as the basis for our unique identifiers. UUIDs
        // cannot be used in xml:id-typed fields, because xml:id 
        // is made from the NCNAME production in XML Schema.
        //
        // An NCNAME looks like this: (simlified out complex unicode)
        //     [A-Za-z_][A-Za-z0-9.-_]*
        //
        // A UUID looks like this:
        //     [0-9A-Fa-f]{8}-(?:[0-9A-Fa-f]{4}-){3}[0-9A-Fa-f]{12}
        //
        // The problem arises when the UUID begins with [0-9], which 
        // violates the NCNAME production.
        //
        // This is fixed trivially by prepending an underscore.
        private const string NcNamePrefix = "_";

        // In some cases we need UniqueId to be a valid URI. In this
        // case we use the urn:uuid: namespace established by 
        // RFC4122. Note that in this case it is not appropriate to 
        // use the auto-incrementing optimization, as the resulting
        // value is no longer properly a UUID. 
        private const string UuidUriPrefix = "urn:uuid:";

        // For non-random identifiers, we optimize the generation of 
        // unique ids by calculating only one UUID per program invocation
        // and adding a 64-bit auto-incrementing value for each id
        // that is needed. 
        private static readonly string reusableUuid = GetRandomUuid();

        // The format of the optimized NCNAMEs produced is:
        //     _[0-9A-Fa-f]{8}-(?:[0-9A-Fa-f]{4}-){3}[0-9A-Fa-f]{12}-[A-Za-z0-9]{8}
        //
        // In other words: underscore + UUID + hyphen + 64-bit auto-incrementing id
        private static readonly string optimizedNcNamePrefix = NcNamePrefix + reusableUuid + "-";

        /// <summary>
        /// Creates a unique ID suitable for use in an xml:id field. The value is 
        /// not hard to guess but is unique.
        /// </summary>
        /// <returns>The unique ID.</returns>
        public static string CreateUniqueId()
        {
            return optimizedNcNamePrefix + GetNextId();
        }

        /// <summary>
        /// Creates a unique ID similar to that created by CreateNonRandomId, 
        /// but instead of an underscore, the supplied prefix is used.
        /// </summary>
        /// <param name="prefix">The prefix to use.</param>
        /// <returns>The unique ID, with the given prefix.</returns>
        public static string CreateUniqueId(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("prefix");
            }

            return prefix + reusableUuid + "-" + GetNextId();
        }

        /// <summary>
        /// Creates a unique, random ID suitable for use in an xml:id field. The 
        /// value is hard to guess and unique.
        /// </summary>
        /// <returns>The unique ID.</returns>
        public static string CreateRandomId()
        {
            return NcNamePrefix + GetRandomUuid();
        }

        /// <summary>
        /// Creates a unique, random ID similar to that created by CreateRandomId,
        /// but instead of an underscore, the supplied prefix is used.
        /// </summary>
        /// <param name="prefix">The prefix to use.</param>
        /// <returns>The random URI.</returns>
        public static string CreateRandomId(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("prefix");
            }

            return prefix + GetRandomUuid();
        }

        /// <summary>
        /// Creates a unique, random ID suitable for use as a URI. The value is 
        /// hard to guess and unique. The URI is in the urn:uuid: namespace.
        /// </summary>
        /// <returns>The random URI.</returns>
        public static Uri CreateRandomUri()
        {
            return new Uri(UuidUriPrefix + GetRandomUuid());
        }

        private static string GetNextId()
        {
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] id = new byte[RandomSaltSize];
            rng.GetBytes(id);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < id.Length; i++)
            {
                builder.AppendFormat("{0:X2}", id[i]);
            }

            return builder.ToString();
        }

        private static string GetRandomUuid()
        {
            return Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture);
        }
    }
}
