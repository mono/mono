//---------------------------------------------------------------------
// <copyright file="EntityCodeGenerator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
//#define ENABLE_TEMPLATE_DEBUGGING

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data.Entity.Design.Common;
using System.Data.Entity.Design.SsdlGenerator;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Xml;

namespace System.Data.Entity.Design
{
    public class EntityCodeGenerator
    {
        private LanguageOption _languageOption = LanguageOption.GenerateCSharpCode;
        private EdmToObjectNamespaceMap _edmToObjectNamespaceMap = new EdmToObjectNamespaceMap();

        public EntityCodeGenerator(LanguageOption languageOption)
        {
            _languageOption = EDesignUtil.CheckLanguageOptionArgument(languageOption, "languageOption");
        }

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

        /// <summary>
        /// Creates a source code file that contains object layer code generated from the specified conceptual schema definition
        /// language (CSDL) file. The list of schema file paths is used to resolve any references contained in the CSDL file.
        /// Note that the targetEntityFrameworkVersion parameter uses internal EntityFramework version numbers as described
        /// in the <see cref="EntityFrameworkVersions"/> class.
        /// </summary>
        /// <param name="sourceEdmSchemaFilePath">The path of the CSDL file.</param>
        /// <param name="targetPath">The path of the file that contains the generated object layer code.</param>
        /// <param name="additionalEdmSchemaFilePaths">
        /// A list of schema file paths that can be used to resolve any references in the source schema (the CSDL file).
        /// If the source schema does not have any dependencies, pass in an empty list.
        /// </param>
        /// <param name="targetEntityFrameworkVersion">The internal Entity Framework version that is being targeted.</param>
        /// <returns>A list of <see cref="EdmSchemaError"/> objects that contains any generated errors.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        [ResourceExposure(ResourceScope.Machine)] // for sourceEdmSchemaFilePath, targetPath and additionalEdmSchema which are machine resources
        [ResourceConsumption(ResourceScope.Machine)] // for InternalGenerateCode method call. But the path is not created in this method.
        public IList<EdmSchemaError> GenerateCode(string sourceEdmSchemaFilePath, string targetPath, IEnumerable<string> additionalEdmSchemaFilePaths, Version targetEntityFrameworkVersion)
        {
            EDesignUtil.CheckTargetEntityFrameworkVersionArgument(targetEntityFrameworkVersion, "targetEntityFrameworkVersion");

            EntityUtil.CheckStringArgument(sourceEdmSchemaFilePath, "sourceEdmSchemaFilePath");
            EDesignUtil.CheckArgumentNull(additionalEdmSchemaFilePaths, "additionalEdmSchemaFilePaths");
            EntityUtil.CheckStringArgument(targetPath, "targetPath");
            EDesignUtil.CheckTargetEntityFrameworkVersionArgument(targetEntityFrameworkVersion, "targetEntityFrameworkVersion");

            return InternalGenerateCode(sourceEdmSchemaFilePath, new LazyTextWriterCreator(targetPath), additionalEdmSchemaFilePaths, targetEntityFrameworkVersion);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        [ResourceExposure(ResourceScope.Machine)] // for sourceEdmSchemaFilePath, targetPath and additionalEdmSchema which are machine resources
        [ResourceConsumption(ResourceScope.Machine)] // for InternalGenerateCode method call. But the path is not created in this method.
        public IList<EdmSchemaError> GenerateCode(string sourceEdmSchemaFilePath, string targetPath, IEnumerable<string> additionalEdmSchemaFilePaths)
        {
            Version targetFrameworkVersion;
            IList<EdmSchemaError> errors = GetMinimumTargetFrameworkVersion(sourceEdmSchemaFilePath, out targetFrameworkVersion);

            if (errors.Where(e => e.Severity == EdmSchemaErrorSeverity.Error).Any())
            {
                return errors;
            }

            return errors.Concat(GenerateCode(sourceEdmSchemaFilePath, targetPath, additionalEdmSchemaFilePaths,
                targetFrameworkVersion)).ToList();
        }

        /// <summary>
        /// Creates a source code file that contains the object layer code generated from the specified conceptual schema
        /// definition language (CSDL) file. Note that the targetEntityFrameworkVersion parameter uses internal Entity
        /// Framework version numbers as described in the <see cref="EntityFrameworkVersions"/> class.
        /// </summary>
        /// <param name="sourceEdmSchemaFilePath">The path of the CSDL file.</param>
        /// <param name="targetPath">The path of the file that contains the generated object layer code.</param>
        /// <param name="targetEntityFrameworkVersion">The internal Entity Framework version that is being targeted.</param>
        /// <returns>A list of <see cref="EdmSchemaError"/> objects that contains any generated errors.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        [ResourceExposure(ResourceScope.Machine)] // for sourceEdmSchemaFilePath and targetPath which are Machine resources
        [ResourceConsumption(ResourceScope.Machine)] // for InternalGenerateCode method call. But the path is not created in this method.
        public IList<EdmSchemaError> GenerateCode(string sourceEdmSchemaFilePath, string targetPath, Version targetEntityFrameworkVersion)
        {
            EntityUtil.CheckStringArgument(sourceEdmSchemaFilePath, "sourceEdmSchemaFilePath");
            EntityUtil.CheckStringArgument(targetPath, "targetPath");
            EDesignUtil.CheckTargetEntityFrameworkVersionArgument(targetEntityFrameworkVersion, "targetEntityFrameworkVersion");

            return InternalGenerateCode(sourceEdmSchemaFilePath, new LazyTextWriterCreator(targetPath), null, targetEntityFrameworkVersion);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        [ResourceExposure(ResourceScope.Machine)] // for sourceEdmSchemaFilePath and targetPath which are Machine resources
        [ResourceConsumption(ResourceScope.Machine)] // for InternalGenerateCode method call. But the path is not created in this method.
        public IList<EdmSchemaError> GenerateCode(string sourceEdmSchemaFilePath, string targetPath)
        {
            Version targetFrameworkVersion;
            IList<EdmSchemaError> errors = GetMinimumTargetFrameworkVersion(sourceEdmSchemaFilePath, out targetFrameworkVersion);

            if (errors.Where(e => e.Severity == EdmSchemaErrorSeverity.Error).Any())
            {
                return errors;
            }

            return errors.Concat(GenerateCode(sourceEdmSchemaFilePath, targetPath, targetFrameworkVersion)).ToList();
        }

        /// <summary>
        /// Generates object layer code using the conceptual schema definition language (CSDL) specified in the
        /// XmlReader object, and outputs the generated code to a TextWriter.
        /// Note that the targetEntityFrameworkVersion parameter uses internal EntityFramework version numbers as
        /// described in the <see cref="EntityFrameworkVersions"/> class.
        /// </summary>
        /// <param name="sourceEdmSchema">An XmlReader that contains the CSDL.</param>
        /// <param name="target">The TextWriter to which the object layer code is written.</param>
        /// <param name="targetEntityFrameworkVersion">The internal Entity Framework version that is being targeted.</param>
        /// <returns>A list of <see cref="EdmSchemaError"/> objects that contains any generated errors.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        [ResourceConsumption(ResourceScope.Machine)] // temp file creation, and passing to InternalGenerateCode
        public IList<EdmSchemaError> GenerateCode(XmlReader sourceEdmSchema, TextWriter target, Version targetEntityFrameworkVersion)
        {
            EDesignUtil.CheckArgumentNull(sourceEdmSchema, "sourceEdmSchema");
            EDesignUtil.CheckArgumentNull(target, "target");
            EDesignUtil.CheckTargetEntityFrameworkVersionArgument(targetEntityFrameworkVersion, "targetEntityFrameworkVersion");

            Version schemaVersion;
            if (!IsValidSchema(sourceEdmSchema, out schemaVersion))
            {
                return new List<EdmSchemaError>() { CreateSourceEdmSchemaNotValidError() };
            }

            using (TempFileCollection collection = new TempFileCollection())
            {
                string tempSourceEdmSchemaPath = collection.AddExtension(XmlConstants.CSpaceSchemaExtension);
                SaveXmlReaderToFile(sourceEdmSchema, tempSourceEdmSchemaPath);

                return InternalGenerateCode(tempSourceEdmSchemaPath, schemaVersion, new LazyTextWriterCreator(target), null, targetEntityFrameworkVersion);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        [ResourceConsumption(ResourceScope.Machine)] // temp file creation, and passing to InternalGenerateCode
        public IList<EdmSchemaError> GenerateCode(XmlReader sourceEdmSchema, TextWriter target)
        {
            Version targetFrameworkVersion;
            IList<EdmSchemaError> errors = GetMinimumTargetFrameworkVersion(sourceEdmSchema, out targetFrameworkVersion);

            if (errors.Where(e => e.Severity == EdmSchemaErrorSeverity.Error).Any())
            {
                return errors;
            }

            return errors.Concat(GenerateCode(sourceEdmSchema, target, targetFrameworkVersion)).ToList();
        }

        /// <summary>
        /// Creates a source code file that contains the object layer code generated from the specified conceptual schema
        /// definition language (CSDL) file. Note that the targetEntityFrameworkVersion parameter uses internal Entity
        /// Framework version numbers as described in the <see cref="EntityFrameworkVersions"/> class.
        /// </summary>
        /// <param name="sourceEdmSchema">An XmlReader that contains the CSDL.</param>
        /// <param name="target">The TextWriter to which the object layer code is written.</param>
        /// <param name="additionalEdmSchemas">
        /// A list of XmlReader objects that contain schemas that are referenced by the source schema (the CSDL).
        /// If the source schema does not have any dependencies, pass in an empty IList object.
        /// </param>
        /// <param name="targetEntityFrameworkVersion">The internal Entity Framework version that is being targeted.</param>
        /// <returns>A list of <see cref="EdmSchemaError"/> objects that contains any generated errors.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        [ResourceConsumption(ResourceScope.Machine)] //Temp file creation, and passing to InternalGenerateCode
        public IList<EdmSchemaError> GenerateCode(XmlReader sourceEdmSchema, TextWriter target, IEnumerable<XmlReader> additionalEdmSchemas, Version targetEntityFrameworkVersion)
        {
            EDesignUtil.CheckArgumentNull(sourceEdmSchema, "sourceEdmSchema");
            EDesignUtil.CheckArgumentNull(additionalEdmSchemas, "additionalEdmSchemas");
            EDesignUtil.CheckArgumentNull(target, "target");
            EDesignUtil.CheckTargetEntityFrameworkVersionArgument(targetEntityFrameworkVersion, "targetEntityFrameworkVersion");

            Version schemaVersion;
            if (!IsValidSchema(sourceEdmSchema, out schemaVersion))
            {
                return new List<EdmSchemaError>() { CreateSourceEdmSchemaNotValidError() };
            }

            using (TempFileCollection collection = new TempFileCollection())
            {

                string tempSourceEdmSchemaPath = collection.AddExtension(XmlConstants.CSpaceSchemaExtension);
                SaveXmlReaderToFile(sourceEdmSchema, tempSourceEdmSchemaPath);
                List<string> additionalTempPaths = new List<string>();
                foreach (XmlReader reader in additionalEdmSchemas)
                {

                    string temp = Path.GetTempFileName() + XmlConstants.CSpaceSchemaExtension;
                    SaveXmlReaderToFile(reader, temp);
                    additionalTempPaths.Add(temp);
                    collection.AddFile(temp, false);
                }
                return InternalGenerateCode(tempSourceEdmSchemaPath, schemaVersion, new LazyTextWriterCreator(target), additionalTempPaths, targetEntityFrameworkVersion);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        [ResourceConsumption(ResourceScope.Machine)] //Temp file creation, and passing to InternalGenerateCode
        public IList<EdmSchemaError> GenerateCode(XmlReader sourceEdmSchema, TextWriter target, IEnumerable<XmlReader> additionalEdmSchemas)
        {
            Version targetFrameworkVersion;
            IList<EdmSchemaError> errors = GetMinimumTargetFrameworkVersion(sourceEdmSchema, out targetFrameworkVersion);

            if (errors.Where(e => e.Severity == EdmSchemaErrorSeverity.Error).Any())
            {
                return errors;
            }

            return errors.Concat(GenerateCode(sourceEdmSchema, target, additionalEdmSchemas, targetFrameworkVersion)).ToList();
        }

        // this overload is for entry points that don't pass in readers, so we need to actually create a reader to get the schemaVersion out
        private IList<EdmSchemaError> InternalGenerateCode(string sourceEdmSchemaFilePath, LazyTextWriterCreator textWriter, IEnumerable<string> additionalEdmSchemaFilePaths, Version targetFrameworkVersion)
        {
            // do this check to maintain backwards compatibility with the behavior shipped in 
            // framework v4
            if (!File.Exists(sourceEdmSchemaFilePath))
            {
                return new List<EdmSchemaError>() { new EdmSchemaError(Strings.EdmSchemaFileNotFound(sourceEdmSchemaFilePath), (int)ModelBuilderErrorCode.FileNotFound, EdmSchemaErrorSeverity.Error, sourceEdmSchemaFilePath, 0, 0) };
            }

            using (XmlReader reader = XmlReader.Create(sourceEdmSchemaFilePath))
            {
                Version schemaVersion;
                if (!IsValidSchema(reader, out schemaVersion))
                {
                    return new List<EdmSchemaError>() { CreateSourceEdmSchemaNotValidError() };
                }

                return InternalGenerateCode(sourceEdmSchemaFilePath, schemaVersion, textWriter, additionalEdmSchemaFilePaths, targetFrameworkVersion);
            }
        }

        private bool IsValidSchema(XmlReader reader, out Version entityFrameworkVersion)
        {
            double schemaVersion;
            DataSpace dataSpace;
            if (System.Data.EntityModel.SchemaObjectModel.SchemaManager.TryGetSchemaVersion(reader, out schemaVersion, out dataSpace) &&
                dataSpace == DataSpace.CSpace)
            {
                entityFrameworkVersion = EntityFrameworkVersionsUtil.ConvertToVersion(schemaVersion);
                return true;
            }
            else if (EntityFrameworkVersions.TryGetEdmxVersion(reader, out entityFrameworkVersion))
            {
                return true;
            }

            return false;
        }

        private IList<EdmSchemaError> GetMinimumTargetFrameworkVersion(string sourceEdmSchemaFilePath, out Version targetFrameworkVersion)
        {
            targetFrameworkVersion = null;
            try
            {
                using (XmlReader reader = XmlReader.Create(sourceEdmSchemaFilePath))
                {
                    return GetMinimumTargetFrameworkVersion(reader, out targetFrameworkVersion);
                }
            }
            catch (System.IO.FileNotFoundException ex)
            {
                return new List<EdmSchemaError>() { EntityClassGenerator.CreateErrorForException(ModelBuilderErrorCode.FileNotFound, ex) };
            }
        }

        private IList<EdmSchemaError> GetMinimumTargetFrameworkVersion(XmlReader sourceEdmSchemaXmlReader, out Version targetFrameworkVersion)
        {
            List<EdmSchemaError> errorList = new List<EdmSchemaError>();
            if (!IsValidSchema(sourceEdmSchemaXmlReader, out targetFrameworkVersion))
            {
                errorList.Add(CreateSourceEdmSchemaNotValidError());
                return errorList;
            }

            if (targetFrameworkVersion < EntityFrameworkVersions.Version2)
            {
                targetFrameworkVersion = EntityFrameworkVersions.Version2;
            }

            if (targetFrameworkVersion > EntityFrameworkVersions.Default)
            {
                errorList.Add(new EdmSchemaError(Strings.DefaultTargetVersionTooLow(EntityFrameworkVersions.Default, targetFrameworkVersion), (int)ModelBuilderErrorCode.SchemaVersionHigherThanTargetVersion, EdmSchemaErrorSeverity.Warning));
            }

            return errorList;
        }

        private IList<EdmSchemaError> InternalGenerateCode(string sourceEdmSchemaFilePath, Version schemaVersion, LazyTextWriterCreator textWriter, IEnumerable<string> additionalEdmSchemaFilePaths, Version targetFrameworkVersion)
        {
            List<EdmSchemaError> errors = new List<EdmSchemaError>();
            try
            {
                if (targetFrameworkVersion == EntityFrameworkVersions.Version1)
                {
                    errors.Add(new EdmSchemaError(Strings.EntityCodeGenTargetTooLow, (int)ModelBuilderErrorCode.TargetVersionNotSupported, EdmSchemaErrorSeverity.Error));
                    return errors;
                }

                if (!MetadataItemCollectionFactory.ValidateActualVersionAgainstTarget(targetFrameworkVersion, schemaVersion, errors))
                {
                    return errors;
                }

                if (schemaVersion == EntityFrameworkVersions.EdmVersion1_1)
                {
                    return GenerateCodeFor1_1Schema(sourceEdmSchemaFilePath, textWriter, additionalEdmSchemaFilePaths);
                }

                ReflectionAdapter codeGenerator = ReflectionAdapter.Create(_languageOption, targetFrameworkVersion);
                codeGenerator.SourceCsdlPath = sourceEdmSchemaFilePath;
                codeGenerator.ReferenceCsdlPaths = additionalEdmSchemaFilePaths;
                codeGenerator.EdmToObjectNamespaceMap = _edmToObjectNamespaceMap.AsDictionary();

                string code = codeGenerator.TransformText();

                if (codeGenerator.Errors.Count != 0)
                {
                    ModelBuilderErrorCode errorCode = ModelBuilderErrorCode.PreprocessTemplateTransformationError;
                    errors.AddRange(codeGenerator.Errors.OfType<CompilerError>().Select(c => ConvertToEdmSchemaError(c, errorCode)));

                    if (codeGenerator.Errors.HasErrors)
                        return errors;
                }

                using (TextWriter writer = textWriter.GetOrCreateTextWriter())
                {
                    writer.Write(code);
                }
            }
            catch (System.UnauthorizedAccessException ex)
            {
                errors.Add(EntityClassGenerator.CreateErrorForException(ModelBuilderErrorCode.SecurityError, ex));
            }
            catch (System.IO.FileNotFoundException ex)
            {
                errors.Add(EntityClassGenerator.CreateErrorForException(ModelBuilderErrorCode.FileNotFound, ex));
            }
            catch (System.Security.SecurityException ex)
            {
                errors.Add(EntityClassGenerator.CreateErrorForException(ModelBuilderErrorCode.SecurityError, ex));
            }
            catch (System.IO.DirectoryNotFoundException ex)
            {
                errors.Add(EntityClassGenerator.CreateErrorForException(ModelBuilderErrorCode.DirectoryNotFound, ex));
            }
            catch (System.IO.IOException ex)
            {
                errors.Add(EntityClassGenerator.CreateErrorForException(ModelBuilderErrorCode.IOException, ex));
            }
            catch (Exception e)
            {
                if (MetadataUtil.IsCatchableExceptionType(e))
                {
                    errors.Add(EntityClassGenerator.CreateErrorForException(ModelBuilderErrorCode.PreprocessTemplateTransformationError, e, sourceEdmSchemaFilePath));
                }
                else
                {
                    throw;
                }
            }

            return errors;
        }

        private static EdmSchemaError CreateSourceEdmSchemaNotValidError()
        {
            return new EdmSchemaError(Strings.EdmSchemaNotValid, (int)ModelBuilderErrorCode.SourceSchemaIsInvalid, EdmSchemaErrorSeverity.Error);
        }

        private IList<EdmSchemaError> GenerateCodeFor1_1Schema(string sourceEdmSchemaFilePath, LazyTextWriterCreator textWriter, IEnumerable<string> additionalEdmSchemaFilePaths)
        {
            EntityClassGenerator generator = new EntityClassGenerator(_languageOption);
            string target = textWriter.TargetFilePath;
            bool cleanUpTarget = false;
            try
            {
                if (textWriter.IsUserSuppliedTextWriter)
                {
                    cleanUpTarget = true;
                    target = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

                }

                IList<EdmSchemaError> errors = generator.GenerateCode(sourceEdmSchemaFilePath, target, additionalEdmSchemaFilePaths != null ? additionalEdmSchemaFilePaths : Enumerable.Empty<string>());

                if (textWriter.IsUserSuppliedTextWriter && !errors.Any(e => e.Severity == EdmSchemaErrorSeverity.Error))
                {
                    textWriter.GetOrCreateTextWriter().Write(File.ReadAllText(target));
                }

                return errors;
            }
            finally
            {
                if (cleanUpTarget && target != null && File.Exists(target))
                {
                    File.Delete(target);
                }
            }

        }

        private static void SaveXmlReaderToFile(XmlReader schema, string tempSchemaPath)
        {
            using (XmlWriter writer = XmlWriter.Create(tempSchemaPath))
            {
                writer.WriteNode(schema, false);
            }
        }

        private static EdmSchemaError ConvertToEdmSchemaError(CompilerError error, ModelBuilderErrorCode defaultErrorCode)
        {
            int errorNumber;
            string message = error.ErrorText;
            bool usePositionInfo = true;
            EdmSchemaErrorSeverity severity = error.IsWarning ? EdmSchemaErrorSeverity.Warning : EdmSchemaErrorSeverity.Error;
            if (int.TryParse(error.ErrorNumber, out errorNumber))
            {
                if (error.Line == -1 && error.Column == -1)
                {
                    usePositionInfo = false;
                }
            }
            else
            {
                message = String.Format(CultureInfo.InvariantCulture, "{0}({1})", error.ErrorText, error.ErrorNumber);
                errorNumber = (int)defaultErrorCode;
            }

            if (usePositionInfo)
            {
                return new EdmSchemaError(message,
                                        errorNumber,
                                        severity,
                                        error.FileName,
                                        error.Line,
                                        error.Column);
            }
            else
            {
                return new EdmSchemaError(message,
                                        errorNumber,
                                        severity);
            }
        }


        static Type _vbCodeGeneratorTypeV2 = null;
        static Type _vbCodeGeneratorTypeV3 = null;
        static Type _csharpCodeGeneratorTypeV2 = null;
        static Type _csharpCodeGeneratorTypeV3 = null;

        private const string CSharpTemplateCodeGenTypeName = "TemplateCodeGenerators.CSharpCodeGenTemplate";
        private const string VBTemplateCodeGenTypeName = "TemplateCodeGenerators.VBCodeGenTemplate";
        private const string CSharpTemplateCodeGenV3TypeName = CSharpTemplateCodeGenTypeName + "V50";
        private const string VBTemplateCodeGenV3TypeName = VBTemplateCodeGenTypeName + "V50";
        private const string CSharpTemplateCodeGenResourceV2 = CSharpTemplateCodeGenTypeName + ".cs";
        private const string CSharpTemplateCodeGenResourceV3 = CSharpTemplateCodeGenTypeName + "V5.0.cs";
        private const string VBTemplateCodeGenResourceV2 = VBTemplateCodeGenTypeName + ".vb";
        private const string VBTemplateCodeGenResourceV3 = VBTemplateCodeGenTypeName + "V5.0.vb";

        private static object CreateCSharpCodeGeneratorV2()
        {
            if (_csharpCodeGeneratorTypeV2 == null)
            {
                Type type = CreateCodeGeneratorType(new Microsoft.CSharp.CSharpCodeProvider(), CSharpTemplateCodeGenResourceV2, CSharpTemplateCodeGenTypeName);
                Interlocked.Exchange(ref _csharpCodeGeneratorTypeV2, type);
            }

            return Activator.CreateInstance(_csharpCodeGeneratorTypeV2);
        }

        private static object CreateCSharpCodeGeneratorV3()
        {
            if (_csharpCodeGeneratorTypeV3 == null)
            {
                Type type = CreateCodeGeneratorType(new Microsoft.CSharp.CSharpCodeProvider(), CSharpTemplateCodeGenResourceV3, CSharpTemplateCodeGenV3TypeName);
                Interlocked.Exchange(ref _csharpCodeGeneratorTypeV3, type);
            }

            return Activator.CreateInstance(_csharpCodeGeneratorTypeV3);
        }

        private static object CreateVBCodeGeneratorV2()
        {
            if (_vbCodeGeneratorTypeV2 == null)
            {
                Type type = CreateCodeGeneratorType(new Microsoft.VisualBasic.VBCodeProvider(), VBTemplateCodeGenResourceV2, VBTemplateCodeGenTypeName);
                Interlocked.Exchange(ref _vbCodeGeneratorTypeV2, type);
            }

            return Activator.CreateInstance(_vbCodeGeneratorTypeV2);
        }

        private static object CreateVBCodeGeneratorV3()
        {
            if (_vbCodeGeneratorTypeV3 == null)
            {
                Type type = CreateCodeGeneratorType(new Microsoft.VisualBasic.VBCodeProvider(), VBTemplateCodeGenResourceV3, VBTemplateCodeGenV3TypeName);
                Interlocked.Exchange(ref _vbCodeGeneratorTypeV3, type);
            }

            return Activator.CreateInstance(_vbCodeGeneratorTypeV3);
        }

        private static Type CreateCodeGeneratorType(System.CodeDom.Compiler.CodeDomProvider compilerProvider, string resourceName, string typeName)
        {
            string sourceCode = null;
            using (Stream sourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(sourceStream))
            {
                sourceCode = reader.ReadToEnd();
            }

            CompilerParameters compilerParams = new CompilerParameters();
            compilerParams.CompilerOptions = "/d:PREPROCESSED_TEMPLATE";
            compilerParams.GenerateInMemory = true;
            compilerParams.GenerateExecutable = false;
            // grab the assemblies by location so that we don't compile against one that we didn't reference
            compilerParams.ReferencedAssemblies.AddRange(new string[] {
                        typeof(System.CodeDom.Compiler.CodeDomProvider).Assembly.Location,              // System.dll
                        typeof(System.Linq.Enumerable).Assembly.Location,                               // System.Core.dll
                        typeof(System.Data.Objects.ObjectContext).Assembly.Location,                    // System.Data.Entity.dll
                        typeof(System.Data.Entity.Design.EntityCodeGenerator).Assembly.Location,        // System.Data.Entity.Design.dll
                        typeof(System.Data.DbType).Assembly.Location,                                   // System.Data.dll
                        typeof(System.Xml.XmlAttribute).Assembly.Location,                              // System.Xml.dll
                        typeof(System.Xml.Linq.XElement).Assembly.Location,                             // System.Xml.Linq.dll
                });

#if !ENABLE_TEMPLATE_DEBUGGING
            CompilerResults results = compilerProvider.CompileAssemblyFromSource(compilerParams, sourceCode);
#else
            // enables debugging
            compilerParams.GenerateInMemory = false;
            compilerParams.IncludeDebugInformation = true;
            string baseName = Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".";
            compilerParams.OutputAssembly = Path.Combine(Path.GetTempPath(), baseName + typeName + ".Assembly.dll");
            string sourceFileName = Path.Combine(Path.GetTempPath(), baseName + typeName + ".Source." + compilerProvider.FileExtension);
            File.WriteAllText(sourceFileName, sourceCode);
            CompilerResults results = compilerProvider.CompileAssemblyFromFile(compilerParams, sourceFileName);
#warning DO NOT CHECK IN LIKE THIS, Dynamic Assembly Debugging is enabled
#endif


            if (results.Errors.HasErrors)
            {
                string message = results.Errors.OfType<CompilerError>().Aggregate(string.Empty, (accumulated, input) => accumulated == string.Empty ? input.ToString() : accumulated + Environment.NewLine + input.ToString());
                throw EDesignUtil.InvalidOperation(message);
            }

            return results.CompiledAssembly.GetType(typeName);
        }



        private class ReflectionAdapter
        {
            private object _instance;
            private MethodInfo _transformText;
            private PropertyInfo _sourceCsdlPath;
            private PropertyInfo _referenceCsdlPaths;
            private PropertyInfo _errors;
            private PropertyInfo _edmToObjectNamespaceMap;

            internal ReflectionAdapter(object instance)
            {
                _instance = instance;
                Type type = _instance.GetType();
                BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
                _transformText = type.GetMethod("TransformText", flags, null, new Type[0], null);
                Debug.Assert(_transformText != null, "Unable to find method, did the signature or name change?");

                _sourceCsdlPath = type.GetProperty("SourceCsdlPath", flags, null, typeof(String), new Type[0], null);
                Debug.Assert(_sourceCsdlPath != null, "Unable to find property, did the signature or name change?");

                _referenceCsdlPaths = type.GetProperty("ReferenceCsdlPaths", flags, null, typeof(IEnumerable<String>), new Type[0], null);
                Debug.Assert(_referenceCsdlPaths != null, "Unable to find property, did the signature or name change?");

                _errors = type.GetProperty("Errors", flags, null, typeof(CompilerErrorCollection), new Type[0], null);
                Debug.Assert(_errors != null, "Unable to find property, did the signature or name change?");

                _edmToObjectNamespaceMap = type.GetProperty("EdmToObjectNamespaceMap", flags, null, typeof(Dictionary<string, string>), new Type[0], null);
                Debug.Assert(_edmToObjectNamespaceMap != null, "Unable to find property, did the signature or name change?");
            }

            internal CompilerErrorCollection Errors
            {
                get { return (CompilerErrorCollection)_errors.GetValue(_instance, null); }
            }

            internal IEnumerable<String> ReferenceCsdlPaths
            {
                set
                {
                    _referenceCsdlPaths.SetValue(_instance, value, null);
                }
            }

            internal string SourceCsdlPath
            {
                get
                {
                    return (String)_sourceCsdlPath.GetValue(_instance, null);
                }
                set
                {
                    _sourceCsdlPath.SetValue(_instance, value, null);
                }
            }

            internal Dictionary<string, string> EdmToObjectNamespaceMap
            {
                set { _edmToObjectNamespaceMap.SetValue(_instance, value, null); }
            }

            internal string TransformText()
            {
                try
                {
                    return (string)_transformText.Invoke(_instance, new object[0]);
                }
                catch (TargetInvocationException e)
                {
                    Exception actual = e.InnerException != null ? e.InnerException : e;

                    System.CodeDom.Compiler.CompilerError error = new System.CodeDom.Compiler.CompilerError();
                    error.ErrorText = actual.Message;
                    error.IsWarning = false;
                    error.FileName = SourceCsdlPath;
                    Errors.Add(error);

                    return string.Empty;
                }

            }

            internal static ReflectionAdapter Create(LanguageOption language, Version targetEntityFrameworkVersion)
            {
                if (language == LanguageOption.GenerateCSharpCode)
                {
                    if (targetEntityFrameworkVersion >= EntityFrameworkVersions.Latest)
                    {
                        return new ReflectionAdapter(CreateCSharpCodeGeneratorV3());
                    }
                    else
                    {
                        return new ReflectionAdapter(CreateCSharpCodeGeneratorV2());
                    }
                }
                else
                {
                    Debug.Assert(language == LanguageOption.GenerateVBCode, "Did you add a new option?");
                    if (targetEntityFrameworkVersion >= EntityFrameworkVersions.Latest)
                    {
                        return new ReflectionAdapter(CreateVBCodeGeneratorV3());
                    }
                    else
                    {
                        return new ReflectionAdapter(CreateVBCodeGeneratorV2());
                    }
                }
            }
        }
    }
}
