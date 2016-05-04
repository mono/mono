//-----------------------------------------------------------------------
// <copyright file="StringUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel
{
    internal static class StringUtil
    {
        /// <summary>
        /// Returns an interned string equivaluent if found
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <returns>Optimized string.</returns>
        public static string OptimizeString(string value)
        {
            if (value != null)
            {
                string interned = string.IsInterned(value);
                if (interned != null)
                {
                    return interned;
                }
            }

            // If the requested string isn't found in the CLR internal pool, don't
            // intern it to avoid memory footprint ----up on the pool itself
            return value;
        }
    }
}
