//---------------------------------------------------------------------
// <copyright file="EntityClassGenerator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Data;
using System.Data.EntityModel.SchemaObjectModel;
using System.Data.Metadata.Edm;
using System.Data.EntityModel;
using System.Data.Entity.Design.Common;
using System.IO;
using System.Xml;
using System.Data.Entity.Design.SsdlGenerator;
using Microsoft.Build.Utilities;
using System.Runtime.Versioning;

namespace System.Data.Entity.Design
{
    /// <summary>
    /// Event handler for the OnTypeGenerated event
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The event args</param>
    public delegate void TypeGeneratedEventHandler(object sender, TypeGeneratedEventArgs e);

    /// <summary>
    /// Event handler for the OnPropertyGenerated event
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The event args</param>
    public delegate void PropertyGeneratedEventHandler(object sender, PropertyGeneratedEventArgs e);

    /// <summary>
    /// Summary description for CodeGenerator.
    /// </summary>
    public sealed class EntityClassGenerator
    {
        #region Instance Fields
        LanguageOption _languageOption = LanguageOption.GenerateCSharpCode;
        EdmToObjectNamespaceMap _edmToObjectNamespaceMap = new EdmToObjectNamespaceMap();
        #endregion

        #region Events

        /// <summary>
        /// The event that is raised when a type is generated
        /// </summary>
        public event TypeGeneratedEventHandler OnTypeGenerated;

        /// <summary>
        /// The event that is raised when a property is generated
        /// </summary>
        public event PropertyGeneratedEventHandler OnPropertyGenerated;

        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        public EntityClassGenerator()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public EntityClassGenerator(LanguageOption languageOption)
        {
            _languageOption = EDesignUtil.CheckLanguageOptionArgument(languageOption, "languageOption");
        }

        /// <summary>
        /// Gets and Sets the Language to use for code generation.
        /// </summary>
        public LanguageOption LanguageOption
        {
            get { return _languageOption; }
            set { _languageOption = EDesignUtil.CheckLanguageOptionArgument(value, "value"); }
        }

        /// <summary>
        /// Gets the map entries use to customize the namespace of .net types that are generated
        /// and referenced by the generated code
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        public EdmToObjectNamespaceMap EdmToObjectNamespaceMap
        {
            get { return _edmToObjectNamespaceMap; }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        public IList<EdmSchemaError> GenerateCode(XmlReader sourceEdmSchema, TextWriter target)
        {
            EDesignUtil.CheckArgumentNull(sourceEdmSchema, "sourceEdmSchema");
            EDesignUtil.CheckArgumentNull(target, "target");
            return GenerateCode(sourceEdmSchema, target, new XmlReader[] { });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        [ResourceExposure(ResourceScope.None)] //No resource is exposed since we pass in null as the target paath.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)] //For GenerateCodeCommon method call. Since we use null as the path, we have not changed the scope of the resource.
        public IList<EdmSchemaError> GenerateCode(XmlReader sourceEdmSchema, TextWriter target, IEnumerable<XmlReader> additionalEdmSchemas)
        {
            EDesignUtil.CheckArgumentNull(sourceEdmSchema, "sourceEdmSchema");
            EDesignUtil.CheckArgumentNull(additionalEdmSchemas, "additionalEdmSchemas");
            EDesignUtil.CheckArgumentNull(target, "target");
            
            List<EdmSchemaError> errors = new List<EdmSchemaError>();
            try
            {
                MetadataArtifactLoader sourceLoader = new MetadataArtifactLoaderXmlReaderWrapper(sourceEdmSchema);
                List<MetadataArtifactLoader> loaders = new List<MetadataArtifactLoader>();
                loaders.Add(sourceLoader);

                int index = 0;
                foreach (XmlReader additionalEdmSchema in additionalEdmSchemas)
                {
                    if (additionalEdmSchema == null)
                    {
                        throw EDesignUtil.Argument(Strings.NullAdditionalSchema("additionalEdmSchema", index));
                    }

                    try
                    {
                        MetadataArtifactLoader loader = new MetadataArtifactLoaderXmlReaderWrapper(additionalEdmSchema);
                        Debug.Assert(loader != null, "when is the loader ever null?");
                        loaders.Add(loader);
                    }
                    catch (Exception e)
                    {
                        if (MetadataUtil.IsCatchableExceptionType(e))
                        {
                            errors.Add(new EdmSchemaError(e.Message,
                                (int)ModelBuilderErrorCode.CodeGenAdditionalEdmSchemaIsInvalid,
                                EdmSchemaErrorSeverity.Error));
                        }
                        else
                        {
                            throw;
                        }
                    }
                    index++;
                }
                ThrowOnAnyNonWarningErrors(errors);

                GenerateCodeCommon(sourceLoader, 
                    loaders, 
                    new LazyTextWriterCreator(target),
                    null,  // source path
                    null,  // target file path
                    false, // dispose readers?
                    errors);
            }
            catch (TerminalErrorException)
            {
                // do nothing
                // just a place to jump when errors are detected
            }

            return errors;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        [ResourceExposure(ResourceScope.Machine)] //Exposes the sourceEdmSchemaFilePath which is a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For GenerateCode method call. But the path is not created in this method.
        public IList<EdmSchemaError> GenerateCode(string sourceEdmSchemaFilePath, string targetFilePath)
        {
            return GenerateCode(sourceEdmSchemaFilePath, targetFilePath, new string[] { });        
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        [ResourceExposure(ResourceScope.Machine)] //Exposes the sourceEdmSchemaFilePath which is a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For MetadataArtifactLoader.Create method call. But the path is not created in this method.
        public IList<EdmSchemaError> GenerateCode(string sourceEdmSchemaFilePath, string targetPath, IEnumerable<string> additionalEdmSchemaFilePaths)
        {
            EDesignUtil.CheckStringArgument(sourceEdmSchemaFilePath, "sourceEdmSchemaFilePath");
            EDesignUtil.CheckArgumentNull(additionalEdmSchemaFilePaths, "additionalEdmSchemaFilePaths");
            EDesignUtil.CheckStringArgument(targetPath, "targetPath");

            List<EdmSchemaError> errors = new List<EdmSchemaError>();
            try
            {
                // create a loader for the source
                HashSet<string> uriRegistry = new HashSet<string>();
                MetadataArtifactLoader sourceLoader;
                try
                {
                    sourceLoader = MetadataArtifactLoader.Create(sourceEdmSchemaFilePath, MetadataArtifactLoader.ExtensionCheck.Specific,
                        XmlConstants.CSpaceSchemaExtension, uriRegistry);
                }
                catch (MetadataException e)
                {
                    errors.Add(CreateErrorForException(ModelBuilderErrorCode.CodeGenSourceFilePathIsInvalid, e, sourceEdmSchemaFilePath));
                    return errors;
                }

                if (sourceLoader.IsComposite)
                {
                    throw new ArgumentException(Strings.CodeGenSourceFilePathIsNotAFile, "sourceEdmSchemaPath");
                }

                // create loaders for all the additional schemas
                List<MetadataArtifactLoader> loaders = new List<MetadataArtifactLoader>();
                loaders.Add(sourceLoader);
                int index = 0;
                foreach (string additionalSchemaFilePath in additionalEdmSchemaFilePaths)
                {
                    if (additionalSchemaFilePath == null)
                    {
                        throw EDesignUtil.Argument(Strings.NullAdditionalSchema("additionalEdmSchemaFilePaths", index));
                    }

                    try
                    {
                        MetadataArtifactLoader loader = MetadataArtifactLoader.Create(additionalSchemaFilePath,
                            MetadataArtifactLoader.ExtensionCheck.Specific,
                            XmlConstants.CSpaceSchemaExtension, uriRegistry);
                        Debug.Assert(loader != null, "when is the loader ever null?");
                        loaders.Add(loader);
                    }
                    catch (Exception e)
                    {
                        if(MetadataUtil.IsCatchableExceptionType(e))
                        {
                            errors.Add(CreateErrorForException(ModelBuilderErrorCode.CodeGenAdditionalEdmSchemaIsInvalid, e, additionalSchemaFilePath));
                        }
                        else
                        {
                            throw;
                        }
                    }
                    index++;
                }

                ThrowOnAnyNonWarningErrors(errors);
                try
                {
                    using (LazyTextWriterCreator target = new LazyTextWriterCreator(targetPath))
                    {
                        GenerateCodeCommon(sourceLoader, loaders, target, sourceEdmSchemaFilePath, targetPath,
                            true, // dispose readers
                            errors);
                    }
                }
                catch (System.IO.IOException ex)
                {
                    errors.Add(CreateErrorForException(System.Data.EntityModel.SchemaObjectModel.ErrorCode.IOException, ex, targetPath));
                    return errors;
                }
            }
            catch (TerminalErrorException)
            {
                // do nothing
                // just a place to jump when errors are detected
            }
            return errors;
        }

        private void GenerateCodeCommon(MetadataArtifactLoader sourceLoader, 
            List<MetadataArtifactLoader> loaders,
            LazyTextWriterCreator target,
            string sourceEdmSchemaFilePath,
            string targetFilePath,
            bool closeReaders,
            List<EdmSchemaError> errors)
        {
            MetadataArtifactLoaderComposite composite = new MetadataArtifactLoaderComposite(loaders);
            
            // create the schema manager from the xml readers
            Dictionary<MetadataArtifactLoader, XmlReader> readerSourceMap = new Dictionary<MetadataArtifactLoader, XmlReader>();
            IList<Schema> schemas;
            List<XmlReader> readers = composite.GetReaders(readerSourceMap);

            try
            {
                IList<EdmSchemaError> schemaManagerErrors =
                    SchemaManager.ParseAndValidate(readers,
                        composite.GetPaths(),
                        SchemaDataModelOption.EntityDataModel,
                        EdmProviderManifest.Instance,
                        out schemas);
                errors.AddRange(schemaManagerErrors);
            }
            finally
            {
                if (closeReaders)
                {
                    MetadataUtil.DisposeXmlReaders(readers);
                }
            }
            ThrowOnAnyNonWarningErrors(errors);
            Debug.Assert(readerSourceMap.ContainsKey(sourceLoader), "the source loader didn't produce any of the xml readers...");
            XmlReader sourceReader = readerSourceMap[sourceLoader];


            // use the index of the "source" xml reader as the index of the "source" schema
            Debug.Assert(readers.Contains(sourceReader), "the source reader is not in the list of readers");
            int index = readers.IndexOf(sourceReader);
            Debug.Assert(index >= 0, "couldn't find the source reader in the list of readers");

            Debug.Assert(readers.Count == schemas.Count, "We have a different number of readers than schemas");
            Schema sourceSchema = schemas[index];
            Debug.Assert(sourceSchema != null, "sourceSchema is null");

            // create the EdmItemCollection from the schemas
            EdmItemCollection itemCollection = new EdmItemCollection(schemas);
            if (EntityFrameworkVersionsUtil.ConvertToVersion(itemCollection.EdmVersion) >= EntityFrameworkVersions.Version2)
            {
                throw EDesignUtil.InvalidOperation(Strings.TargetEntityFrameworkVersionToNewForEntityClassGenerator);  
            }

            // generate code
            ClientApiGenerator generator = new ClientApiGenerator(sourceSchema, itemCollection, this, errors);
            generator.GenerateCode(target, targetFilePath);
        }


        #endregion

        #region Private Methods
        private static EdmSchemaError CreateErrorForException(System.Data.EntityModel.SchemaObjectModel.ErrorCode errorCode, System.Exception exception, string sourceLocation)
        {
            Debug.Assert(exception != null);
            Debug.Assert(sourceLocation != null);

            return new EdmSchemaError(exception.Message, (int)errorCode, EdmSchemaErrorSeverity.Error, sourceLocation, 0, 0, exception);
        }

        internal static EdmSchemaError CreateErrorForException(ModelBuilderErrorCode errorCode, System.Exception exception, string sourceLocation)
        {
            Debug.Assert(exception != null);
            Debug.Assert(sourceLocation != null);

            return new EdmSchemaError(exception.Message, (int)errorCode, EdmSchemaErrorSeverity.Error, sourceLocation, 0, 0, exception);
        }

        internal static EdmSchemaError CreateErrorForException(ModelBuilderErrorCode errorCode, System.Exception exception)
        {
            Debug.Assert(exception != null);

            return new EdmSchemaError(exception.Message, (int)errorCode, EdmSchemaErrorSeverity.Error, null, 0, 0, exception);
        }

        private void ThrowOnAnyNonWarningErrors(List<EdmSchemaError> errors)
        {
            foreach (EdmSchemaError error in errors)
            {
                if (error.Severity != EdmSchemaErrorSeverity.Warning)
                {
                    throw new TerminalErrorException();
                }
            }
        }

        #endregion

        #region Event Helpers

        /// <summary>
        /// Helper method that raises the TypeGenerated event
        /// </summary>
        /// <param name="eventArgs">The event arguments passed to the subscriber</param>
        internal void RaiseTypeGeneratedEvent(TypeGeneratedEventArgs eventArgs)
        {
            if (this.OnTypeGenerated != null)
            {
                this.OnTypeGenerated(this, eventArgs);
            }
        }

        /// <summary>
        /// Helper method that raises the PropertyGenerated event
        /// </summary>
        /// <param name="eventArgs">The event arguments passed to the subscriber</param>
        internal void RaisePropertyGeneratedEvent(PropertyGeneratedEventArgs eventArgs)
        {
            if (this.OnPropertyGenerated != null)
            {
                this.OnPropertyGenerated(this, eventArgs);
            }
        }

        #endregion
    }
}
