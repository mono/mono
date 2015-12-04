//---------------------------------------------------------------------
// <copyright file="StorageMappingItemCollection.ViewDictionary.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Common.CommandTrees;
using System.Data.Common.Utils;
using System.Data.Entity;
using System.Data.Mapping.ViewGeneration;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

// All methods prefixed with 'Serialized' have locked access through the shared Memoizer. Do not call it from outside the Memoizer's scope.
namespace System.Data.Mapping
{
    using System.Data.Common.CommandTrees.ExpressionBuilder;
    using OfTypeQVCacheKey = Pair<EntitySetBase, Pair<EntityTypeBase, bool>>;

    public partial class StorageMappingItemCollection : MappingItemCollection
    {
        internal delegate bool TryGetUserDefinedQueryView(EntitySetBase extent, out GeneratedView generatedView);
        internal delegate bool TryGetUserDefinedQueryViewOfType(OfTypeQVCacheKey extent, out GeneratedView generatedView);

        internal class ViewDictionary
        {

            readonly TryGetUserDefinedQueryView TryGetUserDefinedQueryView;
            readonly TryGetUserDefinedQueryViewOfType TryGetUserDefinedQueryViewOfType;
           
            private StorageMappingItemCollection m_storageMappingItemCollection;

            private static ConfigViewGenerator m_config = new ConfigViewGenerator();

            // List of assemblies from which we have loaded views from
            private List<Assembly> m_knownViewGenAssemblies = new List<Assembly>();

            // Indicates whether the views are being fetched from a generated class or they are being generated at the runtime
            private bool m_generatedViewsMode = true;

            /// <summary>
            /// Caches computation of view generation per <see cref="StorageEntityContainerMapping"/>. Cached value contains both query and update views.
            /// </summary>
            private readonly Memoizer<EntityContainer, Dictionary<EntitySetBase, GeneratedView>> m_generatedViewsMemoizer;
            /// <summary>
            /// Caches computation of getting Type-specific Query Views - either by view gen or user-defined input.
            /// </summary>
            private readonly Memoizer<OfTypeQVCacheKey, GeneratedView> m_generatedViewOfTypeMemoizer;

            internal ViewDictionary(StorageMappingItemCollection storageMappingItemCollection, 
                            out Dictionary<EntitySetBase, GeneratedView> userDefinedQueryViewsDict,
                            out Dictionary<OfTypeQVCacheKey, GeneratedView> userDefinedQueryViewsOfTypeDict)
            {
                this.m_storageMappingItemCollection = storageMappingItemCollection;
                this.m_generatedViewsMemoizer = new Memoizer<EntityContainer, Dictionary<EntitySetBase, GeneratedView>>(SerializedGetGeneratedViews, null);
                this.m_generatedViewOfTypeMemoizer = new Memoizer<OfTypeQVCacheKey, GeneratedView>(SerializedGeneratedViewOfType, OfTypeQVCacheKey.PairComparer.Instance);

                userDefinedQueryViewsDict = new Dictionary<EntitySetBase, GeneratedView>(EqualityComparer<EntitySetBase>.Default);
                userDefinedQueryViewsOfTypeDict = new Dictionary<OfTypeQVCacheKey, GeneratedView>(OfTypeQVCacheKey.PairComparer.Instance);

                TryGetUserDefinedQueryView = userDefinedQueryViewsDict.TryGetValue;
                TryGetUserDefinedQueryViewOfType = userDefinedQueryViewsOfTypeDict.TryGetValue;
            }

            private Dictionary<EntitySetBase, GeneratedView> SerializedGetGeneratedViews(EntityContainer container)
            {
                Debug.Assert(container != null);

                // Note that extentMappingViews will contain both query and update views.
                Dictionary<EntitySetBase, GeneratedView> extentMappingViews;

                // Get the mapping that has the entity container mapped.
                StorageEntityContainerMapping entityContainerMap = MappingMetadataHelper.GetEntityContainerMap(m_storageMappingItemCollection, container);

                // We get here because memoizer didn't find an entry for the container.
                // It might happen that the entry with generated views already exists for the counterpart container, so check it first.
                EntityContainer counterpartContainer = container.DataSpace == DataSpace.CSpace ? 
                    entityContainerMap.StorageEntityContainer : entityContainerMap.EdmEntityContainer;
                if (m_generatedViewsMemoizer.TryGetValue(counterpartContainer, out extentMappingViews))
                {
                    return extentMappingViews;
                }

                extentMappingViews = new Dictionary<EntitySetBase, GeneratedView>(); 
               
                if (!entityContainerMap.HasViews)
                {
                    return extentMappingViews;
                }

                // If we are in generated views mode.
                if (m_generatedViewsMode)
                {
                    if(ObjectItemCollection.ViewGenerationAssemblies!=null && ObjectItemCollection.ViewGenerationAssemblies.Count>0)
                    {
                        SerializedCollectViewsFromObjectCollection(this.m_storageMappingItemCollection.Workspace, extentMappingViews);
                    }
                    else
                    {
                        SerializedCollectViewsFromReferencedAssemblies(this.m_storageMappingItemCollection.Workspace, extentMappingViews);
                    }
                }

                if (extentMappingViews.Count == 0)
                {
                    // We should change the mode to runtime generation of views.
                    this.m_generatedViewsMode = false;
                    this.SerializedGenerateViews(entityContainerMap, extentMappingViews);
                }

                Debug.Assert(extentMappingViews.Count > 0, "view should be generated at this point");

                return extentMappingViews;
            }

            /// <summary>
            /// Call the View Generator's Generate view method
            /// and collect the Views and store it in a local dictionary.
            /// </summary>
            /// <param name="entityContainerMap"></param>
            /// <param name="resultDictionary"></param>
            private void SerializedGenerateViews(StorageEntityContainerMapping entityContainerMap, Dictionary<EntitySetBase, GeneratedView> resultDictionary)
            {
                //If there are no entity set maps, don't call the view generation process
                Debug.Assert(entityContainerMap.HasViews);

                ViewGenResults viewGenResults = ViewgenGatekeeper.GenerateViewsFromMapping(entityContainerMap, m_config);
                KeyToListMap<EntitySetBase, GeneratedView> extentMappingViews = viewGenResults.Views;
                if (viewGenResults.HasErrors)
                {
                    // Can get the list of errors using viewGenResults.Errors
                    throw new MappingException(Helper.CombineErrorMessage(viewGenResults.Errors));
                }

                foreach (KeyValuePair<EntitySetBase, List<GeneratedView>> keyValuePair in extentMappingViews.KeyValuePairs)
                {
                    //Multiple Views are returned for an extent but the first view
                    //is the only one that we will use for now. In the future,
                    //we might start using the other views which are per type within an extent.
                    GeneratedView generatedView;
                    //Add the view to the local dictionary
                            
                    if (!resultDictionary.TryGetValue(keyValuePair.Key, out generatedView))
                    {
                        generatedView = keyValuePair.Value[0];
                        resultDictionary.Add(keyValuePair.Key, generatedView);
                    }
                }
            }

            /// <summary>
            /// Generates a single query view for a given Extent and type. It is used to generate OfType and OfTypeOnly views.
            /// </summary>
            /// <param name="includeSubtypes">Whether the view should include extents that are subtypes of the given entity</param>
            private bool TryGenerateQueryViewOfType(EntityContainer entityContainer, EntitySetBase entity, EntityTypeBase type, bool includeSubtypes, out GeneratedView generatedView)
            {
                Debug.Assert(entityContainer != null);
                Debug.Assert(entity != null);
                Debug.Assert(type != null);

                if (type.Abstract)
                {
                    generatedView = null;
                    return false;
                }

                //Get the mapping that has the entity container mapped.
                StorageEntityContainerMapping entityContainerMap = MappingMetadataHelper.GetEntityContainerMap(m_storageMappingItemCollection, entityContainer);
                Debug.Assert(!entityContainerMap.IsEmpty, "There are no entity set maps");

                bool success;
                ViewGenResults viewGenResults = ViewgenGatekeeper.GenerateTypeSpecificQueryView(entityContainerMap, m_config, entity, type, includeSubtypes, out success);
                if (!success)
                {
                    generatedView = null;
                    return false; //could not generate view
                }

                KeyToListMap<EntitySetBase, GeneratedView> extentMappingViews = viewGenResults.Views;

                if (viewGenResults.HasErrors)
                {
                    throw new MappingException(Helper.CombineErrorMessage(viewGenResults.Errors));
                }

                Debug.Assert(extentMappingViews.AllValues.Count() == 1, "Viewgen should have produced only one view");
                generatedView = extentMappingViews.AllValues.First();

                return true;
            }

            /// <summary>
            /// Tries to generate the Oftype or OfTypeOnly query view for a given entity set and type. 
            /// Returns false if the view could not be generated.
            /// Possible reasons for failing are 
            ///   1) Passing in OfTypeOnly on an abstract type
            ///   2) In user-specified query views mode a query for the given type is absent
            /// </summary>
            internal bool TryGetGeneratedViewOfType(MetadataWorkspace workspace, EntitySetBase entity, EntityTypeBase type, bool includeSubtypes, out GeneratedView generatedView)
            {
                OfTypeQVCacheKey key = new OfTypeQVCacheKey(entity, new Pair<EntityTypeBase, bool>(type, includeSubtypes));
                generatedView = this.m_generatedViewOfTypeMemoizer.Evaluate(key);
                return (generatedView != null);
            }

            /// <summary>
            /// Note: Null return value implies QV was not generated.
            /// </summary>
            /// <returns></returns>
            private GeneratedView SerializedGeneratedViewOfType(OfTypeQVCacheKey arg)
            {
                GeneratedView generatedView;
                //See if we have collected user-defined QueryView
                if(TryGetUserDefinedQueryViewOfType(arg, out generatedView))
                {
                    return generatedView;
                }

                //Now we have to generate the type-specific view
                EntitySetBase entity = arg.First;
                EntityTypeBase type = arg.Second.First;
                bool includeSubtypes = arg.Second.Second;

                if (!TryGenerateQueryViewOfType(entity.EntityContainer, entity, type, includeSubtypes, out generatedView))
                {
                    generatedView = null;
                }
 
                return generatedView;
            }

            /// <summary>
            /// Returns the update or query view for an Extent as a
            /// string.
            /// There are a series of steps that we go through for discovering a view for an extent.
            /// To start with we assume that we are working with Generated Views. To find out the 
            /// generated view we go to the ObjectItemCollection and see if it is not-null. If the ObjectItemCollection
            /// is non-null, we get the view generation assemblies that it might have cached during the
            /// Object metadata discovery.If there are no view generation assemblies we switch to the
            /// runtime view generation strategy. If there are view generation assemblies, we get the list and
            /// go through them and see if there are any assemblies that are there from which we have not already loaded
            /// the views. We collect the views from assemblies that we have not already collected from earlier.
            /// If the ObjectItemCollection is null and we are in the view generation mode, that means that
            /// the query or update is issued from the Value layer and this is the first time view has been asked for.
            /// The compile time view gen for value layer queries will work for very simple scenarios.
            /// If the users wants to get the performance benefit, they should call MetadataWorkspace.LoadFromAssembly.
            /// At this point we go through the referenced assemblies of the entry assembly( this wont work for Asp.net 
            /// or if the viewgen assembly was not referenced by the executing application).
            /// and try to see if there were any view gen assemblies. If there are, we collect the views for all extents.
            /// Once we have all the generated views gathered, we try to get the view for the extent passed in.
            /// If we find one we will return it. If we can't find one an exception will be thrown.
            /// If there were no view gen assemblies either in the ObjectItemCollection or in the list of referenced
            /// assemblies of calling assembly, we change the mode to runtime view generation and will continue to
            /// be in that mode for the rest of the lifetime of the mapping item collection.
            /// </summary>
            internal GeneratedView GetGeneratedView(EntitySetBase extent, MetadataWorkspace workspace, StorageMappingItemCollection storageMappingItemCollection)
            {
                //First check if we have collected a view from user-defined query views
                //Dont need to worry whether to generate Query view or update viw, because that is relative to the extent.
                GeneratedView view;

                if (TryGetUserDefinedQueryView(extent, out view))
                {
                    return view;
                }

                //If this is a foreign key association, manufacture a view on the fly.
                if (extent.BuiltInTypeKind == BuiltInTypeKind.AssociationSet)
                {
                    AssociationSet aSet = (AssociationSet)extent;
                    if (aSet.ElementType.IsForeignKey)
                    {
                        if (m_config.IsViewTracing)
                        {
                            Helpers.StringTraceLine(String.Empty);
                            Helpers.StringTraceLine(String.Empty);
                            Helpers.FormatTraceLine("================= Generating FK Query View for: {0} =================", aSet.Name);
                            Helpers.StringTraceLine(String.Empty);
                            Helpers.StringTraceLine(String.Empty);
                        }

                        // Although we expose a collection of constraints in the API, there is only ever one constraint.
                        Debug.Assert(aSet.ElementType.ReferentialConstraints.Count == 1, "aSet.ElementType.ReferentialConstraints.Count == 1");
                        ReferentialConstraint rc = aSet.ElementType.ReferentialConstraints.Single();

                        EntitySet dependentSet = aSet.AssociationSetEnds[rc.ToRole.Name].EntitySet;                        
                        EntitySet principalSet = aSet.AssociationSetEnds[rc.FromRole.Name].EntitySet;

                        DbExpression qView = dependentSet.Scan();

                        // Introduce an OfType view if the dependent end is a subtype of the entity set
                        EntityType dependentType = MetadataHelper.GetEntityTypeForEnd((AssociationEndMember)rc.ToRole);
                        EntityType principalType = MetadataHelper.GetEntityTypeForEnd((AssociationEndMember)rc.FromRole);
                        if (dependentSet.ElementType.IsBaseTypeOf(dependentType))
                        {
                            qView = qView.OfType(TypeUsage.Create(dependentType));
                        }

                        if (rc.FromRole.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne)
                        {
                            // Filter out instances with existing relationships.
                            qView = qView.Where(e =>
                            {
                                DbExpression filter = null;
                                foreach (EdmProperty fkProp in rc.ToProperties)
                                {
                                    DbExpression notIsNull = e.Property(fkProp).IsNull().Not();
                                    filter = null == filter ? notIsNull : filter.And(notIsNull);
                                }
                                return filter;
                            });
                        }
                        qView = qView.Select(e =>
                        {
                            List<DbExpression> ends = new List<DbExpression>();
                            foreach (AssociationEndMember end in aSet.ElementType.AssociationEndMembers)
                            {
                                if (end.Name == rc.ToRole.Name)
                                {
                                    var keyValues = new List<KeyValuePair<string, DbExpression>>();
                                    foreach (EdmMember keyMember in dependentSet.ElementType.KeyMembers)
                                    {
                                        keyValues.Add(e.Property((EdmProperty)keyMember));
                                    }
                                    ends.Add(dependentSet.RefFromKey(DbExpressionBuilder.NewRow(keyValues), dependentType));
                                }
                                else
                                {
                                    // Manufacture a key using key values.
                                    var keyValues = new List<KeyValuePair<string, DbExpression>>();
                                    foreach (EdmMember keyMember in principalSet.ElementType.KeyMembers)
                                    {
                                        int offset = rc.FromProperties.IndexOf((EdmProperty)keyMember);
                                        keyValues.Add(e.Property(rc.ToProperties[offset]));
                                    }
                                    ends.Add(principalSet.RefFromKey(DbExpressionBuilder.NewRow(keyValues), principalType));
                                }
                            }
                            return TypeUsage.Create(aSet.ElementType).New(ends);
                        });
                        return GeneratedView.CreateGeneratedViewForFKAssociationSet(aSet, aSet.ElementType, new DbQueryCommandTree(workspace, DataSpace.SSpace, qView), storageMappingItemCollection, m_config);
                    }
                }

                // If no User-defined QV is found, call memoized View Generation procedure.
                Dictionary<EntitySetBase, GeneratedView> generatedViews = m_generatedViewsMemoizer.Evaluate(extent.EntityContainer);

                if (!generatedViews.TryGetValue(extent, out view)) 
                {
                    throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.Mapping_Views_For_Extent_Not_Generated(
                        (extent.EntityContainer.DataSpace==DataSpace.SSpace)?"Table":"EntitySet",extent.Name));
                }

                return view;
            }

            /// <summary>
            /// Collect the views from object collection's view gen assembly
            /// </summary>
            /// <param name="workspace"></param>
            /// <param name="objectCollection"></param>
            private void SerializedCollectViewsFromObjectCollection(MetadataWorkspace workspace, Dictionary<EntitySetBase, GeneratedView> extentMappingViews)
            {
                IList<Assembly> allViewGenAssemblies = ObjectItemCollection.ViewGenerationAssemblies;
                if (allViewGenAssemblies != null)
                {
                    foreach (Assembly assembly in allViewGenAssemblies)
                    {
                        object[] viewGenAttributes = assembly.GetCustomAttributes(typeof(System.Data.Mapping.EntityViewGenerationAttribute), false /*inherit*/);
                        if ((viewGenAttributes != null) && (viewGenAttributes.Length != 0))
                        {
                            foreach (EntityViewGenerationAttribute viewGenAttribute in viewGenAttributes)
                            {
                                Type viewContainerType = viewGenAttribute.ViewGenerationType;
                                if (!viewContainerType.IsSubclassOf(typeof(EntityViewContainer)))
                                {
                                    throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.Generated_View_Type_Super_Class(StorageMslConstructs.EntityViewGenerationTypeName));
                                }
                                EntityViewContainer viewContainer = (Activator.CreateInstance(viewContainerType) as EntityViewContainer);
                                Debug.Assert(viewContainer != null, "Should be able to create the type");

                                SerializedAddGeneratedViewsInEntityViewContainer(workspace, viewContainer, extentMappingViews);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// this method do the following check on the generated views in the EntityViewContainer, 
            /// then add those views all at once to the dictionary
            /// 1. there should be one storeageEntityContainerMapping that has the same h
            ///     C side and S side names as the EnittyViewcontainer
            /// 2. Generate the hash for the storageEntityContainerMapping in the MM closure, 
            ///     and this hash should be the same in EntityViewContainer
            /// 3. Generate the hash for all of the view text in the EntityViewContainer and 
            ///     this hash should be the same as the stored on in the EntityViewContainer
            /// </summary>
            /// <param name="entityViewContainer"></param>
            private void SerializedAddGeneratedViewsInEntityViewContainer(MetadataWorkspace workspace, EntityViewContainer entityViewContainer, Dictionary<EntitySetBase, GeneratedView> extentMappingViews)
            {
                StorageEntityContainerMapping storageEntityContainerMapping;
                // first check
                if (!this.TryGetCorrespondingStorageEntityContainerMapping(entityViewContainer,
                    workspace.GetItemCollection(DataSpace.CSSpace).GetItems<StorageEntityContainerMapping>(), out storageEntityContainerMapping))
                {
                    return;
                }

                // second check
                if (!this.SerializedVerifyHashOverMmClosure(storageEntityContainerMapping, entityViewContainer))
                {
                    throw new MappingException(Strings.ViewGen_HashOnMappingClosure_Not_Matching(entityViewContainer.EdmEntityContainerName));

                }

                // third check, prior to the check, we collect all the views in the entity view container to the dictionary
                // if the views are changed then we will throw exception out
                if (this.VerifyViewsHaveNotChanged(workspace, entityViewContainer))
                {
                    this.SerializedAddGeneratedViews(workspace, entityViewContainer, extentMappingViews);
                }
                else
                {
                    throw new InvalidOperationException(System.Data.Entity.Strings.Generated_Views_Changed);
                }
            }
                    

            private bool TryGetCorrespondingStorageEntityContainerMapping(EntityViewContainer viewContainer,
                IEnumerable<StorageEntityContainerMapping> storageEntityContainerMappingList, out StorageEntityContainerMapping storageEntityContainerMapping)
            {
                storageEntityContainerMapping = null;

                foreach (var entityContainerMapping in storageEntityContainerMappingList)
                {
                    // first check
                    if (entityContainerMapping.EdmEntityContainer.Name == viewContainer.EdmEntityContainerName &&
                        entityContainerMapping.StorageEntityContainer.Name == viewContainer.StoreEntityContainerName)
                    {
                        storageEntityContainerMapping = entityContainerMapping;
                        return true;
                    }
                }
                return false;

            }

            private bool SerializedVerifyHashOverMmClosure(StorageEntityContainerMapping entityContainerMapping, EntityViewContainer entityViewContainer)
            {
                if (MetadataMappingHasherVisitor.GetMappingClosureHash(m_storageMappingItemCollection.MappingVersion, entityContainerMapping) ==
                    entityViewContainer.HashOverMappingClosure)
                {
                    return true;
                }
                return false;
            }

            private bool VerifyViewsHaveNotChanged(MetadataWorkspace workspace, EntityViewContainer viewContainer)
            {
                //Now check whether the hash of the generated views match the one
                //we stored in the code file during design
                //Produce the hash and add it to the code
                var mappingCollection = (workspace.GetItemCollection(DataSpace.CSSpace) as StorageMappingItemCollection);
                Debug.Assert(mappingCollection != null,"Must have Mapping Collection in the Metadataworkspace");

                string viewHash = MetadataHelper.GenerateHashForAllExtentViewsContent(mappingCollection.MappingVersion, viewContainer.ExtentViews);
                string storedViewHash = viewContainer.HashOverAllExtentViews;
                if (viewHash != storedViewHash)
                {
                    return false;
                }
                return true;
            }


            //Collect the names of the entitysetbases and the generated views from
            //the generated type into a string so that we can produce a hash over it.
            private void SerializedAddGeneratedViews(MetadataWorkspace workspace, EntityViewContainer viewContainer, Dictionary<EntitySetBase, GeneratedView> extentMappingViews)
            {
                foreach (KeyValuePair<string, string> extentView in viewContainer.ExtentViews)
                {
                    EntityContainer entityContainer = null;
                    EntitySetBase extent = null;


                    string extentFullName = extentView.Key;
                    int extentNameIndex = extentFullName.LastIndexOf('.');

                    if (extentNameIndex != -1)
                    {
                        string entityContainerName = extentFullName.Substring(0, extentNameIndex);
                        string extentName = extentFullName.Substring(extentFullName.LastIndexOf('.') + 1);

                        if (!workspace.TryGetItem<EntityContainer>(entityContainerName, DataSpace.CSpace, out entityContainer))
                        {
                            workspace.TryGetItem<EntityContainer>(entityContainerName, DataSpace.SSpace, out entityContainer);
                        }

                        if (entityContainer != null)
                        {
                            entityContainer.BaseEntitySets.TryGetValue(extentName, false, out extent);
                        }
                    }

                    if (extent == null)
                    {
                        throw new MappingException(System.Data.Entity.Strings.Generated_Views_Invalid_Extent(extentFullName));
                    }

                    //Create a Generated view and cache it
                    GeneratedView generatedView;
                    //Add the view to the local dictionary
                    if (!extentMappingViews.TryGetValue(extent, out generatedView))
                    {
                        generatedView = GeneratedView.CreateGeneratedView(
                            extent, 
                            null, // edmType
                            null, // commandTree
                            extentView.Value, // eSQL
                            m_storageMappingItemCollection,
                            new ConfigViewGenerator());
                        extentMappingViews.Add(extent, generatedView);
                    }
                }
            }


            /// <summary>
            /// Tries to collect the views from the referenced assemblies of Entry assembly.
            /// </summary>
            /// <param name="workspace"></param>
            private void SerializedCollectViewsFromReferencedAssemblies(MetadataWorkspace workspace, Dictionary<EntitySetBase, GeneratedView> extentMappingViews)
            {
                ObjectItemCollection objectCollection;
                ItemCollection itemCollection;
                if (!workspace.TryGetItemCollection(DataSpace.OSpace, out itemCollection))
                {
                    //Possible enhancement : Think about achieving the same thing without creating Object Item Collection.
                    objectCollection = new ObjectItemCollection();
                    itemCollection = objectCollection;
                    // The GetEntryAssembly method can return a null reference
                    //when a managed assembly has been loaded from an unmanaged application.
                    Assembly entryAssembly = Assembly.GetEntryAssembly();
                    if (entryAssembly != null)
                    {
                        objectCollection.ImplicitLoadViewsFromAllReferencedAssemblies(entryAssembly);
                    }
                }
                this.SerializedCollectViewsFromObjectCollection(workspace, extentMappingViews);
            }
        }
    }
}
