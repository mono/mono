//---------------------------------------------------------------------
// <copyright file="EntityContainer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common.Utils;
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Xml;

    /// <summary>
    /// Represents an EntityContainer element.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Name={Name}")]
    internal sealed class EntityContainer : SchemaType
    {
        #region Instance Fields

        private SchemaElementLookUpTable<SchemaElement> _members;
        private ISchemaElementLookUpTable<EntityContainerEntitySet> _entitySets;
        private ISchemaElementLookUpTable<EntityContainerRelationshipSet> _relationshipSets;
        private ISchemaElementLookUpTable<Function> _functionImports;
        private string _unresolvedExtendedEntityContainerName;
        private EntityContainer _entityContainerGettingExtended;
        private bool _isAlreadyValidated;
        private bool _isAlreadyResolved;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an EntityContainer
        /// </summary>
        /// <param name="parentElement">Reference to the schema element.</param>
        public EntityContainer(Schema parentElement)
            : base(parentElement)
        {
            if(Schema.DataModel == SchemaDataModelOption.EntityDataModel)
                OtherContent.Add(Schema.SchemaSource);
        }

        #endregion

        #region Properties, Methods, Events & Delegates

        /// <summary>
        /// 
        /// </summary>
        SchemaElementLookUpTable<SchemaElement> Members
        {
            get
            {
                if (_members == null)
                {
                    _members = new SchemaElementLookUpTable<SchemaElement>();
                }
                return _members;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ISchemaElementLookUpTable<EntityContainerEntitySet> EntitySets
        {
            get
            {
                if (_entitySets == null)
                {
                    _entitySets = new FilteredSchemaElementLookUpTable<EntityContainerEntitySet, SchemaElement>(Members);
                }
                return _entitySets;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ISchemaElementLookUpTable<EntityContainerRelationshipSet> RelationshipSets
        {
            get
            {
                if (_relationshipSets == null)
                {
                    _relationshipSets = new FilteredSchemaElementLookUpTable<EntityContainerRelationshipSet, SchemaElement>(Members);
                }
                return _relationshipSets;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ISchemaElementLookUpTable<Function> FunctionImports
        {
            get
            {
                if (_functionImports == null)
                {
                    _functionImports = new FilteredSchemaElementLookUpTable<Function, SchemaElement>(Members);
                }
                return _functionImports;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public EntityContainer ExtendingEntityContainer
        {
            get
            {
                return _entityContainerGettingExtended;
            }
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.Extends))
            {
                HandleExtendsAttribute(reader);
                return true;
            }

            return false;
        }

        protected override bool HandleElement(XmlReader reader)
        {
            if (base.HandleElement(reader))
            {
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.EntitySet))
            {
                HandleEntitySetElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.AssociationSet))
            {
                HandleAssociationSetElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.FunctionImport))
            {
                HandleFunctionImport(reader);
                return true;
            }
            else if (Schema.DataModel == SchemaDataModelOption.EntityDataModel)
            {
                if (CanHandleElement(reader, XmlConstants.ValueAnnotation))
                {
                    // EF does not support this EDM 3.0 element, so ignore it.
                    SkipElement(reader);
                    return true;
                }
                else if (CanHandleElement(reader, XmlConstants.TypeAnnotation))
                {
                    // EF does not support this EDM 3.0 element, so ignore it.
                    SkipElement(reader);
                    return true;
                }
            }

            return false;
        }

        private void HandleEntitySetElement(XmlReader reader)
        {
            Debug.Assert(reader != null);
            EntityContainerEntitySet set = new EntityContainerEntitySet(this);
            set.Parse(reader);
            Members.Add(set, true, Strings.DuplicateEntityContainerMemberName);
        }

        private void HandleAssociationSetElement(XmlReader reader)
        {
            Debug.Assert(reader != null);
            EntityContainerAssociationSet set = new EntityContainerAssociationSet(this);
            set.Parse(reader);
            Members.Add(set, true, Strings.DuplicateEntityContainerMemberName);
        }

        private void HandleFunctionImport(XmlReader reader)
        {
            Debug.Assert(null != reader);
            FunctionImportElement functionImport = new FunctionImportElement(this);
            functionImport.Parse(reader);
            Members.Add(functionImport, true, Strings.DuplicateEntityContainerMemberName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        private void HandleExtendsAttribute(XmlReader reader)
        {
            _unresolvedExtendedEntityContainerName = HandleUndottedNameAttribute(reader, _unresolvedExtendedEntityContainerName);
        }

        /// <summary>
        /// Resolves the names to element references.
        /// </summary>
        internal override void ResolveTopLevelNames()
        {
            if (!_isAlreadyResolved)
            {
                base.ResolveTopLevelNames();

                SchemaType extendingEntityContainer;
                // If this entity container extends some other entity container, we should validate the entity container name.
                if (!String.IsNullOrEmpty(_unresolvedExtendedEntityContainerName))
                {
                    if (_unresolvedExtendedEntityContainerName == this.Name)
                    {
                        AddError(ErrorCode.EntityContainerCannotExtendItself, EdmSchemaErrorSeverity.Error,
                            System.Data.Entity.Strings.EntityContainerCannotExtendItself(this.Name));
                    }
                    else if (!Schema.SchemaManager.TryResolveType(null, _unresolvedExtendedEntityContainerName, out extendingEntityContainer))
                    {
                        AddError(ErrorCode.InvalidEntityContainerNameInExtends, EdmSchemaErrorSeverity.Error,
                                System.Data.Entity.Strings.InvalidEntityContainerNameInExtends(_unresolvedExtendedEntityContainerName));
                    }
                    else
                    {
                        _entityContainerGettingExtended = (EntityContainer)extendingEntityContainer;

                        // Once you have successfully resolved the entity container, then you should call ResolveNames on the
                        // extending entity containers as well. This is because we will need to look up the chain for resolving
                        // entity set names, since there might be association sets/ function imports that refer to entity sets
                        // belonging in extended entity containers
                        _entityContainerGettingExtended.ResolveTopLevelNames();
                    }
                }

                foreach (SchemaElement element in Members)
                {
                    element.ResolveTopLevelNames();
                }

                _isAlreadyResolved = true;
            }
        }

        internal override void ResolveSecondLevelNames()
        {
            base.ResolveSecondLevelNames();

            foreach (SchemaElement element in Members)
            {
                element.ResolveSecondLevelNames();
            }
        }

        /// <summary>
        /// Do all validation for this element here, and delegate to all sub elements
        /// </summary>
        internal override void Validate()
        {
            // Now before we clone all the entity sets from the entity container that this entity container is extending,
            // we need to make sure that the entity container that is getting extended is already validated. since it might
            // be extending some other entity container, and we might want to populate this entity container, before
            // it gets extended
            if (!_isAlreadyValidated)
            {
                base.Validate();

                // If this entity container extends some other entity container, then we should add all the 
                // sets and function imports from that entity container to this entity container
                if (this.ExtendingEntityContainer != null)
                {
                    // Call Validate on the entity container that is getting extended, so that its entity set
                    // is populated
                    this.ExtendingEntityContainer.Validate();

                    foreach (SchemaElement element in this.ExtendingEntityContainer.Members)
                    {
                        AddErrorKind error = this.Members.TryAdd(element.Clone(this));
                        DuplicateOrEquivalentMemberNameWhileExtendingEntityContainer(element, error);
                    }
                }

                HashSet<string> tableKeys = new HashSet<string>();
                
                foreach (SchemaElement element in Members)
                {
                    EntityContainerEntitySet entitySet = element as EntityContainerEntitySet;
                    if (entitySet != null && Schema.DataModel == SchemaDataModelOption.ProviderDataModel)
                    {
                        CheckForDuplicateTableMapping(tableKeys, entitySet);
                    }
                    element.Validate();
                }

                ValidateRelationshipSetHaveUniqueEnds();

                ValidateOnlyBaseEntitySetTypeDefinesConcurrency();

                // Set isAlreadyValidated to true
                _isAlreadyValidated = true;
            }
        }

        /// <summary>
        /// Find the EntityContainerEntitySet in the same EntityContainer with the name from the extent 
        /// attribute
        /// </summary>
        /// <param name="name">the name of the EntityContainerProperty to find</param>
        /// <returns>The EntityContainerProperty it found or null if it fails to find it</returns>
        internal EntityContainerEntitySet FindEntitySet(string name)
        {
            EntityContainer current = this;
            while(current != null)
            {
                foreach (EntityContainerEntitySet set in current.EntitySets)
                {
                    if (Utils.CompareNames(set.Name, name) == 0)
                    {
                        return set;
                    }
                }

                current = current.ExtendingEntityContainer;
            }

            return null;
        }

        private void DuplicateOrEquivalentMemberNameWhileExtendingEntityContainer(SchemaElement schemaElement,
            AddErrorKind error)
        {
            Debug.Assert(error != AddErrorKind.MissingNameError, "Since entity container members are already resolved, name must never be empty");
            Debug.Assert(this.ExtendingEntityContainer != null, "ExtendingEntityContainer must not be null");

            if (error != AddErrorKind.Succeeded)
            {
                Debug.Assert(error == AddErrorKind.DuplicateNameError, "Error must be duplicate name error");
                schemaElement.AddError(ErrorCode.AlreadyDefined, EdmSchemaErrorSeverity.Error,
                            System.Data.Entity.Strings.DuplicateMemberNameInExtendedEntityContainer(
                                schemaElement.Name, ExtendingEntityContainer.Name, this.Name));
            }
        }

        private void ValidateOnlyBaseEntitySetTypeDefinesConcurrency()
        {
            // collect all the base entitySet types
            Dictionary<SchemaEntityType, EntityContainerEntitySet> baseEntitySetTypes = new Dictionary<SchemaEntityType, EntityContainerEntitySet>();
            foreach (SchemaElement element in Members)
            {
                EntityContainerEntitySet entitySet = element as EntityContainerEntitySet;
                if (entitySet != null && !baseEntitySetTypes.ContainsKey(entitySet.EntityType))
                {
                    baseEntitySetTypes.Add(entitySet.EntityType, entitySet);
                }
            }

            // look through each type in this schema and see if it is derived from a base
            // type if it is then see if it has some "new" Concurrency fields
            foreach (SchemaType type in Schema.SchemaTypes)
            {
                SchemaEntityType itemType = type as SchemaEntityType;
                if (itemType != null)
                {
                    EntityContainerEntitySet set;
                    if (TypeIsSubTypeOf(itemType, baseEntitySetTypes, out set) &&
                       TypeDefinesNewConcurrencyProperties(itemType))
                    {
                        AddError(ErrorCode.ConcurrencyRedefinedOnSubTypeOfEntitySetType,
                            EdmSchemaErrorSeverity.Error,
                            System.Data.Entity.Strings.ConcurrencyRedefinedOnSubTypeOfEntitySetType(itemType.FQName, set.EntityType.FQName, set.FQName));
                    }
                }
            }

        }

        /// <summary>
        /// Validates that if there are more than one relationship set referring to the same type, each role of the relationship type
        /// never refers to the same entity set
        /// </summary>
        private void ValidateRelationshipSetHaveUniqueEnds()
        {
            // Contains the list of ends that have been visited and validated
            List<EntityContainerRelationshipSetEnd> alreadyValidatedEnds = new List<EntityContainerRelationshipSetEnd>();
            bool error = true;

            foreach (EntityContainerRelationshipSet currentSet in this.RelationshipSets)
            {
                foreach (EntityContainerRelationshipSetEnd currentSetEnd in currentSet.Ends)
                {
                    error = false;
                    foreach (EntityContainerRelationshipSetEnd alreadyValidatedEnd in alreadyValidatedEnds)
                    {
                        if (AreRelationshipEndsEqual(alreadyValidatedEnd, currentSetEnd))
                        {
                            AddError(ErrorCode.SimilarRelationshipEnd,
                                     EdmSchemaErrorSeverity.Error,
                                     System.Data.Entity.Strings.SimilarRelationshipEnd(alreadyValidatedEnd.Name, alreadyValidatedEnd.ParentElement.Name,
                                                         currentSetEnd.ParentElement.Name, alreadyValidatedEnd.EntitySet.Name, this.FQName));
                            error = true;
                            break;
                        }
                    }
                    if (!error)
                    {
                        alreadyValidatedEnds.Add(currentSetEnd);
                    }
                }
            }
        }

        private static bool TypeIsSubTypeOf(SchemaEntityType itemType, Dictionary<SchemaEntityType, EntityContainerEntitySet> baseEntitySetTypes, out EntityContainerEntitySet set)
        {
            if (itemType.IsTypeHierarchyRoot)
            {
                // can't be a sub type if we are a base type
                set = null;
                return false;
            }

            // walk up the hierarchy looking for a base that is the base type of an entityset
            for (SchemaEntityType baseType = itemType.BaseType as SchemaEntityType; baseType != null; baseType = baseType.BaseType as SchemaEntityType)
            {
                if (baseEntitySetTypes.ContainsKey(baseType))
                {
                    set = baseEntitySetTypes[baseType];
                    return true;
                }
            }

            set = null;
            return false;
        }

        private static bool TypeDefinesNewConcurrencyProperties(SchemaEntityType itemType)
        {
            foreach (StructuredProperty property in itemType.Properties)
            {
                if (property.Type is ScalarType && MetadataHelper.GetConcurrencyMode(property.TypeUsage) != ConcurrencyMode.None)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Return the fully qualified name for entity container. Since EntityContainer no longer lives in a schema,
        /// the FQName should be same as that of the Name
        /// </summary>
        public override string FQName
        {
            get
            {
                return this.Name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string Identity
        {
            get
            {
                return Name;
            }
        }

        /// <summary>
        /// Adds a child EntitySet's tableKey (Schema/Table combination) to the validation collection
        /// This is used to validate that no child EntitySets share a Schema.Table combination
        /// </summary>
        private void CheckForDuplicateTableMapping(HashSet<string> tableKeys, EntityContainerEntitySet entitySet)
        {
            string schema;
            string table;

            if (String.IsNullOrEmpty(entitySet.DbSchema))
            {
                // if there is no specified DbSchema, use the parent EntityContainer's name
                schema = this.Name;
            }
            else
            {
                schema = entitySet.DbSchema;
            }


            if (String.IsNullOrEmpty(entitySet.Table))
            {
                // if there is no specified Table, use the EntitySet's name
                table = entitySet.Name;
            }
            else
            {
                table = entitySet.Table;
            }

            // create a key using the DbSchema and Table
            string tableKey = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}.{1}", schema, table);
            if (entitySet.DefiningQuery != null)
            {
                // don't consider the schema name for defining queries, because
                // we can't say for sure that it is the same as the entity container
                // so in this example
                //
                // <EntityContainer Name="dbo">
                //   <EntitySet Name="ByVal">
                //     <DefiningQuery>Select col1 from dbi.ByVal</DefiningQuery>
                //   </EntitySet>
                //   <EntitySet Name="ByVal1" Table="ByVal"/>
                //   ...
                //
                // ByVal and ByVal1 should not conflict in our check
                tableKey = entitySet.Name;
            }

            bool alreadyExisted = !tableKeys.Add(tableKey);
            if (alreadyExisted)
            {
                entitySet.AddError(ErrorCode.AlreadyDefined, EdmSchemaErrorSeverity.Error, System.Data.Entity.Strings.DuplicateEntitySetTable(entitySet.Name, schema, table));
            }
        }

        /// <summary>
        /// Returns true if the given two ends are similar - the relationship type that this ends belongs to is the same
        /// and the entity set refered by the ends are same and they have the same role name
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static bool AreRelationshipEndsEqual(EntityContainerRelationshipSetEnd left, EntityContainerRelationshipSetEnd right)
        {
            Debug.Assert(left.ParentElement.ParentElement == right.ParentElement.ParentElement, "both end should belong to the same entity container");

            if (object.ReferenceEquals(left.EntitySet, right.EntitySet) &&
                object.ReferenceEquals(left.ParentElement.Relationship, right.ParentElement.Relationship) &&
                left.Name == right.Name)
            {
                return true;
            }

            return false;
        }
        #endregion
    }

}
