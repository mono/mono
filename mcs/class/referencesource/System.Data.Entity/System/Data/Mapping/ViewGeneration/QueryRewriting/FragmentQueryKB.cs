//---------------------------------------------------------------------
// <copyright file="FragmentQueryKB.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Data.Common.Utils;
using System.Data.Common.Utils.Boolean;
using System.Data.Mapping.ViewGeneration.Structures;
using System.Data.Metadata.Edm;
using System.Linq;

namespace System.Data.Mapping.ViewGeneration.QueryRewriting
{
    internal class FragmentQueryKB : KnowledgeBase<DomainConstraint<BoolLiteral, Constant>>
    {
        private BoolExpr<DomainConstraint<BoolLiteral, Constant>> _kbExpression = TrueExpr<DomainConstraint<BoolLiteral, Constant>>.Value;

        internal override void AddFact(BoolExpr<DomainConstraint<BoolLiteral, Constant>> fact)
        {
            base.AddFact(fact);
            _kbExpression = new AndExpr<DomainConstraint<BoolLiteral, Constant>>(_kbExpression, fact);
        }
        internal BoolExpr<DomainConstraint<BoolLiteral, Constant>> KbExpression
        {
            get { return _kbExpression; }
        }

        internal void CreateVariableConstraints(EntitySetBase extent, MemberDomainMap domainMap, EdmItemCollection edmItemCollection)
        {
            CreateVariableConstraintsRecursion(extent.ElementType, new MemberPath(extent), domainMap, edmItemCollection);
        }

        internal void CreateAssociationConstraints(EntitySetBase extent, MemberDomainMap domainMap, EdmItemCollection edmItemCollection)
        {
            AssociationSet assocSet = extent as AssociationSet;
            if (assocSet != null)
            {
                BoolExpression assocSetExpr = BoolExpression.CreateLiteral(new RoleBoolean(assocSet), domainMap);

                //Set of Keys for this Association Set
                //need to key on EdmMember and EdmType because A, B subtype of C, can have the same id (EdmMember) that is defined in C.
                HashSet<Pair<EdmMember, EntityType>> associationkeys = new HashSet<Pair<EdmMember, EntityType>>();


                //foreach end, add each Key
                foreach (var endMember in assocSet.ElementType.AssociationEndMembers)
                {
                    EntityType type = (EntityType)((RefType)endMember.TypeUsage.EdmType).ElementType;
                    type.KeyMembers.All(member => associationkeys.Add(new Pair<EdmMember, EntityType>(member, type)) || true /* prevent early termination */);
                }

                foreach (AssociationSetEnd end in assocSet.AssociationSetEnds)
                {
                    // construct type condition
                    HashSet<EdmType> derivedTypes = new HashSet<EdmType>();
                    derivedTypes.UnionWith(MetadataHelper.GetTypeAndSubtypesOf(end.CorrespondingAssociationEndMember.TypeUsage.EdmType, edmItemCollection, false));

                    BoolExpression typeCondition = CreateIsOfTypeCondition(new MemberPath(end.EntitySet),
                                                        derivedTypes, domainMap);

                    BoolExpression inRoleExpression = BoolExpression.CreateLiteral(new RoleBoolean(end), domainMap);
                    BoolExpression inSetExpression = BoolExpression.CreateAnd(
                                  BoolExpression.CreateLiteral(new RoleBoolean(end.EntitySet), domainMap),
                                  typeCondition);

                    // InRole -> (InSet AND type(Set)=T)
                    AddImplication(inRoleExpression.Tree, inSetExpression.Tree);

                    if (MetadataHelper.IsEveryOtherEndAtLeastOne(assocSet, end.CorrespondingAssociationEndMember))
                    {
                        AddImplication(inSetExpression.Tree, inRoleExpression.Tree);
                    }

                    // Add equivalence between association set an End/Role if necessary.
                    //   Equivalence is added when a given association end's keys subsumes keys for
                    //   all the other association end.

                    // For example: We have Entity Sets A[id1], B[id2, id3] and an association A_B between them.
                    // Ref Constraint A.id1 = B.id2
                    // In this case, the Association Set has Key <id1, id2, id3>
                    // id1 alone can not identify a unique tuple in the Association Set, but <id2, id3> can.
                    // Therefore we add a constraint: InSet(B) <=> InEnd(A_B.B)

                    if (MetadataHelper.DoesEndKeySubsumeAssociationSetKey(assocSet,
                        end.CorrespondingAssociationEndMember,
                        associationkeys))
                    {
                        AddEquivalence(inRoleExpression.Tree, assocSetExpr.Tree);
                    }

                }

                // add rules for referential constraints (borrowed from LeftCellWrapper.cs)
                AssociationType assocType = assocSet.ElementType;

                foreach (ReferentialConstraint constraint in assocType.ReferentialConstraints)
                {
                    AssociationEndMember toEndMember = (AssociationEndMember)constraint.ToRole;
                    EntitySet toEntitySet = MetadataHelper.GetEntitySetAtEnd(assocSet, toEndMember);
                    // Check if the keys of the entitySet's are equal to what is specified in the constraint
                    // How annoying that KeyMembers returns EdmMember and not EdmProperty
                    IEnumerable<EdmMember> toProperties = Helpers.AsSuperTypeList<EdmProperty, EdmMember>(constraint.ToProperties);
                    if (Helpers.IsSetEqual(toProperties, toEntitySet.ElementType.KeyMembers, EqualityComparer<EdmMember>.Default))
                    {
                        // Now check that the FromEnd is 1..1 (only then will all the Addresses be present in the assoc set)
                        if (constraint.FromRole.RelationshipMultiplicity.Equals(RelationshipMultiplicity.One))
                        {
                            // Make sure that the ToEnd is not 0..* because then the schema is broken
                            Debug.Assert(constraint.ToRole.RelationshipMultiplicity.Equals(RelationshipMultiplicity.Many) == false);
                            // Equate the ends
                            BoolExpression inRoleExpression1 = BoolExpression.CreateLiteral(new RoleBoolean(assocSet.AssociationSetEnds[0]), domainMap);
                            BoolExpression inRoleExpression2 = BoolExpression.CreateLiteral(new RoleBoolean(assocSet.AssociationSetEnds[1]), domainMap);
                            AddEquivalence(inRoleExpression1.Tree, inRoleExpression2.Tree);
                        }
                    }
                }
            }
        }

        internal void CreateEquivalenceConstraintForOneToOneForeignKeyAssociation(AssociationSet assocSet, MemberDomainMap domainMap,
            EdmItemCollection edmItemCollection)
        {
            AssociationType assocType = assocSet.ElementType;
            foreach (ReferentialConstraint constraint in assocType.ReferentialConstraints)
            {
                AssociationEndMember toEndMember = (AssociationEndMember)constraint.ToRole;
                AssociationEndMember fromEndMember = (AssociationEndMember)constraint.FromRole;
                EntitySet toEntitySet = MetadataHelper.GetEntitySetAtEnd(assocSet, toEndMember);
                EntitySet fromEntitySet = MetadataHelper.GetEntitySetAtEnd(assocSet, fromEndMember);

                // Check if the keys of the entitySet's are equal to what is specified in the constraint
                IEnumerable<EdmMember> toProperties = Helpers.AsSuperTypeList<EdmProperty, EdmMember>(constraint.ToProperties);
                if (Helpers.IsSetEqual(toProperties, toEntitySet.ElementType.KeyMembers, EqualityComparer<EdmMember>.Default))
                {
                    //make sure that the method called with a 1:1 association
                    Debug.Assert(constraint.FromRole.RelationshipMultiplicity.Equals(RelationshipMultiplicity.One));
                    Debug.Assert(constraint.ToRole.RelationshipMultiplicity.Equals(RelationshipMultiplicity.One));
                    // Create an Equivalence between the two Sets participating in this AssociationSet
                    BoolExpression fromSetExpression = BoolExpression.CreateLiteral(new RoleBoolean(fromEntitySet), domainMap);
                    BoolExpression toSetExpression = BoolExpression.CreateLiteral(new RoleBoolean(toEntitySet), domainMap);
                    AddEquivalence(fromSetExpression.Tree, toSetExpression.Tree);
                }
            }
        }

        private void CreateVariableConstraintsRecursion(EdmType edmType, MemberPath currentPath, MemberDomainMap domainMap, EdmItemCollection edmItemCollection)
        {
            // Add the types can member have, i.e., its type and its subtypes
            HashSet<EdmType> possibleTypes = new HashSet<EdmType>();
            possibleTypes.UnionWith(MetadataHelper.GetTypeAndSubtypesOf(edmType, edmItemCollection, true));

            foreach (EdmType possibleType in possibleTypes)
            {
                // determine type domain

                HashSet<EdmType> derivedTypes = new HashSet<EdmType>();
                derivedTypes.UnionWith(MetadataHelper.GetTypeAndSubtypesOf(possibleType, edmItemCollection, false));
                if (derivedTypes.Count != 0)
                {
                    BoolExpression typeCondition = CreateIsOfTypeCondition(currentPath, derivedTypes, domainMap);
                    BoolExpression typeConditionComplement = BoolExpression.CreateNot(typeCondition);
                    if (false == typeConditionComplement.IsSatisfiable())
                    {
                        continue;
                    }

                    StructuralType structuralType = (StructuralType)possibleType;
                    foreach (EdmProperty childProperty in structuralType.GetDeclaredOnlyMembers<EdmProperty>())
                    {
                        MemberPath childPath = new MemberPath(currentPath, childProperty);
                        bool isScalar = MetadataHelper.IsNonRefSimpleMember(childProperty);

                        if (domainMap.IsConditionMember(childPath) || domainMap.IsProjectedConditionMember(childPath))
                        {
                            BoolExpression nullCondition;
                            List<Constant> childDomain = new List<Constant>(domainMap.GetDomain(childPath));
                            if (isScalar)
                            {
                                nullCondition = BoolExpression.CreateLiteral(new ScalarRestriction(new MemberProjectedSlot(childPath),
                                                                             new Domain(ScalarConstant.Undefined, childDomain)), domainMap);
                            }
                            else
                            {
                                nullCondition = BoolExpression.CreateLiteral(new TypeRestriction(new MemberProjectedSlot(childPath),
                                                                             new Domain(TypeConstant.Undefined, childDomain)), domainMap);
                            }
                            // Properties not occuring in type are UNDEFINED
                            AddEquivalence(typeConditionComplement.Tree, nullCondition.Tree);
                        }

                        // recurse into complex types
                        if (false == isScalar)
                        {
                            CreateVariableConstraintsRecursion(childPath.EdmType, childPath, domainMap, edmItemCollection);
                        }
                    }
                }
            }
        }

        private static BoolExpression CreateIsOfTypeCondition(MemberPath currentPath, IEnumerable<EdmType> derivedTypes, MemberDomainMap domainMap)
        {
            Domain typeDomain = new Domain(derivedTypes.Select(derivedType => (Constant)new TypeConstant(derivedType)), domainMap.GetDomain(currentPath));
            BoolExpression typeCondition = BoolExpression.CreateLiteral(new TypeRestriction(new MemberProjectedSlot(currentPath), typeDomain), domainMap);
            return typeCondition;
        }
    }
}
