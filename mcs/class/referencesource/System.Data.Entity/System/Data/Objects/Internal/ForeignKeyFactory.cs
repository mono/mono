//---------------------------------------------------------------------
// <copyright file="ForeignKeyFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Data.Objects.DataClasses;

namespace System.Data.Objects.Internal
{
    internal class ForeignKeyFactory
    {
        private const string s_NullPart = "EntityHasNullForeignKey";
        private const string s_NullForeignKey = "EntityHasNullForeignKey.EntityHasNullForeignKey";

        /// <summary>
        /// Returns true if the supplied key represents a Conceptual Null
        /// </summary>
        /// <param name="key">The key to be checked</param>
        public static bool IsConceptualNullKey(EntityKey key)
        {
            if (key == null)
            {
                return false;
            }

            return string.Equals(key.EntityContainerName, s_NullPart) &&
                   string.Equals(key.EntitySetName, s_NullPart);
        }

        /// <summary>
        /// Checks if the Real Key represents different FK values 
        /// than those present when the Conceptual Null was created
        /// </summary>
        /// <param name="conceptualNullKey">The key representing the Conceptual Null</param>
        /// <param name="realKey">The key to be checked</param>
        /// <returns>True if the values are different, false otherwise</returns>
        public static bool IsConceptualNullKeyChanged(EntityKey conceptualNullKey, EntityKey realKey)
        {
            Debug.Assert(IsConceptualNullKey(conceptualNullKey), "The key supplied is not a null key");

            if (realKey == null)
            {
                return true;
            }

            return !EntityKey.InternalEquals(conceptualNullKey, realKey, compareEntitySets: false); 
        }

        /// <summary>
        /// Creates an EntityKey that represents a Conceptual Null
        /// </summary>
        /// <param name="originalKey">An EntityKey representing the existing FK values that could not be nulled</param>
        /// <returns>EntityKey marked as a conceptual null with the FK values from the original key</returns>
        public static EntityKey CreateConceptualNullKey(EntityKey originalKey)
        {
            Debug.Assert(originalKey != null, "Original key can not be null");

            //Conceptual nulls have special entity set name and a copy of the previous values
            EntityKey nullKey = new EntityKey(s_NullForeignKey, originalKey.EntityKeyValues);
            return nullKey;
        }

        /// <summary>
        /// Creates an EntityKey for a principal entity based on the foreign key values contained
        /// in this entity.  This implies that this entity is at the dependent end of the relationship.
        /// </summary>
        /// <param name="dependentEntry">The EntityEntry for the dependent that contains the FK</param>
        /// <param name="relatedEnd">Identifies the principal end for which a key is required</param>
        /// <returns>The key, or null if any value in the key is null</returns>
        public static EntityKey CreateKeyFromForeignKeyValues(EntityEntry dependentEntry, RelatedEnd relatedEnd)
        {
            // Note: there is only ever one constraint per association type
            ReferentialConstraint constraint = ((AssociationType)relatedEnd.RelationMetadata).ReferentialConstraints.First();
            Debug.Assert(constraint.FromRole.Identity == relatedEnd.TargetRoleName, "Unexpected constraint role");
            return CreateKeyFromForeignKeyValues(dependentEntry, constraint, relatedEnd.GetTargetEntitySetFromRelationshipSet(), useOriginalValues: false);
        }

        /// <summary>
        /// Creates an EntityKey for a principal entity based on the foreign key values contained
        /// in this entity.  This implies that this entity is at the dependent end of the relationship.
        /// </summary>
        /// <param name="dependentEntry">The EntityEntry for the dependent that contains the FK</param>
        /// <param name="constraint">The constraint that describes this FK relationship</param>
        /// <param name="principalEntitySet">The entity set at the principal end of the the relationship</param>
        /// <param name="useOriginalValues">If true then the key will be constructed from the original FK values</param>
        /// <returns>The key, or null if any value in the key is null</returns>
        public static EntityKey CreateKeyFromForeignKeyValues(EntityEntry dependentEntry, ReferentialConstraint constraint, EntitySet principalEntitySet, bool useOriginalValues)
        {
            // Build the key values.  If any part of the key is null, then the entire key
            // is considered null.
            var dependentProps = constraint.ToProperties;
            int numValues = dependentProps.Count;
            if (numValues == 1)
            {
                object keyValue = useOriginalValues ?
                    dependentEntry.GetOriginalEntityValue(dependentProps.First().Name) :
                    dependentEntry.GetCurrentEntityValue(dependentProps.First().Name);
                return keyValue == DBNull.Value ? null : new EntityKey(principalEntitySet, keyValue);
            }

            // Note that the properties in the principal entity set may be in a different order than
            // they appear in the constraint.  Therefore, we create name value mappings to ensure that
            // the correct values are associated with the correct properties.
            // Unfortunately, there is not way to call the public EntityKey constructor that takes pairs
            // because the internal "object" constructor hides it.  Even this doesn't work:
            // new EntityKey(principalEntitySet, (IEnumerable<KeyValuePair<string, object>>)keyValues)
            string[] keyNames = principalEntitySet.ElementType.KeyMemberNames;
            Debug.Assert(keyNames.Length == numValues, "Number of entity set key names does not match constraint names");
            object[] values = new object[numValues];
            var principalProps = constraint.FromProperties;
            for (int i = 0; i < numValues; i++)
            {
                object value = useOriginalValues ?
                    dependentEntry.GetOriginalEntityValue(dependentProps[i].Name) :
                    dependentEntry.GetCurrentEntityValue(dependentProps[i].Name);
                if (value == DBNull.Value)
                {
                    return null;
                }
                int keyIndex = Array.IndexOf(keyNames, principalProps[i].Name);
                Debug.Assert(keyIndex >= 0 && keyIndex < numValues, "Could not find constraint prop name in entity set key names");
                values[keyIndex] = value;
            }
            return new EntityKey(principalEntitySet, values);
        }
    }
}
