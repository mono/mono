#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if MONO_STRICT
using System.Data.Linq.Implementation;
using System.Data.Linq.Sugar;
using System.Data.Linq.Identity;
using DbLinq.Util;
using AttributeMappingSource = System.Data.Linq.Mapping.AttributeMappingSource;
using MappingContext = System.Data.Linq.Mapping.MappingContext;
using DbLinq;
#else
using DbLinq.Data.Linq.Implementation;
using DbLinq.Data.Linq.Sugar;
using DbLinq.Data.Linq.Identity;
using DbLinq.Util;
using AttributeMappingSource = DbLinq.Data.Linq.Mapping.AttributeMappingSource;
using MappingContext = DbLinq.Data.Linq.Mapping.MappingContext;
using System.Data.Linq;
#endif

using DbLinq.Factory;
using DbLinq.Logging;
using DbLinq.Vendor;
using DbLinq.Data.Linq.Database;
using DbLinq.Data.Linq.Database.Implementation;
using System.Linq.Expressions;

#if MONO_STRICT
namespace System.Data.Linq
#else
namespace DbLinq.Data.Linq
#endif
{
    public partial class DataContext : IDisposable
    {
        private readonly Dictionary<string, ITable> _tableMap = new Dictionary<string, ITable>();

        public MetaModel Mapping { get; private set; }
        // PC question: at ctor, we get a IDbConnection and the Connection property exposes a DbConnection
        //              WTF?
        public DbConnection Connection { get { return DatabaseContext.Connection as DbConnection; } }

        // all properties below are set public to optionally be injected
        internal IVendor Vendor { get; set; }
        internal IQueryBuilder QueryBuilder { get; set; }
        internal IQueryRunner QueryRunner { get; set; }
        internal IMemberModificationHandler MemberModificationHandler { get; set; }
        internal IDatabaseContext DatabaseContext { get; private set; }
        internal ILogger Logger { get; set; }
        // /all properties...

        // entities may be registered in 3 sets: InsertList, EntityMap and DeleteList
        // InsertList is for new entities
        // DeleteList is for entities to be deleted
        // EntityMap is the cache: entities are alive in the DataContext, identified by their PK (IdentityKey)
        // an entity can only live in one of the three caches, so the DataContext will provide 6 methods:
        // 3 to register in each list, 3 to unregister
        //internal IEntityMap EntityMap { get; set; }
        //internal readonly EntityList InsertList = new EntityList();
        //internal readonly EntityList DeleteList = new EntityList();
        private readonly EntityTracker entityTracker = new EntityTracker();

        private IIdentityReaderFactory identityReaderFactory;
        private readonly IDictionary<Type, IIdentityReader> identityReaders = new Dictionary<Type, IIdentityReader>();


        /// <summary>
        /// The default behavior creates one MappingContext.
        /// </summary>
        [DBLinqExtended]
        internal virtual MappingContext _MappingContext { get; set; }

        [DBLinqExtended]
        internal IVendorProvider _VendorProvider { get; set; }

        [DbLinqToDo]
        public DataContext(IDbConnection connection, MappingSource mapping)
        {
            Init(new DatabaseContext(connection), mapping, null);
        }

        [DbLinqToDo]
        public DataContext(IDbConnection connection)
        {
            Init(new DatabaseContext(connection), null, null);
        }

        [DbLinqToDo]
        public DataContext(string fileOrServerOrConnection, MappingSource mapping)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        public DataContext(string fileOrServerOrConnection)
        {
            throw new NotImplementedException();
        }

        private void Init(IDatabaseContext databaseContext, MappingSource mappingSource, IVendor vendor)
        {

            if (databaseContext == null)
                throw new ArgumentNullException("databaseContext");

            Logger = ObjectFactory.Get<ILogger>();

            _VendorProvider = ObjectFactory.Get<IVendorProvider>();
            if (vendor == null)
                Vendor = _VendorProvider.FindVendorByProviderType(typeof(SqlClient.Sql2005Provider));
            else
                Vendor = vendor;

            DatabaseContext = databaseContext;

            MemberModificationHandler = ObjectFactory.Create<IMemberModificationHandler>(); // not a singleton: object is stateful
            QueryBuilder = ObjectFactory.Get<IQueryBuilder>();
            QueryRunner = ObjectFactory.Get<IQueryRunner>();

            //EntityMap = ObjectFactory.Create<IEntityMap>();
            identityReaderFactory = ObjectFactory.Get<IIdentityReaderFactory>();

            _MappingContext = new MappingContext();

            // initialize the mapping information
            if (mappingSource == null)
                mappingSource = new AttributeMappingSource();
            Mapping = mappingSource.GetModel(GetType());
        }


        public Table<TEntity> GetTable<TEntity>() where TEntity : class
        {
            return (Table<TEntity>)GetTable(typeof(TEntity));
        }

        public ITable GetTable(Type type)
        {
            lock (_tableMap)
            {
                string tableName = type.FullName;
                ITable tableExisting;
                if (_tableMap.TryGetValue(tableName, out tableExisting))
                    return tableExisting;

                var tableNew = Activator.CreateInstance(
                                  typeof(Table<>).MakeGenericType(type)
                                  , BindingFlags.NonPublic | BindingFlags.Instance
                                  , null
                                  , new object[] { this }
                                  , System.Globalization.CultureInfo.CurrentCulture) as ITable;

                _tableMap[tableName] = tableNew;
                return tableNew;
            }
        }

        public void SubmitChanges()
        {
            SubmitChanges(ConflictMode.FailOnFirstConflict);
        }

        /// <summary>
        /// Pings database
        /// </summary>
        /// <returns></returns>
        public bool DatabaseExists()
        {
            try
            {
                return Vendor.Ping(this);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Commits all pending changes to database 
        /// </summary>
        /// <param name="failureMode"></param>
        public virtual void SubmitChanges(ConflictMode failureMode)
        {
            using (DatabaseContext.OpenConnection()) //ConnMgr will close connection for us
            using (IDatabaseTransaction transaction = DatabaseContext.Transaction())
            {
                var queryContext = new QueryContext(this);
                var entityTracks = entityTracker.EnumerateAll().ToList();
                foreach (var entityTrack in entityTracks)
                {
                    switch (entityTrack.EntityState)
                    {
                    case EntityState.ToInsert:
                        var insertQuery = QueryBuilder.GetInsertQuery(entityTrack.Entity, queryContext);
                        QueryRunner.Insert(entityTrack.Entity, insertQuery);
                        Register(entityTrack.Entity);
                        break;
                    case EntityState.ToWatch:
                        if (MemberModificationHandler.IsModified(entityTrack.Entity, Mapping))
                        {
                            var modifiedMembers = MemberModificationHandler.GetModifiedProperties(entityTrack.Entity, Mapping);
                            var updateQuery = QueryBuilder.GetUpdateQuery(entityTrack.Entity, modifiedMembers, queryContext);
                            QueryRunner.Update(entityTrack.Entity, updateQuery, modifiedMembers);

                            RegisterUpdateAgain(entityTrack.Entity);
                        }
                        break;
                    case EntityState.ToDelete:
                        var deleteQuery = QueryBuilder.GetDeleteQuery(entityTrack.Entity, queryContext);
                        QueryRunner.Delete(entityTrack.Entity, deleteQuery);

                        UnregisterDelete(entityTrack.Entity);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                    }
                }
                // TODO: handle conflicts (which can only occur when concurrency mode is implemented)
                transaction.Commit();
            }
        }

        /// <summary>
        /// TODO - allow generated methods to call into stored procedures
        /// </summary>
        [DBLinqExtended]
        internal IExecuteResult _ExecuteMethodCall(DataContext context, System.Reflection.MethodInfo method, params object[] sqlParams)
        {
            using (DatabaseContext.OpenConnection())
            {
                System.Data.Linq.IExecuteResult result = Vendor.ExecuteMethodCall(context, method, sqlParams);
                return result;
            }
        }

        [DbLinqToDo]
        protected IExecuteResult ExecuteMethodCall(object instance, System.Reflection.MethodInfo methodInfo, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        #region Identity management

        [DBLinqExtended]
        internal IIdentityReader _GetIdentityReader(Type t)
        {
            IIdentityReader identityReader;
            lock (identityReaders)
            {
                if (!identityReaders.TryGetValue(t, out identityReader))
                {
                    identityReader = identityReaderFactory.GetReader(t, this);
                    identityReaders[t] = identityReader;
                }
            }
            return identityReader;
        }

        [DBLinqExtended]
        internal object _GetRegisteredEntity(object entity)
        {
            // TODO: check what is faster: by identity or by ref
            var identityReader = _GetIdentityReader(entity.GetType());
            var identityKey = identityReader.GetIdentityKey(entity);
            if (identityKey == null) // if we don't have an entitykey here, it means that the entity has no PK
                return entity;
            // even 
            var registeredEntityTrack = entityTracker.FindByIdentity(identityKey);
            if (registeredEntityTrack != null)
                return registeredEntityTrack.Entity;
            return null;
        }

        //internal object GetRegisteredEntityByKey(IdentityKey identityKey)
        //{
        //    return EntityMap[identityKey];
        //}

        /// <summary>
        /// Registers an entity in a watch state
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [DBLinqExtended]
        internal object _GetOrRegisterEntity(object entity)
        {
            var identityReader = _GetIdentityReader(entity.GetType());
            var identityKey = identityReader.GetIdentityKey(entity);
            SetEntitySetsQueries(entity);
            SetEntityRefQueries(entity);

            // if we have no identity, we can't track it
            if (identityKey == null)
                return entity;

            // try to find an already registered entity and return it
            var registeredEntityTrack = entityTracker.FindByIdentity(identityKey);
            if (registeredEntityTrack != null)
                return registeredEntityTrack.Entity;

            // otherwise, register and return
            entityTracker.RegisterToWatch(entity, identityKey);
            return entity;
        }

        readonly IDataMapper DataMapper = ObjectFactory.Get<IDataMapper>();
        private void SetEntityRefQueries(object entity)
        {
            Type thisType = entity.GetType();
            IList<MemberInfo> properties = DataMapper.GetEntityRefAssociations(thisType);


            foreach (PropertyInfo prop in properties)
            {
                //example of entityRef:Order.Employee
                AssociationAttribute associationInfo = prop.GetAttribute<AssociationAttribute>();
                Type otherTableType = prop.PropertyType;
                IList<MemberInfo> otherPKs = DataMapper.GetPrimaryKeys(Mapping.GetTable(otherTableType));

                if (otherPKs.Count > 1)
                    throw new NotSupportedException("Multiple keys object not supported yet.");

                var otherTable = GetTable(otherTableType);

                //ie:EmployeeTerritories.EmployeeID

                var thisForeignKeyProperty = thisType.GetProperty(associationInfo.ThisKey);
                object thisForeignKeyValue = thisForeignKeyProperty.GetValue(entity, null);

                IEnumerable query = null;
                if (thisForeignKeyValue != null)
                {
                    ParameterExpression p = Expression.Parameter(otherTableType, "other");
                    Expression predicate;
                    if (!(thisForeignKeyProperty.PropertyType.IsNullable()))
                    {
                        predicate = Expression.Equal(Expression.MakeMemberAccess(p, otherPKs.First()),
                                                                    Expression.Constant(thisForeignKeyValue));
                    }
                    else
                    {
                        var ValueProperty = thisForeignKeyProperty.PropertyType.GetProperty("Value");
                        predicate = Expression.Equal(Expression.MakeMemberAccess(p, otherPKs.First()),
                                                                 Expression.Constant(ValueProperty.GetValue(thisForeignKeyValue, null)));
                    }

                    query = GetOtherTableQuery(predicate, p, otherTableType, otherTable) as IEnumerable;
                    //it would be interesting surround the above query with a .Take(1) expression for performance.
                }


                FieldInfo entityRefField = entity.GetType().GetField(associationInfo.Storage, BindingFlags.NonPublic | BindingFlags.Instance);
                object entityRefValue = null;
                if (query != null)
                    entityRefValue = Activator.CreateInstance(entityRefField.FieldType, query);
                else
                    entityRefValue = Activator.CreateInstance(entityRefField.FieldType);
                entityRefField.SetValue(entity, entityRefValue);
            }
        }

        /// <summary>
        /// This method is executed when the entity is being registered. Each EntitySet property has a internal query that can be set using the EntitySet.SetSource method.
        /// Here we set the query source of each EntitySetProperty
        /// </summary>
        /// <param name="entity"></param>
        private void SetEntitySetsQueries(object entity)
        {
            IList<MemberInfo> properties = DataMapper.GetEntitySetAssociations(entity.GetType());
            IList<MemberInfo> thisPKs = DataMapper.GetPrimaryKeys(Mapping.GetTable(entity.GetType()));

            if (thisPKs.Count > 1 && properties.Any())
                throw new NotSupportedException("Multiple keys object not supported yet.");

            object primaryKeyValue = (thisPKs.First() as PropertyInfo).GetValue(entity, null);


            foreach (PropertyInfo prop in properties)
            {
                //example of entitySet: Employee.EmployeeTerritories
                var associationInfo = prop.GetAttribute<AssociationAttribute>();
                Type otherTableType = prop.PropertyType.GetGenericArguments().First();

                //other table:EmployeeTerritories
                var otherTable = GetTable(otherTableType);
                //other table member:EmployeeTerritories.EmployeeID
                var otherTableMember = otherTableType.GetProperty(associationInfo.OtherKey);


                ParameterExpression p = Expression.Parameter(otherTableType, "other");
                Expression predicate;
                if (!(otherTableMember.PropertyType.IsNullable()))
                {
                    predicate = Expression.Equal(Expression.MakeMemberAccess(p, otherTableMember),
                                                                Expression.Constant(primaryKeyValue));
                }
                else
                {
                    var ValueProperty = otherTableMember.PropertyType.GetProperty("Value");
                    predicate = Expression.Equal(Expression.MakeMemberAccess(
                                                                Expression.MakeMemberAccess(p, otherTableMember),
                                                                ValueProperty),
                                                             Expression.Constant(primaryKeyValue));
                }

                var query = GetOtherTableQuery(predicate, p, otherTableType, otherTable);

                var entitySetValue = prop.GetValue(entity, null);

                if (entitySetValue == null)
                {
                    entitySetValue = Activator.CreateInstance(prop.PropertyType);
                    prop.SetValue(entity, entitySetValue, null);
                }

                var setSourceMethod = entitySetValue.GetType().GetMethod("SetSource");
                setSourceMethod.Invoke(entitySetValue, new[] { query });
                //employee.EmployeeTerritories.SetSource(Table[EmployeesTerritories].Where(other=>other.employeeID="WARTH"))
            }
        }

        private object GetOtherTableQuery(Expression predicate, ParameterExpression parameter, Type otherTableType, IQueryable otherTable)
        {
            //predicate: other.EmployeeID== "WARTH"
            Expression lambdaPredicate = Expression.Lambda(predicate, parameter);
            //lambdaPredicate: other=>other.EmployeeID== "WARTH"

            var whereMethod = typeof(Queryable)
                              .GetMethods().First(m => m.Name == "Where")
                              .MakeGenericMethod(otherTableType);


            Expression call = Expression.Call(whereMethod, otherTable.Expression, lambdaPredicate);
            //Table[EmployeesTerritories].Where(other=>other.employeeID="WARTH")

            return otherTable.Provider.CreateQuery(call);
        }

        #endregion

        #region Insert/Update/Delete management

        /// <summary>
        /// Registers an entity for insert
        /// </summary>
        /// <param name="entity"></param>
        internal void RegisterInsert(object entity)
        {
            entityTracker.RegisterToInsert(entity);
        }

        /// <summary>
        /// Registers an entity for update
        /// The entity will be updated only if some of its members have changed after the registration
        /// </summary>
        /// <param name="entity"></param>
        internal void RegisterUpdate(object entity)
        {
            var identityReader = _GetIdentityReader(entity.GetType());
            var identityKey = identityReader.GetIdentityKey(entity);
            // if we have no key, we can not watch
            if (identityKey == null)
                return;
            // register entity
            entityTracker.RegisterToWatch(entity, identityKey);
        }

        /// <summary>
        /// Registers or re-registers an entity and clears its state
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal object Register(object entity)
        {
            var registeredEntity = _GetOrRegisterEntity(entity);
            // the fact of registering again clears the modified state, so we're... clear with that
            MemberModificationHandler.Register(registeredEntity, Mapping);
            return registeredEntity;
        }

        /// <summary>
        /// Registers an entity for update
        /// The entity will be updated only if some of its members have changed after the registration
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entityOriginalState"></param>
        internal void RegisterUpdate(object entity, object entityOriginalState)
        {
            RegisterUpdate(entity);
            MemberModificationHandler.Register(entity, entityOriginalState, Mapping);
        }

        /// <summary>
        /// Clears the current state, and marks the object as clean
        /// </summary>
        /// <param name="entity"></param>
        internal void RegisterUpdateAgain(object entity)
        {
            MemberModificationHandler.ClearModified(entity, Mapping);
        }

        /// <summary>
        /// Registers an entity for delete
        /// </summary>
        /// <param name="entity"></param>
        internal void RegisterDelete(object entity)
        {
            entityTracker.RegisterToDelete(entity);
        }

        /// <summary>
        /// Unregisters entity after deletion
        /// </summary>
        /// <param name="entity"></param>
        internal void UnregisterDelete(object entity)
        {
            entityTracker.RegisterDeleted(entity);
        }

        #endregion

        /// <summary>
        /// Changed object determine 
        /// </summary>
        /// <returns>Lists of inserted, updated, deleted objects</returns>
        public ChangeSet GetChangeSet()
        {
            var inserts = new List<object>();
            var updates = new List<object>();
            var deletes = new List<object>();
            foreach (var entityTrack in entityTracker.EnumerateAll())
            {
                switch (entityTrack.EntityState)
                {
                case EntityState.ToInsert:
                    inserts.Add(entityTrack.Entity);
                    break;
                case EntityState.ToWatch:
                    if (MemberModificationHandler.IsModified(entityTrack.Entity, Mapping))
                        updates.Add(entityTrack.Entity);
                    break;
                case EntityState.ToDelete:
                    deletes.Add(entityTrack.Entity);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
                }
            }
            return new ChangeSet(inserts, updates, deletes);
        }

        /// <summary>
        /// use ExecuteCommand to call raw SQL
        /// </summary>
        public int ExecuteCommand(string command, params object[] parameters)
        {
            var directQuery = QueryBuilder.GetDirectQuery(command, new QueryContext(this));
            return QueryRunner.Execute(directQuery, parameters);
        }

        /// <summary>
        /// Execute raw SQL query and return object
        /// </summary>
        public IEnumerable<TResult> ExecuteQuery<TResult>(string query, params object[] parameters) where TResult : class, new()
        {
            GetTable<TResult>();
            foreach (TResult result in ExecuteQuery(typeof(TResult), query, parameters))
                yield return result;
        }

        public IEnumerable ExecuteQuery(Type elementType, string query, params object[] parameters)
        {
            var queryContext = new QueryContext(this);
            var directQuery = QueryBuilder.GetDirectQuery(query, queryContext);
            return QueryRunner.ExecuteSelect(elementType, directQuery, parameters);
        }

        /// <summary>
        /// TODO: DataLoadOptions ds = new DataLoadOptions(); ds.LoadWith<Customer>(p => p.Orders);
        /// </summary>
        [DbLinqToDo]
        public DataLoadOptions LoadOptions
        {
            get;
            set;
        }

        public DbTransaction Transaction { get; set; }

        public IEnumerable<TResult> Translate<TResult>(DbDataReader reader)
        {
            foreach (TResult result in Translate(typeof(TResult), reader))
                yield return result;
        }

        public IMultipleResults Translate(DbDataReader reader)
        {
            throw new NotImplementedException();
        }

        public IEnumerable Translate(Type elementType, DbDataReader reader)
        {
            return QueryRunner.EnumerateResult(elementType, reader, this);
        }

        public void Dispose()
        {
            //connection closing should not be done here.
            //read: http://msdn2.microsoft.com/en-us/library/bb292288.aspx
        }

        [DbLinqToDo]
        protected virtual void Dispose(bool disposing)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a IDbDataAdapter. Used internally by Vendors
        /// </summary>
        /// <returns></returns>
        internal IDbDataAdapter CreateDataAdapter()
        {
            return DatabaseContext.CreateDataAdapter();
        }

        /// <summary>
        /// Sets a TextWriter where generated SQL commands are written
        /// </summary>
        public System.IO.TextWriter Log { get; set; }

        /// <summary>
        /// Writes the generated SQL on Log (if not null)
        /// Internal helper
        /// </summary>
        /// <param name="sql"></param>
        internal void WriteLog(string sql)
        {
            if (Log != null)
            {
                // Log example:
                //SELECT [t0].[FirstName] AS [Name], [t1].[FirstName] AS [ReportsTo]
                //FROM [dbo].[Employees] AS [t0]
                //LEFT OUTER JOIN [dbo].[Employees] AS [t1] ON [t1].[EmployeeID] = [t0].[ReportsTo]
                //-- Context: SqlProvider(Sql2008) Model: AttributedMetaModel Build: 3.5.30729.1
                Log.WriteLine(sql);
                Log.Write("--");
                Log.Write(" Context: {0}", Vendor.VendorName);
                Log.Write(" Model: {0}", Mapping.GetType().Name);
                Log.Write(" Build: {0}", Assembly.GetExecutingAssembly().GetName().Version);
                Log.WriteLine();
            }
        }

        [DbLinqToDo]
        public bool ObjectTrackingEnabled
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        [DbLinqToDo]
        public int CommandTimeout
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        [DbLinqToDo]
        public bool DeferredLoadingEnabled
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        [DbLinqToDo]
        public ChangeConflictCollection ChangeConflicts
        {
            get { throw new NotImplementedException(); }
        }

        [DbLinqToDo]
        public DbCommand GetCommand(IQueryable query)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        public void Refresh(System.Data.Linq.RefreshMode mode, System.Collections.IEnumerable entities)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        public void Refresh(RefreshMode mode, params object[] entities)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        public void Refresh(RefreshMode mode, object entity)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        public void DeleteDatabase()
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        public void CreateDatabase()
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        protected internal IQueryable<TResult> CreateMethodCallQuery<TResult>(object instance, MethodInfo methodInfo, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        protected internal void ExecuteDynamicDelete(object entity)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        protected internal void ExecuteDynamicInsert(object entity)
        {
            throw new NotImplementedException();
        }

        [DbLinqToDo]
        protected internal void ExecuteDynamicUpdate(object entity)
        {
            throw new NotImplementedException();
        }
    }
}
