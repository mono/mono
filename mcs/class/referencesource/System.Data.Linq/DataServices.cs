using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;

namespace System.Data.Linq {
    using System.Data.Linq.Mapping;
    using System.Data.Linq.Provider;

    internal class CommonDataServices : IDataServices {
        DataContext context;
        MetaModel metaModel;
        IdentityManager identifier;
        ChangeTracker tracker;
        ChangeDirector director;
        bool hasCachedObjects;
        Dictionary<MetaDataMember, IDeferredSourceFactory> factoryMap;

        internal CommonDataServices(DataContext context, MetaModel model) {
            this.context = context;
            this.metaModel = model;
            bool asReadOnly = !context.ObjectTrackingEnabled;
            this.identifier = IdentityManager.CreateIdentityManager(asReadOnly);
            this.tracker = ChangeTracker.CreateChangeTracker(this, asReadOnly);
            this.director = ChangeDirector.CreateChangeDirector(context);
            this.factoryMap = new Dictionary<MetaDataMember, IDeferredSourceFactory>();
        }

        public DataContext Context {
            get { return this.context; }
        }

        public MetaModel Model {
            get { return this.metaModel; }
        }

        internal void SetModel(MetaModel model) {
            this.metaModel = model;
        }

        internal IdentityManager IdentityManager {
            get { return this.identifier; }
        }

        internal ChangeTracker ChangeTracker {
            get { return this.tracker; }
        }

        internal ChangeDirector ChangeDirector {
            get { return this.director; }
        }

        internal IEnumerable<RelatedItem> GetParents(MetaType type, object item) {
            return this.GetRelations(type, item, true);
        }

        internal IEnumerable<RelatedItem> GetChildren(MetaType type, object item) {
            return this.GetRelations(type, item, false);
        }

        private IEnumerable<RelatedItem> GetRelations(MetaType type, object item, bool isForeignKey) {
            foreach (MetaDataMember mm in type.PersistentDataMembers) {
                if (mm.IsAssociation) {
                    MetaType otherType = mm.Association.OtherType;
                    if (mm.Association.IsForeignKey == isForeignKey) {
                        object value = null;
                        if (mm.IsDeferred) {
                            value = mm.DeferredValueAccessor.GetBoxedValue(item);
                        }
                        else {
                            value = mm.StorageAccessor.GetBoxedValue(item);
                        }
                        if (value != null) {
                            if (mm.Association.IsMany) {
                                IEnumerable list = (IEnumerable)value;
                                foreach (object otherItem in list) {
                                    yield return new RelatedItem(otherType.GetInheritanceType(otherItem.GetType()), otherItem);
                                }
                            }
                            else {
                                yield return new RelatedItem(otherType.GetInheritanceType(value.GetType()), value);
                            }
                        }
                    }
                }
            }
        }

        internal void ResetServices() {
            hasCachedObjects = false;
            bool asReadOnly = !context.ObjectTrackingEnabled;
            this.identifier = IdentityManager.CreateIdentityManager(asReadOnly);
            this.tracker = ChangeTracker.CreateChangeTracker(this, asReadOnly);
            this.factoryMap = new Dictionary<MetaDataMember, IDeferredSourceFactory>();
        }

        internal static object[] GetKeyValues(MetaType type, object instance) {
            List<object> keyValues = new List<object>();
            foreach (MetaDataMember mm in type.IdentityMembers) {
                keyValues.Add(mm.MemberAccessor.GetBoxedValue(instance));
            }
            return keyValues.ToArray();
        }

        internal static object[] GetForeignKeyValues(MetaAssociation association, object instance) {
            List<object> keyValues = new List<object>();
            foreach(MetaDataMember mm in association.ThisKey) {
                keyValues.Add(mm.MemberAccessor.GetBoxedValue(instance));
            }
            return keyValues.ToArray();
        }

        internal object GetCachedObject(MetaType type, object[] keyValues) {
            if( type == null ) {
                throw Error.ArgumentNull("type");
            }
            if (!type.IsEntity) {
                return null;
            }
            return this.identifier.Find(type, keyValues);
        }

        internal object GetCachedObjectLike(MetaType type, object instance) {
            if( type == null ) {
                throw Error.ArgumentNull("type");
            }
            if (!type.IsEntity) {
                return null;
            }
            return this.identifier.FindLike(type, instance);
        }

        public bool IsCachedObject(MetaType type, object instance) {
            if( type == null ) {
                throw Error.ArgumentNull("type");
            }
            if (!type.IsEntity) {
                return false;
            }
            return this.identifier.FindLike(type, instance) == instance;
        }

        public object InsertLookupCachedObject(MetaType type, object instance) {
            if( type == null ) {
                throw Error.ArgumentNull("type");
            }
            hasCachedObjects = true;  // flag that we have cached objects
            if (!type.IsEntity) {
                return instance;
            }
            return this.identifier.InsertLookup(type, instance);
        }

        public bool RemoveCachedObjectLike(MetaType type, object instance) {
            if (type == null) {
                throw Error.ArgumentNull("type");
            }
            if (!type.IsEntity) {
                return false;
            }
            return this.identifier.RemoveLike(type, instance);
        }

        public void OnEntityMaterialized(MetaType type, object instance) {
            if (type == null) {
                throw Error.ArgumentNull("type");
            }
            this.tracker.FastTrack(instance);

            if (type.HasAnyLoadMethod) {
                SendOnLoaded(type, instance);
            }
        }

        private static void SendOnLoaded(MetaType type, object item) {
            if (type != null) {
                SendOnLoaded(type.InheritanceBase, item);

                if (type.OnLoadedMethod != null) {
                    try {
                        type.OnLoadedMethod.Invoke(item, new object[] { });
                    } catch (TargetInvocationException tie) {
                        if (tie.InnerException != null) {
                            throw tie.InnerException;
                        }

                        throw;
                    }
                }
            }
        }


        /// <summary>
        /// Returns a query for the entity indicated by the specified key.
        /// </summary>
        internal Expression GetObjectQuery(MetaType type, object[] keyValues) {
            if (type == null) {
                throw Error.ArgumentNull("type");
            }
            if (keyValues == null) {
                throw Error.ArgumentNull("keyValues");
            }
            return this.GetObjectQuery(type, BuildKeyExpressions(keyValues, type.IdentityMembers));
        }

        internal Expression GetObjectQuery(MetaType type, Expression[] keyValues) {
            ITable table = this.context.GetTable(type.InheritanceRoot.Type);
            ParameterExpression serverItem = Expression.Parameter(table.ElementType, "p");

            // create a where expression including all the identity members            
            Expression whereExpression = null;
            for (int i = 0, n = type.IdentityMembers.Count; i < n; i++) {
                MetaDataMember metaMember = type.IdentityMembers[i];
                Expression memberExpression = (metaMember.Member is FieldInfo)
                    ? Expression.Field(serverItem, (FieldInfo)metaMember.Member)
                    : Expression.Property(serverItem, (PropertyInfo)metaMember.Member);
                Expression memberEqualityExpression = Expression.Equal(memberExpression, keyValues[i]);
                whereExpression = (whereExpression != null)
                    ? Expression.And(whereExpression, memberEqualityExpression)
                    : memberEqualityExpression;
            }
            return Expression.Call(typeof(Queryable), "Where", new Type[] { table.ElementType }, table.Expression, Expression.Lambda(whereExpression, serverItem));
        }

        internal Expression GetDataMemberQuery(MetaDataMember member, Expression[] keyValues) {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (keyValues == null)
                throw Error.ArgumentNull("keyValues");
            if (member.IsAssociation) {
                MetaAssociation association = member.Association;
                Type rootType = association.ThisMember.DeclaringType.InheritanceRoot.Type;
                Expression thisSource = Expression.Constant(context.GetTable(rootType));
                if (rootType != association.ThisMember.DeclaringType.Type) {
                    thisSource = Expression.Call(typeof(Enumerable), "Cast", new Type[] { association.ThisMember.DeclaringType.Type }, thisSource);
                }
                Expression thisInstance = Expression.Call(typeof(Enumerable), "FirstOrDefault", new Type[] { association.ThisMember.DeclaringType.Type },
                    System.Data.Linq.SqlClient.Translator.WhereClauseFromSourceAndKeys(thisSource, association.ThisKey.ToArray(), keyValues)
                    );
                Expression otherSource = Expression.Constant(context.GetTable(association.OtherType.InheritanceRoot.Type));
                if (association.OtherType.Type!=association.OtherType.InheritanceRoot.Type) {
                    otherSource = Expression.Call(typeof(Enumerable), "Cast", new Type[] { association.OtherType.Type }, otherSource);
                }
                Expression expr = System.Data.Linq.SqlClient.Translator.TranslateAssociation(
                    this.context, association, otherSource, keyValues, thisInstance
                    );
                return expr;
            }
            else {
                Expression query = this.GetObjectQuery(member.DeclaringType, keyValues);
                Type elementType = System.Data.Linq.SqlClient.TypeSystem.GetElementType(query.Type);
                ParameterExpression p = Expression.Parameter(elementType, "p");
                Expression e = p;
                if (elementType != member.DeclaringType.Type)
                    e = Expression.Convert(e, member.DeclaringType.Type);
                Expression mem = (member.Member is PropertyInfo) 
                    ? Expression.Property(e, (PropertyInfo)member.Member)
                    : Expression.Field(e, (FieldInfo)member.Member);
                LambdaExpression selector = Expression.Lambda(mem, p);
                return Expression.Call(typeof(Queryable), "Select", new Type[] { elementType, selector.Body.Type }, query, selector);
            }
        }

        private static Expression[] BuildKeyExpressions(object[] keyValues, ReadOnlyCollection<MetaDataMember> keyMembers) {
            Expression[] keyValueExpressions = new Expression[keyValues.Length];
            for (int i = 0, n = keyMembers.Count; i < n; i++) {
                MetaDataMember metaMember = keyMembers[i];
                Expression keyValueExpression = Expression.Constant(keyValues[i], metaMember.Type);
                keyValueExpressions[i] = keyValueExpression;
            }
            return keyValueExpressions;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public IDeferredSourceFactory GetDeferredSourceFactory(MetaDataMember member) {
            if (member == null) {
                throw Error.ArgumentNull("member");
            }
            IDeferredSourceFactory factory;
            if (this.factoryMap.TryGetValue(member, out factory)) {
                return factory;
            }
            Type elemType = member.IsAssociation && member.Association.IsMany
                ? System.Data.Linq.SqlClient.TypeSystem.GetElementType(member.Type)
                : member.Type;
            factory = (IDeferredSourceFactory) Activator.CreateInstance(
                typeof(DeferredSourceFactory<>).MakeGenericType(elemType),
                BindingFlags.Instance | BindingFlags.NonPublic, null,
                new object[] { member, this }, null
                );
            this.factoryMap.Add(member, factory);
            return factory;
        }

        class DeferredSourceFactory<T> : IDeferredSourceFactory {
            MetaDataMember member;
            CommonDataServices services;
            ICompiledQuery query;
            bool refersToPrimaryKey;
            T[] empty;

            internal DeferredSourceFactory(MetaDataMember member, CommonDataServices services) {
                this.member = member;
                this.services = services;
                this.refersToPrimaryKey = this.member.IsAssociation && this.member.Association.OtherKeyIsPrimaryKey;
                this.empty = new T[] { };
            }

            public IEnumerable CreateDeferredSource(object instance) {
                if (instance == null)
                    throw Error.ArgumentNull("instance");
                return new DeferredSource(this, instance);
            }

            public IEnumerable CreateDeferredSource(object[] keyValues) {
                if (keyValues == null)
                    throw Error.ArgumentNull("keyValues");
                return new DeferredSource(this, keyValues);
            }

            private IEnumerator<T> Execute(object instance) {
                ReadOnlyCollection<MetaDataMember> keys = null;
                if (this.member.IsAssociation) {
                    keys = this.member.Association.ThisKey;
                }
                else {
                    keys = this.member.DeclaringType.IdentityMembers;
                }
                object[] keyValues = new object[keys.Count];
                for (int i = 0, n = keys.Count; i < n; i++) {
                    object value = keys[i].StorageAccessor.GetBoxedValue(instance);
                    keyValues[i] = value;
                }

                if (this.HasNullForeignKey(keyValues)) {
                    return ((IEnumerable<T>)this.empty).GetEnumerator();
                }

                T cached;
                if (this.TryGetCachedObject(keyValues, out cached)) {
                    return ((IEnumerable<T>)(new T[] { cached })).GetEnumerator();
                }

                if (this.member.LoadMethod != null) {
                    try {
                        object result = this.member.LoadMethod.Invoke(this.services.Context, new object[] { instance });
                        if (typeof(T).IsAssignableFrom(this.member.LoadMethod.ReturnType)) {
                            return ((IEnumerable<T>)new T[] { (T)result }).GetEnumerator();
                        }
                        else {
                            return ((IEnumerable<T>)result).GetEnumerator();
                        }
                    }
                    catch (TargetInvocationException tie) {
                        if (tie.InnerException != null) {
                            throw tie.InnerException;
                        }
                        throw;
                    }
                }
                else {
                    return this.ExecuteKeyQuery(keyValues);
                }
            }

            private IEnumerator<T> ExecuteKeys(object[] keyValues) {
                if (this.HasNullForeignKey(keyValues)) {
                    return ((IEnumerable<T>)this.empty).GetEnumerator();
                }

                T cached;
                if (this.TryGetCachedObject(keyValues, out cached)) {
                    return ((IEnumerable<T>)(new T[] { cached })).GetEnumerator();
                }

                return this.ExecuteKeyQuery(keyValues);
            }

            private bool HasNullForeignKey(object[] keyValues) {
                if (this.refersToPrimaryKey) {
                    bool keyHasNull = false;
                    for (int i = 0, n = keyValues.Length; i < n; i++) {
                        keyHasNull |= keyValues[i] == null;
                    }
                    if (keyHasNull) {
                        return true;
                    }
                }
                return false;
            }

            private bool TryGetCachedObject(object[] keyValues, out T cached) {
                cached = default(T);
                if (this.refersToPrimaryKey) {
                    // look to see if we already have this object in the identity cache
                    MetaType mt = this.member.IsAssociation ? this.member.Association.OtherType : this.member.DeclaringType;
                    object obj = this.services.GetCachedObject(mt, keyValues);
                    if (obj != null) {
                        cached = (T)obj;
                        return true;
                    }
                }
                return false;
            }

            private IEnumerator<T> ExecuteKeyQuery(object[] keyValues) {
                if (this.query == null) {
                    ParameterExpression p = Expression.Parameter(typeof(object[]), "keys");
                    Expression[] keyExprs = new Expression[keyValues.Length];
                    ReadOnlyCollection<MetaDataMember> members = this.member.IsAssociation ? this.member.Association.OtherKey : this.member.DeclaringType.IdentityMembers;
                    for (int i = 0, n = keyValues.Length; i < n; i++) {
                        MetaDataMember mm = members[i];
                        keyExprs[i] = Expression.Convert(
#pragma warning disable 618 // Disable the 'obsolete' warning
                                          Expression.ArrayIndex(p, Expression.Constant(i)),
#pragma warning restore 618
                                          mm.Type
                                      );
                    }
                    Expression q = this.services.GetDataMemberQuery(this.member, keyExprs);
                    LambdaExpression lambda = Expression.Lambda(q, p);
                    this.query = this.services.Context.Provider.Compile(lambda);
                }
                return ((IEnumerable<T>)this.query.Execute(this.services.Context.Provider, new object[] { keyValues }).ReturnValue).GetEnumerator();
            }

            class DeferredSource : IEnumerable<T>, IEnumerable {
                DeferredSourceFactory<T> factory;
                object instance;

                internal DeferredSource(DeferredSourceFactory<T> factory, object instance) {
                    this.factory = factory;
                    this.instance = instance;
                }

                public IEnumerator<T> GetEnumerator() {
                    object[] keyValues = this.instance as object[];
                    if (keyValues != null) {
                        return this.factory.ExecuteKeys(keyValues);
                    }
                    return this.factory.Execute(this.instance);
                }

                IEnumerator IEnumerable.GetEnumerator() {
                    return this.GetEnumerator();
                }
            }
        }

        /// <summary>
        /// Returns true if any objects have been added to the identity cache.  If
        /// object tracking is disabled, this still returns true if any attempts
        /// where made to cache an object.  Thus regardless of object tracking mode,
        /// this can be used as an indicator as to whether any result returning queries
        /// have been executed.
        /// </summary>
        internal bool HasCachedObjects {
            get { 
                return this.hasCachedObjects; 
            }
        }

        public object GetCachedObject(Expression query) {
            if (query == null)
                return null;
            MethodCallExpression mc = query as MethodCallExpression;
            if (mc == null || mc.Arguments.Count < 1 || mc.Arguments.Count > 2)
                return null;
            if (mc.Method.DeclaringType != typeof(Queryable)) {
                return null;
            }
            switch (mc.Method.Name) {
                case "Where":
                case "First":
                case "FirstOrDefault":
                case "Single":
                case "SingleOrDefault":
                    break;
                default:
                    return null;
            }
            if (mc.Arguments.Count == 1) {
                // If it is something like 
                //      context.Customers.Where(c => c.ID = 123).First()
                // then it is equivalent of 
                //      context.Customers.First(c => c.ID = 123)
                // hence reduce to context.Customers.Where(c => c.ID = 123) and process the remaining query
                return GetCachedObject(mc.Arguments[0]);
            }
            UnaryExpression quote = mc.Arguments[1] as UnaryExpression;
            if (quote == null || quote.NodeType != ExpressionType.Quote)
                return null;
            LambdaExpression pred = quote.Operand as LambdaExpression;
            if (pred == null)
                return null;
            ConstantExpression cex = mc.Arguments[0] as ConstantExpression;
            if (cex == null)
                return null;
            ITable t = cex.Value as ITable;
            if (t == null)
                return null;
            Type elementType = System.Data.Linq.SqlClient.TypeSystem.GetElementType(query.Type);
            if (elementType != t.ElementType)
                return null;
            MetaTable metaTable = this.metaModel.GetTable(t.ElementType);
            object[] keyValues = this.GetKeyValues(metaTable.RowType, pred);
            if (keyValues != null) {
                return this.GetCachedObject(metaTable.RowType, keyValues);
            }
            return null;
        }

        internal object[] GetKeyValues(MetaType type, LambdaExpression predicate) {
            if (predicate == null)
                throw Error.ArgumentNull("predicate");
            if (predicate.Parameters.Count != 1)
                return null;
            Dictionary<MetaDataMember, object> keys = new Dictionary<MetaDataMember, object>();
            if (this.GetKeysFromPredicate(type, keys, predicate.Body)
                && keys.Count == type.IdentityMembers.Count) {
                object[] values = keys.OrderBy(kv => kv.Key.Ordinal).Select(kv => kv.Value).ToArray();
                return values;
            }
            return null;
        }

        private bool GetKeysFromPredicate(MetaType type, Dictionary<MetaDataMember, object> keys, Expression expr) {
            BinaryExpression bex = expr as BinaryExpression;
            if (bex == null) {
                MethodCallExpression mex = expr as MethodCallExpression;
                if (mex != null && mex.Method.Name == "op_Equality" && mex.Arguments.Count == 2) {
                    bex = Expression.Equal(mex.Arguments[0], mex.Arguments[1]);
                }
                else {
                    return false;
                }
            }
            switch (bex.NodeType) {
                case ExpressionType.And:
                    return this.GetKeysFromPredicate(type, keys, bex.Left) &&
                           this.GetKeysFromPredicate(type, keys, bex.Right);
                case ExpressionType.Equal:
                    return GetKeyFromPredicate(type, keys, bex.Left, bex.Right) ||
                           GetKeyFromPredicate(type, keys, bex.Right, bex.Left);
                default:
                    return false;
            }
        }

        private static bool GetKeyFromPredicate(MetaType type, Dictionary<MetaDataMember, object> keys, Expression mex, Expression vex) {
            MemberExpression memex = mex as MemberExpression;
            if (memex == null || memex.Expression == null ||
                memex.Expression.NodeType != ExpressionType.Parameter || memex.Expression.Type != type.Type) {
                return false;
            }
            if (!type.Type.IsAssignableFrom(memex.Member.ReflectedType) && !memex.Member.ReflectedType.IsAssignableFrom(type.Type)) {
                return false;
            }
            MetaDataMember mm = type.GetDataMember(memex.Member);
            if (!mm.IsPrimaryKey) {
                return false;
            }
            if (keys.ContainsKey(mm)) {
                return false;
            }
            ConstantExpression cex = vex as ConstantExpression;
            if (cex != null) {
                keys.Add(mm, cex.Value);
                return true;
            }
            InvocationExpression ie = vex as InvocationExpression;
            if (ie != null && ie.Arguments != null && ie.Arguments.Count == 0) {
                ConstantExpression ce = ie.Expression as ConstantExpression;
                if (ce != null) {
                    keys.Add(mm, ((Delegate)ce.Value).DynamicInvoke(new object[] {}));
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Either returns the object from cache if it is in cache, or
        /// queries for it.
        /// </summary> 
        internal object GetObjectByKey(MetaType type, object[] keyValues) {
            // first check the cache                            
            object target = GetCachedObject(type, keyValues);
            if (target == null) {
                // no cached value, so query for it               
                target = ((IEnumerable)this.context.Provider.Execute(this.GetObjectQuery(type, keyValues)).ReturnValue).OfType<object>().SingleOrDefault();
            }
            return target;
        }
    }

    internal struct RelatedItem {
        internal MetaType Type;
        internal object Item;
        internal RelatedItem(MetaType type, object item) {
            this.Type = type;
            this.Item = item;
        }
    }
}
