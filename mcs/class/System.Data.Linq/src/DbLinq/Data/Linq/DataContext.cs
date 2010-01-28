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
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

#if MONO_STRICT
using AttributeMappingSource  = System.Data.Linq.Mapping.AttributeMappingSource;
#else
using AttributeMappingSource  = DbLinq.Data.Linq.Mapping.AttributeMappingSource;
#endif

using DbLinq;
using DbLinq.Data.Linq;
using DbLinq.Data.Linq.Database;
using DbLinq.Data.Linq.Database.Implementation;
using DbLinq.Data.Linq.Identity;
using DbLinq.Data.Linq.Implementation;
using DbLinq.Data.Linq.Mapping;
using DbLinq.Data.Linq.Sugar;
using DbLinq.Factory;
using DbLinq.Util;
using DbLinq.Vendor;

#if MONO_STRICT
namespace System.Data.Linq
#else
namespace DbLinq.Data.Linq
#endif
{
    public partial class DataContext : IDisposable
    {
        //private readonly Dictionary<string, ITable> _tableMap = new Dictionary<string, ITable>();
        private readonly Dictionary<Type, ITable> _tableMap = new Dictionary<Type, ITable>();

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
        // /all properties...

        private bool objectTrackingEnabled = true;
        private bool deferredLoadingEnabled = true;

        private bool queryCacheEnabled = false;

        /// <summary>
        /// Disable the QueryCache: this is surely good for rarely used Select, since preparing
        /// the SelectQuery to be cached could require more time than build the sql from scratch.
        /// </summary>
        [DBLinqExtended]
        public bool QueryCacheEnabled 
        {
            get { return queryCacheEnabled; }
            set { queryCacheEnabled = value; }
        }

        private IEntityTracker currentTransactionEntities;
        private IEntityTracker CurrentTransactionEntities
        {
            get
            {
                if (this.currentTransactionEntities == null)
                {
                    if (this.ObjectTrackingEnabled)
                        this.currentTransactionEntities = new EntityTracker();
                    else
                        this.currentTransactionEntities = new DisabledEntityTracker();
                }
                return this.currentTransactionEntities;
            }
        }

        private IEntityTracker allTrackedEntities;
        private IEntityTracker AllTrackedEntities
        {
            get
            {
                if (this.allTrackedEntities == null)
                {
                    allTrackedEntities = ObjectTrackingEnabled
                        ? (IEntityTracker) new EntityTracker()
                        : (IEntityTracker) new DisabledEntityTracker();
                }
                return this.allTrackedEntities;
            }
        }

        private IIdentityReaderFactory identityReaderFactory;
        private readonly IDictionary<Type, IIdentityReader> identityReaders = new Dictionary<Type, IIdentityReader>();

        /// <summary>
        /// The default behavior creates one MappingContext.
        /// </summary>
        [DBLinqExtended]
        internal virtual MappingContext _MappingContext { get; set; }

        [DBLinqExtended]
        internal IVendorProvider _VendorProvider { get; set; }

        public DataContext(IDbConnection connection, MappingSource mapping)
        {
            Profiler.At("START DataContext(IDbConnection, MappingSource)");
            Init(new DatabaseContext(connection), mapping, null);
            Profiler.At("END DataContext(IDbConnection, MappingSource)");
        }

        public DataContext(IDbConnection connection)
        {
            Profiler.At("START DataContext(IDbConnection)");
            if (connection == null)
                throw new ArgumentNullException("connection");

            Init(new DatabaseContext(connection), null, null);
            Profiler.At("END DataContext(IDbConnection)");
        }

        [DbLinqToDo]
        public DataContext(string fileOrServerOrConnection, MappingSource mapping)
        {
            Profiler.At("START DataContext(string, MappingSource)");
            if (fileOrServerOrConnection == null)
                throw new ArgumentNullException("fileOrServerOrConnection");
            if (mapping == null)
                throw new ArgumentNullException("mapping");

            if (File.Exists(fileOrServerOrConnection))
                throw new NotImplementedException("File names not supported.");

            // Is this a decent server name check?
            // It assumes that the connection string will have at least 2
            // parameters (separated by ';')
            if (!fileOrServerOrConnection.Contains(";"))
                throw new NotImplementedException("Server name not supported.");

            // Assume it's a connection string...
            IVendor ivendor = GetVendor(ref fileOrServerOrConnection);

            IDbConnection dbConnection = ivendor.CreateDbConnection(fileOrServerOrConnection);
            Init(new DatabaseContext(dbConnection), mapping, ivendor);
            Profiler.At("END DataContext(string, MappingSource)");
        }

        /// <summary>
        /// Construct DataContext, given a connectionString.
        /// To determine which DB type to go against, we look for 'DbLinqProvider=xxx' substring.
        /// If not found, we assume that we are dealing with MS Sql Server.
        /// 
        /// Valid values are names of provider DLLs (or any other DLL containing an IVendor implementation)
        /// DbLinqProvider=Mysql
        /// DbLinqProvider=Oracle etc.
        /// </summary>
        /// <param name="connectionString">specifies file or server connection</param>
        [DbLinqToDo]
        public DataContext(string connectionString)
        {
            Profiler.At("START DataContext(string)");
            IVendor ivendor = GetVendor(ref connectionString);

            IDbConnection dbConnection = ivendor.CreateDbConnection(connectionString);
            Init(new DatabaseContext(dbConnection), null, ivendor);

            Profiler.At("END DataContext(string)");
        }

        private IVendor GetVendor(ref string connectionString)
        {
            if (connectionString == null)
                throw new ArgumentNullException("connectionString");

            Assembly assy;
            string vendorClassToLoad;
            GetVendorInfo(ref connectionString, out assy, out vendorClassToLoad);

            var types =
                from type in assy.GetTypes()
                where type.Name.ToLowerInvariant() == vendorClassToLoad.ToLowerInvariant() &&
                    type.GetInterfaces().Contains(typeof(IVendor)) &&
                    type.GetConstructor(Type.EmptyTypes) != null
                select type;
            if (!types.Any())
            {
                throw new ArgumentException(string.Format("Found no IVendor class in assembly `{0}' named `{1}' having a default constructor.",
                    assy.GetName().Name, vendorClassToLoad));
            }
            else if (types.Count() > 1)
            {
                throw new ArgumentException(string.Format("Found too many IVendor classes in assembly `{0}' named `{1}' having a default constructor.",
                    assy.GetName().Name, vendorClassToLoad));
            }
            return (IVendor) Activator.CreateInstance(types.First());
        }

        private void GetVendorInfo(ref string connectionString, out Assembly assembly, out string typeName)
        {
            System.Text.RegularExpressions.Regex reProvider
                = new System.Text.RegularExpressions.Regex(@"DbLinqProvider=([\w\.]+);?");

            string assemblyFile = null;
            string vendor;
            if (!reProvider.IsMatch(connectionString))
            {
                vendor       = "SqlServer";
                assemblyFile = "DbLinq.SqlServer.dll";
            }
            else
            {
                var match    = reProvider.Match(connectionString);
                vendor       = match.Groups[1].Value;
                assemblyFile = "DbLinq." + vendor + ".dll";

                //plain DbLinq - non MONO: 
                //IVendor classes are in DLLs such as "DbLinq.MySql.dll"
                if (vendor.Contains("."))
                {
                    //already fully qualified DLL name?
                    throw new ArgumentException("Please provide a short name, such as 'MySql', not '" + vendor + "'");
                }

                //shorten: "DbLinqProvider=X;Server=Y" -> ";Server=Y"
                connectionString = reProvider.Replace(connectionString, "");
            }

            typeName = vendor + "Vendor";

            try
            {
#if MONO_STRICT
                assembly = typeof (DataContext).Assembly; // System.Data.Linq.dll
#else
                //TODO: check if DLL is already loaded?
                assembly = Assembly.LoadFrom(assemblyFile);
#endif
            }
            catch (Exception e)
            {
                throw new ArgumentException(
                        string.Format(
                            "Unable to load the `{0}' DbLinq vendor within assembly `{1}'.",
                            assemblyFile, vendor),
                        "connectionString", e);
            }
        }

        private void Init(IDatabaseContext databaseContext, MappingSource mappingSource, IVendor vendor)
        {
            if (databaseContext == null)
                throw new ArgumentNullException("databaseContext");

            // Yes, .NET throws an NRE for this.  Why it's not ArgumentNullException, I couldn't tell you.
            if (databaseContext.Connection.ConnectionString == null)
                throw new NullReferenceException();

            string connectionString = databaseContext.Connection.ConnectionString;
            _VendorProvider = ObjectFactory.Get<IVendorProvider>();
            Vendor = vendor ?? 
                (connectionString != null ? GetVendor(ref connectionString) : null) ??
                _VendorProvider.FindVendorByProviderType(typeof(SqlClient.Sql2005Provider));
            
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

        /// <summary>
        /// Checks if the table is allready mapped or maps it if not.
        /// </summary>
        /// <param name="tableType">Type of the table.</param>
        /// <exception cref="InvalidOperationException">Thrown if the table is not mappable.</exception>
        private void CheckTableMapping(Type tableType)
        {
            //This will throw an exception if the table is not found
            if(Mapping.GetTable(tableType) == null)
            {
                throw new InvalidOperationException("The type '" + tableType.Name + "' is not mapped as a Table.");
            }
        }

        /// <summary>
        /// Returns a Table for the type TEntity.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the type TEntity is not mappable as a Table.</exception>
        /// <typeparam name="TEntity">The table type.</typeparam>
        public Table<TEntity> GetTable<TEntity>() where TEntity : class
        {
            return (Table<TEntity>)GetTable(typeof(TEntity));
        }

        /// <summary>
        /// Returns a Table for the given type.
        /// </summary>
        /// <param name="type">The table type.</param>
        /// <exception cref="InvalidOperationException">If the type is not mappable as a Table.</exception>
        public ITable GetTable(Type type)
        {
            Profiler.At("DataContext.GetTable(typeof({0}))", type != null ? type.Name : null);
            ITable tableExisting;
            if (_tableMap.TryGetValue(type, out tableExisting))
                return tableExisting;

            //Check for table mapping
            CheckTableMapping(type);

            var tableNew = Activator.CreateInstance(
                              typeof(Table<>).MakeGenericType(type)
                              , BindingFlags.NonPublic | BindingFlags.Instance
                              , null
                              , new object[] { this }
                              , System.Globalization.CultureInfo.CurrentCulture) as ITable;

            _tableMap[type] = tableNew;
            return tableNew;
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
            if (this.objectTrackingEnabled == false)
                throw new InvalidOperationException("Object tracking is not enabled for the current data context instance.");
            using (DatabaseContext.OpenConnection()) //ConnMgr will close connection for us
            {
                if (Transaction != null)
                    SubmitChangesImpl(failureMode);
                else
                {
                    using (IDatabaseTransaction transaction = DatabaseContext.Transaction())
                    {
                        try
                        {
                            Transaction = (DbTransaction) transaction.Transaction;
                            SubmitChangesImpl(failureMode);
                            // TODO: handle conflicts (which can only occur when concurrency mode is implemented)
                            transaction.Commit();
                        }
                        finally
                        {
                            Transaction = null;
                        }
                    }
                }
            }
        }

        void SubmitChangesImpl(ConflictMode failureMode)
        {
            var queryContext = new QueryContext(this);

            // There's no sense in updating an entity when it's going to 
            // be deleted in the current transaction, so do deletes first.
            foreach (var entityTrack in CurrentTransactionEntities.EnumerateAll().ToList())
            {
                switch (entityTrack.EntityState)
                {
                    case EntityState.ToDelete:
                        var deleteQuery = QueryBuilder.GetDeleteQuery(entityTrack.Entity, queryContext);
                        QueryRunner.Delete(entityTrack.Entity, deleteQuery);

                        UnregisterDelete(entityTrack.Entity);
                        AllTrackedEntities.RegisterToDelete(entityTrack.Entity);
                        AllTrackedEntities.RegisterDeleted(entityTrack.Entity);
                        break;
                    default:
                        // ignore.
                        break;
                }
            }
            foreach (var entityTrack in CurrentTransactionEntities.EnumerateAll()
                    .Concat(AllTrackedEntities.EnumerateAll())
                    .ToList())
            {
                switch (entityTrack.EntityState)
                {
                    case EntityState.ToInsert:
                        foreach (var toInsert in GetReferencedObjects(entityTrack.Entity))
                        {
                            InsertEntity(toInsert, queryContext);
                        }
                        break;
                    case EntityState.ToWatch:
                        foreach (var toUpdate in GetReferencedObjects(entityTrack.Entity))
                        {
                            UpdateEntity(toUpdate, queryContext);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private IEnumerable<object> GetReferencedObjects(object value)
        {
            var values = new EntitySet<object>();
            FillReferencedObjects(value, values);
            return values;
        }

        // Breadth-first traversal of an object graph
        private void FillReferencedObjects(object parent, EntitySet<object> values)
        {
            if (parent == null)
                return;
            var children = new Queue<object>();
			children.Enqueue(parent);
			while (children.Count > 0)
			{
                object value = children.Dequeue();
                values.Add(value);
                IEnumerable<MetaAssociation> associationList = Mapping.GetMetaType(value.GetType()).Associations.Where(a => !a.IsForeignKey);
                if (associationList.Any())
			    {
				    foreach (MetaAssociation association in associationList)
                    {
                        var memberData = association.ThisMember;
                        var entitySetValue = memberData.Member.GetMemberValue(value);

                        if (entitySetValue != null)
                        {
						    var hasLoadedOrAssignedValues = entitySetValue.GetType().GetProperty("HasLoadedOrAssignedValues");
						    if (!((bool)hasLoadedOrAssignedValues.GetValue(entitySetValue, null)))
							    continue;   // execution deferred; ignore.
						    foreach (var o in ((IEnumerable)entitySetValue))
							    children.Enqueue(o);
					    }
                    }
                }
			}
        }

        private void InsertEntity(object entity, QueryContext queryContext)
        {
            var insertQuery = QueryBuilder.GetInsertQuery(entity, queryContext);
            QueryRunner.Insert(entity, insertQuery);
            Register(entity);
            UpdateReferencedObjects(entity);
            MoveToAllTrackedEntities(entity, true);
        }

        private void UpdateEntity(object entity, QueryContext queryContext)
        {
            if (!AllTrackedEntities.ContainsReference(entity))
                InsertEntity(entity, queryContext);
            else if (MemberModificationHandler.IsModified(entity, Mapping))
            {
                var modifiedMembers = MemberModificationHandler.GetModifiedProperties(entity, Mapping);
                var updateQuery = QueryBuilder.GetUpdateQuery(entity, modifiedMembers, queryContext);
                QueryRunner.Update(entity, updateQuery, modifiedMembers);

                RegisterUpdateAgain(entity);
                UpdateReferencedObjects(entity);
                MoveToAllTrackedEntities(entity, false);
            }
        }

        private void UpdateReferencedObjects(object root)
        {
            var metaType = Mapping.GetMetaType(root.GetType());
            foreach (var assoc in metaType.Associations)
            {
                var memberData = assoc.ThisMember;
				//This is not correct - AutoSyncing applies to auto-updating columns, such as a TimeStamp, not to foreign key associations, which is always automatically synched
				//Confirmed against default .NET l2sql - association columns are always set, even if AutoSync==AutoSync.Never
				//if (memberData.Association.ThisKey.Any(m => (m.AutoSync != AutoSync.Always) && (m.AutoSync != sync)))
                //    continue;
                var oks = memberData.Association.OtherKey.Select(m => m.StorageMember).ToList();
                if (oks.Count == 0)
                    continue;
                var pks = memberData.Association.ThisKey
                    .Select(m => m.StorageMember.GetMemberValue(root))
                    .ToList();
                if (pks.Count != oks.Count)
                    throw new InvalidOperationException(
                        string.Format("Count of primary keys ({0}) doesn't match count of other keys ({1}).",
                            pks.Count, oks.Count));
                var members = memberData.Member.GetMemberValue(root) as IEnumerable;
                if (members == null)
                    continue;
                foreach (var member in members)
                {
                    for (int i = 0; i < pks.Count; ++i)
                    {
                        oks[i].SetMemberValue(member, pks[i]);
                    }
                }
            }
        }

        private void MoveToAllTrackedEntities(object entity, bool insert)
        {
            if (!ObjectTrackingEnabled)
                return;
            if (CurrentTransactionEntities.ContainsReference(entity))
            {
                CurrentTransactionEntities.RegisterToDelete(entity);
                if (!insert)
                    CurrentTransactionEntities.RegisterDeleted(entity);
            }
            if (!AllTrackedEntities.ContainsReference(entity))
            {
                var identityReader = _GetIdentityReader(entity.GetType());
                AllTrackedEntities.RegisterToWatch(entity, identityReader.GetIdentityKey(entity));
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
            if (!identityReaders.TryGetValue(t, out identityReader))
            {
                identityReader = identityReaderFactory.GetReader(t, this);
                identityReaders[t] = identityReader;
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
            var registeredEntityTrack = 
                CurrentTransactionEntities.FindByIdentity(identityKey) ??
                AllTrackedEntities.FindByIdentity(identityKey);
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
            var registeredEntityTrack = 
                CurrentTransactionEntities.FindByIdentity(identityKey) ??
                AllTrackedEntities.FindByIdentity(identityKey);
            if (registeredEntityTrack != null)
                return registeredEntityTrack.Entity;

            // otherwise, register and return
            AllTrackedEntities.RegisterToWatch(entity, identityKey);
            return entity;
        }

        readonly IDataMapper DataMapper = ObjectFactory.Get<IDataMapper>();
		private void SetEntityRefQueries(object entity)
		{
            if (!this.deferredLoadingEnabled)
                return;

            // BUG: This is ignoring External Mappings from XmlMappingSource.

			Type thisType = entity.GetType();
			IEnumerable<MetaAssociation> associationList = Mapping.GetMetaType(entity.GetType()).Associations.Where(a => a.IsForeignKey);
			foreach (MetaAssociation association in associationList)
			{
				//example of entityRef:Order.Employee
				var memberData = association.ThisMember;
				Type otherTableType = association.OtherType.Type;
				ParameterExpression p = Expression.Parameter(otherTableType, "other");

				var otherTable = GetTable(otherTableType);

				//ie:EmployeeTerritories.EmployeeID
				var foreignKeys = memberData.Association.ThisKey;
				BinaryExpression predicate = null;
				var otherPKs = memberData.Association.OtherKey;
				IEnumerator<MetaDataMember> otherPKEnumerator = otherPKs.GetEnumerator();

				if (otherPKs.Count != foreignKeys.Count)
					throw new InvalidOperationException("Foreign keys don't match ThisKey");
				foreach (MetaDataMember key in foreignKeys)
				{
					otherPKEnumerator.MoveNext();

					var thisForeignKeyProperty = (PropertyInfo)key.Member;
					object thisForeignKeyValue = thisForeignKeyProperty.GetValue(entity, null);

					if (thisForeignKeyValue != null)
					{
						BinaryExpression keyPredicate;
						if (!(thisForeignKeyProperty.PropertyType.IsNullable()))
						{
							keyPredicate = Expression.Equal(Expression.MakeMemberAccess(p, otherPKEnumerator.Current.Member),
																		Expression.Constant(thisForeignKeyValue));
						}
						else
						{
							var ValueProperty = thisForeignKeyProperty.PropertyType.GetProperty("Value");
							keyPredicate = Expression.Equal(Expression.MakeMemberAccess(p, otherPKEnumerator.Current.Member),
																	 Expression.Constant(ValueProperty.GetValue(thisForeignKeyValue, null)));
						}

						if (predicate == null)
							predicate = keyPredicate;
						else
							predicate = Expression.And(predicate, keyPredicate);
					}
				}
				IEnumerable query = null;
				if (predicate != null)
				{
					query = GetOtherTableQuery(predicate, p, otherTableType, otherTable) as IEnumerable;
					//it would be interesting surround the above query with a .Take(1) expression for performance.
				}

				// If no separate Storage is specified, use the member directly
				MemberInfo storage = memberData.StorageMember;
				if (storage == null)
					storage = memberData.Member;

				 // Check that the storage is a field or a writable property
				if (!(storage is FieldInfo) && !(storage is PropertyInfo && ((PropertyInfo)storage).CanWrite)) {
					throw new InvalidOperationException(String.Format(
						"Member {0}.{1} is not a field nor a writable property",
						storage.DeclaringType, storage.Name));
				}

				Type storageType = storage.GetMemberType();

				object entityRefValue = null;
				if (query != null)
					entityRefValue = Activator.CreateInstance(storageType, query);
				else
					entityRefValue = Activator.CreateInstance(storageType);

				storage.SetMemberValue(entity, entityRefValue);
			}
		}

        /// <summary>
        /// This method is executed when the entity is being registered. Each EntitySet property has a internal query that can be set using the EntitySet.SetSource method.
        /// Here we set the query source of each EntitySetProperty
        /// </summary>
        /// <param name="entity"></param>
        private void SetEntitySetsQueries(object entity)
        {
            if (!this.deferredLoadingEnabled)
                return;

            // BUG: This is ignoring External Mappings from XmlMappingSource.

			IEnumerable<MetaAssociation> associationList = Mapping.GetMetaType(entity.GetType()).Associations.Where(a => !a.IsForeignKey);

			if (associationList.Any())
			{
				foreach (MetaAssociation association in associationList)
                {
					//example of entitySet: Employee.EmployeeTerritories
					var memberData = association.ThisMember;
					Type otherTableType = association.OtherType.Type;
                    ParameterExpression p = Expression.Parameter(otherTableType, "other");

                    //other table:EmployeeTerritories
                    var otherTable = GetTable(otherTableType);

					var otherKeys = memberData.Association.OtherKey;
					var thisKeys = memberData.Association.ThisKey;
                    if (otherKeys.Count != thisKeys.Count)
                        throw new InvalidOperationException("This keys don't match OtherKey");
                    BinaryExpression predicate = null;
                    IEnumerator<MetaDataMember> thisKeyEnumerator = thisKeys.GetEnumerator();
					foreach (MetaDataMember otherKey in otherKeys)
                    {
                        thisKeyEnumerator.MoveNext();
                        //other table member:EmployeeTerritories.EmployeeID
						var otherTableMember = (PropertyInfo)otherKey.Member;

                        BinaryExpression keyPredicate;
                        if (!(otherTableMember.PropertyType.IsNullable()))
                        {
                            keyPredicate = Expression.Equal(Expression.MakeMemberAccess(p, otherTableMember),
                                                                        Expression.Constant(thisKeyEnumerator.Current.Member.GetMemberValue(entity)));
                        }
                        else
                        {
                            var ValueProperty = otherTableMember.PropertyType.GetProperty("Value");
                            keyPredicate = Expression.Equal(Expression.MakeMemberAccess(
                                                                        Expression.MakeMemberAccess(p, otherTableMember),
                                                                        ValueProperty),
                                                                     Expression.Constant(thisKeyEnumerator.Current.Member.GetMemberValue(entity)));
                        }
                        if (predicate == null)
                            predicate = keyPredicate;
                        else
                            predicate = Expression.And(predicate, keyPredicate);
                    }

                    var query = GetOtherTableQuery(predicate, p, otherTableType, otherTable);

					var entitySetValue = memberData.Member.GetMemberValue(entity);

                    if (entitySetValue == null)
                    {
						entitySetValue = Activator.CreateInstance(memberData.Member.GetMemberType());
						memberData.Member.SetMemberValue(entity, entitySetValue);
                    }

                    var hasLoadedOrAssignedValues = entitySetValue.GetType().GetProperty("HasLoadedOrAssignedValues");
                    if ((bool)hasLoadedOrAssignedValues.GetValue(entitySetValue, null))
                        continue;

                    var setSourceMethod = entitySetValue.GetType().GetMethod("SetSource");
                    setSourceMethod.Invoke(entitySetValue, new[] { query });
                    //employee.EmployeeTerritories.SetSource(Table[EmployeesTerritories].Where(other=>other.employeeID="WARTH"))
                }
            }
        }

		private static MethodInfo _WhereMethod = typeof(Queryable).GetMethods().First(m => m.Name == "Where");
        internal object GetOtherTableQuery(Expression predicate, ParameterExpression parameter, Type otherTableType, IQueryable otherTable)
        {
            //predicate: other.EmployeeID== "WARTH"
            Expression lambdaPredicate = Expression.Lambda(predicate, parameter);
            //lambdaPredicate: other=>other.EmployeeID== "WARTH"

			Expression call = Expression.Call(_WhereMethod.MakeGenericMethod(otherTableType), otherTable.Expression, lambdaPredicate);
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
            CurrentTransactionEntities.RegisterToInsert(entity);
        }

        private void DoRegisterUpdate(object entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (!this.objectTrackingEnabled)
                return;

            var identityReader = _GetIdentityReader(entity.GetType());
            var identityKey = identityReader.GetIdentityKey(entity);
            // if we have no key, we can not watch
            if (identityKey == null || identityKey.Keys.Count == 0)
                return;
            // register entity
            AllTrackedEntities.RegisterToWatch(entity, identityKey);
        }

        /// <summary>
        /// Registers an entity for update
        /// The entity will be updated only if some of its members have changed after the registration
        /// </summary>
        /// <param name="entity"></param>
        internal void RegisterUpdate(object entity)
        {
            DoRegisterUpdate(entity);
			MemberModificationHandler.Register(entity, Mapping);
        }

        /// <summary>
        /// Registers or re-registers an entity and clears its state
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal object Register(object entity)
        {
            if (! this.objectTrackingEnabled)
                return entity;
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
            if (!this.objectTrackingEnabled)
                return;
            DoRegisterUpdate(entity);
            MemberModificationHandler.Register(entity, entityOriginalState, Mapping);
        }

        /// <summary>
        /// Clears the current state, and marks the object as clean
        /// </summary>
        /// <param name="entity"></param>
        internal void RegisterUpdateAgain(object entity)
        {
            if (!this.objectTrackingEnabled)
                return;
            MemberModificationHandler.ClearModified(entity, Mapping);
        }

        /// <summary>
        /// Registers an entity for delete
        /// </summary>
        /// <param name="entity"></param>
        internal void RegisterDelete(object entity)
        {
            if (!this.objectTrackingEnabled)
                return;
            CurrentTransactionEntities.RegisterToDelete(entity);
        }

        /// <summary>
        /// Unregisters entity after deletion
        /// </summary>
        /// <param name="entity"></param>
        internal void UnregisterDelete(object entity)
        {
            if (!this.objectTrackingEnabled)
                return;
            CurrentTransactionEntities.RegisterDeleted(entity);
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
            foreach (var entityTrack in CurrentTransactionEntities.EnumerateAll()
                    .Concat(AllTrackedEntities.EnumerateAll()))
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
            if (query == null)
                throw new ArgumentNullException("query");

            return CreateExecuteQueryEnumerable<TResult>(query, parameters);
        }

        private IEnumerable<TResult> CreateExecuteQueryEnumerable<TResult>(string query, object[] parameters)
            where TResult : class, new()
        {
            foreach (TResult result in ExecuteQuery(typeof(TResult), query, parameters))
                yield return result;
        }

        public IEnumerable ExecuteQuery(Type elementType, string query, params object[] parameters)
        {
            if (elementType == null)
                throw new ArgumentNullException("elementType");
            if (query == null)
                throw new ArgumentNullException("query");

            var queryContext = new QueryContext(this);
            var directQuery = QueryBuilder.GetDirectQuery(query, queryContext);
            return QueryRunner.ExecuteSelect(elementType, directQuery, parameters);
        }

        /// <summary>
        /// Gets or sets the load options
        /// </summary>
        [DbLinqToDo]
		public DataLoadOptions LoadOptions
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

        public DbTransaction Transaction { get; set; }

        /// <summary>
        /// Runs the given reader and returns columns.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        public IEnumerable<TResult> Translate<TResult>(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            return CreateTranslateIterator<TResult>(reader);
        }

        IEnumerable<TResult> CreateTranslateIterator<TResult>(DbDataReader reader)
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
            if (elementType == null)
                throw new ArgumentNullException("elementType");
            if (reader == null)
                throw new ArgumentNullException("reader");

            return QueryRunner.EnumerateResult(elementType, reader, this);
        }

        public void Dispose()
        {
            //connection closing should not be done here.
            //read: http://msdn2.microsoft.com/en-us/library/bb292288.aspx

			//We own the instance of MemberModificationHandler - we must unregister listeners of entities we attached to
			MemberModificationHandler.UnregisterAll();
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
        public TextWriter Log { get; set; }

        /// <summary>
        /// Writes text on Log (if not null)
        /// Internal helper
        /// </summary>
        /// <param name="text"></param>
        internal void WriteLog(string text)
        {
            if (Log != null)
                Log.WriteLine(text);
        }

        /// <summary>
        /// Write an IDbCommand to Log (if non null)
        /// </summary>
        /// <param name="command"></param>
        internal void WriteLog(IDbCommand command)
        {
            if (Log != null)
            {
                Log.WriteLine(command.CommandText);
                foreach (IDbDataParameter parameter in command.Parameters)
                    WriteLog(parameter);
                Log.Write("--");
                Log.Write(" Context: {0}", Vendor.VendorName);
                Log.Write(" Model: {0}", Mapping.GetType().Name);
                Log.Write(" Build: {0}", Assembly.GetExecutingAssembly().GetName().Version);
                Log.WriteLine();
            }
        }

        /// <summary>
        /// Writes and IDbDataParameter to Log (if non null)
        /// </summary>
        /// <param name="parameter"></param>
        internal void WriteLog(IDbDataParameter parameter)
        {
            if (Log != null)
            {
                // -- @p0: Input Int (Size = 0; Prec = 0; Scale = 0) [2]
                // -- <name>: <direction> <type> (...) [<value>]
                Log.WriteLine("-- {0}: {1} {2} (Size = {3}; Prec = {4}; Scale = {5}) [{6}]",
                    parameter.ParameterName, parameter.Direction, parameter.DbType,
                    parameter.Size, parameter.Precision, parameter.Scale, parameter.Value);
            }
        }

        public bool ObjectTrackingEnabled
        {
            get { return this.objectTrackingEnabled; }
            set 
            {
                if (this.currentTransactionEntities != null && value != this.objectTrackingEnabled)
                    throw new InvalidOperationException("Data context options cannot be modified after results have been returned from a query.");
                this.objectTrackingEnabled = value;
            }
        }

        [DbLinqToDo]
        public int CommandTimeout
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool DeferredLoadingEnabled
        {
            get { return this.deferredLoadingEnabled; }
            set
            {
                if (this.currentTransactionEntities != null && value != this.deferredLoadingEnabled)
                    throw new InvalidOperationException("Data context options cannot be modified after results have been returned from a query.");
                this.deferredLoadingEnabled = value;
            }
        }

        [DbLinqToDo]
        public ChangeConflictCollection ChangeConflicts
        {
            get { throw new NotImplementedException(); }
        }

        [DbLinqToDo]
        public DbCommand GetCommand(IQueryable query)
        {
            DbCommand dbCommand = GetIDbCommand(query) as DbCommand;
            if (dbCommand == null)
                throw new InvalidOperationException();

            return dbCommand;
        }

        [DBLinqExtended]
        public IDbCommand GetIDbCommand(IQueryable query)
        {
            if (query == null)
                throw new ArgumentNullException("query");

            var qp = query.Provider as QueryProvider;
            if (qp == null)
                throw new InvalidOperationException();

            if (qp.ExpressionChain.Expressions.Count == 0)
                qp.ExpressionChain.Expressions.Add(CreateDefaultQuery(query));

            return qp.GetQuery(null).GetCommand().Command;
        }

        private Expression CreateDefaultQuery(IQueryable query)
        {
            // Manually create the expression tree for: IQueryable<TableType>.Select(e => e)
            var identityParameter = Expression.Parameter(query.ElementType, "e");
            var identityBody = Expression.Lambda(
                typeof(Func<,>).MakeGenericType(query.ElementType, query.ElementType),
                identityParameter,
                new[] { identityParameter }
            );

            return Expression.Call(
                typeof(Queryable),
                "Select",
                new[] { query.ElementType, query.ElementType },
                query.Expression,
                Expression.Quote(identityBody)
            );
        }

        [DbLinqToDo]
        public void Refresh(RefreshMode mode, IEnumerable entities)
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
