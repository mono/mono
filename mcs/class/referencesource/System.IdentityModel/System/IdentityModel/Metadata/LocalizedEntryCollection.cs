//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System.Collections.ObjectModel;
using System.Globalization;

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// A collection of <see cref="LocalizedEntry"/> objects.
    /// </summary>
    /// <typeparam name="T">The <see cref="LocalizedEntry"/> type.</typeparam>
    public class LocalizedEntryCollection<T> : KeyedCollection<CultureInfo, T> where T : LocalizedEntry
    {
        /// <summary>
        /// Gets the key for the <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The input item for which the key is to be retrieved.</param>
        /// <returns>A <see cref="CultureInfo"/> object representing the key for the <paramref name="item"/>.</returns>
        protected override CultureInfo GetKeyForItem(T item)
        {
            return item.Language;
        }
    }
}
