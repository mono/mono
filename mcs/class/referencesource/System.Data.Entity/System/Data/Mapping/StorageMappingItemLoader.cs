//---------------------------------------------------------------------
// <copyright file="StorageMappingItemLoader.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common.Utils;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Data.Entity;

namespace System.Data.Mapping
{
    using Triple = Pair<EntitySetBase, Pair<EntityTypeBase, bool>>;

    /// <summary>
    /// The class loads an MSL file into memory and exposes CSMappingMetadata interfaces.
    /// The primary consumers of the interfaces are view genration and tools.
    /// </summary>
    /// <example>
    /// For Example if conceptually you could represent the CS MSL file as following
    /// --Mapping 
    ///   --EntityContainerMapping ( CNorthwind-->SNorthwind )
    ///     --EntitySetMapping
    ///       --EntityTypeMapping
    ///         --TableMappingFragment
    ///           --EntityKey
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///           --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///           --DiscriminatorProperyMap ( constant value-->SMemberMetadata )
    ///       --EntityTypeMapping
    ///         --TableMappingFragment
    ///           --EntityKey
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///           --ComplexPropertyMap
    ///             --ComplexTypeMap
    ///               --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///               --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    ///           --DiscriminatorProperyMap ( constant value-->SMemberMetadata )
    ///     --AssociationSetMapping 
    ///       --AssociationTypeMapping
    ///         --TableMappingFragment
    ///           --EndPropertyMap
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///             --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    ///           --EndPropertyMap
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///   --EntityContainerMapping ( CMyDatabase-->SMyDatabase )
    ///     --CompositionSetMapping
    ///       --CompositionTypeMapping
    ///         --TableMappingFragment
    ///           --ParentEntityKey
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///           --EntityKey
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///             --ScalarPropertyMap ( CMemberMetadata-->Constant value )
    ///           --ComplexPropertyMap
    ///             --ComplexTypeMap
    ///               --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///               --DiscriminatorProperyMap ( constant value-->SMemberMetadata )
    ///           --ScalarPropertyMap ( CMemberMetadata-->Constant value )
    /// The CCMappingSchemaLoader loads an Xml file that has a conceptual structure
    /// equivalent to the above example into in-memory data structure in a
    /// top-dwon approach.
    /// </example>
    /// <remarks>
    /// The loader uses XPathNavigator to parse the XML. The advantage of using XPathNavigator
    /// over DOM is that it exposes the line number of the current xml content.
    /// This is really helpful when throwing exceptions. Another advantage is
    /// </remarks>
    internal class StorageMappingItemLoader
    {
        #region Constructors
        /// <summary>
        /// Public constructor.
        /// For Beta2 we wont support delay loading Mapping information and we would also support
        /// only one mapping file for workspace.
        /// </summary>
        /// <param name="edmCollection"></param>
        /// <param name="storeItemCollection"></param>
        /// <param name="fileName"></param>
        /// <param name="scalarMemberMappings">Dictionary to keep the list of all scalar member mappings</param>
        internal StorageMappingItemLoader(XmlReader reader, StorageMappingItemCollection storageMappingItemCollection, string fileName, Dictionary<EdmMember, KeyValuePair<TypeUsage, TypeUsage>> scalarMemberMappings)
        {
            Debug.Assert(storageMappingItemCollection != null);
            Debug.Assert(scalarMemberMappings != null);

            this.m_storageMappingItemCollection = storageMappingItemCollection;
            this.m_alias = new Dictionary<string, string>(StringComparer.Ordinal);
            //The fileName field in this class will always have absolute path since
            //StorageMappingItemCollection would have already done it while
            //preparing the filePaths
            if (fileName != null)
            {
                this.m_sourceLocation = fileName;
            }
            else
            {
                this.m_sourceLocation = null;
            }
            m_parsingErrors = new List<EdmSchemaError>();
            this.m_scalarMemberMappings = scalarMemberMappings;
            m_containerMapping = LoadMappingItems(reader);
            if (m_currentNamespaceUri != null)
            {
                if (m_currentNamespaceUri == StorageMslConstructs.NamespaceUriV1)
                {
                    m_version = StorageMslConstructs.MappingVersionV1;
                }
                else if (m_currentNamespaceUri == StorageMslConstructs.NamespaceUriV2)
                {
                    m_version = StorageMslConstructs.MappingVersionV2;
                }
                else
                {
                    Debug.Assert(m_currentNamespaceUri == StorageMslConstructs.NamespaceUriV3, "Did you add a new Namespace?");
                    m_version = StorageMslConstructs.MappingVersionV3;
                }
            }
        }
        #endregion

        #region Fields
        private Dictionary<string, string> m_alias;  //To support the aliasing mechanism provided by MSL.
        private StorageMappingItemCollection m_storageMappingItemCollection; //StorageMappingItemCollection
        private string m_sourceLocation; //location identifier for the MSL file.
        private List<EdmSchemaError> m_parsingErrors;
        private Dictionary<EdmMember, KeyValuePair<TypeUsage, TypeUsage>> m_scalarMemberMappings; // dictionary of all the scalar member mappings - this is to validate that no property is mapped to different store types across mappings.
        private bool m_hasQueryViews;  //set to true if any of the SetMaps have a query view so that 
        private string m_currentNamespaceUri;
        private StorageEntityContainerMapping m_containerMapping;
        private double m_version;

        // cached xsd schema
        private static XmlSchemaSet s_mappingXmlSchema;
        #endregion

        #region Properties
        internal double MappingVersion
        {
            get { return m_version; }
        }

        internal IList<EdmSchemaError> ParsingErrors
        {
            get { return m_parsingErrors; }

        }

        internal bool HasQueryViews
        {
            get { return m_hasQueryViews; }
        }

        internal StorageEntityContainerMapping ContainerMapping
        {
            get { return m_containerMapping; }
        }

        private EdmItemCollection EdmItemCollection
        {
            get { return m_storageMappingItemCollection.EdmItemCollection; }
        }

        private StoreItemCollection StoreItemCollection
        {
            get { return m_storageMappingItemCollection.StoreItemCollection; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// The LoadMappingSchema method loads the mapping file and initializes the
        /// MappingSchema that represents this mapping file.
        /// For Beta2 atleast, we will support only one EntityContainerMapping per mapping file.
        /// </summary>
        /// <returns></returns>
        private StorageEntityContainerMapping LoadMappingItems(XmlReader innerReader)
        {
            // Using XPathDocument to load the xml file into memory.
            XmlReader reader = GetSchemaValidatingReader(innerReader);

            try
            {
                XPathDocument doc = new XPathDocument(reader);
                // If there were any xsd validation errors, we would have caught these while creatring xpath document.
                if (m_parsingErrors.Count != 0)
                {
                    // If the errors were only warnings continue, otherwise return the errors without loading the mapping.
                    if (!MetadataHelper.CheckIfAllErrorsAreWarnings(m_parsingErrors))
                    {
                        return null;
                    }
                }

                // Create an XPathNavigator to navigate the document in a forward only manner.
                // The XPathNavigator can also be used to run quries through the document while still maintaining
                // the current position. This will be helpful in running validation rules that are not part of Schema.
                XPathNavigator nav = doc.CreateNavigator();
                return LoadMappingItems(nav.Clone());
            }
            catch (XmlException xmlException)
            {
                // There must have been a xml parsing exception. Add the exception information to the error list.
                EdmSchemaError error = new EdmSchemaError(Strings.Mapping_InvalidMappingSchema_Parsing(xmlException.Message)
                    , (int)StorageMappingErrorCode.XmlSchemaParsingError, EdmSchemaErrorSeverity.Error, m_sourceLocation, xmlException.LineNumber, xmlException.LinePosition);
                m_parsingErrors.Add(error);
            }

            // Do not close the wrapping reader here, as doing so will close the inner reader. See SQLBUDT 522950 for details.

            return null;
        }

        private StorageEntityContainerMapping LoadMappingItems(XPathNavigator nav)
        {
            // XSD validation is not validating missing Root element.
            if (!MoveToRootElement(nav) || (nav.NodeType != XPathNodeType.Element))
            {
                StorageMappingItemLoader.AddToSchemaErrors(
                    Strings.Mapping_Invalid_CSRootElementMissing(
                        StorageMslConstructs.NamespaceUriV1,
                        StorageMslConstructs.NamespaceUriV2,
                        StorageMslConstructs.NamespaceUriV3), 
                    StorageMappingErrorCode.RootMappingElementMissing, 
                    m_sourceLocation,
                    (IXmlLineInfo)nav, m_parsingErrors);
                // There is no point in going forward if the required root element is not found.
                return null;
            }
            StorageEntityContainerMapping entityContainerMap = LoadMappingChildNodes(nav.Clone());
            // If there were any parsing errors, invalidate the entity container map and return null.
            if (m_parsingErrors.Count != 0)
            {
                // If all the schema errors are warnings, don't return null.
                if (!MetadataHelper.CheckIfAllErrorsAreWarnings(m_parsingErrors))
                {
                    entityContainerMap = null;
                }
            }
            return entityContainerMap;
        }

        private bool MoveToRootElement(XPathNavigator nav)
        {
            if (nav.MoveToChild(StorageMslConstructs.MappingElement, StorageMslConstructs.NamespaceUriV3))
            {
                // found v3 schema
                m_currentNamespaceUri = StorageMslConstructs.NamespaceUriV3;
                return true;
            }
            else if (nav.MoveToChild(StorageMslConstructs.MappingElement, StorageMslConstructs.NamespaceUriV2))
            {
                // found v2 schema
                m_currentNamespaceUri = StorageMslConstructs.NamespaceUriV2;
                return true;
            }
            else if (nav.MoveToChild(StorageMslConstructs.MappingElement, StorageMslConstructs.NamespaceUriV1))
            {
                m_currentNamespaceUri = StorageMslConstructs.NamespaceUriV1;
                return true;
            }
            //the xml namespace corresponds to neither v1 namespace nor v2 namespace
            return false;
        }

        /// <summary>
        /// The method loads the child nodes for the root Mapping node
        /// into the internal datastructures.
        /// </summary>
        private StorageEntityContainerMapping LoadMappingChildNodes(XPathNavigator nav)
        {
            bool hasContainerMapping;
            // If there are any Alias elements in the document, they should be the first ones.
            // This method can only move to the Alias element since comments, PIS etc wont have any Namespace
            // though they could have same name as Alias element.
            if (nav.MoveToChild(StorageMslConstructs.AliasElement, m_currentNamespaceUri))
            {
                // Collect all the alias elements.
                do
                {
                    m_alias.Add(StorageMappingItemLoader.GetAttributeValue(nav.Clone(), StorageMslConstructs.AliasKeyAttribute), StorageMappingItemLoader.GetAttributeValue(nav.Clone(), StorageMslConstructs.AliasValueAttribute));
                } while (nav.MoveToNext(StorageMslConstructs.AliasElement, m_currentNamespaceUri));
                // Now move on to the Next element that will be "EntityContainer" element.
                hasContainerMapping = nav.MoveToNext(XPathNodeType.Element);
            }
            else
            {
                // Since there was no Alias element, move on to the Container element.
                hasContainerMapping = nav.MoveToChild(XPathNodeType.Element);
            }

            // Load entity container mapping if any.
            var containerMapping = hasContainerMapping ? LoadEntityContainerMapping(nav.Clone()) : null;
            return containerMapping;
        }

        /// <summary>
        /// The method loads and returns the EntityContainer Mapping node.
        /// </summary>
        private StorageEntityContainerMapping LoadEntityContainerMapping(XPathNavigator nav)
        {
            IXmlLineInfo navLineInfo = (IXmlLineInfo)nav;

            // The element name can only be EntityContainerMapping element name since XSD validation should have guarneteed this.
            Debug.Assert(nav.LocalName == StorageMslConstructs.EntityContainerMappingElement);
            string entityContainerName = GetAttributeValue(nav.Clone(), StorageMslConstructs.CdmEntityContainerAttribute);
            string storageEntityContainerName = GetAttributeValue(nav.Clone(), StorageMslConstructs.StorageEntityContainerAttribute);

            bool generateUpdateViews = GetBoolAttributeValue(nav.Clone(), StorageMslConstructs.GenerateUpdateViews, true /* default is true */);

            StorageEntityContainerMapping entityContainerMapping;
            EntityContainer entityContainerType;
            EntityContainer storageEntityContainerType;

            // Now that we support partial mapping, we should first check if the entity container mapping is
            // already present. If its already present, we should add the new child nodes to the existing entity container mapping
            if (m_storageMappingItemCollection.TryGetItem<StorageEntityContainerMapping>(
                    entityContainerName, out entityContainerMapping))
            {
                entityContainerType = entityContainerMapping.EdmEntityContainer;
                storageEntityContainerType = entityContainerMapping.StorageEntityContainer;

                // The only thing we need to make sure is that the storage entity container mapping is the same.
                if (storageEntityContainerName != storageEntityContainerType.Name)
                {
                    AddToSchemaErrors(Strings.StorageEntityContainerNameMismatchWhileSpecifyingPartialMapping(
                            storageEntityContainerName, storageEntityContainerType.Name, entityContainerType.Name),
                        StorageMappingErrorCode.StorageEntityContainerNameMismatchWhileSpecifyingPartialMapping,
                        m_sourceLocation, navLineInfo, m_parsingErrors);

                    return null;
                }
            }
            else
            {
                // At this point we know that the EdmEntityContainer has not been mapped already.
                // If we do find that StorageEntityContainer has already been mapped, return null.
                if (m_storageMappingItemCollection.ContainsStorageEntityContainer(storageEntityContainerName))
                {
                    AddToSchemaErrorsWithMemberInfo(Strings.Mapping_AlreadyMapped_StorageEntityContainer, storageEntityContainerName,
                        StorageMappingErrorCode.AlreadyMappedStorageEntityContainer, m_sourceLocation, navLineInfo, m_parsingErrors);
                    return null;
                }

                // Get the CDM EntityContainer by this name from the metadata workspace.
                this.EdmItemCollection.TryGetEntityContainer(entityContainerName, out entityContainerType);
                if (entityContainerType == null)
                {
                    AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_EntityContainer,
                        entityContainerName, StorageMappingErrorCode.InvalidEntityContainer, m_sourceLocation,
                        navLineInfo, m_parsingErrors);
                }

                this.StoreItemCollection.TryGetEntityContainer(storageEntityContainerName, out storageEntityContainerType);
                if (storageEntityContainerType == null)
                {
                    AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_StorageEntityContainer, storageEntityContainerName,
                        StorageMappingErrorCode.InvalidEntityContainer, m_sourceLocation, navLineInfo, m_parsingErrors);
                }

                // If the EntityContainerTypes are not found, there is no point in continuing with the parsing.
                if ((entityContainerType == null) || (storageEntityContainerType == null))
                {
                    return null;
                }

                // Create an EntityContainerMapping object to hold the mapping information for this EntityContainer.
                // Create a MappingKey and pass it in.
                entityContainerMapping = new StorageEntityContainerMapping(entityContainerType, storageEntityContainerType,
                    m_storageMappingItemCollection, generateUpdateViews /* make validate same as generateUpdateView*/, generateUpdateViews);
                entityContainerMapping.StartLineNumber = navLineInfo.LineNumber;
                entityContainerMapping.StartLinePosition = navLineInfo.LinePosition;
            }

            // Load the child nodes for the created EntityContainerMapping.
            LoadEntityContainerMappingChildNodes(nav.Clone(), entityContainerMapping, storageEntityContainerType);
            return entityContainerMapping;
        }
        
        /// <summary>
        /// The method loads the child nodes for the EntityContainer Mapping node
        /// into the internal datastructures.
        /// </summary>
        private void LoadEntityContainerMappingChildNodes(XPathNavigator nav, StorageEntityContainerMapping entityContainerMapping, EntityContainer storageEntityContainerType)
        {
            IXmlLineInfo xmlLineInfoNav = (IXmlLineInfo)nav;
            bool anyEntitySetMapped = false;

            //If there is no child node for the EntityContainerMapping Element, return.
            if (nav.MoveToChild(XPathNodeType.Element))
            {
                //The valid child nodes for EntityContainerMapping node are various SetMappings( EntitySet, AssociationSet etc ).
                //Loop through the child nodes and lod them as children of the EntityContainerMapping object.
                do
                {
                    switch (nav.LocalName)
                    {
                        case StorageMslConstructs.EntitySetMappingElement:
                            {
                                LoadEntitySetMapping(nav.Clone(), entityContainerMapping, storageEntityContainerType);
                                anyEntitySetMapped = true;
                                break;
                            }
                        case StorageMslConstructs.AssociationSetMappingElement:
                            {
                                LoadAssociationSetMapping(nav.Clone(), entityContainerMapping, storageEntityContainerType);
                                break;
                            }
                        case StorageMslConstructs.FunctionImportMappingElement:
                            {
                                LoadFunctionImportMapping(nav.Clone(), entityContainerMapping, storageEntityContainerType);
                                break;
                            }
                        default:
                            AddToSchemaErrors(Strings.Mapping_InvalidContent_Container_SubElement,
                                StorageMappingErrorCode.SetMappingExpected, m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                            break;
                    }
                } while (nav.MoveToNext(XPathNodeType.Element));
            }

            //If the EntityContainer contains entity sets but they are not mapped then we should add an error
            if (entityContainerMapping.EdmEntityContainer.BaseEntitySets.Count != 0 && !anyEntitySetMapped)
            {
                AddToSchemaErrorsWithMemberInfo(Strings.ViewGen_Missing_Sets_Mapping,
                    entityContainerMapping.EdmEntityContainer.Name, StorageMappingErrorCode.EmptyContainerMapping,
                    this.m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                return;
            }

            ValidateFunctionAssociationFunctionMappingUnique(nav.Clone(), entityContainerMapping);
            ValidateModificationFunctionMappingConsistentForAssociations(nav.Clone(), entityContainerMapping);
            ValidateQueryViewsClosure(nav.Clone(), entityContainerMapping);
            ValidateEntitySetFunctionMappingClosure(nav.Clone(), entityContainerMapping);
            // The fileName field in this class will always have absolute path since StorageMappingItemCollection would have already done it while
            // preparing the filePaths.
            entityContainerMapping.SourceLocation = m_sourceLocation;
        }

        /// <summary>
        /// Validates that collocated association sets are consistently mapped for each entity set (all operations or none). In the case
        /// of relationships between sub-types of an entity set, ensures the relationship mapping is legal.
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="entityContainerMapping"></param>
        private void ValidateModificationFunctionMappingConsistentForAssociations(XPathNavigator nav, StorageEntityContainerMapping entityContainerMapping)
        {
            foreach (StorageEntitySetMapping entitySetMapping in entityContainerMapping.EntitySetMaps)
            {
                if (entitySetMapping.ModificationFunctionMappings.Count > 0)
                {
                    // determine the set of association sets that should be mapped for every operation
                    Set<AssociationSetEnd> expectedEnds = new Set<AssociationSetEnd>(
                        entitySetMapping.ImplicitlyMappedAssociationSetEnds).MakeReadOnly();

                    // check that each operation covers each association set
                    foreach (StorageEntityTypeModificationFunctionMapping entityTypeMapping in entitySetMapping.ModificationFunctionMappings)
                    {
                        if (null != entityTypeMapping.DeleteFunctionMapping)
                        {
                            ValidateModificationFunctionMappingConsistentForAssociations(nav, entitySetMapping, entityTypeMapping,
                                entityTypeMapping.DeleteFunctionMapping,
                                expectedEnds, StorageMslConstructs.DeleteFunctionElement);
                        }
                        if (null != entityTypeMapping.InsertFunctionMapping)
                        {
                            ValidateModificationFunctionMappingConsistentForAssociations(nav, entitySetMapping, entityTypeMapping,
                                entityTypeMapping.InsertFunctionMapping,
                                expectedEnds, StorageMslConstructs.InsertFunctionElement);
                        }
                        if (null != entityTypeMapping.UpdateFunctionMapping)
                        {
                            ValidateModificationFunctionMappingConsistentForAssociations(nav, entitySetMapping, entityTypeMapping,
                                entityTypeMapping.UpdateFunctionMapping,
                                expectedEnds, StorageMslConstructs.UpdateFunctionElement);
                        }
                    }
                }
            }
        }
        private void ValidateModificationFunctionMappingConsistentForAssociations(
            XPathNavigator nav,
            StorageEntitySetMapping entitySetMapping,
            StorageEntityTypeModificationFunctionMapping entityTypeMapping,
            StorageModificationFunctionMapping functionMapping,
            Set<AssociationSetEnd> expectedEnds, string elementName)
        {
            IXmlLineInfo xmlLineInfoNav = (IXmlLineInfo)nav;

            // check that all expected association sets are mapped for in this function mapping
            Set<AssociationSetEnd> actualEnds = new Set<AssociationSetEnd>(functionMapping.CollocatedAssociationSetEnds);
            actualEnds.MakeReadOnly();

            // check that all required ends are present
            foreach (AssociationSetEnd expectedEnd in expectedEnds)
            {
                // check that the association set is required based on the entity type
                if (MetadataHelper.IsAssociationValidForEntityType(expectedEnd, entityTypeMapping.EntityType))
                {
                    if (!actualEnds.Contains(expectedEnd))
                    {
                        AddToSchemaErrorWithMessage(Strings.Mapping_ModificationFunction_AssociationSetNotMappedForOperation(
                            entitySetMapping.Set.Name,
                            expectedEnd.ParentAssociationSet.Name,
                            elementName,
                            entityTypeMapping.EntityType.FullName),
                            StorageMappingErrorCode.InvalidModificationFunctionMappingAssociationSetNotMappedForOperation,
                            m_sourceLocation,
                            xmlLineInfoNav,
                            m_parsingErrors);
                    }
                }
            }

            // check that no ends with invalid types are included
            foreach (AssociationSetEnd actualEnd in actualEnds)
            {
                if (!MetadataHelper.IsAssociationValidForEntityType(actualEnd, entityTypeMapping.EntityType))
                {
                    AddToSchemaErrorWithMessage(Strings.Mapping_ModificationFunction_AssociationEndMappingInvalidForEntityType(
                        entityTypeMapping.EntityType.FullName,
                        actualEnd.ParentAssociationSet.Name,
                        MetadataHelper.GetEntityTypeForEnd(MetadataHelper.GetOppositeEnd(actualEnd).CorrespondingAssociationEndMember).FullName),
                        StorageMappingErrorCode.InvalidModificationFunctionMappingAssociationEndMappingInvalidForEntityType,
                        m_sourceLocation,
                        xmlLineInfoNav,
                        m_parsingErrors);
                }
            }
        }

        /// <summary>
        /// Validates that association sets are only mapped once.
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="entityContainerMapping">Container to validate</param>
        private void ValidateFunctionAssociationFunctionMappingUnique(XPathNavigator nav, StorageEntityContainerMapping entityContainerMapping)
        {
            Dictionary<EntitySetBase, int> mappingCounts = new Dictionary<EntitySetBase, int>();

            // Walk through all entity set mappings
            foreach (StorageEntitySetMapping entitySetMapping in entityContainerMapping.EntitySetMaps)
            {
                if (entitySetMapping.ModificationFunctionMappings.Count > 0)
                {
                    // Get set of association sets implicitly mapped associations to avoid double counting
                    Set<EntitySetBase> associationSets = new Set<EntitySetBase>();
                    foreach (AssociationSetEnd end in entitySetMapping.ImplicitlyMappedAssociationSetEnds)
                    {
                        associationSets.Add(end.ParentAssociationSet);
                    }

                    foreach (EntitySetBase associationSet in associationSets)
                    {
                        IncrementCount(mappingCounts, associationSet);
                    }
                }
            }

            // Walk through all association set mappings
            foreach (StorageAssociationSetMapping associationSetMapping in entityContainerMapping.RelationshipSetMaps)
            {
                if (null != associationSetMapping.ModificationFunctionMapping)
                {
                    IncrementCount(mappingCounts, associationSetMapping.Set);
                }
            }

            // Check for redundantly mapped association sets
            List<string> violationNames = new List<string>();
            foreach (KeyValuePair<EntitySetBase, int> mappingCount in mappingCounts)
            {
                if (mappingCount.Value > 1)
                {
                    violationNames.Add(mappingCount.Key.Name);
                }
            }

            if (0 < violationNames.Count)
            {
                // Warn the user that association sets are mapped multiple times                
                AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_AssociationSetAmbiguous,
                    StringUtil.ToCommaSeparatedString(violationNames), StorageMappingErrorCode.AmbiguousModificationFunctionMappingForAssociationSet,
                    m_sourceLocation, (IXmlLineInfo)nav, m_parsingErrors);

            }
        }

        private static void IncrementCount<T>(Dictionary<T, int> counts, T key)
        {
            int count;
            if (counts.TryGetValue(key, out count))
            {
                count++;
            }
            else
            {
                count = 1;
            }
            counts[key] = count;
        }

        /// <summary>
        /// Validates that all or no related extents have function mappings. If an EntitySet or an AssociationSet has a function mapping,
        /// then all the sets that touched the same store tableSet must also have function mappings.
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="entityContainerMapping">Container to validate.</param>
        private void ValidateEntitySetFunctionMappingClosure(XPathNavigator nav, StorageEntityContainerMapping entityContainerMapping)
        {
            // here we build a mapping between the tables and the sets,
            // setmapping => typemapping => mappingfragments, foreach mappingfragments we have one Tableset,
            // then add the tableset with setmapping to the dictionary

            KeyToListMap<EntitySet, StorageSetMapping> setMappingPerTable =
                new KeyToListMap<EntitySet, StorageSetMapping>(EqualityComparer<EntitySet>.Default);

            // Walk through all set mappings
            foreach (var setMapping in entityContainerMapping.AllSetMaps)
            {
                foreach (var typeMapping in setMapping.TypeMappings)
                {
                    foreach (var fragment in typeMapping.MappingFragments)
                    {
                        setMappingPerTable.Add(fragment.TableSet, setMapping);
                    }
                }
            }

            // Get set of association sets implicitly mapped associations to avoid double counting
            Set<EntitySetBase> implicitMappedAssociationSets = new Set<EntitySetBase>();

            // Walk through all entity set mappings
            foreach (StorageEntitySetMapping entitySetMapping in entityContainerMapping.EntitySetMaps)
            {
                if (entitySetMapping.ModificationFunctionMappings.Count > 0)
                {
                    foreach (AssociationSetEnd end in entitySetMapping.ImplicitlyMappedAssociationSetEnds)
                    {
                        implicitMappedAssociationSets.Add(end.ParentAssociationSet);
                    }
                }
            }

            foreach (var table in setMappingPerTable.Keys)
            {
                // if any of the sets who touches the same table has modification function, 
                // then all the sets that touches the same table should have modification function
                if (setMappingPerTable.ListForKey(table).Any(s => s.HasModificationFunctionMapping || implicitMappedAssociationSets.Any(aset=> aset == s.Set)) &&
                    setMappingPerTable.ListForKey(table).Any(s => !s.HasModificationFunctionMapping && !implicitMappedAssociationSets.Any(aset => aset == s.Set)))
                {
                    AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_MissingSetClosure,
                        StringUtil.ToCommaSeparatedString(setMappingPerTable.ListForKey(table)
                            .Where(s => !s.HasModificationFunctionMapping).Select(s=>s.Set.Name)),
                        StorageMappingErrorCode.MissingSetClosureInModificationFunctionMapping, m_sourceLocation, (IXmlLineInfo)nav
                        , m_parsingErrors);
                }
            }
        }

        private static void ValidateClosureAmongSets(StorageEntityContainerMapping entityContainerMapping, Set<EntitySetBase> sets, Set<EntitySetBase> additionalSetsInClosure)
        {
            bool nodeFound;
            do
            {
                nodeFound = false;
                List<EntitySetBase> newNodes = new List<EntitySetBase>();

                // Register entity sets dependencies for association sets
                foreach (EntitySetBase entitySetBase in additionalSetsInClosure)
                {
                    AssociationSet associationSet = entitySetBase as AssociationSet;
                    //Foreign Key Associations do not add to the dependancies
                    if (associationSet != null
                        && !associationSet.ElementType.IsForeignKey)
                    {
                        // add the entity sets bound to the end roles to the required list
                        foreach (AssociationSetEnd end in associationSet.AssociationSetEnds)
                        {
                            if (!additionalSetsInClosure.Contains(end.EntitySet))
                            {
                                newNodes.Add(end.EntitySet);
                            }
                        }
                    }
                }

                // Register all association sets referencing known entity sets
                foreach (EntitySetBase entitySetBase in entityContainerMapping.EdmEntityContainer.BaseEntitySets)
                {
                    AssociationSet associationSet = entitySetBase as AssociationSet;
                    //Foreign Key Associations do not add to the dependancies
                    if (associationSet != null
                        && !associationSet.ElementType.IsForeignKey)
                    {
                        // check that this association set isn't already in the required set
                        if (!additionalSetsInClosure.Contains(associationSet))
                        {
                            foreach (AssociationSetEnd end in associationSet.AssociationSetEnds)
                            {
                                if (additionalSetsInClosure.Contains(end.EntitySet))
                                {
                                    // this association set must be added to the required list if
                                    // any of its ends are in that list
                                    newNodes.Add(associationSet);
                                    break; // no point adding the association set twice
                                }
                            }
                        }
                    }
                }

                if (0 < newNodes.Count)
                {
                    nodeFound = true;
                    additionalSetsInClosure.AddRange(newNodes);
                }
            }
            while (nodeFound);

            additionalSetsInClosure.Subtract(sets);
        }

        /// <summary>
        /// Validates that all or no related extents have query views defined. If an extent has a query view defined, then
        /// all related extents must also have query views.
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="entityContainerMapping">Container to validate.</param>
        private void ValidateQueryViewsClosure(XPathNavigator nav, StorageEntityContainerMapping entityContainerMapping)
        {
            //If there is no query view defined, no need to validate
            if (!m_hasQueryViews)
            {
                return;
            }
            // Check that query views apply to complete subgraph by tracking which extents have query
            // mappings and which extents must include query views
            Set<EntitySetBase> setsWithQueryViews = new Set<EntitySetBase>();
            Set<EntitySetBase> setsRequiringQueryViews = new Set<EntitySetBase>();

            // Walk through all set mappings
            foreach (StorageSetMapping setMapping in entityContainerMapping.AllSetMaps)
            {
                if (setMapping.QueryView != null)
                {
                    // a function mapping exists for this entity set
                    setsWithQueryViews.Add(setMapping.Set);
                }
            }

            // Initialize sets requiring function mapping with the sets that are actually function mapped
            setsRequiringQueryViews.AddRange(setsWithQueryViews);

            ValidateClosureAmongSets(entityContainerMapping, setsWithQueryViews, setsRequiringQueryViews);

            // Check that no required entity or association sets are missing
            if (0 < setsRequiringQueryViews.Count)
            {
                AddToSchemaErrorsWithMemberInfo(Strings.Mapping_Invalid_Query_Views_MissingSetClosure,
                    StringUtil.ToCommaSeparatedString(setsRequiringQueryViews),
                    StorageMappingErrorCode.MissingSetClosureInQueryViews, m_sourceLocation, (IXmlLineInfo)nav
                    , m_parsingErrors);
            }
        }

        /// <summary>
        /// The method loads the child nodes for the EntitySet Mapping node
        /// into the internal datastructures.
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="entityContainerMapping"></param>
        /// <param name="storageEntityContainerType"></param>
        private void LoadEntitySetMapping(XPathNavigator nav, StorageEntityContainerMapping entityContainerMapping, EntityContainer storageEntityContainerType)
        {
            //Get the EntitySet name 
            string entitySetName = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.EntitySetMappingNameAttribute);
            //Get the EntityType name, need to parse it if the mapping information is being specified for multiple types 
            string entityTypeName = StorageMappingItemLoader.GetAttributeValue(nav.Clone(), StorageMslConstructs.EntitySetMappingTypeNameAttribute);
            //Get the table name. This might be emptystring since the user can have a TableMappingFragment instead of this.
            string tableName = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.EntitySetMappingStoreEntitySetAttribute);
            
            bool distinctFlag = GetBoolAttributeValue(nav.Clone(), StorageMslConstructs.MappingFragmentMakeColumnsDistinctAttribute, false /*default value*/);
            
            EntitySet entitySet;

            // First check to see if the Entity Set Mapping is already specified. It can be specified, in the same schema file later on
            // on a totally different file. Since we support partial mapping, we should just add mapping fragments or entity type
            // mappings to the existing entity set mapping
            StorageEntitySetMapping setMapping = (StorageEntitySetMapping)entityContainerMapping.GetEntitySetMapping(entitySetName);

            // Update the info about the schema element
            IXmlLineInfo navLineInfo = (IXmlLineInfo)nav;

            if (setMapping == null)
            {
                //Try to find the EntitySet with the given name in the EntityContainer.
                if (!entityContainerMapping.EdmEntityContainer.TryGetEntitySetByName(entitySetName, /*ignoreCase*/ false, out entitySet))
                {
                    //If no EntitySet with the given name exists, than add a schema error and return
                    AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Entity_Set, entitySetName,
                        StorageMappingErrorCode.InvalidEntitySet, m_sourceLocation, navLineInfo, m_parsingErrors);
                    //There is no point in continuing the loding of this EntitySetMapping if the EntitySet is not found
                    return;
                }
                //Create the EntitySet Mapping which contains the mapping information for EntitySetMap.
                setMapping = new StorageEntitySetMapping(entitySet, entityContainerMapping);
            }
            else
            {
                entitySet = (EntitySet)setMapping.Set;
            }

            //Set the Start Line Information on Fragment
            setMapping.StartLineNumber = navLineInfo.LineNumber;
            setMapping.StartLinePosition = navLineInfo.LinePosition;
            entityContainerMapping.AddEntitySetMapping(setMapping);

            //If the TypeName was not specified as an attribute, than an EntityTypeMapping element should be present 
            if (String.IsNullOrEmpty(entityTypeName))
            {
                if (nav.MoveToChild(XPathNodeType.Element))
                {

                    do
                    {
                        switch (nav.LocalName)
                        {
                            case StorageMslConstructs.EntityTypeMappingElement:
                                {
                                    //TableName could also be specified on EntityTypeMapping element
                                    tableName = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.EntityTypeMappingStoreEntitySetAttribute);
                                    //Load the EntityTypeMapping into memory.
                                    LoadEntityTypeMapping(nav.Clone(), setMapping, tableName, storageEntityContainerType, false /*No distinct flag so far*/, entityContainerMapping.GenerateUpdateViews);
                                    break;
                                }
                            case StorageMslConstructs.QueryViewElement:
                                {
                                    if (!(String.IsNullOrEmpty(tableName)))
                                    {
                                        AddToSchemaErrorsWithMemberInfo(Strings.Mapping_TableName_QueryView, entitySetName,
                                            StorageMappingErrorCode.TableNameAttributeWithQueryView, m_sourceLocation, navLineInfo, m_parsingErrors);
                                        return;
                                    }
                                    //Load the Query View into the set mapping,
                                    //if you get an error, return immediately since 
                                    //you go on, you could be giving lot of dubious errors
                                    if(!LoadQueryView(nav.Clone(), setMapping))
                                    {
                                        return;
                                    }
                                    break;
                                }
                            default:
                                AddToSchemaErrors(Strings.Mapping_InvalidContent_TypeMapping_QueryView,
                                    StorageMappingErrorCode.InvalidContent, m_sourceLocation, navLineInfo, m_parsingErrors);
                                break;
                        }
                    } while (nav.MoveToNext(XPathNodeType.Element));
                }
            }
            else
            {
                //Load the EntityTypeMapping into memory.
                LoadEntityTypeMapping(nav.Clone(), setMapping, tableName, storageEntityContainerType, distinctFlag, entityContainerMapping.GenerateUpdateViews);
            }
            ValidateAllEntityTypesHaveFunctionMapping(nav.Clone(), setMapping);
            //Add a schema error if the set mapping has no content
            if (setMapping.HasNoContent)
            {
                AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Emtpty_SetMap, entitySet.Name,
                    StorageMappingErrorCode.EmptySetMapping, m_sourceLocation, navLineInfo, m_parsingErrors);
            }
        }

        // Ensure if any type has a function mapping, all types have function mappings
        private void ValidateAllEntityTypesHaveFunctionMapping(XPathNavigator nav, StorageEntitySetMapping setMapping)
        {
            Set<EdmType> functionMappedTypes = new Set<EdmType>();
            foreach (StorageEntityTypeModificationFunctionMapping modificationFunctionMapping in setMapping.ModificationFunctionMappings)
            {
                functionMappedTypes.Add(modificationFunctionMapping.EntityType);
            }
            if (0 < functionMappedTypes.Count)
            {
                Set<EdmType> unmappedTypes = new Set<EdmType>(MetadataHelper.GetTypeAndSubtypesOf(setMapping.Set.ElementType, EdmItemCollection, false /*includeAbstractTypes*/));
                unmappedTypes.Subtract(functionMappedTypes);

                // Remove abstract types
                Set<EdmType> abstractTypes = new Set<EdmType>();
                foreach (EntityType unmappedType in unmappedTypes)
                {
                    if (unmappedType.Abstract)
                    {
                        abstractTypes.Add(unmappedType);
                    }
                }
                unmappedTypes.Subtract(abstractTypes);

                // See if there are any remaining entity types requiring function mapping
                if (0 < unmappedTypes.Count)
                {
                    AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_MissingEntityType,
                        StringUtil.ToCommaSeparatedString(unmappedTypes),
                        StorageMappingErrorCode.MissingModificationFunctionMappingForEntityType, m_sourceLocation, (IXmlLineInfo)nav
                        , m_parsingErrors);
                }
            }
        }

        private bool TryParseEntityTypeAttribute(
            XPathNavigator nav,
            EntityType rootEntityType,
            Func<EntityType, string> typeNotAssignableMessage,
            out Set<EntityType> isOfTypeEntityTypes,
            out Set<EntityType> entityTypes)
        {
            IXmlLineInfo xmlLineInfoNav = (IXmlLineInfo)nav;
            string entityTypeAttribute = GetAttributeValue(nav.Clone(), StorageMslConstructs.EntitySetMappingTypeNameAttribute);

            isOfTypeEntityTypes = new Set<EntityType>();
            entityTypes = new Set<EntityType>();

            // get components of type declaration
            var entityTypeNames = entityTypeAttribute.Split(StorageMslConstructs.TypeNameSperator).Select(s => s.Trim());

            // figure out each component
            foreach (var name in entityTypeNames)
            {
                bool isTypeOf = name.StartsWith(StorageMslConstructs.IsTypeOf, StringComparison.Ordinal);
                string entityTypeName;
                if (isTypeOf)
                {
                    // get entityTypeName of OfType(entityTypeName)
                    if (!name.EndsWith(StorageMslConstructs.IsTypeOfTerminal, StringComparison.Ordinal))
                    {
                        AddToSchemaErrorWithMessage(Strings.Mapping_InvalidContent_IsTypeOfNotTerminated,
                            StorageMappingErrorCode.InvalidEntityType, m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                        // No point in continuing with an error in the entitytype name
                        return false;
                    }
                    entityTypeName = name.Substring(StorageMslConstructs.IsTypeOf.Length);
                    entityTypeName = entityTypeName.Substring(0, entityTypeName.Length - StorageMslConstructs.IsTypeOfTerminal.Length).Trim();
                }
                else
                {
                    entityTypeName = name;
                }

                // resolve aliases
                entityTypeName = GetAliasResolvedValue(entityTypeName);

                EntityType entityType;
                if (!this.EdmItemCollection.TryGetItem<EntityType>(entityTypeName, out entityType))
                {
                    AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Entity_Type, entityTypeName,
                        StorageMappingErrorCode.InvalidEntityType, m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                    // No point in continuing with an error in the entitytype name
                    return false;
                }
                if (!(Helper.IsAssignableFrom(rootEntityType, entityType)))
                {
                    IXmlLineInfo lineInfo = xmlLineInfoNav;
                    AddToSchemaErrorWithMessage(
                        typeNotAssignableMessage(entityType),
                        StorageMappingErrorCode.InvalidEntityType, m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                    //no point in continuing with an error in the entitytype name
                    return false;
                }

                // Using TypeOf construct on an abstract type that does not have
                // any concrete descendants is not allowed
                if (entityType.Abstract)
                {
                    if (isTypeOf)
                    {
                        IEnumerable<EdmType> typeAndSubTypes = MetadataHelper.GetTypeAndSubtypesOf(entityType, EdmItemCollection, false /*includeAbstractTypes*/);
                        if (!typeAndSubTypes.GetEnumerator().MoveNext())
                        {
                            AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_AbstractEntity_IsOfType, entityType.FullName,
                                StorageMappingErrorCode.MappingOfAbstractType, m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                            return false;
                        }
                    }
                    else
                    {
                        AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_AbstractEntity_Type, entityType.FullName,
                            StorageMappingErrorCode.MappingOfAbstractType, m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                        return false;
                    }
                }

                // Add type to set
                if (isTypeOf)
                {
                    isOfTypeEntityTypes.Add(entityType);
                }
                else
                {
                    entityTypes.Add(entityType);
                }
            }

            // No failures
            return true;
        }

        /// <summary>
        /// The method loads the child nodes for the EntityType Mapping node
        /// into the internal datastructures.
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="entitySetMapping"></param>
        /// <param name="tableName"></param>
        /// <param name="storageEntityContainerType"></param>
        private void LoadEntityTypeMapping(XPathNavigator nav, StorageEntitySetMapping entitySetMapping, string tableName, EntityContainer storageEntityContainerType, bool distinctFlagAboveType, bool generateUpdateViews)
        {
            IXmlLineInfo xmlLineInfoNav = (IXmlLineInfo)nav;

            //Create an EntityTypeMapping to hold the information for EntityType mapping.
            StorageEntityTypeMapping entityTypeMapping = new StorageEntityTypeMapping(entitySetMapping);

            //Get entity types
            Set<EntityType> entityTypes;
            Set<EntityType> isOfTypeEntityTypes;
            EntityType rootEntityType = (EntityType)entitySetMapping.Set.ElementType;
            if (!TryParseEntityTypeAttribute(nav.Clone(), rootEntityType,
                e => Strings.Mapping_InvalidContent_Entity_Type_For_Entity_Set(e.FullName, rootEntityType.FullName, entitySetMapping.Set.Name),
                out isOfTypeEntityTypes,
                out entityTypes))
            {
                // Return if we cannot parse entity types
                return;
            }

            // Register all mapped types
            foreach (EntityType entityType in entityTypes)
            {
                entityTypeMapping.AddType(entityType);
            }
            foreach (EntityType isOfTypeEntityType in isOfTypeEntityTypes)
            {
                entityTypeMapping.AddIsOfType(isOfTypeEntityType);
            }

            //If the table name was not specified on the EntitySetMapping element nor the EntityTypeMapping element
            //than a table mapping fragment element should be present
            //Loop through the TableMappingFragment elements and add them to EntityTypeMappings
            if (String.IsNullOrEmpty(tableName))
            {
                if (!nav.MoveToChild(XPathNodeType.Element))
                    return;
                do
                {
                    if (nav.LocalName == StorageMslConstructs.ModificationFunctionMappingElement)
                    {
                        entitySetMapping.HasModificationFunctionMapping = true;
                        LoadEntityTypeModificationFunctionMapping(nav.Clone(), entitySetMapping, entityTypeMapping);
                    }
                    else if (nav.LocalName != StorageMslConstructs.MappingFragmentElement)
                    {
                        AddToSchemaErrors(Strings.Mapping_InvalidContent_Table_Expected,
                            StorageMappingErrorCode.TableMappingFragmentExpected, m_sourceLocation, xmlLineInfoNav
                            , m_parsingErrors);
                    }
                    else
                    {
                        bool distinctFlag = GetBoolAttributeValue(nav.Clone(), StorageMslConstructs.MappingFragmentMakeColumnsDistinctAttribute, false /*default value*/);

                        if (generateUpdateViews && distinctFlag)
                        {
                            AddToSchemaErrors(Strings.Mapping_DistinctFlagInReadWriteContainer,
                                StorageMappingErrorCode.DistinctFragmentInReadWriteContainer, m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                        }
                        
                        tableName = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.MappingFragmentStoreEntitySetAttribute);
                        StorageMappingFragment fragment = LoadMappingFragment(nav.Clone(), entityTypeMapping, tableName, storageEntityContainerType, distinctFlag);
                        //The fragment can be null in the cases of validation errors.
                        if (fragment != null)
                        {
                            entityTypeMapping.AddFragment(fragment);
                        }
                    }
                } while (nav.MoveToNext(XPathNodeType.Element));
            }
            else
            {
                if (nav.LocalName == StorageMslConstructs.ModificationFunctionMappingElement)
                {
                    // function mappings cannot exist in the context of a table mapping
                    AddToSchemaErrors(Strings.Mapping_ModificationFunction_In_Table_Context,
                        StorageMappingErrorCode.InvalidTableNameAttributeWithModificationFunctionMapping,
                        m_sourceLocation, xmlLineInfoNav
                        , m_parsingErrors);
                }

                if (generateUpdateViews && distinctFlagAboveType)
                {
                    AddToSchemaErrors(Strings.Mapping_DistinctFlagInReadWriteContainer,
                        StorageMappingErrorCode.DistinctFragmentInReadWriteContainer, m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                }

                StorageMappingFragment fragment = LoadMappingFragment(nav.Clone(), entityTypeMapping, tableName,
                    storageEntityContainerType, distinctFlagAboveType);
                //The fragment can be null in the cases of validation errors.
                if (fragment != null)
                {
                    entityTypeMapping.AddFragment(fragment);
                }
            }
            entitySetMapping.AddTypeMapping(entityTypeMapping);
        }


        /// <summary>
        /// Loads modification function mappings for entity type.
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="entitySetMapping"></param>
        /// <param name="entityTypeMapping"></param>
        private void LoadEntityTypeModificationFunctionMapping(
            XPathNavigator nav,
            StorageEntitySetMapping entitySetMapping,
            StorageEntityTypeMapping entityTypeMapping)
        {
            IXmlLineInfo xmlLineInfoNav = (IXmlLineInfo)nav;

            // Function mappings can apply only to a single type.
            if (entityTypeMapping.IsOfTypes.Count != 0 || entityTypeMapping.Types.Count != 1)
            {
                AddToSchemaErrors(Strings.Mapping_ModificationFunction_Multiple_Types,
                    StorageMappingErrorCode.InvalidModificationFunctionMappingForMultipleTypes,
                    m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                return;
            }
            EntityType entityType = (EntityType)entityTypeMapping.Types[0];
            //Function Mapping is not allowed to be defined for Abstract Types
            if (entityType.Abstract)
            {
                AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_AbstractEntity_FunctionMapping, entityType.FullName,
                    StorageMappingErrorCode.MappingOfAbstractType, m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                return;
            }

            // check that no mapping exists for this entity type already
            foreach (StorageEntityTypeModificationFunctionMapping existingMapping in entitySetMapping.ModificationFunctionMappings)
            {
                if (existingMapping.EntityType.Equals(entityType))
                {
                    AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_RedundantEntityTypeMapping,
                        entityType.Name, StorageMappingErrorCode.RedundantEntityTypeMappingInModificationFunctionMapping, m_sourceLocation, xmlLineInfoNav
                        , m_parsingErrors);
                    return;
                }
            }

            // create function loader
            ModificationFunctionMappingLoader functionLoader = new ModificationFunctionMappingLoader(this, entitySetMapping.Set);

            // Load all function definitions (for insert, delete and update)
            StorageModificationFunctionMapping deleteFunctionMapping = null;
            StorageModificationFunctionMapping insertFunctionMapping = null;
            StorageModificationFunctionMapping updateFunctionMapping = null;
            if (nav.MoveToChild(XPathNodeType.Element))
            {
                do
                {
                    switch (nav.LocalName)
                    {
                        case StorageMslConstructs.DeleteFunctionElement:
                            deleteFunctionMapping = functionLoader.LoadEntityTypeModificationFunctionMapping(nav.Clone(), entitySetMapping.Set, false, true, entityType);
                            break;
                        case StorageMslConstructs.InsertFunctionElement:
                            insertFunctionMapping = functionLoader.LoadEntityTypeModificationFunctionMapping(nav.Clone(), entitySetMapping.Set, true, false, entityType);
                            break;
                        case StorageMslConstructs.UpdateFunctionElement:
                            updateFunctionMapping = functionLoader.LoadEntityTypeModificationFunctionMapping(nav.Clone(), entitySetMapping.Set, true, true, entityType);
                            break;
                    }
                } while (nav.MoveToNext(XPathNodeType.Element));
            }


            // Ensure that assocation set end mappings bind to the same end (e.g., in Person Manages Person
            // self-association, ensure that the manager end or the report end is mapped but not both)
            IEnumerable<StorageModificationFunctionParameterBinding> parameterList = new List<StorageModificationFunctionParameterBinding>();
            if (null != deleteFunctionMapping)
            {
                parameterList = Helper.Concat(parameterList, deleteFunctionMapping.ParameterBindings);
            }
            if (null != insertFunctionMapping)
            {
                parameterList = Helper.Concat(parameterList, insertFunctionMapping.ParameterBindings);
            }
            if (null != updateFunctionMapping)
            {
                parameterList = Helper.Concat(parameterList, updateFunctionMapping.ParameterBindings);
            }

            var associationEnds = new Dictionary<AssociationSet, AssociationEndMember>();
            foreach (StorageModificationFunctionParameterBinding parameterBinding in parameterList)
            {
                if (null != parameterBinding.MemberPath.AssociationSetEnd)
                {
                    AssociationSet associationSet = parameterBinding.MemberPath.AssociationSetEnd.ParentAssociationSet;
                    // the "end" corresponds to the second member in the path, e.g.
                    // ID<-Manager where Manager is the end
                    AssociationEndMember currentEnd = parameterBinding.MemberPath.AssociationSetEnd.CorrespondingAssociationEndMember;

                    AssociationEndMember existingEnd;
                    if (associationEnds.TryGetValue(associationSet, out existingEnd) &&
                        existingEnd != currentEnd)
                    {
                        AddToSchemaErrorWithMessage(Strings.Mapping_ModificationFunction_MultipleEndsOfAssociationMapped(
                            currentEnd.Name, existingEnd.Name, associationSet.Name),
                            StorageMappingErrorCode.InvalidModificationFunctionMappingMultipleEndsOfAssociationMapped, m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                        return;
                    }
                    else
                    {
                        associationEnds[associationSet] = currentEnd;
                    }
                }
            }

            // Register the function mapping on the entity set mapping
            StorageEntityTypeModificationFunctionMapping mapping = new StorageEntityTypeModificationFunctionMapping(
                entityType, deleteFunctionMapping, insertFunctionMapping, updateFunctionMapping);
            
            entitySetMapping.AddModificationFunctionMapping(mapping);
        }

        /// <summary>
        /// The method loads the query view for the Set Mapping node
        /// into the internal datastructures.
        /// </summary>
        private bool LoadQueryView(XPathNavigator nav, StorageSetMapping setMapping)
        {
            Debug.Assert(nav.LocalName == StorageMslConstructs.QueryViewElement);

            string queryView = nav.Value;
            bool includeSubtypes = false;

            string typeNameString = StorageMappingItemLoader.GetAttributeValue(nav.Clone(), StorageMslConstructs.EntitySetMappingTypeNameAttribute);
            if (typeNameString != null)
            {
                typeNameString = typeNameString.Trim();
            }

            if (setMapping.QueryView == null)
            {
                // QV must be the special-case first view.
                if (typeNameString != null)
                {
                    AddToSchemaErrorsWithMemberInfo(val => Strings.Mapping_TypeName_For_First_QueryView,
                        setMapping.Set.Name, StorageMappingErrorCode.TypeNameForFirstQueryView,
                        m_sourceLocation, (IXmlLineInfo)nav, m_parsingErrors);
                    return false;
                }

                if (String.IsNullOrEmpty(queryView))
                {
                    AddToSchemaErrorsWithMemberInfo(Strings.Mapping_Empty_QueryView,
                        setMapping.Set.Name, StorageMappingErrorCode.EmptyQueryView,
                        m_sourceLocation, (IXmlLineInfo)nav, m_parsingErrors);
                    return false;
                }
                setMapping.QueryView = queryView;
                this.m_hasQueryViews = true;
                return true;
            }
            else
            {
                //QV must be typeof or typeofonly view
                if (typeNameString == null || typeNameString.Trim().Length == 0)
                {
                    AddToSchemaErrorsWithMemberInfo(Strings.Mapping_QueryView_TypeName_Not_Defined,
                        setMapping.Set.Name, StorageMappingErrorCode.NoTypeNameForTypeSpecificQueryView,
                        m_sourceLocation, (IXmlLineInfo)nav, m_parsingErrors);
                    return false;
                }

                //Get entity types
                Set<EntityType> entityTypes;
                Set<EntityType> isOfTypeEntityTypes;
                EntityType rootEntityType = (EntityType)setMapping.Set.ElementType;
                if (!TryParseEntityTypeAttribute(nav.Clone(), rootEntityType,
                    e => Strings.Mapping_InvalidContent_Entity_Type_For_Entity_Set(e.FullName, rootEntityType.FullName, setMapping.Set.Name),
                    out isOfTypeEntityTypes,
                    out entityTypes))
                {
                    // Return if we cannot parse entity types
                    return false;
                }
                Debug.Assert(isOfTypeEntityTypes.Count > 0 || entityTypes.Count > 0);
                Debug.Assert(!(isOfTypeEntityTypes.Count > 0 && entityTypes.Count > 0));

                EntityType entityType;
                if (isOfTypeEntityTypes.Count == 1)
                {   //OfType View
                    entityType = isOfTypeEntityTypes.First();
                    includeSubtypes = true;
                }
                else if (entityTypes.Count == 1)
                {   //OfTypeOnly View
                    entityType = entityTypes.First();
                    includeSubtypes = false;
                }
                else
                {
                    //More than one type
                    AddToSchemaErrorsWithMemberInfo(Strings.Mapping_QueryViewMultipleTypeInTypeName, setMapping.Set.ToString(),
                        StorageMappingErrorCode.TypeNameContainsMultipleTypesForQueryView, m_sourceLocation, (IXmlLineInfo)nav, m_parsingErrors);
                    return false;
                }

                //Check if IsTypeOf(A) and A is the base type
                if (includeSubtypes && setMapping.Set.ElementType.EdmEquals(entityType))
                {   //Don't allow TypeOFOnly(a) if a is a base type. 
                    AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_QueryView_For_Base_Type, entityType.ToString(), setMapping.Set.ToString(),
                        StorageMappingErrorCode.IsTypeOfQueryViewForBaseType, m_sourceLocation, (IXmlLineInfo)nav, m_parsingErrors);
                    return false;                    
                }

                if (String.IsNullOrEmpty(queryView))
                {
                    if (includeSubtypes)
                    {
                        AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_Empty_QueryView_OfType,
                            entityType.Name, setMapping.Set.Name, StorageMappingErrorCode.EmptyQueryView,
                            m_sourceLocation, (IXmlLineInfo)nav, m_parsingErrors);
                        return false;
                    }
                    else
                    {
                        AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_Empty_QueryView_OfTypeOnly,
                            setMapping.Set.Name, entityType.Name, StorageMappingErrorCode.EmptyQueryView,
                            m_sourceLocation, (IXmlLineInfo)nav, m_parsingErrors);
                        return false;
                    }
                }


                //Add it to the QV cache
                Triple key = new Triple(setMapping.Set, new Pair<EntityTypeBase, bool>(entityType, includeSubtypes));


                if (setMapping.ContainsTypeSpecificQueryView(key))
                { //two QVs for the same type 

                    EdmSchemaError error = null;
                    if (includeSubtypes)
                    {
                        error =
                            new EdmSchemaError(
                                Strings.Mapping_QueryView_Duplicate_OfType(setMapping.Set, entityType),
                                (int)StorageMappingErrorCode.QueryViewExistsForEntitySetAndType, EdmSchemaErrorSeverity.Error, m_sourceLocation,
                                ((IXmlLineInfo)nav).LineNumber, ((IXmlLineInfo)nav).LinePosition);
                    }
                    else
                    {
                        error =
                            new EdmSchemaError(
                                Strings.Mapping_QueryView_Duplicate_OfTypeOnly(setMapping.Set, entityType),
                                (int)StorageMappingErrorCode.QueryViewExistsForEntitySetAndType, EdmSchemaErrorSeverity.Error, m_sourceLocation,
                                ((IXmlLineInfo)nav).LineNumber, ((IXmlLineInfo)nav).LinePosition);
                    }

                    m_parsingErrors.Add(error);
                    return false;
                }

                setMapping.AddTypeSpecificQueryView(key, queryView);
                return true;
            }
        }        

        /// <summary>
        /// The method loads the child nodes for the AssociationSet Mapping node
        /// into the internal datastructures.
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="entityContainerMapping"></param>
        /// <param name="storageEntityContainerType"></param>
        private void LoadAssociationSetMapping(XPathNavigator nav, StorageEntityContainerMapping entityContainerMapping, EntityContainer storageEntityContainerType)
        {
            IXmlLineInfo navLineInfo = (IXmlLineInfo)nav;

            //Get the AssociationSet name 
            string associationSetName = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.AssociationSetMappingNameAttribute);
            //Get the AssociationType name, need to parse it if the mapping information is being specified for multiple types 
            string associationTypeName = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.AssociationSetMappingTypeNameAttribute);
            //Get the table name. This might be emptystring since the user can have a TableMappingFragment instead of this.
            string tableName = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.EntitySetMappingStoreEntitySetAttribute);
            //Try to find the AssociationSet with the given name in the EntityContainer.
            RelationshipSet relationshipSet;
            entityContainerMapping.EdmEntityContainer.TryGetRelationshipSetByName(associationSetName, false /*ignoreCase*/, out relationshipSet);
            AssociationSet associationSet = relationshipSet as AssociationSet;
            //If no AssociationSet with the given name exists, than Add a schema error and return
            if (associationSet == null)
            {
                AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Association_Set, associationSetName,
                    StorageMappingErrorCode.InvalidAssociationSet, m_sourceLocation, navLineInfo, m_parsingErrors);
                //There is no point in continuing the loading of association set map if the AssociationSetName has a problem
                return;
            }

            if (associationSet.ElementType.IsForeignKey)
            {
                ReferentialConstraint constraint = associationSet.ElementType.ReferentialConstraints.Single();
                IEnumerable<EdmMember> dependentKeys = MetadataHelper.GetEntityTypeForEnd((AssociationEndMember)constraint.ToRole).KeyMembers;
                if (associationSet.ElementType.ReferentialConstraints.Single().ToProperties.All(p => dependentKeys.Contains(p)))
                {
                    EdmSchemaError error = AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_ForeignKey_Association_Set_PKtoPK, associationSetName,
                        StorageMappingErrorCode.InvalidAssociationSet, m_sourceLocation, navLineInfo, m_parsingErrors);
                    //Downgrade to a warning if the foreign key constraint is between keys (for back-compat reasons)
                    error.Severity = EdmSchemaErrorSeverity.Warning;
                }
                else
                {
                    AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_ForeignKey_Association_Set, associationSetName,
                        StorageMappingErrorCode.InvalidAssociationSet, m_sourceLocation, navLineInfo, m_parsingErrors);
                }
                return;
            }

            if (entityContainerMapping.ContainsAssociationSetMapping(associationSet))
            {
                //Can not add this set mapping since our storage dictionary won't allow
                //duplicate maps
                AddToSchemaErrorsWithMemberInfo(Strings.Mapping_Duplicate_CdmAssociationSet_StorageMap, associationSetName,
                    StorageMappingErrorCode.DuplicateSetMapping, m_sourceLocation, navLineInfo, m_parsingErrors);
                return;

            }
            //Create the AssociationSet Mapping which contains the mapping information for association set.
            StorageAssociationSetMapping setMapping = new StorageAssociationSetMapping(associationSet, entityContainerMapping);

            //Set the Start Line Information on Fragment
            setMapping.StartLineNumber = navLineInfo.LineNumber;
            setMapping.StartLinePosition = navLineInfo.LinePosition;


            if (!nav.MoveToChild(XPathNodeType.Element))
            {
                AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Emtpty_SetMap, associationSet.Name,
                    StorageMappingErrorCode.EmptySetMapping, m_sourceLocation, navLineInfo, m_parsingErrors);
                return;
            }

            entityContainerMapping.AddAssociationSetMapping(setMapping);

            //If there is a query view it has to be the first element
            if (nav.LocalName == StorageMslConstructs.QueryViewElement)
            {
                if (!(String.IsNullOrEmpty(tableName)))
                {
                    AddToSchemaErrorsWithMemberInfo(Strings.Mapping_TableName_QueryView, associationSetName,
                        StorageMappingErrorCode.TableNameAttributeWithQueryView, m_sourceLocation, navLineInfo, m_parsingErrors);
                    return;
                }
                //Load the Query View into the set mapping,
                //if you get an error, return immediately since 
                //you go on, you could be giving lot of dubious errors
                if (!LoadQueryView(nav.Clone(), setMapping))
                {
                    return;
                }
                //If there are no more elements just return
                if (!nav.MoveToNext(XPathNodeType.Element))
                {
                    return;
                }
            }

            if ((nav.LocalName == StorageMslConstructs.EndPropertyMappingElement) ||
                     (nav.LocalName == StorageMslConstructs.ModificationFunctionMappingElement))
            {
                if ((String.IsNullOrEmpty(associationTypeName)))
                {
                    AddToSchemaErrors(Strings.Mapping_InvalidContent_Association_Type_Empty,
                        StorageMappingErrorCode.InvalidAssociationType, m_sourceLocation, navLineInfo, m_parsingErrors);
                    return;
                }
                //Load the AssociationTypeMapping into memory.
                LoadAssociationTypeMapping(nav.Clone(), setMapping, associationTypeName, tableName, storageEntityContainerType);
            }
            else if (nav.LocalName == StorageMslConstructs.ConditionElement)
            {
                AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_AssociationSet_Condition, associationSetName,
                    StorageMappingErrorCode.InvalidContent, m_sourceLocation, navLineInfo, m_parsingErrors);
                return;
            }
            else
            {

                Debug.Assert(false, "XSD validation should ensure this");
            }
        }

        #region LoadFunctionImportMapping implementation
        /// <summary>
        /// The method loads a function import mapping element
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="entityContainerMapping"></param>
        /// <param name="storageEntityContainerType"></param>
        private void LoadFunctionImportMapping(XPathNavigator nav, StorageEntityContainerMapping entityContainerMapping, EntityContainer storageEntityContainerType)
        {
            IXmlLineInfo lineInfo = (IXmlLineInfo)(nav.Clone());

            // Get target (store) function
            EdmFunction targetFunction;
            if (!TryGetFunctionImportStoreFunction(nav, out targetFunction))
            {
                return;
            }

            // Get source (model) function
            EdmFunction functionImport;
            if (!TryGetFunctionImportModelFunction(nav, entityContainerMapping, out functionImport))
            {
                return;
            }

            // Validate composability alignment of function import and target function.
            if (!functionImport.IsComposableAttribute && targetFunction.IsComposableAttribute)
            {
                AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_TargetFunctionMustBeNonComposable(functionImport.FullName, targetFunction.FullName),
                    StorageMappingErrorCode.MappingFunctionImportTargetFunctionMustBeNonComposable,
                    m_sourceLocation, lineInfo, m_parsingErrors);
                return;
            }
            else if (functionImport.IsComposableAttribute && !targetFunction.IsComposableAttribute)
            {
                AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_TargetFunctionMustBeComposable(functionImport.FullName, targetFunction.FullName),
                    StorageMappingErrorCode.MappingFunctionImportTargetFunctionMustBeComposable,
                    m_sourceLocation, lineInfo, m_parsingErrors);
                return;
            }

            // Validate parameters are compatible between the store and model functions
            ValidateFunctionImportMappingParameters(nav, targetFunction, functionImport);

            // Process type mapping information
            var typeMappingsList = new List<List<FunctionImportStructuralTypeMapping>>();
            if (nav.MoveToChild(XPathNodeType.Element))
            {
                int resultSetIndex = 0;
                do 
                {
                    if (nav.LocalName == StorageMslConstructs.FunctionImportMappingResultMapping)
                    {
                        List<FunctionImportStructuralTypeMapping> typeMappings = GetFunctionImportMappingResultMapping(nav.Clone(), lineInfo, targetFunction, functionImport, resultSetIndex, typeMappingsList);
                        typeMappingsList.Add(typeMappings);

                    }
                    resultSetIndex++;
                } while (nav.MoveToNext(XPathNodeType.Element));
            }

            // Verify that there are the right number of result mappings
            if (typeMappingsList.Count > 0 && typeMappingsList.Count != functionImport.ReturnParameters.Count)
            {
                AddToSchemaErrors(Strings.Mapping_FunctionImport_ResultMappingCountDoesNotMatchResultCount(functionImport.Identity),
                    StorageMappingErrorCode.FunctionResultMappingCountMismatch, m_sourceLocation, lineInfo, m_parsingErrors);
                return;
            }

            if (functionImport.IsComposableAttribute)
            {
                //
                // Add composable function import mapping to the list.
                //

                // Function mapping is allowed only for TVFs on the s-space.
                var cTypeTargetFunction = this.StoreItemCollection.ConvertToCTypeFunction(targetFunction);
                var cTypeTvfElementType = System.Data.Common.TypeHelpers.GetTvfReturnType(cTypeTargetFunction);
                var sTypeTvfElementType = System.Data.Common.TypeHelpers.GetTvfReturnType(targetFunction);
                if (cTypeTvfElementType == null)
                {
                    Debug.Assert(sTypeTvfElementType == null, "sTypeTvfElementType == null");
                    AddToSchemaErrors(Strings.Mapping_FunctionImport_ResultMapping_InvalidSType(functionImport.Identity),
                        StorageMappingErrorCode.MappingFunctionImportTVFExpected, m_sourceLocation, lineInfo, m_parsingErrors);
                    return;
                }

                Debug.Assert(functionImport.ReturnParameters.Count == 1, "functionImport.ReturnParameters.Count == 1 for a composable function import.");
                var typeMappings = typeMappingsList.Count > 0 ? typeMappingsList[0] : new List<FunctionImportStructuralTypeMapping>();

                FunctionImportMappingComposable mapping = null;
                EdmType resultType;
                if (MetadataHelper.TryGetFunctionImportReturnType<EdmType>(functionImport, 0, out resultType))
                {
                    if (Helper.IsStructuralType(resultType))
                    {
                        if (!TryCreateFunctionImportMappingComposableWithStructuralResult(
                                functionImport,
                                cTypeTargetFunction,
                                typeMappings,
                                (StructuralType)resultType,
                                cTypeTvfElementType,
                                sTypeTvfElementType,
                                lineInfo,
                                out mapping))
                        {
                            return;
                        }
                    }
                    else
                    {
                        Debug.Assert(TypeSemantics.IsScalarType(resultType), "TypeSemantics.IsScalarType(resultType)");
                        Debug.Assert(typeMappings.Count == 0, "typeMappings.Count == 0");
                        if (!TryCreateFunctionImportMappingComposableWithScalarResult(
                                functionImport,
                                cTypeTargetFunction,
                                targetFunction,
                                resultType,
                                cTypeTvfElementType,
                                sTypeTvfElementType,
                                lineInfo,
                                out mapping))
                        {
                            return;
                        }
                    }
                }
                else
                {
                    Debug.Fail("Composable function import must have return type.");
                }
                Debug.Assert(mapping != null, "mapping != null");

                entityContainerMapping.AddFunctionImportMapping(functionImport, mapping);
            }
            else
            {
                //
                // Add non-composable function import mapping to the list.
                //

                var mapping = new FunctionImportMappingNonComposable(functionImport, targetFunction, typeMappingsList, this.EdmItemCollection);

                // Verify that all entity types can be produced.
                foreach (FunctionImportStructuralTypeMappingKB resultMapping in mapping.ResultMappings)
                {
                    resultMapping.ValidateTypeConditions(/*validateAmbiguity: */false, m_parsingErrors, m_sourceLocation);
                }

                // Verify that function imports returning abstract types include explicit mappings
                for (int i = 0; i < mapping.ResultMappings.Count; i++)
                {
                    EntityType returnEntityType;
                    if (MetadataHelper.TryGetFunctionImportReturnType<EntityType>(functionImport, i, out returnEntityType) &&
                        returnEntityType.Abstract &&
                        mapping.GetResultMapping(i).NormalizedEntityTypeMappings.Count == 0)
                    {
                        AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_FunctionImport_ImplicitMappingForAbstractReturnType, returnEntityType.FullName,
                            functionImport.Identity, StorageMappingErrorCode.MappingOfAbstractType, m_sourceLocation, lineInfo, m_parsingErrors);
                    }
                }

                entityContainerMapping.AddFunctionImportMapping(functionImport, mapping);
            }
        }

        private bool TryGetFunctionImportStoreFunction(XPathNavigator nav, out EdmFunction targetFunction)
        {
            IXmlLineInfo xmlLineInfoNav = (IXmlLineInfo)nav;
            targetFunction = null;

            // Get the function name
            string functionName = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.FunctionImportMappingFunctionNameAttribute);

            // Try to find the function definition
            ReadOnlyCollection<EdmFunction> functionOverloads = this.StoreItemCollection.GetFunctions(functionName);

            if (functionOverloads.Count == 0)
            {
                AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_StoreFunctionDoesNotExist(functionName),
                    StorageMappingErrorCode.MappingFunctionImportStoreFunctionDoesNotExist,
                    m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                return false;
            }
            else if (functionOverloads.Count > 1)
            {
                AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_FunctionAmbiguous(functionName),
                    StorageMappingErrorCode.MappingFunctionImportStoreFunctionAmbiguous,
                    m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                return false;
            }

            targetFunction = functionOverloads.Single();

            return true;
        }

        private bool TryGetFunctionImportModelFunction(
            XPathNavigator nav,
            StorageEntityContainerMapping entityContainerMapping,
            out EdmFunction functionImport)
        {
            IXmlLineInfo xmlLineInfoNav = (IXmlLineInfo)nav;

            // Get the function import name
            string functionImportName = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.FunctionImportMappingFunctionImportNameAttribute);

            // Try to find the function import
            EntityContainer modelContainer = entityContainerMapping.EdmEntityContainer;
            functionImport = null;
            foreach (EdmFunction functionImportCandidate in modelContainer.FunctionImports)
            {
                if (functionImportCandidate.Name == functionImportName)
                {
                    functionImport = functionImportCandidate;
                    break;
                }
            }
            if (null == functionImport)
            {
                AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_FunctionImportDoesNotExist(functionImportName, entityContainerMapping.EdmEntityContainer.Name),
                    StorageMappingErrorCode.MappingFunctionImportFunctionImportDoesNotExist,
                    m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                return false;
            }

            // check that no existing mapping exists for this function import
            FunctionImportMapping targetFunctionCollision;
            if (entityContainerMapping.TryGetFunctionImportMapping(functionImport, out targetFunctionCollision))
            {
                AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_FunctionImportMappedMultipleTimes(functionImportName),
                    StorageMappingErrorCode.MappingFunctionImportFunctionImportMappedMultipleTimes,
                    m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                return false;
            }
            return true;
        }

        private void ValidateFunctionImportMappingParameters(XPathNavigator nav, EdmFunction targetFunction, EdmFunction functionImport)
        {
            IXmlLineInfo xmlLineInfoNav = (IXmlLineInfo)nav;

            foreach (FunctionParameter targetParameter in targetFunction.Parameters)
            {
                // find corresponding import parameter
                FunctionParameter importParameter;
                if (!functionImport.Parameters.TryGetValue(targetParameter.Name, false, out importParameter))
                {
                    AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_TargetParameterHasNoCorrespondingImportParameter(targetParameter.Name),
                        StorageMappingErrorCode.MappingFunctionImportTargetParameterHasNoCorrespondingImportParameter,
                        m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                }
                else
                {
                    // parameters must have the same direction (in|out)
                    if (targetParameter.Mode != importParameter.Mode)
                    {
                        AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_IncompatibleParameterMode(targetParameter.Name, targetParameter.Mode, importParameter.Mode),
                            StorageMappingErrorCode.MappingFunctionImportIncompatibleParameterMode,
                            m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                    }

                    PrimitiveType importType = Helper.AsPrimitive(importParameter.TypeUsage.EdmType);
                    Debug.Assert(importType != null, "Function import parameters must be primitive.");

                    if (Helper.IsSpatialType(importType))
                    {
                        importType = Helper.GetSpatialNormalizedPrimitiveType(importType);
                    }


                    PrimitiveType cspaceTargetType = (PrimitiveType)StoreItemCollection.StoreProviderManifest.GetEdmType(targetParameter.TypeUsage).EdmType;
                    if (cspaceTargetType == null)
                    {
                        AddToSchemaErrorWithMessage(Strings.Mapping_ProviderReturnsNullType(targetParameter.Name),
                            StorageMappingErrorCode.MappingStoreProviderReturnsNullEdmType,
                            m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                        return;
                    }

                    // there are no type facets declared for function parameter types;
                    // we simply verify the primitive type kind is equivalent. 
                    // for enums we just use the underlying enum type.
                    if (cspaceTargetType.PrimitiveTypeKind != importType.PrimitiveTypeKind)
                    {
                        var schemaErrorMessage = Helper.IsEnumType(importParameter.TypeUsage.EdmType) ? 
                            Strings.Mapping_FunctionImport_IncompatibleEnumParameterType(
                                targetParameter.Name, 
                                cspaceTargetType.Name, 
                                importParameter.TypeUsage.EdmType.FullName,
                                Helper.GetUnderlyingEdmTypeForEnumType(importParameter.TypeUsage.EdmType).Name) : 
                            Strings.Mapping_FunctionImport_IncompatibleParameterType(
                                targetParameter.Name, 
                                cspaceTargetType.Name, 
                                importType.Name);

                        AddToSchemaErrorWithMessage(
                            schemaErrorMessage,
                            StorageMappingErrorCode.MappingFunctionImportIncompatibleParameterType,
                            m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                    }
                }
            }

            foreach (FunctionParameter importParameter in functionImport.Parameters)
            {
                // find corresponding target parameter
                FunctionParameter targetParameter;
                if (!targetFunction.Parameters.TryGetValue(importParameter.Name, false, out targetParameter))
                {
                    AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_ImportParameterHasNoCorrespondingTargetParameter(importParameter.Name),
                        StorageMappingErrorCode.MappingFunctionImportImportParameterHasNoCorrespondingTargetParameter,
                        m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                }
            }
        }

        private List<FunctionImportStructuralTypeMapping> GetFunctionImportMappingResultMapping(
            XPathNavigator nav,
            IXmlLineInfo functionImportMappingLineInfo,
            EdmFunction targetFunction,
            EdmFunction functionImport, 
            int resultSetIndex,
            List<List<FunctionImportStructuralTypeMapping>> typeMappingsList)
        {
            List<FunctionImportStructuralTypeMapping> typeMappings = new List<FunctionImportStructuralTypeMapping>();
 
            if (nav.MoveToChild(XPathNodeType.Element))
            {
                do
                {
                    EntitySet entitySet = functionImport.EntitySets.Count > resultSetIndex ?
                                          functionImport.EntitySets[resultSetIndex] : null;

                    if (nav.LocalName == StorageMslConstructs.EntityTypeMappingElement)
                    {
                        EntityType resultEntityType;
                        if (MetadataHelper.TryGetFunctionImportReturnType<EntityType>(functionImport, resultSetIndex, out resultEntityType))
                        {
                            // Cannot specify an entity type mapping for a function import that does not return members of an entity set.
                            if (entitySet == null)
                            {
                                AddToSchemaErrors(Strings.Mapping_FunctionImport_EntityTypeMappingForFunctionNotReturningEntitySet(
                                    StorageMslConstructs.EntityTypeMappingElement, functionImport.Identity),
                                    StorageMappingErrorCode.MappingFunctionImportEntityTypeMappingForFunctionNotReturningEntitySet,
                                    m_sourceLocation, functionImportMappingLineInfo, m_parsingErrors);
                            }
                                
                            FunctionImportEntityTypeMapping typeMapping;
                            if (TryLoadFunctionImportEntityTypeMapping(
                                    nav.Clone(),
                                    resultEntityType,
                                    (EntityType e) => Strings.Mapping_FunctionImport_InvalidContentEntityTypeForEntitySet(e.FullName,
                                                                                                                            resultEntityType.FullName,
                                                                                                                            entitySet.Name,
                                                                                                                            functionImport.Identity),
                                    out typeMapping))
                            {
                                typeMappings.Add(typeMapping);
                            }
                        }
                        else
                        {
                            AddToSchemaErrors(Strings.Mapping_FunctionImport_ResultMapping_InvalidCTypeETExpected(functionImport.Identity),
                                StorageMappingErrorCode.MappingFunctionImportUnexpectedEntityTypeMapping,
                                m_sourceLocation, functionImportMappingLineInfo, m_parsingErrors);
                        }
                    }
                    else if (nav.LocalName == StorageMslConstructs.ComplexTypeMappingElement)
                    {
                        ComplexType resultComplexType;
                        if (MetadataHelper.TryGetFunctionImportReturnType<ComplexType>(functionImport, resultSetIndex, out resultComplexType))
                        {
                            Debug.Assert(entitySet == null, "entitySet == null for complex type mapping in function imports.");

                            FunctionImportComplexTypeMapping typeMapping;
                            if (TryLoadFunctionImportComplexTypeMapping(nav.Clone(), resultComplexType, functionImport, out typeMapping))
                            {
                                typeMappings.Add(typeMapping);
                            }
                        }
                        else
                        {
                            AddToSchemaErrors(Strings.Mapping_FunctionImport_ResultMapping_InvalidCTypeCTExpected(functionImport.Identity),
                                StorageMappingErrorCode.MappingFunctionImportUnexpectedComplexTypeMapping,
                                m_sourceLocation, functionImportMappingLineInfo, m_parsingErrors);
                        }
                    }
                }
                while (nav.MoveToNext(XPathNodeType.Element));
            }

            return typeMappings;
        }

        private bool TryLoadFunctionImportComplexTypeMapping(
            XPathNavigator nav,
            ComplexType resultComplexType,
            EdmFunction functionImport,
            out FunctionImportComplexTypeMapping typeMapping)
        {
            typeMapping = null;
            var lineInfo = new LineInfo(nav);

            ComplexType complexType;
            if (!TryParseComplexTypeAttribute(nav, resultComplexType, functionImport, out complexType))
            {
                return false;
            }

            Collection<FunctionImportReturnTypePropertyMapping> columnRenameMappings = new Collection<FunctionImportReturnTypePropertyMapping>();

            if (!LoadFunctionImportStructuralType(nav.Clone(), new List<StructuralType>() { complexType }, columnRenameMappings, null))
            {
                return false;
            }
                
            typeMapping = new FunctionImportComplexTypeMapping(complexType, columnRenameMappings, lineInfo);
            return true;
        }

        private bool TryParseComplexTypeAttribute(XPathNavigator nav, ComplexType resultComplexType, EdmFunction functionImport, out ComplexType complexType)
        {
            IXmlLineInfo xmlLineInfoNav = (IXmlLineInfo)nav;
            string complexTypeName = GetAttributeValue(nav.Clone(), StorageMslConstructs.ComplexTypeMappingTypeNameAttribute);
            complexTypeName = GetAliasResolvedValue(complexTypeName);

            if (!this.EdmItemCollection.TryGetItem<ComplexType>(complexTypeName, out complexType))
            {
                AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Complex_Type, complexTypeName,
                    StorageMappingErrorCode.InvalidComplexType, m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                return false;
            }

            if (!Helper.IsAssignableFrom(resultComplexType, complexType))
            {
                IXmlLineInfo lineInfo = xmlLineInfoNav;
                AddToSchemaErrorWithMessage(
                    Strings.Mapping_FunctionImport_ResultMapping_MappedTypeDoesNotMatchReturnType(functionImport.Identity, complexType.FullName),
                    StorageMappingErrorCode.InvalidComplexType, m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                return false;
            }

            return true;
        }

        private bool TryLoadFunctionImportEntityTypeMapping(
            XPathNavigator nav,
            EntityType resultEntityType,
            Func<EntityType, string> registerEntityTypeMismatchError,
            out FunctionImportEntityTypeMapping typeMapping)
        {
            typeMapping = null;
            var lineInfo = new LineInfo(nav);

            // Process entity type.
            string entityTypeString = GetAttributeValue(nav.Clone(), StorageMslConstructs.EntitySetMappingTypeNameAttribute);
            Set<EntityType> isOfTypeEntityTypes;
            Set<EntityType> entityTypes;
            {
                // Verify the entity type is appropriate to the function import's result entity type.
                if (!TryParseEntityTypeAttribute(nav.Clone(), resultEntityType, registerEntityTypeMismatchError, out isOfTypeEntityTypes, out entityTypes))
                {
                    return false;
                }
            }

            IEnumerable<StructuralType> currentTypesInHierachy = isOfTypeEntityTypes.Concat(entityTypes).Distinct().OfType<StructuralType>();
            Collection<FunctionImportReturnTypePropertyMapping> columnRenameMappings = new Collection<FunctionImportReturnTypePropertyMapping>();

            // Process all conditions and column renames.
            List<FunctionImportEntityTypeMappingCondition> conditions = new List<FunctionImportEntityTypeMappingCondition>();

            if (!LoadFunctionImportStructuralType(nav.Clone(), currentTypesInHierachy, columnRenameMappings, conditions))
            {
                return false;
            }

            typeMapping = new FunctionImportEntityTypeMapping(isOfTypeEntityTypes, entityTypes, conditions, columnRenameMappings, lineInfo);
            return true;
        }

        private bool LoadFunctionImportStructuralType(
            XPathNavigator nav,
            IEnumerable<StructuralType> currentTypes,
            Collection<FunctionImportReturnTypePropertyMapping> columnRenameMappings, 
            List<FunctionImportEntityTypeMappingCondition> conditions)
        {
            Debug.Assert(null != columnRenameMappings, "columnRenameMappings cannot be null");
            Debug.Assert(null != nav, "nav cannot be null");
            Debug.Assert(null != currentTypes, "currentTypes cannot be null");

            IXmlLineInfo lineInfo = (IXmlLineInfo)(nav.Clone());

            if (nav.MoveToChild(XPathNodeType.Element))
            {
                do
                {
                    if (nav.LocalName == StorageMslConstructs.ScalarPropertyElement)
                    {
                        LoadFunctionImportStructuralTypeMappingScalarProperty(nav, columnRenameMappings, currentTypes);
                    }
                    if (nav.LocalName == StorageMslConstructs.ConditionElement)
                    {
                        LoadFunctionImportEntityTypeMappingCondition(nav, conditions);
                    }
                }
                while (nav.MoveToNext(XPathNodeType.Element));
            }

            bool errorFound = false;
            if (null != conditions)
            {
                // make sure a single condition is specified per column
                HashSet<string> columnsWithConditions = new HashSet<string>();
                foreach (var condition in conditions)
                {
                    if (!columnsWithConditions.Add(condition.ColumnName))
                    {
                        AddToSchemaErrorWithMessage(
                            Strings.Mapping_InvalidContent_Duplicate_Condition_Member(condition.ColumnName),
                            StorageMappingErrorCode.ConditionError,
                            m_sourceLocation, lineInfo, m_parsingErrors);
                        errorFound = true;
                    }
                }
            }
            return !errorFound;
        }

        private void LoadFunctionImportStructuralTypeMappingScalarProperty(
            XPathNavigator nav,
            Collection<FunctionImportReturnTypePropertyMapping> columnRenameMappings,
            IEnumerable<StructuralType> currentTypes)
        {
            var lineInfo = new LineInfo(nav);
            string memberName = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.ScalarPropertyNameAttribute);
            string columnName = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.ScalarPropertyColumnNameAttribute);

            // Negative case: the property name is invalid
            if (!currentTypes.All(t=>t.Members.Contains(memberName)))
            {
                AddToSchemaErrorWithMessage(
                    Strings.Mapping_InvalidContent_Cdm_Member(memberName),
                    StorageMappingErrorCode.InvalidEdmMember,
                    m_sourceLocation, lineInfo, m_parsingErrors);
            }

            if (columnRenameMappings.Any(m => m.CMember == memberName))
            {
                // Negative case: duplicate member name mapping in one type rename mapping
                AddToSchemaErrorWithMessage(
                    Strings.Mapping_InvalidContent_Duplicate_Cdm_Member(memberName),
                    StorageMappingErrorCode.DuplicateMemberMapping,
                    m_sourceLocation, lineInfo, m_parsingErrors);
            }
            else
            {
                columnRenameMappings.Add(new FunctionImportReturnTypeScalarPropertyMapping(memberName, columnName, lineInfo));
            }
        }

        private bool TryCreateFunctionImportMappingComposableWithStructuralResult(
            EdmFunction functionImport,
            EdmFunction cTypeTargetFunction,
            List<FunctionImportStructuralTypeMapping> typeMappings,
            StructuralType structuralResultType,
            RowType cTypeTvfElementType,
            RowType sTypeTvfElementType,
            IXmlLineInfo lineInfo,
            out FunctionImportMappingComposable mapping)
        {
            mapping = null;

            // If it is an implicit structural type mapping, add a type mapping fragment for the return type of the function import,
            // unless it is an abstract type.
            if (typeMappings.Count == 0)
            {
                StructuralType resultType;
                if (MetadataHelper.TryGetFunctionImportReturnType<StructuralType>(functionImport, 0, out resultType))
                {
                    if (resultType.Abstract)
                    {
                        AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_FunctionImport_ImplicitMappingForAbstractReturnType,
                            resultType.FullName, functionImport.Identity,
                            StorageMappingErrorCode.MappingOfAbstractType, m_sourceLocation, lineInfo, m_parsingErrors);
                        return false;
                    }
                    if (resultType.BuiltInTypeKind == BuiltInTypeKind.EntityType)
                    {
                        typeMappings.Add(new FunctionImportEntityTypeMapping(
                            Enumerable.Empty<EntityType>(),
                            new EntityType[] { (EntityType)resultType },
                            Enumerable.Empty<FunctionImportEntityTypeMappingCondition>(),
                            new Collection<FunctionImportReturnTypePropertyMapping>(),
                            new LineInfo(lineInfo)));
                    }
                    else
                    {
                        Debug.Assert(resultType.BuiltInTypeKind == BuiltInTypeKind.ComplexType, "resultType.BuiltInTypeKind == BuiltInTypeKind.ComplexType");
                        typeMappings.Add(new FunctionImportComplexTypeMapping(
                            (ComplexType)resultType,
                            new Collection<FunctionImportReturnTypePropertyMapping>(),
                            new LineInfo(lineInfo)));
                    }
                }
            }

            // Validate and convert FunctionImportEntityTypeMapping elements into structure suitable for composable function import mapping.
            var functionImportKB = new FunctionImportStructuralTypeMappingKB(typeMappings, this.EdmItemCollection);

            var structuralTypeMappings = new List<Tuple<StructuralType, List<StorageConditionPropertyMapping>, List<StoragePropertyMapping>>>();
            EdmProperty[] targetFunctionKeys = null;
            if (functionImportKB.MappedEntityTypes.Count > 0)
            {
                // Validate TPH ambiguity.
                if (!functionImportKB.ValidateTypeConditions(/*validateAmbiguity: */true, m_parsingErrors, m_sourceLocation))
                {
                    return false;
                }

                // For each mapped entity type, prepare list of conditions and list of property mappings.
                for (int i = 0; i < functionImportKB.MappedEntityTypes.Count; ++i)
                {
                    List<StorageConditionPropertyMapping> typeConditions;
                    List<StoragePropertyMapping> propertyMappings;
                    if (TryConvertToEntityTypeConditionsAndPropertyMappings(
                            functionImport,
                            functionImportKB,
                            i,
                            cTypeTvfElementType,
                            sTypeTvfElementType,
                            lineInfo, out typeConditions, out propertyMappings))
                    {
                        structuralTypeMappings.Add(Tuple.Create((StructuralType)functionImportKB.MappedEntityTypes[i], typeConditions, propertyMappings));
                    }
                }
                if (structuralTypeMappings.Count < functionImportKB.MappedEntityTypes.Count)
                {
                    // Some of the entity types produced errors during conversion, exit.
                    return false;
                }

                // Infer target function keys based on the c-space entity types.
                if (!TryInferTVFKeys(structuralTypeMappings, out targetFunctionKeys))
                {
                    AddToSchemaErrorsWithMemberInfo(Strings.Mapping_FunctionImport_CannotInferTargetFunctionKeys, functionImport.Identity,
                        StorageMappingErrorCode.MappingFunctionImportCannotInferTargetFunctionKeys, m_sourceLocation, lineInfo, m_parsingErrors);
                    return false;
                }
            }
            else
            {
                ComplexType resultComplexType;
                if (MetadataHelper.TryGetFunctionImportReturnType<ComplexType>(functionImport, 0, out resultComplexType))
                {
                    // Gather and validate complex type property mappings.
                    List<StoragePropertyMapping> propertyMappings;
                    if (!TryConvertToProperyMappings(resultComplexType, cTypeTvfElementType, sTypeTvfElementType, functionImport, functionImportKB, lineInfo, out propertyMappings))
                    {
                        return false;
                    }
                    structuralTypeMappings.Add(Tuple.Create((StructuralType)resultComplexType, new List<StorageConditionPropertyMapping>(), propertyMappings));
                }
                else
                {
                    Debug.Fail("Function import return type is expected to be a collection of complex type.");
                }
            }

            mapping = new FunctionImportMappingComposable(
                functionImport,
                cTypeTargetFunction,
                structuralTypeMappings,
                targetFunctionKeys,
                m_storageMappingItemCollection,
                m_sourceLocation,
                new LineInfo(lineInfo));
            return true;
        }

        /// <summary>
        /// Attempts to infer key columns of the target function based on the function import mapping.
        /// </summary>
        internal static bool TryInferTVFKeys(List<Tuple<StructuralType, List<StorageConditionPropertyMapping>, List<StoragePropertyMapping>>> structuralTypeMappings, out EdmProperty[] keys)
        {
            keys = null;
            Debug.Assert(structuralTypeMappings.Count > 0, "Function import returning entities must have non-empty structuralTypeMappings.");
            foreach (var typeMapping in structuralTypeMappings)
            {
                EdmProperty[] currentKeys;
                if (!TryInferTVFKeysForEntityType((EntityType)typeMapping.Item1, typeMapping.Item3, out currentKeys))
                {
                    keys = null;
                    return false;
                }
                if (keys == null)
                {
                    keys = currentKeys;
                }
                else
                {
                    // Make sure all keys are mapped to the same columns.
                    Debug.Assert(keys.Length == currentKeys.Length, "All subtypes must have the same number of keys.");
                    for (int i = 0; i < keys.Length; ++i)
                    {
                        if (!keys[i].EdmEquals(currentKeys[i]))
                        {
                            keys = null;
                            return false;
                        }
                    }
                }
            }
            // Make sure columns are non-nullable, otherwise it shouldn't be considered a key.
            for (int i = 0; i < keys.Length; ++i)
            {
                if (keys[i].Nullable)
                {
                    keys = null;
                    return false;
                }
            }
            return true;
        }

        private static bool TryInferTVFKeysForEntityType(EntityType entityType, List<StoragePropertyMapping> propertyMappings, out EdmProperty[] keys)
        {
            keys = new EdmProperty[entityType.KeyMembers.Count];
            for (int i = 0; i < keys.Length; ++i)
            {
                var mapping = propertyMappings[entityType.Properties.IndexOf((EdmProperty)entityType.KeyMembers[i])] as StorageScalarPropertyMapping;
                if (mapping == null)
                {
                    keys = null;
                    return false;
                }
                keys[i] = mapping.ColumnProperty;
            }
            return true;
        }

        private bool TryCreateFunctionImportMappingComposableWithScalarResult(
            EdmFunction functionImport,
            EdmFunction cTypeTargetFunction,
            EdmFunction sTypeTargetFunction,
            EdmType scalarResultType,
            RowType cTypeTvfElementType,
            RowType sTypeTvfElementType,
            IXmlLineInfo lineInfo,
            out FunctionImportMappingComposable mapping)
        {
            mapping = null;

            // Make sure that TVF returns exactly one column
            if (cTypeTvfElementType.Properties.Count > 1)
            {
                AddToSchemaErrors(Strings.Mapping_FunctionImport_ScalarMappingToMulticolumnTVF(functionImport.Identity, sTypeTargetFunction.Identity),
                    StorageMappingErrorCode.MappingFunctionImportScalarMappingToMulticolumnTVF, m_sourceLocation, lineInfo, m_parsingErrors);
                return false;
            }

            // Make sure that scalarResultType agrees with the column type.
            if (!ValidateFunctionImportMappingResultTypeCompatibility(TypeUsage.Create(scalarResultType), cTypeTvfElementType.Properties[0].TypeUsage))
            {

                AddToSchemaErrors(Strings.Mapping_FunctionImport_ScalarMappingTypeMismatch(
                    functionImport.ReturnParameter.TypeUsage.EdmType.FullName,
                    functionImport.Identity,
                    sTypeTargetFunction.ReturnParameter.TypeUsage.EdmType.FullName,
                    sTypeTargetFunction.Identity),
                    StorageMappingErrorCode.MappingFunctionImportScalarMappingTypeMismatch, m_sourceLocation, lineInfo, m_parsingErrors);
                return false;
            }

            mapping = new FunctionImportMappingComposable(
                functionImport,
                cTypeTargetFunction,
                null,
                null,
                m_storageMappingItemCollection,
                m_sourceLocation,
                new LineInfo(lineInfo));
            return true;
        }

        private bool ValidateFunctionImportMappingResultTypeCompatibility(TypeUsage cSpaceMemberType, TypeUsage sSpaceMemberType)
        {
            // Function result data flows from S-side to C-side.
            var fromType = sSpaceMemberType;
            var toType = ResolveTypeUsageForEnums(cSpaceMemberType);

            bool directlyPromotable = TypeSemantics.IsStructurallyEqualOrPromotableTo(fromType, toType);
            bool inverselyPromotable = TypeSemantics.IsStructurallyEqualOrPromotableTo(toType, fromType);

            // We are quite lax here. We only require that values belong to the same class (can flow in one or the other direction).
            // We could require precisely s-type to be promotable to c-type, but in this case it won't be possible to reuse the same 
            // c-types for mapped functions and entity sets, because entity sets (read-write) require c-types to be promotable to s-types.
            return directlyPromotable || inverselyPromotable;
        }

        private void LoadFunctionImportEntityTypeMappingCondition(XPathNavigator nav, List<FunctionImportEntityTypeMappingCondition> conditions)
        {
            var lineInfo = new LineInfo(nav);

            string columnName = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.ConditionColumnNameAttribute);
            string value = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.ConditionValueAttribute);
            string isNull = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.ConditionIsNullAttribute);

            //Either Value or NotNull need to be specifid on the condition mapping but not both
            if ((isNull != null) && (value != null))
            {
                AddToSchemaErrors(Strings.Mapping_InvalidContent_ConditionMapping_Both_Values,
                    StorageMappingErrorCode.ConditionError, m_sourceLocation, lineInfo, m_parsingErrors);
            }
            else if ((isNull == null) && (value == null))
            {
                AddToSchemaErrors(Strings.Mapping_InvalidContent_ConditionMapping_Either_Values,
                    StorageMappingErrorCode.ConditionError, m_sourceLocation, lineInfo, m_parsingErrors);
            }
            else
            {
                if (isNull != null)
                {
                    bool isNullValue = Convert.ToBoolean(isNull, CultureInfo.InvariantCulture);
                    conditions.Add(new FunctionImportEntityTypeMappingConditionIsNull(columnName, isNullValue, lineInfo));
                }
                else
                {
                    XPathNavigator columnValue = nav.Clone();
                    columnValue.MoveToAttribute(StorageMslConstructs.ConditionValueAttribute, string.Empty);
                    conditions.Add(new FunctionImportEntityTypeMappingConditionValue(columnName, columnValue, lineInfo));
                }
            }
        }

        private bool TryConvertToEntityTypeConditionsAndPropertyMappings(
            EdmFunction functionImport,
            FunctionImportStructuralTypeMappingKB functionImportKB,
            int typeID,
            RowType cTypeTvfElementType,
            RowType sTypeTvfElementType,
            IXmlLineInfo navLineInfo,
            out List<StorageConditionPropertyMapping> typeConditions,
            out List<StoragePropertyMapping> propertyMappings)
        {
            var entityType = functionImportKB.MappedEntityTypes[typeID];
            typeConditions = new List<StorageConditionPropertyMapping>();

            bool errorFound = false;

            // Gather and validate entity type conditions from the type-producing fragments.
            foreach (var entityTypeMapping in functionImportKB.NormalizedEntityTypeMappings.Where(f => f.ImpliedEntityTypes[typeID]))
            {
                foreach (var condition in entityTypeMapping.ColumnConditions.Where(c => c != null))
                {
                    EdmProperty column;
                    if (sTypeTvfElementType.Properties.TryGetValue(condition.ColumnName, false, out column))
                    {
                        object value;
                        bool? isNull;
                        if (condition.ConditionValue.IsSentinel)
                        {
                            value = null;
                            if (condition.ConditionValue == ValueCondition.IsNull)
                            {
                                isNull = true;
                            }
                            else
                            {
                                Debug.Assert(condition.ConditionValue == ValueCondition.IsNotNull, "Only IsNull or IsNotNull condition values are expected.");
                                isNull = false;
                            }
                        }
                        else
                        {
                            var cTypeColumn = cTypeTvfElementType.Properties[column.Name];
                            Debug.Assert(cTypeColumn != null, "cTypeColumn != null");
                            Debug.Assert(Helper.IsPrimitiveType(cTypeColumn.TypeUsage.EdmType), "S-space columns are expected to be of a primitive type.");
                            var cPrimitiveType = (PrimitiveType)cTypeColumn.TypeUsage.EdmType;
                            Debug.Assert(cPrimitiveType.ClrEquivalentType != null, "Scalar Types should have associated clr type");
                            Debug.Assert(condition is FunctionImportEntityTypeMappingConditionValue, "Non-sentinel condition is expected to be of type FunctionImportEntityTypeMappingConditionValue.");
                            value = ((FunctionImportEntityTypeMappingConditionValue)condition).GetConditionValue(
                                cPrimitiveType.ClrEquivalentType,
                                handleTypeNotComparable: () =>
                                {
                                    AddToSchemaErrorWithMemberAndStructure(
                                        Strings.Mapping_InvalidContent_ConditionMapping_InvalidPrimitiveTypeKind, column.Name, column.TypeUsage.EdmType.FullName,
                                        StorageMappingErrorCode.ConditionError,
                                        m_sourceLocation, condition.LineInfo, m_parsingErrors);
                                },
                                handleInvalidConditionValue: () =>
                                {
                                    AddToSchemaErrors(
                                        Strings.Mapping_ConditionValueTypeMismatch,
                                        StorageMappingErrorCode.ConditionError,
                                        m_sourceLocation, condition.LineInfo, m_parsingErrors);
                                });
                            if (value == null)
                            {
                                errorFound = true;
                                continue;
                            }
                            isNull = null;
                        }
                        typeConditions.Add(new StorageConditionPropertyMapping(null, column, value, isNull));
                    }
                    else
                    {
                        AddToSchemaErrorsWithMemberInfo(
                            Strings.Mapping_InvalidContent_Column, condition.ColumnName,
                            StorageMappingErrorCode.InvalidStorageMember,
                            m_sourceLocation, condition.LineInfo, m_parsingErrors);
                    }
                }
            }

            // Gather and validate entity type property mappings.
            errorFound |= !TryConvertToProperyMappings(entityType, cTypeTvfElementType, sTypeTvfElementType, functionImport, functionImportKB, navLineInfo, out propertyMappings);

            return !errorFound;
        }

        private bool TryConvertToProperyMappings(
            StructuralType structuralType,
            RowType cTypeTvfElementType,
            RowType sTypeTvfElementType,
            EdmFunction functionImport,
            FunctionImportStructuralTypeMappingKB functionImportKB,
            IXmlLineInfo navLineInfo,
            out List<StoragePropertyMapping> propertyMappings)
        {
            propertyMappings = new List<StoragePropertyMapping>();

            // Gather and validate structuralType property mappings.
            bool errorFound = false;
            foreach (EdmProperty property in Common.TypeHelpers.GetAllStructuralMembers(structuralType))
            {
                // Only scalar property mappings are supported at the moment.
                if (!Helper.IsScalarType(property.TypeUsage.EdmType))
                {
                    EdmSchemaError error = new EdmSchemaError(
                        Strings.Mapping_Invalid_CSide_ScalarProperty(property.Name),
                        (int)StorageMappingErrorCode.InvalidTypeInScalarProperty,
                        EdmSchemaErrorSeverity.Error,
                        m_sourceLocation, navLineInfo.LineNumber, navLineInfo.LinePosition);
                    m_parsingErrors.Add(error);
                    errorFound = true;
                    continue;
                }

                string columnName = null;
                IXmlLineInfo columnMappingLineInfo = null;
                FunctionImportReturnTypeStructuralTypeColumnRenameMapping columnRenameMapping;
                bool explicitPropertyMapping;
                if (functionImportKB.ReturnTypeColumnsRenameMapping.TryGetValue(property.Name, out columnRenameMapping))
                {
                    explicitPropertyMapping = true;
                    columnName = columnRenameMapping.GetRename(structuralType, out columnMappingLineInfo);
                }
                else
                {
                    explicitPropertyMapping = false;
                    columnName = property.Name;
                }
                columnMappingLineInfo = columnMappingLineInfo != null && columnMappingLineInfo.HasLineInfo() ? columnMappingLineInfo : navLineInfo;

                EdmProperty column;
                if (sTypeTvfElementType.Properties.TryGetValue(columnName, false, out column))
                {
                    Debug.Assert(cTypeTvfElementType.Properties.Contains(columnName), "cTypeTvfElementType.Properties.Contains(columnName)");
                    var cTypeColumn = cTypeTvfElementType.Properties[columnName];
                    if (ValidateFunctionImportMappingResultTypeCompatibility(property.TypeUsage, cTypeColumn.TypeUsage))
                    {
                        propertyMappings.Add(new StorageScalarPropertyMapping(property, column));
                    }
                    else
                    {
                        EdmSchemaError error = new EdmSchemaError(
                            GetInvalidMemberMappingErrorMessage(property, column),
                            (int)StorageMappingErrorCode.IncompatibleMemberMapping,
                            EdmSchemaErrorSeverity.Error,
                            m_sourceLocation, columnMappingLineInfo.LineNumber, columnMappingLineInfo.LinePosition);
                        m_parsingErrors.Add(error);
                    }
                }
                else
                {
                    if (explicitPropertyMapping)
                    {
                        AddToSchemaErrorsWithMemberInfo(
                            Strings.Mapping_InvalidContent_Column, columnName,
                            StorageMappingErrorCode.InvalidStorageMember,
                            m_sourceLocation, columnMappingLineInfo, m_parsingErrors);
                    }
                    else
                    {
                        var error = new EdmSchemaError(
                            Strings.Mapping_FunctionImport_PropertyNotMapped(property.Name, structuralType.FullName, functionImport.Identity),
                            (int)StorageMappingErrorCode.MappingFunctionImportReturnTypePropertyNotMapped,
                            EdmSchemaErrorSeverity.Error,
                            m_sourceLocation, columnMappingLineInfo.LineNumber, columnMappingLineInfo.LinePosition);
                        m_parsingErrors.Add(error);
                        errorFound = true;
                    }
                }
            }

            // Make sure that propertyMappings is in the order of properties of the structuredType.
            // The rest of the code depends on it.
            Debug.Assert(errorFound ||
                Common.TypeHelpers.GetAllStructuralMembers(structuralType).Count == propertyMappings.Count &&
                Common.TypeHelpers.GetAllStructuralMembers(structuralType).Cast<EdmMember>().Zip(propertyMappings)
                    .All(ppm => ppm.Key.EdmEquals(ppm.Value.EdmProperty)), "propertyMappings order does not correspond to the order of properties in the structuredType.");

            return !errorFound;
        }
        #endregion

        /// <summary>
        /// The method loads the child nodes for the AssociationType Mapping node
        /// into the internal datastructures.
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="associationSetMapping"></param>
        /// <param name="associationTypeName"></param>
        /// <param name="tableName"></param>
        /// <param name="storageEntityContainerType"></param>
        private void LoadAssociationTypeMapping(XPathNavigator nav, StorageAssociationSetMapping associationSetMapping, string associationTypeName, string tableName, EntityContainer storageEntityContainerType)
        {
            IXmlLineInfo navLineInfo = (IXmlLineInfo)nav;

            //Get the association type for association type name specified in MSL
            //If no AssociationType with the given name exists, add a schema error and return
            AssociationType associationType;
            this.EdmItemCollection.TryGetItem<AssociationType>(associationTypeName, out associationType);
            if (associationType == null)
            {
                //There is no point in continuing loading if the AssociationType is null
                AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Association_Type, associationTypeName,
                    StorageMappingErrorCode.InvalidAssociationType, m_sourceLocation, navLineInfo, m_parsingErrors);
                return;
            }
            //Verify that AssociationType specified should be the declared type of
            //AssociationSet or a derived Type of it.
            //Future Enhancement : Change the code to use EdmEquals
            if ((!(associationSetMapping.Set.ElementType.Equals(associationType))))
            {
                AddToSchemaErrorWithMessage(Strings.Mapping_Invalid_Association_Type_For_Association_Set(associationTypeName,
                    associationSetMapping.Set.ElementType.FullName, associationSetMapping.Set.Name),
                    StorageMappingErrorCode.DuplicateTypeMapping, m_sourceLocation, navLineInfo, m_parsingErrors);
                return;
            }

            //Create an AssociationTypeMapping to hold the information for AssociationType mapping.
            StorageAssociationTypeMapping associationTypeMapping = new StorageAssociationTypeMapping(associationType, associationSetMapping);
            associationSetMapping.AddTypeMapping(associationTypeMapping);
            //If the table name was not specified on the AssociationSetMapping element 
            //Then there should have been a query view. Otherwise throw.
            if (String.IsNullOrEmpty(tableName) && (associationSetMapping.QueryView == null))
            {
                AddToSchemaErrors(Strings.Mapping_InvalidContent_Table_Expected, StorageMappingErrorCode.InvalidTable,
                    m_sourceLocation, navLineInfo, m_parsingErrors);
            }
            else
            {
                StorageMappingFragment fragment = LoadAssociationMappingFragment(nav.Clone(), associationSetMapping, associationTypeMapping, tableName, storageEntityContainerType);
                if (fragment != null)
                {
                    //Fragment can be null because of validation errors
                    associationTypeMapping.AddFragment(fragment);
                }
            }
        }

        /// <summary>
        /// Loads function mappings for the entity type.
        /// </summary>
        /// <param name="associationSetMapping"></param>
        /// <param name="associationTypeMapping"></param>
        /// <param name="nav"></param>
        private void LoadAssociationTypeModificationFunctionMapping(
            XPathNavigator nav,
            StorageAssociationSetMapping associationSetMapping,
            StorageAssociationTypeMapping associationTypeMapping)
        {
            // create function loader
            ModificationFunctionMappingLoader functionLoader = new ModificationFunctionMappingLoader(this, associationSetMapping.Set);

            // Load all function definitions (for insert, delete and update)
            StorageModificationFunctionMapping deleteFunctionMapping = null;
            StorageModificationFunctionMapping insertFunctionMapping = null;
            if (nav.MoveToChild(XPathNodeType.Element))
            {
                do
                {
                    switch (nav.LocalName)
                    {
                        case StorageMslConstructs.DeleteFunctionElement:
                            deleteFunctionMapping = functionLoader.LoadAssociationSetModificationFunctionMapping(nav.Clone(), associationSetMapping.Set, false);
                            break;
                        case StorageMslConstructs.InsertFunctionElement:
                            insertFunctionMapping = functionLoader.LoadAssociationSetModificationFunctionMapping(nav.Clone(), associationSetMapping.Set, true);
                            break;
                    }
                } while (nav.MoveToNext(XPathNodeType.Element));
            }

            // register function mapping information
            associationSetMapping.ModificationFunctionMapping = new StorageAssociationSetModificationFunctionMapping(
                (AssociationSet)associationSetMapping.Set, deleteFunctionMapping, insertFunctionMapping);
        }

        /// <summary>
        /// The method loads the child nodes for the TableMappingFragment under the EntityType node
        /// into the internal datastructures.
        /// </summary>
        private StorageMappingFragment LoadMappingFragment(
            XPathNavigator nav,
            StorageEntityTypeMapping typeMapping,
            string tableName,
            EntityContainer storageEntityContainerType,
            bool distinctFlag)
        {
            IXmlLineInfo navLineInfo = (IXmlLineInfo)nav;

            //First make sure that there was no QueryView specified for this Set
            if (typeMapping.SetMapping.QueryView != null)
            {
                AddToSchemaErrorsWithMemberInfo(Strings.Mapping_QueryView_PropertyMaps, typeMapping.SetMapping.Set.Name,
                    StorageMappingErrorCode.PropertyMapsWithQueryView, m_sourceLocation, navLineInfo, m_parsingErrors);
                return null;
            }

            //Get the table type that represents this table
            EntitySet tableMember;
            storageEntityContainerType.TryGetEntitySetByName(tableName, false /*ignoreCase*/, out tableMember);
            if (tableMember == null)
            {
                //There is no point in continuing loading if the Table on S side can not be found
                AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Table, tableName,
                    StorageMappingErrorCode.InvalidTable, m_sourceLocation, navLineInfo, m_parsingErrors);
                return null;
            }
            EntityType tableType = tableMember.ElementType;
            //Create a table mapping fragment to hold the mapping information for a TableMappingFragment node
            StorageMappingFragment fragment = new StorageMappingFragment(tableMember, typeMapping, distinctFlag);
            //Set the Start Line Information on Fragment
            fragment.StartLineNumber = navLineInfo.LineNumber;
            fragment.StartLinePosition = navLineInfo.LinePosition;

            //Go through the property mappings for this TableMappingFragment and load them in memory.
            if (nav.MoveToChild(XPathNodeType.Element))
            {
                do
                {
                    //need to get the type that this member exists in
                    EdmType containerType = null;
                    string propertyName = StorageMappingItemLoader.GetAttributeValue(nav.Clone(), StorageMslConstructs.ComplexPropertyNameAttribute);
                    //PropertyName could be null for Condition Maps
                    if (propertyName != null)
                    {
                        containerType = typeMapping.GetContainerType(propertyName);
                    }
                    switch (nav.LocalName)
                    {
                        case StorageMslConstructs.ScalarPropertyElement:
                            StorageScalarPropertyMapping scalarMap = LoadScalarPropertyMapping(nav.Clone(), containerType, tableType.Properties);
                            if (scalarMap != null)
                            {
                                //scalarMap can be null in invalid cases
                                fragment.AddProperty(scalarMap);
                            }
                            break;
                        case StorageMslConstructs.ComplexPropertyElement:
                            StorageComplexPropertyMapping complexMap =
                                LoadComplexPropertyMapping(nav.Clone(), containerType, tableType.Properties);
                            //Complex Map can be null in case of invalid MSL files.
                            if (complexMap != null)
                            {
                                fragment.AddProperty(complexMap);
                            }
                            break;
                        case StorageMslConstructs.ConditionElement:
                            StorageConditionPropertyMapping conditionMap =
                                LoadConditionPropertyMapping(nav.Clone(), containerType, tableType.Properties);
                            //conditionMap can be null in cases of invalid Map
                            if (conditionMap != null)
                            {
                                fragment.AddConditionProperty(conditionMap, duplicateMemberConditionError: (member) =>
                                    {
                                        AddToSchemaErrorsWithMemberInfo(
                                                            Strings.Mapping_InvalidContent_Duplicate_Condition_Member, member.Name,
                                                            StorageMappingErrorCode.ConditionError,
                                                            m_sourceLocation, navLineInfo, m_parsingErrors);
                                    });
                            }
                            break;
                        default:
                            AddToSchemaErrors(Strings.Mapping_InvalidContent_General,
                                StorageMappingErrorCode.InvalidContent, m_sourceLocation, navLineInfo, m_parsingErrors);
                            break;
                    }
                } while (nav.MoveToNext(XPathNodeType.Element));
            }

            nav.MoveToChild(XPathNodeType.Element);
            return fragment;
        }

        /// <summary>
        /// The method loads the child nodes for the TableMappingFragment under the AssociationType node
        /// into the internal datastructures.
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="typeMapping"></param>
        /// <param name="setMapping"></param>
        /// <param name="tableName"></param>
        /// <param name="storageEntityContainerType"></param>
        /// <returns></returns>
        private StorageMappingFragment LoadAssociationMappingFragment(XPathNavigator nav, StorageAssociationSetMapping setMapping, StorageAssociationTypeMapping typeMapping, string tableName, EntityContainer storageEntityContainerType)
        {
            IXmlLineInfo navLineInfo = (IXmlLineInfo)nav;
            StorageMappingFragment fragment = null;
            EntityType tableType = null;

            //If there is a query view, Dont create a mapping fragment since there should n't be one
            if (setMapping.QueryView == null)
            {
                //Get the table type that represents this table
                EntitySet tableMember;
                storageEntityContainerType.TryGetEntitySetByName(tableName, false /*ignoreCase*/, out tableMember);
                if (tableMember == null)
                {
                    //There is no point in continuing loading if the Table is null
                    AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Table, tableName,
                        StorageMappingErrorCode.InvalidTable, m_sourceLocation, navLineInfo, m_parsingErrors);
                    return null;
                }
                tableType = tableMember.ElementType;
                //Create a Mapping fragment and load all the End node under it
                fragment = new StorageMappingFragment(tableMember, typeMapping, false /*No distinct flag*/);
                //Set the Start Line Information on Fragment, For AssociationSet there are 
                //no fragments, so the start Line Info is same as that of Set
                fragment.StartLineNumber = setMapping.StartLineNumber;
                fragment.StartLinePosition = setMapping.StartLinePosition;
            }

            do
            {
                //need to get the type that this member exists in
                switch (nav.LocalName)
                {
                    case StorageMslConstructs.EndPropertyMappingElement:
                        //Make sure that there was no QueryView specified for this Set
                        if (setMapping.QueryView != null)
                        {
                            AddToSchemaErrorsWithMemberInfo(Strings.Mapping_QueryView_PropertyMaps, setMapping.Set.Name,
                                StorageMappingErrorCode.PropertyMapsWithQueryView, m_sourceLocation, navLineInfo, m_parsingErrors);
                            return null;
                        }
                        string endName = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.EndPropertyMappingNameAttribute);
                        EdmMember endMember = null;
                        typeMapping.AssociationType.Members.TryGetValue(endName, false, out endMember);
                        AssociationEndMember end = endMember as AssociationEndMember;
                        if (end == null)
                        {
                            //Don't try to load the end property map if the end property itself is null
                            AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_End, endName,
                                StorageMappingErrorCode.InvalidEdmMember, m_sourceLocation, navLineInfo, m_parsingErrors);
                            continue;
                        }
                        fragment.AddProperty((LoadEndPropertyMapping(nav.Clone(), end, tableType)));
                        break;
                    case StorageMslConstructs.ConditionElement:
                        //Make sure that there was no QueryView specified for this Set
                        if (setMapping.QueryView != null)
                        {
                            AddToSchemaErrorsWithMemberInfo(Strings.Mapping_QueryView_PropertyMaps, setMapping.Set.Name,
                                StorageMappingErrorCode.PropertyMapsWithQueryView, m_sourceLocation, navLineInfo, m_parsingErrors);
                            return null;
                        }
                        //Need to add validation for conditions in Association mapping fragment.
                        StorageConditionPropertyMapping conditionMap = LoadConditionPropertyMapping(nav.Clone(), null /*containerType*/, tableType.Properties);
                        //conditionMap can be null in cases of invalid Map
                        if (conditionMap != null)
                        {
                            fragment.AddConditionProperty(conditionMap, duplicateMemberConditionError: (member) =>
                                {
                                    AddToSchemaErrorsWithMemberInfo(
                                        Strings.Mapping_InvalidContent_Duplicate_Condition_Member, member.Name,
                                        StorageMappingErrorCode.ConditionError,
                                        m_sourceLocation, navLineInfo, m_parsingErrors);
                                });
                        }
                        break;
                    case StorageMslConstructs.ModificationFunctionMappingElement:
                        setMapping.HasModificationFunctionMapping = true;
                        LoadAssociationTypeModificationFunctionMapping(nav.Clone(), setMapping, typeMapping);
                        break;
                    default:
                        AddToSchemaErrors(Strings.Mapping_InvalidContent_General,
                            StorageMappingErrorCode.InvalidContent, m_sourceLocation, navLineInfo, m_parsingErrors);
                        break;
                }
            } while (nav.MoveToNext(XPathNodeType.Element));

            return fragment;
        }

        /// <summary>
        /// The method loads the ScalarProperty mapping
        /// into the internal datastructures.
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="containerType"></param>
        /// <param name="tableType"></param>
        /// <returns></returns>
        private StorageScalarPropertyMapping LoadScalarPropertyMapping(XPathNavigator nav, EdmType containerType, ReadOnlyMetadataCollection<EdmProperty> tableProperties)
        {
            IXmlLineInfo xmlLineInfoNav = (IXmlLineInfo)nav;

            //Get the property name from MSL.
            string propertyName = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.ScalarPropertyNameAttribute);
            EdmProperty member = null;
            if (!String.IsNullOrEmpty(propertyName))
            {
                //If the container type is a collection type, there wouldn't be a member to represent this scalar property
                if (containerType == null || !(Helper.IsCollectionType(containerType)))
                {
                    //If container type is null that means we have not found the member in any of the IsOfTypes.
                    if (containerType != null)
                    {
                        if (Helper.IsRefType(containerType))
                        {
                            RefType refType = (RefType)containerType;
                            ((EntityType)refType.ElementType).Properties.TryGetValue(propertyName, false /*ignoreCase*/, out member);
                        }
                        else
                        {
                            EdmMember tempMember;
                            (containerType as StructuralType).Members.TryGetValue(propertyName, false, out tempMember);
                            member = tempMember as EdmProperty;
                        }
                    }
                    if (member == null)
                    {
                        AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Cdm_Member, propertyName,
                            StorageMappingErrorCode.InvalidEdmMember, m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
                    }
                }
            }
            //Get the property from Storeside
            string columnName = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.ScalarPropertyColumnNameAttribute);
            Debug.Assert(columnName != null, "XSD validation should have caught this");
            EdmProperty columnMember;
            tableProperties.TryGetValue(columnName, false, out columnMember);
            if (columnMember == null)
            {
                AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Column, columnName,
                    StorageMappingErrorCode.InvalidStorageMember, m_sourceLocation, xmlLineInfoNav, m_parsingErrors);
            }
            //Don't create scalar property map if the property or column metadata is null
            if ((member == null) || (columnMember == null))
            {
                return null;
            }

            if (!Helper.IsScalarType(member.TypeUsage.EdmType))
            {
                EdmSchemaError error = new EdmSchemaError(
                    Strings.Mapping_Invalid_CSide_ScalarProperty(
                        member.Name),
                    (int)StorageMappingErrorCode.InvalidTypeInScalarProperty,
                    EdmSchemaErrorSeverity.Error,
                    m_sourceLocation,
                    xmlLineInfoNav.LineNumber,
                    xmlLineInfoNav.LinePosition);
                m_parsingErrors.Add(error);
                return null;
            }

            ValidateAndUpdateScalarMemberMapping(member, columnMember, xmlLineInfoNav);
            StorageScalarPropertyMapping scalarPropertyMapping = new StorageScalarPropertyMapping(member, columnMember);
            return scalarPropertyMapping;
        }

        /// <summary>
        /// The method loads the ComplexProperty mapping into the internal datastructures.
        /// </summary>
        private StorageComplexPropertyMapping LoadComplexPropertyMapping(XPathNavigator nav, EdmType containerType, ReadOnlyMetadataCollection<EdmProperty> tableProperties)
        {
            IXmlLineInfo navLineInfo = (IXmlLineInfo)nav;

            CollectionType collectionType = containerType as CollectionType;
            //Get the property name from MSL
            string propertyName = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.ComplexPropertyNameAttribute);
            //Get the member metadata from the contianer type passed in.
            //But if the continer type is collection type, there would n't be any member to represent the member.
            EdmProperty member = null;
            EdmType memberType = null;
            //If member specified the type name, it takes precedence
            string memberTypeName = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.ComplexTypeMappingTypeNameAttribute);
            StructuralType containerStructuralType = containerType as StructuralType;

            if (String.IsNullOrEmpty(memberTypeName))
            {
                if (collectionType == null)
                {
                    if (containerStructuralType != null)
                    {
                        EdmMember tempMember;
                        containerStructuralType.Members.TryGetValue(propertyName, false /*ignoreCase*/, out tempMember);
                        member = tempMember as EdmProperty;
                        if (member == null)
                        {
                            AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Cdm_Member, propertyName,
                                StorageMappingErrorCode.InvalidEdmMember, m_sourceLocation, navLineInfo, m_parsingErrors);
                        }
                        memberType = member.TypeUsage.EdmType;
                    }
                    else
                    {
                        AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Cdm_Member, propertyName,
                                                   StorageMappingErrorCode.InvalidEdmMember, m_sourceLocation, navLineInfo, m_parsingErrors);
                    }
                }
                else
                {
                    memberType = collectionType.TypeUsage.EdmType;
                }
            }
            else
            {
                //If container type is null that means we have not found the member in any of the IsOfTypes.
                if (containerType != null)
                {
                    EdmMember tempMember;
                    containerStructuralType.Members.TryGetValue(propertyName, false /*ignoreCase*/, out tempMember);
                    member = tempMember as EdmProperty;
                }
                if (member == null)
                {
                    AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Cdm_Member, propertyName,
                        StorageMappingErrorCode.InvalidEdmMember, m_sourceLocation, navLineInfo, m_parsingErrors);
                }
                this.EdmItemCollection.TryGetItem<EdmType>(memberTypeName, out memberType);
                memberType = memberType as ComplexType;
                // If member type is null, that means the type wasn't found in the workspace
                if (memberType == null)
                {
                    AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Complex_Type, memberTypeName,
                        StorageMappingErrorCode.InvalidComplexType, m_sourceLocation, navLineInfo, m_parsingErrors);
                }
            }

            StorageComplexPropertyMapping complexPropertyMapping = new StorageComplexPropertyMapping(member);

            XPathNavigator cloneNav = nav.Clone();
            bool hasComplexTypeMappingElements = false;
            if (cloneNav.MoveToChild(XPathNodeType.Element))
            {
                if (cloneNav.LocalName == StorageMslConstructs.ComplexTypeMappingElement)
                {
                    hasComplexTypeMappingElements = true;
                }
            }

            //There is no point in continuing if the complex member or complex member type is null
            if ((member == null) || (memberType == null))
            {
                return null;
            }

            if (hasComplexTypeMappingElements)
            {
                nav.MoveToChild(XPathNodeType.Element);
                do
                {
                    complexPropertyMapping.AddTypeMapping(LoadComplexTypeMapping(nav.Clone(), null, tableProperties));
                }
                while (nav.MoveToNext(XPathNodeType.Element));
            }
            else
            {
                complexPropertyMapping.AddTypeMapping(LoadComplexTypeMapping(nav.Clone(), memberType, tableProperties));
            }
            return complexPropertyMapping;
        }

        private StorageComplexTypeMapping LoadComplexTypeMapping(XPathNavigator nav, EdmType type, ReadOnlyMetadataCollection<EdmProperty> tableType)
        {
            //Get the IsPartial attribute from MSL
            bool isPartial = false;
            string partialAttribute = StorageMappingItemLoader.GetAttributeValue(nav.Clone(), StorageMslConstructs.ComplexPropertyIsPartialAttribute);
            if (!String.IsNullOrEmpty(partialAttribute))
            {
                //XSD validation should have guarenteed that the attribute value can only be true or false
                Debug.Assert(partialAttribute == "true" || partialAttribute == "false");
                isPartial = Convert.ToBoolean(partialAttribute, System.Globalization.CultureInfo.InvariantCulture);
            }
            //Create an ComplexTypeMapping to hold the information for Type mapping.
            StorageComplexTypeMapping typeMapping = new StorageComplexTypeMapping(isPartial);
            if (type != null)
            {
                typeMapping.AddType(type as ComplexType);
            }
            else
            {
                Debug.Assert(nav.LocalName == StorageMslConstructs.ComplexTypeMappingElement);
                string typeName = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.ComplexTypeMappingTypeNameAttribute);
                int index = typeName.IndexOf(StorageMslConstructs.TypeNameSperator);
                string currentTypeName = null;
                do
                {
                    if (index != -1)
                    {
                        currentTypeName = typeName.Substring(0, index);
                        typeName = typeName.Substring(index + 1, (typeName.Length - (index + 1)));
                    }
                    else
                    {
                        currentTypeName = typeName;
                        typeName = string.Empty;
                    }

                    int isTypeOfIndex = currentTypeName.IndexOf(StorageMslConstructs.IsTypeOf, StringComparison.Ordinal);
                    if (isTypeOfIndex == 0)
                    {
                        currentTypeName = currentTypeName.Substring(StorageMslConstructs.IsTypeOf.Length, (currentTypeName.Length - (StorageMslConstructs.IsTypeOf.Length + 1)));
                        currentTypeName = GetAliasResolvedValue(currentTypeName);
                    }
                    else
                    {
                        currentTypeName = GetAliasResolvedValue(currentTypeName);
                    }
                    ComplexType complexType;
                    this.EdmItemCollection.TryGetItem<ComplexType>(currentTypeName, out complexType);
                    if (complexType == null)
                    {
                        AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Complex_Type, currentTypeName,
                            StorageMappingErrorCode.InvalidComplexType, m_sourceLocation, (IXmlLineInfo)nav, m_parsingErrors);
                        index = typeName.IndexOf(StorageMslConstructs.TypeNameSperator);
                        continue;
                    }
                    if (isTypeOfIndex == 0)
                    {
                        typeMapping.AddIsOfType(complexType);
                    }
                    else
                    {
                        typeMapping.AddType(complexType);
                    }
                    index = typeName.IndexOf(StorageMslConstructs.TypeNameSperator);
                } while (typeName.Length != 0);
            }

            //Now load the children of ComplexTypeMapping
            if (nav.MoveToChild(XPathNodeType.Element))
            {
                do
                {
                    EdmType containerType = typeMapping.GetOwnerType(StorageMappingItemLoader.GetAttributeValue(nav.Clone(), StorageMslConstructs.ComplexPropertyNameAttribute));
                    switch (nav.LocalName)
                    {
                        case StorageMslConstructs.ScalarPropertyElement:
                            StorageScalarPropertyMapping scalarMap =
                                LoadScalarPropertyMapping(nav.Clone(), containerType, tableType);
                            //ScalarMap can be null in case of invalid MSL files
                            if (scalarMap != null)
                            {
                                typeMapping.AddProperty(scalarMap);
                            }
                            break;
                        case StorageMslConstructs.ComplexPropertyElement:
                            StorageComplexPropertyMapping complexMap =
                                LoadComplexPropertyMapping(nav.Clone(), containerType, tableType);
                            //complexMap can be null in case of invalid maps
                            if (complexMap != null)
                            {
                                typeMapping.AddProperty(complexMap);
                            }
                            break;
                        case StorageMslConstructs.ConditionElement:
                            StorageConditionPropertyMapping conditionMap =
                                LoadConditionPropertyMapping(nav.Clone(), containerType, tableType);
                            if (conditionMap != null)
                            {
                                typeMapping.AddConditionProperty(conditionMap, duplicateMemberConditionError: (member) =>
                                    {
                                        AddToSchemaErrorsWithMemberInfo(
                                            Strings.Mapping_InvalidContent_Duplicate_Condition_Member, member.Name,
                                            StorageMappingErrorCode.ConditionError,
                                            m_sourceLocation, (IXmlLineInfo)nav, m_parsingErrors);
                                    });
                            }
                            break;
                        default:
                            throw System.Data.Entity.Error.NotSupported();
                    }
                } while (nav.MoveToNext(XPathNodeType.Element));
            }
            return typeMapping;

        }

        /// <summary>
        /// The method loads the EndProperty mapping
        /// into the internal datastructures.
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="end"></param>
        /// <param name="tableType"></param>
        /// <returns></returns>
        private StorageEndPropertyMapping LoadEndPropertyMapping(XPathNavigator nav, AssociationEndMember end, EntityType tableType)
        {
            //FutureEnhancement : Change End Property Mapping to not derive from
            //                    StoragePropertyMapping
            StorageEndPropertyMapping endMapping = new StorageEndPropertyMapping(null);
            endMapping.EndMember = end;

            nav.MoveToChild(XPathNodeType.Element);
            do
            {
                switch (nav.LocalName)
                {
                    case StorageMslConstructs.ScalarPropertyElement:
                        RefType endRef = end.TypeUsage.EdmType as RefType;
                        Debug.Assert(endRef != null);
                        EntityTypeBase containerType = endRef.ElementType;
                        StorageScalarPropertyMapping scalarMap = LoadScalarPropertyMapping(nav.Clone(), containerType, tableType.Properties);
                        //Scalar Property Mapping can be null
                        //in case of invalid MSL files.
                        if (scalarMap != null)
                        {

                            //Make sure that the properties mapped as part of EndProperty maps are the key properties.
                            //If any other property is mapped, we should raise an error.
                            if (!containerType.KeyMembers.Contains(scalarMap.EdmProperty))
                            {
                                IXmlLineInfo navLineInfo = (IXmlLineInfo)nav;
                                AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_EndProperty, scalarMap.EdmProperty.Name,
                                    StorageMappingErrorCode.InvalidEdmMember, m_sourceLocation, navLineInfo, m_parsingErrors);
                                return null;

                            }
                            endMapping.AddProperty(scalarMap);
                        }
                        break;
                    default:
                        Debug.Fail("XSD validation should have ensured that End EdmProperty Maps only have Schalar properties");
                        break;
                }
            } while (nav.MoveToNext(XPathNodeType.Element));
            return endMapping;
        }

        /// <summary>
        /// The method loads the ConditionProperty mapping
        /// into the internal datastructures.
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="containerType"></param>
        /// <param name="tableType"></param>
        /// <returns></returns>
        private StorageConditionPropertyMapping LoadConditionPropertyMapping(XPathNavigator nav, EdmType containerType, ReadOnlyMetadataCollection<EdmProperty> tableProperties)
        {
            //Get the CDM side property name.
            string propertyName = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.ConditionNameAttribute);
            //Get the Store side property name from Storeside
            string columnName = GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.ConditionColumnNameAttribute);

            IXmlLineInfo navLineInfo = (IXmlLineInfo)nav;

            //Either the property name or column name can be specified but both can not be.
            if ((propertyName != null) && (columnName != null))
            {
                AddToSchemaErrors(Strings.Mapping_InvalidContent_ConditionMapping_Both_Members,
                    StorageMappingErrorCode.ConditionError, m_sourceLocation, navLineInfo, m_parsingErrors);
                return null;
            }
            if ((propertyName == null) && (columnName == null))
            {
                AddToSchemaErrors(Strings.Mapping_InvalidContent_ConditionMapping_Either_Members,
                    StorageMappingErrorCode.ConditionError, m_sourceLocation, navLineInfo, m_parsingErrors);
                return null;
            }

            EdmProperty member = null;
            //Get the CDM EdmMember reprsented by the name specified.
            if (propertyName != null)
            {
                EdmMember tempMember;
                //If container type is null that means we have not found the member in any of the IsOfTypes.
                if (containerType != null)
                {
                    ((StructuralType)containerType).Members.TryGetValue(propertyName, false /*ignoreCase*/, out tempMember);
                    member = tempMember as EdmProperty;
                }
            }

            //Get the column EdmMember represented by the column name specified
            EdmProperty columnMember = null;
            if (columnName != null)
            {
                tableProperties.TryGetValue(columnName, false, out columnMember);
            }

            //Get the member for which the condition is being specified
            EdmProperty conditionMember = (columnMember != null) ? columnMember : member;
            if (conditionMember == null)
            {
                AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_ConditionMapping_InvalidMember, ((columnName != null) ? columnName : propertyName),
                    StorageMappingErrorCode.ConditionError, m_sourceLocation, navLineInfo, m_parsingErrors);
                return null;
            }

            Nullable<bool> isNullValue = null;
            object value = null;
            //Get the attribute value for IsNull attribute
            string isNullAttribute = StorageMappingItemLoader.GetAttributeValue(nav.Clone(), StorageMslConstructs.ConditionIsNullAttribute);

            //Get strongly Typed value if the condition was specified for a specific condition
            EdmType edmType = conditionMember.TypeUsage.EdmType;
            if (Helper.IsPrimitiveType(edmType))
            {
                //Decide if the member is of a type that we would allow a condition on.
                //First convert the type to C space, if this is a condition in s space( before checking this).
                TypeUsage cspaceTypeUsage;
                if (conditionMember.DeclaringType.DataSpace == DataSpace.SSpace)
                {
                    cspaceTypeUsage = StoreItemCollection.StoreProviderManifest.GetEdmType(conditionMember.TypeUsage);
                    if (cspaceTypeUsage == null)
                    {
                        AddToSchemaErrorWithMessage(Strings.Mapping_ProviderReturnsNullType(conditionMember.Name),
                            StorageMappingErrorCode.MappingStoreProviderReturnsNullEdmType,
                            m_sourceLocation, navLineInfo, m_parsingErrors);
                        return null;
                    }
                }
                else
                {
                    cspaceTypeUsage = conditionMember.TypeUsage;
                }
                PrimitiveType memberType = ((PrimitiveType)cspaceTypeUsage.EdmType);
                Type clrMemberType = memberType.ClrEquivalentType;
                PrimitiveTypeKind primitiveTypeKind = memberType.PrimitiveTypeKind;
                //Only a subset of primitive types can be used in Conditions that are specified over values.
                //IsNull conditions can be specified on any primitive types
                if ((isNullAttribute == null) && !IsTypeSupportedForCondition(primitiveTypeKind))
                {
                    AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_InvalidContent_ConditionMapping_InvalidPrimitiveTypeKind,
                        conditionMember.Name, edmType.FullName, StorageMappingErrorCode.ConditionError,
                        m_sourceLocation, navLineInfo, m_parsingErrors);
                    return null;
                }
                Debug.Assert(clrMemberType != null, "Scalar Types should have associated clr type");
                //If the value is not compatible with the type, just add an error and return
                if(!StorageMappingItemLoader.TryGetTypedAttributeValue(nav.Clone(), StorageMslConstructs.ConditionValueAttribute, clrMemberType, m_sourceLocation, m_parsingErrors, out value))
                {
                    return null;
                }
            }
            else if (Helper.IsEnumType(edmType))
            {
                // Enumeration type - get the actual value
                value = StorageMappingItemLoader.GetEnumAttributeValue(nav.Clone(), StorageMslConstructs.ConditionValueAttribute, (EnumType)edmType, m_sourceLocation, m_parsingErrors);
            }
            else
            {
                // Since NullableComplexTypes are not being supported,
                // we don't allow conditions on complex types
                AddToSchemaErrors(Strings.Mapping_InvalidContent_ConditionMapping_NonScalar,
                    StorageMappingErrorCode.ConditionError, m_sourceLocation, navLineInfo, m_parsingErrors);
                return null;

            }
            //Either Value or NotNull need to be specifid on the condition mapping but not both
            if ((isNullAttribute != null) && (value != null))
            {
                AddToSchemaErrors(Strings.Mapping_InvalidContent_ConditionMapping_Both_Values,
                    StorageMappingErrorCode.ConditionError, m_sourceLocation, navLineInfo, m_parsingErrors);
                return null;
            }
            if ((isNullAttribute == null) && (value == null))
            {
                AddToSchemaErrors(Strings.Mapping_InvalidContent_ConditionMapping_Either_Values,
                    StorageMappingErrorCode.ConditionError, m_sourceLocation, navLineInfo, m_parsingErrors);
                return null;
            }

            if (isNullAttribute != null)
            {
                //XSD validation should have guarenteed that the attribute value can only be true or false
                Debug.Assert(isNullAttribute == "true" || isNullAttribute == "false");
                isNullValue = Convert.ToBoolean(isNullAttribute, System.Globalization.CultureInfo.InvariantCulture);
            }

            if (columnMember != null && (columnMember.IsStoreGeneratedComputed || columnMember.IsStoreGeneratedIdentity))
            {
                AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_ConditionMapping_Computed, columnMember.Name,
                    StorageMappingErrorCode.ConditionError, m_sourceLocation, navLineInfo, m_parsingErrors);
                return null;
            }

            StorageConditionPropertyMapping conditionPropertyMapping = new StorageConditionPropertyMapping(member, columnMember, value, isNullValue);
            return conditionPropertyMapping;
        }

        internal static bool IsTypeSupportedForCondition(PrimitiveTypeKind primitiveTypeKind)
        {
            switch (primitiveTypeKind)
            {
                case PrimitiveTypeKind.Boolean:
                case PrimitiveTypeKind.Byte:
                case PrimitiveTypeKind.Int16:
                case PrimitiveTypeKind.Int32:
                case PrimitiveTypeKind.Int64:
                case PrimitiveTypeKind.String:
                case PrimitiveTypeKind.SByte:
                    return true;
                case PrimitiveTypeKind.Binary:
                case PrimitiveTypeKind.DateTime:
                case PrimitiveTypeKind.Time:
                case PrimitiveTypeKind.DateTimeOffset:
                case PrimitiveTypeKind.Double:
                case PrimitiveTypeKind.Guid:
                case PrimitiveTypeKind.Single:
                case PrimitiveTypeKind.Decimal:
                    return false;
                default:
                    Debug.Fail("New primitive type kind added?");
                    return false;
            }
        }

        private static XmlSchemaSet GetOrCreateSchemaSet()
        {
            if (s_mappingXmlSchema == null)
            {
                //Get the xsd stream for CS MSL Xsd.
                XmlSchemaSet set = new XmlSchemaSet();
                AddResourceXsdToSchemaSet(set, StorageMslConstructs.ResourceXsdNameV1);
                AddResourceXsdToSchemaSet(set, StorageMslConstructs.ResourceXsdNameV2);
                AddResourceXsdToSchemaSet(set, StorageMslConstructs.ResourceXsdNameV3);
                System.Threading.Interlocked.CompareExchange(ref s_mappingXmlSchema, set, null);
            }

            return s_mappingXmlSchema;
        }

        private static void AddResourceXsdToSchemaSet(XmlSchemaSet set, string resourceName)
        {
            using (XmlReader xsdReader = System.Data.Common.DbProviderServices.GetXmlResource(resourceName))
            {
                XmlSchema xmlSchema = XmlSchema.Read(xsdReader, null);
                set.Add(xmlSchema);
            }
        }

        /// <summary>
        /// Throws a new MappingException giving out the line number and 
        /// File Name where the error in Mapping specification is present.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="errorCode"></param>
        /// <param name="uri"></param>
        /// <param name="lineInfo"></param>
        /// <param name="parsingErrors">Error Collection where the parsing errors are collected</param>
        private static void AddToSchemaErrors(string message, StorageMappingErrorCode errorCode, string location, IXmlLineInfo lineInfo, IList<EdmSchemaError> parsingErrors)
        {
            EdmSchemaError error = new EdmSchemaError(message, (int)errorCode, EdmSchemaErrorSeverity.Error, location, lineInfo.LineNumber, lineInfo.LinePosition);
            parsingErrors.Add(error);
        }

        private static EdmSchemaError AddToSchemaErrorsWithMemberInfo(Func<object, string> messageFormat, string errorMember, StorageMappingErrorCode errorCode, string location, IXmlLineInfo lineInfo, IList<EdmSchemaError> parsingErrors)
        {
            EdmSchemaError error = new EdmSchemaError(messageFormat(errorMember), (int)errorCode, EdmSchemaErrorSeverity.Error, location, lineInfo.LineNumber, lineInfo.LinePosition);
            parsingErrors.Add(error);
            return error;
        }

        private static void AddToSchemaErrorWithMemberAndStructure(Func<object, object, string> messageFormat, string errorMember,
            string errorStructure, StorageMappingErrorCode errorCode, string location, IXmlLineInfo lineInfo, IList<EdmSchemaError> parsingErrors)
        {
            EdmSchemaError error = new EdmSchemaError(
                messageFormat(errorMember, errorStructure)
                , (int)errorCode, EdmSchemaErrorSeverity.Error, location, lineInfo.LineNumber, lineInfo.LinePosition);
            parsingErrors.Add(error);
        }

        private static void AddToSchemaErrorWithMessage(string errorMessage, StorageMappingErrorCode errorCode, string location, IXmlLineInfo lineInfo, IList<EdmSchemaError> parsingErrors)
        {
            EdmSchemaError error = new EdmSchemaError(errorMessage, (int)errorCode, EdmSchemaErrorSeverity.Error, location, lineInfo.LineNumber, lineInfo.LinePosition);
            parsingErrors.Add(error);
        }

        /// <summary>
        /// Resolve the attribute value based on the aliases provided as part of MSL file.
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        private string GetAliasResolvedAttributeValue(XPathNavigator nav, string attributeName)
        {
            return GetAliasResolvedValue(StorageMappingItemLoader.GetAttributeValue(nav, attributeName));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        private bool GetBoolAttributeValue(XPathNavigator nav, string attributeName, bool defaultValue)
        {
            bool boolValue = defaultValue;
            object boolObj = Helper.GetTypedAttributeValue(nav, attributeName, typeof(bool));

            if (boolObj != null)
            {
                boolValue = (bool)boolObj;
            }
            return boolValue;
        }


        /// <summary>
        /// The method simply calls the helper method on Helper class with the 
        /// namespaceURI that is default for CSMapping.
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        private static string GetAttributeValue(XPathNavigator nav, string attributeName)
        {
            return Helper.GetAttributeValue(nav, attributeName);
        }

        /// <summary>
        /// The method simply calls the helper method on Helper class with the 
        /// namespaceURI that is default for CSMapping.
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="attributeName"></param>
        /// <param name="clrType"></param>
        /// <param name="uri"></param>
        /// <param name="parsingErrors">Error Collection where the parsing errors are collected</param>
        /// <returns></returns>
        private static bool TryGetTypedAttributeValue(XPathNavigator nav, string attributeName, Type clrType, string sourceLocation, IList<EdmSchemaError> parsingErrors, out object value)
        {
            value = null;
            try
            {
                value = Helper.GetTypedAttributeValue(nav, attributeName, clrType);
            }
            catch (FormatException)
            {
                StorageMappingItemLoader.AddToSchemaErrors(Strings.Mapping_ConditionValueTypeMismatch,
                    StorageMappingErrorCode.ConditionError, sourceLocation, (IXmlLineInfo)nav, parsingErrors);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns the enum EdmMember corresponding to attribute name in enumType.
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="attributeName"></param>
        /// <param name="enumType"></param>
        /// <param name="uri"></param>
        /// <param name="parsingErrors">Error Collection where the parsing errors are collected</param>
        /// <returns></returns>
        private static EnumMember GetEnumAttributeValue(XPathNavigator nav, string attributeName, EnumType enumType, string sourceLocation, IList<EdmSchemaError> parsingErrors)
        {
            IXmlLineInfo xmlLineInfoNav = (IXmlLineInfo)nav;

            string value = GetAttributeValue(nav, attributeName);
            if (String.IsNullOrEmpty(value))
            {
                StorageMappingItemLoader.AddToSchemaErrorsWithMemberInfo(Strings.Mapping_Enum_EmptyValue, enumType.FullName,
                    StorageMappingErrorCode.InvalidEnumValue, sourceLocation, xmlLineInfoNav, parsingErrors);
            }

            EnumMember result;
            bool found = enumType.Members.TryGetValue(value, false, out result);
            if (!found)
            {
                StorageMappingItemLoader.AddToSchemaErrorsWithMemberInfo(Strings.Mapping_Enum_InvalidValue, value,
                    StorageMappingErrorCode.InvalidEnumValue, sourceLocation, xmlLineInfoNav, parsingErrors);
            }
            return result;
        }

        /// <summary>
        /// Resolve the string value based on the aliases provided as part of MSL file.
        /// </summary>
        /// <param name="aliasedString"></param>
        /// <returns></returns>
        private string GetAliasResolvedValue(string aliasedString)
        {
            if ((aliasedString == null) || (aliasedString.Length == 0))
                return aliasedString;
            //For now all attributes have no namespace
            int aliasIndex = aliasedString.LastIndexOf('.');
            //If no '.' in the string, than obviously the string is not aliased
            if (aliasIndex == -1)
                return aliasedString;
            string aliasKey = aliasedString.Substring(0, aliasIndex);
            string aliasValue;
            m_alias.TryGetValue(aliasKey, out aliasValue);
            if (aliasValue != null)
            {
                aliasedString = aliasValue + aliasedString.Substring(aliasIndex);
            }
            return aliasedString;
        }

        /// <summary>
        /// Creates Xml Reader with settings required for
        /// XSD validation.
        /// </summary>
        /// <param name="innerReader"></param>
        private XmlReader GetSchemaValidatingReader(XmlReader innerReader)
        {
            //Create the reader setting that will be used while
            //loading the MSL.
            XmlReaderSettings readerSettings = GetXmlReaderSettings();
            XmlReader reader = XmlReader.Create(innerReader, readerSettings);

            return reader;
        }


        private XmlReaderSettings GetXmlReaderSettings()
        {
            XmlReaderSettings readerSettings = System.Data.EntityModel.SchemaObjectModel.Schema.CreateEdmStandardXmlReaderSettings();

            readerSettings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
            readerSettings.ValidationEventHandler += this.XsdValidationCallBack;
            readerSettings.ValidationType = ValidationType.Schema;
            readerSettings.Schemas = GetOrCreateSchemaSet();
            return readerSettings;
        }


        /// <summary>
        /// The method is called by the XSD validation event handler when
        /// ever there are warnings or errors.
        /// We ignore the warnings but the errors will result in exception.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void XsdValidationCallBack(object sender, ValidationEventArgs args)
        {
            if (args.Severity != XmlSeverityType.Warning)
            {
                string sourceLocation = null;
                if (!string.IsNullOrEmpty(args.Exception.SourceUri))
                {
                    sourceLocation = Helper.GetFileNameFromUri(new Uri(args.Exception.SourceUri));
                }
                EdmSchemaErrorSeverity severity = EdmSchemaErrorSeverity.Error;
                if (args.Severity == XmlSeverityType.Warning)
                    severity = EdmSchemaErrorSeverity.Warning;
                EdmSchemaError error = new EdmSchemaError(Strings.Mapping_InvalidMappingSchema_validation(args.Exception.Message)
                    , (int)StorageMappingErrorCode.XmlSchemaValidationError, severity, sourceLocation, args.Exception.LineNumber, args.Exception.LinePosition);
                m_parsingErrors.Add(error);
            }
        }


        /// <summary>
        /// Validate the scalar property mapping - makes sure that the cspace type is promotable to the store side and updates
        /// the store type usage
        /// </summary>
        /// <param name="member"></param>
        /// <param name="columnMember"></param>
        /// <param name="lineInfo"></param>
        private void ValidateAndUpdateScalarMemberMapping(EdmProperty member, EdmProperty columnMember, IXmlLineInfo lineInfo)
        {
            Debug.Assert(
                Helper.IsScalarType(member.TypeUsage.EdmType), 
                "c-space member type must be of primitive or enumeration type");
            Debug.Assert(Helper.IsPrimitiveType(columnMember.TypeUsage.EdmType), "s-space column type must be primitive");

            KeyValuePair<TypeUsage, TypeUsage> memberMappingInfo;
            if (!m_scalarMemberMappings.TryGetValue(member, out memberMappingInfo))
            {
                int errorCount = m_parsingErrors.Count;

                // Validates that the CSpace member type is promotable to the SSpace member types and returns a typeUsage which contains
                // the store equivalent type for the CSpace member type.
                // For e.g. If a CSpace member of type Edm.Int32 maps to SqlServer.Int64, the return type usage will contain SqlServer.int
                //          which is store equivalent type for Edm.Int32
                TypeUsage storeEquivalentTypeUsage = Helper.ValidateAndConvertTypeUsage(member,
                    columnMember, lineInfo, m_sourceLocation, m_parsingErrors, StoreItemCollection);

                // If the cspace type is not compatible with the store type, add a schema error and return
                if (storeEquivalentTypeUsage == null)
                {
                    if (errorCount == m_parsingErrors.Count)
                    {
                        EdmSchemaError error = new EdmSchemaError(
                            GetInvalidMemberMappingErrorMessage(member, columnMember),
                            (int)StorageMappingErrorCode.IncompatibleMemberMapping, EdmSchemaErrorSeverity.Error,
                            m_sourceLocation, lineInfo.LineNumber,
                            lineInfo.LinePosition);
                        m_parsingErrors.Add(error);
                    }
                }
                else
                {
                    m_scalarMemberMappings.Add(member, new KeyValuePair<TypeUsage, TypeUsage>(storeEquivalentTypeUsage, columnMember.TypeUsage));
                }
            }
            else
            {
                // Get the store member type to which the cspace member was mapped to previously
                TypeUsage storeMappedTypeUsage = memberMappingInfo.Value;
                TypeUsage modelColumnMember = columnMember.TypeUsage.GetModelTypeUsage();
                if (!Object.ReferenceEquals(columnMember.TypeUsage.EdmType, storeMappedTypeUsage.EdmType))
                {
                    EdmSchemaError error = new EdmSchemaError(
                        Strings.Mapping_StoreTypeMismatch_ScalarPropertyMapping(
                                                             member.Name,
                                                             storeMappedTypeUsage.EdmType.Name),
                        (int)StorageMappingErrorCode.CSpaceMemberMappedToMultipleSSpaceMemberWithDifferentTypes,
                        EdmSchemaErrorSeverity.Error,
                        m_sourceLocation,
                        lineInfo.LineNumber,
                        lineInfo.LinePosition);
                    m_parsingErrors.Add(error);
                }
                // Check if the cspace facets are promotable to the new store type facets
                else if (!TypeSemantics.IsSubTypeOf(ResolveTypeUsageForEnums(member.TypeUsage), modelColumnMember))
                {
                    EdmSchemaError error = new EdmSchemaError(
                        GetInvalidMemberMappingErrorMessage(member, columnMember),
                        (int)StorageMappingErrorCode.IncompatibleMemberMapping, EdmSchemaErrorSeverity.Error,
                        m_sourceLocation, lineInfo.LineNumber,
                        lineInfo.LinePosition);
                    m_parsingErrors.Add(error);
                }
            }
        }

        private string GetInvalidMemberMappingErrorMessage(EdmMember cSpaceMember, EdmMember sSpaceMember)
        {
            return Strings.Mapping_Invalid_Member_Mapping(
                cSpaceMember.TypeUsage.EdmType + GetFacetsForDisplay(cSpaceMember.TypeUsage),
                cSpaceMember.Name,
                cSpaceMember.DeclaringType.FullName,
                sSpaceMember.TypeUsage.EdmType + GetFacetsForDisplay(sSpaceMember.TypeUsage),
                sSpaceMember.Name,
                sSpaceMember.DeclaringType.FullName);
        }

        private string GetFacetsForDisplay(TypeUsage typeUsage)
        {
            Debug.Assert(typeUsage != null);

            ReadOnlyMetadataCollection<Facet> facets = typeUsage.Facets;
            if (facets == null || facets.Count == 0)
            {
                return string.Empty;
            }

            int numFacets = facets.Count;

            StringBuilder facetDisplay = new StringBuilder("[");

            for (int i = 0; i < numFacets-1; ++i)
            {
                facetDisplay.AppendFormat("{0}={1},", facets[i].Name, facets[i].Value ?? string.Empty);
            }

            facetDisplay.AppendFormat("{0}={1}]", facets[numFacets - 1].Name, facets[numFacets-1].Value ?? string.Empty);

            return facetDisplay.ToString();
        }

        #endregion

        #region Nested types
        /// <summary>
        /// Encapsulates state and functionality for loading a modification function mapping.
        /// </summary>
        private class ModificationFunctionMappingLoader
        {
            // Storage mapping loader
            private readonly StorageMappingItemLoader m_parentLoader;

            // Mapped function
            private EdmFunction m_function;

            // Entity set mapped by this function (may be null)
            private readonly EntitySet m_entitySet;

            // Association set mapped by this function (may be null)
            private readonly AssociationSet m_associationSet;

            // Model entity container (used to resolve set names)
            private readonly EntityContainer m_modelContainer;

            // Item collection (used to resolve function and type names)
            private readonly EdmItemCollection m_edmItemCollection;

            // Item collection (used to resolve function and type names)
            private readonly StoreItemCollection m_storeItemCollection;

            // Indicates whether the function can be bound to "current"
            // versions of properties (i.e., inserts and updates)
            private bool m_allowCurrentVersion;

            // Indicates whether the function can be bound to "original"
            // versions of properties (i.e., deletes and updates)
            private bool m_allowOriginalVersion;

            // Tracks which function parameters have been seen so far.
            private readonly Set<FunctionParameter> m_seenParameters;

            // Tracks members navigated to arrive at the current element
            private readonly Stack<EdmMember> m_members;

            // When set, indicates we are interpreting a navigation property on the given set.
            private AssociationSet m_associationSetNavigation;

            // Initialize loader
            internal ModificationFunctionMappingLoader(
                StorageMappingItemLoader parentLoader,
                EntitySetBase extent)
            {
                m_parentLoader = EntityUtil.CheckArgumentNull(parentLoader, "parentLoader");
                // initialize member fields
                m_modelContainer = EntityUtil.CheckArgumentNull<EntitySetBase>(extent, "extent").EntityContainer;
                m_edmItemCollection = parentLoader.EdmItemCollection;
                m_storeItemCollection = parentLoader.StoreItemCollection;
                m_entitySet = extent as EntitySet;
                if (null == m_entitySet)
                {
                    // do a cast here since the extent must either be an entity set
                    // or an association set
                    m_associationSet = (AssociationSet)extent;
                }
                m_seenParameters = new Set<FunctionParameter>();
                m_members = new Stack<EdmMember>();
            }

            internal StorageModificationFunctionMapping LoadEntityTypeModificationFunctionMapping(XPathNavigator nav, EntitySetBase entitySet, bool allowCurrentVersion, bool allowOriginalVersion, EntityType entityType)
            {
                FunctionParameter rowsAffectedParameter;
                m_function = LoadAndValidateFunctionMetadata(nav.Clone(), out rowsAffectedParameter);
                if (m_function == null)
                {
                    return null;
                }
                m_allowCurrentVersion = allowCurrentVersion;
                m_allowOriginalVersion = allowOriginalVersion;

                // Load all parameter bindings and result bindings
                IEnumerable<StorageModificationFunctionParameterBinding> parameters = LoadParameterBindings(nav.Clone(), entityType);
                IEnumerable<StorageModificationFunctionResultBinding> resultBindings = LoadResultBindings(nav.Clone(), entityType);

                StorageModificationFunctionMapping functionMapping = new StorageModificationFunctionMapping(entitySet, entityType, m_function, parameters, rowsAffectedParameter, resultBindings);

                return functionMapping;
            }


            // Loads a function mapping for an association set
            internal StorageModificationFunctionMapping LoadAssociationSetModificationFunctionMapping(XPathNavigator nav, EntitySetBase entitySet, bool isInsert)
            {
                FunctionParameter rowsAffectedParameter;
                m_function = LoadAndValidateFunctionMetadata(nav.Clone(), out rowsAffectedParameter);
                if (m_function == null)
                {
                    return null;
                }
                if (isInsert)
                {
                    m_allowCurrentVersion = true;
                    m_allowOriginalVersion = false;
                }
                else
                {
                    m_allowCurrentVersion = false;
                    m_allowOriginalVersion = true;
                }

                // Load all parameter bindings
                IEnumerable<StorageModificationFunctionParameterBinding> parameters = LoadParameterBindings(nav.Clone(), m_associationSet.ElementType);

                StorageModificationFunctionMapping mapping = new StorageModificationFunctionMapping(entitySet, entitySet.ElementType, m_function, parameters, rowsAffectedParameter, null);
                return mapping;
            }

            // Loads all result bindings.
            private IEnumerable<StorageModificationFunctionResultBinding> LoadResultBindings(XPathNavigator nav, EntityType entityType)
            {
                List<StorageModificationFunctionResultBinding> resultBindings = new List<StorageModificationFunctionResultBinding>();
                IXmlLineInfo xmlLineInfoNav = (IXmlLineInfo)nav;

                // walk through all children, filtering on result bindings
                if (nav.MoveToChild(XPathNodeType.Element))
                {
                    do
                    {
                        if (nav.LocalName == StorageMslConstructs.ResultBindingElement)
                        {
                            // retrieve attributes
                            string propertyName = m_parentLoader.GetAliasResolvedAttributeValue(nav.Clone(),
                                StorageMslConstructs.ResultBindingPropertyNameAttribute);
                            string columnName = m_parentLoader.GetAliasResolvedAttributeValue(nav.Clone(),
                                StorageMslConstructs.ScalarPropertyColumnNameAttribute);

                            // resolve metadata
                            EdmProperty property = null;
                            if (null == propertyName ||
                                !entityType.Properties.TryGetValue(propertyName, false, out property))
                            {
                                // add a schema error and return if the property does not exist
                                StorageMappingItemLoader.AddToSchemaErrorWithMemberAndStructure(
                                    Strings.Mapping_ModificationFunction_PropertyNotFound,
                                    propertyName, entityType.Name,
                                    StorageMappingErrorCode.InvalidEdmMember, m_parentLoader.m_sourceLocation,
                                    xmlLineInfoNav, m_parentLoader.m_parsingErrors);
                                return new List<StorageModificationFunctionResultBinding>();
                            }

                            // construct element binding (no type checking is required at mapping load time)
                            StorageModificationFunctionResultBinding resultBinding = new StorageModificationFunctionResultBinding(columnName, property);
                            resultBindings.Add(resultBinding);
                        }
                    } while (nav.MoveToNext(XPathNodeType.Element));
                }

                // check for duplicate mappings of single properties
                KeyToListMap<EdmProperty, string> propertyToColumnNamesMap = new KeyToListMap<EdmProperty, string>(EqualityComparer<EdmProperty>.Default);
                foreach (StorageModificationFunctionResultBinding resultBinding in resultBindings)
                {
                    propertyToColumnNamesMap.Add(resultBinding.Property, resultBinding.ColumnName);
                }
                foreach (EdmProperty property in propertyToColumnNamesMap.Keys)
                {
                    ReadOnlyCollection<string> columnNames = propertyToColumnNamesMap.ListForKey(property);
                    if (1 < columnNames.Count)
                    {
                        StorageMappingItemLoader.AddToSchemaErrorWithMemberAndStructure(
                            Strings.Mapping_ModificationFunction_AmbiguousResultBinding,
                            property.Name, StringUtil.ToCommaSeparatedString(columnNames),
                            StorageMappingErrorCode.AmbiguousResultBindingInModificationFunctionMapping,
                            m_parentLoader.m_sourceLocation, xmlLineInfoNav,
                            m_parentLoader.m_parsingErrors);
                        return new List<StorageModificationFunctionResultBinding>();
                    }
                }

                return resultBindings;
            }

            // Loads parameter bindings from the given node, validating bindings:
            // - All parameters are covered
            // - Referenced names exist in type
            // - Parameter and scalar type are compatible
            // - Legal versions are given
            private IEnumerable<StorageModificationFunctionParameterBinding> LoadParameterBindings(XPathNavigator nav, StructuralType type)
            {
                // recursively retrieve bindings (current member path is empty)
                // immediately construct a list of bindings to force execution of the LoadParameterBindings
                // yield method
                List<StorageModificationFunctionParameterBinding> parameterBindings = new List<StorageModificationFunctionParameterBinding>(
                    LoadParameterBindings(nav.Clone(), type, restrictToKeyMembers: false));

                // check that all parameters have been mapped
                Set<FunctionParameter> unmappedParameters = new Set<FunctionParameter>(m_function.Parameters);
                unmappedParameters.Subtract(m_seenParameters);
                if (0 != unmappedParameters.Count)
                {
                    AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_ModificationFunction_MissingParameter,
                        m_function.FullName, StringUtil.ToCommaSeparatedString(unmappedParameters),
                        StorageMappingErrorCode.InvalidParameterInModificationFunctionMapping,
                        m_parentLoader.m_sourceLocation, (IXmlLineInfo)nav,
                        m_parentLoader.m_parsingErrors);
                    return new List<StorageModificationFunctionParameterBinding>();
                }

                return parameterBindings;
            }

            private IEnumerable<StorageModificationFunctionParameterBinding> LoadParameterBindings(XPathNavigator nav, StructuralType type,
                bool restrictToKeyMembers)
            {
                // walk through all child bindings
                if (nav.MoveToChild(XPathNodeType.Element))
                {
                    do
                    {
                        switch (nav.LocalName)
                        {
                            case StorageMslConstructs.ScalarPropertyElement:
                                {
                                    StorageModificationFunctionParameterBinding binding = LoadScalarPropertyParameterBinding(
                                        nav.Clone(), type, restrictToKeyMembers);
                                    if (binding != null)
                                    {
                                        yield return binding;
                                    }
                                    else
                                    {
                                        yield break;
                                    }
                                }
                                break;
                            case StorageMslConstructs.ComplexPropertyElement:
                                {
                                    ComplexType complexType;
                                    EdmMember property = LoadComplexTypeProperty(
                                        nav.Clone(), type, out complexType);
                                    if (property != null)
                                    {

                                        // recursively retrieve mappings
                                        m_members.Push(property);
                                        foreach (StorageModificationFunctionParameterBinding binding in
                                            LoadParameterBindings(nav.Clone(), complexType, restrictToKeyMembers))
                                        {
                                            yield return binding;
                                        }
                                        m_members.Pop();
                                    }
                                }
                                break;
                            case StorageMslConstructs.AssociationEndElement:
                                {
                                    AssociationSetEnd toEnd = LoadAssociationEnd(nav.Clone());
                                    if (toEnd != null)
                                    {

                                        // translate the bindings for the association end
                                        m_members.Push(toEnd.CorrespondingAssociationEndMember);
                                        m_associationSetNavigation = toEnd.ParentAssociationSet;
                                        foreach (StorageModificationFunctionParameterBinding binding in
                                            LoadParameterBindings(nav.Clone(), toEnd.EntitySet.ElementType, true /* restrictToKeyMembers */))
                                        {
                                            yield return binding;
                                        }
                                        m_associationSetNavigation = null;
                                        m_members.Pop();
                                    }
                                }
                                break;
                            case StorageMslConstructs.EndPropertyMappingElement:
                                {
                                    AssociationSetEnd end = LoadEndProperty(nav.Clone());
                                    if (end != null)
                                    {

                                        // translate the bindings for the end property
                                        m_members.Push(end.CorrespondingAssociationEndMember);
                                        foreach (StorageModificationFunctionParameterBinding binding in
                                            LoadParameterBindings(nav.Clone(), end.EntitySet.ElementType, true /* restrictToKeyMembers */))
                                        {
                                            yield return binding;
                                        }
                                        m_members.Pop();
                                    }
                                }
                                break;
                        }
                    } while (nav.MoveToNext(XPathNodeType.Element));
                }
            }

            private AssociationSetEnd LoadAssociationEnd(XPathNavigator nav)
            {

                IXmlLineInfo xmlLineInfoNav = (IXmlLineInfo)nav;

                // retrieve element attributes
                string associationSetName = m_parentLoader.GetAliasResolvedAttributeValue(
                    nav.Clone(), StorageMslConstructs.AssociationSetAttribute);
                string fromRole = m_parentLoader.GetAliasResolvedAttributeValue(
                    nav.Clone(), StorageMslConstructs.FromAttribute);
                string toRole = m_parentLoader.GetAliasResolvedAttributeValue(
                    nav.Clone(), StorageMslConstructs.ToAttribute);

                // retrieve metadata
                RelationshipSet relationshipSet = null;
                AssociationSet associationSet;

                // validate the association set exists
                if (null == associationSetName ||
                    !m_modelContainer.TryGetRelationshipSetByName(associationSetName, false, out relationshipSet) ||
                    BuiltInTypeKind.AssociationSet != relationshipSet.BuiltInTypeKind)
                {
                    StorageMappingItemLoader.AddToSchemaErrorsWithMemberInfo(
                        Strings.Mapping_ModificationFunction_AssociationSetDoesNotExist,
                        associationSetName, StorageMappingErrorCode.InvalidAssociationSet,
                        m_parentLoader.m_sourceLocation, xmlLineInfoNav,
                        m_parentLoader.m_parsingErrors);
                    return null;
                }
                associationSet = (AssociationSet)relationshipSet;

                // validate the from end exists
                AssociationSetEnd fromEnd = null;
                if (null == fromRole ||
                    !associationSet.AssociationSetEnds.TryGetValue(fromRole, false, out fromEnd))
                {
                    StorageMappingItemLoader.AddToSchemaErrorsWithMemberInfo(
                        Strings.Mapping_ModificationFunction_AssociationSetRoleDoesNotExist,
                        fromRole, StorageMappingErrorCode.InvalidAssociationSetRoleInModificationFunctionMapping,
                        m_parentLoader.m_sourceLocation, xmlLineInfoNav, m_parentLoader.m_parsingErrors);
                    return null;
                }

                // validate the to end exists
                AssociationSetEnd toEnd = null;
                if (null == toRole ||
                    !associationSet.AssociationSetEnds.TryGetValue(toRole, false, out toEnd))
                {
                    StorageMappingItemLoader.AddToSchemaErrorsWithMemberInfo(
                    Strings.Mapping_ModificationFunction_AssociationSetRoleDoesNotExist,
                    toRole, StorageMappingErrorCode.InvalidAssociationSetRoleInModificationFunctionMapping,
                    m_parentLoader.m_sourceLocation, xmlLineInfoNav, m_parentLoader.m_parsingErrors);
                    return null;
                }

                // validate ends reference the current entity set
                if (!fromEnd.EntitySet.Equals(m_entitySet))
                {
                    StorageMappingItemLoader.AddToSchemaErrorsWithMemberInfo(
                        Strings.Mapping_ModificationFunction_AssociationSetFromRoleIsNotEntitySet,
                        fromRole, StorageMappingErrorCode.InvalidAssociationSetRoleInModificationFunctionMapping,
                        m_parentLoader.m_sourceLocation, xmlLineInfoNav, m_parentLoader.m_parsingErrors);
                    return null;
                }

                // validate cardinality of to end (can be at most one)
                if (toEnd.CorrespondingAssociationEndMember.RelationshipMultiplicity != RelationshipMultiplicity.One &&
                    toEnd.CorrespondingAssociationEndMember.RelationshipMultiplicity != RelationshipMultiplicity.ZeroOrOne)
                {
                    StorageMappingItemLoader.AddToSchemaErrorsWithMemberInfo(
                    Strings.Mapping_ModificationFunction_AssociationSetCardinality,
                    toRole, StorageMappingErrorCode.InvalidAssociationSetCardinalityInModificationFunctionMapping,
                    m_parentLoader.m_sourceLocation, xmlLineInfoNav, m_parentLoader.m_parsingErrors);
                    return null;
                }

                // if this is a FK, raise an error or a warning if the mapping would have been allowed in V1
                // (all dependent properties are part of the primary key)
                if (associationSet.ElementType.IsForeignKey)
                {
                    ReferentialConstraint constraint = associationSet.ElementType.ReferentialConstraints.Single();
                    EdmSchemaError error = StorageMappingItemLoader.AddToSchemaErrorsWithMemberInfo(
                        Strings.Mapping_ModificationFunction_AssociationEndMappingForeignKeyAssociation,
                        toRole, StorageMappingErrorCode.InvalidModificationFunctionMappingAssociationEndForeignKey, m_parentLoader.m_sourceLocation,
                        xmlLineInfoNav, m_parentLoader.m_parsingErrors);

                    if (fromEnd.CorrespondingAssociationEndMember == constraint.ToRole &&
                        constraint.ToProperties.All(p => m_entitySet.ElementType.KeyMembers.Contains(p)))
                    {
                        // Just a warning...
                        error.Severity = EdmSchemaErrorSeverity.Warning;
                    }
                    else
                    {
                        return null;
                    }
                }
                return toEnd;
            }

            private AssociationSetEnd LoadEndProperty(XPathNavigator nav)
            {
                // retrieve element attributes
                string role = m_parentLoader.GetAliasResolvedAttributeValue(
                    nav.Clone(), StorageMslConstructs.EndPropertyMappingNameAttribute);

                // validate the role exists
                AssociationSetEnd end = null;
                if (null == role ||
                    !m_associationSet.AssociationSetEnds.TryGetValue(role, false, out end))
                {
                    StorageMappingItemLoader.AddToSchemaErrorsWithMemberInfo(
                    Strings.Mapping_ModificationFunction_AssociationSetRoleDoesNotExist,
                    role, StorageMappingErrorCode.InvalidAssociationSetRoleInModificationFunctionMapping,
                    m_parentLoader.m_sourceLocation, (IXmlLineInfo)nav, m_parentLoader.m_parsingErrors);
                    return null;
                }

                return end;
            }

            private EdmMember LoadComplexTypeProperty(XPathNavigator nav, StructuralType type, out ComplexType complexType)
            {

                IXmlLineInfo xmlLineInfoNav = (IXmlLineInfo)nav;

                // retrieve element attributes
                string propertyName = m_parentLoader.GetAliasResolvedAttributeValue(
                    nav.Clone(), StorageMslConstructs.ComplexPropertyNameAttribute);
                string typeName = m_parentLoader.GetAliasResolvedAttributeValue(
                    nav.Clone(), StorageMslConstructs.ComplexTypeMappingTypeNameAttribute);

                // retrieve metadata
                EdmMember property = null;
                if (null == propertyName ||
                    !type.Members.TryGetValue(propertyName, false, out property))
                {
                    // raise exception if the property does not exist
                    StorageMappingItemLoader.AddToSchemaErrorWithMemberAndStructure(
                        Strings.Mapping_ModificationFunction_PropertyNotFound,
                        propertyName, type.Name, StorageMappingErrorCode.InvalidEdmMember,
                        m_parentLoader.m_sourceLocation, xmlLineInfoNav, m_parentLoader.m_parsingErrors);
                    complexType = null;
                    return null;
                }
                complexType = null;
                if (null == typeName ||
                    !m_edmItemCollection.TryGetItem<ComplexType>(typeName, out complexType))
                {
                    // raise exception if the type does not exist
                    StorageMappingItemLoader.AddToSchemaErrorsWithMemberInfo(
                    Strings.Mapping_ModificationFunction_ComplexTypeNotFound,
                    typeName, StorageMappingErrorCode.InvalidComplexType,
                    m_parentLoader.m_sourceLocation, xmlLineInfoNav
                    , m_parentLoader.m_parsingErrors);
                    return null;
                }
                if (!property.TypeUsage.EdmType.Equals(complexType) &&
                    !Helper.IsSubtypeOf(property.TypeUsage.EdmType, complexType))
                {
                    // raise exception if the complex type is incorrect
                    StorageMappingItemLoader.AddToSchemaErrorWithMemberAndStructure(
                        Strings.Mapping_ModificationFunction_WrongComplexType,
                        typeName, property.Name, StorageMappingErrorCode.InvalidComplexType,
                        m_parentLoader.m_sourceLocation, xmlLineInfoNav
                        , m_parentLoader.m_parsingErrors);
                    return null;
                }
                return property;
            }

            private StorageModificationFunctionParameterBinding LoadScalarPropertyParameterBinding(XPathNavigator nav, StructuralType type, bool restrictToKeyMembers)
            {
                IXmlLineInfo xmlLineInfoNav = (IXmlLineInfo)nav;

                // get attribute values
                string parameterName = m_parentLoader.GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.ParameterNameAttribute);
                string propertyName = m_parentLoader.GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.ScalarPropertyNameAttribute);
                string version = m_parentLoader.GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.ParameterVersionAttribute);

                // determine version
                bool isCurrent = false;
                if (null == version)
                {
                    // use default
                    if (!m_allowOriginalVersion)
                    {
                        isCurrent = true;
                    }
                    else if (!m_allowCurrentVersion)
                    {
                        isCurrent = false;
                    }
                    else
                    {
                        // add a schema error and return as there is no default
                        StorageMappingItemLoader.AddToSchemaErrors(
                            Strings.Mapping_ModificationFunction_MissingVersion,
                            StorageMappingErrorCode.MissingVersionInModificationFunctionMapping, m_parentLoader.m_sourceLocation,
                            xmlLineInfoNav, m_parentLoader.m_parsingErrors);
                        return null;

                    }
                }
                else
                {
                    // check the value given by the user
                    isCurrent = version == StorageMslConstructs.ParameterVersionAttributeCurrentValue;
                }
                if (isCurrent && !m_allowCurrentVersion)
                {
                    //Add a schema error and return  since the 'current' property version is not available
                    StorageMappingItemLoader.AddToSchemaErrors(
                        Strings.Mapping_ModificationFunction_VersionMustBeOriginal,
                        StorageMappingErrorCode.InvalidVersionInModificationFunctionMapping,
                        m_parentLoader.m_sourceLocation, xmlLineInfoNav
                        , m_parentLoader.m_parsingErrors);
                    return null;
                }
                if (!isCurrent && !m_allowOriginalVersion)
                {
                    // Add a schema error and return  since the 'original' property version is not available
                    StorageMappingItemLoader.AddToSchemaErrors(
                        Strings.Mapping_ModificationFunction_VersionMustBeCurrent,
                        StorageMappingErrorCode.InvalidVersionInModificationFunctionMapping,
                        m_parentLoader.m_sourceLocation, xmlLineInfoNav
                        , m_parentLoader.m_parsingErrors);
                    return null;
                }

                // retrieve metadata
                FunctionParameter parameter = null;
                if (null == parameterName ||
                    !m_function.Parameters.TryGetValue(parameterName, false, out parameter))
                {
                    //Add a schema error and return  if the parameter does not exist
                    StorageMappingItemLoader.AddToSchemaErrorWithMemberAndStructure(
                        Strings.Mapping_ModificationFunction_ParameterNotFound,
                        parameterName, m_function.Name,
                        StorageMappingErrorCode.InvalidParameterInModificationFunctionMapping,
                        m_parentLoader.m_sourceLocation, xmlLineInfoNav
                        , m_parentLoader.m_parsingErrors);
                    return null;
                }
                EdmMember property = null;
                if (restrictToKeyMembers)
                {
                    if (null == propertyName ||
                        !((EntityType)type).KeyMembers.TryGetValue(propertyName, false, out property))
                    {
                        // raise exception if the property does not exist
                        StorageMappingItemLoader.AddToSchemaErrorWithMemberAndStructure(
                            Strings.Mapping_ModificationFunction_PropertyNotKey,
                            propertyName, type.Name,
                            StorageMappingErrorCode.InvalidEdmMember,
                            m_parentLoader.m_sourceLocation, xmlLineInfoNav, m_parentLoader.m_parsingErrors);
                        return null;
                    }
                }
                else
                {
                    if (null == propertyName ||
                        !type.Members.TryGetValue(propertyName, false, out property))
                    {
                        // raise exception if the property does not exist
                        StorageMappingItemLoader.AddToSchemaErrorWithMemberAndStructure(
                            Strings.Mapping_ModificationFunction_PropertyNotFound,
                            propertyName, type.Name,
                            StorageMappingErrorCode.InvalidEdmMember,
                            m_parentLoader.m_sourceLocation, xmlLineInfoNav, m_parentLoader.m_parsingErrors);
                        return null;
                    }
                }

                // check that the parameter hasn't already been seen
                if (m_seenParameters.Contains(parameter))
                {
                    StorageMappingItemLoader.AddToSchemaErrorsWithMemberInfo(
                        Strings.Mapping_ModificationFunction_ParameterBoundTwice,
                        parameterName, StorageMappingErrorCode.ParameterBoundTwiceInModificationFunctionMapping,
                        m_parentLoader.m_sourceLocation, xmlLineInfoNav, m_parentLoader.m_parsingErrors);
                    return null;
                }

                int errorCount = m_parentLoader.m_parsingErrors.Count;

                TypeUsage mappedStoreType = Helper.ValidateAndConvertTypeUsage(property,
                                                                               xmlLineInfoNav,
                                                                               m_parentLoader.m_sourceLocation,
                                                                               property.TypeUsage,
                                                                               parameter.TypeUsage,
                                                                               m_parentLoader.m_parsingErrors,
                                                                               m_storeItemCollection);

                // validate type compatibility
                if (mappedStoreType == null && errorCount == m_parentLoader.m_parsingErrors.Count)
                {
                    AddToSchemaErrorWithMessage(
                        Strings.Mapping_ModificationFunction_PropertyParameterTypeMismatch(
                                                             property.TypeUsage.EdmType,
                                                             property.Name,
                                                             property.DeclaringType.FullName,
                                                             parameter.TypeUsage.EdmType,
                                                             parameter.Name,
                                                             m_function.FullName),
                        StorageMappingErrorCode.InvalidModificationFunctionMappingPropertyParameterTypeMismatch,
                        m_parentLoader.m_sourceLocation,
                        xmlLineInfoNav,
                        m_parentLoader.m_parsingErrors);
                }

                // create the binding object
                m_members.Push(property);

                // if the member path includes a FK relationship, remap to the corresponding FK property
                IEnumerable<EdmMember> members = m_members;
                AssociationSet associationSetNavigation = m_associationSetNavigation;
                if (m_members.Last().BuiltInTypeKind == BuiltInTypeKind.AssociationEndMember)
                {
                    AssociationEndMember targetEnd = (AssociationEndMember)m_members.Last();
                    AssociationType associationType = (AssociationType)targetEnd.DeclaringType;
                    if (associationType.IsForeignKey)
                    {
                        ReferentialConstraint constraint = associationType.ReferentialConstraints.Single();
                        if (constraint.FromRole == targetEnd)
                        {
                            int ordinal = constraint.FromProperties.IndexOf((EdmProperty)m_members.First());

                            // rebind to the foreign key (no longer an association set navigation)
                            members = new EdmMember[] { constraint.ToProperties[ordinal], };
                            associationSetNavigation = null;
                        }
                    }
                }
                StorageModificationFunctionParameterBinding binding = new StorageModificationFunctionParameterBinding(parameter, new StorageModificationFunctionMemberPath(
                    members, associationSetNavigation), isCurrent);
                m_members.Pop();

                // remember that we've seen a binding for this parameter
                m_seenParameters.Add(parameter);

                return binding;
            }

            /// <summary>
            /// Loads function metadata and ensures the function is supportable for function mapping.
            /// </summary>
            private EdmFunction LoadAndValidateFunctionMetadata(XPathNavigator nav, out FunctionParameter rowsAffectedParameter)
            {
                IXmlLineInfo xmlLineInfoNav = (IXmlLineInfo)nav;

                // Different operations may be mapped to the same function (e.g. both INSERT and UPDATE are handled by a single
                // UPSERT function). Between loading functions, we can clear the set of seen parameters, because we may see them
                // again and don't want to claim there's a collision in such cases.
                m_seenParameters.Clear();

                // retrieve function attributes from the current element
                string functionName = m_parentLoader.GetAliasResolvedAttributeValue(nav.Clone(), StorageMslConstructs.FunctionNameAttribute);
                rowsAffectedParameter = null;

                // find function metadata
                System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> functionOverloads =
                    m_storeItemCollection.GetFunctions(functionName);

                if (functionOverloads.Count == 0)
                {
                    AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_UnknownFunction, functionName,
                        StorageMappingErrorCode.InvalidModificationFunctionMappingUnknownFunction, m_parentLoader.m_sourceLocation,
                        xmlLineInfoNav, m_parentLoader.m_parsingErrors);
                    return null;
                }

                if (1 < functionOverloads.Count)
                {
                    AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_AmbiguousFunction, functionName,
                        StorageMappingErrorCode.InvalidModificationFunctionMappingAmbiguousFunction, m_parentLoader.m_sourceLocation,
                        xmlLineInfoNav, m_parentLoader.m_parsingErrors);
                    return null;
                }

                EdmFunction function = functionOverloads[0];

                // check function is legal for function mapping
                if (MetadataHelper.IsComposable(function))
                { // only non-composable functions are permitted
                    AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_NotValidFunction, functionName,
                        StorageMappingErrorCode.InvalidModificationFunctionMappingNotValidFunction, m_parentLoader.m_sourceLocation,
                        xmlLineInfoNav, m_parentLoader.m_parsingErrors);
                    return null;
                }

                // check for parameter
                string rowsAffectedParameterName = GetAttributeValue(nav, StorageMslConstructs.RowsAffectedParameterAttribute);
                if (!string.IsNullOrEmpty(rowsAffectedParameterName))
                {
                    // check that the parameter exists
                    if (!function.Parameters.TryGetValue(rowsAffectedParameterName, false, out rowsAffectedParameter))
                    {
                        AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_RowsAffectedParameterDoesNotExist(
                            rowsAffectedParameterName, function.FullName),
                            StorageMappingErrorCode.MappingFunctionImportRowsAffectedParameterDoesNotExist,
                            m_parentLoader.m_sourceLocation, xmlLineInfoNav, m_parentLoader.m_parsingErrors);
                        return null;
                    }
                    // check that the parameter is an out parameter
                    if (ParameterMode.Out != rowsAffectedParameter.Mode && ParameterMode.InOut != rowsAffectedParameter.Mode)
                    {
                        AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_RowsAffectedParameterHasWrongMode(
                            rowsAffectedParameterName, rowsAffectedParameter.Mode, ParameterMode.Out, ParameterMode.InOut),
                            StorageMappingErrorCode.MappingFunctionImportRowsAffectedParameterHasWrongMode,
                            m_parentLoader.m_sourceLocation, xmlLineInfoNav, m_parentLoader.m_parsingErrors);
                        return null;
                    }
                    // check that the parameter type is an integer type
                    PrimitiveType rowsAffectedParameterType = (PrimitiveType)rowsAffectedParameter.TypeUsage.EdmType;

                    if (!TypeSemantics.IsIntegerNumericType(rowsAffectedParameter.TypeUsage))
                    {
                        AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_RowsAffectedParameterHasWrongType(
                            rowsAffectedParameterName, rowsAffectedParameterType.PrimitiveTypeKind),
                            StorageMappingErrorCode.MappingFunctionImportRowsAffectedParameterHasWrongType,
                            m_parentLoader.m_sourceLocation, xmlLineInfoNav, m_parentLoader.m_parsingErrors);
                        return null;
                    }
                    m_seenParameters.Add(rowsAffectedParameter);
                }

                // check that all parameters are allowed
                foreach (FunctionParameter parameter in function.Parameters)
                {
                    if (ParameterMode.In != parameter.Mode && rowsAffectedParameterName != parameter.Name)
                    { // rows affected is 'out' not 'in'
                        AddToSchemaErrorWithMessage(Strings.Mapping_ModificationFunction_NotValidFunctionParameter(functionName,
                            parameter.Name, StorageMslConstructs.RowsAffectedParameterAttribute), StorageMappingErrorCode.InvalidModificationFunctionMappingNotValidFunctionParameter,
                            m_parentLoader.m_sourceLocation, xmlLineInfoNav, m_parentLoader.m_parsingErrors);
                        return null;
                    }
                }

                return function;
            }
        }
        #endregion

        /// <summary>
        /// Checks whether the <paramref name="typeUsage"/> represents a type usage for an enumeration type and if
        /// this is the case creates a new type usage built using the underlying type of the enumeration type.
        /// </summary>
        /// <param name="typeUsage">TypeUsage to resolve.</param>
        /// <returns>
        /// If <paramref name="typeUsage"/> represents a TypeUsage for enumeration type the method returns a new
        /// TypeUsage instance created using the underlying type of the enumeration type. Otherwise the method 
        /// returns <paramref name="typeUsage"/>.
        /// </returns>
        private static TypeUsage ResolveTypeUsageForEnums(TypeUsage typeUsage)
        {
            Debug.Assert(typeUsage != null, "typeUsage != null");

            return Helper.IsEnumType(typeUsage.EdmType) ?
                TypeUsage.Create(Helper.GetUnderlyingEdmTypeForEnumType(typeUsage.EdmType), typeUsage.Facets) :
                typeUsage;
        }
    }
}
