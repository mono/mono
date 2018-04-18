//---------------------------------------------------------------------
// <copyright file="ConstraintManager.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data.Common;
using System.Data.Query.InternalTrees;
using md=System.Data.Metadata.Edm;
//using System.Diagnostics; // Please use PlanCompiler.Assert instead of Debug.Assert in this class...

// It is fine to use Debug.Assert in cases where you assert an obvious thing that is supposed
// to prevent from simple mistakes during development (e.g. method argument validation 
// in cases where it was you who created the variables or the variables had already been validated or 
// in "else" clauses where due to code changes (e.g. adding a new value to an enum type) the default 
// "else" block is chosen why the new condition should be treated separately). This kind of asserts are 
// (can be) helpful when developing new code to avoid simple mistakes but have no or little value in 
// the shipped product. 
// PlanCompiler.Assert *MUST* be used to verify conditions in the trees. These would be assumptions 
// about how the tree was built etc. - in these cases we probably want to throw an exception (this is
// what PlanCompiler.Assert does when the condition is not met) if either the assumption is not correct 
// or the tree was built/rewritten not the way we thought it was.
// Use your judgment - if you rather remove an assert than ship it use Debug.Assert otherwise use
// PlanCompiler.Assert.

//
// The ConstraintManager module manages foreign key constraints for a query. It reshapes
// referential constraints supplied by metadata into a more useful form.
//
namespace System.Data.Query.PlanCompiler
{
    /// <summary>
    /// A simple class that represents a pair of extents
    /// </summary>
    internal class ExtentPair
    {
        #region public surface
        /// <summary>
        /// Return the left component of the pair
        /// </summary>
        internal md.EntitySetBase Left { get { return m_left; } }

        /// <summary>
        /// Return the right component of the pair
        /// </summary>
        internal md.EntitySetBase Right { get { return m_right; } }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            ExtentPair other = obj as ExtentPair;
            return (other != null) && other.Left.Equals(this.Left) && other.Right.Equals(this.Right);
        }

        /// <summary>
        /// Hashcode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (this.Left.GetHashCode() << 4) ^ this.Right.GetHashCode();
        }
        #endregion

        #region constructors
        internal ExtentPair(md.EntitySetBase left, md.EntitySetBase right)
        {
            m_left = left;
            m_right = right;
        }
        #endregion

        #region private state
        private md.EntitySetBase m_left;
        private md.EntitySetBase m_right;
        #endregion
    }

    /// <summary>
    /// Information about a foreign-key constraint
    /// </summary>
    internal class ForeignKeyConstraint
    {
        #region public surface

        /// <summary>
        /// Parent key properties
        /// </summary>
        internal List<string> ParentKeys { get { return m_parentKeys; } }
        /// <summary>
        /// Child key properties
        /// </summary>
        internal List<string> ChildKeys { get { return m_childKeys; } }

        /// <summary>
        /// Get the parent-child pair
        /// </summary>
        internal ExtentPair Pair { get { return m_extentPair; } }

        /// <summary>
        /// Return the child rowcount
        /// </summary>
        internal md.RelationshipMultiplicity ChildMultiplicity { get { return m_constraint.ToRole.RelationshipMultiplicity; } }

        /// <summary>
        /// Get the corresponding parent (key) property, for a specific child (foreign key) property
        /// </summary>
        /// <param name="childPropertyName">child (foreign key) property name</param>
        /// <param name="parentPropertyName">corresponding parent property name</param>
        /// <returns>true, if the parent property was found</returns>
        internal bool GetParentProperty(string childPropertyName, out string parentPropertyName)
        {
            BuildKeyMap();
            return m_keyMap.TryGetValue(childPropertyName, out parentPropertyName);
        }
        #endregion

        #region constructors
        internal ForeignKeyConstraint(md.RelationshipType relType, md.RelationshipSet relationshipSet, md.ReferentialConstraint constraint)
        {
            md.AssociationSet assocSet = relationshipSet as md.AssociationSet;
            md.AssociationEndMember fromEnd = constraint.FromRole as md.AssociationEndMember;
            md.AssociationEndMember toEnd = constraint.ToRole as md.AssociationEndMember;

            // Currently only Associations are supported
            if (null == assocSet || null == fromEnd || null == toEnd)
            {
                throw EntityUtil.NotSupported();
            }

            m_constraint = constraint;
            md.EntitySet parent = System.Data.Common.Utils.MetadataHelper.GetEntitySetAtEnd(assocSet, fromEnd);// relationshipSet.GetRelationshipEndExtent(constraint.FromRole);
            md.EntitySet child = System.Data.Common.Utils.MetadataHelper.GetEntitySetAtEnd(assocSet, toEnd);// relationshipSet.GetRelationshipEndExtent(constraint.ToRole);
            m_extentPair = new ExtentPair(parent, child);
            m_childKeys = new List<string>();
            foreach (md.EdmProperty prop in constraint.ToProperties)
            {
                m_childKeys.Add(prop.Name);
            }

            m_parentKeys = new List<string>();
            foreach (md.EdmProperty prop in constraint.FromProperties)
            {
                m_parentKeys.Add(prop.Name);
            }

            PlanCompiler.Assert((md.RelationshipMultiplicity.ZeroOrOne == fromEnd.RelationshipMultiplicity || md.RelationshipMultiplicity.One == fromEnd.RelationshipMultiplicity), "from-end of relationship constraint cannot have multiplicity greater than 1");
        }
        #endregion

        #region private state
        private ExtentPair m_extentPair;
        private List<string> m_parentKeys;
        private List<string> m_childKeys;
        private md.ReferentialConstraint m_constraint;
        private Dictionary<string, string> m_keyMap;
        #endregion

        #region private methods

        /// <summary>
        /// Build up an equivalence map of primary keys and foreign keys (ie) for each
        /// foreign key column, identify the corresponding primary key property
        /// </summary>
        private void BuildKeyMap()
        {
            if (m_keyMap != null)
            {
                return;
            }

            m_keyMap = new Dictionary<string, string>();
            IEnumerator<md.EdmProperty> parentProps = m_constraint.FromProperties.GetEnumerator();
            IEnumerator<md.EdmProperty> childProps = m_constraint.ToProperties.GetEnumerator();
            while (true)
            {
                bool parentOver = !parentProps.MoveNext();
                bool childOver = !childProps.MoveNext();
                PlanCompiler.Assert(parentOver == childOver, "key count mismatch");
                if (parentOver)
                {
                    break;
                }
                m_keyMap[childProps.Current.Name] = parentProps.Current.Name;
            }
        }
        #endregion
    }

    /// <summary>
    /// Keeps track of all foreign key relationships
    /// </summary>
    internal class ConstraintManager
    {
        #region public methods
        /// <summary>
        /// Is there a parent child relationship between table1 and table2 ?
        /// </summary>
        /// <param name="table1">parent table ?</param>
        /// <param name="table2">child table ?</param>
        /// <param name="constraints">list of constraints ?</param>
        /// <returns>true if there is at least one constraint</returns>
        internal bool IsParentChildRelationship(md.EntitySetBase table1, md.EntitySetBase table2,
            out List<ForeignKeyConstraint> constraints)
        {
            LoadRelationships(table1.EntityContainer);
            LoadRelationships(table2.EntityContainer);

            ExtentPair extentPair = new ExtentPair(table1, table2);
            return m_parentChildRelationships.TryGetValue(extentPair, out constraints);
        }

        /// <summary>
        /// Load all relationships in this entity container
        /// </summary>
        /// <param name="entityContainer"></param>
        internal void LoadRelationships(md.EntityContainer entityContainer)
        {
            // Check to see if I've already loaded information for this entity container
            if (m_entityContainerMap.ContainsKey(entityContainer))
            {
                return;
            }

            // Load all relationships from this entitycontainer
            foreach (md.EntitySetBase e in entityContainer.BaseEntitySets)
            {
                md.RelationshipSet relationshipSet = e as md.RelationshipSet;
                if (relationshipSet == null)
                {
                    continue;
                }

                // Relationship sets can only contain relationships
                md.RelationshipType relationshipType = (md.RelationshipType)relationshipSet.ElementType;
                md.AssociationType assocType = relationshipType as md.AssociationType;

                //
                // Handle only binary Association relationships for now
                //
                if (null == assocType || !IsBinary(relationshipType))
                {
                    continue;
                }

                foreach (md.ReferentialConstraint constraint in assocType.ReferentialConstraints)
                {
                    List<ForeignKeyConstraint> fkConstraintList;
                    ForeignKeyConstraint fkConstraint = new ForeignKeyConstraint(relationshipType, relationshipSet, constraint);
                    if (!m_parentChildRelationships.TryGetValue(fkConstraint.Pair, out fkConstraintList))
                    {
                        fkConstraintList = new List<ForeignKeyConstraint>();
                        m_parentChildRelationships[fkConstraint.Pair] = fkConstraintList;
                    }
                    //
                    // Theoretically, we can have more than one fk constraint between
                    // the 2 tables (though, it is unlikely)
                    //
                    fkConstraintList.Add(fkConstraint);
                }
            }

            // Mark this entity container as already loaded
            m_entityContainerMap[entityContainer] = entityContainer;
        }
        #endregion

        #region constructors
        internal ConstraintManager()
        {
            m_entityContainerMap = new Dictionary<md.EntityContainer, md.EntityContainer>();
            m_parentChildRelationships = new Dictionary<ExtentPair, List<ForeignKeyConstraint>>();
        }
        #endregion

        #region private state
        private Dictionary<md.EntityContainer, md.EntityContainer> m_entityContainerMap;
        private Dictionary<ExtentPair, List<ForeignKeyConstraint>> m_parentChildRelationships;
        #endregion

        #region private methods

        /// <summary>
        /// Is this relationship a binary relationship (ie) does it have exactly 2 end points?
        /// 
        /// This should ideally be a method supported by RelationType itself
        /// </summary>
        /// <param name="relationshipType"></param>
        /// <returns>true, if this is a binary relationship</returns>
        private static bool IsBinary(md.RelationshipType relationshipType)
        {
            int endCount = 0;
            foreach(md.EdmMember member in relationshipType.Members)
            {
                if (member is md.RelationshipEndMember)
                {
                    endCount++;
                    if (endCount > 2)
                    {
                        return false;
                    }
                }
            }
            return (endCount == 2);
        }
        #endregion
    }
}
