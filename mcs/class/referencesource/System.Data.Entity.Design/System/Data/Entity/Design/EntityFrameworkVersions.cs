//---------------------------------------------------------------------
// <copyright file="EntityFrameworkVersions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Data.Metadata.Edm;
using System.Data.Entity.Design.Common;
using System.IO;
using System.Data.Mapping;
using System.Data.EntityModel.SchemaObjectModel;
using System.Linq;
using System.Xml;

namespace System.Data.Entity.Design
{
    public static class EntityFrameworkVersions
    {
        public static readonly Version Version1 = EntityFrameworkVersionsUtil.Version1;
        public static readonly Version Version2 = EntityFrameworkVersionsUtil.Version2;
        public static readonly Version Version3 = EntityFrameworkVersionsUtil.Version3;

        internal static Version EdmVersion1_1 { get { return EntityFrameworkVersionsUtil.EdmVersion1_1; } }

        /// <summary>
        /// Returns the stream of the XSD corresponding to the frameworkVersion, and dataSpace passed in.
        /// </summary>
        /// <param name="entityFrameworkVersion">The version of the EntityFramework that you want the Schema XSD for.</param>
        /// <param name="dataSpace">The data space of the schem XSD that you want.</param>
        /// <returns>Stream version of the XSD</returns>
        public static Stream GetSchemaXsd(Version entityFrameworkVersion, DataSpace dataSpace)
        {
            EDesignUtil.CheckTargetEntityFrameworkVersionArgument(entityFrameworkVersion, "entityFrameworkVersion");

            string resourceName = null;
            switch(dataSpace)
            {
                case DataSpace.CSpace:
                    resourceName =  GetEdmSchemaXsdResourceName(entityFrameworkVersion);
                    break;
                case DataSpace.CSSpace:
                    resourceName =  GetMappingSchemaXsdResourceName(entityFrameworkVersion);
                    break;
                case DataSpace.SSpace:
                    resourceName =  GetStoreSchemaXsdResourceName(entityFrameworkVersion);
                    break;
                default:
                    throw EDesignUtil.Argument("dataSpace");
            }

            Debug.Assert(!string.IsNullOrEmpty(resourceName), "Did you forget to map something new?");

            Assembly dataEntity = typeof(EdmItemCollection).Assembly;
            return dataEntity.GetManifestResourceStream(resourceName);
        }

        private static string GetStoreSchemaXsdResourceName(Version entityFrameworkVersion)
        {
            Debug.Assert(IsValidVersion(entityFrameworkVersion), "Did you forget to check for valid versions before calling this private method?");
            Dictionary<string, XmlSchemaResource> map = new Dictionary<string, XmlSchemaResource>();
            XmlSchemaResource.AddStoreSchemaResourceMapEntries(map, GetEdmVersion(entityFrameworkVersion));
            return map[GetStoreSchemaNamespace(entityFrameworkVersion)].ResourceName;
            
        }

        private static string GetMappingSchemaXsdResourceName(Version entityFrameworkVersion)
        {
            Debug.Assert(IsValidVersion(entityFrameworkVersion), "Did you forget to check for valid versions before calling this private method?");
            Dictionary<string, XmlSchemaResource> map = new Dictionary<string, XmlSchemaResource>();
            XmlSchemaResource.AddMappingSchemaResourceMapEntries(map, GetEdmVersion(entityFrameworkVersion));
            return map[GetMappingSchemaNamespace(entityFrameworkVersion)].ResourceName;
        }

        private static double GetEdmVersion(Version entityFrameworkVersion)
        {
            Debug.Assert(IsValidVersion(entityFrameworkVersion), "Did you add a new version or forget to check the version");
            if (entityFrameworkVersion.Major == 1)
            {
                if (entityFrameworkVersion.Minor == 1)
                {
                    return XmlConstants.EdmVersionForV1_1;
                }
                else
                {
                    return XmlConstants.EdmVersionForV1;
                }
            }
            else if (entityFrameworkVersion.Major == 2)
            {
                return XmlConstants.EdmVersionForV2;
            }
            else
            {
                Debug.Assert(entityFrameworkVersion == EntityFrameworkVersions.Version3, "did you add a new version?");
                return XmlConstants.EdmVersionForV3;
            }
        }

        private static string GetEdmSchemaXsdResourceName(Version entityFrameworkVersion)
        {
            Debug.Assert(ValidVersions.Contains(entityFrameworkVersion), "Did you forget to check for valid versions before calling this private method?");
            Dictionary<string, XmlSchemaResource> map = new Dictionary<string, XmlSchemaResource>();
            XmlSchemaResource.AddEdmSchemaResourceMapEntries(map, GetEdmVersion(entityFrameworkVersion));
            return map[GetEdmSchemaNamespace(entityFrameworkVersion)].ResourceName;
        }

        internal static string GetSchemaNamespace(Version entityFrameworkVersion, DataSpace dataSpace)
        {
            Debug.Assert(IsValidVersion(entityFrameworkVersion), "Did you add a new version or forget to check the version");
            Debug.Assert(dataSpace == DataSpace.CSpace ||
                         dataSpace == DataSpace.CSSpace ||
                         dataSpace == DataSpace.SSpace, "only support the three spaces with an xml file format");
            switch (dataSpace)
            {
                case DataSpace.CSpace:
                    return GetEdmSchemaNamespace(entityFrameworkVersion);
                case DataSpace.SSpace:
                    return GetStoreSchemaNamespace(entityFrameworkVersion);
                default:
                    return GetMappingSchemaNamespace(entityFrameworkVersion);
            }
        }

        private static string GetStoreSchemaNamespace(Version entityFrameworkVersion)
        {
            Debug.Assert(ValidVersions.Contains(entityFrameworkVersion), "Did you forget to check for valid versions before calling this private method?");
            if (entityFrameworkVersion == EntityFrameworkVersions.Version1)
            {
                return XmlConstants.TargetNamespace_1;
            }
            else if (entityFrameworkVersion == EntityFrameworkVersions.Version2)
            {
                return XmlConstants.TargetNamespace_2;
            }
            else
            {
                Debug.Assert(entityFrameworkVersion == EntityFrameworkVersions.Version3, "did you add a new version?");
                return XmlConstants.TargetNamespace_3;
            }
        }

        private static string GetMappingSchemaNamespace(Version entityFrameworkVersion)
        {
            Debug.Assert(ValidVersions.Contains(entityFrameworkVersion), "Did you forget to check for valid versions before calling this private method?");
            if (entityFrameworkVersion == EntityFrameworkVersions.Version1)
            {
                return StorageMslConstructs.NamespaceUriV1;
            }
            else if (entityFrameworkVersion == EntityFrameworkVersions.Version2)
            {
                return StorageMslConstructs.NamespaceUriV2;
            }
            else
            {
                Debug.Assert(entityFrameworkVersion == EntityFrameworkVersions.Version3, "did you add a new version?");
                return StorageMslConstructs.NamespaceUriV3;
            }
        }

        private static string GetEdmSchemaNamespace(Version entityFrameworkVersion)
        {
            Debug.Assert(ValidVersions.Contains(entityFrameworkVersion), "Did you forget to check for valid versions before calling this private method?");
            if (entityFrameworkVersion == EntityFrameworkVersions.Version1)
            {
                return XmlConstants.ModelNamespace_1;
            }
            else if (entityFrameworkVersion == EntityFrameworkVersions.Version2)
            {
                return XmlConstants.ModelNamespace_2;
            }
            else
            {
                Debug.Assert(entityFrameworkVersion == EntityFrameworkVersions.Version3, "did you add a new version?");
                return XmlConstants.ModelNamespace_3;
            }
        }

        internal static Version Default = Version2;
        internal static Version Latest = Version3;
        internal static Version[] ValidVersions = new Version[] { Version1, Version2, Version3 };
        internal static bool IsValidVersion(Version entityFrameworkVersion)
        {
            return ValidVersions.Contains(entityFrameworkVersion);
        }

        // this method will skip down to the first element, or to the end if it doesn't find one
        internal static bool TryGetEdmxVersion(XmlReader reader, out Version entityFrameworkVersion)
        {
            // to make life simpler, we skip down to the first/root element, unless we're
            // already there
            if (!reader.EOF && reader.NodeType != XmlNodeType.Element)
            {
                while (reader.Read() && reader.NodeType != XmlNodeType.Element)
                {
                }
            }

            if (!reader.EOF &&
                (reader.LocalName == EntityDesignerUtils.EdmxRootElementName))
            {
                return TryGetEdmxVersion(reader.NamespaceURI, out entityFrameworkVersion);
            }

            entityFrameworkVersion = default(Version);
            return false;
        }

        internal static bool TryGetEdmxVersion(string xmlNamespaceName, out Version entityFrameworkVersion)
        {
            switch (xmlNamespaceName)
            {
                case EntityDesignerUtils.EdmxNamespaceUriV1:
                    entityFrameworkVersion = EntityFrameworkVersions.Version1;
                    return true;
                case EntityDesignerUtils.EdmxNamespaceUriV2:
                    entityFrameworkVersion = EntityFrameworkVersions.Version2;
                    return true;
                case EntityDesignerUtils.EdmxNamespaceUriV3:
                    entityFrameworkVersion = EntityFrameworkVersions.Version3;
                    return true;
                default:
                    entityFrameworkVersion = default(Version);
                    return false;
            }
        }

    }
}
