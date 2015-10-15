//---------------------------------------------------------------------
// <copyright file="EntityStoreSchemaGenerator.LoadMethodSessionState.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System.Data.Entity.Design.Common;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Data.Entity.Design.SsdlGenerator;
using System.Diagnostics;

namespace System.Data.Entity.Design
{
    public sealed partial class EntityStoreSchemaGenerator
    {
        // responsible for holding all the 
        // state for a single execution of the Load
        // method
        private class LoadMethodSessionState
        {
            public List<AssociationType> AssociationTypes = new List<AssociationType>();
            public UniqueIdentifierService UsedTypeNames = new UniqueIdentifierService(false,s=>s.Replace(".", "_"));
            public IEnumerable<EntityStoreSchemaFilterEntry> Filters;
            public Dictionary<EntityType, EntitySet> EntityTypeToSet = new Dictionary<EntityType, EntitySet>();
            public Dictionary<RelationshipEndMember, EntityType> RelationshipEndTypeLookup = new Dictionary<RelationshipEndMember, EntityType>();
            public Dictionary<string, EntityContainer> EntityContainerLookup = new Dictionary<string, EntityContainer>();
            public HashSet<EntityType> ReadOnlyEntities = new HashSet<EntityType>();
            public HashSet<EdmType> InvalidTypes = new HashSet<EdmType>();
            public MetadataItemSerializer.ErrorsLookup ItemToErrorsMap = new MetadataItemSerializer.ErrorsLookup();
            public StoreItemCollection ItemCollection;
            public List<EdmFunction> Functions = new List<EdmFunction>();

            private Dictionary<DbObjectKey, EntityType> _entityLookup = new Dictionary<DbObjectKey, EntityType>();
            private Dictionary<EntityType, DbObjectKey> _reverseEntityLookup = new Dictionary<EntityType, DbObjectKey>();
            private HashSet<DbObjectKey> _missingEntities = new HashSet<DbObjectKey>();
            private HashSet<DbObjectKey> _tablesWithoutKeys = new HashSet<DbObjectKey>();
            private Dictionary<DbObjectKey, RowType> _tvfReturnTypeLookup = new Dictionary<DbObjectKey, RowType>();
            private string _storeNamespace;

            private readonly Version _targetEntityFrameworkVersion;

            public LoadMethodSessionState(Version targetEntityFrameworkVersion)
            {
                _targetEntityFrameworkVersion = targetEntityFrameworkVersion;
            }

            public IEnumerable<EdmSchemaError> Errors
            {
                get
                {
                    foreach (List<EdmSchemaError> errors in ItemToErrorsMap.Values)
                    {
                        foreach (EdmSchemaError error in errors)
                        {
                            yield return error;
                        }
                    }
                }
            }

            public void AddTableWithoutKey(DbObjectKey tableKey)
            {
                _tablesWithoutKeys.Add(tableKey);
            }

            public bool ContainsTableWithoutKey(DbObjectKey tableKey)
            {
                return _tablesWithoutKeys.Contains(tableKey);
            }

            public EntitySet GetEntitySet(RelationshipEndMember end)
            {
                EntityType type = RelationshipEndTypeLookup[end];
                return EntityTypeToSet[type];
            }

            public void AddEntity(DbObjectKey key, EntityType type)
            {
                _entityLookup.Add(key, type);
                _reverseEntityLookup.Add(type, key);
            }

            public IEnumerable<EntityType> GetAllEntities()
            {
                return _entityLookup.Values;
            }

            public bool TryGetEntity(DbObjectKey key, out EntityType type)
            {
                if (_entityLookup.TryGetValue(key, out type))
                {
                    return true;
                }

                //Ignore relationships which refer to table without keys
                if (ContainsTableWithoutKey(key))
                {
                    return false;
                }

                if (!_missingEntities.Contains(key))
                {
                    AddErrorsForType(null,
                        new EdmSchemaError(
                        Strings.TableReferencedByAssociationWasNotFound(key),
                        (int)ModelBuilderErrorCode.MissingEntity,
                        EdmSchemaErrorSeverity.Error));
                    _missingEntities.Add(key);
                }
                return false;
            }

            public DbObjectKey GetKey(EntityType type)
            {
                DbObjectKey key;
                bool value = _reverseEntityLookup.TryGetValue(type, out key);
                Debug.Assert(value, "How did we possibly get an EntityType without a TableKey");
                return key;
            }

            public void AddTvfReturnType(DbObjectKey key, RowType type)
            {
                _tvfReturnTypeLookup.Add(key, type);
            }

            public bool TryGetTvfReturnType(DbObjectKey key, out RowType type)
            {
                return _tvfReturnTypeLookup.TryGetValue(key, out type);
            }

            /// <summary>
            /// Attempts to get the primitive store type for the given type name. This method takes the target .NET Framework
            /// into account and only returns primitive types that are supported by the Framework that is being targeted.
            /// If a type is recognized but excluded because it is not supported by the target Framework then the excludedForTarget
            /// flag is set to true.
            /// </summary>
            internal bool TryGetStorePrimitiveType(string typeName, out PrimitiveType primitiveType, out bool excludedForTarget)
            {
                excludedForTarget = false;
                var success = ItemCollection.TryGetItem<PrimitiveType>(StoreNamespace + "." + typeName, false, out primitiveType);

                // If targetting 4.0 using 4.5 then we need to ignore geometry and geography types just like we would have done when
                // generating with 4.0. We can only get the base spatial types since we won't reverse engineer to derived spatial
                // types. We don't need to do anything for enums because we will never reverse engineer to an enum.
                if (success &&
                    _targetEntityFrameworkVersion.CompareTo(EntityFrameworkVersions.Version3) < 0 &&
                    (primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Geography || primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Geometry))
                {
                    excludedForTarget = true;
                    primitiveType = null;
                    return false;
                }
                return success;
            }

            internal string StoreNamespace
            {
                get
                {
                    if (_storeNamespace == null)
                    {
                        foreach (PrimitiveType type in ItemCollection.GetItems<PrimitiveType>())
                        {
                            if (MetadataUtil.IsStoreType(type))
                            {
                                _storeNamespace = type.NamespaceName;
                                break;
                            }
                        }
                    }
                    return _storeNamespace;
                }
            }

            internal void AddErrorsForType(EdmType type, params EdmSchemaError [] errors)
            {
                AddErrorsForType(type, (ICollection<EdmSchemaError>)errors);
            }

            internal void AddErrorsForType(EdmType type, ICollection<EdmSchemaError> errors)
            {
                if (errors.Count != 0)
                {
                    if (type == null)
                    {
                        type = MetadataItemSerializer.NoSpecificTypeSentinal;
                    }
                    
                    if (ItemToErrorsMap.ContainsKey(type))
                    {
                        ItemToErrorsMap[type].AddRange(errors);
                    }
                    else
                    {
                        List<EdmSchemaError> list = errors as List<EdmSchemaError>;
                        if (list == null)
                        {
                            list = new List<EdmSchemaError>(errors);
                        }

                        ItemToErrorsMap.Add(type, list);
                    }
                }
            }
        }
    }
}
