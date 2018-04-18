//---------------------------------------------------------------------
// <copyright file="EntityDataSourceView.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.DynamicData;
using System.Runtime.CompilerServices;

namespace System.Web.UI.WebControls
{
    public class EntityDataSourceView : DataSourceView, IStateManager
    {
        /// <summary>
        /// Helper class used to manage EntityDataSourceWrapperCollection collections. 
        /// </summary>
        /// <remarks>
        /// Entities in EntityDataSourceWrapperCollection are wrapped in order to allow for easy access to nested properties (and also 
        /// to be able to handle independendent associations - it was especially important in v1 where foreign keys were not supported). 
        /// This is handy when we need to get or set property values because we set values the same way regeradless if a 
        /// property is nested or not. Additional benefit is that the entities from EntityDataSourceWrapperCollection can be 
        /// easily stored and restored into/from the viewstate.
        /// We cache entities in EntityDataSourceWrapperCollection in two cases:
        /// - EntityDataSource.EnableFlattening flag is set to true. In this case the user requested entities to be flattened and wrapping allows 
        ///   for an easy translation between tabular and hierarchical forms
        /// - EntityDataSource.EnableUpdate flag is set meaning the user allows for updating entities. In this case we use EntityDataSourceWrapperCollection
        ///   to cache original values so that we can store them easily in the viewstate. The reason for storing values in the viewstate is that the 
        ///   update operation can be mapped to a function. If we don't store the original values we would invoke the update function but some parameters
        ///   would not be. This would result in data corruption as we would override existing data in the database with default values (nulls) or an exception
        ///   if the target column in the database does not allow null values.
        ///   When the parameters would not be set? This happens when the dictionary containing oldValues passed to ExecuteUpdate method is missing some 
        ///   values. This in turn can happen if EnableFlattening is set to false and the entity contains a complex property. The values of child properties of the
        ///   complex property are not displayed to the user and as a result the values for columns that were not shown are not passed back to the 
        ///   ExecuteUpdate method. Another case is when the user uses a GridView bound to EntityDataSource. If the user hides some columns the values
        ///   for the hidden columns will not be passed to the ExecuteUpdate method.
        /// Note that when entity flattening is enabled we will always store values returned from Select in the viewstate so we don't need to do anything special
        /// for update. When entity flattening is disabled we always store the values if EnableUpdate is set to true. This is not really needed if the update operation
        /// is not mapped to a function but if you are not inside System.Data.Entity.dll there is no way to tell whether the operation is mapped to a function so we need 
        /// to do it always.
        /// </remarks>
        private class WrapperCollectionManager
        {
            /// <summary>
            /// Modes the manager can be working in.
            /// </summary>
            public enum ManagerMode 
            { 
                /// <summary>
                /// None - entities are not stored. <see cref="_collection"/> is null. 
                /// Both <see cref="FlattenedEntityCollection"/> and <see cref="UpdateCache"/> return null.
                /// </summary>
                None, 

                /// <summary>
                /// Entites are stored because <see cref="EntityDataSource.EnableFlattening"/> flag is set to true.
                /// <see cref="FlattenedEntityCollection"/> returns non-null value whiel <see cref="UpdateCache"/> returns null.
                /// </summary>
                FlattenedEntities,

                /// <summary>
                /// Entites are stored because <see cref="EntityDataSource.EnableFlattening"/> flag is set to false but 
                /// <see cref="EntityDataSource.EnableUpdate"/> is set to true.
                /// <see cref="FlattenedEntityCollection"/> returns non-null value whiel <see cref="UpdateCache"/> returns null.
                /// </summary>
                UpdateCache 
            }

            private ManagerMode _collectionMode;
            private EntityDataSourceWrapperCollection _collection;

            /// <summary>
            /// Mode the WrapperCollectionManager is working in.
            /// </summary>
            public ManagerMode Mode
            {
                get { return _collectionMode; }
            }

            /// <summary>
            /// Gets collection containing flattened entities. Possibly null.
            /// </summary>
            public EntityDataSourceWrapperCollection FlattenedEntityCollection
            {
                get { return _collectionMode == ManagerMode.FlattenedEntities ? _collection : null; }
            }

            /// <summary>
            /// Gets collection cached entities. Possibly null.
            /// </summary>
            public EntityDataSourceWrapperCollection UpdateCache
            {
                get { return _collectionMode == ManagerMode.UpdateCache ? _collection : null; } 
            }

            /// <summary>
            /// Creates a collection for storing wrapped entities.
            /// </summary>
            /// <param name="context">ObjectContext</param>
            /// <param name="entitySet">Entity set the stored belongs to.</param>
            /// <param name="CSpaceFilteredEntityType">What entity type restrict the collection to. Null if all derived entity types are allowed.</param>
            /// <param name="mode">What mode to store the entities. Never <see cref="ManagerMode.None"/>.</param>
            public void CreateCollection(ObjectContext context, EntitySet entitySet, EntityType CSpaceFilteredEntityType, ManagerMode mode)
            {
                Debug.Assert(context != null);
                Debug.Assert(entitySet != null);
                Debug.Assert(mode != ManagerMode.None);
                Debug.Assert(_collectionMode == ManagerMode.None || _collectionMode == mode, "Cannot reset a collection working in a different mode.");

                _collectionMode = mode;
                _collection = new EntityDataSourceWrapperCollection(context, entitySet, CSpaceFilteredEntityType);
            }

            /// <summary>
            /// Wraps the <paramref name="entity"/> and adds it to the collection.
            /// </summary>
            /// <param name="entity">Entity to add to the collection. Must not be null.</param>
            public void AddWrappedEntity(object entity)
            {
                Debug.Assert(entity != null);
                Debug.Assert(Mode != ManagerMode.None);

                _collection.AddWrappedEntity(entity);
            }
        }

        private EntityDataSource _owner;
        private ObjectContext _ctx = null;
        private ReadOnlyMetadataCollection<EdmMember> _keyMembers = null;

        private static readonly object EventContextCreated = new object();
        private static readonly object EventContextCreating = new object();
        private static readonly object EventContextDisposing = new object();
        private static readonly object EventDeleted = new object();
        private static readonly object EventDeleting = new object();
        private static readonly object EventInserted = new object();
        private static readonly object EventInserting = new object();
        private static readonly object EventSelected = new object();
        private static readonly object EventSelecting = new object();
        private static readonly object EventUpdated = new object();
        private static readonly object EventUpdating = new object();
        private static readonly object EventException = new object();
        private static readonly object EventQueryCreated = new object();

        // values saved in ViewState
        private bool _disableUpdates = false;
        private bool _tracking = false;

        private Dictionary<string, ArrayList> _originalProperties;

        private WrapperCollectionManager _collectionManager = new WrapperCollectionManager();

        #region Constructor
        
        /// <summary>
        /// Initialize a new named instance of the EntityDataSourceView class, and 
        /// associates the specified EntityDataSource with it.
        /// </summary>
        public EntityDataSourceView(EntityDataSource owner, string viewName)
            : base(owner, viewName)
        {
            _owner = owner;
        }
        #endregion Constructor

        #region ExecuteSelect

        private static readonly MethodInfo _executeSelectMethod = typeof(EntityDataSourceView).GetMethod("ExecuteSelectTyped", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo _continueSelectMethod = typeof(EntityDataSourceView).GetMethod("ContinueSelectTyped", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly Type[] queryBuilderCreatorArgTypes = { typeof(DataSourceSelectArguments), typeof(string), typeof(ObjectParameter[]), 
                                                                             typeof(string), typeof(ObjectParameter[]), typeof(string),
                                                                             typeof(string), typeof(string), typeof(ObjectParameter[]),
                                                                             typeof(OrderByBuilder), typeof(string) };

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
        {
            // reset collections
            _collectionManager = new WrapperCollectionManager();

            AddSupportedCapabilities(arguments);
            arguments.RaiseUnsupportedCapabilitiesError(this);

            ConstructContext();

            var selectArgs = new EntityDataSourceSelectingEventArgs(_owner, arguments);
            OnSelecting(selectArgs);
            if (selectArgs.Cancel)
            {
                return null;
            }

            _disableUpdates = _owner.ValidateUpdatableConditions();

            if (_owner.ValidateWrappable() || CanUpdate)
            {
                _collectionManager.CreateCollection(
                    Context, 
                    EntitySet, 
                    CSpaceFilteredEntityType,
                    _owner.ValidateWrappable() ? WrapperCollectionManager.ManagerMode.FlattenedEntities : WrapperCollectionManager.ManagerMode.UpdateCache);
            }            

            if (!string.IsNullOrEmpty(_owner.Select))
            {
                return ExecuteSelectTyped<DbDataRecord>(arguments, EntityDataSourceRecordQueryBuilder.Create);
            }
            else if (!string.IsNullOrEmpty(_owner.CommandText))
            {
                return ExecuteSelectTyped<object>(arguments, EntityDataSourceObjectQueryBuilder<object>.Create);
            }
            else
            {
                Type builderType = typeof(EntityDataSourceObjectQueryBuilder<>).MakeGenericType(EntityClrType);
                MethodInfo getCreatorMethod = builderType.GetMethod("GetCreator", BindingFlags.Static | BindingFlags.NonPublic);
                object createDelegate = getCreatorMethod.Invoke(null, null);
                try
                {
                    return (IEnumerable)_executeSelectMethod.MakeGenericMethod(EntityClrType).Invoke(this, new object[] { arguments, createDelegate });
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private IEnumerable ExecuteSelectTyped<T>(DataSourceSelectArguments arguments, EntityDataSourceQueryBuilder<T>.Creator qbConstructor)
        {
            string whereClause;
            ObjectParameter[] whereParameters;
            GenerateWhereClause(out whereClause, out whereParameters);
            string entitySetQueryExpression = GenerateEntitySetQueryExpression();

            var orderByBuilder = new OrderByBuilder(arguments.SortExpression, _collectionManager.FlattenedEntityCollection,
                _owner.OrderBy, _owner.AutoGenerateOrderByClause, _owner.OrderByParameters,
                CanPage, //There's no need to generate the default OrderBy clause if paging is disabled. Prevents an unnecessary sort at the server.
                _owner);

            EntityDataSourceQueryBuilder<T> queryBuilder =
                qbConstructor(
                        arguments,
                        _owner.CommandText, _owner.GetCommandParameters(),
                        whereClause, whereParameters, entitySetQueryExpression,
                        _owner.Select, _owner.GroupBy, _owner.GetSelectParameters(),
                        orderByBuilder,
                        _owner.Include);

            // We need to keep two copies, the unsorted and sorted because if the event does not
            // modify the query we'll need to revert back to the unsorted one so we can re-apply the
            // ESQL sort criteria as part of skip/take.
            ObjectQuery<T> query = queryBuilder.BuildBasicQuery(Context, arguments.RetrieveTotalRowCount);
            ObjectQuery<T> sortedQuery = queryBuilder.ApplyOrderBy(query);

            var queryEventArgs = new QueryCreatedEventArgs(sortedQuery);
            OnQueryCreated(queryEventArgs);

            var queryReturned = queryEventArgs.Query;
            bool wasQueryModified = (queryReturned != sortedQuery);
            if (wasQueryModified)
            {
                // Check that we still have an object query
                if (queryReturned as ObjectQuery == null)
                {
                    throw new InvalidOperationException(Strings.EntityDataSourceView_QueryCreatedNotAnObjectQuery(queryReturned.GetType().FullName, typeof(T).Name));
                }

                // Check that new type is at least substitutable
                var elementType = queryReturned.ElementType;
                if (elementType != typeof(T))
                {
                    if (!typeof(T).IsAssignableFrom(elementType))
                    {
                        throw new InvalidOperationException(Strings.EntityDataSourceView_QueryCreatedWrongType(elementType.Name, typeof(T).Name));
                    }

                    // Recreate the query builder for new type and then run it
                    var newQueryBuilderCreateMethod = typeof(EntityDataSourceObjectQueryBuilder<>).MakeGenericType(queryReturned.ElementType).GetMethod("Create", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, queryBuilderCreatorArgTypes, null);
                    Debug.Assert(newQueryBuilderCreateMethod != null, "Unable to bind to EntityDataSourceObjectQueryBuilder<T>.Create (static, non-visible) method");

                    // If the query results were wrapped before, we need to reset the wrapper to use the new type
                    if (_collectionManager.Mode == WrapperCollectionManager.ManagerMode.FlattenedEntities)
                    {
                        Debug.Assert(_collectionManager.FlattenedEntityCollection != null);

                        MetadataWorkspace workspace = Context.MetadataWorkspace;
                        EntityType newOSpaceType = workspace.GetItem<EntityType>(elementType.FullName, DataSpace.OSpace);
                        EntityType newCSpaceType = (EntityType)workspace.GetEdmSpaceType((StructuralType)newOSpaceType);

                        _collectionManager.CreateCollection(Context, EntitySet, newCSpaceType, WrapperCollectionManager.ManagerMode.FlattenedEntities);
                        
                        // Don't need to regenerate the where clause and parameters because they must be declaratively specified and if they reference
                        // properties that only exist in the subtype we would have already failed to generate the where clause previously and couldn't get this far
                        
                        // Regeneate the OrderByBuilder so that we can apply the sort expression later based on the new wrapper properties
                        orderByBuilder = new OrderByBuilder(arguments.SortExpression, _collectionManager.FlattenedEntityCollection,
                            _owner.OrderBy, _owner.AutoGenerateOrderByClause, _owner.OrderByParameters,
                            CanPage, //There's no need to generate the default OrderBy clause if paging is disabled. Prevents an unnecessary sort at the server.
                            _owner);
                    }

                    object newQueryBuilder = newQueryBuilderCreateMethod.Invoke(null, new object[] {
                         arguments,
                        _owner.CommandText, _owner.GetCommandParameters(),
                        whereClause, whereParameters, entitySetQueryExpression,
                        _owner.Select, _owner.GroupBy, _owner.GetSelectParameters(),
                        orderByBuilder,
                        _owner.Include });
                    return (IEnumerable)_continueSelectMethod.MakeGenericMethod(elementType).Invoke(this, new object[] { arguments, newQueryBuilder, queryReturned, wasQueryModified });
                }

                // Type hasn't changed, we can dispatch as normal
                query = queryReturned as ObjectQuery<T>;
            }

            return ContinueSelectTyped<T>(arguments, queryBuilder, query, wasQueryModified);
        }

        private IEnumerable ContinueSelectTyped<T>(DataSourceSelectArguments arguments, EntityDataSourceQueryBuilder<T> queryBuilder, ObjectQuery<T> queryT, bool wasQueryModified)
        {
            // Reset the MergeOption to AppendOnly
            queryT.MergeOption = MergeOption.AppendOnly;
            queryT = queryBuilder.CompleteBuild(queryT, Context, arguments.RetrieveTotalRowCount, wasQueryModified);

            // SelectedEventArgs.TotalRowCount has three possible states:
            //  1. The databound control requests it via arguments.RetrieveTotalRowCount
            //     This returns the total number of rows on all pages.
            //     arguments.TotalRowCount is only set if arguments.RetrieveTotalRowCount is true.
            //  2. Paging is disabled via !CanPage.
            //     This returns the number of rows returned. On one page.
            //  3. Else it returns negative one.
            int totalRowCount = -1;

            if (arguments.RetrieveTotalRowCount)
            {
                // The Selected event args gets the total number of rows on all pages.
                totalRowCount = queryBuilder.TotalCount;
                // The databound control requests totalRowCount. We return the total rows on all pages.
                arguments.TotalRowCount = totalRowCount;
            }

            if (!_disableUpdates)
            {
                Debug.Assert(null != EntitySet, "Can't be updatable with a null EntitySet");
                EntityDataSourceUtil.
                  CheckNonPolymorphicTypeUsage(EntitySet.ElementType,
                                             Context.MetadataWorkspace.GetItemCollection(DataSpace.CSpace),
                                             _owner.EntityTypeFilter);
            }

            IEnumerable entities = null;
            try
            {
                entities = queryBuilder.Execute(queryT);
            }
            catch (Exception e)
            {
                entities = null;
                var selectedArgs = new EntityDataSourceSelectedEventArgs(e);
                OnSelected(selectedArgs);
                OnException(new DynamicValidatorEventArgs(e, DynamicDataSourceOperation.Select));
                if (!selectedArgs.ExceptionHandled)
                {
                    throw;
                }
            }

            // OnSelected outside "try" to prevent double-calling OnSelected if the first OnSelected throws
            if (null != entities)
            {
                if (!CanPage)
                {
                    // Paging is disabled, totalRowCount gets the number of rows returned from this query.
                    totalRowCount = ((IList)entities).Count;
                }
                OnSelected(new EntityDataSourceSelectedEventArgs(Context, entities, totalRowCount, arguments));
            }

            var checkEntitySet = wasQueryModified && !String.IsNullOrEmpty(_owner.EntitySetName) && (CanDelete || CanUpdate);

            if (_collectionManager.Mode == WrapperCollectionManager.ManagerMode.None && !checkEntitySet)
            {
                Debug.Assert(
                    _collectionManager.FlattenedEntityCollection == null && _collectionManager.UpdateCache == null,
                    "ManagerMode is None so both collections should be null.");

                return entities;
            }

            foreach (object element in entities)
            {
                var elementEntitySet = Context.ObjectStateManager.GetObjectStateEntry(element).EntitySet;
                if (elementEntitySet != EntitySet)
                {
                    throw new InvalidOperationException(Strings.EntityDataSourceView_EntitySetMismatchWithQueryResults(elementEntitySet, EntitySet));
                }
                if (_collectionManager.Mode != WrapperCollectionManager.ManagerMode.None)
                {
                    _collectionManager.AddWrappedEntity(element);
                }
            }

            // If EnableFlattening flag is true return flattened entities. Otherwise if we ended up here 
            // because we needed to cache values in the viewstate for further updates we return non-flattened entities. 
            // Same happens if neither of the above is true and we ended up here because we needed to verify that 
            // all entities are from the same entity set.
            return _collectionManager.Mode == WrapperCollectionManager.ManagerMode.FlattenedEntities ? 
                    _collectionManager.FlattenedEntityCollection : 
                    entities;
        }

        /// <summary>
        /// Restrict the query to the type specified by EntityTypeFilter.
        /// </summary>
        /// <returns></returns>
        private string GenerateEntitySetQueryExpression()
        {
            if (null == EntitySet)
            {
                return String.Empty;
            }

            string entitySetIdentifier = EntityDataSourceUtil.CreateEntitySqlSetIdentifier(EntitySet);

            if (String.IsNullOrEmpty(_owner.EntityTypeFilter))
            {
                return entitySetIdentifier;
            }

            Debug.Assert(EntityOSpaceType != null, "EntitySet is not null, EntityOSpaceType should also be defined.");

            // oftype ([Northwind].[Products], only [Northwind].[ActiveProducts])
            StringBuilder queryExpressionBuilder = new StringBuilder();
            queryExpressionBuilder.Append("oftype (");
            queryExpressionBuilder.Append(entitySetIdentifier);
            queryExpressionBuilder.Append(", only ");
            queryExpressionBuilder.Append(EntityDataSourceUtil.CreateEntitySqlTypeIdentifier(EntityOSpaceType));
            queryExpressionBuilder.Append(")");

            return queryExpressionBuilder.ToString();
        }

        #endregion ExecuteSelect

        #region ExecuteUpdate

        protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues)
        {
            if (!CanUpdate)
            {
                throw new InvalidOperationException(Strings.EntityDataSourceView_UpdateDisabledForThisControl);
            }

            ConstructContext();
            EntityDataSourceChangingEventArgs changingArgs;

            Dictionary<string, object> originalEntityValues = new Dictionary<string, object>();
            EntityDataSourceWrapperCollection wrapperCollection =
                new EntityDataSourceWrapperCollection(Context, EntitySet, CSpaceFilteredEntityType);
            EntityDataSourceWrapper modifiedEntityWrapper;
            try
            {               
                object entity = EntityDataSourceUtil.InitializeType(this.EntityClrType);
                Context.AddObject(_owner.FQEntitySetName, entity);
                modifiedEntityWrapper = new EntityDataSourceWrapper(wrapperCollection, entity);

                // Get the values in the following order
                // 1. Key values from the page
                // 2. Values from view state
                // 3. Values from oldValues
                ConvertProperties(keys, modifiedEntityWrapper.GetProperties(), /* ParameterCollection */null, originalEntityValues);
                GetValuesFromViewState(originalEntityValues);
                ConvertProperties(oldValues, modifiedEntityWrapper.GetProperties(), /* ParameterCollection */null, originalEntityValues);

                // Validate that we have values for key properties
                EntityDataSourceUtil.ValidateKeyPropertyValuesExist(modifiedEntityWrapper, originalEntityValues);

                // Populate the entity with values
                EntityDataSourceUtil.SetAllPropertiesWithVerification(modifiedEntityWrapper, originalEntityValues, /*overwrite*/true);
            }
            catch (EntityDataSourceValidationException e)
            {
                changingArgs = new EntityDataSourceChangingEventArgs(e);
                OnUpdating(changingArgs);
                OnException(new DynamicValidatorEventArgs(e, DynamicDataSourceOperation.Update));
                if (!changingArgs.ExceptionHandled)
                {
                    throw;
                }
                return -1;
            }

            //Modify the properties with the new values
            try
            {
                Context.AcceptAllChanges(); //Puts modifiedEntityWrapper into unchanged state.
                UpdateEntity(originalEntityValues, values, modifiedEntityWrapper, _owner.UpdateParameters);
            }
            catch (EntityDataSourceValidationException e)
            {
                changingArgs = new EntityDataSourceChangingEventArgs(e);
                OnUpdating(changingArgs);
                OnException(new DynamicValidatorEventArgs(e, DynamicDataSourceOperation.Update));
                if (!changingArgs.ExceptionHandled)
                {
                    throw;
                }
                return -1;
            }
            try
            {
                // Call DetectChanges to ensure the changes to the entity are reflected in the state manager
                Context.DetectChanges();
            }
            catch (Exception e)
            {
                changingArgs = new EntityDataSourceChangingEventArgs(e);
                OnUpdating(changingArgs);
                OnException(new DynamicValidatorEventArgs(e, DynamicDataSourceOperation.Update));
                if (!changingArgs.ExceptionHandled)
                {
                    throw;
                }
                return -1;
            }

            try
            {
                // Call DetectChanges to ensure the changes to the entity are reflected in the state manager
                Context.DetectChanges();
            }
            catch (Exception e)
            {
                changingArgs = new EntityDataSourceChangingEventArgs(e);
                OnUpdating(changingArgs);
                OnException(new DynamicValidatorEventArgs(e, DynamicDataSourceOperation.Update));
                if (!changingArgs.ExceptionHandled)
                {
                    throw;
                }
                return -1;
            }

            changingArgs = new EntityDataSourceChangingEventArgs(Context, modifiedEntityWrapper.WrappedEntity);
            OnUpdating(changingArgs);
            if (changingArgs.Cancel)
            {
                return -1;
            }

            try
            {
                Context.SaveChanges();
            }
            catch (Exception e)
            {
                // Catches SaveChanges exceptions.
                EntityDataSourceChangedEventArgs changedArgs = new EntityDataSourceChangedEventArgs(e);
                OnUpdated(changedArgs);
                OnException(new DynamicValidatorEventArgs(e, DynamicDataSourceOperation.Update));
                if (!changedArgs.ExceptionHandled)
                {
                    throw;
                }
                return -1;
            }
            OnUpdated(new EntityDataSourceChangedEventArgs(Context, modifiedEntityWrapper.WrappedEntity));
            return 1;

        }

        #endregion ExecuteUpdate

        #region ExecuteDelete
        protected override int ExecuteDelete(IDictionary keys, IDictionary oldValues)
        {
            if (!CanDelete)
            {
                throw new InvalidOperationException(Strings.EntityDataSourceView_DeleteDisabledForThiscontrol);
            }

            ConstructContext();

            EntityDataSourceWrapperCollection wrapperCollection = new EntityDataSourceWrapperCollection(Context, EntitySet, CSpaceFilteredEntityType);
            EntityDataSourceWrapper entityWrapper;
            object entity;
            EntityDataSourceChangingEventArgs changingArgs;
            try
            {
                entity = EntityDataSourceUtil.InitializeType(this.EntityClrType);
                Context.AddObject(_owner.FQEntitySetName, entity); // Add/AcceptAllChanges because wrappers must contain attached entities
                entityWrapper = new EntityDataSourceWrapper(wrapperCollection, entity);

                // Get the values in the following order
                // 1. Key values from the page
                // 2. Values from view state
                // 3. Values from oldValues
                Dictionary<string, object> entityValues = new Dictionary<string, object>();
                ConvertProperties(keys, entityWrapper.GetProperties(), _owner.DeleteParameters, entityValues);
                GetValuesFromViewState(entityValues);
                ConvertProperties(oldValues, entityWrapper.GetProperties(), _owner.DeleteParameters, entityValues);

                // Validate that we have values for key properties
                EntityDataSourceUtil.ValidateKeyPropertyValuesExist(entityWrapper, entityValues);

                // Populate the entity with values
                EntityDataSourceUtil.SetAllPropertiesWithVerification(entityWrapper, entityValues, /*overwrite*/true);

                Context.AcceptAllChanges(); //Force the entity just added into unchanged state. Wrapped entities must be tracked.
            }
            catch (EntityDataSourceValidationException e)
            {
                changingArgs = new EntityDataSourceChangingEventArgs(e);
                OnDeleting(changingArgs);
                OnException(new DynamicValidatorEventArgs(e, DynamicDataSourceOperation.Delete));
                if (!changingArgs.ExceptionHandled)
                {
                    throw;
                }
                return -1;
            }
            changingArgs = new EntityDataSourceChangingEventArgs(Context, entityWrapper.WrappedEntity);
            OnDeleting(changingArgs); //Outside "try" to prevent from begin called twice.
            if (changingArgs.Cancel)
            {
                return -1;
            }

            try
            {
                Context.DeleteObject(entityWrapper.WrappedEntity);
                Context.SaveChanges();
            }
            catch (Exception e)
            {
                // Catches errors on the context.
                EntityDataSourceChangedEventArgs changedArgs = new EntityDataSourceChangedEventArgs(e);
                OnDeleted(changedArgs);
                OnException(new DynamicValidatorEventArgs(e, DynamicDataSourceOperation.Delete));
                if (!changedArgs.ExceptionHandled)
                {
                    throw;
                }
                return -1;
            }
            OnDeleted(new EntityDataSourceChangedEventArgs(Context, entity)); //Outside "try" to prevent being called twice.
            return 1;
        }
        #endregion ExecuteDelete

        #region ExecuteInsert

        protected override int ExecuteInsert(IDictionary values)
        {
            if (!CanInsert)
            {
                throw new InvalidOperationException(Strings.EntityDataSourceView_InsertDisabledForThisControl);
            }

            ConstructContext();

            EntityDataSourceChangingEventArgs changingArgs;
            EntityDataSourceWrapperCollection wrapperCollection = new EntityDataSourceWrapperCollection(Context, EntitySet, CSpaceFilteredEntityType);
            EntityDataSourceWrapper entityWrapper;
            try
            {
                object entity = EntityDataSourceUtil.InitializeType(this.EntityClrType);
                Context.AddObject(_owner.FQEntitySetName, entity);
                entityWrapper = new EntityDataSourceWrapper(wrapperCollection, entity);
                CreateEntityForInsert(entityWrapper, values, _owner.InsertParameters);
            }
            catch (EntityDataSourceValidationException e)
            {
                changingArgs = new EntityDataSourceChangingEventArgs(e);
                OnInserting(changingArgs);
                OnException(new DynamicValidatorEventArgs(e, DynamicDataSourceOperation.Insert));
                if (!changingArgs.ExceptionHandled)
                {
                    throw;
                }
                return -1;
            }

            changingArgs = new EntityDataSourceChangingEventArgs(Context, entityWrapper.WrappedEntity);
            OnInserting(changingArgs); //Could return an entity to insert.
            if (changingArgs.Cancel)
            {
                return -1;
            }

            if (!Object.ReferenceEquals(entityWrapper.WrappedEntity, changingArgs.Entity))
            {
                Context.Detach(entityWrapper.WrappedEntity);
                Context.AddObject(_owner.EntitySetName, changingArgs.Entity);
                entityWrapper = new EntityDataSourceWrapper(wrapperCollection, changingArgs.Entity);
            }

            try
            {
                Context.SaveChanges();
            }
            catch (Exception e)
            {
                EntityDataSourceChangedEventArgs changedArgs = new EntityDataSourceChangedEventArgs(e);
                OnInserted(changedArgs);
                OnException(new DynamicValidatorEventArgs(e, DynamicDataSourceOperation.Insert));
                if (!changedArgs.ExceptionHandled)
                {
                    throw;
                }
                return -1;
            }
            OnInserted(new EntityDataSourceChangedEventArgs(Context, entityWrapper.WrappedEntity));
            return 1;
        }

        #endregion ExecuteInsert

        #region Public Methods
        public DataTable GetViewSchema()
        {
            EntityDataSourceViewSchema propTable;
            if (_owner.ValidateWrappable())
            {
                ConstructContext();
                EntityDataSourceWrapperCollection wrappers = new EntityDataSourceWrapperCollection(Context, EntitySet, CSpaceFilteredEntityType);
                propTable = new EntityDataSourceViewSchema(wrappers);
            }
            else
            {
                string where = _owner.Where;
                _owner.Where = "0=1";
                try
                {
                    DataSourceSelectArguments args = new DataSourceSelectArguments();
                    args.RetrieveTotalRowCount = false;
                    IEnumerable results = ExecuteSelect(args);
                    ITypedList typedList = results as ITypedList;
                    if (null != typedList)
                    {
                        propTable = new EntityDataSourceViewSchema(typedList);
                    }
                    else if (_owner.HasIdentity())
                    {
                        // If the results have an identity/primary keys, gather them from restricted type or the element type from the set the control is querying
                        // Make sure to use keys from the ObjectSpace type in case there were name mappings
                        EntityType entityType = Context.MetadataWorkspace.GetObjectSpaceType(CSpaceFilteredEntityType ?? EntitySet.ElementType) as EntityType;
                        propTable = new EntityDataSourceViewSchema(results, entityType.KeyMembers.Select(x => x.Name).ToArray());
                    }
                    else
                    {
                        propTable = new EntityDataSourceViewSchema(results);
                    }
                }
                finally
                {
                    _owner.Where = where;
                }
            }
            return propTable;
        }
        #endregion Public Methods

        #region Private Methods

        #region ExecuteSelect Support

        private void GenerateWhereClause(out string whereClause, out ObjectParameter[] whereParameters)
        {
            if (!_owner.AutoGenerateWhereClause)
            {
                whereClause = _owner.Where;
                whereParameters = _owner.GetWhereParameters();
                return;
            }

            //This is the automatically generated Where clause.
            IOrderedDictionary paramValues = _owner.WhereParameters.GetValues(_owner.HttpContext, _owner);
            // Under some conditions, the paramValues has a null entry.
            StringBuilder whereClauseBuilder = new StringBuilder();
            List<ObjectParameter> whereParameterList = new List<ObjectParameter>();
            bool first = true;

            int idx = 0;
            foreach (DictionaryEntry de in paramValues)
            {
                string propertyName = (string)(de.Key);
                if (0 < propertyName.Length && null != de.Value)
                {
                    if (!String.IsNullOrEmpty(_owner.EntitySetName) && !EntityDataSourceUtil.PropertyIsOnEntity(propertyName, _collectionManager.FlattenedEntityCollection, EntitySet, null))
                    {
                        throw new InvalidOperationException(Strings.EntityDataSourceView_PropertyDoesNotExistOnEntity(propertyName, EntityClrType.FullName));
                    }

                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        whereClauseBuilder.Append(" AND ");
                    }

                    string namedParameterName = "NamedParameter" + idx.ToString(CultureInfo.InvariantCulture);

                    whereClauseBuilder.Append(EntityDataSourceUtil.GetEntitySqlValueForColumnName(propertyName, _collectionManager.FlattenedEntityCollection));
                    whereClauseBuilder.Append("=@");
                    whereClauseBuilder.Append(namedParameterName);

                    whereParameterList.Add(new ObjectParameter(namedParameterName, de.Value));
                    idx++;
                }
            }

            whereParameters = whereParameterList.ToArray();
            whereClause = whereClauseBuilder.ToString();
        }

        #endregion ExecuteSelect Support

        #region Entity Creation Support for Update/Insert/Delete

        private object ConvertProperty(object origPropertyValue, PropertyDescriptor propertyDescriptor, WebControlParameterProxy referenceParameter)
        {
            if (null == propertyDescriptor)
            {
                return null;
            }

            object propValue = origPropertyValue;
            propValue = Parameter_GetValue(propValue, referenceParameter, true);
            propValue = EntityDataSourceUtil.ConvertType(propValue, propertyDescriptor.PropertyType, propertyDescriptor.Name);
            return propValue;
        }

        private void ConvertWCProperty(IDictionary values,
            Dictionary<string, object> convertedValues,
            List<string> visitedProperties,
            PropertyDescriptor pd,
            ParameterCollection referenceParameters,
            ref Dictionary<string, Exception> exceptions)
        {
            Debug.Assert(pd != null, "PropertyDescriptor is null");
            string propertyName = pd.Name;
            WebControlParameterProxy wcParameter = new WebControlParameterProxy(propertyName, referenceParameters, _owner);
            object propValue = null;
            object value = null;
            if (null != values)
            {
                value = values[propertyName];
            }
            try
            {
                propValue = ConvertProperty(value, pd, wcParameter);
            }
            catch (Exception e)
            {
                if (null == exceptions)
                {
                    exceptions = new Dictionary<string, Exception>();
                }
                exceptions.Add(propertyName, e);
            }
            convertedValues[propertyName] = propValue;
            visitedProperties.Add(propertyName);
        }
        private void ConvertProperties(IDictionary values, PropertyDescriptorCollection propertyDescriptors, ParameterCollection referenceParameters, Dictionary<string, object> convertedValues)
        {
            List<string> visitedProperties = new List<string>();
            Dictionary<string, Exception> exceptions = null;

            // "values" come from the web page. Either via keys or original values. This loops sets them as first priority
            foreach (string propertyName in values.Keys)
            {
                if (!convertedValues.ContainsKey(propertyName))
                {
                    PropertyDescriptor pd = propertyDescriptors.Find(propertyName, /*ignoreCase*/ false);
                    if (pd == null)
                    {
                        throw new InvalidOperationException(Strings.EntityDataSourceView_UnknownProperty(propertyName));
                    }
                    ConvertWCProperty(values, convertedValues, visitedProperties,
                        pd, referenceParameters, ref exceptions);
                }
                // else ignore the value because it is already contained in the set of output values
            }

            // If a property hasn't been set by visible columns or DataKeyNames, then we set them here.
            // "referenceParameters" are Insert, Update or DeleteParameters assigning default values to
            // columns that are not set by the control
            if (null != referenceParameters)
            {
                IOrderedDictionary referenceValues = referenceParameters.GetValues(_owner.HttpContext, _owner);
                foreach (string propertyName in referenceValues.Keys)
                {
                    if (!visitedProperties.Contains(propertyName))
                    {
                        PropertyDescriptor pd = propertyDescriptors.Find(propertyName, /*ignoreCase*/ false);
                        if (pd == null)
                        {
                            throw new InvalidOperationException(Strings.EntityDataSourceView_UnknownProperty(propertyName));
                        }
                        ConvertWCProperty(null, convertedValues, visitedProperties,
                            pd, referenceParameters, ref exceptions);
                    }
                }
            }

            if (null != exceptions)
            {
                // The IDynamicValidationException encapsulates all of the data conversion errors.
                // This exposes one of the encapsulated errors in its own message like:
                //   "Error while setting property 'ProductName': 'The value cannot be null.'."
                string key = exceptions.Keys.First();
                throw new EntityDataSourceValidationException(
                    Strings.EntityDataSourceView_DataConversionError(
                        key, exceptions[key].Message), exceptions);
            }
        }

        private void CreateEntityForInsert(EntityDataSourceWrapper entityWrapper, IDictionary values, ParameterCollection insertParameters)
        {
            EntityDataSourceUtil.ValidateWebControlParameterNames(entityWrapper, insertParameters, _owner);

            // Throws EntityDataSourceValidationException if data conversion fails
            Dictionary<string, object> entityValues = new Dictionary<string, object>();
            ConvertProperties(values, entityWrapper.GetProperties(), insertParameters, entityValues);

            EntityDataSourceUtil.SetAllPropertiesWithVerification(entityWrapper, entityValues, /*overwrite*/true);
        }

        private void UpdateEntity(Dictionary<string, object> originalEntityValues, IDictionary values,
                                  EntityDataSourceWrapper entityWrapper, ParameterCollection updateParameters)
        {
            EntityDataSourceUtil.ValidateWebControlParameterNames(entityWrapper, updateParameters, _owner);
            Dictionary<string, object> currentEntityValues = new Dictionary<string, object>();
            ConvertProperties(values, entityWrapper.GetProperties(), updateParameters, currentEntityValues);
            Dictionary<string, object> allModifiedProperties = new Dictionary<string, object>();

            // Compare the propertyValues from the original values from the page to those in the new "values" properties.
            // Note that the comparison is between the values that came back from the databound control, BUT
            // The values on the entity in the entityWrapper came from ViewState. We can't compare the values from the page
            // To those stored in ViewState in case they didn't roundtrip well. Hence this next loop.
            foreach (string propertyName in currentEntityValues.Keys)
            {
                object originalValue = null;
                originalEntityValues.TryGetValue(propertyName, out originalValue);
                object newValue = currentEntityValues[propertyName];

                if (!OriginalValueMatches(originalValue, newValue))
                {
                    allModifiedProperties[propertyName] = newValue;
                }
            }

            EntityDataSourceUtil.SetAllPropertiesWithVerification(entityWrapper, allModifiedProperties, /*overwrite*/false);
        }

        #endregion Entity Creation Support

        #region ViewState Storage

        internal void StoreOriginalPropertiesIntoViewState()
        {
            EntityDataSourceWrapperCollection wrapperCollection = _collectionManager.FlattenedEntityCollection ?? _collectionManager.UpdateCache;

            if (null == wrapperCollection ||
                0 == wrapperCollection.Count ||
                !_owner.StoreOriginalValuesInViewState ||
                (!CanDelete && !CanUpdate)) //Only store entities into viewstate if Delete or Update is enabled.
            {
                return;
            }

            _originalProperties = new Dictionary<string, ArrayList>();

            PropertyDescriptorCollection collection = wrapperCollection.GetItemProperties(null);

            // Retrieve the named properties from each entity and place them into the viewstate collection.
            foreach (EntityDataSourceWrapper wrapper in wrapperCollection)
            {
                foreach (PropertyDescriptor propertyDescriptor in collection)
                {
                    EntityDataSourceWrapperPropertyDescriptor wrapperPropertyDescriptor = (EntityDataSourceWrapperPropertyDescriptor)propertyDescriptor;

                    if (wrapperPropertyDescriptor.Column.IsInteresting && wrapperPropertyDescriptor.Column.IsScalar)
                    {
                        object property = wrapperPropertyDescriptor.GetValue(wrapper);
                        string propertyName = wrapperPropertyDescriptor.DisplayName;

                        if (!_originalProperties.ContainsKey(propertyName))
                        {
                            _originalProperties[propertyName] = new ArrayList();
                        }

                        (_originalProperties[propertyName]).Add(property);
                    }
                }
            }
        }

        private void GetValuesFromViewState(Dictionary<string, object> entityValues)
        {
            int idx = FindIdxOfPropertiesStoredInViewState(entityValues);

            if (0 <= idx) // "-1" indicates that the value was not found in ViewState
            {
                foreach (string propertyName in _originalProperties.Keys)
                {
                    object property = (_originalProperties[propertyName])[idx];
                    entityValues[propertyName] = property;
                }
            }
        }

        /// <summary>
        /// Returns the index of the entity in ViewState that has key values that match the values from the page.
        /// If a match is not found, it returns -1.
        /// </summary>
        /// <param name="mergedKeysAndOldValues"></param>
        /// <returns></returns>
        private int FindIdxOfPropertiesStoredInViewState(IDictionary mergedKeysAndOldValues)
        {
            if (null == _originalProperties || 0 == _originalProperties.Count)
            {
                return -1;
            }

            Dictionary<string, object> localMergedKeysAndOldValues = (Dictionary<string, object>)mergedKeysAndOldValues;

            // This get the number of entities from the first property's values.
            // The ArrayList that holds the values contains the count of the number of entities.
            int numEntities = (_originalProperties.First().Value).Count;
            for (int idx = 0; idx < numEntities; idx++)
            {
                bool match = true;
                foreach (EdmMember edmMember in KeyMembers)
                {
                    string propertyName = edmMember.Name;
                    // The page must return the key values.
                    if (!localMergedKeysAndOldValues.ContainsKey(propertyName))
                    {
                        match = false;
                        break;
                    }

                    object origValue = (_originalProperties[propertyName])[idx];
                    object pageValue = localMergedKeysAndOldValues[propertyName];
                    if (!OriginalValueMatches(origValue, pageValue))
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    return idx;
                }
            }
            return -1;
        }

        private bool OriginalValueMatches(object originalValue, object value)
        {
            if (null == originalValue)
            {
                if (null == value)
                {
                    return true;
                }
                return false;
            }

            // NOTE: Comparing IEnumerable contents instead of instances to ensure that
            // timestamp columns (of type byte[]) can be matched appropriately.
            IEnumerable originalValueEnumerable = originalValue as IEnumerable;
            IEnumerable valueEnumerable = value as IEnumerable;
            if ((originalValueEnumerable != null) && (valueEnumerable != null))
            {
                return EnumerableContentEquals(originalValueEnumerable, valueEnumerable);
            }

            return originalValue.Equals(value);
        }

        #endregion ViewState Storage

        #region Context and Query construction

        private void ConstructContext()
        {
            this.DisposeContext();

            Type contextType = null;
            EntityDataSourceContextCreatingEventArgs creatingArgs = new EntityDataSourceContextCreatingEventArgs();
            OnContextCreating(creatingArgs);
            if (null != creatingArgs.Context) //Context was created in event code
            {
                _ctx = creatingArgs.Context;
                contextType = _ctx.GetType();
            }
            else if (null != _owner.ContextType || !String.IsNullOrEmpty(_owner.ContextTypeName))
            {
                //Construct the context.
                if (null != _owner.ContextType)
                {
                    contextType = _owner.ContextType;
                }
                else
                {
                    contextType = System.Web.Compilation.BuildManager.GetType(_owner.ContextTypeName, /*throw on error*/ true);
                }

                ConstructorInfo ctxInfo = contextType.GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance,
                    null, System.Type.EmptyTypes, null);

                if (null == ctxInfo)
                {
                    throw new InvalidOperationException(Strings.EntityDataSourceView_NoParameterlessConstructorForTheContext);
                }

                _ctx = (ObjectContext)ctxInfo.Invoke(new object[0]);
            }
            else // Non-strongly-typed context built from ConnectionString and DefaultContainerName
            {
                if (!String.IsNullOrEmpty(_owner.DefaultContainerName) &&
                    !String.IsNullOrEmpty(_owner.ConnectionString))
                {
                    _ctx = new ObjectContext(_owner.ConnectionString);

                    if (System.Web.Hosting.HostingEnvironment.IsHosted)
                    {
                        // Since we don't have the type from the strongly-typed context,
                        // load from all of the referenced assemblies, including code from App_Code and the top-level directory:
                        // http://msdn2.microsoft.com/en-us/library/system.web.compilation.buildmanager.getreferencedassemblies.aspx
                        ICollection codeAssemblies = System.Web.Compilation.BuildManager.GetReferencedAssemblies();
                        foreach (Assembly assembly in codeAssemblies)
                        {
                            if (ShouldTryLoadTypesFrom(assembly))
                            {
                                try
                                {
                                    _ctx.MetadataWorkspace.LoadFromAssembly(assembly);
                                }
                                catch (ReflectionTypeLoadException)
                                {
                                    // BuildManager returns all assemblies that could possibly be involved in generating this page (e.g. 
                                    // assemblies that are defined in config files, assemblies generated for the files compiled on the fly,
                                    // assemblies that are in the bin folder. If for some reason (security, missing dependencies etc.) we are 
                                    // not able to load one of these just skip it and continue looking instead of dying. In vast majority of 
                                    // cases the assembly causing problem is not the assembly that contains the types we are interested in. 
                                    // If this exception happens for an assembly we are interested in then we will be missing OSpace type for 
                                    // a CSpace type and will throw when we will try using the type for the first time.
                                }
                            }
                        }
                    }
                }
                else if (!String.IsNullOrEmpty(_owner.DefaultContainerName) &&
                          null != _owner.Connection)
                {
                    _ctx = new ObjectContext(_owner.Connection);
                }
                else
                {
                    throw new InvalidOperationException(Strings.EntityDataSourceView_ObjectContextMustBeSpecified);
                }
                // Must set the DefaultContainerName for both of the above conditions.
                _ctx.DefaultContainerName = _owner.DefaultContainerName;
                contextType = typeof(ObjectContext);
            }

            _ctx.MetadataWorkspace.LoadFromAssembly(System.Reflection.Assembly.GetCallingAssembly());
            _ctx.MetadataWorkspace.LoadFromAssembly(contextType.Assembly);


            // Error Checking on the Context
            ValidateContainerName();

            OnContextCreated(new EntityDataSourceContextCreatedEventArgs(Context));
        }

        // QueryCreated Event
        private void OnQueryCreated(QueryCreatedEventArgs e)
        {
            var handler = (EventHandler<QueryCreatedEventArgs>)Events[EventQueryCreated];
            if (null != handler)
            {
                handler(this, e);
            }
        }

        [CLSCompliant(false)]
        public event EventHandler<QueryCreatedEventArgs> QueryCreated
        {
            add { Events.AddHandler(EventQueryCreated, value); }
            remove { Events.RemoveHandler(EventQueryCreated, value); }
        }

        internal void DisposeContext()
        {
            if (null != _ctx)
            {
                EntityDataSourceContextDisposingEventArgs disposeArgs = new EntityDataSourceContextDisposingEventArgs(_ctx);
                OnContextDisposing(disposeArgs);
                if (!disposeArgs.Cancel)
                {
                    _ctx.Dispose();
                    _ctx = null;
                }
            }
        }

        #endregion Context and Query construction
        #endregion Private Methods

        #region Public Overrides
        public override bool CanInsert
        {
            get { return _owner.EnableInsert && !_disableUpdates; }
        }
        public override bool CanUpdate
        {
            get { return _owner.EnableUpdate && !_disableUpdates; }
        }
        public override bool CanDelete
        {
            get { return _owner.EnableDelete && !_disableUpdates; }
        }
        public override bool CanSort
        {
            get { return _owner.AutoSort; }
        }
        public override bool CanPage
        {
            get { return _owner.AutoPage; }
        }
        public override bool CanRetrieveTotalRowCount
        {
            get { return _owner.AutoPage; }
        }
        #endregion Public Overrides

        #region Events

        //Oryx exception event
        private void OnException(DynamicValidatorEventArgs args)
        {
            var handler = (EventHandler<DynamicValidatorEventArgs>)Events[EventException];
            if (null != handler)
            {
                handler(this, args);
            }
        }
        public event EventHandler<DynamicValidatorEventArgs> Exception
        {
            add { Events.AddHandler(EventException, value); }
            remove { Events.RemoveHandler(EventException, value); }
        }

        // ContextCreating Event
        private void OnContextCreating(EntityDataSourceContextCreatingEventArgs e)
        {
            var handler = (EventHandler<EntityDataSourceContextCreatingEventArgs>)Events[EventContextCreating];
            if (null != handler)
            {
                handler(this, e);
            }
        }
        public event EventHandler<EntityDataSourceContextCreatingEventArgs> ContextCreating
        {
            add { Events.AddHandler(EventContextCreating, value); }
            remove { Events.RemoveHandler(EventContextCreating, value); }
        }

        // ContextCreated Event
        private void OnContextCreated(EntityDataSourceContextCreatedEventArgs e)
        {
            var handler = (EventHandler<EntityDataSourceContextCreatedEventArgs>)Events[EventContextCreated];
            if (null != handler)
            {
                handler(this, e);
            }
        }
        public event EventHandler<EntityDataSourceContextCreatedEventArgs> ContextCreated
        {
            add { Events.AddHandler(EventContextCreated, value); }
            remove { Events.RemoveHandler(EventContextCreated, value); }
        }

        // ContextDisposing Event
        private void OnContextDisposing(EntityDataSourceContextDisposingEventArgs e)
        {
            var handler = (EventHandler<EntityDataSourceContextDisposingEventArgs>)Events[EventContextDisposing];
            if (null != handler)
            {
                handler(this, e);
            }
        }
        public event EventHandler<EntityDataSourceContextDisposingEventArgs> ContextDisposing
        {
            add { Events.AddHandler(EventContextDisposing, value); }
            remove { Events.RemoveHandler(EventContextDisposing, value); }
        }

        // Selecting Event
        private void OnSelecting(EntityDataSourceSelectingEventArgs e)
        {
            var handler = (EventHandler<EntityDataSourceSelectingEventArgs>)Events[EventSelecting];
            if (null != handler)
            {
                handler(this, e);
            }
        }
        public event EventHandler<EntityDataSourceSelectingEventArgs> Selecting
        {
            add { Events.AddHandler(EventSelecting, value); }
            remove { Events.RemoveHandler(EventSelecting, value); }
        }

        // Selected Event
        private void OnSelected(EntityDataSourceSelectedEventArgs e)
        {
            var handler = (EventHandler<EntityDataSourceSelectedEventArgs>)Events[EventSelected];
            if (null != handler)
            {
                handler(this, e);
            }
        }
        public event EventHandler<EntityDataSourceSelectedEventArgs> Selected
        {
            add { Events.AddHandler(EventSelected, value); }
            remove { Events.RemoveHandler(EventSelected, value); }
        }

        // Deleting Event
        private void OnDeleting(EntityDataSourceChangingEventArgs e)
        {
            var handler = (EventHandler<EntityDataSourceChangingEventArgs>)Events[EventDeleting];
            if (null != handler)
            {
                handler(this, e);
            }
        }
        public event EventHandler<EntityDataSourceChangingEventArgs> Deleting
        {
            add { Events.AddHandler(EventDeleting, value); }
            remove { Events.RemoveHandler(EventDeleting, value); }
        }

        // Deleted Event
        private void OnDeleted(EntityDataSourceChangedEventArgs e)
        {
            var handler = (EventHandler<EntityDataSourceChangedEventArgs>)Events[EventDeleted];
            if (null != handler)
            {
                handler(this, e);
            }
        }
        public event EventHandler<EntityDataSourceChangedEventArgs> Deleted
        {
            add { Events.AddHandler(EventDeleted, value); }
            remove { Events.RemoveHandler(EventDeleted, value); }
        }

        //Inserting Event
        private void OnInserting(EntityDataSourceChangingEventArgs e)
        {
            var handler = (EventHandler<EntityDataSourceChangingEventArgs>)Events[EventInserting];
            if (handler != null)
            {
                handler(this, e);
            }
        }
        public event EventHandler<EntityDataSourceChangingEventArgs> Inserting
        {
            add { Events.AddHandler(EventInserting, value); }
            remove { Events.RemoveHandler(EventInserting, value); }
        }

        //Inserted Event
        private void OnInserted(EntityDataSourceChangedEventArgs e)
        {
            var handler = (EventHandler<EntityDataSourceChangedEventArgs>)Events[EventInserted];
            if (handler != null)
            {
                handler(this, e);
            }
        }
        public event EventHandler<EntityDataSourceChangedEventArgs> Inserted
        {
            add { Events.AddHandler(EventInserted, value); }
            remove { Events.RemoveHandler(EventInserted, value); }
        }

        //Updating Event
        private void OnUpdating(EntityDataSourceChangingEventArgs e)
        {
            var handler = (EventHandler<EntityDataSourceChangingEventArgs>)Events[EventUpdating];
            if (handler != null)
            {
                handler(this, e);
            }
        }
        public event EventHandler<EntityDataSourceChangingEventArgs> Updating
        {
            add { Events.AddHandler(EventUpdating, value); }
            remove { Events.RemoveHandler(EventUpdating, value); }
        }

        //Updated Event
        private void OnUpdated(EntityDataSourceChangedEventArgs e)
        {
            var handler = (EventHandler<EntityDataSourceChangedEventArgs>)Events[EventUpdated];
            if (handler != null)
            {
                handler(this, e);
            }
        }
        public event EventHandler<EntityDataSourceChangedEventArgs> Updated
        {
            add { Events.AddHandler(EventUpdated, value); }
            remove { Events.RemoveHandler(EventUpdated, value); }
        }


        internal void RaiseChangedEvent()
        {
            OnDataSourceViewChanged(EventArgs.Empty);
        }

        #endregion Events

        #region Private Properties

        private StructuralType EntityOSpaceType
        {
            get
            {
                // If the CSpaceType is not determinable (e.g. DataSource.EntitySetName is not specified),
                // then return null.
                if (null == EntityCSpaceType)
                {
                    return null;
                }
                StructuralType oSpaceType = Context.MetadataWorkspace.GetObjectSpaceType(EntityCSpaceType);
                return oSpaceType;
            }
        }

        // Returns the type for EntityTypeFilter, or 
        // null if both EntitySet and EntityTypeFilter are not specified.
        private EntityType CSpaceFilteredEntityType
        {
            get
            {
                EntityType cSpaceType = null;
                if (null == EntitySet)
                {
                    return null; //read-only scenario in which the EntitySet is not specified on the DataSource
                }

                // Return the type specified in EntityTypeFilter
                if (!String.IsNullOrEmpty(_owner.EntityTypeFilter))
                {
                    cSpaceType = (EntityType)Context.MetadataWorkspace.GetType(_owner.EntityTypeFilter, EntitySet.ElementType.NamespaceName, DataSpace.CSpace);
                    if (!EntityDataSourceUtil.IsTypeOrSubtypeOf(EntitySet.ElementType, cSpaceType, Context.MetadataWorkspace.GetItemCollection(DataSpace.CSpace)))
                    {
                        throw new InvalidOperationException(Strings.EntityDataSourceView_FilteredEntityTypeMustBeDerivableFromEntitySet(_owner.EntityTypeFilter, _owner.EntitySetName));
                    }
                    return cSpaceType;
                }

                return null;

            }
        }

        // Returns the actualy EntityCSpaceType to be returned from the query, either via
        // EntityTypeFilter or via the base type for the EntitySet.
        private EntityType EntityCSpaceType
        {
            get
            {
                EntityType cSpaceType = CSpaceFilteredEntityType;
                if (null != cSpaceType)
                {
                    return cSpaceType;
                }

                if (null != EntitySet)
                {
                    //If EntityDataSource.EntityTypeFilter is not specified, return the base type for the EntitySet.
                    cSpaceType = EntitySet.ElementType;
                }
                return cSpaceType;
            }

        }

        private EntityContainer EntityContainer
        {
            get
            {
                return Context.MetadataWorkspace.GetEntityContainer(ContainerName, DataSpace.CSpace);
            }
        }

        /// <summary>
        /// The EntitySet Associated with this DataSource. If EntityDataSource.EntitySetName is not set, then
        /// This property returns null.
        /// </summary>
        private EntitySet EntitySet
        {
            get
            {
                if (String.IsNullOrEmpty(_owner.EntitySetName))
                {
                    return null;
                }
                return EntityContainer.GetEntitySetByName(_owner.EntitySetName, /*ignoreCase*/ false);
            }
        }

        private Type EntityClrType
        {
            get
            {
                ObjectItemCollection objectItemCollection =
                    (ObjectItemCollection)(Context.MetadataWorkspace.GetItemCollection(DataSpace.OSpace));
                Type clrType = objectItemCollection.GetClrType(EntityOSpaceType);
                return clrType;
            }
        }

        private ObjectContext Context
        {
            get
            {
                Debug.Assert(null != _ctx, "The context hasn't yet been constructed");
                return _ctx;
            }
        }

        private ReadOnlyMetadataCollection<EdmMember> KeyMembers
        {
            get
            {
                if (null == _keyMembers)
                {
                    EntityContainer entityContainer = Context.MetadataWorkspace.GetEntityContainer(ContainerName, DataSpace.CSpace);
                    EntitySet entitySet = entityContainer.GetEntitySetByName(_owner.EntitySetName, false);

                    _keyMembers = ((EntityType)(entitySet.ElementType)).KeyMembers;
                }
                return _keyMembers;
            }
        }

        private string ContainerName
        {
            get
            {
                if (!string.IsNullOrEmpty(_owner.DefaultContainerName))
                {
                    return _owner.DefaultContainerName;
                }
                if (!string.IsNullOrEmpty(Context.DefaultContainerName))
                {
                    return Context.DefaultContainerName;
                }
                throw new InvalidOperationException(Strings.EntityDataSourceView_ContainerNameMustBeSpecified);
            }
        }

        #endregion Private Properties

        #region private getters

        #endregion private getters

        #region IStateManager implementation
        bool IStateManager.IsTrackingViewState
        {
            get { return _tracking; }
        }
        void IStateManager.LoadViewState(object savedState)
        {
            if (null != savedState)
            {
                var state = (Pair)savedState;
                _disableUpdates = (bool)state.First;
                _originalProperties = (Dictionary<string, ArrayList>)state.Second;
            }
        }

        object IStateManager.SaveViewState()
        {
            StoreOriginalPropertiesIntoViewState();
            return new Pair(_disableUpdates, _originalProperties);
        }
        void IStateManager.TrackViewState()
        {
            _tracking = true;
        }
        #endregion

        #region Utilities and Error Checking

        private void AddSupportedCapabilities(DataSourceSelectArguments arguments)
        {
            if (CanSort)
            {
                arguments.AddSupportedCapabilities(DataSourceCapabilities.Sort);
            }
            if (CanPage)
            {
                arguments.AddSupportedCapabilities(DataSourceCapabilities.Page);
                arguments.AddSupportedCapabilities(DataSourceCapabilities.RetrieveTotalRowCount);
            }
        }

        internal void ValidateEntitySetName()
        {
            EntityContainer entityContainer = Context.MetadataWorkspace.GetEntityContainer(ContainerName, DataSpace.CSpace);
            EntitySet entitySet;
            if (!entityContainer.TryGetEntitySetByName(_owner.EntitySetName, /*ignoreCase*/false, out entitySet))
            {
                throw new InvalidOperationException(Strings.EntityDataSourceView_EntitySetDoesNotExistOnTheContainer(_owner.EntitySetName));
            }
        }

        private void ValidateContainerName()
        {
            EntityContainer container;
            if (!Context.MetadataWorkspace.TryGetEntityContainer(ContainerName, DataSpace.CSpace, out container))
            {
                throw new InvalidOperationException(Strings.EntityDataSourceView_ContainerNameDoesNotExistOnTheContext(ContainerName));
            }
        }

        // From LinqDataSourceHelper. Timestamp columns (of type byte[]) are of type IEnumerable.
        // This routine compares all elements of the IEnumerable. 
        private static bool EnumerableContentEquals(IEnumerable enumerableA, IEnumerable enumerableB)
        {
            IEnumerator enumeratorA = enumerableA.GetEnumerator();
            IEnumerator enumeratorB = enumerableB.GetEnumerator();
            while (enumeratorA.MoveNext())
            {
                if (!enumeratorB.MoveNext())
                    return false;
                object itemA = enumeratorA.Current;
                object itemB = enumeratorB.Current;
                if (itemA == null)
                {
                    if (itemB != null)
                        return false;
                }
                else if (!itemA.Equals(itemB))
                    return false;
            }
            if (enumeratorB.MoveNext())
                return false;
            return true;
        }

        // This routine modified from System.Web.Ui.WebControls.
        private static object Parameter_GetValue(object value, WebControlParameterProxy parameter, bool ignoreNullableTypeChanges)
        {
            // Convert.ChangeType() throws if you attempt to convert to DBNull, so we have to special case it.
            if (parameter.TypeCode == TypeCode.DBNull)
            {
                return DBNull.Value;
            }

            // Get the value and convert it to the default value if it is null
            if (parameter.ConvertEmptyStringToNull)
            {
                string stringValue = value as string;
                if ((stringValue != null) && (stringValue.Length == 0))
                {
                    value = null;
                }

            }

            if (value == null) // Fill it with values from referenceParameters
            {
                // Use the parameter value if it is non-null 
                if (!parameter.HasValue)
                {
                    return value;
                }

                object parameterValue = parameter.Value;
                string valueString = parameterValue as String;
                if (parameter.TypeCode == TypeCode.String && parameter.ConvertEmptyStringToNull && String.IsNullOrEmpty(valueString))
                {
                    parameterValue = null;
                }

                if (null == parameterValue)
                {
                    return null;
                }
                value = parameterValue;
            }

            Debug.Assert(value != null, "Value should not be null at this point.");

            if (parameter.TypeCode == TypeCode.Object || parameter.TypeCode == TypeCode.Empty)
            {
                return value;
            }

            // For ObjectDataSource we special-case Nullable<T> and do nothing because these
            // types will get converted when we actually call the method.
            if (ignoreNullableTypeChanges)
            {
                Type valueType = value.GetType();
                if (valueType.IsGenericType && (valueType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    return value;
                }
            }
            return value = Convert.ChangeType(value, parameter.TypeCode, CultureInfo.CurrentCulture); ;
        }



        private static byte[] EcmaPublicKeyToken      = { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 }; // b77a5c561934e089
        private static byte[] MicrosoftPublicKeyToken = { 0xb0, 0x3f, 0x5f, 0x7f, 0x11, 0xd5, 0x0a, 0x3a }; // b03f5f7f11d50a3a
        private static byte[] SharedLibPublicKeyToken = { 0x31, 0xbf, 0x38, 0x56, 0xad, 0x36, 0x4e, 0x35 }; // 31bf3856ad364e35

        /// <summary>
        /// Checks whether to try loading types from the <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">Assembly to be checked.</param>
        /// <returns><c>true</c> if we should try loading types from this assembly. <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Assemblies that are part of .NET Framework don't have entity/complex/enum types that could correspond to EdmTypes. 
        /// Therefore we should not try loading types from these assemblies. There are a lot of types there so it is costly
        /// but we know that we would not find any interesting types there. This method filters out these assemblies.
        /// </remarks>
        private static bool ShouldTryLoadTypesFrom(Assembly assembly)
        {
            // assembly.GetName() required FileIOPermission and won't work in partial trust.
            // The workaround is to parse the assembly.FullName
            var asmPublicKeyToken = new AssemblyName(assembly.FullName).GetPublicKeyToken();

            Debug.Assert(asmPublicKeyToken != null);

            return !(asmPublicKeyToken.SequenceEqual(EcmaPublicKeyToken) ||
                      asmPublicKeyToken.SequenceEqual(MicrosoftPublicKeyToken) ||
                      asmPublicKeyToken.SequenceEqual(SharedLibPublicKeyToken));
        }

        #endregion Utilities

        //public override void Delete(IDictionary keys, IDictionary oldValues, DataSourceViewOperationCallback callback){}
        //public override void Insert(IDictionary values, DataSourceViewOperationCallback callback){}
        //public override void Select(DataSourceSelectArguments arguments, DataSourceViewSelectCallback callback){}
        //public override void Update(IDictionary keys, IDictionary values, IDictionary oldValues, DataSourceViewOperationCallback callback){}

    }
}
