//---------------------------------------------------------------------
// <copyright file="RelPropertyHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.Data.Common;
using System.Data.Metadata.Edm;

namespace System.Data.Query.InternalTrees
{

    /// <summary>
    /// A "Rel" property is best thought of as a collocated reference (aka foreign key). 
    /// Any entity may have zero or more rel-properties carried along with it (purely
    /// as a means to optimize for common relationship traversal scenarios)
    /// 
    /// Although the definition is lax here, we only deal with RelProperties that
    /// are one-ended (ie) the target multiplicity is at most One.
    /// 
    /// Consider for example, an Order entity with a (N:1) Order-Customer relationship. The Customer ref
    /// will be treated as a rel property for the Order entity. 
    /// Similarly, the OrderLine entity may have an Order ref rel property (assuming that there was 
    /// a N:1 relationship between OrderLine and Order)
    /// </summary>
    internal sealed class RelProperty
    {
        #region private state
        private readonly RelationshipType m_relationshipType;
        private readonly RelationshipEndMember m_fromEnd;
        private readonly RelationshipEndMember m_toEnd;
        #endregion

        #region constructors
        internal RelProperty(RelationshipType relationshipType, RelationshipEndMember fromEnd, RelationshipEndMember toEnd)
        {
            m_relationshipType = relationshipType;
            m_fromEnd = fromEnd;
            m_toEnd = toEnd;
        }
        #endregion

        #region public APIs
        /// <summary>
        /// The relationship
        /// </summary>
        public RelationshipType Relationship { get { return m_relationshipType; } }

        /// <summary>
        /// The source end of the relationship
        /// </summary>
        public RelationshipEndMember FromEnd { get { return m_fromEnd; } }

        /// <summary>
        /// the target end of the relationship
        /// </summary>
        public RelationshipEndMember ToEnd { get { return m_toEnd; } }

        /// <summary>
        /// Our definition of equality
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            RelProperty other = obj as RelProperty;
            return (other != null &&
                this.Relationship.EdmEquals(other.Relationship) &&
                this.FromEnd.EdmEquals(other.FromEnd) &&
                this.ToEnd.EdmEquals(other.ToEnd));
        }

        /// <summary>
        /// our hash code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.ToEnd.Identity.GetHashCode();
        }

        /// <summary>
        /// String form
        /// </summary>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public override string ToString()
        {
            return m_relationshipType.ToString() + ":" +
                m_fromEnd.ToString() + ":" +
                m_toEnd.ToString();
        }

        #endregion
    }

    /// <summary>
    /// A helper class for all rel-properties
    /// </summary>
    internal sealed class RelPropertyHelper
    {
        #region private state
        private Dictionary<EntityTypeBase, List<RelProperty>> _relPropertyMap;
        private HashSet<RelProperty> _interestingRelProperties;
        #endregion

        #region private methods
        /// <summary>
        /// Add the rel property induced by the specified relationship, (if the target
        /// end has a multiplicity of one)
        /// We only keep track of rel-properties that are "interesting" 
        /// </summary>
        /// <param name="associationType">the association relationship</param>
        /// <param name="fromEnd">source end of the relationship traversal</param>
        /// <param name="toEnd">target end of the traversal</param>
        private void AddRelProperty(AssociationType associationType,
            AssociationEndMember fromEnd, AssociationEndMember toEnd)
        {
            if (toEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many)
            {
                return;
            }
            RelProperty prop = new RelProperty(associationType, fromEnd, toEnd);
            if (_interestingRelProperties == null ||
                !_interestingRelProperties.Contains(prop))
            {
                return;
            }

            EntityTypeBase entityType = (EntityTypeBase)((RefType)fromEnd.TypeUsage.EdmType).ElementType;
            List<RelProperty> propList;
            if (!_relPropertyMap.TryGetValue(entityType, out propList))
            {
                propList = new List<RelProperty>();
                _relPropertyMap[entityType] = propList;
            }
            propList.Add(prop);
        }

        /// <summary>
        /// Add any rel properties that are induced by the supplied relationship
        /// </summary>
        /// <param name="relationshipType">the relationship</param>
        private void ProcessRelationship(RelationshipType relationshipType)
        {
            AssociationType associationType = relationshipType as AssociationType;
            if (associationType == null)
            {
                return;
            }

            // Handle only binary associations
            if (associationType.AssociationEndMembers.Count != 2)
            {
                return;
            }

            AssociationEndMember end0 = associationType.AssociationEndMembers[0];
            AssociationEndMember end1 = associationType.AssociationEndMembers[1];

            AddRelProperty(associationType, end0, end1);
            AddRelProperty(associationType, end1, end0);
        }

        #endregion

        #region constructors
        internal RelPropertyHelper(MetadataWorkspace ws, HashSet<RelProperty> interestingRelProperties)
        {
            _relPropertyMap = new Dictionary<EntityTypeBase, List<RelProperty>>();
            _interestingRelProperties = interestingRelProperties;

            foreach (RelationshipType relationshipType in ws.GetItems<RelationshipType>(DataSpace.CSpace))
            {
                ProcessRelationship(relationshipType);
            }
        }
        #endregion

        #region public APIs

        /// <summary>
        /// Get the rel properties declared by this type (and *not* by any of its subtypes)
        /// </summary>
        /// <param name="entityType">the entity type</param>
        /// <returns>set of rel properties declared for this type</returns>
        internal IEnumerable<RelProperty> GetDeclaredOnlyRelProperties(EntityTypeBase entityType)
        {
            List<RelProperty> relProperties;
            if (_relPropertyMap.TryGetValue(entityType, out relProperties))
            {
                foreach (RelProperty p in relProperties)
                {
                    yield return p;
                }
            }
            yield break;
        }

        /// <summary>
        /// Get the rel-properties of this entity and its supertypes (starting from the root)
        /// </summary>
        /// <param name="entityType">the entity type</param>
        /// <returns>set of rel-properties for this entity type (and its supertypes)</returns>
        internal IEnumerable<RelProperty> GetRelProperties(EntityTypeBase entityType)
        {
            if (entityType.BaseType != null)
            {
                foreach (RelProperty p in GetRelProperties(entityType.BaseType as EntityTypeBase))
                {
                    yield return p;
                }
            }

            foreach (RelProperty p in GetDeclaredOnlyRelProperties(entityType))
            {
                yield return p;
            }

        }

        #endregion
    }

}
