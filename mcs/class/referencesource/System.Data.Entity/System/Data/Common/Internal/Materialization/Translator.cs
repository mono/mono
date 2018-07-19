//------------------------------------------------------------------------------
// <copyright file="Translator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common.Internal.Materialization
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common.QueryCache;
    using System.Data.Common.Utils;
    using System.Data.Entity;
    using System.Data.Mapping;
    using System.Data.Metadata.Edm;
    using System.Data.Objects;
    using System.Data.Objects.DataClasses;
    using System.Data.Objects.ELinq;
    using System.Data.Objects.Internal;
    using System.Data.Query.InternalTrees;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;

    /// <summary>
    /// Struct containing the requested type and parent column map used
    /// as the arg in the Translator visitor.
    /// </summary>
    internal struct TranslatorArg
    {
        internal readonly Type RequestedType;

        internal TranslatorArg(Type requestedType)
        {
            this.RequestedType = requestedType;
        }
    }

    /// <summary>
    /// Type returned by the Translator visitor; allows us to put the logic
    /// to ensure a specific return type in a single place, instead of in 
    /// each Visit method.
    /// </summary>
    internal class TranslatorResult
    {
        private readonly Expression ReturnedExpression;
        private readonly Type RequestedType;

        internal TranslatorResult(Expression returnedExpression, Type requestedType)
        {
            this.RequestedType = requestedType;
            this.ReturnedExpression = returnedExpression;
        }

        /// <summary>
        /// Return the expression; wrapped with the appropriate cast/convert
        /// logic to guarantee it's type.
        /// </summary>
        internal Expression Expression
        {
            get
            {
                Expression result = Translator.Emit_EnsureType(ReturnedExpression, RequestedType);
                return result;
            }
        }

        /// <summary>
        /// Return the expression without attempting to cast/convert to the requested type.
        /// </summary>
        internal Expression UnconvertedExpression
        {
            get
            {
                return ReturnedExpression;
            }
        }

        /// <summary>
        /// Checks if the expression represents an wrapped entity and if so creates an expression
        /// that extracts the raw entity from the wrapper.
        /// </summary>
        internal Expression UnwrappedExpression
        {
            get
            {
                if (!typeof(IEntityWrapper).IsAssignableFrom(ReturnedExpression.Type))
                {
                    return ReturnedExpression;
                }
                return Translator.Emit_UnwrapAndEnsureType(ReturnedExpression, RequestedType);
            }
        }
    }

    /// <summary>
    /// For collection results, we really want to know the expression to
    /// get the coordinator from its stateslot as well, so we have an 
    /// additional one...
    /// </summary>
    internal class CollectionTranslatorResult : TranslatorResult
    {
        internal readonly Expression ExpressionToGetCoordinator;

        internal CollectionTranslatorResult(Expression returnedExpression, ColumnMap columnMap, Type requestedType, Expression expressionToGetCoordinator)
            : base(returnedExpression, requestedType)
        {
            this.ExpressionToGetCoordinator = expressionToGetCoordinator;
        }
    }

    /// <summary>
    /// Translates query ColumnMap into ShaperFactory. Basically, we interpret the 
    /// ColumnMap and compile delegates used to materialize results.
    /// </summary>
    internal class Translator : ColumnMapVisitorWithResults<TranslatorResult, TranslatorArg>
    {
        #region private state

        /// <summary>
        /// Gets the O-Space Metadata workspace.
        /// </summary>
        private readonly MetadataWorkspace _workspace;

        /// <summary>
        /// Gets structure telling us how to interpret 'span' rows (includes implicit
        /// relationship span and explicit full span via ObjectQuery.Include().
        /// </summary>
        private readonly SpanIndex _spanIndex;

        /// <summary>
        /// Gets the MergeOption for the current query (influences our handling of 
        /// entities when they are materialized).
        /// </summary>
        private readonly MergeOption _mergeOption;

        /// <summary>
        /// When true, indicates we're processing for the value layer (BridgeDataReader)
        /// and not the ObjectMaterializer
        /// </summary>
        private readonly bool IsValueLayer;

        /// <summary>
        /// Gets scratchpad for topmost nested reader coordinator.
        /// </summary>
        private CoordinatorScratchpad _rootCoordinatorScratchpad;

        /// <summary>
        /// Gets scratchpad for the coordinator builder for the nested reader currently
        /// being translated or emitted.
        /// </summary>
        private CoordinatorScratchpad _currentCoordinatorScratchpad;

        /// <summary>
        /// Gets number of 'Shaper.State' slots allocated (used to hold onto intermediate
        /// values during materialization)
        /// </summary>
        private int _stateSlotCount;

        /// <summary>
        /// Set to true if any Entity/Complex type/property for which we're emitting a
        /// handler is non-public. Used to determine which security checks are necessary 
        /// when invoking the delegate.
        /// </summary>
        private bool _hasNonPublicMembers;

        /// <summary>
        /// Local cache of ObjectTypeMappings for EdmTypes (to prevent expensive lookups).
        /// </summary>
        private readonly Dictionary<EdmType, ObjectTypeMapping> _objectTypeMappings = new Dictionary<EdmType, ObjectTypeMapping>();

        #endregion

        #region constructor

        private Translator(MetadataWorkspace workspace, SpanIndex spanIndex, MergeOption mergeOption, bool valueLayer)
        {
            _workspace = workspace;
            _spanIndex = spanIndex;
            _mergeOption = mergeOption;
            IsValueLayer = valueLayer;
        }

        #endregion

        #region "public" surface area

        /// <summary>
        /// The main entry point for the translation process. Given a ColumnMap, returns 
        /// a ShaperFactory which can be used to materialize results for a query.
        /// </summary>
        internal static ShaperFactory<TRequestedType> TranslateColumnMap<TRequestedType>(QueryCacheManager queryCacheManager, ColumnMap columnMap, MetadataWorkspace workspace, SpanIndex spanIndex, MergeOption mergeOption, bool valueLayer)
        {
            Debug.Assert(columnMap is CollectionColumnMap, "root column map must be a collection for a query");

            // If the query cache already contains a plan, then we're done
            ShaperFactory<TRequestedType> result;
            string columnMapKey = ColumnMapKeyBuilder.GetColumnMapKey(columnMap, spanIndex);
            ShaperFactoryQueryCacheKey<TRequestedType> cacheKey = new ShaperFactoryQueryCacheKey<TRequestedType>(columnMapKey, mergeOption, valueLayer);

            if (queryCacheManager.TryCacheLookup<ShaperFactoryQueryCacheKey<TRequestedType>, ShaperFactory<TRequestedType>>(cacheKey, out result))
            {
                return result;
            }

            // Didn't find it in the cache, so we have to do the translation;  First create
            // the translator visitor that recursively tranforms ColumnMaps into Expressions
            // stored on the CoordinatorScratchpads it also constructs.  We'll compile those
            // expressions into delegates later.
            Translator translator = new Translator(workspace, spanIndex, mergeOption, valueLayer);
            columnMap.Accept(translator, new TranslatorArg(typeof(IEnumerable<>).MakeGenericType(typeof(TRequestedType))));

            Debug.Assert(null != translator._rootCoordinatorScratchpad, "translating the root of the query must populate _rootCoordinatorBuilder"); // how can this happen?

            // We're good. Go ahead and recursively compile the CoordinatorScratchpads we
            // created in the vistor into CoordinatorFactories which contain compiled
            // delegates for the expressions we generated.
            CoordinatorFactory<TRequestedType> coordinatorFactory = (CoordinatorFactory<TRequestedType>)translator._rootCoordinatorScratchpad.Compile();

            // Along the way we constructed a nice delegate to perform runtime permission 
            // checks (e.g. for LinkDemand and non-public members).  We need that now.
            Action checkPermissionsDelegate = translator.GetCheckPermissionsDelegate();

            // Finally, take everything we've produced, and create the ShaperFactory to
            // contain it all, then add it to the query cache so we don't need to do this
            // for this query again.
            result = new ShaperFactory<TRequestedType>(translator._stateSlotCount, coordinatorFactory, checkPermissionsDelegate, mergeOption);
            QueryCacheEntry cacheEntry = new QueryCacheEntry(cacheKey, result);
            if (queryCacheManager.TryLookupAndAdd(cacheEntry, out cacheEntry))
            {
                // Someone beat us to it. Use their result instead.
                result = (ShaperFactory<TRequestedType>)cacheEntry.GetTarget();
            }
            return result;
        }

        /// <summary>
        /// Compiles a delegate taking a Shaper instance and returning values. Used to compile 
        /// Expressions produced by the emitter.
        /// 
        /// Asserts MemberAccess to skip visbility check.  
        /// This means that that security checks are skipped. Before calling this
        /// method you must ensure that you've done a TestComple on expressions provided
        /// by the user to ensure the compilation doesn't violate them.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2128")]
        [System.Security.SecuritySafeCritical]
        [ReflectionPermission(SecurityAction.Assert, MemberAccess = true)]
        internal static Func<Shaper, TResult> Compile<TResult>(Expression body)
        {
            var lambda = Expression.Lambda<Func<Shaper, TResult>>(body, Shaper_Parameter);
            return lambda.Compile();
        }

        /// <summary>
        /// Non-generic version of Compile (where the result type is passed in as an argument rather
        /// than a type parameter)
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static object Compile(Type resultType, Expression body)
        {
            MethodInfo compile = Translator_Compile.MakeGenericMethod(resultType);
            return compile.Invoke(null, new object[] { body });
        }

        #endregion

        #region helpers

        /// <summary>
        /// Allocates a slot in 'Shaper.State' which can be used as storage for 
        /// materialization tasks (e.g. remembering key values for a nested collection)
        /// </summary>
        private int AllocateStateSlot()
        {
            return _stateSlotCount++;
        }

        /// <summary>
        /// Returns a delegate performing necessary permission checks identified
        /// by this translator.  This delegate must be called every time a row is 
        /// read from the ObjectResult enumerator, since the enumerator can be 
        /// passed across security contexts.
        /// </summary>
        private Action GetCheckPermissionsDelegate()
        {
            // Emit an action to check runtime permissions.
            return _hasNonPublicMembers ? (Action)DemandMemberAccess : null;
        }

        private static void DemandMemberAccess()
        {
            LightweightCodeGenerator.MemberAccessReflectionPermission.Demand();
        }

        /// <summary>
        /// Try compiling the user expressions to ensure it would succeed without an 
        /// assert (user expressions are inlined with references to EF internals which 
        /// require the assert so we need to check the user expressions separately).
        /// 
        /// This method is called every time a new query result is returned to make sure
        /// the user expressions can be compiled in the current security context.
        /// </summary>
        private static void VerifyUserExpressions(IEnumerable<Expression<Func<object>>> userExpressions)
        {
            // As an optimization, check if we have member access permission. If so,
            // we know the compile would succeed and don't need to make the effort.
            if (!LightweightCodeGenerator.HasMemberAccessReflectionPermission())
            {
                // If we don't have MemberAccess, compile the expressions to see if they
                // might be satisfied by RestrictedMemberAccess.
                foreach (Expression<Func<object>> userExpression in userExpressions)
                {
                    userExpression.Compile();
                }
            }
        }

        /// <summary>
        /// Return the CLR type we're supposed to materialize for the TypeUsage
        /// </summary>
        private Type DetermineClrType(TypeUsage typeUsage)
        {
            return DetermineClrType(typeUsage.EdmType);
        }

        /// <summary>
        /// Return the CLR type we're supposed to materialize for the EdmType
        /// </summary>
        private Type DetermineClrType(EdmType edmType)
        {
            Type result = null;
            // Normalize for spandex
            edmType = ResolveSpanType(edmType);

            switch (edmType.BuiltInTypeKind)
            {
                case BuiltInTypeKind.EntityType:
                case BuiltInTypeKind.ComplexType:
                    if (IsValueLayer)
                    {
                        result = typeof(RecordState);
                    }
                    else
                    {
                        result = LookupObjectMapping(edmType).ClrType.ClrType;
                    }
                    break;

                case BuiltInTypeKind.RefType:
                    result = typeof(EntityKey);
                    break;

                case BuiltInTypeKind.CollectionType:
                    if (IsValueLayer)
                    {
                        result = typeof(Coordinator<RecordState>);
                    }
                    else
                    {
                        EdmType edmElementType = ((CollectionType)edmType).TypeUsage.EdmType;
                        result = DetermineClrType(edmElementType);
                        result = typeof(IEnumerable<>).MakeGenericType(result);
                    }
                    break;

                case BuiltInTypeKind.EnumType:
                    if (IsValueLayer)
                    {
                        result = DetermineClrType(((EnumType)edmType).UnderlyingType);
                    }
                    else
                    {
                        result = LookupObjectMapping(edmType).ClrType.ClrType;
                        result = typeof(Nullable<>).MakeGenericType(result);
                    }
                    break;

                case BuiltInTypeKind.PrimitiveType:
                    result = ((PrimitiveType)edmType).ClrEquivalentType;
                    if (result.IsValueType)
                    {
                        result = typeof(Nullable<>).MakeGenericType(result);
                    }
                    break;

                case BuiltInTypeKind.RowType:
                    if (IsValueLayer)
                    {
                        result = typeof(RecordState);
                    }
                    else
                    {
                        // LINQ has anonymous types that aren't going to show up in our
                        // metadata workspace, and we don't want to hydrate a record when
                        // we need an anonymous type.  ELINQ solves this by annotating the
                        // edmType with some additional information, which we'll pick up 
                        // here.
                        InitializerMetadata initializerMetadata = ((RowType)edmType).InitializerMetadata;
                        if (null != initializerMetadata)
                        {
                            result = initializerMetadata.ClrType;
                        }
                        else
                        {
                            // Otherwise, by default, we'll give DbDataRecord results (the 
                            // user can also cast to IExtendedDataRecord)
                            result = typeof(DbDataRecord);
                        }
                    }
                    break;

                default:
                    Debug.Fail(string.Format(CultureInfo.CurrentCulture, "The type {0} was not the expected scalar, enumeration, collection, structural, nominal, or reference type.", edmType.GetType()));
                    break;
            }
            Debug.Assert(null != result, "no result?"); // just making sure we cover this in the switch statement.

            return result;
        }

        /// <summary>
        /// Get the ConstructorInfo for the type specified, and ensure we keep track
        /// of any security requirements that the type has.
        /// </summary>
        private ConstructorInfo GetConstructor(Type type)
        {
            ConstructorInfo result = null;
            if (!type.IsAbstract)
            {
                result = LightweightCodeGenerator.GetConstructorForType(type);

                // remember security requirements for this constructor
                if (!LightweightCodeGenerator.IsPublic(result))
                {
                    _hasNonPublicMembers = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves object mapping metadata for the given type. The first time a type 
        /// is encountered, we cache the metadata to avoid repeating the work for every 
        /// row in result. 
        /// 
        /// Caching at the materializer rather than workspace/metadata cache level optimizes
        /// for transient types (including row types produced for span, LINQ initializations, 
        /// collections and projections).
        /// </summary>
        private ObjectTypeMapping LookupObjectMapping(EdmType edmType)
        {
            Debug.Assert(null != edmType, "no edmType?"); // edmType must not be null.

            ObjectTypeMapping result;

            EdmType resolvedType = ResolveSpanType(edmType);
            if (null == resolvedType)
            {
                resolvedType = edmType;
            }

            if (!_objectTypeMappings.TryGetValue(resolvedType, out result))
            {
                result = Util.GetObjectMapping(resolvedType, _workspace);
                _objectTypeMappings.Add(resolvedType, result);
            }
            return result;
        }

        /// <summary>
        /// Remove spanned info from the edmType
        /// </summary>
        /// <param name="edmType"></param>
        /// <returns></returns>
        private EdmType ResolveSpanType(EdmType edmType)
        {
            EdmType result = edmType;

            switch (result.BuiltInTypeKind)
            {
                case BuiltInTypeKind.CollectionType:
                    // For collections, we have to edmType from the (potentially) spanned
                    // element of the collection, then build a new Collection around it.
                    result = ResolveSpanType(((CollectionType)result).TypeUsage.EdmType);
                    if (null != result)
                    {
                        result = new CollectionType(result);
                    }
                    break;

                case BuiltInTypeKind.RowType:
                    // If there is a SpanMap, pick up the EdmType from the first column
                    // in the record, otherwise it's just the type we already have.
                    RowType rowType = (RowType)result;
                    if (null != _spanIndex && _spanIndex.HasSpanMap(rowType))
                    {
                        result = rowType.Members[0].TypeUsage.EdmType;
                    }
                    break;
            }
            return result;
        }

        /// <summary>
        /// Creates an expression representing an inline delegate of type Func&lt;Shaper, body.Type&gt;
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private LambdaExpression CreateInlineDelegate(Expression body)
        {
            // Note that we call through to a typed method so that we can call Expression.Lambda<Func> instead
            // of the straightforward Expression.Lambda. The latter requires FullTrust.
            Type delegateReturnType = body.Type;
            MethodInfo createMethod = Translator_TypedCreateInlineDelegate.MakeGenericMethod(delegateReturnType);
            LambdaExpression result = (LambdaExpression)createMethod.Invoke(this, new object[] { body });
            return result;
        }

        private Expression<Func<Shaper, T>> TypedCreateInlineDelegate<T>(Expression body)
        {
            Expression<Func<Shaper, T>> result = Expression.Lambda<Func<Shaper, T>>(body, Shaper_Parameter);
            _currentCoordinatorScratchpad.AddInlineDelegate(result);
            return result;
        }

        #endregion

        #region Lightweight CodeGen emitters

        #region static Reflection info used in emitters

        private static readonly MethodInfo DbDataReader_GetValue = typeof(DbDataReader).GetMethod("GetValue");
        private static readonly MethodInfo DbDataReader_GetString = typeof(DbDataReader).GetMethod("GetString");
        private static readonly MethodInfo DbDataReader_GetInt16 = typeof(DbDataReader).GetMethod("GetInt16");
        private static readonly MethodInfo DbDataReader_GetInt32 = typeof(DbDataReader).GetMethod("GetInt32");
        private static readonly MethodInfo DbDataReader_GetInt64 = typeof(DbDataReader).GetMethod("GetInt64");
        private static readonly MethodInfo DbDataReader_GetBoolean = typeof(DbDataReader).GetMethod("GetBoolean");
        private static readonly MethodInfo DbDataReader_GetDecimal = typeof(DbDataReader).GetMethod("GetDecimal");
        private static readonly MethodInfo DbDataReader_GetFloat = typeof(DbDataReader).GetMethod("GetFloat");
        private static readonly MethodInfo DbDataReader_GetDouble = typeof(DbDataReader).GetMethod("GetDouble");
        private static readonly MethodInfo DbDataReader_GetDateTime = typeof(DbDataReader).GetMethod("GetDateTime");
        private static readonly MethodInfo DbDataReader_GetGuid = typeof(DbDataReader).GetMethod("GetGuid");
        private static readonly MethodInfo DbDataReader_GetByte = typeof(DbDataReader).GetMethod("GetByte");
        private static readonly MethodInfo DbDataReader_IsDBNull = typeof(DbDataReader).GetMethod("IsDBNull");

        private static readonly ConstructorInfo EntityKey_ctor_SingleKey = typeof(EntityKey).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(EntitySet), typeof(object) }, null);
        private static readonly ConstructorInfo EntityKey_ctor_CompositeKey = typeof(EntityKey).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(EntitySet), typeof(object[]) }, null);

        private static readonly MethodInfo IEntityKeyWithKey_EntityKey = typeof(System.Data.Objects.DataClasses.IEntityWithKey).GetProperty("EntityKey").GetSetMethod();

        private static readonly MethodInfo IEqualityComparerOfString_Equals = typeof(IEqualityComparer<String>).GetMethod("Equals", new Type[] { typeof(string), typeof(string) });

        private static readonly ConstructorInfo MaterializedDataRecord_ctor = typeof(MaterializedDataRecord).GetConstructor(
                                                                                            BindingFlags.NonPublic | BindingFlags.Instance,
                                                                                            null, new Type[] { typeof(MetadataWorkspace), typeof(TypeUsage), typeof(object[]) },
                                                                                            null);

        private static readonly MethodInfo RecordState_GatherData = typeof(RecordState).GetMethod("GatherData", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo RecordState_SetNullRecord = typeof(RecordState).GetMethod("SetNullRecord", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly MethodInfo Shaper_Discriminate = typeof(Shaper).GetMethod("Discriminate");
        private static readonly MethodInfo Shaper_GetPropertyValueWithErrorHandling = typeof(Shaper).GetMethod("GetPropertyValueWithErrorHandling");
        private static readonly MethodInfo Shaper_GetColumnValueWithErrorHandling = typeof(Shaper).GetMethod("GetColumnValueWithErrorHandling");
        private static readonly MethodInfo Shaper_GetGeographyColumnValue = typeof(Shaper).GetMethod("GetGeographyColumnValue");
        private static readonly MethodInfo Shaper_GetGeometryColumnValue = typeof(Shaper).GetMethod("GetGeometryColumnValue");
        private static readonly MethodInfo Shaper_GetSpatialColumnValueWithErrorHandling = typeof(Shaper).GetMethod("GetSpatialColumnValueWithErrorHandling");
        private static readonly MethodInfo Shaper_GetSpatialPropertyValueWithErrorHandling = typeof(Shaper).GetMethod("GetSpatialPropertyValueWithErrorHandling");
        private static readonly MethodInfo Shaper_HandleEntity = typeof(Shaper).GetMethod("HandleEntity");
        private static readonly MethodInfo Shaper_HandleEntityAppendOnly = typeof(Shaper).GetMethod("HandleEntityAppendOnly");
        private static readonly MethodInfo Shaper_HandleEntityNoTracking = typeof(Shaper).GetMethod("HandleEntityNoTracking");
        private static readonly MethodInfo Shaper_HandleFullSpanCollection = typeof(Shaper).GetMethod("HandleFullSpanCollection");
        private static readonly MethodInfo Shaper_HandleFullSpanElement = typeof(Shaper).GetMethod("HandleFullSpanElement");
        private static readonly MethodInfo Shaper_HandleIEntityWithKey = typeof(Shaper).GetMethod("HandleIEntityWithKey");
        private static readonly MethodInfo Shaper_HandleRelationshipSpan = typeof(Shaper).GetMethod("HandleRelationshipSpan");
        private static readonly MethodInfo Shaper_SetColumnValue = typeof(Shaper).GetMethod("SetColumnValue");
        private static readonly MethodInfo Shaper_SetEntityRecordInfo = typeof(Shaper).GetMethod("SetEntityRecordInfo");
        private static readonly MethodInfo Shaper_SetState = typeof(Shaper).GetMethod("SetState");
        private static readonly MethodInfo Shaper_SetStatePassthrough = typeof(Shaper).GetMethod("SetStatePassthrough");

        private static readonly MethodInfo Translator_BinaryEquals = typeof(Translator).GetMethod("BinaryEquals", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo Translator_CheckedConvert = typeof(Translator).GetMethod("CheckedConvert", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo Translator_Compile = typeof(Translator).GetMethod("Compile", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(Expression) }, null);
        private static readonly MethodInfo Translator_MultipleDiscriminatorPolymorphicColumnMapHelper = typeof(Translator).GetMethod("MultipleDiscriminatorPolymorphicColumnMapHelper", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo Translator_TypedCreateInlineDelegate = typeof(Translator).GetMethod("TypedCreateInlineDelegate", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly PropertyInfo EntityWrapperFactory_NullWrapper = typeof(EntityWrapperFactory).GetProperty("NullWrapper", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly PropertyInfo IEntityWrapper_Entity = typeof(IEntityWrapper).GetProperty("Entity");
        private static readonly MethodInfo EntityProxyTypeInfo_SetEntityWrapper = typeof(EntityProxyTypeInfo).GetMethod("SetEntityWrapper", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly ConstructorInfo PocoPropertyAccessorStrategy_ctor = typeof(PocoPropertyAccessorStrategy).GetConstructor(new Type[] { typeof(object) });
        private static readonly ConstructorInfo EntityWithChangeTrackerStrategy_ctor = typeof(EntityWithChangeTrackerStrategy).GetConstructor(new Type[] { typeof(IEntityWithChangeTracker) });
        private static readonly ConstructorInfo EntityWithKeyStrategy_ctor = typeof(EntityWithKeyStrategy).GetConstructor(new Type[] { typeof(IEntityWithKey) });
        private static readonly ConstructorInfo PocoEntityKeyStrategy_ctor = typeof(PocoEntityKeyStrategy).GetConstructor(new Type[0]);
        private static readonly PropertyInfo SnapshotChangeTrackingStrategy_Instance = typeof(SnapshotChangeTrackingStrategy).GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);

        private static readonly MethodInfo EntityWrapperFactory_GetPocoPropertyAccessorStrategyFunc = typeof(EntityWrapperFactory).GetMethod("GetPocoPropertyAccessorStrategyFunc", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo EntityWrapperFactory_GetNullPropertyAccessorStrategyFunc = typeof(EntityWrapperFactory).GetMethod("GetNullPropertyAccessorStrategyFunc", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo EntityWrapperFactory_GetEntityWithChangeTrackerStrategyFunc = typeof(EntityWrapperFactory).GetMethod("GetEntityWithChangeTrackerStrategyFunc", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo EntityWrapperFactory_GetSnapshotChangeTrackingStrategyFunc = typeof(EntityWrapperFactory).GetMethod("GetSnapshotChangeTrackingStrategyFunc", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo EntityWrapperFactory_GetEntityWithKeyStrategyStrategyFunc = typeof(EntityWrapperFactory).GetMethod("GetEntityWithKeyStrategyStrategyFunc", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo EntityWrapperFactory_GetPocoEntityKeyStrategyFunc = typeof(EntityWrapperFactory).GetMethod("GetPocoEntityKeyStrategyFunc", BindingFlags.NonPublic | BindingFlags.Static);

        #endregion

        #region static expressions used in emitters

        private static readonly Expression DBNull_Value = Expression.Constant(DBNull.Value, typeof(object));

        internal static readonly ParameterExpression Shaper_Parameter = Expression.Parameter(typeof(Shaper), "shaper");
        private static readonly ParameterExpression EntityParameter = Expression.Parameter(typeof(object), "entity");

        internal static readonly Expression Shaper_Reader = Expression.Field(Shaper_Parameter, typeof(Shaper).GetField("Reader"));
        private static readonly Expression Shaper_Workspace = Expression.Field(Shaper_Parameter, typeof(Shaper).GetField("Workspace"));
        private static readonly Expression Shaper_State = Expression.Field(Shaper_Parameter, typeof(Shaper).GetField("State"));
        private static readonly Expression Shaper_Context = Expression.Field(Shaper_Parameter, typeof(Shaper).GetField("Context"));
        private static readonly Expression Shaper_Context_Options = Expression.Property(Shaper_Context, typeof(ObjectContext).GetProperty("ContextOptions"));
        private static readonly Expression Shaper_ProxyCreationEnabled = Expression.Property(Shaper_Context_Options, typeof(ObjectContextOptions).GetProperty("ProxyCreationEnabled"));

        #endregion

        /// <summary>
        /// Create expression to AndAlso the expressions and return the result.
        /// </summary>
        private static Expression Emit_AndAlso(IEnumerable<Expression> operands)
        {
            Expression result = null;
            foreach (Expression operand in operands)
            {
                if (result == null)
                {
                    result = operand;
                }
                else
                {
                    result = Expression.AndAlso(result, operand);
                }
            }
            return result;
        }

        /// <summary>
        /// Create expression to bitwise-or the expressions and return the result.
        /// </summary>
        private static Expression Emit_BitwiseOr(IEnumerable<Expression> operands)
        {
            Expression result = null;
            foreach (Expression operand in operands)
            {
                if (result == null)
                {
                    result = operand;
                }
                else
                {
                    result = Expression.Or(result, operand);
                }
            }
            return result;
        }

        /// <summary>
        /// Creates an expression with null value. If the given type cannot be assigned
        /// a null value, we create a value that throws when materializing. We don't throw statically
        /// because we consistently defer type checks until materialization.
        /// 
        /// See SQL BU 588980.
        /// </summary>
        /// <param name="type">Type of null expression.</param>
        /// <returns>Null expression.</returns>
        internal static Expression Emit_NullConstant(Type type)
        {
            Expression nullConstant;
            EntityUtil.CheckArgumentNull(type, "type");

            // check if null can be assigned to the type
            if (type.IsClass || TypeSystem.IsNullableType(type))
            {
                // create the constant directly if it accepts null
                nullConstant = Expression.Constant(null, type);
            }
            else
            {
                // create (object)null and then cast to the type
                nullConstant = Emit_EnsureType(Expression.Constant(null, typeof(object)), type);
            }
            return nullConstant;
        }

        /// <summary>
        /// Emits an expression that represnts a NullEntityWrapper instance.
        /// </summary>
        /// <param name="type">The type of the null to be wrapped</param>
        /// <returns>An expression represnting a wrapped null</returns>
        internal static Expression Emit_WrappedNullConstant(Type type)
        {
            return Expression.Property(null, EntityWrapperFactory_NullWrapper);
        }

        /// <summary>
        /// Create expression that guarantees the input expression is of the specified
        /// type; no Convert is added if the expression is already of the same type.
        /// 
        /// Internal because it is called from the TranslatorResult.
        /// </summary>
        internal static Expression Emit_EnsureType(Expression input, Type type)
        {
            Expression result = input;
            if (input.Type != type && !typeof(IEntityWrapper).IsAssignableFrom(input.Type))
            {
                if (type.IsAssignableFrom(input.Type))
                {
                    // simple convert, just to make sure static type checks succeed
                    result = Expression.Convert(input, type);
                }
                else
                {
                    // user is asking for the 'wrong' type... add exception handling
                    // in case of failure
                    MethodInfo checkedConvertMethod = Translator_CheckedConvert.MakeGenericMethod(input.Type, type);
                    result = Expression.Call(checkedConvertMethod, input);
                }
            }
            return result;
        }

        /// <summary>
        /// Uses Emit_EnsureType and then wraps the result in an IEntityWrapper instance.
        /// </summary>
        /// <param name="input">The expression that creates the entity to be wrapped</param>
        /// <param name="keyReader">Expression to read the entity key</param>
        /// <param name="entitySetReader">Expression to read the entity set</param>
        /// <param name="requestedType">The type that was actuall requested by the client--may be object</param>
        /// <param name="identityType">The type of the identity type of the entity being materialized--never a proxy type</param>
        /// <param name="actualType">The actual type being materialized--may be a proxy type</param>
        /// <param name="mergeOption">Either NoTracking or AppendOnly depending on whether the entity is to be tracked</param>
        /// <param name="isProxy">If true, then a proxy is being created</param>
        /// <returns>An expression representing the IEntityWrapper for the new entity</returns>
        internal static Expression Emit_EnsureTypeAndWrap(Expression input, Expression keyReader, Expression entitySetReader, Type requestedType, Type identityType, Type actualType, MergeOption mergeOption, bool isProxy)
        {
            Expression result = Emit_EnsureType(input, requestedType); // Needed to ensure appropriate exception is thrown
            if (!requestedType.IsClass)
            {
                result = Emit_EnsureType(input, typeof(object));
            }
            result = Emit_EnsureType(result, actualType); // Needed to ensure appropriate type for wrapper constructor
            return CreateEntityWrapper(result, keyReader, entitySetReader, actualType, identityType, mergeOption, isProxy);
        }

        /// <summary>
        /// Returns an expression that creates an IEntityWrapper approprioate for the type of entity being materialized.
        /// </summary>
        private static Expression CreateEntityWrapper(Expression input, Expression keyReader, Expression entitySetReader, Type actualType, Type identityType, MergeOption mergeOption, bool isProxy)
        {
            Expression result;
            bool isIEntityWithKey = typeof(IEntityWithKey).IsAssignableFrom(actualType);
            bool isIEntityWithRelationships = typeof(IEntityWithRelationships).IsAssignableFrom(actualType);
            bool isIEntityWithChangeTracker = typeof(IEntityWithChangeTracker).IsAssignableFrom(actualType);
            if (isIEntityWithRelationships && isIEntityWithChangeTracker && isIEntityWithKey && !isProxy)
            {
                // This is the case where all our interfaces are implemented by the entity and we are not creating a proxy.
                // This is the case that absolutely must be kept fast.  It is a simple call to the wrapper constructor.
                Type genericType = typeof(LightweightEntityWrapper<>).MakeGenericType(actualType);
                ConstructorInfo ci = genericType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, null,
                                                                new Type[] { actualType, typeof(EntityKey), typeof(EntitySet), typeof(ObjectContext), typeof(MergeOption), typeof(Type) }, null);
                result = Expression.New(ci, input, keyReader, entitySetReader, Shaper_Context, Expression.Constant(mergeOption, typeof(MergeOption)), Expression.Constant(identityType, typeof(Type)));
            }
            else
            {
                // This is the general case.  We choose various strategy objects based on the interfaces implemented and
                // whether or not we are creating a proxy.
                // We pass in lambdas to create the strategy objects so that they can have the materialized entity as
                // a parameter while still being set in the wrapper constructor.
                Expression propertyAccessorStrategy = !isIEntityWithRelationships || isProxy ?
                                                      Expression.Call(EntityWrapperFactory_GetPocoPropertyAccessorStrategyFunc) :
                                                      Expression.Call(EntityWrapperFactory_GetNullPropertyAccessorStrategyFunc);

                Expression keyStrategy = isIEntityWithKey ?
                                         Expression.Call(EntityWrapperFactory_GetEntityWithKeyStrategyStrategyFunc) :
                                         Expression.Call(EntityWrapperFactory_GetPocoEntityKeyStrategyFunc);

                Expression changeTrackingStrategy = isIEntityWithChangeTracker ?
                                                    Expression.Call(EntityWrapperFactory_GetEntityWithChangeTrackerStrategyFunc) :
                                                    Expression.Call(EntityWrapperFactory_GetSnapshotChangeTrackingStrategyFunc);

                Type genericType = isIEntityWithRelationships ?
                                   typeof(EntityWrapperWithRelationships<>).MakeGenericType(actualType) :
                                   typeof(EntityWrapperWithoutRelationships<>).MakeGenericType(actualType);

                ConstructorInfo ci = genericType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, null,
                                                                new Type[] { actualType, typeof(EntityKey), typeof(EntitySet), typeof(ObjectContext), typeof(MergeOption), typeof(Type),
                                                                             typeof(Func<object, IPropertyAccessorStrategy>), typeof(Func<object, IChangeTrackingStrategy>), typeof(Func<object, IEntityKeyStrategy>) }, null);
                result = Expression.New(ci, input, keyReader, entitySetReader, Shaper_Context, Expression.Constant(mergeOption, typeof(MergeOption)), Expression.Constant(identityType, typeof(Type)),
                                        propertyAccessorStrategy, changeTrackingStrategy, keyStrategy);
            }
            result = Expression.Convert(result, typeof(IEntityWrapper));
            return result;
        }

        /// <summary>
        /// Takes an expression that represents an IEntityWrapper instance and creates a new
        /// expression that extracts the raw entity from this.
        /// </summary>
        internal static Expression Emit_UnwrapAndEnsureType(Expression input, Type type)
        {
            return Translator.Emit_EnsureType(Expression.Property(input, IEntityWrapper_Entity), type);
        }

        /// <summary>
        /// Method that the generated expression calls when the types are not 
        /// assignable
        /// </summary>
        private static TTarget CheckedConvert<TSource, TTarget>(TSource value)
        {
            checked
            {
                try
                {
                    return (TTarget)(object)value;
                }
                catch (InvalidCastException)
                {
                    Type valueType = value.GetType();

                    // In the case of CompensatingCollection<T>, simply report IEnumerable<T> in the
                    // exception message because the user has no reason to know what the type represents.
                    if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(CompensatingCollection<>))
                    {
                        valueType = typeof(IEnumerable<>).MakeGenericType(valueType.GetGenericArguments());
                    }
                    throw EntityUtil.ValueInvalidCast(valueType, typeof(TTarget));
                }
                catch (NullReferenceException)
                {
                    throw EntityUtil.ValueNullReferenceCast(typeof(TTarget));
                }
            }
        }

        /// <summary>
        /// Create expression to compare the results of two expressions and return
        /// whether they are equal.  Note we have special case logic for byte arrays.
        /// </summary>
        private static Expression Emit_Equal(Expression left, Expression right)
        {
            Expression result;
            Debug.Assert(left.Type == right.Type, "equals with different types");
            if (typeof(byte[]) == left.Type)
            {
                result = Expression.Call(Translator_BinaryEquals, left, right);
            }
            else
            {
                result = Expression.Equal(left, right);
            }
            return result;
        }

        /// <summary>
        /// Helper method used in expressions generated by Emit_Equal to perform a 
        /// byte-by-byte comparison of two byte arrays.  There really ought to be 
        /// a way to do this in the framework but I'm unaware of it.
        /// </summary>
        private static bool BinaryEquals(byte[] left, byte[] right)
        {
            if (null == left)
            {
                return null == right;
            }
            else if (null == right)
            {
                return false;
            }
            if (left.Length != right.Length)
            {
                return false;
            }
            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Creates expression to construct an EntityKey. Assumes that both the key has 
        /// a value (Emit_EntityKey_HasValue == true) and that the EntitySet has value 
        /// (EntitySet != null).
        /// </summary>
        private static Expression Emit_EntityKey_ctor(Translator translator, EntityIdentity entityIdentity, bool isForColumnValue, out Expression entitySetReader) 
        {
            Expression result;
            Expression setEntitySetStateSlotValue = null;

            // First build the expressions that read each value that comprises the EntityKey
            List<Expression> keyReaders = new List<Expression>(entityIdentity.Keys.Length);
            for (int i = 0; i < entityIdentity.Keys.Length; i++)
            {
                Expression keyReader = entityIdentity.Keys[i].Accept(translator, new TranslatorArg(typeof(object))).Expression;
                keyReaders.Add(keyReader);
            }

            // Next build the expression that determines us the entitySet; how we do this differs 
            // depending on whether we have a simple or discriminated identity.

            SimpleEntityIdentity simpleEntityIdentity = entityIdentity as SimpleEntityIdentity;
            if (null != simpleEntityIdentity)
            {
                if (simpleEntityIdentity.EntitySet == null)
                {
                    // 'Free-floating' entities do not have entity keys.
                    entitySetReader = Expression.Constant(null, typeof(EntitySet));
                    return Expression.Constant(null, typeof(EntityKey));
                }
                // For SimpleEntityIdentities, the entitySet expression is a constant
                entitySetReader = Expression.Constant(simpleEntityIdentity.EntitySet, typeof(EntitySet));
            }
            else
            {
                // For DiscriminatedEntityIdentities, the we have to search the EntitySetMap 
                // for the matching discriminator value; we'll get the discriminator first, 
                // the compare them all in sequence.                
                DiscriminatedEntityIdentity discriminatedEntityIdentity = (DiscriminatedEntityIdentity)entityIdentity;

                Expression discriminator = discriminatedEntityIdentity.EntitySetColumnMap.Accept(translator, new TranslatorArg(typeof(int?))).Expression;
                EntitySet[] entitySets = discriminatedEntityIdentity.EntitySetMap;

                // 


                // (_discriminator == 0 ? entitySets[0] : (_discriminator == 1 ? entitySets[1] ... : null)
                entitySetReader = Expression.Constant(null, typeof(EntitySet));
                for (int i = 0; i < entitySets.Length; i++)
                {
                    entitySetReader = Expression.Condition(
                                                Expression.Equal(discriminator, Expression.Constant(i, typeof(int?))),
                                                Expression.Constant(entitySets[i], typeof(EntitySet)),
                                                entitySetReader
                                                );
                }

                // Allocate a stateSlot to contain the entitySet we determine, and ensure we
                // store it there on the way to constructing the key.
                int entitySetStateSlotNumber = translator.AllocateStateSlot();
                setEntitySetStateSlotValue = Emit_Shaper_SetStatePassthrough(entitySetStateSlotNumber, entitySetReader);
                entitySetReader = Emit_Shaper_GetState(entitySetStateSlotNumber, typeof(EntitySet));
            }

            // And now that we have all the pieces, construct the EntityKey using the appropriate
            // constructor (there's an optimized constructor for the single key case)
            if (1 == entityIdentity.Keys.Length)
            {
                // new EntityKey(entitySet, keyReaders[0])
                result = Expression.New(EntityKey_ctor_SingleKey,
                                            entitySetReader,
                                            keyReaders[0]);
            }
            else
            {
                // new EntityKey(entitySet, { keyReaders[0], ... keyReaders[n] })
                result = Expression.New(EntityKey_ctor_CompositeKey,
                                            entitySetReader,
                                            Expression.NewArrayInit(typeof(object), keyReaders));
            }

            // In the case where we've had to store the entitySetReader value in a 
            // state slot, we test the value for non-null before we construct the 
            // entityKey.  We use this opportunity to stuff the value into the state
            // slot, so the code above that attempts to read it from there will find
            // it.
            if (null != setEntitySetStateSlotValue)
            {
                Expression noEntityKeyExpression;
                if (translator.IsValueLayer && !isForColumnValue)
                {
                    noEntityKeyExpression = Expression.Constant(EntityKey.NoEntitySetKey, typeof(EntityKey));
                }
                else
                {
                    noEntityKeyExpression = Expression.Constant(null, typeof(EntityKey));
                }
                result = Expression.Condition(
                                            Expression.Equal(setEntitySetStateSlotValue, Expression.Constant(null, typeof(EntitySet))),
                                            noEntityKeyExpression,
                                            result
                                            );
            }
            return result;
        }

        /// <summary>
        /// Create expression that verifies that the entityKey has a value.  Note we just
        /// presume that if the first key is non-null, all the keys will be valid.
        /// </summary>
        private static Expression Emit_EntityKey_HasValue(SimpleColumnMap[] keyColumns)
        {
            Debug.Assert(0 < keyColumns.Length, "empty keyColumns?");

            // !shaper.Reader.IsDBNull(keyColumn[0].ordinal)
            Expression result = Emit_Reader_IsDBNull(keyColumns[0]);
            result = Expression.Not(result);
            return result;
        }

        /// <summary>
        /// Create expression to call the GetValue method of the shaper's source data reader
        /// </summary>
        private static Expression Emit_Reader_GetValue(int ordinal, Type type)
        {
            // (type)shaper.Reader.GetValue(ordinal)
            Expression result = Emit_EnsureType(Expression.Call(Shaper_Reader, DbDataReader_GetValue, Expression.Constant(ordinal)), type);
            return result;
        }

        /// <summary>
        /// Create expression to call the IsDBNull method of the shaper's source data reader
        /// </summary>
        private static Expression Emit_Reader_IsDBNull(int ordinal)
        {
            // shaper.Reader.IsDBNull(ordinal)
            Expression result = Expression.Call(Shaper_Reader, DbDataReader_IsDBNull, Expression.Constant(ordinal));
            return result;
        }

        /// <summary>
        /// Create expression to call the IsDBNull method of the shaper's source data reader 
        /// for the scalar column represented by the column map.
        /// </summary>
        private static Expression Emit_Reader_IsDBNull(ColumnMap columnMap)
        {
            // 
            Expression result = Emit_Reader_IsDBNull(((ScalarColumnMap)columnMap).ColumnPos);
            return result;
        }

        /// <summary>
        /// Create expression to read a property value with error handling
        /// </summary>
        private static Expression Emit_Shaper_GetPropertyValueWithErrorHandling(Type propertyType, int ordinal, string propertyName, string typeName, TypeUsage columnType)
        {
            // // shaper.GetSpatialColumnValueWithErrorHandling(ordinal, propertyName, typeName, primitiveColumnType) OR shaper.GetColumnValueWithErrorHandling(ordinal, propertyName, typeName)
            Expression result;
            PrimitiveTypeKind primitiveColumnType;
            if (Helper.IsSpatialType(columnType, out primitiveColumnType))
            {
                result = Expression.Call(Shaper_Parameter, Shaper_GetSpatialPropertyValueWithErrorHandling.MakeGenericMethod(propertyType), 
                    Expression.Constant(ordinal), Expression.Constant(propertyName), Expression.Constant(typeName), Expression.Constant(primitiveColumnType, typeof(PrimitiveTypeKind)));
            }
            else
            {
                result = Expression.Call(Shaper_Parameter, Shaper_GetPropertyValueWithErrorHandling.MakeGenericMethod(propertyType), Expression.Constant(ordinal), Expression.Constant(propertyName), Expression.Constant(typeName));
            }
            return result;
        }

        /// <summary>
        /// Create expression to read a column value with error handling
        /// </summary>
        private static Expression Emit_Shaper_GetColumnValueWithErrorHandling(Type resultType, int ordinal, TypeUsage columnType)
        {
            // shaper.GetSpatialColumnValueWithErrorHandling(ordinal, primitiveColumnType) OR shaper.GetColumnValueWithErrorHandling(ordinal)
            Expression result;
            PrimitiveTypeKind primitiveColumnType;
            if (Helper.IsSpatialType(columnType, out primitiveColumnType))
            {
                primitiveColumnType = Helper.IsGeographicType((PrimitiveType)columnType.EdmType) ? PrimitiveTypeKind.Geography : PrimitiveTypeKind.Geometry;
                result = Expression.Call(Shaper_Parameter, Shaper_GetSpatialColumnValueWithErrorHandling.MakeGenericMethod(resultType), Expression.Constant(ordinal), Expression.Constant(primitiveColumnType, typeof(PrimitiveTypeKind)));
            }
            else
            {
                result = Expression.Call(Shaper_Parameter, Shaper_GetColumnValueWithErrorHandling.MakeGenericMethod(resultType), Expression.Constant(ordinal));
            }
            return result;
        }

        /// <summary>
        /// Create expression to read a column value of type System.Data.Spatial.DbGeography by delegating to the DbSpatialServices implementation of the underlying provider
        /// </summary>
        private static Expression Emit_Shaper_GetGeographyColumnValue(int ordinal)
        {
            // shaper.GetGeographyColumnValue(ordinal)
            Expression result = Expression.Call(Shaper_Parameter, Shaper_GetGeographyColumnValue, Expression.Constant(ordinal));
            return result;
        }

        /// <summary>
        /// Create expression to read a column value of type System.Data.Spatial.DbGeometry by delegating to the DbSpatialServices implementation of the underlying provider
        /// </summary>
        private static Expression Emit_Shaper_GetGeometryColumnValue(int ordinal)
        {
            // shaper.GetGeometryColumnValue(ordinal)
            Expression result = Expression.Call(Shaper_Parameter, Shaper_GetGeometryColumnValue, Expression.Constant(ordinal));
            return result;
        }

        /// <summary>
        /// Create expression to read an item from the shaper's state array
        /// </summary>
        private static Expression Emit_Shaper_GetState(int stateSlotNumber, Type type)
        {
            // (type)shaper.State[stateSlotNumber]
            Expression result = Emit_EnsureType(Expression.ArrayIndex(Shaper_State, Expression.Constant(stateSlotNumber)), type);
            return result;
        }

        /// <summary>
        /// Create expression to set an item in the shaper's state array
        /// </summary>
        private static Expression Emit_Shaper_SetState(int stateSlotNumber, Expression value)
        {
            // shaper.SetState<T>(stateSlotNumber, value)
            Expression result = Expression.Call(Shaper_Parameter, Shaper_SetState.MakeGenericMethod(value.Type), Expression.Constant(stateSlotNumber), value);
            return result;
        }

        /// <summary>
        /// Create expression to set an item in the shaper's state array
        /// </summary>
        private static Expression Emit_Shaper_SetStatePassthrough(int stateSlotNumber, Expression value)
        {
            // shaper.SetState<T>(stateSlotNumber, value)
            Expression result = Expression.Call(Shaper_Parameter, Shaper_SetStatePassthrough.MakeGenericMethod(value.Type), Expression.Constant(stateSlotNumber), value);
            return result;
        }
        #endregion

        #region ColumnMapVisitor implementation

        // utility accept that looks up CLR type
        private static TranslatorResult AcceptWithMappedType(Translator translator, ColumnMap columnMap, ColumnMap parent)
        {
            Type type = translator.DetermineClrType(columnMap.Type);
            TranslatorResult result = columnMap.Accept(translator, new TranslatorArg(type));
            return result;
        }

        #region structured columns

        /// <summary>
        /// Visit(ComplexTypeColumnMap)
        /// </summary>
        internal override TranslatorResult Visit(ComplexTypeColumnMap columnMap, TranslatorArg arg)
        {
            Expression result = null;
            Expression nullSentinelCheck = null;

            if (null != columnMap.NullSentinel)
            {
                nullSentinelCheck = Emit_Reader_IsDBNull(columnMap.NullSentinel);
            }

            if (IsValueLayer)
            {
                result = BuildExpressionToGetRecordState(columnMap, null, null, nullSentinelCheck);
            }
            else
            {
                ComplexType complexType = (ComplexType)columnMap.Type.EdmType;
                Type clrType = DetermineClrType(complexType);
                ConstructorInfo constructor = GetConstructor(clrType);

                // Build expressions to read the property values from the source data 
                // reader and bind them to their target properties
                List<MemberBinding> propertyBindings = CreatePropertyBindings(columnMap, clrType, complexType.Properties);

                // We have all the property bindings now; go ahead and build the expression to
                // construct the type and store the property values.
                result = Expression.MemberInit(Expression.New(constructor), propertyBindings);

                // If there's a null sentinel, then everything above is gated upon whether 
                // it's value is DBNull.Value.
                if (null != nullSentinelCheck)
                {
                    // shaper.Reader.IsDBNull(nullsentinelOridinal) ? (type)null : result
                    result = Expression.Condition(nullSentinelCheck, Emit_NullConstant(result.Type), result);
                }
            }
            return new TranslatorResult(result, arg.RequestedType);
        }

        /// <summary>
        /// Visit(EntityColumnMap)
        /// </summary>
        internal override TranslatorResult Visit(EntityColumnMap columnMap, TranslatorArg arg)
        {
            Expression result;

            // Build expressions to read the entityKey and determine the entitySet. Note
            // that we attempt to optimize things such that we won't construct anything 
            // that isn't needed, depending upon the interfaces the clrType derives from 
            // and the MergeOption that was requested.
            //
            // We always need the entitySet, except when MergeOption.NoTracking
            //
            // We always need the entityKey, except when MergeOption.NoTracking and the
            // clrType doesn't derive from IEntityWithKey
            EntityIdentity entityIdentity = columnMap.EntityIdentity;
            Expression entitySetReader = null;
            Expression entityKeyReader = Emit_EntityKey_ctor(this, entityIdentity, false, out entitySetReader);

            if (IsValueLayer)
            {                
                Expression nullCheckExpression = Expression.Not(Emit_EntityKey_HasValue(entityIdentity.Keys));
                //Expression nullCheckExpression = Emit_EntityKey_HasValue(entityIdentity.Keys);
                result = BuildExpressionToGetRecordState(columnMap, entityKeyReader, entitySetReader, nullCheckExpression);
            }
            else
            {
                Expression constructEntity = null;

                EntityType cSpaceType = (EntityType)columnMap.Type.EdmType;
                Debug.Assert(cSpaceType.BuiltInTypeKind == BuiltInTypeKind.EntityType, "Type was " + cSpaceType.BuiltInTypeKind);
                ClrEntityType oSpaceType = (ClrEntityType)LookupObjectMapping(cSpaceType).ClrType;
                Type clrType = oSpaceType.ClrType;

                // Build expressions to read the property values from the source data 
                // reader and bind them to their target properties
                List<MemberBinding> propertyBindings = CreatePropertyBindings(columnMap, clrType, cSpaceType.Properties);

                // We have all the property bindings now; go ahead and build the expression to
                // construct the entity or proxy and store the property values.  We'll wrap it with more
                // stuff that needs to happen (or not) below.
                EntityProxyTypeInfo proxyTypeInfo = EntityProxyFactory.GetProxyType(oSpaceType);

                // If no proxy type exists for the entity, construct the regular entity object.
                // If a proxy type does exist, examine the ObjectContext.ContextOptions.ProxyCreationEnabled flag
                // to determine whether to create a regular or proxy entity object.

                Expression constructNonProxyEntity = Emit_ConstructEntity(oSpaceType, propertyBindings, entityKeyReader, entitySetReader, arg, null);
                if (proxyTypeInfo == null)
                {
                    constructEntity = constructNonProxyEntity;
                }
                else
                {
                    Expression constructProxyEntity = Emit_ConstructEntity(oSpaceType, propertyBindings, entityKeyReader, entitySetReader, arg, proxyTypeInfo);

                    constructEntity = Expression.Condition(Shaper_ProxyCreationEnabled,
                                                           constructProxyEntity,
                                                           constructNonProxyEntity);
                }

                // If we're tracking, call HandleEntity (or HandleIEntityWithKey or 
                // HandleEntityAppendOnly) as appropriate
                if (MergeOption.NoTracking != _mergeOption)
                {
                    Type actualType = proxyTypeInfo == null ? clrType : proxyTypeInfo.ProxyType;
                    if (typeof(IEntityWithKey).IsAssignableFrom(actualType) && MergeOption.AppendOnly != _mergeOption)
                    {
                        constructEntity = Expression.Call(Shaper_Parameter, Shaper_HandleIEntityWithKey.MakeGenericMethod(clrType),
                                                                                            constructEntity,
                                                                                            entitySetReader
                                                                                            );
                    }
                    else
                    {
                        if (MergeOption.AppendOnly == _mergeOption)
                        {
                            // pass through a delegate creating the entity rather than the actual entity, so we can avoid
                            // the cost of materialization when the entity is already in the state manager

                            //Func<Shaper, TEntity> entityDelegate = shaper => constructEntity(shaper);
                            LambdaExpression entityDelegate = CreateInlineDelegate(constructEntity);
                            constructEntity = Expression.Call(Shaper_Parameter, Shaper_HandleEntityAppendOnly.MakeGenericMethod(clrType),
                                                                                            entityDelegate,
                                                                                            entityKeyReader,
                                                                                            entitySetReader
                                                                                            );
                        }
                        else
                        {
                            constructEntity = Expression.Call(Shaper_Parameter, Shaper_HandleEntity.MakeGenericMethod(clrType),
                                                                                            constructEntity,
                                                                                            entityKeyReader,
                                                                                            entitySetReader
                                                                                            );
                        }
                    }
                }
                else
                {
                    constructEntity = Expression.Call(Shaper_Parameter, Shaper_HandleEntityNoTracking.MakeGenericMethod(clrType),
                                                                                            constructEntity
                                                                                            );
                }

                // All the above is gated upon whether there really is an entity value; 
                // we won't bother executing anything unless there is an entityKey value,
                // otherwise we'll just return a typed null.
                result = Expression.Condition(
                                            Emit_EntityKey_HasValue(entityIdentity.Keys),
                                            constructEntity,
                                            Emit_WrappedNullConstant(arg.RequestedType)
                                            );
            }

            return new TranslatorResult(result, arg.RequestedType);
        }

        private Expression Emit_ConstructEntity(EntityType oSpaceType, IEnumerable<MemberBinding> propertyBindings, Expression entityKeyReader, Expression entitySetReader, TranslatorArg arg, EntityProxyTypeInfo proxyTypeInfo)
        {
            bool isProxy = proxyTypeInfo != null;
            Type clrType = oSpaceType.ClrType;
            Type actualType;

            Expression constructEntity;

            if (isProxy)
            {
                constructEntity = Expression.MemberInit(Expression.New(proxyTypeInfo.ProxyType), propertyBindings);
                actualType = proxyTypeInfo.ProxyType;
            }
            else
            {
                ConstructorInfo constructor = GetConstructor(clrType);
                constructEntity = Expression.MemberInit(Expression.New(constructor), propertyBindings);
                actualType = clrType;
            }

            // After calling the constructor, immediately create an IEntityWrapper instance for the entity.
            constructEntity = Emit_EnsureTypeAndWrap(constructEntity, entityKeyReader, entitySetReader, arg.RequestedType, clrType, actualType,
                                                     _mergeOption == MergeOption.NoTracking ? MergeOption.NoTracking : MergeOption.AppendOnly, isProxy);

            if (isProxy)
            {
                // Since we created a proxy, we now need to give it a reference to the wrapper that we just created.
                constructEntity = Expression.Call(Expression.Constant(proxyTypeInfo), EntityProxyTypeInfo_SetEntityWrapper, constructEntity);

                if (proxyTypeInfo.InitializeEntityCollections != null)
                {
                    constructEntity = Expression.Call(proxyTypeInfo.InitializeEntityCollections, constructEntity);
                }
            }

            return constructEntity;
        }

        /// <summary>
        /// Prepare a list of PropertyBindings for each item in the specified property 
        /// collection such that the mapped property of the specified clrType has its
        /// value set from the source data reader.  
        /// 
        /// Along the way we'll keep track of non-public properties and properties that
        /// have link demands, so we can ensure enforce them.
        /// </summary>
        private List<MemberBinding> CreatePropertyBindings(StructuredColumnMap columnMap, Type clrType, ReadOnlyMetadataCollection<EdmProperty> properties)
        {
            List<MemberBinding> result = new List<MemberBinding>(columnMap.Properties.Length);

            ObjectTypeMapping mapping = LookupObjectMapping(columnMap.Type.EdmType);

            for (int i = 0; i < columnMap.Properties.Length; i++)
            {
                EdmProperty edmProperty = mapping.GetPropertyMap(properties[i].Name).ClrProperty;

                // get MethodInfo for setter
                MethodInfo propertyAccessor;
                Type propertyType;
                LightweightCodeGenerator.ValidateSetterProperty(edmProperty.EntityDeclaringType, edmProperty.PropertySetterHandle, out propertyAccessor, out propertyType);

                // determine if any security checks are required
                if (!LightweightCodeGenerator.IsPublic(propertyAccessor))
                {
                    _hasNonPublicMembers = true;
                }

                // get translation of property value
                Expression valueReader = columnMap.Properties[i].Accept(this, new TranslatorArg(propertyType)).Expression;

                ScalarColumnMap scalarColumnMap = columnMap.Properties[i] as ScalarColumnMap;
                if (null != scalarColumnMap)
                {
                    string propertyName = propertyAccessor.Name.Substring(4); // substring to strip "set_"

                    // create a value reader with error handling
                    Expression valueReaderWithErrorHandling = Emit_Shaper_GetPropertyValueWithErrorHandling(propertyType, scalarColumnMap.ColumnPos, propertyName, propertyAccessor.DeclaringType.Name, scalarColumnMap.Type);
                    _currentCoordinatorScratchpad.AddExpressionWithErrorHandling(valueReader, valueReaderWithErrorHandling);
                }

                Type entityDeclaringType = Type.GetTypeFromHandle(edmProperty.EntityDeclaringType);
                MemberBinding binding = Expression.Bind(GetProperty(propertyAccessor, entityDeclaringType), valueReader);
                result.Add(binding);
            }
            return result;
        }

        /// <summary>
        /// Gets the PropertyInfo representing the property with which the given setter method is associated.
        /// This code is taken from Expression.Bind(MethodInfo) but adapted to take a type such that it
        /// will work in cases in which the property was declared on a generic base class.  In such cases,
        /// the declaringType needs to be the actual entity type, rather than the base class type.  Note that
        /// declaringType can be null, in which case the setterMethod.DeclaringType is used.
        /// </summary>
        private static PropertyInfo GetProperty(MethodInfo setterMethod, Type declaringType)
        {
            if (declaringType == null)
            {
                declaringType = setterMethod.DeclaringType;
            }
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            foreach (PropertyInfo propertyInfo in declaringType.GetProperties(bindingAttr))
            {
                if (propertyInfo.GetSetMethod(nonPublic: true) == setterMethod)
                {
                    return propertyInfo;
                }
            }
            Debug.Fail("Should always find a property for the setterMethod since we got the setter method from a property in the first place.");
            return null;
        }

        /// <summary>
        /// Visit(SimplePolymorphicColumnMap)
        /// </summary>
        internal override TranslatorResult Visit(SimplePolymorphicColumnMap columnMap, TranslatorArg arg)
        {
            Expression result;

            // We're building a conditional ladder, where we'll compare each 
            // discriminator value with the one from the source data reader, and 
            // we'll pick that type if they match.
            Expression discriminatorReader = AcceptWithMappedType(this, columnMap.TypeDiscriminator, columnMap).Expression;

            if (IsValueLayer)
            {
                result = Emit_EnsureType(
                                BuildExpressionToGetRecordState(columnMap, null, null, Expression.Constant(true)),
                                arg.RequestedType);
            }
            else
            {
                result = Emit_WrappedNullConstant(arg.RequestedType); // the default
            }

            foreach (var typeChoice in columnMap.TypeChoices)
            {
                // determine CLR type for the type choice, and don't bother adding 
                // this choice if it can't produce a result
                Type type = DetermineClrType(typeChoice.Value.Type);

                if (type.IsAbstract)
                {
                    continue;
                }

                Expression discriminatorConstant = Expression.Constant(typeChoice.Key, discriminatorReader.Type);
                Expression discriminatorMatches;

                // For string types, we have to use a specific comparison that handles
                // trailing spaces properly, not just the general equality test we use 
                // elsewhere.
                if (discriminatorReader.Type == typeof(string))
                {
                    discriminatorMatches = Expression.Call(Expression.Constant(TrailingSpaceStringComparer.Instance), IEqualityComparerOfString_Equals, discriminatorConstant, discriminatorReader);
                }
                else
                {
                    discriminatorMatches = Emit_Equal(discriminatorConstant, discriminatorReader);
                }

                result = Expression.Condition(discriminatorMatches,
                                            typeChoice.Value.Accept(this, arg).Expression,
                                            result);

            }
            return new TranslatorResult(result, arg.RequestedType);
        }

        /// <summary>
        /// Visit(MultipleDiscriminatorPolymorphicColumnMap)
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal override TranslatorResult Visit(MultipleDiscriminatorPolymorphicColumnMap columnMap, TranslatorArg arg)
        {
            MethodInfo multipleDiscriminatorPolymorphicColumnMapHelper = Translator_MultipleDiscriminatorPolymorphicColumnMapHelper.MakeGenericMethod(arg.RequestedType);
            Expression result = (Expression)multipleDiscriminatorPolymorphicColumnMapHelper.Invoke(this, new object[] { columnMap, arg });
            return new TranslatorResult(result, arg.RequestedType);
        }

        /// <summary>
        /// Helper method to simplify the construction of the types; I'm just too lazy to 
        /// create all the nested generic types needed to this by hand.
        /// </summary>
        private Expression MultipleDiscriminatorPolymorphicColumnMapHelper<TElement>(MultipleDiscriminatorPolymorphicColumnMap columnMap, TranslatorArg arg)
        {
            // construct an array of discriminator values
            Expression[] discriminatorReaders = new Expression[columnMap.TypeDiscriminators.Length];
            for (int i = 0; i < discriminatorReaders.Length; i++)
            {
                discriminatorReaders[i] = columnMap.TypeDiscriminators[i].Accept(this, new TranslatorArg(typeof(object))).Expression;
            }
            Expression discriminatorValues = Expression.NewArrayInit(typeof(object), discriminatorReaders);

            // Next build the expressions that will construct the type choices. An array of KeyValuePair<EntityType, Func<Shaper, TElement>>
            List<Expression> elementDelegates = new List<Expression>();
            Type typeDelegatePairType = typeof(KeyValuePair<EntityType, Func<Shaper, TElement>>);
            ConstructorInfo typeDelegatePairConstructor = typeDelegatePairType.GetConstructor(new Type[] { typeof(EntityType), typeof(Func<Shaper, TElement>) });
            foreach (var typeChoice in columnMap.TypeChoices)
            {
                Expression typeReader = Emit_EnsureType(AcceptWithMappedType(this, typeChoice.Value, columnMap).UnwrappedExpression, typeof(TElement));
                LambdaExpression typeReaderDelegate = CreateInlineDelegate(typeReader);
                Expression typeDelegatePair = Expression.New(
                                                    typeDelegatePairConstructor, 
                                                    Expression.Constant(typeChoice.Key), 
                                                    typeReaderDelegate
                                                    );
                elementDelegates.Add(typeDelegatePair);
            }

            // invoke shaper.Discrimate({ discriminatorValue1...discriminatorValueN }, discriminateDelegate, elementDelegates)
            MethodInfo shaperDiscriminateOfT = Shaper_Discriminate.MakeGenericMethod(typeof(TElement));
            Expression result = Expression.Call(Shaper_Parameter, shaperDiscriminateOfT,
                                                    discriminatorValues,
                                                    Expression.Constant(columnMap.Discriminate),
                                                    Expression.NewArrayInit(typeDelegatePairType, elementDelegates)
                                                    );
            return result;
        }

        /// <summary>
        /// Visit(RecordColumnMap)
        /// </summary>
        internal override TranslatorResult Visit(RecordColumnMap columnMap, TranslatorArg arg)
        {
            Expression result = null;
            Expression nullSentinelCheck = null;

            if (null != columnMap.NullSentinel)
            {
                nullSentinelCheck = Emit_Reader_IsDBNull(columnMap.NullSentinel);
            }

            if (IsValueLayer)
            {
                result = BuildExpressionToGetRecordState(columnMap, null, null, nullSentinelCheck);
            }
            else
            {
                Debug.Assert(columnMap.Type.EdmType.BuiltInTypeKind == BuiltInTypeKind.RowType, "RecordColumnMap without RowType?"); // we kind of depend upon this 
                Expression nullConstant;

                // There are (at least) three different reasons we have a RecordColumnMap
                // so pick the method that handles the reason we have for this one.
                InitializerMetadata initializerMetadata;
                if (InitializerMetadata.TryGetInitializerMetadata(columnMap.Type, out initializerMetadata))
                {
                    result = HandleLinqRecord(columnMap, initializerMetadata);
                    nullConstant = Emit_NullConstant(result.Type);
                }
                else
                {
                    RowType spanRowType = (RowType)columnMap.Type.EdmType;

                    if (null != _spanIndex && _spanIndex.HasSpanMap(spanRowType))
                    {
                        result = HandleSpandexRecord(columnMap, arg, spanRowType);
                        nullConstant = Emit_WrappedNullConstant(result.Type);
                    }
                    else
                    {
                        result = HandleRegularRecord(columnMap, arg, spanRowType);
                        nullConstant = Emit_NullConstant(result.Type);
                    }
                }

                // If there is a null sentinel process it accordingly.
                if (null != nullSentinelCheck)
                {
                    // shaper.Reader.IsDBNull(nullsentinelOridinal) ? (type)null : result
                    result = Expression.Condition(nullSentinelCheck, nullConstant, result);
                }
            }
            return new TranslatorResult(result, arg.RequestedType);
        }

        private Expression BuildExpressionToGetRecordState(StructuredColumnMap columnMap, Expression entityKeyReader, Expression entitySetReader, Expression nullCheckExpression)
        {
            RecordStateScratchpad recordStateScratchpad = _currentCoordinatorScratchpad.CreateRecordStateScratchpad();

            int stateSlotNumber = AllocateStateSlot();
            recordStateScratchpad.StateSlotNumber = stateSlotNumber;

            int propertyCount = columnMap.Properties.Length;
            int readerCount = (null != entityKeyReader) ? propertyCount + 1 : propertyCount;

            recordStateScratchpad.ColumnCount = propertyCount;

            // We can have an entity here, even though it's a RecordResultColumn, because
            // it may be a polymorphic type; eg: TREAT(Product AS DiscontinuedProduct); we
            // construct an EntityRecordInfo with a sentinel EntityNotValidKey as it's Key
            EntityType entityTypeMetadata = null;
            if (TypeHelpers.TryGetEdmType<EntityType>(columnMap.Type, out entityTypeMetadata))
            {
                recordStateScratchpad.DataRecordInfo = new EntityRecordInfo(entityTypeMetadata, EntityKey.EntityNotValidKey, null);
            }
            else
            {
                TypeUsage edmType = Helper.GetModelTypeUsage(columnMap.Type);
                recordStateScratchpad.DataRecordInfo = new DataRecordInfo(edmType);
            }

            Expression[] propertyReaders = new Expression[readerCount];
            string[] propertyNames = new string[recordStateScratchpad.ColumnCount];
            TypeUsage[] typeUsages = new TypeUsage[recordStateScratchpad.ColumnCount];

            for (int ordinal = 0; ordinal < propertyCount; ordinal++)
            {
                Expression propertyReader = columnMap.Properties[ordinal].Accept(this, new TranslatorArg(typeof(Object))).Expression;

                // recordState.SetColumnValue(i, propertyReader ?? DBNull.Value)
                propertyReaders[ordinal] = Expression.Call(Shaper_Parameter, Shaper_SetColumnValue, 
                                                        Expression.Constant(stateSlotNumber),
                                                        Expression.Constant(ordinal), 
                                                        Expression.Coalesce(propertyReader, DBNull_Value)
                                                    );

                propertyNames[ordinal] = columnMap.Properties[ordinal].Name;
                typeUsages[ordinal] = columnMap.Properties[ordinal].Type;
            }

            if (null != entityKeyReader)
            {
                propertyReaders[readerCount - 1] = Expression.Call(Shaper_Parameter, Shaper_SetEntityRecordInfo,
                                                        Expression.Constant(stateSlotNumber),
                                                        entityKeyReader,
                                                        entitySetReader);
            }

            recordStateScratchpad.GatherData = Emit_BitwiseOr(propertyReaders);
            recordStateScratchpad.PropertyNames = propertyNames;
            recordStateScratchpad.TypeUsages = typeUsages;

            // Finally, build the expression to read the recordState from the shaper state

            // (RecordState)shaperState.State[stateSlotNumber].GatherData(shaper)           
            Expression result = Expression.Call(Emit_Shaper_GetState(stateSlotNumber, typeof(RecordState)), RecordState_GatherData, Shaper_Parameter);

            // If there's a null check, then everything above is gated upon whether 
            // it's value is DBNull.Value.
            if (null != nullCheckExpression)
            {
                Expression nullResult = Expression.Call(Emit_Shaper_GetState(stateSlotNumber, typeof(RecordState)), RecordState_SetNullRecord, Shaper_Parameter);
                // nullCheckExpression ? (type)null : result
                result = Expression.Condition(nullCheckExpression, nullResult, result);
            }
            return result;
        }

        /// <summary>
        /// Build expression to materialize LINQ initialization types (anonymous 
        /// types, IGrouping, EntityCollection)
        /// </summary>
        private Expression HandleLinqRecord(RecordColumnMap columnMap, InitializerMetadata initializerMetadata)
        {
            List<TranslatorResult> propertyReaders = new List<TranslatorResult>(columnMap.Properties.Length);

            foreach (var pair in columnMap.Properties.Zip(initializerMetadata.GetChildTypes()))
            {
                ColumnMap propertyColumnMap = pair.Key;
                Type type = pair.Value;

                // Note that we're not just blindly using the type from the column map
                // because we need to match the type thatthe initializer says it needs; 
                // that's why were not using AcceptWithMappedType;
                if (null == type)
                {
                    type = DetermineClrType(propertyColumnMap.Type);
                }

                TranslatorResult propertyReader = propertyColumnMap.Accept(this, new TranslatorArg(type));
                propertyReaders.Add(propertyReader);
            }

            Expression result = initializerMetadata.Emit(this, propertyReaders);
            return result;
        }

        /// <summary>
        /// Build expression to materialize a data record.
        /// </summary>
        private Expression HandleRegularRecord(RecordColumnMap columnMap, TranslatorArg arg, RowType spanRowType)
        {
            // handle regular records

            // Build an array of expressions that read the individual values from the 
            // source data reader.
            Expression[] columnReaders = new Expression[columnMap.Properties.Length];
            for (int i = 0; i < columnReaders.Length; i++)
            {
                Expression columnReader = AcceptWithMappedType(this, columnMap.Properties[i], columnMap).UnwrappedExpression;

                // ((object)columnReader) ?? DBNull.Value
                columnReaders[i] = Expression.Coalesce(Emit_EnsureType(columnReader, typeof(object)), DBNull_Value);
            }
            // new object[] {columnReader0..columnReaderN}
            Expression columnReaderArray = Expression.NewArrayInit(typeof(object), columnReaders);


            // Get an expression representing the TypeUsage of the MaterializedDataRecord 
            // we're about to construct; we need to remove the span information from it, 
            // though, since we don't want to surface that...
            TypeUsage type = columnMap.Type;
            if (null != _spanIndex)
            {
                type = _spanIndex.GetSpannedRowType(spanRowType) ?? type;
            }
            Expression typeUsage = Expression.Constant(type, typeof(TypeUsage));

            // new MaterializedDataRecord(Shaper.Workspace, typeUsage, values)
            Expression result = Emit_EnsureType(Expression.New(MaterializedDataRecord_ctor, Shaper_Workspace, typeUsage, columnReaderArray), arg.RequestedType);
            return result;
        }

        /// <summary>
        /// Build expression to materialize the spanned information
        /// </summary>
        private Expression HandleSpandexRecord(RecordColumnMap columnMap, TranslatorArg arg, RowType spanRowType)
        {
            Dictionary<int, AssociationEndMember> spanMap = _spanIndex.GetSpanMap(spanRowType);

            // First, build the expression to materialize the root item.
            Expression result = columnMap.Properties[0].Accept(this, arg).Expression;

            // Now build expressions that call into the appropriate shaper method
            // for the type of span for each spanned item.
            for (int i = 1; i < columnMap.Properties.Length; i++)
            {
                AssociationEndMember targetMember = spanMap[i];
                TranslatorResult propertyTranslatorResult = AcceptWithMappedType(this, columnMap.Properties[i], columnMap);
                Expression spannedResultReader = propertyTranslatorResult.Expression;

                // figure out the flavor of the span
                CollectionTranslatorResult collectionTranslatorResult = propertyTranslatorResult as CollectionTranslatorResult;
                if (null != collectionTranslatorResult)
                {
                    Expression expressionToGetCoordinator = collectionTranslatorResult.ExpressionToGetCoordinator;

                    // full span collection
                    Type elementType = spannedResultReader.Type.GetGenericArguments()[0];

                    MethodInfo handleFullSpanCollectionMethod = Shaper_HandleFullSpanCollection.MakeGenericMethod(arg.RequestedType, elementType);
                    result = Expression.Call(Shaper_Parameter, handleFullSpanCollectionMethod, result, expressionToGetCoordinator, Expression.Constant(targetMember));
                }
                else
                {
                    if (typeof(EntityKey) == spannedResultReader.Type)
                    {
                        // relationship span
                        MethodInfo handleRelationshipSpanMethod = Shaper_HandleRelationshipSpan.MakeGenericMethod(arg.RequestedType);
                        result = Expression.Call(Shaper_Parameter, handleRelationshipSpanMethod, result, spannedResultReader, Expression.Constant(targetMember));
                    }
                    else
                    {
                        // full span element
                        MethodInfo handleFullSpanElementMethod = Shaper_HandleFullSpanElement.MakeGenericMethod(arg.RequestedType, spannedResultReader.Type);
                        result = Expression.Call(Shaper_Parameter, handleFullSpanElementMethod, result, spannedResultReader, Expression.Constant(targetMember));
                    }
                }
            }
            return result;
        }

        #endregion

        #region collection columns

        /// <summary>
        /// Visit(SimpleCollectionColumnMap)
        /// </summary>
        internal override TranslatorResult Visit(SimpleCollectionColumnMap columnMap, TranslatorArg arg)
        {
            return ProcessCollectionColumnMap(columnMap, arg);
        }

        /// <summary>
        /// Visit(DiscriminatedCollectionColumnMap)
        /// </summary>
        internal override TranslatorResult Visit(DiscriminatedCollectionColumnMap columnMap, TranslatorArg arg)
        {
            return ProcessCollectionColumnMap(columnMap, arg, columnMap.Discriminator, columnMap.DiscriminatorValue);
        }

        /// <summary>
        /// Common code for both Simple and Discrminated Column Maps.
        /// </summary>
        private TranslatorResult ProcessCollectionColumnMap(CollectionColumnMap columnMap, TranslatorArg arg)
        {
            return ProcessCollectionColumnMap(columnMap, arg, null, null);
        }

        /// <summary>
        /// Common code for both Simple and Discrminated Column Maps.
        /// </summary>
        private TranslatorResult ProcessCollectionColumnMap(CollectionColumnMap columnMap, TranslatorArg arg, ColumnMap discriminatorColumnMap, object discriminatorValue)
        {
            Type elementType = DetermineElementType(arg.RequestedType, columnMap);

            // CoordinatorScratchpad aggregates information about the current nested
            // result (represented by the given CollectionColumnMap)
            CoordinatorScratchpad coordinatorScratchpad = new CoordinatorScratchpad(elementType);

            // enter scope for current coordinator when translating children, etc.
            EnterCoordinatorTranslateScope(coordinatorScratchpad);


            ColumnMap elementColumnMap = columnMap.Element;

            if (IsValueLayer)
            {
                StructuredColumnMap structuredElement = elementColumnMap as StructuredColumnMap;

                // If we have a collection of non-structured types we have to put 
                // a structure around it, because we don't have data readers of 
                // scalars, only structures.  We don't need a null sentinel because
                // this structure can't ever be null.
                if (null == structuredElement)
                {
                    ColumnMap[] columnMaps = new ColumnMap[1] { columnMap.Element };
                    elementColumnMap = new RecordColumnMap(columnMap.Element.Type, columnMap.Element.Name, columnMaps, null);
                }
            }

            // Build the expression that will construct the element of the collection
            // from the source data reader.
            // We use UnconvertedExpression here so we can defer doing type checking in case
            // we need to translate to a POCO collection later in the process.
            Expression elementReader = elementColumnMap.Accept(this, new TranslatorArg(elementType)).UnconvertedExpression;

            // Build the expression(s) that read the collection's keys from the source
            // data reader; note that the top level collection may not have keys if there
            // are no children.
            Expression[] keyReaders;

            if (null != columnMap.Keys)
            {
                keyReaders = new Expression[columnMap.Keys.Length];
                for (int i = 0; i < keyReaders.Length; i++)
                {
                    Expression keyReader = AcceptWithMappedType(this, columnMap.Keys[i], columnMap).Expression;
                    keyReaders[i] = keyReader;
                }
            }
            else
            {
                keyReaders = new Expression[] { };
            }

            // Build the expression that reads the discriminator value from the source
            // data reader.
            Expression discriminatorReader = null;
            if (null != discriminatorColumnMap)
            {
                discriminatorReader = AcceptWithMappedType(this, discriminatorColumnMap, columnMap).Expression;
            }

            // get expression retrieving the coordinator
            Expression expressionToGetCoordinator = BuildExpressionToGetCoordinator(elementType, elementReader, keyReaders, discriminatorReader, discriminatorValue, coordinatorScratchpad);
            MethodInfo getElementsExpression = typeof(Coordinator<>).MakeGenericType(elementType).GetMethod("GetElements", BindingFlags.NonPublic | BindingFlags.Instance);

            Expression result;
            if (IsValueLayer)
            {
                result = expressionToGetCoordinator;
            }
            else
            {
                // coordinator.GetElements()
                result = Expression.Call(expressionToGetCoordinator, getElementsExpression);

                // Perform the type check that was previously deferred so we could process POCO collections.
                coordinatorScratchpad.Element = Emit_EnsureType(coordinatorScratchpad.Element, elementType);

                // When materializing specifically requested collection types, we need
                // to transfer the results from the Enumerable to the requested collection.
                Type innerElementType;
                if (EntityUtil.TryGetICollectionElementType(arg.RequestedType, out innerElementType))
                {
                    // Given we have some type that implements ICollection<T>, we need to decide what concrete
                    // collection type to instantiate--See EntityUtil.DetermineCollectionType for details.
                    var typeToInstantiate = EntityUtil.DetermineCollectionType(arg.RequestedType);

                    if (typeToInstantiate == null)
                    {
                        throw EntityUtil.InvalidOperation(Strings.ObjectQuery_UnableToMaterializeArbitaryProjectionType(arg.RequestedType));
                    }

                    Type listOfElementType = typeof(List<>).MakeGenericType(innerElementType);
                    if (typeToInstantiate != listOfElementType)
                    {
                        coordinatorScratchpad.InitializeCollection = Emit_EnsureType(
                            Expression.New(GetConstructor(typeToInstantiate)),
                            typeof(ICollection<>).MakeGenericType(innerElementType));
                    }
                    result = Emit_EnsureType(result, arg.RequestedType);
                }
                else
                {
                    // If any compensation is required (returning IOrderedEnumerable<T>, not 
                    // just vanilla IEnumerable<T> we must wrap the result with a static class
                    // that is of the type expected.
                    if (!arg.RequestedType.IsAssignableFrom(result.Type))
                    {
                        // new CompensatingCollection<TElement>(_collectionReader)
                        Type compensatingCollectionType = typeof(CompensatingCollection<>).MakeGenericType(elementType);
                        ConstructorInfo constructorInfo = compensatingCollectionType.GetConstructors()[0];
                        result = Emit_EnsureType(Expression.New(constructorInfo, result), compensatingCollectionType);
                    }
                }
            }
            ExitCoordinatorTranslateScope();
            return new CollectionTranslatorResult(result, columnMap, arg.RequestedType, expressionToGetCoordinator);
        }

        /// <summary>
        /// Returns the CLR Type of the element of the collection
        /// </summary>
        private Type DetermineElementType(Type collectionType, CollectionColumnMap columnMap)
        {
            Type result = null;

            if (IsValueLayer)
            {
                result = typeof(RecordState);
            }
            else
            {
                result = TypeSystem.GetElementType(collectionType);

                // GetElementType returns the input type if it is not a collection.
                if (result == collectionType)
                {
                    // if the user isn't asking for a CLR collection type (e.g. ObjectQuery<object>("{{1, 2}}")), we choose for them
                    TypeUsage edmElementType = ((CollectionType)columnMap.Type.EdmType).TypeUsage; // the TypeUsage of the Element of the collection.
                    result = DetermineClrType(edmElementType);
                }
            }
            return result;
        }

        /// <summary>
        /// Build up the coordinator graph using Enter/ExitCoordinatorTranslateScope.
        /// </summary>
        private void EnterCoordinatorTranslateScope(CoordinatorScratchpad coordinatorScratchpad)
        {
            if (null == _rootCoordinatorScratchpad)
            {
                coordinatorScratchpad.Depth = 0;
                _rootCoordinatorScratchpad = coordinatorScratchpad;
                _currentCoordinatorScratchpad = coordinatorScratchpad;
            }
            else
            {
                coordinatorScratchpad.Depth = _currentCoordinatorScratchpad.Depth + 1;
                _currentCoordinatorScratchpad.AddNestedCoordinator(coordinatorScratchpad);
                _currentCoordinatorScratchpad = coordinatorScratchpad;
            }
        }

        private void ExitCoordinatorTranslateScope()
        {
            _currentCoordinatorScratchpad = _currentCoordinatorScratchpad.Parent;
        }

        /// <summary>
        /// Return an expression to read the coordinator from a state slot at 
        /// runtime.  This is the method where we store the expressions we've
        /// been building into the CoordinatorScratchpad, which we'll compile
        /// later, once we've left the visitor.
        /// </summary>
        private Expression BuildExpressionToGetCoordinator(Type elementType, Expression element, Expression[] keyReaders, Expression discriminator, object discriminatorValue, CoordinatorScratchpad coordinatorScratchpad)
        {
            int stateSlotNumber = AllocateStateSlot();
            coordinatorScratchpad.StateSlotNumber = stateSlotNumber;

            // Ensure that the element type of the collec element translator
            coordinatorScratchpad.Element = element;

            // Build expressions to set the key values into their state slots, and
            // to compare the current values from the source reader with the values
            // in the slots.
            List<Expression> setKeyTerms = new List<Expression>(keyReaders.Length);
            List<Expression> checkKeyTerms = new List<Expression>(keyReaders.Length);

            foreach (Expression keyReader in keyReaders)
            {
                // allocate space for the key value in the reader state
                int keyStateSlotNumber = AllocateStateSlot();

                // SetKey: readerState.SetState<T>(stateSlot, keyReader)
                setKeyTerms.Add(Emit_Shaper_SetState(keyStateSlotNumber, keyReader));

                // CheckKey: ((T)readerState.State[ordinal]).Equals(keyValue)
                checkKeyTerms.Add(Emit_Equal(
                                        Emit_Shaper_GetState(keyStateSlotNumber, keyReader.Type), 
                                        keyReader
                                        )
                                 );
            }

            // For setting keys, we use BitwiseOr so that we don't short-circuit (all  
            // key terms are set)
            coordinatorScratchpad.SetKeys = Emit_BitwiseOr(setKeyTerms);

            // When checking for equality, we use AndAlso so that we short-circuit (return
            // as soon as key values don't match)
            coordinatorScratchpad.CheckKeys = Emit_AndAlso(checkKeyTerms);

            if (null != discriminator)
            {
                // discriminatorValue == discriminator
                coordinatorScratchpad.HasData = Emit_Equal(   
                                                    Expression.Constant(discriminatorValue, discriminator.Type), 
                                                    discriminator
                                                    );
            }

            // Finally, build the expression to read the coordinator from the state
            // (Coordinator<elementType>)readerState.State[stateOrdinal]
            Expression result = Emit_Shaper_GetState(stateSlotNumber, typeof(Coordinator<>).MakeGenericType(elementType));
            return result;
        }

        #endregion

        #region "scalar" columns

        /// <summary>
        /// Visit(RefColumnMap)
        /// 
        /// If the entityKey has a value, then return it otherwise return a null 
        /// valued EntityKey.  The EntityKey construction is the tricky part.
        /// </summary>
        internal override TranslatorResult Visit(RefColumnMap columnMap, TranslatorArg arg)
        {
            EntityIdentity entityIdentity = columnMap.EntityIdentity;
            Expression entitySetReader; // Ignored here; used when constructing Entities

            // hasValue ? entityKey : (EntityKey)null
            Expression result = Expression.Condition(
                                                Emit_EntityKey_HasValue(entityIdentity.Keys),
                                                Emit_EntityKey_ctor(this, entityIdentity, true, out entitySetReader),
                                                Expression.Constant(null, typeof(EntityKey))
                                                );
            return new TranslatorResult(result, arg.RequestedType);
        }

        /// <summary>
        /// Visit(ScalarColumnMap)
        /// 
        /// Pretty basic stuff here; we just call the method that matches the
        /// type of the column.  Of course we have to handle nullable/non-nullable
        /// types, and non-value types.
        /// </summary>
        internal override TranslatorResult Visit(ScalarColumnMap columnMap, TranslatorArg arg)
        {
            Type type = arg.RequestedType;
            TypeUsage columnType = columnMap.Type;
            int ordinal = columnMap.ColumnPos;
            Expression result;
            
            // 1. Create an expression to access the column value as an instance of the correct type. For non-spatial types this requires a call to one of the
            //    DbDataReader GetXXX methods; spatial values must be read using the provider's spatial services implementation.
            // 2. If the type was nullable (strings, byte[], Nullable<T>), wrap the expression with a check for the DBNull value and produce the correct typed null instead.
            //    Since the base spatial types (DbGeography/DbGeometry) are reference types, this is always required for spatial columns.
            // 3. Also create a version of the expression with error handling so that we can throw better exception messages when needed
            //
            PrimitiveTypeKind typeKind;
            if (Helper.IsSpatialType(columnType, out typeKind))
            {
                Debug.Assert(Helper.IsGeographicType((PrimitiveType)columnType.EdmType) || Helper.IsGeometricType((PrimitiveType)columnType.EdmType), "Spatial primitive type is neither Geometry or Geography?");
                result = Emit_Conditional_NotDBNull(Helper.IsGeographicType((PrimitiveType)columnType.EdmType) ? Emit_EnsureType(Emit_Shaper_GetGeographyColumnValue(ordinal), type)
                                                                                            : Emit_EnsureType(Emit_Shaper_GetGeometryColumnValue(ordinal), type),
                                                    ordinal, type);
            }
            else
            {
                bool needsNullableCheck;
                MethodInfo readerMethod = GetReaderMethod(type, out needsNullableCheck);

                result = Expression.Call(Shaper_Reader, readerMethod, Expression.Constant(ordinal));

                // if the requested type is a nullable enum we need to cast it first to the non-nullable enum type to avoid InvalidCastException.
                // Note that we guard against null values by wrapping the expression with DbNullCheck later. Also we don't actually 
                // look at the type of the value returned by reader. If the value is not castable to enum we will fail with cast exception.
                Type nonNullableType = TypeSystem.GetNonNullableType(type);
                if (nonNullableType.IsEnum && nonNullableType != type)
                {
                    Debug.Assert(needsNullableCheck, "This is a nullable enum so needsNullableCheck should be true to emit code that handles null values read from the reader.");

                    result = Expression.Convert(result, nonNullableType);
                }
                else if(type == typeof(object))
                {
                    Debug.Assert(!needsNullableCheck, "If the requested type is object there is no special handling for null values returned from the reader.");

                    // special case for an OSpace query where the requested type is object but the column type is of an enum type. In this case
                    // we want to return a boxed value of enum type instead a boxed value of the enum underlying type. We also need to handle null
                    // values to return DBNull to be consistent with behavior for primitive types (e.g. int)
                    if (!IsValueLayer && TypeSemantics.IsEnumerationType(columnType))
                    {
                        result = Expression.Condition(
                                    Emit_Reader_IsDBNull(ordinal),
                                        result,
                                        Expression.Convert(Expression.Convert(result, TypeSystem.GetNonNullableType(DetermineClrType(columnType.EdmType))), typeof(object)));
                    }
                }

                // (type)shaper.Reader.Get???(ordinal)
                result = Emit_EnsureType(result, type);
                
                if (needsNullableCheck)
                {
                    result = Emit_Conditional_NotDBNull(result, ordinal, type);
                }
            }

            Expression resultWithErrorHandling = Emit_Shaper_GetColumnValueWithErrorHandling(arg.RequestedType, ordinal, columnType);
            _currentCoordinatorScratchpad.AddExpressionWithErrorHandling(result, resultWithErrorHandling);
            return new TranslatorResult(result, type);
        }

        private static Expression Emit_Conditional_NotDBNull(Expression result, int ordinal, Type columnType)
        {
            result = Expression.Condition(Emit_Reader_IsDBNull(ordinal),
                                          Expression.Constant(TypeSystem.GetDefaultValue(columnType), columnType),
                                          result);
            return result;
        }

        internal static MethodInfo GetReaderMethod(Type type, out bool isNullable)
        {
            Debug.Assert(null != type, "type required");

            MethodInfo result;
            isNullable = false;

            // determine if this is a Nullable<T>
            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (null != underlyingType)
            {
                isNullable = true;
                type = underlyingType;
            }
            
            TypeCode typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.String:
                    result = DbDataReader_GetString;
                    isNullable = true;
                    break;
                case TypeCode.Int16:
                    result = DbDataReader_GetInt16;
                    break;
                case TypeCode.Int32:
                    result = DbDataReader_GetInt32;
                    break;
                case TypeCode.Int64:
                    result = DbDataReader_GetInt64;
                    break;
                case TypeCode.Boolean:
                    result = DbDataReader_GetBoolean;
                    break;
                case TypeCode.Decimal:
                    result = DbDataReader_GetDecimal;
                    break;
                case TypeCode.Double:
                    result = DbDataReader_GetDouble;
                    break;
                case TypeCode.Single:
                    result = DbDataReader_GetFloat;
                    break;
                case TypeCode.DateTime:
                    result = DbDataReader_GetDateTime;
                    break;
                case TypeCode.Byte:
                    result = DbDataReader_GetByte;
                    break;
                default:
                    if (typeof(Guid) == type)
                    {
                        // Guid doesn't have a type code
                        result = DbDataReader_GetGuid;
                    }
                    else if (typeof(TimeSpan) == type ||
                             typeof(DateTimeOffset) == type)
                    {
                        // TimeSpan and DateTimeOffset don't have a type code or a specific
                        // GetXXX method
                        result = DbDataReader_GetValue;
                    }
                    else if (typeof(Object) == type)
                    {
                        // We assume that Object means we want DBNull rather than null. I believe this is a bug.
                        result = DbDataReader_GetValue;
                    }
                    else
                    {
                        result = DbDataReader_GetValue;
                        isNullable = true;
                    }
                    break;
            }
            return result;
        }

        /// <summary>
        /// Visit(VarRefColumnMap)
        /// 
        /// This should throw; VarRefColumnMaps should be removed by the PlanCompiler.
        /// </summary>
        internal override TranslatorResult Visit(VarRefColumnMap columnMap, TranslatorArg arg)
        {
            Debug.Fail("VarRefColumnMap should be substituted at this point");
            throw EntityUtil.InvalidOperation(String.Empty);
        }

        #endregion

        #endregion
    }
}
