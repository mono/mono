//---------------------------------------------------------------------
// <copyright file="OneToOneMappingSerializer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Common.Utils;
using System.Data.Mapping;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace System.Data.Entity.Design.Common
{
    internal class OneToOneMappingSerializer
    {
        internal class MappingLookups
        {
            internal Dictionary<EntityType, EntityType> StoreEntityTypeToModelEntityType = new Dictionary<EntityType, EntityType>();
            internal Dictionary<EdmProperty, EdmProperty> StoreEdmPropertyToModelEdmProperty = new Dictionary<EdmProperty, EdmProperty>();
            internal Dictionary<EntitySet, EntitySet> StoreEntitySetToModelEntitySet = new Dictionary<EntitySet, EntitySet>();
            
            internal Dictionary<AssociationType, AssociationType> StoreAssociationTypeToModelAssociationType = new Dictionary<AssociationType, AssociationType>();
            internal Dictionary<AssociationEndMember, AssociationEndMember> StoreAssociationEndMemberToModelAssociationEndMember = new Dictionary<AssociationEndMember, AssociationEndMember>();
            internal Dictionary<AssociationSet, AssociationSet> StoreAssociationSetToModelAssociationSet = new Dictionary<AssociationSet, AssociationSet>();
            internal Dictionary<AssociationSetEnd, AssociationSetEnd> StoreAssociationSetEndToModelAssociationSetEnd = new Dictionary<AssociationSetEnd, AssociationSetEnd>();

            internal List<CollapsedEntityAssociationSet> CollapsedEntityAssociationSets = new List<CollapsedEntityAssociationSet>();

            internal List<Tuple<EdmFunction, EdmFunction>> StoreFunctionToFunctionImport = new List<Tuple<EdmFunction, EdmFunction>>();
        }

        // this class represents a construct found in the ssdl where a link table
        // contained no data (all its properties were part of its keys)
        // it has exactly two associations
        // the entity type is the TO side of both associations          
        // all the colums are used as TO columns in the constraint
        internal class CollapsedEntityAssociationSet
        {
            private EntitySet _storeEntitySet;
            private List<AssociationSet> _storeAssociationSets = new List<AssociationSet>(2);
            private AssociationSet _modelAssociationSet;

            public AssociationSet ModelAssociationSet
            {
                get { return _modelAssociationSet; }
                set 
                {
                    Debug.Assert(_modelAssociationSet == null, "why is this getting set multiple times, it should only be set after the new set is created");
                    _modelAssociationSet = value;
                }
            }

            public CollapsedEntityAssociationSet(EntitySet entitySet)
            {
                Debug.Assert(entitySet != null, "entitySet parameter is null");
                _storeEntitySet = entitySet;
            }

            public EntitySet EntitySet
            {
                get { return _storeEntitySet; }
            }

            public List<AssociationSet> AssociationSets
            {
                get { return _storeAssociationSets; }
            }
            
            public void GetStoreAssociationSetEnd(int index, out AssociationSetEnd storeAssociationSetEnd, out RelationshipMultiplicity multiplicity, out OperationAction deleteBehavior)
            {
                Debug.Assert(index >= 0 && index < AssociationSets.Count, "out of bounds dude!!");
                Debug.Assert(AssociationSets.Count == 2, "This code depends on only having exactly two AssociationSets");
                GetFromAssociationSetEnd(AssociationSets[index], AssociationSets[(index+1)%2], out storeAssociationSetEnd, out multiplicity, out deleteBehavior);
            }

            private void GetFromAssociationSetEnd(AssociationSet definingSet, AssociationSet multiplicitySet, out AssociationSetEnd associationSetEnd, out RelationshipMultiplicity multiplicity, out OperationAction deleteBehavior)
            {
                // for a situation like this (CD is CascadeDelete)
                // 
                // --------  CD   --------  CD   --------
                // | A    |1 <-  1| AtoB |* <-  1|  B   |  
                // |      |-------|      |-------|      | 
                // |      |       |      |       |      |
                // --------       --------       --------
                // 
                // You get
                // --------  CD   --------
                // |  A   |* <-  1|  B   |
                // |      |-------|      |
                // |      |       |      |
                // --------       --------
                // 
                // Notice that the of the new "link table association" muliplicities are opposite of what comming into the original link table
                // this seems counter intuitive at first, but makes sense when you think all the way through it
                //
                // CascadeDelete Behavior (we can assume the runtime will always delete cascade 
                //                         to the link table from the outside tables (it actually doesn't, but that is a 















                associationSetEnd = GetAssociationSetEnd(definingSet, true);
                AssociationSetEnd multiplicityAssociationSetEnd = GetAssociationSetEnd(multiplicitySet, false);
                multiplicity = multiplicityAssociationSetEnd.CorrespondingAssociationEndMember.RelationshipMultiplicity;
                deleteBehavior = OperationAction.None;
                if (multiplicity != RelationshipMultiplicity.Many)
                {
                    OperationAction otherEndBehavior = GetAssociationSetEnd(definingSet, false).CorrespondingAssociationEndMember.DeleteBehavior;
                    if(otherEndBehavior == OperationAction.None)
                    {
                        // Since the other end does not have an operation
                        // that means that only one end could possibly have an operation, that is good
                        // so set it the operation
                        deleteBehavior = multiplicityAssociationSetEnd.CorrespondingAssociationEndMember.DeleteBehavior;
                    }
                }
            }

            private static AssociationSetEnd GetAssociationSetEnd(AssociationSet set, bool fromEnd)
            {
                Debug.Assert(set.ElementType.ReferentialConstraints.Count == 1, "no referenctial constraint for association[0]");
                ReferentialConstraint constraint = set.ElementType.ReferentialConstraints[0];

                Debug.Assert(set.AssociationSetEnds.Count == 2, "Associations are assumed to have two ends");
                int toEndIndex, fromEndIndex;
                if (set.AssociationSetEnds[0].CorrespondingAssociationEndMember == constraint.FromRole)
                {
                    fromEndIndex = 0;
                    toEndIndex = 1;
                   
                }
                else
                {
                    fromEndIndex = 1;
                    toEndIndex = 0;
                }


                if (fromEnd)
                {
                    return set.AssociationSetEnds[fromEndIndex];
                }
                else
                {
                    return set.AssociationSetEnds[toEndIndex];
                }
            }

            public bool MeetsRequirementsForCollapsableAssociation
            {
                get
                {
                    if (_storeAssociationSets.Count != 2)
                        return false;

                    ReferentialConstraint constraint0;
                    ReferentialConstraint constraint1;
                    GetConstraints(out constraint0, out constraint1);
                    if (!IsEntityDependentSideOfBothAssociations(constraint0, constraint1))
                        return false;

                    if (!IsAtLeastOneColumnOfBothDependentRelationshipColumnSetsNonNullable(constraint0, constraint1))
                        return false;

                    if (!AreAllEntityColumnsMappedAsToColumns(constraint0, constraint1))
                        return false;

                    if (IsAtLeastOneColumnFKInBothAssociations(constraint0, constraint1))
                        return false;

                    return true;
                }
            }

            private bool IsAtLeastOneColumnFKInBothAssociations(ReferentialConstraint constraint0, ReferentialConstraint constraint1)
            {
                return constraint1.ToProperties.Any(c => constraint0.ToProperties.Contains(c));
            }

            private bool IsAtLeastOneColumnOfBothDependentRelationshipColumnSetsNonNullable(ReferentialConstraint constraint0, ReferentialConstraint constraint1)
            {
                return ToPropertyHasNonNullableColumn(constraint0) && ToPropertyHasNonNullableColumn(constraint1);
            }

            private static bool ToPropertyHasNonNullableColumn(ReferentialConstraint constraint)
            {
                foreach (EdmProperty property in constraint.ToProperties)
                {
                    if (!property.Nullable)
                    {
                        return true;
                    }
                }
                return false;
            }

            private bool AreAllEntityColumnsMappedAsToColumns(ReferentialConstraint constraint0, ReferentialConstraint constraint1)
            {
                Set<string> names = new Set<string>();
                AddToPropertyNames(constraint0, names);
                AddToPropertyNames(constraint1, names);
                return names.Count == _storeEntitySet.ElementType.Properties.Count;
            }

            private static void AddToPropertyNames(ReferentialConstraint constraint, Set<string> names)
            {
                foreach (EdmProperty property in constraint.ToProperties)
                {
                    names.Add(property.Name);
                }
            }

            private bool IsEntityDependentSideOfBothAssociations(ReferentialConstraint constraint0, ReferentialConstraint constraint1)
            {
                return ((RefType)constraint0.ToRole.TypeUsage.EdmType).ElementType == _storeEntitySet.ElementType && ((RefType)constraint1.ToRole.TypeUsage.EdmType).ElementType == _storeEntitySet.ElementType;
            }

            private void GetConstraints(out ReferentialConstraint constraint0, out ReferentialConstraint constraint1)
            {
                Debug.Assert(_storeAssociationSets.Count == 2, "don't call this method if you don't have two associations");
                Debug.Assert(_storeAssociationSets[0].ElementType.ReferentialConstraints.Count == 1, "no referenctial constraint for association[0]");
                Debug.Assert(_storeAssociationSets[1].ElementType.ReferentialConstraints.Count == 1, "no referenctial constraint for association[1]");
                constraint0 = _storeAssociationSets[0].ElementType.ReferentialConstraints[0];
                constraint1 = _storeAssociationSets[1].ElementType.ReferentialConstraints[0];
            }
        }

        private MappingLookups _lookups;
        private EntityContainer _storeContainer;
        private EntityContainer _modelContainer;
        private string _xmlNamespace;

        internal OneToOneMappingSerializer(MappingLookups lookups,
            EntityContainer storeContainer,
            EntityContainer modelContainer,
            Version schemaVersion)
        {
            EDesignUtil.CheckArgumentNull(lookups, "lookups");
            EDesignUtil.CheckArgumentNull(storeContainer, "storeContainer");
            EDesignUtil.CheckArgumentNull(modelContainer, "modelContainer");
            _lookups = lookups;
            _storeContainer = storeContainer;
            _modelContainer = modelContainer;
            _xmlNamespace = EntityFrameworkVersions.GetSchemaNamespace(schemaVersion, DataSpace.CSSpace);
        }
       
        public void WriteXml(XmlWriter writer)
        {
            EDesignUtil.CheckArgumentNull(writer, "writer");

            WriteMappingStartElement(writer);
            WriteEntityContainerMappingElement(writer);
            writer.WriteEndElement();
        }

        private void WriteEntityContainerMappingElement(XmlWriter writer)
        {
            writer.WriteStartElement(StorageMslConstructs.EntityContainerMappingElement, _xmlNamespace);
            writer.WriteAttributeString(StorageMslConstructs.StorageEntityContainerAttribute, _storeContainer.Name);
            writer.WriteAttributeString(StorageMslConstructs.CdmEntityContainerAttribute, _modelContainer.Name);

            foreach (EntitySet set in _lookups.StoreEntitySetToModelEntitySet.Keys)
            {
                EntitySet modelEntitySet = _lookups.StoreEntitySetToModelEntitySet[set];
                WriteEntitySetMappingElement(writer, set, modelEntitySet);
            }

            foreach(AssociationSet set in _lookups.StoreAssociationSetToModelAssociationSet.Keys)
            {
                AssociationSet modelAssociationSet = _lookups.StoreAssociationSetToModelAssociationSet[set];
                WriteAssociationSetMappingElement(writer, set, modelAssociationSet);
            }

            foreach (CollapsedEntityAssociationSet set in _lookups.CollapsedEntityAssociationSets)
            {
                WriteAssociationSetMappingElement(writer, set);
            }

            foreach (var functionMapping in _lookups.StoreFunctionToFunctionImport)
            {
                var storeFunction = functionMapping.Item1;
                var functionImport = functionMapping.Item2;
                WriteFunctionImportMappingElement(writer, storeFunction, functionImport);
            }

            writer.WriteEndElement();
        }

        private void WriteFunctionImportMappingElement(XmlWriter writer, EdmFunction storeFunction, EdmFunction functionImport)
        {
            Debug.Assert(storeFunction.IsComposableAttribute, "storeFunction.IsComposableAttribute");
            Debug.Assert(storeFunction.ReturnParameters.Count == 1, "storeFunction.ReturnParameters.Count == 1");
            Debug.Assert(functionImport.IsComposableAttribute, "functionImport.IsComposableAttribute");
            Debug.Assert(functionImport.ReturnParameters.Count == 1, "functionImport.ReturnParameters.Count == 1");

            writer.WriteStartElement(StorageMslConstructs.FunctionImportMappingElement, _xmlNamespace);
            writer.WriteAttributeString(StorageMslConstructs.FunctionImportMappingFunctionNameAttribute, storeFunction.FullName);
            writer.WriteAttributeString(StorageMslConstructs.FunctionImportMappingFunctionImportNameAttribute, functionImport.Name);

            RowType tvfReturnType = TypeHelpers.GetTvfReturnType(storeFunction);
            if (tvfReturnType != null)
            {
                // Table-valued function
                Debug.Assert(functionImport.ReturnParameter.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType, "functionImport is expected to return Collection(ComplexType)");
                var modelCollectionType = (CollectionType)functionImport.ReturnParameter.TypeUsage.EdmType;
                Debug.Assert(modelCollectionType.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.ComplexType, "functionImport is expected to return Collection(ComplexType)");
                var modelComplexType = (ComplexType)modelCollectionType.TypeUsage.EdmType;

                // Write ResultMapping/ComplexTypeMapping
                writer.WriteStartElement(StorageMslConstructs.FunctionImportMappingResultMapping, _xmlNamespace);
                writer.WriteStartElement(StorageMslConstructs.ComplexTypeMappingElement, _xmlNamespace);
                writer.WriteAttributeString(StorageMslConstructs.ComplexTypeMappingTypeNameAttribute, modelComplexType.FullName);
                foreach (EdmProperty storeProperty in tvfReturnType.Properties)
                {
                    EdmProperty modelProperty = _lookups.StoreEdmPropertyToModelEdmProperty[storeProperty];
                    WriteScalarPropertyElement(writer, storeProperty, modelProperty);
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            else
            {
                Debug.Fail("Only TVF store functions are supported.");
            }

            writer.WriteEndElement();
        }

        private void WriteAssociationSetMappingElement(XmlWriter writer, CollapsedEntityAssociationSet collapsedAssociationSet)
        {
            if (!collapsedAssociationSet.ModelAssociationSet.ElementType.IsForeignKey)
            {
                writer.WriteStartElement(StorageMslConstructs.AssociationSetMappingElement, _xmlNamespace);
                writer.WriteAttributeString(StorageMslConstructs.AssociationSetMappingNameAttribute, collapsedAssociationSet.ModelAssociationSet.Name);
                writer.WriteAttributeString(StorageMslConstructs.AssociationSetMappingTypeNameAttribute, collapsedAssociationSet.ModelAssociationSet.ElementType.FullName);
                writer.WriteAttributeString(StorageMslConstructs.AssociationSetMappingStoreEntitySetAttribute, collapsedAssociationSet.EntitySet.Name);


                for (int i = 0; i < collapsedAssociationSet.AssociationSets.Count; i++)
                {
                    AssociationSetEnd storeEnd;
                    RelationshipMultiplicity multiplicity;
                    OperationAction deleteBehavior;
                    collapsedAssociationSet.GetStoreAssociationSetEnd(i, out storeEnd, out multiplicity, out deleteBehavior);
                    AssociationSetEnd modelEnd = _lookups.StoreAssociationSetEndToModelAssociationSetEnd[storeEnd];
                    WriteEndPropertyElement(writer, storeEnd, modelEnd);
                }

                // don't need condition element

                writer.WriteEndElement();
            }
        }

        private void WriteAssociationSetMappingElement(XmlWriter writer, AssociationSet store, AssociationSet model)
        {
            if (!model.ElementType.IsForeignKey)
            {
                writer.WriteStartElement(StorageMslConstructs.AssociationSetMappingElement, _xmlNamespace);
                writer.WriteAttributeString(StorageMslConstructs.AssociationSetMappingNameAttribute, model.Name);
                writer.WriteAttributeString(StorageMslConstructs.AssociationSetMappingTypeNameAttribute, model.ElementType.FullName);

                // all column names must be the primary key of the 
                // end, but as columns in the Fk table.
                AssociationSetEnd foreignKeyTableEnd = GetAssociationSetEndForForeignKeyTable(store);
                writer.WriteAttributeString(StorageMslConstructs.AssociationSetMappingStoreEntitySetAttribute, foreignKeyTableEnd.EntitySet.Name);

                foreach (AssociationSetEnd storeEnd in store.AssociationSetEnds)
                {
                    AssociationSetEnd modelEnd = _lookups.StoreAssociationSetEndToModelAssociationSetEnd[storeEnd];
                    WriteEndPropertyElement(writer, storeEnd, modelEnd);
                }

                ReferentialConstraint constraint = GetReferentialConstraint(store);
                foreach (EdmProperty fkColumn in constraint.ToProperties)
                {
                    if (fkColumn.Nullable)
                    {
                        WriteConditionElement(writer, fkColumn);
                    }
                }

                writer.WriteEndElement();
            }
        }

        private void WriteConditionElement(XmlWriter writer, EdmProperty fkColumn)
        {
            writer.WriteStartElement(StorageMslConstructs.ConditionElement, _xmlNamespace);
            writer.WriteAttributeString(StorageMslConstructs.ConditionColumnNameAttribute, fkColumn.Name);
            writer.WriteAttributeString(StorageMslConstructs.ConditionIsNullAttribute, "false");
            writer.WriteEndElement();
        }

        private static AssociationSetEnd GetAssociationSetEndForForeignKeyTable(AssociationSet store)
        {
            ReferentialConstraint constraint = GetReferentialConstraint(store);
            return store.AssociationSetEnds.GetValue(constraint.ToRole.Name, false);
        }

        internal static ReferentialConstraint GetReferentialConstraint(AssociationSet set)
        {
            // this seeems like a hack, but it is what we have right now.
            ReferentialConstraint constraint = null;
            foreach (ReferentialConstraint rc in set.ElementType.ReferentialConstraints)
            {
                Debug.Assert(constraint == null, "we should only get one");
                constraint = rc;
            }
            Debug.Assert(constraint != null, "we should get at least one constraint");
            return constraint;
        }

        private void WriteEndPropertyElement(XmlWriter writer, AssociationSetEnd store, AssociationSetEnd model)
        {
            writer.WriteStartElement(StorageMslConstructs.EndPropertyMappingElement, _xmlNamespace);
            writer.WriteAttributeString(StorageMslConstructs.EndPropertyMappingNameAttribute, model.Name);
            foreach (EdmProperty storeKeyMember in store.EntitySet.ElementType.KeyMembers)
            {
                EdmProperty modelKeyMember = _lookups.StoreEdmPropertyToModelEdmProperty[storeKeyMember];
                EdmProperty storeFkTableMember = GetAssociatedFkColumn(store, storeKeyMember);
                WriteScalarPropertyElement(writer, storeFkTableMember, modelKeyMember);
            }
            writer.WriteEndElement();
        }

        private static EdmProperty GetAssociatedFkColumn(AssociationSetEnd store, EdmProperty storeKeyProperty)
        {
            ReferentialConstraint constraint = GetReferentialConstraint(store.ParentAssociationSet);
            if (store.Name == constraint.FromRole.Name)
            {
                for (int i = 0; i < constraint.FromProperties.Count; i++)
                {
                    if (constraint.FromProperties[i] == storeKeyProperty)
                    {
                        // return the matching Fk column
                        return constraint.ToProperties[i];
                    }
                }
            }

                return storeKeyProperty;
        }

        private void WriteEntitySetMappingElement(XmlWriter writer, EntitySet store, EntitySet model)
        {
            writer.WriteStartElement(StorageMslConstructs.EntitySetMappingElement, _xmlNamespace);
            writer.WriteAttributeString(StorageMslConstructs.EntitySetMappingNameAttribute, model.Name);
            WriteEntityTypeMappingElement(writer, store, model);
            writer.WriteEndElement();
        }

        private void WriteEntityTypeMappingElement(XmlWriter writer, EntitySet store, EntitySet model)
        {
            writer.WriteStartElement(StorageMslConstructs.EntityTypeMappingElement, _xmlNamespace);
            writer.WriteAttributeString(StorageMslConstructs.EntityTypeMappingTypeNameAttribute, model.ElementType.FullName);
            WriteMappingFragmentElement(writer, store, model);
            writer.WriteEndElement();
        }

        private void WriteMappingFragmentElement(XmlWriter writer, EntitySet store, EntitySet model)
        {
            writer.WriteStartElement(StorageMslConstructs.MappingFragmentElement, _xmlNamespace);
            writer.WriteAttributeString(StorageMslConstructs.EntityTypeMappingStoreEntitySetAttribute, store.Name);
            foreach (EdmProperty storeProperty in store.ElementType.Properties)
            {
                // we don't add the fk properties to c-space, so some are missing,
                // check to see if we have a map for this one
                if (_lookups.StoreEdmPropertyToModelEdmProperty.ContainsKey(storeProperty))
                {
                    EdmProperty modelProperty = _lookups.StoreEdmPropertyToModelEdmProperty[storeProperty];
                    WriteScalarPropertyElement(writer, storeProperty, modelProperty);
                }
            }
            writer.WriteEndElement();
        }

        private void WriteScalarPropertyElement(XmlWriter writer, EdmProperty store, EdmProperty model)
        {
            Debug.Assert(store.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType, "only expect scalar type properties");
            Debug.Assert(model.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType, "only expect scalar type properties");

            writer.WriteStartElement(StorageMslConstructs.ScalarPropertyElement, _xmlNamespace);
            writer.WriteAttributeString(StorageMslConstructs.ScalarPropertyNameAttribute, model.Name);
            writer.WriteAttributeString(StorageMslConstructs.ScalarPropertyColumnNameAttribute, store.Name);
            writer.WriteEndElement();
        }

        private void WriteMappingStartElement(XmlWriter writer)
        {
            writer.WriteStartElement(StorageMslConstructs.MappingElement, _xmlNamespace);
            writer.WriteAttributeString(StorageMslConstructs.MappingSpaceAttribute, "C-S");
        }
    }
}
