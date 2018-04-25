//---------------------------------------------------------------------
// <copyright file="MappingModelBuildProvider.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Globalization;
using System.Web;
using System.Web.Hosting;
using System.Web.Compilation;

namespace System.Data.Entity.Design.AspNet
{
    /// <summary>
    /// The ASP .NET Build provider for the MSL in ADO .NET
    /// </summary>
    /// 
    [BuildProviderAppliesTo(BuildProviderAppliesTo.Code)]
    public class MappingModelBuildProvider : System.Web.Compilation.BuildProvider
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MappingModelBuildProvider()
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
        /// 
        /// </summary>
        /// <param name="assemblyBuilder"></param>
        public override void GenerateCode(AssemblyBuilder assemblyBuilder)
        {
            BuildProviderUtils.AddArtifactReference(assemblyBuilder, this, base.VirtualPath);
        }
    }
}
