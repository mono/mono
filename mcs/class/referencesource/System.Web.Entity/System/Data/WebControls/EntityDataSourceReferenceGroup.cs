//---------------------------------------------------------------------
// <copyright file="EntityDataSourceReferenceGroup.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Objects.DataClasses;
using System.Data.Objects;
using System.Data;
using System.Runtime.CompilerServices;

namespace System.Web.UI.WebControls
{
    /// <summary>
    /// Groups together reference columns pointing at the same association end.
    /// </summary>
    internal abstract class EntityDataSourceReferenceGroup
    {
        private readonly AssociationSetEnd end;

        protected EntityDataSourceReferenceGroup(AssociationSetEnd end)
        {
            EntityDataSourceUtil.CheckArgumentNull(end, "end");

            this.end = end;
        }

        internal AssociationSetEnd End { get { return this.end; } }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static EntityDataSourceReferenceGroup Create(Type entityType, AssociationSetEnd end)
        {
            EntityDataSourceUtil.CheckArgumentNull(entityType, "entityType");

            Type groupType = typeof(EntityDataSourceReferenceGroup<>).MakeGenericType(entityType);
            return (EntityDataSourceReferenceGroup)Activator.CreateInstance(groupType, new object[] { end });
        }

        internal abstract void SetKeyValues(EntityDataSourceWrapper wrapper, Dictionary<string, object> newKeyValues);

        internal abstract EntityKey GetEntityKey(EntityDataSourceWrapper entity);
    }

    internal class EntityDataSourceReferenceGroup<T> : EntityDataSourceReferenceGroup
        where T : class
    {
        public EntityDataSourceReferenceGroup(AssociationSetEnd end)
            : base(end)
        {
        }

        internal override void SetKeyValues(EntityDataSourceWrapper wrapper, Dictionary<string, object> newKeyValues)
        {
            EntityDataSourceUtil.CheckArgumentNull(wrapper, "wrapper");

            EntityReference<T> reference = GetRelatedReference(wrapper);

            EntityKey originalEntityKeys = reference.EntityKey;

            
            if (null != newKeyValues)
            {
                if(null != originalEntityKeys)
                {
                    // mix the missing keys from the original values
                    foreach (var originalEntityKey in originalEntityKeys.EntityKeyValues)
                    {
                        object newKeyValue;
                        if (newKeyValues.TryGetValue(originalEntityKey.Key, out newKeyValue))
                        {
                            // if any part of the key is null, the EntityKey is null
                            if (null == newKeyValue)
                            {
                                newKeyValues = null;
                                break;
                            }
                        }
                        else
                        {
                            // add the original value for this partial key since it is not saved in the viewstate
                            newKeyValues.Add(originalEntityKey.Key, originalEntityKey.Value);
                        }
                    }
                }
                else
                {
                    // what we have in the newKeyValues should be sufficient to set the key
                    // but if any value is null, the whole key is null
                    foreach (var newKey in newKeyValues)
                    {
                        if (null == newKey.Value)
                        {
                            newKeyValues = null;
                            break;
                        }
                    }
                }
            }

            if (null == newKeyValues)
            {
                // if the entity key is a compound key, and if any partial key is null, then the entitykey is null
                reference.EntityKey = null;
            }
            else
            {
                reference.EntityKey = new EntityKey(EntityDataSourceUtil.GetQualifiedEntitySetName(End.EntitySet), (IEnumerable<KeyValuePair<string, object>>)newKeyValues);
            }
        }

        internal override EntityKey GetEntityKey(EntityDataSourceWrapper entity)
        {
            EntityKey key = GetRelatedReference(entity).EntityKey;
            return key;
        }

        private EntityReference<T> GetRelatedReference(EntityDataSourceWrapper entity)
        {
            RelationshipManager relationshipManager = entity.RelationshipManager;
            Debug.Assert(relationshipManager != null, "couldn't get a relationship manager");
            EntityReference<T> reference = relationshipManager.GetRelatedReference<T>(
                this.End.ParentAssociationSet.ElementType.FullName,
                this.End.CorrespondingAssociationEndMember.Name);
            return reference;
        }
    }
}
