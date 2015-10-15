//---------------------------------------------------------------------
// <copyright file="EntityViewGenerator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.Metadata.Edm;
using System.Data.Mapping;
using System.Data.EntityClient;
using System.Data.EntityModel;
using System.Data.Common.CommandTrees;
using System.Reflection;
using System.Security.Cryptography;
using System.Data.Entity.Design.Common;
using System.Diagnostics;
using System.Globalization;
using System.Data.Common.Utils;
using Microsoft.Build.Utilities;
using System.Runtime.Versioning;
using System.Linq;

namespace System.Data.Entity.Design
{
    /// <summary>
    /// EntityViewGenerator class produces the views for the extents in the passed in StorageMappingItemCollection.
    /// The views are written as code to the passed in output stream. There are a set of options that user
    /// can use to control the code generation process. The options should be apssed into the constrcutor.
    /// While storing the views in the code, the view generator class also stores a Hash value produced based
    /// on the content of the views and the names of extents. We also generate a hash for each schema file( csdl, ssdl and msl) 
    /// that was used in view generation process and store the hash in the generated code.The entity runtime will try to discover this
    /// type and if it does discover it will use the generated views in this type. The discovery process is
    /// explained in detail in the comments for StorageMappingItemCollection class. 
    /// The runtime will throw an exception if any of the the hash values produced in the design time does not match
    /// the hash values produced at the runtime.
    /// </summary>
    public class EntityViewGenerator
    {
        #region Constructors
        /// <summary>
        /// Create the instance of ViewGenerator with the given language option.
        /// </summary>
        /// <param name="languageOption">Language Option for generated code.</param>
        public EntityViewGenerator(LanguageOption languageOption)
        {
            m_languageOption = EDesignUtil.CheckLanguageOptionArgument(languageOption, "languageOption");
        }

        /// <summary>
        /// Create the instance of ViewGenerator using C# as the default 
        /// language option.
        /// </summary>
        public EntityViewGenerator()
            : this(LanguageOption.GenerateCSharpCode)
        {
        }

        #endregion

        #region Fields
        private LanguageOption m_languageOption;
        private static readonly int MAXONELINELENGTH = 2046;
        private static readonly int ONELINELENGTH = 80;
        private static readonly int ERRORCODE_MAPPINGALLQUERYVIEWATCOMPILETIME = 2088;
        #endregion

        #region Properties
        /// <summary>
        /// Language Option for generated code.
        /// </summary>
        public LanguageOption LanguageOption
        {
            get { return m_languageOption; }
            set { m_languageOption = EDesignUtil.CheckLanguageOptionArgument(value, "value"); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Generates the views for the extents in the mapping item collection and produces
        /// the code for a type that will cache these views. The methods also produces
        /// a hash based on the StorageEntityContainerMapping, which contains all the 
        /// metadata and mapping. It also produces a hash based
        /// on the view content and the name of the extents.
        /// </summary>
        /// <param name="mappingCollection">Mapping Item Collection for which views should be generated</param>
        /// <param name="outputUri">Uri to which generated code needs to be written</param>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the outputPath as part of ConnectionString which are a Machine resource.
        [ResourceConsumption(ResourceScope.Machine)] //For StreamWriter constructor call. But the path to the stream is not created in this method.
        [CLSCompliant(false)]
        public IList<EdmSchemaError> GenerateViews(StorageMappingItemCollection mappingCollection, string outputPath)
        {
            EDesignUtil.CheckArgumentNull(mappingCollection, "mappingCollection");
            EDesignUtil.CheckStringArgument(outputPath, "outputPath");

            TextWriter outputWriter = null;
            try
            {
                return InternalGenerateViews(mappingCollection, () => new StreamWriter(outputPath), out outputWriter);
            }
            finally
            {
                if (outputWriter != null)
                {
                    outputWriter.Dispose();
                }
            }
        }

        /// <summary>
        /// Generates the views for the extents in the mapping item collection and produces
        /// the code for a type that will cache these views. The methods also produces
        /// a hash based on the storageEntityContainerMapping object, which contains all the 
        /// metadata and mapping. It also produces a hash based
        /// on the view content and the name of the extents.
        /// </summary>
        /// <param name="mappingCollection">Mapping Item Collection for which views should be generated</param>
        /// <param name="outputWriter">Output writer to which we want to write the code</param>
        [CLSCompliant(false)]
        public IList<EdmSchemaError> GenerateViews(StorageMappingItemCollection mappingCollection, TextWriter outputWriter)
        {
            EDesignUtil.CheckArgumentNull(mappingCollection, "mappingCollection");

            Version targetEntityFrameworkVersion;
            IList<EdmSchemaError> errorList = GetMinimumTargetFrameworkVersion(mappingCollection, out targetEntityFrameworkVersion);

            return GenerateViews(mappingCollection, outputWriter, targetEntityFrameworkVersion).Concat(errorList).ToList();
        }

        /// <summary>
        /// Generates the views for the extents in the mapping item collection and produces
        /// the code for a type that will cache these views. The methods also produces
        /// a hash based on the storageEntityContainerMapping object, which contains all the 
        /// metadata and mapping. It also produces a hash based
        /// on the view content and the name of the extents.
        /// </summary>
        /// <param name="mappingCollection">Mapping Item Collection for which views should be generated</param>
        /// <param name="outputWriter">Output writer to which we want to write the code</param>
        [CLSCompliant(false)]
        public IList<EdmSchemaError> GenerateViews(StorageMappingItemCollection mappingCollection, TextWriter outputWriter, Version targetEntityFrameworkVersion)
        {
            EDesignUtil.CheckArgumentNull(mappingCollection, "mappingCollection");
            EDesignUtil.CheckArgumentNull(outputWriter, "outputWriter");

            EDesignUtil.CheckTargetEntityFrameworkVersionArgument(targetEntityFrameworkVersion, "targetEntityFrameworkVersion");
            CheckForCompatibleSchemaAndTarget(mappingCollection, targetEntityFrameworkVersion);

            TextWriter writer;
            return InternalGenerateViews(mappingCollection, () => outputWriter, out writer);
        }

        private static void CheckForCompatibleSchemaAndTarget(StorageMappingItemCollection mappingCollection, Version targetEntityFrameworkVersion)
        {
            Version mappingVersion = EntityFrameworkVersionsUtil.ConvertToVersion(mappingCollection.MappingVersion);
            if (targetEntityFrameworkVersion < mappingVersion)
            {
                throw EDesignUtil.Argument(Strings.TargetVersionSchemaVersionMismatch(targetEntityFrameworkVersion, mappingVersion), null);
            }

            Version edmVersion = EntityFrameworkVersionsUtil.ConvertToVersion(mappingCollection.EdmItemCollection.EdmVersion);
            if (targetEntityFrameworkVersion < edmVersion)
            {
                throw EDesignUtil.Argument(Strings.TargetVersionSchemaVersionMismatch(targetEntityFrameworkVersion, edmVersion), null);
            }
        }

        private IList<EdmSchemaError> InternalGenerateViews(
            StorageMappingItemCollection mappingCollection,
            Func<TextWriter> GetWriter,
            out TextWriter outputWriter)
        {
            IList<EdmSchemaError> schemaErrors;
            CodeDomProvider provider;
            Dictionary<EntitySetBase, string> generatedViews;
            if (GetViewsWithErrors(mappingCollection, out provider, out schemaErrors, out generatedViews))
            {
                outputWriter = null;
                return schemaErrors;
            }

            outputWriter = GetWriter();
            GenerateAndStoreViews(mappingCollection, generatedViews,
                outputWriter, provider, schemaErrors);
            
            return schemaErrors;
        }

        /// <summary>
        /// Validates the mappingCollections and returns the schemaErrors.
        /// </summary>
        /// <param name="mappingCollection"></param>
        /// <returns>list of EdmSchemaError</returns>
        [CLSCompliant(false)]
        public static IList<EdmSchemaError> Validate(StorageMappingItemCollection mappingCollection)
        {
            EDesignUtil.CheckArgumentNull(mappingCollection, "mappingCollection");

            Version targetEntityFrameworkVersion;
            IList<EdmSchemaError> errorList = GetMinimumTargetFrameworkVersion(mappingCollection, out targetEntityFrameworkVersion);

            return Validate(mappingCollection, targetEntityFrameworkVersion).Concat(errorList).ToList();
        }
        
        /// <summary>
        /// Validates the mappingCollections and returns the schemaErrors.
        /// </summary>
        /// <param name="mappingCollection"></param>
        /// <returns>list of EdmSchemaError</returns>
        [CLSCompliant(false)]
        public static IList<EdmSchemaError> Validate(StorageMappingItemCollection mappingCollection, Version targetEntityFrameworkVersion)
        {
            EDesignUtil.CheckTargetEntityFrameworkVersionArgument(targetEntityFrameworkVersion, "targetEntityFrameworkVersion");
            CheckForCompatibleSchemaAndTarget(mappingCollection, targetEntityFrameworkVersion);

            // purpose of this API is to validate the mappingCollection, it basically will call GetEntitySetViews

            EDesignUtil.CheckArgumentNull(mappingCollection, "mappingCollection");

            // we need a temp var to to pass it to GetViews (since we will directly invoke GetViews)
            Dictionary<EntitySetBase, string> generatedViews;
            // mappingCollection will be validated and schemaErrors will be returned from GetViews API
            IList<EdmSchemaError> schemaErrors;

            // Validate entity set views.
            GetEntitySetViews(mappingCollection, out schemaErrors, out generatedViews);

            // Validate function imports and their mapping.
            foreach (var containerMapping in mappingCollection.GetItems<StorageEntityContainerMapping>())
            {
                foreach (var functionImport in containerMapping.EdmEntityContainer.FunctionImports)
                {
                    FunctionImportMapping functionImportMapping;
                    if (containerMapping.TryGetFunctionImportMapping(functionImport, out functionImportMapping))
                    {
                        if (functionImport.IsComposableAttribute)
                        {
                            ((FunctionImportMappingComposable)functionImportMapping).ValidateFunctionView(schemaErrors);
                        }
                    }
                    else
                    {
                        schemaErrors.Add(new EdmSchemaError(
                            Strings.UnmappedFunctionImport(functionImport.Identity),
                            (int)StorageMappingErrorCode.UnmappedFunctionImport,
                            EdmSchemaErrorSeverity.Warning));
                    }
                }
            }

            Debug.Assert(schemaErrors != null, "schemaErrors is null");
            return HandleValidationErrors(schemaErrors);
        }

        private static IList<EdmSchemaError> HandleValidationErrors(IList<EdmSchemaError> schemaErrors)
        {
            IEnumerable<EdmSchemaError> warningsToRemove = 
                schemaErrors.Where(s => 
                    s.ErrorCode == ERRORCODE_MAPPINGALLQUERYVIEWATCOMPILETIME); // When EntityContainerMapping only has QueryView, the mapping is valid

            return schemaErrors.Except(warningsToRemove).ToList();
        }

        private bool GetViewsWithErrors(StorageMappingItemCollection mappingCollection, out CodeDomProvider provider, out IList<EdmSchemaError> schemaErrors, out Dictionary<EntitySetBase, string> generatedViews)
        {
            GetViewsAndCodeDomProvider(mappingCollection, out provider, out schemaErrors, out generatedViews);
            //If the generated views are empty because of errors or warnings, then don't create an output file.
            if (generatedViews.Count == 0 && schemaErrors.Count > 0)
            {                
                return true;
            }
            return false;
        }

        private void GetViewsAndCodeDomProvider(StorageMappingItemCollection mappingCollection, out CodeDomProvider provider, out IList<EdmSchemaError> schemaErrors, out Dictionary<EntitySetBase, string> generatedViews)
        {
            //Create a CodeDomProvider based on options.
            provider = null;
            switch (m_languageOption)
            {
                case LanguageOption.GenerateCSharpCode:
                    provider = new Microsoft.CSharp.CSharpCodeProvider();
                    break;

                case LanguageOption.GenerateVBCode:
                    provider = new Microsoft.VisualBasic.VBCodeProvider();
                    break;
            }

            //Get the views for the Entity Sets and Association Sets in the mapping item collection
            GetEntitySetViews(mappingCollection, out schemaErrors, out generatedViews);
        }

        private static void GetEntitySetViews(StorageMappingItemCollection mappingCollection,
            out IList<EdmSchemaError> schemaErrors, out Dictionary<EntitySetBase, string> generatedViews)
        {
            generatedViews = mappingCollection.GenerateEntitySetViews(out schemaErrors);
        }
        
        private static IList<EdmSchemaError> GetMinimumTargetFrameworkVersion(StorageMappingItemCollection mappingCollection, out Version targetFrameworkVersion)
        {
            List<EdmSchemaError> errorList = new List<EdmSchemaError>();

            targetFrameworkVersion = EntityFrameworkVersionsUtil.ConvertToVersion(mappingCollection.EdmItemCollection.EdmVersion);

            if (targetFrameworkVersion > EntityFrameworkVersions.Default)
            {
                errorList.Add(new EdmSchemaError(Strings.DefaultTargetVersionTooLow(EntityFrameworkVersions.Default, targetFrameworkVersion), (int)System.Data.Entity.Design.SsdlGenerator.ModelBuilderErrorCode.SchemaVersionHigherThanTargetVersion, EdmSchemaErrorSeverity.Warning));
            }

            return errorList;
        }

        /// <summary>
        /// Generates the code to store the views in a C# or a VB file based on the
        /// options passed in by the user.
        /// </summary>
        /// <param name="mappingCollection"></param>
        /// <param name="generatedViews"></param>
        /// <param name="sourceWriter"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        private static void GenerateAndStoreViews(StorageMappingItemCollection mappingCollection,
            Dictionary<EntitySetBase, string> generatedViews, TextWriter sourceWriter, CodeDomProvider provider, IList<EdmSchemaError> schemaErrors)
        {
            EdmItemCollection edmCollection = mappingCollection.EdmItemCollection;
            StoreItemCollection storeCollection = mappingCollection.StoreItemCollection;

            //Create an emtpty compile unit and build up the generated code
            CodeCompileUnit compileUnit = new CodeCompileUnit();

            //Add the namespace for generated code
            CodeNamespace codeNamespace = new CodeNamespace(EntityViewGenerationConstants.NamespaceName);
            //Add copyright notice to the namespace comment.
            compileUnit.Namespaces.Add(codeNamespace);

            foreach (var storageEntityContainerMapping in mappingCollection.GetItems<StorageEntityContainerMapping>())
            {
                //Throw warning when containerMapping contains query view for 
                if (HasQueryView(storageEntityContainerMapping))
                {
                    schemaErrors.Add(new EdmSchemaError(
                        Strings.UnsupportedQueryViewInEntityContainerMapping(storageEntityContainerMapping.Identity), 
                        (int)StorageMappingErrorCode.UnsupportedQueryViewInEntityContainerMapping, 
                        EdmSchemaErrorSeverity.Warning));
                    continue;
                }

                #region Class Declaration

                string edmContainerName = storageEntityContainerMapping.EdmEntityContainer.Name;
                string storeContainerName = storageEntityContainerMapping.StorageEntityContainer.Name;

                string hashOverMappingClosure = MetadataMappingHasherVisitor.GetMappingClosureHash(edmCollection.EdmVersion, storageEntityContainerMapping);

                StringBuilder inputForTypeNameContent = new StringBuilder(hashOverMappingClosure);

                string viewStorageTypeName = EntityViewGenerationConstants.ViewGenerationTypeNamePrefix + StringHashBuilder.ComputeHash(MetadataHelper.CreateMetadataHashAlgorithm(edmCollection.EdmVersion),  inputForTypeNameContent.ToString()).ToUpperInvariant();

                //Add typeof expression to get the type that contains ViewGen type. This will help us in avoiding to go through 
                //all the types in the assembly. I have also verified that this works with VB with a root namespace prepended to the
                //namespace since VB is picking up the type correctly as long as it is in the same assembly even with out the root namespace.
                CodeTypeOfExpression viewGenTypeOfExpression = new CodeTypeOfExpression(EntityViewGenerationConstants.NamespaceName + "." + viewStorageTypeName);
                //Add the assembly attribute that marks the assembly as the one that contains the generated views
                CodeAttributeDeclaration viewGenAttribute = new CodeAttributeDeclaration(EntityViewGenerationConstants.ViewGenerationCustomAttributeName);
                CodeAttributeArgument viewGenTypeArgument = new CodeAttributeArgument(viewGenTypeOfExpression);
                viewGenAttribute.Arguments.Add(viewGenTypeArgument);
                compileUnit.AssemblyCustomAttributes.Add(viewGenAttribute);

                //Add the type which will be the class that contains all the views in this assembly
                CodeTypeDeclaration viewStoringType = CreateTypeForStoringViews(viewStorageTypeName);

                //Add the constructor, this will be the only method that this type will contain
                //Create empty constructor.
                CodeConstructor viewStoringTypeConstructor = CreateConstructorForViewStoringType();
                viewStoringType.Attributes = MemberAttributes.Public;


                //Get an expression that expresses this instance
                CodeThisReferenceExpression thisRef = new CodeThisReferenceExpression();


                string viewHash = MetadataHelper.GenerateHashForAllExtentViewsContent(edmCollection.EdmVersion, GenerateDictionaryForEntitySetNameAndView(generatedViews));


                CodeAssignStatement EdmEntityContainerNameStatement = 
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(thisRef, EntityViewGenerationConstants.EdmEntityContainerName), 
                        new CodePrimitiveExpression(edmContainerName));
                CodeAssignStatement StoreEntityContainerNameStatement =
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(thisRef, EntityViewGenerationConstants.StoreEntityContainerName),
                        new CodePrimitiveExpression(storeContainerName));
                CodeAssignStatement HashOverMappingClosureStatement =
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(thisRef, EntityViewGenerationConstants.HashOverMappingClosure),
                        new CodePrimitiveExpression(hashOverMappingClosure));
                CodeAssignStatement HashOverAllExtentViewsStatement =
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(thisRef, EntityViewGenerationConstants.HashOverAllExtentViews),
                        new CodePrimitiveExpression(viewHash));
                CodeAssignStatement ViewCountStatement =
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(thisRef, EntityViewGenerationConstants.ViewCountPropertyName),
                        new CodePrimitiveExpression(generatedViews.Count));

                viewStoringTypeConstructor.Statements.Add(EdmEntityContainerNameStatement);
                viewStoringTypeConstructor.Statements.Add(StoreEntityContainerNameStatement);
                viewStoringTypeConstructor.Statements.Add(HashOverMappingClosureStatement);
                viewStoringTypeConstructor.Statements.Add(HashOverAllExtentViewsStatement);
                viewStoringTypeConstructor.Statements.Add(ViewCountStatement);

                //Add the constructor to the type
                viewStoringType.Members.Add(viewStoringTypeConstructor);
                //Add the method to store views to the type
                CreateAndAddGetViewAtMethod(viewStoringType, generatedViews);

                //Add the type to the namespace
                codeNamespace.Types.Add(viewStoringType);

                #endregion
            }

            if (codeNamespace.Types.Count > 0)
            {
                GenerateCode(sourceWriter, provider, compileUnit);
                sourceWriter.Flush();
            }
        }

        private static bool HasQueryView(StorageEntityContainerMapping storageEntityContainerMapping)
        {
            foreach (EntitySetBase extent in storageEntityContainerMapping.EdmEntityContainer.BaseEntitySets)
            {
                if (storageEntityContainerMapping.HasQueryViewForSetMap(extent.Name))
                {
                    return true;
                }
            }
            return false;
        }

        private static Dictionary<string, string> GenerateDictionaryForEntitySetNameAndView(Dictionary<EntitySetBase, string> dictionary)
        {
            Dictionary<string, string> newDictionary = new Dictionary<string, string>();
            foreach (var item in dictionary)
            {
                newDictionary.Add(GetExtentFullName(item.Key), item.Value);
            }
            return newDictionary;
        }

        /// <summary>
        /// Write code to the given stream from the compile unit.
        /// </summary>
        /// <param name="sourceWriter"></param>
        /// <param name="provider"></param>
        /// <param name="compileUnit"></param>
        private static void GenerateCode(TextWriter sourceWriter, CodeDomProvider provider, CodeCompileUnit compileUnit)
        {
            CodeGeneratorOptions styleOptions = new CodeGeneratorOptions();
            styleOptions.BracingStyle = "C";
            styleOptions.BlankLinesBetweenMembers = true;
            styleOptions.VerbatimOrder = true;
            provider.GenerateCodeFromCompileUnit(compileUnit, sourceWriter, styleOptions);
        }

        /// <summary>
        /// Generate Code to put the views in the generated code.
        /// </summary>
        /// <param name="typeDeclaration"></param>
        /// <param name="generatedViews"></param>
        /// <returns></returns>
        private static void CreateAndAddGetViewAtMethod(CodeTypeDeclaration typeDeclaration, Dictionary<EntitySetBase, string> generatedViews)
        {

            //Add the views to a method
            CodeMemberMethod getViewAtMethod = new CodeMemberMethod();
            getViewAtMethod.Name = EntityViewGenerationConstants.GetViewAtMethodName;
            getViewAtMethod.ReturnType = new CodeTypeReference(typeof(KeyValuePair<,>).MakeGenericType(new Type[] { typeof(string), typeof(string) }));
            CodeParameterDeclarationExpression parameter = new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(int)), "index");
            getViewAtMethod.Parameters.Add(parameter);
            getViewAtMethod.Comments.Add(new CodeCommentStatement(EntityViewGenerationConstants.SummaryStartElement, true /*docComment*/));
            getViewAtMethod.Comments.Add(new CodeCommentStatement(Strings.GetViewAtMethodComments, true /*docComment*/));
            getViewAtMethod.Comments.Add(new CodeCommentStatement(EntityViewGenerationConstants.SummaryEndElement, true /*docComment*/));

            getViewAtMethod.Attributes = MemberAttributes.Family | MemberAttributes.Override;

            typeDeclaration.Members.Add(getViewAtMethod);
            
            int index = 0;
            CodeVariableReferenceExpression indexParameterReference = new CodeVariableReferenceExpression(getViewAtMethod.Parameters[0].Name);

            foreach (KeyValuePair<EntitySetBase, string> generatedViewPair in generatedViews)
            {
                // the CodeDom does not support the following scenarios
                // 1. switch statement
                // 2. if(){} else if(){}
                // The original design here was to have the following code,
                // if() { else { if(){} } }
                // but this had some drawbacks as described in TFS workitem 590996
                // Given the not supported scenarios in CodeDom, we choose only use if statement in this case

                // if(index == 0)
                CodeConditionStatement currentIf = new CodeConditionStatement(new CodeBinaryOperatorExpression(
                indexParameterReference, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(index)));

                getViewAtMethod.Statements.Add(currentIf);

                EntitySetBase entitySet = generatedViewPair.Key;
                string extentFullName = GetExtentFullName(entitySet);
                CodeMemberMethod viewMethod = CreateViewReturnMethod(extentFullName, index, generatedViewPair.Value);
                typeDeclaration.Members.Add(viewMethod);

                // return GetNorthwindContext_Customers();
                CodeMethodReturnStatement returnViewMethodCall = new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(null, viewMethod.Name)));
                currentIf.TrueStatements.Add(returnViewMethodCall);

                index++;
            }
            
            // if an invalid index is asked for throw
            getViewAtMethod.Statements.Add(new CodeThrowExceptionStatement(
                                            new CodeObjectCreateExpression(new CodeTypeReference(typeof(IndexOutOfRangeException)))));
        }

        private static CodeMemberMethod CreateViewReturnMethod(string extentFullName, int index, string viewText)
        {
            //Add the views to a method
            CodeMemberMethod viewMethod = new CodeMemberMethod();
            viewMethod.Name = "GetView" + index.ToString(CultureInfo.InvariantCulture);

            viewMethod.Attributes = MemberAttributes.Private;
            viewMethod.ReturnType = new CodeTypeReference(typeof(KeyValuePair<,>).MakeGenericType(new Type[] { typeof(string), typeof(string) }));
            viewMethod.Comments.Add(new CodeCommentStatement(EntityViewGenerationConstants.SummaryStartElement, true /*docComment*/));
            viewMethod.Comments.Add(new CodeCommentStatement(Strings.IndividualViewComments(extentFullName), true /*docComment*/));
            viewMethod.Comments.Add(new CodeCommentStatement(EntityViewGenerationConstants.SummaryEndElement, true /*docComment*/));

            CodeExpression viewTextExpression;
            // only use the StringBuilder if we have to.
            if (viewText.Length > MAXONELINELENGTH)
            {
                CreateSizedStringBuilder(viewMethod.Statements, viewText.Length);
                foreach (var appendExpression in GetAppendViewStringsExpressions(viewText))
                {
                    // viewString.Append(xxx);
                    viewMethod.Statements.Add(appendExpression);
                }

                viewTextExpression = new CodeMethodInvokeExpression(GetViewStringBuilderVariable(), "ToString");
            }
            else
            {
                viewTextExpression = new CodePrimitiveExpression(viewText);
            }

            // return new System.Collections.Generic.KeyValuePair<string, string>("dbo.Products", viewString.ToString());
            // or
            // return new System.Collections.Generic.KeyValuePair<string, string>("dbo.Products", "SELECT value c...");
            CodeObjectCreateExpression newExpression =
                new CodeObjectCreateExpression(
                    viewMethod.ReturnType,
                    new CodePrimitiveExpression(extentFullName),
                    viewTextExpression);
            viewMethod.Statements.Add(new CodeMethodReturnStatement(newExpression));

            return viewMethod;
        }

        private static void CreateSizedStringBuilder(CodeStatementCollection statements, int capacity)
        {
            // StringBuilder viewString = new StringBuilder(237);
            CodeVariableDeclarationStatement viewStringDeclaration = new CodeVariableDeclarationStatement(typeof(StringBuilder), "viewString");
            CodeObjectCreateExpression viewStringConstruct = new CodeObjectCreateExpression(typeof(StringBuilder), new CodePrimitiveExpression(capacity));
            viewStringDeclaration.InitExpression = viewStringConstruct;

            statements.Add(viewStringDeclaration);
        }

        private static IEnumerable<string> SplitViewStrings(string largeViewString)
        {
            for (int i = 0; i <= largeViewString.Length / ONELINELENGTH; i++)
            {
                if (i * ONELINELENGTH + ONELINELENGTH < largeViewString.Length)
                {
                    yield return largeViewString.Substring(i * ONELINELENGTH, ONELINELENGTH);
                }
                else
                {
                    // the very last part of the splited string
                    yield return largeViewString.Substring(i * ONELINELENGTH);
                }
            }
        }

        private static IEnumerable<CodeMethodInvokeExpression> GetViewStringsAppendToStringBuilder(
            params string[] viewStrings)
        {
            foreach (var viewString in viewStrings)
            {
                // viewString.Append("xxx");
                yield return AppendStringToStringBuilder(GetViewStringBuilderVariable(), viewString);
            }
        }

        private static CodeVariableReferenceExpression GetViewStringBuilderVariable()
        {
            return new CodeVariableReferenceExpression("viewString");
        }

        private static CodeMethodInvokeExpression AppendStringToStringBuilder(
            CodeVariableReferenceExpression stringBuilder, string stringToAppend)
        {
            return new CodeMethodInvokeExpression(
                stringBuilder, "Append", new CodePrimitiveExpression(stringToAppend));
        }

        private static IEnumerable<CodeMethodInvokeExpression> GetAppendViewStringsExpressions(string viewString)
        {
            if (viewString.Length > MAXONELINELENGTH)
            {
                // if the string is longer than 2046 charactors, we splitted them in to 80 each
                // and append them using StringBuilder
                return GetViewStringsAppendToStringBuilder(SplitViewStrings(viewString).ToArray<string>());
            }
            else
            {
                return GetViewStringsAppendToStringBuilder(viewString);
            }
        }

        private static string GetExtentFullName(EntitySetBase entitySet)
        {
            //We store the full Extent Name in the generated code which is
            //EntityContainer name + "." + entitysetName
            return entitySet.EntityContainer.Name + EntityViewGenerationConstants.QualificationCharacter + entitySet.Name;

        }

        /// <summary>
        /// Get the constructor for the type that will contain the generated views
        /// </summary>
        /// <returns></returns>
        private static CodeConstructor CreateConstructorForViewStoringType()
        {
            CodeConstructor constructor = new CodeConstructor();
            //Mark it as public
            constructor.Attributes = MemberAttributes.Public;
            //Add constructor comments
            constructor.Comments.Add(new CodeCommentStatement(EntityViewGenerationConstants.SummaryStartElement, true /*docComment*/));
            constructor.Comments.Add(new CodeCommentStatement(Strings.ConstructorComments, true /*docComment*/));
            constructor.Comments.Add(new CodeCommentStatement(EntityViewGenerationConstants.SummaryEndElement, true /*docComment*/));
            return constructor;
        }

        /// <summary>
        /// Get the type declaration for the type that will contain the views.
        /// </summary>
        /// <returns></returns>
        private static CodeTypeDeclaration CreateTypeForStoringViews(string viewStorageTypeName)
        {
            CodeTypeDeclaration typeDecl = new CodeTypeDeclaration(viewStorageTypeName);
            typeDecl.TypeAttributes = TypeAttributes.Sealed | TypeAttributes.Public;
            //This type should derive from the framework type EntityViewContainer which reduces the amount
            //of generated code
            typeDecl.BaseTypes.Add(EntityViewGenerationConstants.BaseTypeName);
            //Add type comments
            typeDecl.Comments.Add(new CodeCommentStatement(EntityViewGenerationConstants.SummaryStartElement, true /*docComment*/));
            typeDecl.Comments.Add(new CodeCommentStatement(Strings.TypeComments, true /*docComment*/));
            typeDecl.Comments.Add(new CodeCommentStatement(EntityViewGenerationConstants.SummaryEndElement, true /*docComment*/));
            return typeDecl;
        }
        #endregion
    }
}
