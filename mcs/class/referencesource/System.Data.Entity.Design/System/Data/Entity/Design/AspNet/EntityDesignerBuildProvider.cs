//---------------------------------------------------------------------
// <copyright file="EntityDesignerBuildProvider.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.Hosting;
using System.Web.Compilation;
using System.Xml;
using System.Data.Metadata.Edm;

namespace System.Data.Entity.Design.AspNet
{
    /// <summary>
    /// The ASP .NET Build provider for the CSDL in ADO .NET
    /// </summary>
    /// 
    [BuildProviderAppliesTo(BuildProviderAppliesTo.Code)]
    public class EntityDesignerBuildProvider : System.Web.Compilation.BuildProvider
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public EntityDesignerBuildProvider()
        {
        }

        /// <summary>
        /// We want ASP .NET to always reset the app domain when we have to rebuild
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public override BuildProviderResultFlags GetResultFlags(CompilerResults results)
        {
            return BuildProviderResultFlags.ShutdownAppDomainOnChange;
        } 

        /// <summary>
        /// Extract the CSDL, SSDL and MSL nodes from the EDMX file and store them
        /// as embedded resources
        /// </summary>
        /// <param name="assemblyBuilder"></param>
        public override void GenerateCode(AssemblyBuilder assemblyBuilder)
        {
            using (StreamReader edmxInputStream = new StreamReader(VirtualPathProvider.OpenFile(base.VirtualPath)))
            {
                // load up an XML document representing the edmx file
                XmlElement conceptualSchemaElement;
                XmlElement mappingElement;
                XmlElement storageSchemaElement;
                string embedAsResourcePropertyValue;
                EntityDesignerUtils.ExtractConceptualMappingAndStorageNodes(edmxInputStream, out conceptualSchemaElement, out mappingElement, out storageSchemaElement, out embedAsResourcePropertyValue);

                if (null == conceptualSchemaElement)
                {
                    throw new XmlException("No Conceptual Schema node to embed as a resource", null, 0, 0);
                }

                if (null == storageSchemaElement)
                {
                    throw new XmlException("No Storage Schema node to embed as a resource", null, 0, 0);
                }

                if (null == mappingElement)
                {
                    throw new XmlException("No Mapping node to embed as a resource", null, 0, 0);
                }

                // construct output paths where the CSDL/MSL/SSDL resources will be placed
                string virtualPathPrefix = base.VirtualPath.Replace(EntityDesignerUtils._edmxFileExtension, String.Empty);
                string csdlResourceName = BuildProviderUtils.GetResourceNameForVirtualPath(virtualPathPrefix + XmlConstants.CSpaceSchemaExtension);
                string ssdlResourceName = BuildProviderUtils.GetResourceNameForVirtualPath(virtualPathPrefix + XmlConstants.SSpaceSchemaExtension);
                string mslResourceName = BuildProviderUtils.GetResourceNameForVirtualPath(virtualPathPrefix + XmlConstants.CSSpaceSchemaExtension);

                SetupEmbeddedResource(assemblyBuilder, this, conceptualSchemaElement, csdlResourceName);
                SetupEmbeddedResource(assemblyBuilder, this, storageSchemaElement, ssdlResourceName);
                SetupEmbeddedResource(assemblyBuilder, this, mappingElement, mslResourceName);
            }

        }

        private static void SetupEmbeddedResource(AssemblyBuilder assemblyBuilder,
            BuildProvider prov, XmlElement xmlElement, string resourceName)
        {
            using (Stream resStream = assemblyBuilder.CreateEmbeddedResource(prov, resourceName))
            {
                EntityDesignerUtils.OutputXmlElementToStream(xmlElement, resStream);
            }
        }
    }
}
