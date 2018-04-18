//---------------------------------------------------------------------
// <copyright file="EntityModelBuildProvider.cs" company="Microsoft">
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
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Compilation;
using System.Xml;
using System.Data.Entity.Design;
using System.Data.Metadata.Edm;

namespace System.Data.Entity.Design.AspNet
{
    /// <summary>
    /// The ASP .NET Build provider for the CSDL in ADO .NET
    /// </summary>
    /// 
    [BuildProviderAppliesTo(BuildProviderAppliesTo.Code)]
    public class EntityModelBuildProvider : System.Web.Compilation.BuildProvider
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public EntityModelBuildProvider()
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        public override void GenerateCode(AssemblyBuilder assemblyBuilder)
        {
            // look at the assembly builder to see which language we should use in the App_Code directory
            EntityCodeGenerator generator = null;
            if (assemblyBuilder.CodeDomProvider.FileExtension.ToLowerInvariant() == "cs")
            {
                generator = new EntityCodeGenerator(LanguageOption.GenerateCSharpCode);
            }
            else
            {
                generator = new EntityCodeGenerator(LanguageOption.GenerateVBCode);
            }

            // generate the code for our CSDL file
            IList<EdmSchemaError> errors = null;
            using (XmlReader input = XmlReader.Create(VirtualPathProvider.OpenFile(base.VirtualPath)))
            {
                using (StringWriter output = new StringWriter(CultureInfo.InvariantCulture))
                {
                    // Read from input and generate into output, put errors in a class member
                    var entityFrameworkVersion = GetEntityFrameworkVersion(BuildManager.TargetFramework.Version);
                    errors = generator.GenerateCode(input, output, entityFrameworkVersion);
                    if (errors.Count == 0)
                    {
                        output.Flush();
                        assemblyBuilder.AddCodeCompileUnit(this, new CodeSnippetCompileUnit(output.ToString()));
                    }
                }
            }

            // if there are errors, package this data into XmlExceptions and throw this
            // if we are in VS, the ASP .NET stack will place this information in the error pane
            // if we are in the ASP .NET runtime, it will use this information to build the error page
            if (errors != null && errors.Count > 0)
            {
                XmlException inner = null;
                XmlException outer = null;
                foreach (EdmSchemaError error in errors)
                {
                    outer = new XmlException(error.Message, inner, error.Line, error.Column);
                    inner = outer;
                }

                throw outer;
            }

            BuildProviderUtils.AddArtifactReference(assemblyBuilder, this, base.VirtualPath);
        }

        private static Version GetEntityFrameworkVersion(Version targetFrameworkVersion)
        {
            Debug.Assert(targetFrameworkVersion != null, "targetFrameworkVersion should not be null.");
            Debug.Assert(targetFrameworkVersion >= new Version(3, 5), "This assembly doesn't exist pre-3.5.");

            if (targetFrameworkVersion < new Version(4, 0))
            {
                return EntityFrameworkVersions.Version1;
            }
            if (targetFrameworkVersion < new Version(4, 5))
            {
                return EntityFrameworkVersions.Version2;
            }
            return EntityFrameworkVersions.Version3;
        }
    }
}
