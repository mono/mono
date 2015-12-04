//---------------------------------------------------------------------
// <copyright file="MetadataItemCollectionFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  	willa
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Entity.Design
{
    using System.Collections.ObjectModel;
    using System.Data.Metadata.Edm;
    using System.Globalization;

    [CLSCompliant(false)]
    public static class MetadataExtensionMethods
    {
        /// <summary>
        /// Get the list of primitive types for the given version of Edm
        /// </summary>
        /// <param name="itemCollection">The item collection from which to retrieve the list of primitive types</param>
        /// <param name="edmVersion">The version of edm to use</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
        public static System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType> GetPrimitiveTypes(this EdmItemCollection itemCollection, Version edmVersion)
        {
            if (edmVersion == EntityFrameworkVersionsUtil.Version3)
            {
                return itemCollection.GetPrimitiveTypes(3.0);
            }
            else if (edmVersion == EntityFrameworkVersionsUtil.Version2)
            {
                return itemCollection.GetPrimitiveTypes(2.0);
            }
            else if (edmVersion == EntityFrameworkVersionsUtil.Version1)
            {
                return itemCollection.GetPrimitiveTypes(1.0);
            }
            else if (edmVersion == EntityFrameworkVersionsUtil.EdmVersion1_1)
            {
                return itemCollection.GetPrimitiveTypes(1.1);
            }
            else
            {
                string versionString = edmVersion.ToString(2);
                return itemCollection.GetPrimitiveTypes(double.Parse(versionString, CultureInfo.InvariantCulture));
            }
        }
    }
}
