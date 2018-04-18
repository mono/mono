//---------------------------------------------------------------------
// <copyright file="BuildProviderUtils.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;

namespace System.Data.Entity.Design.AspNet
{
    /// <summary>
    /// A place to put common methods used by our build providers
    /// </summary>
    /// 
    internal class BuildProviderUtils
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        private BuildProviderUtils()
        {
        }

        internal static void AddArtifactReference(AssemblyBuilder assemblyBuilder, BuildProvider prov, string virtualPath)
        {
            // add the artifact as a resource to the DLL
            using (Stream input = VirtualPathProvider.OpenFile(virtualPath))
            {
                // derive the resource name
                string name = BuildProviderUtils.GetResourceNameForVirtualPath(virtualPath);

                using (Stream resStream = assemblyBuilder.CreateEmbeddedResource(prov, name))
                {
                    int byteRead = input.ReadByte();
                    while (byteRead != -1)
                    {
                        resStream.WriteByte((byte)byteRead);
                        byteRead = input.ReadByte();
                    }
                }
            }
        }

        /// <summary>
        /// Transforms a virtual path string into a valid resource name.
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        internal static string GetResourceNameForVirtualPath(string virtualPath)
        {
            string name = VirtualPathUtility.ToAppRelative(virtualPath);
            Debug.Assert(name.StartsWith("~/", StringComparison.OrdinalIgnoreCase), "Expected app-relative path to start with ~/");
            if (name.StartsWith("~/", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(2);
            }

            name = name.Replace("/", ".");

            Debug.Assert(name.StartsWith(".", StringComparison.OrdinalIgnoreCase) == false, "resource name unexpectedly starts with .");

            return name;
        }

    }
}
