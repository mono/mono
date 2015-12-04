//---------------------------------------------------------------------
// <copyright file="ClientApiGenerator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.EntityModel.Emitters;
using SOM = System.Data.EntityModel.SchemaObjectModel;
using System.Diagnostics;
using System.Data.Metadata.Edm;
using System.Data.Entity.Design;
using System.IO;
using System.Data.EntityModel.SchemaObjectModel;
using System.Data.Entity.Design.SsdlGenerator;
using System.Linq;
using System.Data.Entity.Design.Common;
using System.Runtime.Versioning;

namespace System.Data.EntityModel
{
    /// <summary>
    /// Summary description for ClientApiGenerator.
    /// </summary>
    internal sealed class ClientApiGenerator
    {
        #region Instance Fields
        private string _codeNamespace = null;
        private CodeCompileUnit _compileUnit = null;
        private bool _isLanguageCaseSensitive = true;

        private EdmItemCollection _edmItemCollection = null;
        private Schema _sourceSchema = null;
        private FixUpCollection _fixUps = null;
        private AttributeEmitter _attributeEmitter = null;
        EntityClassGenerator _generator;
        List<EdmSchemaError> _errors;
        TypeReference _typeReference = new TypeReference();

        #endregion

        #region Public Methods
        public ClientApiGenerator(Schema sourceSchema, EdmItemCollection edmItemCollection, EntityClassGenerator generator, List<EdmSchemaError> errors)
        {
            Debug.Assert(sourceSchema != null, "sourceSchema is null");
            Debug.Assert(edmItemCollection != null, "edmItemCollection is null");
            Debug.Assert(generator != null, "generator is null");
            Debug.Assert(errors != null, "errors is null");

            _edmItemCollection = edmItemCollection;
            _sourceSchema = sourceSchema;
            _generator = generator;
            _errors = errors;
            _attributeEmitter = new AttributeEmitter(_typeReference);
        }

        /// <summary>
        /// Parses a source Schema and outputs client-side generated code to
        /// the output TextWriter.
        /// </summary>
        /// <param name="schema">The source Schema</param>
        /// <param name="output">The TextWriter in which to write the output</param>
        /// <param name="outputUri">The Uri for the output. Can be null.</param>
        /// <returns>A list of GeneratorErrors.</returns>
        [ResourceExposure(ResourceScope.None)] //No resource is exposed.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)] //For Path.GetTempPath method. 
                                                                            //We use tha path to create a temp file stream which is consistent with the resource consumption of machine.

        internal void GenerateCode(LazyTextWriterCreator target, string targetLocation)
        {
            Debug.Assert(target != null, "target parameter is null");

            IndentedTextWriter indentedTextWriter = null;
            System.IO.Stream tempFileStream = null;
            System.IO.StreamReader reader = null;
            System.IO.StreamWriter writer = null;
            TempFileCollection tempFiles = null;
            try
            {
                CodeDomProvider provider = null;
                switch (Language)
                {
                    case LanguageOption.GenerateCSharpCode:
                        provider = new Microsoft.CSharp.CSharpCodeProvider();
                        break;

                    case LanguageOption.GenerateVBCode:
                        provider = new Microsoft.VisualBasic.VBCodeProvider();
                        break;
                }

                _isLanguageCaseSensitive = (provider.LanguageOptions & LanguageOptions.CaseInsensitive) == 0;

                new NamespaceEmitter(this, _codeNamespace, target.TargetFilePath).Emit();

                // if there were errors we don't need the output file
                if (RealErrorsExist)
                {
                    return;
                }

                if (FixUps.Count == 0 || !FixUpCollection.IsLanguageSupported(Language))
                {
                    indentedTextWriter = new IndentedTextWriter(target.GetOrCreateTextWriter(), "\t");
                }
                else
                {
                    // need to write to a temporary file so we can do fixups...
                    tempFiles = new TempFileCollection(Path.GetTempPath());
                    string filename = Path.Combine(tempFiles.TempDir, "EdmCodeGenFixup-" + Guid.NewGuid().ToString() + ".tmp");
                    tempFiles.AddFile(filename, false);
                    tempFileStream = new System.IO.FileStream(filename, System.IO.FileMode.CreateNew, System.IO.FileAccess.ReadWrite,
                        System.IO.FileShare.None);
                    indentedTextWriter = new IndentedTextWriter(new System.IO.StreamWriter(tempFileStream), "\t");
                }

                CodeGeneratorOptions styleOptions = new CodeGeneratorOptions();
                styleOptions.BracingStyle = "C";
                styleOptions.BlankLinesBetweenMembers = false;
                styleOptions.VerbatimOrder = true;
                provider.GenerateCodeFromCompileUnit(CompileUnit, indentedTextWriter, styleOptions);

                // if we wrote to a temp file need to post process the file...
                if (tempFileStream != null)
                {
                    indentedTextWriter.Flush();
                    tempFileStream.Seek(0, System.IO.SeekOrigin.Begin);
                    reader = new System.IO.StreamReader(tempFileStream);
                    FixUps.Do(reader, target.GetOrCreateTextWriter(), Language, SourceObjectNamespaceName != string.Empty);
                }
            }
            catch (System.UnauthorizedAccessException ex)
            {
                AddError(ModelBuilderErrorCode.SecurityError, EdmSchemaErrorSeverity.Error, ex);
            }
            catch (System.IO.FileNotFoundException ex)
            {
                AddError(ModelBuilderErrorCode.FileNotFound, EdmSchemaErrorSeverity.Error, ex);
            }
            catch (System.Security.SecurityException ex)
            {
                AddError(ModelBuilderErrorCode.SecurityError, EdmSchemaErrorSeverity.Error, ex);
            }
            catch (System.IO.DirectoryNotFoundException ex)
            {
                AddError(ModelBuilderErrorCode.DirectoryNotFound, EdmSchemaErrorSeverity.Error, ex);
            }
            catch (System.IO.IOException ex)
            {
                AddError(ModelBuilderErrorCode.IOException, EdmSchemaErrorSeverity.Error, ex);
            }
            finally
            {
                if (indentedTextWriter != null)
                {
                    indentedTextWriter.Close();
                }
                if (tempFileStream != null)
                {
                    tempFileStream.Close();
                }
                if (tempFiles != null)
                {
                    tempFiles.Delete();
                    ((IDisposable)tempFiles).Dispose();
                }
                if (reader != null)
                {
                    reader.Close();
                }
                if (writer != null)
                {
                    writer.Close();
                }
            }
        }

        /// <summary>
        /// Verification code invoked for types
        /// </summary>
        /// <param name="item">The type being generated</param>
        internal void VerifyLanguageCaseSensitiveCompatibilityForType(GlobalItem item)
        {
            if (_isLanguageCaseSensitive)
            {
                return; // no validation necessary
            }

            try
            {
                _edmItemCollection.GetItem<GlobalItem>(
                                                        item.Identity,
                                                        true   // ignore case
                                                    );
            }
            catch (InvalidOperationException)
            {
                AddError(Strings.ItemExistsWithDifferentCase(item.BuiltInTypeKind.ToString(), item.Identity), ModelBuilderErrorCode.IncompatibleSettingForCaseSensitiveOption,
                    EdmSchemaErrorSeverity.Error, item.Identity);
            }
        }


        /// <summary>
        /// Verification code invoked for properties
        /// </summary>
        /// <param name="item">The property or navigation property being generated</param>
        internal void VerifyLanguageCaseSensitiveCompatibilityForProperty(EdmMember item)
        {
            if (_isLanguageCaseSensitive)
            {
                return; // no validation necessary
            }

            Debug.Assert(item != null);

            ReadOnlyMetadataCollection<EdmMember> members = item.DeclaringType.Members;
            
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (EdmMember member in members)
            {
                if (set.Contains(member.Identity) &&
                    item.Identity.Equals(member.Identity, StringComparison.OrdinalIgnoreCase))
                {
                    AddError(Strings.PropertyExistsWithDifferentCase(item.Identity), ModelBuilderErrorCode.IncompatibleSettingForCaseSensitiveOption,
                    EdmSchemaErrorSeverity.Error, item.DeclaringType.FullName, item.Identity);
                }
                else
                {
                    set.Add(member.Identity);
                }
            }
        }

        /// <summary>
        /// Verification code invoked for entity sets
        /// </summary>
        /// <param name="item">The entity container being generated</param>
        internal void VerifyLanguageCaseSensitiveCompatibilityForEntitySet(System.Data.Metadata.Edm.EntityContainer item)
        {
            if (_isLanguageCaseSensitive)
            {
                return; // no validation necessary
            }

            Debug.Assert(item != null);

            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (EntitySetBase entitySetBase in item.BaseEntitySets)
            {
                if (MetadataUtil.IsEntitySet(entitySetBase))
                {
                    EntitySet entitySet = (EntitySet)entitySetBase;
                    if (set.Contains(entitySet.Identity))
                    {
                        AddError(ModelBuilderErrorCode.IncompatibleSettingForCaseSensitiveOption,
                        EdmSchemaErrorSeverity.Error, new InvalidOperationException(Strings.EntitySetExistsWithDifferentCase(entitySet.Identity)),
                        item.Name);
                    }
                    else
                    {
                        set.Add(entitySet.Identity);
                    }
                }
            }
        }

        internal IEnumerable<EdmType> GetDirectSubTypes(EdmType edmType)
        {
            return _edmItemCollection.GetItems<EdmType>().Where(b => b.BaseType == edmType);
        }

        private static System.Data.EntityModel.SchemaObjectModel.SchemaElement GetSchemaElement(
            System.Data.EntityModel.SchemaObjectModel.Schema schema, string itemIdentity)
        {
            List<System.Data.EntityModel.SchemaObjectModel.SchemaType> schemaTypes =
                           schema.SchemaTypes.Where(p => p.Identity == itemIdentity).ToList();
            if (null != schemaTypes && schemaTypes.Count > 0)
            {
                return (System.Data.EntityModel.SchemaObjectModel.SchemaElement)schemaTypes.First();
            }
            else
            {
                return null;
            }

        }

        internal static void GetElementLocationInfo(System.Data.EntityModel.SchemaObjectModel.Schema schema, string itemIdentity, out int lineNumber, out int linePosition)
        {
            System.Data.EntityModel.SchemaObjectModel.SchemaElement element = GetSchemaElement(schema, itemIdentity);

            if(null != element)
            {
                lineNumber = element.LineNumber;
                linePosition = element.LinePosition;
            }
            else
            {
                lineNumber = linePosition = -1;
            }
        }

        internal static void GetElementLocationInfo(System.Data.EntityModel.SchemaObjectModel.Schema schema, string parentIdentity, string itemIdentity, out int lineNumber, out int linePosition)
        {
            lineNumber = linePosition = -1;

            System.Data.EntityModel.SchemaObjectModel.SchemaElement element = GetSchemaElement(schema, parentIdentity);
            System.Data.EntityModel.SchemaObjectModel.StructuredType elementWithProperty = 
                element as System.Data.EntityModel.SchemaObjectModel.StructuredType;

            if (null != elementWithProperty && elementWithProperty.Properties.ContainsKey(itemIdentity))
            {
                    lineNumber = elementWithProperty.Properties[itemIdentity].LineNumber;
                    linePosition = elementWithProperty.Properties[itemIdentity].LinePosition;
            }
            else if( null != element)
            {
                lineNumber = element.LineNumber;
                linePosition = element.LinePosition;
            }
        }

        #endregion

        #region Internal Properties

        internal LanguageOption Language
        {
            get
            {
                return _generator.LanguageOption;
            }
        }

        internal TypeReference TypeReference
        {
            get { return _typeReference; }
        }

        internal CodeCompileUnit CompileUnit
        {
            get
            {
                if (_compileUnit == null)
                    _compileUnit = new CodeCompileUnit();

                return _compileUnit;
            }
        }

        public void AddError(string message, ModelBuilderErrorCode errorCode, EdmSchemaErrorSeverity severity)
        {
            _errors.Add(new EdmSchemaError(message, (int)errorCode, severity));
        }

        public void AddError(ModelBuilderErrorCode errorCode, EdmSchemaErrorSeverity severity, Exception ex)
        {
            _errors.Add(new EdmSchemaError(ex.Message, (int)errorCode, severity, ex));
        }

        internal void AddError(string message, ModelBuilderErrorCode errorCode, EdmSchemaErrorSeverity severity, Exception ex)
        {
            _errors.Add(new EdmSchemaError(message, (int)errorCode, severity, ex));
        }

        internal void AddError(ModelBuilderErrorCode errorCode, EdmSchemaErrorSeverity severity, Exception ex, string itemIdentity)
        {
            int lineNumber, linePosition;
            ClientApiGenerator.GetElementLocationInfo(this._sourceSchema, itemIdentity, out lineNumber, out linePosition);

            _errors.Add(new EdmSchemaError(ex.Message, (int)errorCode, severity, this._sourceSchema.Location, lineNumber, linePosition, ex));
        }

        internal void AddError(string message, ModelBuilderErrorCode errorCode, EdmSchemaErrorSeverity severity, string itemIdentity)
        {
            int lineNumber, linePosition;
            ClientApiGenerator.GetElementLocationInfo(this._sourceSchema, itemIdentity, out lineNumber, out linePosition);

            _errors.Add(new EdmSchemaError(message, (int)errorCode, severity, this._sourceSchema.Location, lineNumber, linePosition));
        }

        internal void AddError(string message, ModelBuilderErrorCode errorCode, EdmSchemaErrorSeverity severity, string parentIdentity, string itemIdentity)
        {
            int lineNumber, linePosition;
            ClientApiGenerator.GetElementLocationInfo(this._sourceSchema, parentIdentity, itemIdentity, out lineNumber, out linePosition);

            _errors.Add(new EdmSchemaError(message, (int)errorCode, severity, this._sourceSchema.Location, lineNumber, linePosition));
        }

        /// <summary>
        /// Check collection for any real errors (Severity != Warning)
        /// </summary>
        public bool RealErrorsExist
        {
            get
            {
                foreach (EdmSchemaError error in _errors)
                {
                    if (error.Severity != EdmSchemaErrorSeverity.Warning)
                        return true;
                }
                return false;
            }
        }

        public IEnumerable<GlobalItem> GetSourceTypes()
        {
            foreach (SOM.SchemaType type in _sourceSchema.SchemaTypes)
            {
                if (type is SOM.ModelFunction)
                {
                    continue;
                }

                yield return _edmItemCollection.GetItem<GlobalItem>(type.Identity);
            }
        }

        public CodeTypeReference GetFullyQualifiedTypeReference(EdmType type)
        {
            string fullObjectName = CreateFullName(GetObjectNamespace(type.NamespaceName), type.Name);
            return TypeReference.FromString(fullObjectName);
        }

        public CodeTypeReference GetFullyQualifiedTypeReference(EdmType type, bool addGlobalQualifier)
        {
            string fullObjectName = CreateFullName(GetObjectNamespace(type.NamespaceName), type.Name);
            return TypeReference.FromString(fullObjectName, addGlobalQualifier);
        }

        private string CreateFullName(string namespaceName, string name)
        {
            if (string.IsNullOrEmpty(namespaceName))
            {
                return name;
            }

            return namespaceName + "." + name;
        }

        public CodeTypeReference GetLeastPossibleQualifiedTypeReference(EdmType type)
        {
            string typeRef;

            if (type.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
            {
                return type.ClrType.IsValueType ? TypeReference.NullableForType(type.ClrType) : TypeReference.ForType(type.ClrType);
            }
            else
            {
                if (type.NamespaceName == SourceEdmNamespaceName)
                {
                    // we are already generating in this namespace, no need to qualify it
                    typeRef = type.Name;
                }
                else
                {
                    typeRef = CreateFullName(GetObjectNamespace(type.NamespaceName), type.Name);
                }
            }

            return TypeReference.FromString(typeRef);
        }

        public string SourceEdmNamespaceName
        {
            get
            {
                return _sourceSchema.Namespace;
            }
        }

        public string SourceObjectNamespaceName
        {
            get
            {
                return GetObjectNamespace(SourceEdmNamespaceName);
            }
        }

        private string GetObjectNamespace(string csdlNamespaceName)
        {
            Debug.Assert(csdlNamespaceName != null, "csdlNamespaceName is null");

            string objectNamespace;
            if (_generator.EdmToObjectNamespaceMap.TryGetObjectNamespace(csdlNamespaceName, out objectNamespace))
            {
                return objectNamespace;
            }

            return csdlNamespaceName;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        internal FixUpCollection FixUps
        {
            get
            {
                if (_fixUps == null)
                    _fixUps = new FixUpCollection();

                return _fixUps;
            }
        }

        internal AttributeEmitter AttributeEmitter
        {
            get { return _attributeEmitter; }
        }

        internal bool IsLanguageCaseSensitive
        {
            get { return _isLanguageCaseSensitive; }
        }

        internal StringComparison LanguageAppropriateStringComparer
        {
            get
            {
                if (IsLanguageCaseSensitive)
                {
                    return StringComparison.Ordinal;
                }
                else
                {
                    return StringComparison.OrdinalIgnoreCase;
                }
            }
        }

        /// <summary>
        /// Helper method that raises the TypeGenerated event
        /// </summary>
        /// <param name="eventArgs">The event arguments passed to the subscriber</param>
        internal void RaiseTypeGeneratedEvent(TypeGeneratedEventArgs eventArgs)
        {
            _generator.RaiseTypeGeneratedEvent(eventArgs);
        }

        /// <summary>
        /// Helper method that raises the PropertyGenerated event
        /// </summary>
        /// <param name="eventArgs">The event arguments passed to the subscriber</param>
        internal void RaisePropertyGeneratedEvent(PropertyGeneratedEventArgs eventArgs)
        {
            _generator.RaisePropertyGeneratedEvent(eventArgs);
        }

        #endregion

    }
}
