//---------------------------------------------------------------------
// <copyright file="MetadataItemCollectionFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  	 Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Entity.Design
{
    using System.Data.Entity;
    using System.Data.EntityModel;
    using System.Xml;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Metadata.Edm;
    using System.Data.Mapping;
    using System.Data.Entity.Design.Common;
    using Microsoft.Build.Utilities;
    using System.Data.Entity.Design.SsdlGenerator;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Factory for creating ItemCollections. This class is to be used for 
    /// design time scenarios. The consumers of the methods in this class
    /// will get an error list instead of an exception if there are errors in schema files. 
    /// </summary>
    [CLSCompliant(false)]
    public static class MetadataItemCollectionFactory
    {
        /// <summary>
        /// Create an EdmItemCollection with the passed in parameters.
        /// Add any errors caused during the ItemCollection creation
        /// to the error list passed in.
        /// </summary>
        /// <param name="readers"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        public static EdmItemCollection CreateEdmItemCollection(IEnumerable<XmlReader> readers,
            out IList<EdmSchemaError> errors)
        {
            System.Collections.ObjectModel.ReadOnlyCollection<string> filePaths = null;
            return new EdmItemCollection(readers, filePaths, out errors);
        }

        /// <summary>
        /// Create an EdmItemCollection with the passed in parameters.
        /// Add any errors caused during the ItemCollection creation
        /// to the error list passed in.
        /// </summary>
        /// <param name="readers"></param>
        /// <param name="targetEntityFrameworkVersion"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        public static EdmItemCollection CreateEdmItemCollection(IEnumerable<XmlReader> readers,
            Version targetEntityFrameworkVersion, 
            out IList<EdmSchemaError> errors)
        {
            EDesignUtil.CheckTargetEntityFrameworkVersionArgument(targetEntityFrameworkVersion, "targetEntityFrameworkVersion");

            EdmItemCollection edmItemCollection = CreateEdmItemCollection(readers, out errors);
            if (!errors.Any(e => e.Severity == EdmSchemaErrorSeverity.Error))
            {
                ValidateActualVersionAgainstTarget(targetEntityFrameworkVersion, EntityFrameworkVersionsUtil.ConvertToVersion(edmItemCollection.EdmVersion), errors);
            }

            return edmItemCollection;
        }

        internal static bool ValidateActualVersionAgainstTarget(Version maxExpectedVersion, Version actualVersion, IList<EdmSchemaError> errors)
        {
            if (!(actualVersion <= maxExpectedVersion))
            {
                errors.Add(new EdmSchemaError(Strings.TargetVersionSchemaVersionMismatch(maxExpectedVersion, actualVersion), (int)ModelBuilderErrorCode.SchemaVersionHigherThanTargetVersion, EdmSchemaErrorSeverity.Error));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Create an StoreItemCollection with the passed in parameters.
        /// Add any errors caused during the ItemCollection creation
        /// to the error list passed in.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="readers"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public static StoreItemCollection CreateStoreItemCollection(IEnumerable<XmlReader> readers,
            out IList<EdmSchemaError> errors)
        {
            return new StoreItemCollection(readers, null, out errors);
        }

        /// <summary>
        /// Create an StoreItemCollection with the passed in parameters.
        /// Add any errors caused during the ItemCollection creation
        /// to the error list passed in.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="readers"></param>
        /// <param name="targetEntityFrameworkVersion"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public static StoreItemCollection CreateStoreItemCollection(
            IEnumerable<XmlReader> readers,
            Version targetEntityFrameworkVersion,
            out IList<EdmSchemaError> errors)
        {
            EDesignUtil.CheckTargetEntityFrameworkVersionArgument(targetEntityFrameworkVersion, "targetEntityFrameworkVersion");
            return CreateStoreItemCollection(readers, out errors);
        }


        /// <summary>
        /// Create a StorageMappingItemCollection with the passed in parameters.
        /// Add any errors caused during the ItemCollection creation
        /// to the error list passed in.
        /// </summary>
        /// <param name="edmCollection"></param>
        /// <param name="storeCollection"></param>
        /// <param name="readers"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
        public static StorageMappingItemCollection CreateStorageMappingItemCollection(EdmItemCollection edmCollection,
            StoreItemCollection storeCollection, IEnumerable<XmlReader> readers, out IList<EdmSchemaError> errors)
        {
            return new StorageMappingItemCollection(edmCollection, storeCollection, readers, null, out errors);
        }

        /// <summary>
        /// Create a StorageMappingItemCollection with the passed in parameters.
        /// Add any errors caused during the ItemCollection creation
        /// to the error list passed in.
        /// </summary>
        /// <param name="edmCollection"></param>
        /// <param name="storeCollection"></param>
        /// <param name="readers"></param>
        /// <param name="targetEntityFrameworkVersion"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
        public static StorageMappingItemCollection CreateStorageMappingItemCollection(
            EdmItemCollection edmCollection,
            StoreItemCollection storeCollection, 
            IEnumerable<XmlReader> readers, 
            Version targetEntityFrameworkVersion, 
            out IList<EdmSchemaError> errors)
        {
            EDesignUtil.CheckArgumentNull(edmCollection, "edmCollection");
            EDesignUtil.CheckArgumentNull(storeCollection, "storeCollection");
            EDesignUtil.CheckArgumentNull(readers, "readers");
            EDesignUtil.CheckTargetEntityFrameworkVersionArgument(targetEntityFrameworkVersion, "targetEntityFrameworkVersion");
            if (EntityFrameworkVersionsUtil.ConvertToVersion(edmCollection.EdmVersion) > targetEntityFrameworkVersion)
            {
                throw EDesignUtil.Argument("edmCollection");
            }

            StorageMappingItemCollection storageMappingItemCollection = CreateStorageMappingItemCollection(edmCollection, storeCollection, readers, out errors);
            if (!errors.Any(e => e.Severity == EdmSchemaErrorSeverity.Error))
            {
                ValidateActualVersionAgainstTarget(targetEntityFrameworkVersion, EntityFrameworkVersionsUtil.ConvertToVersion(storageMappingItemCollection.MappingVersion), errors);
            }
            return storageMappingItemCollection;
        }
    }
}
