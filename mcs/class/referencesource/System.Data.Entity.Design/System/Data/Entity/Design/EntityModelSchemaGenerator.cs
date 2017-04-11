//---------------------------------------------------------------------
// <copyright file="EntityModelSchemaGenerator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;
using System.Data.Common;
using System.Data.Common.Utils;
using System.Data.Metadata.Edm;
using System.Data.Entity.Design.Common;
using System.Data.Entity.Design.SsdlGenerator;
using System.Reflection;
using System.Data.Entity.Design.PluralizationServices;
using Microsoft.Build.Utilities;
using System.Runtime.Versioning;
using System.Linq;

namespace System.Data.Entity.Design
{
    /// <summary>
    /// The class creates a default CCMapping between an EntityContainer in S space
    /// and an EntityContainer in C space. The Mapping will be created based on the
    /// declared types of extents. So Inheritance does not work.
    /// </summary>
    public sealed class EntityModelSchemaGenerator  
    {
        private const string ENTITY_CONTAINER_NAME_SUFFIX = "Context";
        private const string NAMESPACE_NAME_SUFFIX = "Model";
        private const string DEFAULT_NAMESPACE_NAME = "Application";

        #region Constructors
        /// <summary>
        /// Constructs an EntityModelGenerator
        /// </summary>
        /// <param name="storeEntityContainer"></param>
        public EntityModelSchemaGenerator(EntityContainer storeEntityContainer)
        {
            Initialize(storeEntityContainer, null, null, null);
        }

        /// <summary>
        /// Constructs an EntityModelGenerator
        /// </summary>
        /// <param name="storeEntityContainer">The Store EntityContainer to create the Model Metadata from.</param>
        /// <param name="namespaceName">The name to give the namespace. If null, the name of the storeEntityContainer will be used.</param>
        /// <param name="modelEntityContainerName">The name to give the Model EntityContainer. If null, a modified version of the namespace of the of a type referenced in storeEntityContainer will be used.</param>
        public EntityModelSchemaGenerator(EntityContainer storeEntityContainer, string namespaceName, string modelEntityContainerName)
        {
            EDesignUtil.CheckArgumentNull(namespaceName, "namespaceName");
            EDesignUtil.CheckArgumentNull(modelEntityContainerName, "modelEntityContainerName");
            Initialize(storeEntityContainer, null, namespaceName, modelEntityContainerName);
        }

        /// <summary>
        /// Constructs an EntityModelGenerator
        /// </summary>
        /// <param name="storeItemCollection">The StoreItemCollection that contains an EntityContainer and other items to create the Model Metadata from.</param>
        /// <param name="namespaceName">The name to give the namespace. If null, the name of the storeEntityContainer will be used.</param>
        /// <param name="modelEntityContainerName">The name to give the Model EntityContainer. If null, a modified version of the namespace of the of a type referenced in storeEntityContainer will be used.</param>
        [CLSCompliant(false)]
        public EntityModelSchemaGenerator(StoreItemCollection storeItemCollection, string namespaceName, string modelEntityContainerName)
        {
            EDesignUtil.CheckArgumentNull(storeItemCollection, "storeItemCollection");
            EDesignUtil.CheckArgumentNull(namespaceName, "namespaceName");
            EDesignUtil.CheckArgumentNull(modelEntityContainerName, "modelEntityContainerName");

            var storeContainers = storeItemCollection.GetItems<EntityContainer>().ToArray();
            if (storeContainers.Length != 1)
            {
                throw EDesignUtil.SingleStoreEntityContainerExpected("storeItemCollection");
            }

            Initialize(
                storeContainers[0],
                storeItemCollection.GetItems<EdmFunction>().Where(f => f.IsFromProviderManifest == false &&
                                                                       f.IsComposableAttribute == true &&
                                                                       f.AggregateAttribute == false),
                namespaceName,
                modelEntityContainerName);
        }

        private void Initialize(EntityContainer storeEntityContainer, IEnumerable<EdmFunction> storeFunctions, string namespaceName, string modelEntityContainerName)
        {
            EDesignUtil.CheckArgumentNull(storeEntityContainer, "storeEntityContainer");
            if (!MetadataUtil.IsStoreType(storeEntityContainer))
            {
                throw EDesignUtil.InvalidStoreEntityContainer(storeEntityContainer.Name, "storeEntityContainer");
            }

            if (namespaceName != null)
            {
                EDesignUtil.CheckStringArgument(namespaceName, "namespaceName");
                string adjustedNamespaceName = CreateValildModelNamespaceName(namespaceName);
                if (adjustedNamespaceName != namespaceName)
                {
                    // the user gave us a bad namespace name
                    throw EDesignUtil.InvalidNamespaceNameArgument(namespaceName);
                }
            }
            
            if (modelEntityContainerName != null)
            {
                EDesignUtil.CheckStringArgument(modelEntityContainerName, "modelEntityContainerName");
                string adjustedEntityContainerName = CreateModelName(modelEntityContainerName);
                if (adjustedEntityContainerName != modelEntityContainerName)
                {
                    throw EDesignUtil.InvalidEntityContainerNameArgument(modelEntityContainerName);
                }
                if (modelEntityContainerName == storeEntityContainer.Name)
                {
                    throw EDesignUtil.DuplicateEntityContainerName(modelEntityContainerName, storeEntityContainer.Name);
                }
            }

            _storeEntityContainer = storeEntityContainer;
            _storeFunctions = storeFunctions != null ? storeFunctions.ToArray() : null;
            _namespaceName = namespaceName;
            _modelEntityContainerName = modelEntityContainerName;
            this._pluralizationServiceHandler = new EntityDesignPluralizationHandler(null);

            SetupFields();
        }

        #endregion

        #region Fields
        private EntityContainer _storeEntityContainer;
        private EdmFunction[] _storeFunctions;
        private EntityContainer _modelEntityContainer = null;
        private OneToOneMappingSerializer.MappingLookups _mappingLookups = null;
        string _namespaceName = null;
        string _modelEntityContainerName = null;
        private EdmItemCollection _edmItemCollection;
        private EntityDesignPluralizationHandler _pluralizationServiceHandler = null;
        private Version _targetEntityFrameworkVersion;
        private bool _hasAnnotationNamespace;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the EntityContainer that was created
        /// </summary>
        public EntityContainer EntityContainer
        {
            get
            {
                return _modelEntityContainer;
            }
        }
        /// <summary>
        /// Gets the EntityContainer that was created
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        [CLSCompliant(false)]
        public EdmItemCollection EdmItemCollection
        {
            get
            {
                return _edmItemCollection;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pluralization")]
        public PluralizationService PluralizationService
        {
            get
            {
                Debug.Assert(this._pluralizationServiceHandler != null, "all constructor should call initialize() to set the handler");
                return this._pluralizationServiceHandler.Service;
            }
            set
            {
                this._pluralizationServiceHandler = new EntityDesignPluralizationHandler(value);
            }
        }

        /// <summary>
        /// Indicates whether foreign key properties should be exposed on entity types.
        /// </summary>
        public bool GenerateForeignKeyProperties
        {
            get;
            set;
        }

        #endregion

        #region public methods
        /// <summary>
        /// This method reads the s-space metadata objects and creates
        /// corresponding c-space metadata objects
        /// </summary>
        public IList<EdmSchemaError> GenerateMetadata()
        {
            // default the newest version
            _targetEntityFrameworkVersion = EntityFrameworkVersions.Latest;
            return InternalGenerateMetadata();
        }

        /// <summary>
        /// This method reads the s-space metadata objects and creates
        /// corresponding c-space metadata objects
        /// </summary>
        public IList<EdmSchemaError> GenerateMetadata(Version targetEntityFrameworkVersion)
        {
            EDesignUtil.CheckTargetEntityFrameworkVersionArgument(targetEntityFrameworkVersion, "targetEntityFrameworkVersion");
            _targetEntityFrameworkVersion = targetEntityFrameworkVersion;
            return InternalGenerateMetadata();
        }

        /// <summary>
        /// Writes the Schema to xml
        /// </summary>
        /// <param name="outputFileName">The name of the file to write the xml to.</param>
        public void WriteModelSchema(string outputFileName)
        {
            EDesignUtil.CheckStringArgument(outputFileName, "outputFileName");
            CheckValidSchema();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            using (XmlWriter writer = XmlWriter.Create(outputFileName, settings))
            {
                InternalWriteModelSchema(writer);
            }
        }

        /// <summary>
        /// Writes the Schema to xml.
        /// </summary>
        /// <param name="writer">The XmlWriter to write the xml to.</param>
        public void WriteModelSchema(XmlWriter writer)
        {
            EDesignUtil.CheckArgumentNull(writer, "writer");
            CheckValidSchema();
            InternalWriteModelSchema(writer);
        }

        /// <summary>
        /// Writes the cs mapping Schema to xml
        /// </summary>
        /// <param name="outputFileName">The name of the file to write the xml to.</param>
        public void WriteStorageMapping(string outputFileName)
        {
            EDesignUtil.CheckStringArgument(outputFileName, "outputFileName");
            CheckValidSchema();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            using (XmlWriter writer = XmlWriter.Create(outputFileName, settings))
            {
                InternalWriteStorageMapping(writer);
            }
        }

        /// <summary>
        /// Writes the Schema to xml.
        /// </summary>
        /// <param name="writer">The XmlWriter to write the xml to.</param>
        public void WriteStorageMapping(XmlWriter writer)
        {
            EDesignUtil.CheckArgumentNull(writer, "writer");
            CheckValidSchema();
            InternalWriteStorageMapping(writer);
        }
        #endregion

        // responsible for holding all the 
        // state for a single execution of the Load
        // method
        private class LoadMethodSessionState
        {
            public EdmItemCollection EdmItemCollection;
            public IList<EdmSchemaError> Errors = new List<EdmSchemaError>();
            public UniqueIdentifierService UsedGlobalModelTypeNames = new UniqueIdentifierService(false);
            public UniqueIdentifierService UsedEntityContainerItemNames = new UniqueIdentifierService(false);
            public Dictionary<EdmProperty, AssociationType> FkProperties = new Dictionary<EdmProperty, AssociationType>();
            public Dictionary<EntitySet, OneToOneMappingSerializer.CollapsedEntityAssociationSet> CandidateCollapsedAssociations = new Dictionary<EntitySet, OneToOneMappingSerializer.CollapsedEntityAssociationSet>();
            public OneToOneMappingSerializer.MappingLookups MappingLookups = new OneToOneMappingSerializer.MappingLookups();

            internal void AddError(string message, ModelBuilderErrorCode errorCode, EdmSchemaErrorSeverity severity, Exception e)
            {
                Debug.Assert(message != null, "message parameter is null");
                if (null == e)
                {
                    Errors.Add(new EdmSchemaError(message, (int)errorCode, severity));
                }
                else
                {
                    Errors.Add(new EdmSchemaError(message, (int)errorCode, severity, e));
                }
            }
        }

        [ResourceExposure(ResourceScope.None)] //No resource is exposed.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)] //For EdmItemCollection constructor call.  
                                                                            //Since we pass in empty collection for paths, we do not have any resource exposure here.                                                               
        private IList<EdmSchemaError> InternalGenerateMetadata()
        {
            if (_modelEntityContainer != null)
            {
                _modelEntityContainer = null;
                _mappingLookups = null;
                _edmItemCollection = null;
            }

            LoadMethodSessionState session = new LoadMethodSessionState();

            try
            {
                session.EdmItemCollection = new EdmItemCollection();
                if (this.GenerateForeignKeyProperties && this._targetEntityFrameworkVersion < EntityFrameworkVersions.Version2)
                {
                    session.AddError(Strings.UnableToGenerateForeignKeyPropertiesForV1, ModelBuilderErrorCode.UnableToGenerateForeignKeyPropertiesForV1, EdmSchemaErrorSeverity.Error, null);
                    return session.Errors;
                }

                List<AssociationSet> storeAssociationSets = new List<AssociationSet>();
                CollectAllFkProperties(session);

                EntityContainer modelEntityContainer = new EntityContainer(_modelEntityContainerName, DataSpace.CSpace);

                // create the EntityTypes and EntitySets, and save up the AssociationSets for later.
                foreach (EntitySetBase storeSet in _storeEntityContainer.BaseEntitySets)
                {
                    switch (storeSet.BuiltInTypeKind)
                    {
                        case BuiltInTypeKind.AssociationSet:
                            // save these, and create them after the EntityTypes and EntitySets have been created
                            string errorMessage;
                            if (this.GenerateForeignKeyProperties || !EntityStoreSchemaGenerator.IsFkPartiallyContainedInPK(((AssociationSet)storeSet).ElementType, out errorMessage))
                            {
                                storeAssociationSets.Add((AssociationSet)storeSet);
                            }
                            else
                            {
                                session.AddError(errorMessage, ModelBuilderErrorCode.UnsupportedForeinKeyPattern, EdmSchemaErrorSeverity.Error, null);
                            }
                            break;
                        case BuiltInTypeKind.EntitySet:
                            EntitySet set = (EntitySet)storeSet;
                            session.CandidateCollapsedAssociations.Add(set, new OneToOneMappingSerializer.CollapsedEntityAssociationSet(set));
                            break;
                        default:
                            // error
                            throw EDesignUtil.MissingGenerationPatternForType(storeSet.BuiltInTypeKind);
                    }
                }

                foreach (AssociationSet storeAssociationSet in storeAssociationSets)
                {
                    SaveAssociationForCollapsedAssociationCandidate(session, storeAssociationSet);
                }

                Set<AssociationSet> associationSetsFromCollapseCandidateRejects = new Set<AssociationSet>();
                IEnumerable<OneToOneMappingSerializer.CollapsedEntityAssociationSet> invalidCandidates = FindAllInvalidCollapsedAssociationCandidates(session);

                // now that we have gone through all of the association sets, 
                foreach (OneToOneMappingSerializer.CollapsedEntityAssociationSet collapsed in invalidCandidates)
                {
                    session.CandidateCollapsedAssociations.Remove(collapsed.EntitySet);

                    // just create the entity set and save the association set to be added later
                    EntitySet entitySet = CreateModelEntitySet(session, collapsed.EntitySet);
                    modelEntityContainer.AddEntitySetBase(entitySet);
                    associationSetsFromCollapseCandidateRejects.AddRange(collapsed.AssociationSets);                        
                }

                // create all the associations for the invalid collapsed entity association candidates
                foreach (AssociationSet storeAssociationSet in (IEnumerable<AssociationSet>)associationSetsFromCollapseCandidateRejects)
                {
                    if (!IsAssociationPartOfCandidateCollapsedAssociation(session, storeAssociationSet))
                    {
                        AssociationSet set = CreateModelAssociationSet(session, storeAssociationSet);
                        modelEntityContainer.AddEntitySetBase(set);
                    }
                }
                
                // save the set that needs to be created and mapped
                session.MappingLookups.CollapsedEntityAssociationSets.AddRange(session.CandidateCollapsedAssociations.Values);

                // do this in a seperate loop so we are sure all the necessary EntitySets have been created
                foreach (OneToOneMappingSerializer.CollapsedEntityAssociationSet collapsed in session.MappingLookups.CollapsedEntityAssociationSets)
                {
                    AssociationSet set = CreateModelAssociationSet(session, collapsed);
                    modelEntityContainer.AddEntitySetBase(set);
                }
                if (this._targetEntityFrameworkVersion >= EntityFrameworkVersions.Version2)
                {
                    Debug.Assert(EntityFrameworkVersions.Latest == EntityFrameworkVersions.Version3, "Did you add a new framework version");
                    // add LazyLoadingEnabled=true to the EntityContainer
                    MetadataProperty lazyLoadingAttribute =
                        new MetadataProperty(
                            DesignXmlConstants.EdmAnnotationNamespace + ":" + DesignXmlConstants.LazyLoadingEnabled,
                            TypeUsage.CreateStringTypeUsage(
                                PrimitiveType.GetEdmPrimitiveType(
                                    PrimitiveTypeKind.String),
                                    false,
                                    false),
                            true);
                    modelEntityContainer.AddMetadataProperties(new List<MetadataProperty>() { lazyLoadingAttribute });
                    this._hasAnnotationNamespace = true;
                }

                // Map store functions to function imports.
                MapFunctions(session, modelEntityContainer);
                
                if (!EntityStoreSchemaGenerator.HasErrorSeverityErrors(session.Errors))
                {
                    // add them to the collection so they will work if someone wants to use the collection
                    foreach (EntityType type in session.MappingLookups.StoreEntityTypeToModelEntityType.Values)
                    {
                        type.SetReadOnly();
                        session.EdmItemCollection.AddInternal(type);
                    }

                    foreach (AssociationType type in session.MappingLookups.StoreAssociationTypeToModelAssociationType.Values)
                    {
                        type.SetReadOnly();
                        session.EdmItemCollection.AddInternal(type);
                    }

                    foreach (OneToOneMappingSerializer.CollapsedEntityAssociationSet set in session.MappingLookups.CollapsedEntityAssociationSets)
                    {
                        set.ModelAssociationSet.ElementType.SetReadOnly();
                        session.EdmItemCollection.AddInternal(set.ModelAssociationSet.ElementType);
                    }
                    modelEntityContainer.SetReadOnly();
                    session.EdmItemCollection.AddInternal(modelEntityContainer);

                    _modelEntityContainer = modelEntityContainer;
                    _mappingLookups = session.MappingLookups;
                    _edmItemCollection = session.EdmItemCollection;
                }

            }
            catch (Exception e)
            {
                if (MetadataUtil.IsCatchableExceptionType(e))
                {
                    // an exception in the code is definitely an error
                    string message = EDesignUtil.GetMessagesFromEntireExceptionChain(e);
                    session.AddError(message,
                    ModelBuilderErrorCode.UnknownError,
                    EdmSchemaErrorSeverity.Error,
                    e);
                }
                else
                {
                    throw;
                }

            }
            return session.Errors;
        }

        private void InternalWriteModelSchema(XmlWriter writer)
        {
            Debug.Assert(writer != null, "writer != null");
            Debug.Assert(_edmItemCollection != null, "_edmItemCollection != null");
            Debug.Assert(_namespaceName != null, "_namespaceName != null");
            Debug.Assert(_targetEntityFrameworkVersion != null, "_targetEntityFrameworkVersion != null");

            if (this._hasAnnotationNamespace)
            {
                KeyValuePair<string, string> namespacesPrefix = new KeyValuePair<string, string>(DesignXmlConstants.AnnotationPrefix, DesignXmlConstants.EdmAnnotationNamespace);
                MetadataItemSerializer.WriteXml(writer, _edmItemCollection, _namespaceName, _targetEntityFrameworkVersion, namespacesPrefix);
            }
            else
            {
                MetadataItemSerializer.WriteXml(writer, _edmItemCollection, _namespaceName, _targetEntityFrameworkVersion);
            }
        }

        private void InternalWriteStorageMapping(XmlWriter writer)
        {
            Debug.Assert(writer != null, "writer != null");
            Debug.Assert(_mappingLookups != null, "_mappingLookups != null");
            Debug.Assert(_storeEntityContainer != null, "_storeEntityContainer != null");
            Debug.Assert(_modelEntityContainer != null, "_modelEntityContainer != null");
            Debug.Assert(_targetEntityFrameworkVersion != null, "_targetEntityFrameworkVersion != null");
            OneToOneMappingSerializer serializer = new OneToOneMappingSerializer(_mappingLookups, _storeEntityContainer, _modelEntityContainer, _targetEntityFrameworkVersion);
            serializer.WriteXml(writer);
        }

        private void MapFunctions(LoadMethodSessionState session, EntityContainer modelEntityContainer)
        {
            if (_storeFunctions == null || _storeFunctions.Length == 0 || _targetEntityFrameworkVersion < EntityFrameworkVersions.Version3)
            {
                return;
            }

            //
            // Create function imports and appropriate complex types for return parameters and add them to the item collection (session.EdmItemCollection).
            // Create and add function mappings.
            //

            var interestingStoreFunctions = _storeFunctions.Where(
                f => f.IsComposableAttribute &&
                     !f.AggregateAttribute &&
                     f.Parameters.All(p => p.Mode == ParameterMode.In));
            foreach (var storeFunction in interestingStoreFunctions)
            {
                RowType tvfReturnType = TypeHelpers.GetTvfReturnType(storeFunction);
                if (tvfReturnType == null)
                {
                    continue;
                }

                // Create function import name.
                string functionImportName = CreateModelName(storeFunction.Name, session.UsedEntityContainerItemNames);
               
                // Create function import parameters.
                UniqueIdentifierService usedParameterNames = new UniqueIdentifierService(false);
                var parameters = storeFunction.Parameters.Select(p => CreateFunctionImportParameter(p, usedParameterNames)).ToArray();
                var failedStoreParameterName = storeFunction.Parameters.Select(p => p.Name).Except(parameters.Select(p => p.Name)).FirstOrDefault();
                if (failedStoreParameterName != null)
                {
                    session.AddError(Strings.UnableToGenerateFunctionImportParameterName(failedStoreParameterName, storeFunction.Identity),
                        ModelBuilderErrorCode.UnableToGenerateFunctionImportParameterName, EdmSchemaErrorSeverity.Warning, null);
                    continue;
                }

                // Create new complex type and register it in the item collection.
                var complexType = CreateModelComplexTypeForTvfResult(session, functionImportName, tvfReturnType);
                complexType.SetReadOnly();
                session.EdmItemCollection.AddInternal(complexType);

                var collectionType = complexType.GetCollectionType();
                collectionType.SetReadOnly();
                var returnTypeUsage = TypeUsage.Create(collectionType);

                // Create function import and register it in the item collection.
                var functionImport = new EdmFunction(functionImportName, _modelEntityContainerName, DataSpace.CSpace, new EdmFunctionPayload()
                {
                    Name = functionImportName,
                    NamespaceName = _namespaceName,
                    ReturnParameters = new FunctionParameter[] {new FunctionParameter(EdmConstants.ReturnType, returnTypeUsage, ParameterMode.ReturnValue)},
                    Parameters = parameters,
                    DataSpace = DataSpace.CSpace,
                    IsComposable = true,
                    IsFunctionImport = true
                });
                functionImport.SetReadOnly();
                modelEntityContainer.AddFunctionImport(functionImport);

                // Add mapping tuple.
                session.MappingLookups.StoreFunctionToFunctionImport.Add(Tuple.Create(storeFunction, functionImport));
            }
        }

        private FunctionParameter CreateFunctionImportParameter(FunctionParameter storeParameter, UniqueIdentifierService usedParameterNames)
        {
            Debug.Assert(storeParameter.Mode == ParameterMode.In, "Function import mapping is supported only for 'In' parameters.");
            string name = CreateModelName(storeParameter.Name, usedParameterNames);
            TypeUsage cspaceTypeUsage = storeParameter.TypeUsage.GetModelTypeUsage();
            var modelParameter = new FunctionParameter(name, cspaceTypeUsage, storeParameter.Mode);
            return modelParameter;
        }

        private ComplexType CreateModelComplexTypeForTvfResult(LoadMethodSessionState session, string functionImportName, RowType tvfReturnType)
        {
            Debug.Assert(MetadataUtil.IsStoreType(tvfReturnType), "this is not a store type");

            // create all the properties
            List<EdmMember> members = new List<EdmMember>();
            UniqueIdentifierService usedPropertyNames = new UniqueIdentifierService(false);

            string name = CreateModelName(functionImportName + "_Result", session.UsedGlobalModelTypeNames);

            // Don't want to have a property with the same name as the complex type
            usedPropertyNames.RegisterUsedIdentifier(name);
            foreach (EdmProperty storeProperty in tvfReturnType.Properties)
            {
                EdmProperty property = CreateModelProperty(session, storeProperty, usedPropertyNames);
                members.Add(property);
            }

            var complexType = new ComplexType(name, _namespaceName, DataSpace.CSpace);
            foreach (var m in members)
            {
                complexType.AddMember(m);
            }
            return complexType;
        }

        private IEnumerable<OneToOneMappingSerializer.CollapsedEntityAssociationSet> FindAllInvalidCollapsedAssociationCandidates(LoadMethodSessionState session)
        {
            Set<OneToOneMappingSerializer.CollapsedEntityAssociationSet> invalid = new Set<OneToOneMappingSerializer.CollapsedEntityAssociationSet>();
            Dictionary<EntitySet, OneToOneMappingSerializer.CollapsedEntityAssociationSet> newCandidates = new Dictionary<EntitySet,OneToOneMappingSerializer.CollapsedEntityAssociationSet>();
            foreach (OneToOneMappingSerializer.CollapsedEntityAssociationSet collapsed in session.CandidateCollapsedAssociations.Values)
            {
                if (!collapsed.MeetsRequirementsForCollapsableAssociation)
                {
                    invalid.Add(collapsed);
                }
                else
                {
                    newCandidates.Add(collapsed.EntitySet, collapsed);
                }
            }


            foreach (OneToOneMappingSerializer.CollapsedEntityAssociationSet collapsed in newCandidates.Values)
            {
                foreach (AssociationSet set in collapsed.AssociationSets)
                {
                    EntitySet end0Set = set.AssociationSetEnds[0].EntitySet;
                    EntitySet end1Set = set.AssociationSetEnds[1].EntitySet;
                   
                    // if both ends of the association are candidates throw both candidates out
                    // because we can't collapse two entities out
                    // and we don't know which entity we should collapse 
                    // so we won't collapse either
                    if (newCandidates.ContainsKey(end0Set) &&
                        newCandidates.ContainsKey(end1Set))
                    {
                        OneToOneMappingSerializer.CollapsedEntityAssociationSet collapsed0 = session.CandidateCollapsedAssociations[end0Set];
                        OneToOneMappingSerializer.CollapsedEntityAssociationSet collapsed1 = session.CandidateCollapsedAssociations[end1Set];
                        invalid.Add(collapsed0);
                        invalid.Add(collapsed1);
                    }

                }
            }

            return invalid;
        }

        private bool IsAssociationPartOfCandidateCollapsedAssociation(LoadMethodSessionState session, AssociationSet storeAssociationSet)
        {
            foreach (AssociationSetEnd end in storeAssociationSet.AssociationSetEnds)
            {
                if (session.CandidateCollapsedAssociations.ContainsKey(end.EntitySet))
                {
                    return true;
                }
            }

            return false;
        }

        private void SaveAssociationForCollapsedAssociationCandidate(LoadMethodSessionState session, AssociationSet storeAssociationSet)
        {
            foreach (AssociationSetEnd end in storeAssociationSet.AssociationSetEnds)
            {
                OneToOneMappingSerializer.CollapsedEntityAssociationSet collapsed;
                if (session.CandidateCollapsedAssociations.TryGetValue(end.EntitySet, out collapsed))
                {
                    collapsed.AssociationSets.Add(storeAssociationSet);
                }
            }
        }

        private void CollectAllFkProperties(LoadMethodSessionState session)
        {
            foreach (EntitySetBase storeSet in _storeEntityContainer.BaseEntitySets)
            {
                if (storeSet.BuiltInTypeKind == BuiltInTypeKind.AssociationSet)
                {
                    ReferentialConstraint constraint = OneToOneMappingSerializer.GetReferentialConstraint(((AssociationSet)storeSet));
                    foreach (EdmProperty property in constraint.ToProperties)
                    {
                        if (!session.FkProperties.ContainsKey(property))
                        {
                            session.FkProperties.Add(property, ((AssociationSet)storeSet).ElementType);
                        }
                    }
                }
            }
        }

        // Get ModelSchemaNamespace name
        private void SetupFields()
        {
            if( _modelEntityContainerName != null && _namespaceName != null)
            {
                return;
            }

            string targetSchemaNamespace = null;

            //Try and get the target schema namespace from one of the types or functions defined in the schema
            foreach(EntitySetBase type in _storeEntityContainer.BaseEntitySets)
            {
                targetSchemaNamespace = type.ElementType.NamespaceName;
                break;
            }


            if (string.IsNullOrEmpty(targetSchemaNamespace))
            {
                // the default
                targetSchemaNamespace = DEFAULT_NAMESPACE_NAME;
            }

            
            if(_namespaceName == null)
            {
                // if the schema namespace has 'Target' then replace it with '.Model
                int index = targetSchemaNamespace.IndexOf("Target", StringComparison.OrdinalIgnoreCase);
                if (index > 0)
                {
                    _namespaceName = CreateValildModelNamespaceName(targetSchemaNamespace.Substring(0, index) + NAMESPACE_NAME_SUFFIX);
                }
                else
                {
                    // else just append .Model to the name
                    _namespaceName = CreateValildModelNamespaceName(targetSchemaNamespace + NAMESPACE_NAME_SUFFIX);
                }
            }


            if(_modelEntityContainerName == null)
            {
                // get default container name
                int dotIndex = targetSchemaNamespace.IndexOf('.');
                if (dotIndex > 0)
                {
                    _modelEntityContainerName = CreateModelName(targetSchemaNamespace.Substring(0, dotIndex) + ENTITY_CONTAINER_NAME_SUFFIX);
                }
                else
                {
                    _modelEntityContainerName = CreateModelName(targetSchemaNamespace + ENTITY_CONTAINER_NAME_SUFFIX);
                }

                int targetIndex = _modelEntityContainerName.IndexOf("Target", StringComparison.OrdinalIgnoreCase);
                if (targetIndex > 0)
                {
                    _modelEntityContainerName = CreateModelName(targetSchemaNamespace.Substring(0, targetIndex) + ENTITY_CONTAINER_NAME_SUFFIX);
                }
            }
        }

        private void CheckValidSchema()
        {
            if (_modelEntityContainer == null)
            {
                throw EDesignUtil.EntityModelGeneratorSchemaNotLoaded();
            }
        }

        private EntitySet CreateModelEntitySet(LoadMethodSessionState session, EntitySet storeEntitySet)
        {
            EntityType entity = CreateModelEntityType(session, storeEntitySet.ElementType);

            string name = CreateModelName(this._pluralizationServiceHandler.GetEntitySetName(storeEntitySet.Name), session.UsedEntityContainerItemNames);
            
            EntitySet set = new EntitySet(name, null, null, null, entity);
            session.MappingLookups.StoreEntitySetToModelEntitySet.Add(storeEntitySet, set);
            return set;
        }

        private EntityType CreateModelEntityType(LoadMethodSessionState session, EntityType storeEntityType)
        {
            Debug.Assert(MetadataUtil.IsStoreType(storeEntityType), "this is not a store type");
            Debug.Assert(storeEntityType.BaseType == null, "we are assuming simple generation from a database where no types will have a base type");

            EntityType foundEntity;
            if (session.MappingLookups.StoreEntityTypeToModelEntityType.TryGetValue(storeEntityType, out foundEntity))
            {
                // this entity type is used in two different entity sets
                return foundEntity;
            }

            // create all the properties
            List<EdmMember> members = new List<EdmMember>();
            List<String> keyMemberNames = new List<string>();
            UniqueIdentifierService usedPropertyNames = new UniqueIdentifierService(false);

            string name = CreateModelName(this._pluralizationServiceHandler.GetEntityTypeName(storeEntityType.Name), session.UsedGlobalModelTypeNames);

            // Don't want to have a property with the same name as the entity type
            usedPropertyNames.RegisterUsedIdentifier(name);
            foreach (EdmProperty storeProperty in storeEntityType.Properties)
            {
                // add fk properties only if requested
                EdmMember member;
                bool isKey = storeEntityType.KeyMembers.TryGetValue(storeProperty.Name, false, out member);

                AssociationType association;
                if (isKey || this.GenerateForeignKeyProperties || !session.FkProperties.TryGetValue(storeProperty, out association))
                {
                    EdmProperty property = CreateModelProperty(session, storeProperty, usedPropertyNames);
                    members.Add(property);
                    if (isKey)
                    {
                        keyMemberNames.Add(property.Name);
                    }
                }
            }

            var entityType = new EntityType(name, _namespaceName, DataSpace.CSpace, keyMemberNames, members);
            session.MappingLookups.StoreEntityTypeToModelEntityType.Add(storeEntityType, entityType);
            return entityType;
        }

        private EdmProperty CreateModelProperty(LoadMethodSessionState session, EdmProperty storeProperty, UniqueIdentifierService usedPropertyNames)
        {
            string name = CreateModelName(storeProperty.Name, usedPropertyNames);
            TypeUsage cspaceTypeUsage = storeProperty.TypeUsage.GetModelTypeUsage();

            EdmProperty property = new EdmProperty(name, cspaceTypeUsage);

            this.AddStoreGeneratedPatternAnnoation(property, storeProperty.TypeUsage);

            session.MappingLookups.StoreEdmPropertyToModelEdmProperty.Add(storeProperty, property);
            return property;
        }

        private void AddStoreGeneratedPatternAnnoation(EdmProperty cSpaceProperty, TypeUsage storeTypeUsage)
        {
            if (storeTypeUsage.Facets.Contains(DesignXmlConstants.StoreGeneratedPattern))
            {
                List<MetadataProperty> annotation = new List<MetadataProperty>();
                annotation.Add(
                    new MetadataProperty(
                        DesignXmlConstants.EdmAnnotationNamespace + ":" + DesignXmlConstants.StoreGeneratedPattern, 
                        TypeUsage.CreateStringTypeUsage(
                            PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String),
                            false/*isUnicode*/, false/*isFixLength*/), 
                        storeTypeUsage.Facets.GetValue(DesignXmlConstants.StoreGeneratedPattern, false).Value));
                
                cSpaceProperty.AddMetadataProperties(annotation);
                this._hasAnnotationNamespace = true;
            }
        }

        private AssociationSet CreateModelAssociationSet(LoadMethodSessionState session, OneToOneMappingSerializer.CollapsedEntityAssociationSet collapsedAssociationSet)
        {
            // create the association
            string associationName = CreateModelName(collapsedAssociationSet.EntitySet.Name, session.UsedGlobalModelTypeNames);
            AssociationType association = new AssociationType(associationName,
                _namespaceName, false, DataSpace.CSpace);

            // create the association set
            string associationSetName = CreateModelName(collapsedAssociationSet.EntitySet.Name, session.UsedEntityContainerItemNames);
            AssociationSet set = new AssociationSet(associationSetName, association);
            
            // create the association and association set end members
            UniqueIdentifierService usedEndMemberNames = new UniqueIdentifierService(false);
            for(int i = 0; i < collapsedAssociationSet.AssociationSets.Count; i++)
            {
                AssociationSetEnd storeEnd;
                RelationshipMultiplicity multiplicity;
                OperationAction deleteBehavior;
                collapsedAssociationSet.GetStoreAssociationSetEnd(i, out storeEnd, out multiplicity, out deleteBehavior);
                AssociationEndMember end = CreateAssociationEndMember(session, storeEnd.CorrespondingAssociationEndMember, usedEndMemberNames, multiplicity, deleteBehavior);
                association.AddMember(end);

                EntitySet entitySet = session.MappingLookups.StoreEntitySetToModelEntitySet[storeEnd.EntitySet];
                AssociationSetEnd setEnd = new AssociationSetEnd(entitySet, set, end);
                set.AddAssociationSetEnd(setEnd);
                session.MappingLookups.StoreAssociationSetEndToModelAssociationSetEnd.Add(storeEnd, setEnd);
            }

            // don't need a referential constraint

            CreateModelNavigationProperties(session, association);

            collapsedAssociationSet.ModelAssociationSet = set;
           
            return set;
        }

        private AssociationSet CreateModelAssociationSet(LoadMethodSessionState session, AssociationSet storeAssociationSet)
        {

            AssociationType association;
            // we will get a value when the same association is used for multiple association sets
            if (! session.MappingLookups.StoreAssociationTypeToModelAssociationType.TryGetValue(storeAssociationSet.ElementType, out association))
            {
                association = CreateModelAssociationType(session, storeAssociationSet.ElementType);
                session.MappingLookups.StoreAssociationTypeToModelAssociationType.Add(storeAssociationSet.ElementType, association);
            }

            string name = CreateModelName(storeAssociationSet.Name, session.UsedEntityContainerItemNames);
            AssociationSet set = new AssociationSet(name, association);
            
            foreach(AssociationSetEnd storeEnd in storeAssociationSet.AssociationSetEnds)
            {
                AssociationSetEnd end = CreateModelAssociationSetEnd(session, storeEnd, set);
                session.MappingLookups.StoreAssociationSetEndToModelAssociationSetEnd.Add(storeEnd, end);
                set.AddAssociationSetEnd(end);
            }
            session.MappingLookups.StoreAssociationSetToModelAssociationSet.Add(storeAssociationSet, set);
            return set;
        }

        private AssociationSetEnd CreateModelAssociationSetEnd(LoadMethodSessionState session, AssociationSetEnd storeEnd, AssociationSet parentModelAssociationSet)
        {
            AssociationEndMember associationEnd = session.MappingLookups.StoreAssociationEndMemberToModelAssociationEndMember[storeEnd.CorrespondingAssociationEndMember];
            EntitySet entitySet = session.MappingLookups.StoreEntitySetToModelEntitySet[storeEnd.EntitySet];
            string role = associationEnd.Name;
            AssociationSetEnd end = new AssociationSetEnd(entitySet, parentModelAssociationSet, associationEnd);
            return end;
        }

        private AssociationType CreateModelAssociationType(LoadMethodSessionState session, AssociationType storeAssociationType)
        {
            UniqueIdentifierService usedEndMemberNames = new UniqueIdentifierService(false);
            string name = CreateModelName(storeAssociationType.Name, session.UsedGlobalModelTypeNames);

            bool isFkAssociation = false;
            if (_targetEntityFrameworkVersion > EntityFrameworkVersions.Version1)
            {
                isFkAssociation = this.GenerateForeignKeyProperties || RequiresModelReferentialConstraint(storeAssociationType);
            }
            AssociationType association = new AssociationType(name,
                _namespaceName,
                isFkAssociation,
                DataSpace.CSpace);
            KeyValuePair<string, RelationshipMultiplicity> endMultiplicityOverride = CreateEndMultiplicityOverride(session, storeAssociationType, association);

            foreach (AssociationEndMember storeEndMember in storeAssociationType.RelationshipEndMembers)
            {
                AssociationEndMember end = CreateAssociationEndMember(session, storeEndMember, endMultiplicityOverride, usedEndMemberNames);
                session.MappingLookups.StoreAssociationEndMemberToModelAssociationEndMember.Add(storeEndMember, end);
                association.AddMember(end);
            }

            ReferentialConstraint constraint = CreateReferentialConstraint(session, storeAssociationType);
            if (constraint != null)
            {
                association.AddReferentialConstraint(constraint);
            }

            CreateModelNavigationProperties(session, association);

            return association;
        }

        private KeyValuePair<string, RelationshipMultiplicity> CreateEndMultiplicityOverride(LoadMethodSessionState session, AssociationType storeAssociation, AssociationType modelAssociation)
        {
            // does the store have any constraints
            if (storeAssociation.ReferentialConstraints.Count == 0)
            {
                return new KeyValuePair<string, RelationshipMultiplicity>();
            }

            ReferentialConstraint storeConstraint = storeAssociation.ReferentialConstraints[0];

            //For foreign key associations, having any nullable columns will imply 0..1
            //multiplicity, while for independent associations, all columns must be non-nullable for 
            //0..1 association.
            bool nullableColumnsImplyingOneToOneMultiplicity = false;
            if (this.GenerateForeignKeyProperties)
            {
                nullableColumnsImplyingOneToOneMultiplicity = storeConstraint.ToProperties.All(p => p.Nullable == false);
            }
            else
            {
                nullableColumnsImplyingOneToOneMultiplicity = storeConstraint.ToProperties.Any(p => p.Nullable == false);
            }

            if (storeConstraint.FromRole.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne &&
               storeConstraint.ToRole.RelationshipMultiplicity == RelationshipMultiplicity.Many &&
               nullableColumnsImplyingOneToOneMultiplicity)
            {
                return new KeyValuePair<string, RelationshipMultiplicity>(storeConstraint.FromRole.Name, RelationshipMultiplicity.One);
            }

            return new KeyValuePair<string, RelationshipMultiplicity>();
        }

        private ReferentialConstraint CreateReferentialConstraint(LoadMethodSessionState session, AssociationType storeAssociation)
        {
            Debug.Assert(session != null, "session parameter is null");
            Debug.Assert(storeAssociation != null, "storeAssociation parameter is null");
            Debug.Assert(storeAssociation.ReferentialConstraints.Count <= 1, "We don't have a reason to have more than one constraint yet");

            // does the store have any constraints
            if (storeAssociation.ReferentialConstraints.Count == 0)
            {    
                return null;
            }

            ReferentialConstraint storeConstraint = storeAssociation.ReferentialConstraints[0];
            Debug.Assert(storeConstraint.FromProperties.Count == storeConstraint.ToProperties.Count, "FromProperties and ToProperties have different counts");
            Debug.Assert(storeConstraint.FromProperties.Count != 0, "No properties in the constraint, why does the constraint exist?");
            Debug.Assert(storeConstraint.ToProperties[0].DeclaringType.BuiltInTypeKind == BuiltInTypeKind.EntityType, "The property is not from an EntityType");

            EntityType toType = (EntityType)storeConstraint.ToProperties[0].DeclaringType;
            // If we are generating foreign keys, there is always a referential constraint. Otherwise, check
            // if the dependent end includes key properties. If so, this implies that there is a referential
            // constraint. Otherwise, it is assumed that the foreign key properties are not defined in the 
            // entity (verified ealier).
            if (!this.GenerateForeignKeyProperties && !RequiresModelReferentialConstraint(storeConstraint, toType)) 
            {
                return null;
            }
            // we need a constraint so lets build it
            int count = storeConstraint.FromProperties.Count;
            EdmProperty[] fromProperties = new EdmProperty[count];
            EdmProperty[] toProperties = new EdmProperty[count];
            AssociationEndMember fromRole = session.MappingLookups.StoreAssociationEndMemberToModelAssociationEndMember[(AssociationEndMember)storeConstraint.FromRole];
            AssociationEndMember toRole = session.MappingLookups.StoreAssociationEndMemberToModelAssociationEndMember[(AssociationEndMember)storeConstraint.ToRole];
            for (int index = 0; index < count; index++)
            {
                fromProperties[index] = session.MappingLookups.StoreEdmPropertyToModelEdmProperty[storeConstraint.FromProperties[index]];
                toProperties[index] = session.MappingLookups.StoreEdmPropertyToModelEdmProperty[storeConstraint.ToProperties[index]];
            }

            ReferentialConstraint newConstraint = new ReferentialConstraint(
                fromRole,
                toRole,
                fromProperties,
                toProperties);

            return newConstraint;
        }
        
        private static bool RequiresModelReferentialConstraint(AssociationType storeAssociation)
        {
            // does the store have any constraints
            if (storeAssociation.ReferentialConstraints.Count == 0)
            {
                return false;
            }

            ReferentialConstraint storeConstraint = storeAssociation.ReferentialConstraints[0];
            Debug.Assert(storeConstraint.FromProperties.Count == storeConstraint.ToProperties.Count, "FromProperties and ToProperties have different counts");
            Debug.Assert(storeConstraint.FromProperties.Count != 0, "No properties in the constraint, why does the constraint exist?");
            Debug.Assert(storeConstraint.ToProperties[0].DeclaringType.BuiltInTypeKind == BuiltInTypeKind.EntityType, "The property is not from an EntityType");

            EntityType toType = (EntityType)storeConstraint.ToProperties[0].DeclaringType;
            return RequiresModelReferentialConstraint(storeConstraint, toType);
        }

        private static bool RequiresModelReferentialConstraint(ReferentialConstraint storeConstraint, EntityType toType)
        {
            return toType.KeyMembers.Contains(storeConstraint.ToProperties[0]);
        }

        private void CreateModelNavigationProperties(LoadMethodSessionState session, AssociationType association)
        {
            Debug.Assert(association.Members.Count == 2, "this code assumes two ends");
            AssociationEndMember end1 = (AssociationEndMember)association.Members[0];
            AssociationEndMember end2 = (AssociationEndMember)association.Members[1];

            CreateModelNavigationProperty(session, end1, end2);
            CreateModelNavigationProperty(session, end2, end1);
        }

        private void CreateModelNavigationProperty(LoadMethodSessionState session, AssociationEndMember from, AssociationEndMember to)
        {
            EntityType entityType = (EntityType)((RefType)from.TypeUsage.EdmType).ElementType;
            UniqueIdentifierService usedMemberNames = new UniqueIdentifierService(false);
            LoadNameLookupWithUsedMemberNames(entityType, usedMemberNames);
            string name = CreateModelName(this._pluralizationServiceHandler.GetNavigationPropertyName(to, to.Name), usedMemberNames);
            NavigationProperty navigationProperty = new NavigationProperty(name, to.TypeUsage);
            navigationProperty.RelationshipType = (AssociationType)to.DeclaringType;
            navigationProperty.ToEndMember = to;
            navigationProperty.FromEndMember = from;
            entityType.AddMember(navigationProperty);
        }

        private void LoadNameLookupWithUsedMemberNames(EntityType entityType, UniqueIdentifierService usedMemberNames)
        {
            // a property should not have the same name as its entity
            usedMemberNames.RegisterUsedIdentifier(entityType.Name);
            foreach (EdmMember member in entityType.Members)
            {
                usedMemberNames.RegisterUsedIdentifier(member.Name);
            }
        }

        private AssociationEndMember CreateAssociationEndMember(LoadMethodSessionState session, AssociationEndMember storeEndMember, KeyValuePair<string, RelationshipMultiplicity> endMultiplicityOverride,  UniqueIdentifierService usedEndMemberNames)
        {
            RelationshipMultiplicity multiplicity = storeEndMember.RelationshipMultiplicity;
            if (endMultiplicityOverride.Key != null && endMultiplicityOverride.Key == storeEndMember.Name)
            {
                multiplicity = endMultiplicityOverride.Value;
            }
            return CreateAssociationEndMember(session, storeEndMember, usedEndMemberNames, multiplicity, storeEndMember.DeleteBehavior);
        }

        private AssociationEndMember CreateAssociationEndMember(LoadMethodSessionState session, AssociationEndMember storeEndMember, UniqueIdentifierService usedEndMemberNames, RelationshipMultiplicity multiplicityOverride, OperationAction deleteBehaviorOverride)
        {
            Debug.Assert(storeEndMember.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.RefType, "The type is not a ref type");
            Debug.Assert(((RefType)storeEndMember.TypeUsage.EdmType).ElementType.BuiltInTypeKind == BuiltInTypeKind.EntityType, "the ref is not holding on to an EntityType");
            
            EntityType storeEntityType = ((EntityType)((RefType)storeEndMember.TypeUsage.EdmType).ElementType);
            EntityType modelEntityType = session.MappingLookups.StoreEntityTypeToModelEntityType[storeEntityType];

            string name = CreateModelName(storeEndMember.Name, usedEndMemberNames);
            AssociationEndMember end = new AssociationEndMember(name, modelEntityType.GetReferenceType(), multiplicityOverride);
            end.DeleteBehavior = deleteBehaviorOverride;
            return end;            
        }

        private string CreateModelName(string storeName, UniqueIdentifierService usedNames)
        {
            string newName = CreateModelName(storeName);
            newName = usedNames.AdjustIdentifier(newName);
            return newName;            
        }

        private static string CreateValildModelNamespaceName(string storeNamespaceName)
        {
            return CreateValidNamespaceName(storeNamespaceName, 'C');
        }

        internal static string CreateValidNamespaceName(string storeNamespaceName, char appendToFrontIfFirstCharIsInvalid)
        {
            List<string> namespaceParts = new List<string>();
            foreach (string sPart in storeNamespaceName.Split(new char[] { '.' }))
            {
                // each part of a namespace needs to be a valid 
                // cspace name
                namespaceParts.Add(CreateValidEcmaName(sPart, appendToFrontIfFirstCharIsInvalid));
            }

            string modelNamespaceName = "";
            for (int i = 0; i < namespaceParts.Count - 1; i++)
            {
                modelNamespaceName += namespaceParts[i] + ".";
            }
            modelNamespaceName += namespaceParts[namespaceParts.Count - 1];


            // We might get a clash in names if ssdl has two types named #table and $table. Both will generate C_table
            // We leave it to the calling method to resolve any name clashes
            return modelNamespaceName;
        }
 
        //This method maps invalid characters such as &^%,etc in order to generate valid names
        private static string CreateModelName(string storeName)
        {
            return CreateValidEcmaName(storeName, 'C');
        }
        
        internal static string CreateValidEcmaName(string name, char appendToFrontIfFirstCharIsInvalid)
        {
            char[] ecmaNameArray = name.ToCharArray();
            for (int i = 0; i < ecmaNameArray.Length; i++)
            {
                // replace non -(letters or digits) with _ ( underscore )
                if (!char.IsLetterOrDigit(ecmaNameArray[i]))
                {
                    ecmaNameArray[i] = '_';
                }
            }

            string ecmaName = new string(ecmaNameArray);
            // the first letter in a part should only be a char
            // if the part is empty then implies that we have the situation like ".abc", "abc.", "ab..c", 
            // neither of them are accepted by the schema
            if (string.IsNullOrEmpty(name) || !char.IsLetter(ecmaName[0]))
            {
                ecmaName = appendToFrontIfFirstCharIsInvalid + ecmaName;
            }

            return ecmaName;
        }
    }
}
