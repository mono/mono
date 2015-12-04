//---------------------------------------------------------------------
// <copyright file="MemberPath.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Data.Common.CommandTrees;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Common.Utils;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Data.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Metadata.Edm;
using System.Linq;

namespace System.Data.Mapping.ViewGeneration.Structures
{
    /// <summary>
    /// A class that corresponds to a path in some extent, e.g., Person, Person.addr, Person.addr.state
    /// Empty path represents path to the extent.
    /// </summary>
    internal sealed class MemberPath : InternalBase, IEquatable<MemberPath>
    {
        #region Fields
        /// <summary>
        /// The base entity set.
        /// </summary>
        private readonly EntitySetBase m_extent;
        /// <summary>
        ///  List of members in the path.
        /// </summary>
        private readonly List<EdmMember> m_path;
        internal static readonly IEqualityComparer<MemberPath> EqualityComparer = new Comparer();
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a member path that corresponds to <paramref name="path"/> in the <paramref name="extent"/> (or the extent itself).
        /// </summary>
        internal MemberPath(EntitySetBase extent, IEnumerable<EdmMember> path)
        {
            m_extent = extent;
            m_path = path.ToList();
        }

        /// <summary>
        /// Creates a member path that corresponds to the <paramref name="extent"/>.
        /// </summary>
        internal MemberPath(EntitySetBase extent)
            : this(extent, Enumerable.Empty<EdmMember>())
        { }

        /// <summary>
        /// Creates a path corresponding to <paramref name="extent"/>.<paramref name="member"/>
        /// </summary>
        internal MemberPath(EntitySetBase extent, EdmMember member)
            : this(extent, Enumerable.Repeat<EdmMember>(member, 1))
        { }

        /// <summary>
        /// Creates a member path corresponding to the path <paramref name="prefix"/>.<paramref name="last"/>
        /// </summary>
        internal MemberPath(MemberPath prefix, EdmMember last)
        {
            m_extent = prefix.m_extent;
            m_path = new List<EdmMember>(prefix.m_path);
            m_path.Add(last);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Returns the first path item in a non-empty path, otherwise null.
        /// </summary>
        internal EdmMember RootEdmMember
        {
            get { return m_path.Count > 0 ? m_path[0] : null; }
        }

        /// <summary>
        /// Returns the last path item in a non-empty path, otherwise null.
        /// </summary>
        internal EdmMember LeafEdmMember
        {
            get { return m_path.Count > 0 ? m_path[m_path.Count - 1] : null; }
        }

        /// <summary>
        /// For non-empty paths returns name of the last path item, otherwise returns name of <see cref="Extent"/>.
        /// </summary>
        internal string LeafName
        {
            get
            {
                if (m_path.Count == 0)
                {
                    return m_extent.Name;
                }
                else
                {
                    return LeafEdmMember.Name;
                }
            }
        }

        /// <summary>
        /// Tells path represents a computed slot.
        /// </summary>
        internal bool IsComputed
        {
            get
            {
                if (m_path.Count == 0)
                {
                    return false;
                }
                else
                {
                    return RootEdmMember.IsStoreGeneratedComputed;
                }
            }
        }

        /// <summary>
        /// Returns the default value the slot represented by the path. If no default value is present, returns null.
        /// </summary>
        internal object DefaultValue
        {
            get
            {
                if (m_path.Count == 0)
                {
                    return null;
                }
                Facet facet;
                if (LeafEdmMember.TypeUsage.Facets.TryGetValue(EdmProviderManifest.DefaultValueFacetName, false, out facet))
                {
                    return facet.Value;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns true if slot represented by the path is part of a key.
        /// </summary>
        internal bool IsPartOfKey
        {
            get
            {
                if (m_path.Count == 0)
                {
                    return false;
                }
                return MetadataHelper.IsPartOfEntityTypeKey(LeafEdmMember);
            }
        }

        /// <summary>
        /// Returns true if slot represented by the path is nullable.
        /// </summary>
        internal bool IsNullable
        {
            get
            {
                if (m_path.Count == 0)
                {
                    return false;
                }
                return MetadataHelper.IsMemberNullable(LeafEdmMember);
            }
        }

        /// <summary>
        /// If path corresponds to an entity set (empty path) or an association end (<see cref="Extent"/> is as association set, and path length is 1), 
        /// returns <see cref="EntitySet"/> associated with the value of the slot represented by this path, otherwise returns null.
        /// </summary>
        internal EntitySet EntitySet
        {
            get
            {
                if (m_path.Count == 0)
                {
                    return m_extent as EntitySet;
                }
                else if (m_path.Count == 1)
                {
                    AssociationEndMember endMember = this.RootEdmMember as AssociationEndMember;
                    if (endMember != null)
                    {
                        EntitySet result = MetadataHelper.GetEntitySetAtEnd((AssociationSet)m_extent, endMember);
                        return result;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Extent of the path.
        /// </summary>
        internal EntitySetBase Extent
        {
            get { return m_extent; }
        }

        /// <summary>
        /// Returns the type of attribute denoted by the path. 
        /// For example, member type of Person.addr.zip would be integer. For extent, it is the element type.
        /// </summary>
        internal EdmType EdmType
        {
            get
            {
                if (m_path.Count > 0)
                {
                    return LeafEdmMember.TypeUsage.EdmType;
                }
                else
                {
                    return m_extent.ElementType;
                }
            }
        }

        /// <summary>
        /// Returns Cql field alias generated from the path items.
        /// </summary>
        internal string CqlFieldAlias
        {
            get
            {
                string alias = PathToString(true);
                if (false == alias.Contains("_"))
                {
                    // if alias of the member does not contain any "_", we can replace "." with "_" so that we can get a simple identifier.
                    alias = alias.Replace('.', '_');
                }
                StringBuilder builder = new StringBuilder();
                CqlWriter.AppendEscapedName(builder, alias);
                return builder.ToString();
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns false iff the path is 
        /// * A descendant of some nullable property
        /// * A descendant of an optional composition/collection
        /// * A descendant of a property that does not belong to the basetype/rootype of its parent.
        /// </summary>
        internal bool IsAlwaysDefined(Dictionary<EntityType, Set<EntityType>> inheritanceGraph)
        {
            if (m_path.Count == 0)
            {
                // Extents are always defined
                return true;
            }

            EdmMember member = m_path.Last();

            //Dont check last member, thats the property we are testing
            for (int i = 0; i < m_path.Count - 1; i++)
            {
                EdmMember current = m_path[i];
                // If member is nullable then "this" will not always be defined
                if (MetadataHelper.IsMemberNullable(current))
                {
                    return false;
                }
            }

            //Now check if there are any concrete types other than all subtypes of Type defining this member

            //by definition association types member are always present since they are IDs
            if (m_path[0].DeclaringType is AssociationType)
            {
                return true;
            }

            EntityType entitySetType = m_extent.ElementType as EntityType;
            if (entitySetType == null) //association type
            {
                return true;
            }

            //well, we handle the first case because we don't knwo how to get to subtype (i.e. the edge to avoid)
            EntityType memberDeclaringType = m_path[0].DeclaringType as EntityType;
            EntityType parentType = memberDeclaringType.BaseType as EntityType;


            if (entitySetType.EdmEquals(memberDeclaringType) || MetadataHelper.IsParentOf(memberDeclaringType, entitySetType) || parentType == null)
            {
                return true;
            }
            else if (!parentType.Abstract && !MetadataHelper.DoesMemberExist(parentType, member))
            {
                return false;
            }

            bool result = !RecurseToFindMemberAbsentInConcreteType(parentType, memberDeclaringType, member, entitySetType, inheritanceGraph);
            return result;
        }

        private static bool RecurseToFindMemberAbsentInConcreteType(EntityType current, EntityType avoidEdge, EdmMember member, EntityType entitySetType, Dictionary<EntityType, Set<EntityType>> inheritanceGraph)
        {
            Set<EntityType> edges = inheritanceGraph[current];

            //for each outgoing edge (from current) where the edge is not the one to avoid,
            // navigate depth-first
            foreach (var edge in edges.Where(type => !type.EdmEquals(avoidEdge)))
            {
                //Dont traverse above the EntitySet's Element type
                if (entitySetType.BaseType != null && entitySetType.BaseType.EdmEquals(edge))
                {
                    continue;
                }

                if (!edge.Abstract && !MetadataHelper.DoesMemberExist(edge, member))
                {
                    //found it.. I'm the concrete type that has member absent.
                    return true;
                }

                if (RecurseToFindMemberAbsentInConcreteType(edge, current /*avoid traversing down back here*/, member, entitySetType, inheritanceGraph))
                {
                    //one of the edges reachable from me found it
                    return true;
                }
            }
            //no body found this counter example
            return false;
        }

        /// <summary>
        /// Determines all the identifiers used in the path and adds them to <paramref name="identifiers"/>.
        /// </summary>
        internal void GetIdentifiers(CqlIdentifiers identifiers)
        {
            // Get the extent name and extent type name
            identifiers.AddIdentifier(m_extent.Name);
            identifiers.AddIdentifier(m_extent.ElementType.Name);
            foreach (EdmMember member in m_path)
            {
                identifiers.AddIdentifier(member.Name);
            }
        }

        /// <summary>
        /// Returns true iff all members are nullable properties, i.e., if even one of them is non-nullable, returns false.
        /// </summary>
        internal static bool AreAllMembersNullable(IEnumerable<MemberPath> members)
        {
            foreach (MemberPath path in members)
            {
                if (path.m_path.Count == 0)
                {
                    return false; // Extents are not nullable
                }
                if (path.IsNullable == false)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a string that has the list of properties in <paramref name="members"/> (i.e., just the last name) if <paramref name="fullPath"/> is false.
        /// Else the <paramref name="fullPath"/> is added.
        /// </summary>
        internal static string PropertiesToUserString(IEnumerable<MemberPath> members, bool fullPath)
        {
            bool isFirst = true;
            StringBuilder builder = new StringBuilder();
            foreach (MemberPath path in members)
            {
                if (isFirst == false)
                {
                    builder.Append(", ");
                }
                isFirst = false;
                if (fullPath)
                {
                    builder.Append(path.PathToString(false));
                }
                else
                {
                    builder.Append(path.LeafName);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Given a member path and an alias, returns an eSQL string correspondng to the fully-qualified name <paramref name="blockAlias"/>.path, e.g., T1.Address.Phone.Zip.
        /// If a subcomponent belongs to subclass, generates a treat for it, e.g. "TREAT(T1 as Customer).Address".
        /// Or even "TREAT(TREAT(T1 AS Customer).Address as USAddress).Zip".
        /// </summary>
        internal StringBuilder AsEsql(StringBuilder inputBuilder, string blockAlias)
        {
            // Due to the TREAT stuff, we cannot build incrementally.
            // So we use a local StringBuilder - it should not be that inefficient (one extra copy).
            StringBuilder builder = new StringBuilder();
            
            // Add blockAlias as a starting point for blockAlias.member1.member2...
            CqlWriter.AppendEscapedName(builder, blockAlias);

            // Process all items in the path.
            AsCql(
                // accessMember action
                (memberName) =>
                {
                    builder.Append('.');
                    CqlWriter.AppendEscapedName(builder, memberName);
                },
                // getKey action
                () =>
                {
                    builder.Insert(0, "Key(");
                    builder.Append(")");
                },
                // treatAs action
                (treatAsType) =>
                {
                    builder.Insert(0, "TREAT(");
                    builder.Append(" AS ");
                    CqlWriter.AppendEscapedTypeName(builder, treatAsType);
                    builder.Append(')');
                });

            inputBuilder.Append(builder.ToString());
            return inputBuilder;
        }

        internal DbExpression AsCqt(DbExpression row)
        {
            DbExpression cqt = row;

            // Process all items in the path.
            AsCql(
                // accessMember action
                (memberName) =>
                {
                    cqt = DbExpressionBuilder.Property(cqt, memberName);
                },
                // getKey action
                () =>
                {
                    cqt = cqt.GetRefKey();
                },
                // treatAs action
                (treatAsType) =>
                {
                    var typeUsage = TypeUsage.Create(treatAsType);
                    cqt = cqt.TreatAs(typeUsage);
                });

            return cqt;
        }

        internal void AsCql(Action<string> accessMember, Action getKey, Action<StructuralType> treatAs)
        {
            // Keep track of the previous type so that we can determine if we need to cast or not.
            EdmType prevType = m_extent.ElementType;

            foreach (EdmMember member in m_path)
            {
                // If prevType is a ref (e.g., ref to CPerson), we need to get the type that it is pointing to and then look for this member in that type.
                StructuralType prevStructuralType;
                RefType prevRefType;
                if (Helper.IsRefType(prevType))
                {
                    prevRefType = (RefType)prevType;
                    prevStructuralType = prevRefType.ElementType;
                }
                else
                {
                    prevRefType = null;
                    prevStructuralType = (StructuralType)prevType;
                }

                // Check whether the prevType has the present member in it.
                // If not, we will need to cast the prev type to the appropriate subtype.
                bool found = MetadataHelper.DoesMemberExist(prevStructuralType, member);

                if (prevRefType != null)
                {
                    // For reference types, the key must be present in the element type itself.
                    // E.g., if we have Ref(CPerson), the key must be present as CPerson.pid or CPerson.Address.Phone.Number (i.e., in a complex type).
                    // Note that it cannot be present in the subtype of address or phone either, i.e., this path better not have any TREATs.
                    // We are at CPerson right now. So if we say Key(CPerson), we will get a row with all the key elements.
                    // Then we can continue going down the path in CPerson

                    Debug.Assert(found == true, "We did not find the key property in a ref's element type - it cannot be in a subtype");
                    Debug.Assert(MetadataHelper.IsPartOfEntityTypeKey(member), "Member is expected to be a key property");

                    // Emit KEY(current path segment)
                    getKey();
                }
                else if (false == found)
                {
                    // Need to add Treat(... as ...) expression in the beginning.
                    // Note that it does handle cases like TREAT(TREAT(T1 AS Customer).Address as USAddress).Zip

                    Debug.Assert(prevRefType == null, "We do not allow subtyping in key extraction from Refs");

                    // Emit TREAT(current path segment as member.DeclaringType)
                    treatAs(member.DeclaringType);
                }

                // Add the member's access. We had a path "T1.A.B" till now.
                accessMember(member.Name);

                prevType = member.TypeUsage.EdmType;
            }
        }

        public bool Equals(MemberPath right)
        {
            return EqualityComparer.Equals(this, right);
        }

        public override bool Equals(object obj)
        {
            MemberPath right = obj as MemberPath;
            if (obj == null)
            {
                return false;
            }
            return Equals(right);
        }

        public override int GetHashCode()
        {
            return EqualityComparer.GetHashCode(this);
        }

        /// <summary>
        /// Returns true if the member denoted by the path corresponds to a scalar (primitive or enum).
        /// </summary>
        internal bool IsScalarType()
        {
            return this.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType ||
                   this.EdmType.BuiltInTypeKind == BuiltInTypeKind.EnumType;
        }

        internal static IEnumerable<MemberPath> GetKeyMembers(EntitySetBase extent, MemberDomainMap domainMap)
        {
            MemberPath extentPath = new MemberPath(extent);
            List<MemberPath> keyAttributes = new List<MemberPath>(
                extentPath.GetMembers(extentPath.Extent.ElementType, null /* isScalar */, null /* isConditional */, true /* isPartOfKey */, domainMap));
            Debug.Assert(keyAttributes.Any(), "No key attributes?");
            return keyAttributes;
        }

        internal IEnumerable<MemberPath> GetMembers(EdmType edmType, bool? isScalar, bool? isConditional, bool? isPartOfKey, MemberDomainMap domainMap)
        {
            MemberPath currentPath = this;
            StructuralType structuralType = (StructuralType)edmType;
            foreach (EdmMember edmMember in structuralType.Members)
            {
                if (edmMember is AssociationEndMember)
                {
                    // get end's keys
                    foreach (MemberPath endKey in new MemberPath(currentPath, edmMember).GetMembers(
                                                         ((RefType)edmMember.TypeUsage.EdmType).ElementType,
                                                         isScalar, isConditional, true /*isPartOfKey*/, domainMap))
                    {
                        yield return endKey;
                    }
                }
                bool isActuallyScalar = MetadataHelper.IsNonRefSimpleMember(edmMember);
                if (isScalar == null || isScalar == isActuallyScalar)
                {
                    EdmProperty childProperty = edmMember as EdmProperty;
                    if (childProperty != null)
                    {
                        bool isActuallyKey = MetadataHelper.IsPartOfEntityTypeKey(childProperty);
                        if (isPartOfKey == null || isPartOfKey == isActuallyKey)
                        {
                            MemberPath childPath = new MemberPath(currentPath, childProperty);
                            bool isActuallyConditional = domainMap.IsConditionMember(childPath);
                            if (isConditional == null || isConditional == isActuallyConditional)
                            {
                                yield return childPath;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if this path and <paramref name="path1"/> are equivalent on the C-side via a referential constraint.
        /// </summary>
        internal bool IsEquivalentViaRefConstraint(MemberPath path1)
        {
            MemberPath path0 = this;

            // Now check if they are equivalent via referential constraint

            // For example,
            // * Person.pid and PersonAddress.Person.pid are equivalent
            // * Person.pid and PersonAddress.Address.pid are equivalent
            // * Person.pid and Address.pid are equivalent if there is a referential constraint
            // * PersonAddress.Person.pid and PersonAddress.Address.pid are
            //   equivalent if there is a referential constraint

            // In short, Person.pid, Address.pid, PersonAddress.Address.pid,
            // PersonAddress.Person.pid are the same

            if (path0.EdmType is EntityTypeBase || path1.EdmType is EntityTypeBase ||
                MetadataHelper.IsNonRefSimpleMember(path0.LeafEdmMember) == false ||
                MetadataHelper.IsNonRefSimpleMember(path1.LeafEdmMember) == false)
            {
                // If the path corresponds to a top level extent only, ignore
                // it. Or if it is not a scalar
                return false;
            }

            AssociationSet assocSet0 = path0.Extent as AssociationSet;
            AssociationSet assocSet1 = path1.Extent as AssociationSet;
            EntitySet entitySet0 = path0.Extent as EntitySet;
            EntitySet entitySet1 = path1.Extent as EntitySet;
            bool result = false;

            if (assocSet0 != null && assocSet1 != null)
            {
                // PersonAddress.Person.pid and PersonAddress.Address.pid case
                // Check if they are the same association or not
                if (assocSet0.Equals(assocSet1) == false)
                {
                    return false;
                }
                result = AreAssocationEndPathsEquivalentViaRefConstraint(path0, path1, assocSet0);
            }
            else if (entitySet0 != null && entitySet1 != null)
            {
                // Person.pid, Address.pid case
                // Find all the associations between the two sets. If the
                // fields are equivalent via any association + referential
                // constraint, return true
                List<AssociationSet> assocSets = MetadataHelper.GetAssociationsForEntitySets(entitySet0, entitySet1);
                foreach (AssociationSet assocSet in assocSets)
                {
                    // For Person.pid, get PersonAddress.Person.pid or
                    MemberPath assocEndPath0 = path0.GetCorrespondingAssociationPath(assocSet);
                    MemberPath assocEndPath1 = path1.GetCorrespondingAssociationPath(assocSet);
                    if (AreAssocationEndPathsEquivalentViaRefConstraint(assocEndPath0, assocEndPath1, assocSet))
                    {
                        result = true;
                        break;
                    }
                }
            }
            else
            {
                // One of them is an assocSet and the other is an entity set
                AssociationSet assocSet = assocSet0 != null ? assocSet0 : assocSet1;
                EntitySet entitySet = entitySet0 != null ? entitySet0 : entitySet1;
                Debug.Assert(assocSet != null && entitySet != null,
                             "One set must be association and the other must be entity set");

                MemberPath assocEndPathA = path0.Extent is AssociationSet ? path0 : path1;
                MemberPath entityPath = path0.Extent is EntitySet ? path0 : path1;
                MemberPath assocEndPathB = entityPath.GetCorrespondingAssociationPath(assocSet);
                if (assocEndPathB == null)
                {
                    //An EntitySet might participate in multiple AssociationSets
                    //and this might not be the association set that defines the expected referential
                    //constraint
                    //Return false since this does not have any referential constraint specified
                    result = false;
                }
                else
                {
                    result = AreAssocationEndPathsEquivalentViaRefConstraint(assocEndPathA, assocEndPathB, assocSet);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if <paramref name="assocPath0"/> and <paramref name="assocPath1"/> are equivalent via a referential constraint in <paramref name="assocSet"/>.
        /// Requires: <paramref name="assocPath0"/> and <paramref name="assocPath1"/> correspond to paths in <paramref name="assocSet"/>.
        /// </summary>
        private static bool AreAssocationEndPathsEquivalentViaRefConstraint(MemberPath assocPath0,
                                                                            MemberPath assocPath1,
                                                                            AssociationSet assocSet)
        {
            Debug.Assert(assocPath0.Extent.Equals(assocSet) && assocPath1.Extent.Equals(assocSet),
                         "Extent for paths must be assocSet");

            AssociationEndMember end0 = assocPath0.RootEdmMember as AssociationEndMember;
            AssociationEndMember end1 = assocPath1.RootEdmMember as AssociationEndMember;
            EdmProperty property0 = assocPath0.LeafEdmMember as EdmProperty;
            EdmProperty property1 = assocPath1.LeafEdmMember as EdmProperty;

            if (end0 == null || end1 == null || property0 == null || property1 == null)
            {
                return false;
            }

            // Now check if these fields are connected via a referential constraint
            AssociationType assocType = assocSet.ElementType;
            bool foundConstraint = false;

            foreach (ReferentialConstraint constraint in assocType.ReferentialConstraints)
            {
                bool isFrom0 = end0.Name == constraint.FromRole.Name &&
                    end1.Name == constraint.ToRole.Name;
                bool isFrom1 = end1.Name == constraint.FromRole.Name &&
                    end0.Name == constraint.ToRole.Name;

                if (isFrom0 || isFrom1)
                {
                    // Found an RI for the two sets. Make sure that the properties are at the same ordinal

                    // isFrom0 is true when end0 corresponds to FromRole and end1 to ToRole
                    ReadOnlyMetadataCollection<EdmProperty> properties0 = isFrom0 ? constraint.FromProperties : constraint.ToProperties;
                    ReadOnlyMetadataCollection<EdmProperty> properties1 = isFrom0 ? constraint.ToProperties : constraint.FromProperties;
                    int indexForPath0 = properties0.IndexOf(property0);
                    int indexForPath1 = properties1.IndexOf(property1);
                    if (indexForPath0 == indexForPath1 && indexForPath0 != -1)
                    {
                        foundConstraint = true;
                        break;
                    }
                }
            }
            return foundConstraint;
        }

        /// <summary>
        /// Returns the member path corresponding to that field in the <paramref name="assocSet"/>. E.g., given Address.pid, returns PersonAddress.Address.pid.
        /// For self-associations, such as ManagerEmployee with referential constraints (and we have 
        /// [ManagerEmployee.Employee.mid, ManagerEmployee.Employee.eid, ManagerEmployee.Manager.mid]), given Employee.mid, returns
        /// ManagerEmployee.Employee.mid or ManagerEmployee.Manager.mid
        /// 
        /// Note: the path need not correspond to a key field of an entity set <see cref="Extent"/>.
        /// </summary>
        private MemberPath GetCorrespondingAssociationPath(AssociationSet assocSet)
        {
            Debug.Assert(this.Extent is EntitySet, "path must be in the context of an entity set");

            // Find the end corresponding to the entity set
            AssociationEndMember end = MetadataHelper.GetSomeEndForEntitySet(assocSet, (EntitySet)m_extent);
            // An EntitySet might participate in multiple AssociationSets and
            // this might not be the association set that defines the expected referential constraint.
            if (end == null)
            {
                return null;
            }
            // Create the new members using the end
            List<EdmMember> newMembers = new List<EdmMember>();
            newMembers.Add(end);
            newMembers.AddRange(m_path);
            // The extent is the assocSet
            MemberPath result = new MemberPath(assocSet, newMembers);
            return result;
        }

        /// <summary>
        /// If member path identifies a relationship end, return its scope. Otherwise, returns null.
        /// </summary>
        internal EntitySet GetScopeOfRelationEnd()
        {
            if (m_path.Count == 0)
            {
                return null;
            }

            AssociationEndMember relationEndMember = LeafEdmMember as AssociationEndMember;
            if (relationEndMember == null)
            {
                return null;
            }

            // Yes, it's a reference, determine its entity set refScope
            AssociationSet associationSet = (AssociationSet)m_extent;
            EntitySet result = MetadataHelper.GetEntitySetAtEnd(associationSet, relationEndMember);
            return result;
        }

        /// <summary>
        /// Returns a string of the form "a.b.c" that corresponds to the items in the path. This string can be used for tests or localization.
        /// If <paramref name="forAlias"/>=true, we return a string that is relevant for Cql aliases, else we return the exact path.
        /// </summary>
        internal string PathToString(bool? forAlias)
        {
            StringBuilder builder = new StringBuilder();

            if (forAlias != null)
            {
                if (forAlias == true)
                {
                    // For the 0th entry, we just choose the type of the element in
                    // which the first entry belongs, e.g., if Addr belongs to CCustomer,
                    // we choose CCustomer and not CPerson. 
                    if (m_path.Count == 0)
                    {
                        EntityTypeBase type = m_extent.ElementType;
                        return type.Name;
                    }
                    builder.Append(m_path[0].DeclaringType.Name); // Get CCustomer here
                }
                else
                {
                    // Append the extent name
                    builder.Append(m_extent.Name);
                }
            }

            // Just join the path using "."
            for (int i = 0; i < m_path.Count; i++)
            {
                builder.Append('.');
                builder.Append(m_path[i].Name);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Returns a human-readable string corresponding to the path.
        /// </summary>
        internal override void ToCompactString(StringBuilder builder)
        {
            builder.Append(PathToString(false));
        }

        internal void ToCompactString(StringBuilder builder, string instanceToken)
        {
            builder.Append(instanceToken + PathToString(null));
        }
        #endregion

        #region Comparer
        private sealed class Comparer : IEqualityComparer<MemberPath>
        {
            public bool Equals(MemberPath left, MemberPath right)
            {
                if (object.ReferenceEquals(left, right))
                {
                    return true;
                }
                // One of them is non-null at least. So if the other one is
                // null, we cannot be equal
                if (left == null || right == null)
                {
                    return false;
                }
                // Both are non-null at this point
                // Checks that the paths are equal component-wise
                if (left.m_extent.Equals(right.m_extent) == false || left.m_path.Count != right.m_path.Count)
                {
                    return false;
                }

                for (int i = 0; i < left.m_path.Count; i++)
                {
                    // Comparing MemberMetadata -- can use Equals
                    if (false == left.m_path[i].Equals(right.m_path[i]))
                    {
                        return false;
                    }
                }
                return true;
            }

            public int GetHashCode(MemberPath key)
            {
                int result = key.m_extent.GetHashCode();
                foreach (EdmMember member in key.m_path)
                {
                    result ^= member.GetHashCode();
                }
                return result;
            }
        }
        #endregion
    }
}
