//---------------------------------------------------------------------
// <copyright file="AssociationSetMetadata.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Data.Metadata.Edm;
using System.Data.Common.Utils;
using System.Data.Common.CommandTrees;
using System.Collections.Generic;
using System.Linq;
namespace System.Data.Mapping.Update.Internal
{
    /// <summary>
    /// Encapsulates information about ends of an association set needed to correctly
    /// interpret updates.
    /// </summary>
    internal sealed class AssociationSetMetadata
    {
        /// <summary>
        /// Gets association ends that must be modified if the association
        /// is changed (e.g. the mapping of the association is conditioned
        /// on some property of the end)
        /// </summary>
        internal readonly Set<AssociationEndMember> RequiredEnds;
        /// <summary>
        /// Gets association ends that may be implicitly modified as a result
        /// of changes to the association (e.g. collocated entity with server
        /// generated value)
        /// </summary>
        internal readonly Set<AssociationEndMember> OptionalEnds;
        /// <summary>
        /// Gets association ends whose values may influence the association
        /// (e.g. where there is a ReferentialIntegrity or "foreign key" constraint)
        /// </summary>
        internal readonly Set<AssociationEndMember> IncludedValueEnds;
        /// <summary>
        /// true iff. there are interesting ends for this association set.
        /// </summary>
        internal bool HasEnds
        {
            get { return 0 < RequiredEnds.Count || 0 < OptionalEnds.Count || 0 < IncludedValueEnds.Count; }
        }

        /// <summary>
        /// Initialize Metadata for an AssociationSet
        /// </summary>
        internal AssociationSetMetadata(Set<EntitySet> affectedTables, AssociationSet associationSet, MetadataWorkspace workspace)
        {
            // If there is only 1 table, there can be no ambiguity about the "destination" of a relationship, so such
            // sets are not typically required.
            bool isRequired = 1 < affectedTables.Count;

            // determine the ends of the relationship
            var ends = associationSet.AssociationSetEnds;

            // find collocated entities
            foreach (EntitySet table in affectedTables)
            {
                // Find extents influencing the table
                var influencingExtents = MetadataHelper.GetInfluencingEntitySetsForTable(table, workspace);
               
                foreach (EntitySet influencingExtent in influencingExtents)
                {
                    foreach (var end in ends)
                    {
                        // If the extent is an end of the relationship and we haven't already added it to the
                        // required set...
                        if (end.EntitySet.EdmEquals(influencingExtent))
                        {
                            if (isRequired)
                            {
                                AddEnd(ref RequiredEnds, end.CorrespondingAssociationEndMember);
                            }
                            else if (null == RequiredEnds || !RequiredEnds.Contains(end.CorrespondingAssociationEndMember))
                            {
                                AddEnd(ref OptionalEnds, end.CorrespondingAssociationEndMember);
                            }
                        }
                    }
                }
            }

            // fix Required and Optional sets
            FixSet(ref RequiredEnds);
            FixSet(ref OptionalEnds);

            // for associations with referential constraints, the principal end is always interesting
            // since its key values may take precedence over the key values of the dependent end
            foreach (ReferentialConstraint constraint in associationSet.ElementType.ReferentialConstraints)
            {
                // FromRole is the principal end in the referential constraint
                AssociationEndMember principalEnd = (AssociationEndMember)constraint.FromRole;

                if (!RequiredEnds.Contains(principalEnd) &&
                    !OptionalEnds.Contains(principalEnd))
                {
                    AddEnd(ref IncludedValueEnds, principalEnd);
                }
            }

            FixSet(ref IncludedValueEnds);
        }

        /// <summary>
        /// Initialize given required ends. 
        /// </summary>
        internal AssociationSetMetadata(IEnumerable<AssociationEndMember> requiredEnds)
        {
            if (requiredEnds.Any())
            {
                RequiredEnds = new Set<AssociationEndMember>(requiredEnds);
            }
            FixSet(ref RequiredEnds);
            FixSet(ref OptionalEnds);
            FixSet(ref IncludedValueEnds);
        }
        
        static private void AddEnd(ref Set<AssociationEndMember> set, AssociationEndMember element)
        {
            if (null == set)
            {
                set = new Set<AssociationEndMember>();
            }
            set.Add(element);
        }

        static private void FixSet(ref Set<AssociationEndMember> set)
        {
            if (null == set)
            {
                set = Set<AssociationEndMember>.Empty;
            }
            else
            {
                set.MakeReadOnly();
            }
        }
    }
}
