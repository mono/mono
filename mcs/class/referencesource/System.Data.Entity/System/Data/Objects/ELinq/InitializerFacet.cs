//---------------------------------------------------------------------
// <copyright file="InitializerFacet.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
//---------------------------------------------------------------------

namespace System.Data.Objects.ELinq
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Common.Internal.Materialization;
    using System.Data.Metadata.Edm;
    using System.Data.Objects.DataClasses;
    using System.Data.Objects.Internal;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    /// <summary>
    /// Facet encapsulating information necessary to initialize a LINQ projection
    /// result.
    /// </summary>
    internal abstract class InitializerMetadata : IEquatable<InitializerMetadata>
    {
        internal readonly Type ClrType;
        internal static readonly MethodInfo UserExpressionMarker = typeof(InitializerMetadata).GetMethod("MarkAsUserExpression", BindingFlags.NonPublic | BindingFlags.Static);
        private static long s_identifier;
        internal readonly string Identity;
        private static readonly string s_identifierPrefix = typeof(InitializerMetadata).Name;

        private InitializerMetadata(Type clrType)
        {
            Debug.Assert(null != clrType);
            ClrType = clrType;
            Identity = s_identifierPrefix + Interlocked.Increment(ref s_identifier).ToString(CultureInfo.InvariantCulture);
        }

        // Gets the kind of this initializer (grouping, row, etc.)
        internal abstract InitializerMetadataKind Kind { get; }

        // Attempts to retrieve the initializer facet from a type usage
        internal static bool TryGetInitializerMetadata(TypeUsage typeUsage, out InitializerMetadata initializerMetadata)
        {
            initializerMetadata = null;
            if (BuiltInTypeKind.RowType == typeUsage.EdmType.BuiltInTypeKind)
            {
                initializerMetadata = ((RowType)typeUsage.EdmType).InitializerMetadata;
            }
            return null != initializerMetadata;
        }

        // Initializes an initializer for an IGrouping return type
        // Requires: resultType is IGrouping<T, K> instance.
        internal static InitializerMetadata CreateGroupingInitializer(EdmItemCollection itemCollection, Type resultType)
        {
            return itemCollection.GetCanonicalInitializerMetadata(new GroupingInitializerMetadata(resultType));
        }

        // Initializes an initializer for a MemberInit expression
        internal static InitializerMetadata CreateProjectionInitializer(EdmItemCollection itemCollection, MemberInitExpression initExpression,
            MemberInfo[] members)
        {
            return itemCollection.GetCanonicalInitializerMetadata(new ProjectionInitializerMetadata(initExpression, members));
        }

        // Initializes an initializer for a New expression
        internal static InitializerMetadata CreateProjectionInitializer(EdmItemCollection itemCollection, NewExpression newExpression)
        {
            return itemCollection.GetCanonicalInitializerMetadata(new ProjectionNewMetadata(newExpression));
        }

        // Initializes an initializer for a New expression with no properties
        internal static InitializerMetadata CreateEmptyProjectionInitializer(EdmItemCollection itemCollection, NewExpression newExpression)
        {
            return itemCollection.GetCanonicalInitializerMetadata(new EmptyProjectionNewMetadata(newExpression));
        }

        // Creates metadata for entity collection materialization
        internal static InitializerMetadata CreateEntityCollectionInitializer(EdmItemCollection itemCollection, Type type, NavigationProperty navigationProperty)
        {
            return itemCollection.GetCanonicalInitializerMetadata(new EntityCollectionInitializerMetadata(type, navigationProperty));
        }

        private static T MarkAsUserExpression<T>(T value)
        {
            // No op. This is used as a marker inside of an expression tree to indicate
            // that the input expression is not trusted.
            return value;
        }

        internal virtual void AppendColumnMapKey(ColumnMapKeyBuilder builder)
        {
            // by default, the type is sufficient (more information is needed for EntityCollection and initializers)
            builder.Append("CLR-", this.ClrType);
        }

        public override bool Equals(object obj)
        {
            Debug.Fail("use typed Equals method only");
            return Equals(obj as InitializerMetadata);
        }

        public bool Equals(InitializerMetadata other)
        {
            Debug.Assert(null != other, "must not use a null key");
            if (object.ReferenceEquals(this, other)) { return true; }
            if (this.Kind != other.Kind) { return false; }
            if (!this.ClrType.Equals(other.ClrType)) { return false; }
            return IsStructurallyEquivalent(other);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2303", Justification="ClrType is not expected to be an Embedded Interop Type.")]
        public override int GetHashCode()
        {
            return ClrType.GetHashCode();
        }

        /// <summary>
        /// Requires: other has the same type as this and refers to the same CLR type
        /// Determine whether this Metadata is compatible with the other based on record layout.
        /// </summary>
        protected virtual bool IsStructurallyEquivalent(InitializerMetadata other)
        {
            return true;
        }
        
        /// <summary>
        /// Produces an expression initializing an instance of ClrType (given emitters for input
        /// columns)
        /// </summary>
        internal abstract Expression Emit(Translator translator, List<TranslatorResult> propertyTranslatorResults);

        /// <summary>
        /// Yields expected types for input columns. Null values are returned for children
        /// whose type is irrelevant to the initializer.
        /// </summary>
        internal abstract IEnumerable<Type> GetChildTypes();

        /// <summary>
        /// return a list of propertyReader expressions from an array of translator results.
        /// </summary>
        /// <param name="propertyTranslatorResults"></param>
        /// <returns></returns>
        protected static List<Expression> GetPropertyReaders(List<TranslatorResult> propertyTranslatorResults)
        {
            List<Expression> propertyReaders = propertyTranslatorResults.Select(s => s.UnwrappedExpression).ToList();
            return propertyReaders;
        }

        /// <summary>
        /// Implementation of IGrouping that can be initialized using the standard
        /// initializer pattern supported by ELinq
        /// </summary>
        /// <typeparam name="K">Type of key</typeparam>
        /// <typeparam name="T">Type of record</typeparam>
        private class Grouping<K, T> : IGrouping<K, T>
        {
            public Grouping(K key, IEnumerable<T> group)
            {
                _key = key;
                _group = group;
            }

            private readonly K _key;
            private readonly IEnumerable<T> _group;

            public K Key
            {
                get { return _key; }
            }

            public IEnumerable<T> Group
            {
                get { return _group; }
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                if (null == _group)
                {
                    yield break;
                }
                foreach (T member in _group)
                {
                    yield return member;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<T>)this).GetEnumerator();
            }
        }
        
        /// <summary>
        /// Metadata for grouping initializer.
        /// </summary>
        private class GroupingInitializerMetadata : InitializerMetadata
        {
            internal GroupingInitializerMetadata(Type type)
                : base(type)
            {
            }

            internal override InitializerMetadataKind Kind { get { return InitializerMetadataKind.Grouping; } }

            internal override Expression Emit(Translator translator, List<TranslatorResult> propertyTranslatorResults)
            {
                // Create expression of the form:
                // new Grouping<K, T>(children[0], children[1])

                // Collect information...
                Debug.Assert(ClrType.IsGenericType &&
                    typeof(IGrouping<,>).Equals(ClrType.GetGenericTypeDefinition()));
                Debug.Assert(propertyTranslatorResults.Count == 2);
                Type keyType = this.ClrType.GetGenericArguments()[0];
                Type groupElementType = this.ClrType.GetGenericArguments()[1];
                Type groupType = typeof(Grouping<,>).MakeGenericType(keyType, groupElementType);
                ConstructorInfo constructor = groupType.GetConstructors().Single();

                // new Grouping<K, T>(children[0], children[1])
                Expression newGrouping = Expression.Convert(Expression.New(constructor, GetPropertyReaders(propertyTranslatorResults)), this.ClrType);

                return newGrouping;
            }

            internal override IEnumerable<Type> GetChildTypes()
            {
                // Collect information...
                Debug.Assert(ClrType.IsGenericType &&
                    typeof(IGrouping<,>).Equals(ClrType.GetGenericTypeDefinition()));
                Type keyType = this.ClrType.GetGenericArguments()[0];
                Type groupElementType = this.ClrType.GetGenericArguments()[1];

                // key
                yield return keyType;
                // group
                yield return typeof(IEnumerable<>).MakeGenericType(groupElementType);
            }
        }

        /// <summary>
        /// Metadata for anonymous type materialization.
        /// </summary>
        private class ProjectionNewMetadata : InitializerMetadata
        {
            internal ProjectionNewMetadata(NewExpression newExpression)
                : base(newExpression.Type)
            {
                Debug.Assert(null != newExpression);
                _newExpression = newExpression;
            }

            private readonly NewExpression _newExpression;

            internal override InitializerMetadataKind Kind { get { return InitializerMetadataKind.ProjectionNew; } }

            protected override bool IsStructurallyEquivalent(InitializerMetadata other)
            {                
                // caller must ensure the type matches                
                ProjectionNewMetadata otherProjection = (ProjectionNewMetadata)other;
                if (this._newExpression.Members == null && otherProjection._newExpression.Members == null)
                {
                    return true;
                }

                if (this._newExpression.Members == null || otherProjection._newExpression.Members == null)
                {
                    return false;
                }

                if (this._newExpression.Members.Count != otherProjection._newExpression.Members.Count)
                {
                    return false;
                }

                for (int i = 0; i < this._newExpression.Members.Count; i++)
                {
                    MemberInfo thisMember = this._newExpression.Members[i];
                    MemberInfo otherMember = otherProjection._newExpression.Members[i];
                    if (!thisMember.Equals(otherMember))
                    {
                        return false;
                    }
                }

                return true;
            }

            internal override Expression Emit(Translator translator, List<TranslatorResult> propertyTranslatorResults)
            {
                // Create expression of the form:
                // _newExpression(children)

                // (ClrType)null
                Expression nullProjection = Expression.Constant(null, this.ClrType);

                // _newExpression with members rebound
                Expression newProjection = Expression.New(_newExpression.Constructor, GetPropertyReaders(propertyTranslatorResults));

                // Indicate that this expression is provided by the user and should not be trusted.
                return Expression.Call(UserExpressionMarker.MakeGenericMethod(newProjection.Type), newProjection);
            }

            internal override IEnumerable<Type> GetChildTypes()
            {
                // return all argument types
                return _newExpression.Arguments.Select(arg => arg.Type);
            }

            internal override void AppendColumnMapKey(ColumnMapKeyBuilder builder)
            {
                base.AppendColumnMapKey(builder);
                builder.Append(_newExpression.Constructor.ToString());
                foreach (var member in _newExpression.Members ?? Enumerable.Empty<MemberInfo>())
                {
                    builder.Append("DT", member.DeclaringType);
                    builder.Append("." + member.Name);
                }
            }
        }

        private class EmptyProjectionNewMetadata : ProjectionNewMetadata
        {
            internal EmptyProjectionNewMetadata(NewExpression newExpression)
                : base(newExpression)
            {
            }
            internal override Expression Emit(Translator translator, List<TranslatorResult> propertyReaders)
            {
                // ignore sentinel column
                return base.Emit(translator, new List<TranslatorResult>());
            }
            internal override IEnumerable<Type> GetChildTypes()
            {
                // ignore sentinel column
                yield return null;
            }
        }

        /// <summary>
        /// Metadata for standard projection initializers.
        /// </summary>
        private class ProjectionInitializerMetadata : InitializerMetadata
        {
            internal ProjectionInitializerMetadata(MemberInitExpression initExpression, MemberInfo[] members)
                : base(initExpression.Type)
            {
                Debug.Assert(null != initExpression);
                Debug.Assert(null != members);
                _initExpression = initExpression;
                _members = members;
            }

            private readonly MemberInitExpression _initExpression;
            private readonly MemberInfo[] _members;

            internal override InitializerMetadataKind Kind { get { return InitializerMetadataKind.ProjectionInitializer; } }

            protected override bool IsStructurallyEquivalent(InitializerMetadata other)
            {
                // caller must ensure the type matches
                ProjectionInitializerMetadata otherProjection = (ProjectionInitializerMetadata)other;
                if (this._initExpression.Bindings.Count != otherProjection._initExpression.Bindings.Count)
                {
                    return false;
                }

                for (int i = 0; i < this._initExpression.Bindings.Count; i++)
                {
                    MemberBinding thisBinding = this._initExpression.Bindings[i];
                    MemberBinding otherBinding = otherProjection._initExpression.Bindings[i];
                    if (!thisBinding.Member.Equals(otherBinding.Member))
                    {
                        return false;
                    }
                }

                return true;
            }

            internal override Expression Emit(Translator translator, List<TranslatorResult> propertyReaders)
            {
                // Create expression of the form:
                // _initExpression(children)

                // create member bindings (where values are taken from children)
                MemberBinding[] memberBindings = new MemberBinding[_initExpression.Bindings.Count];
                MemberBinding[] constantMemberBindings = new MemberBinding[memberBindings.Length];
                for (int i = 0; i < memberBindings.Length; i++)
                {
                    MemberBinding originalBinding = _initExpression.Bindings[i];
                    Expression value = propertyReaders[i].UnwrappedExpression;
                    MemberBinding newBinding = Expression.Bind(originalBinding.Member, value);
                    MemberBinding constantBinding = Expression.Bind(originalBinding.Member, Expression.Constant(
                        TypeSystem.GetDefaultValue(value.Type), value.Type));
                    memberBindings[i] = newBinding;
                    constantMemberBindings[i] = constantBinding;
                }

                Expression newProjection = Expression.MemberInit(_initExpression.NewExpression, memberBindings);

                // Indicate that this expression is provided by the user and should not be trusted.
                return Expression.Call(UserExpressionMarker.MakeGenericMethod(newProjection.Type), newProjection);
            }

            internal override IEnumerable<Type> GetChildTypes()
            {
                // return all argument types
                foreach (var binding in _initExpression.Bindings)
                {
                    // determine member type
                    Type memberType;
                    string name;
                    TypeSystem.PropertyOrField(binding.Member, out name, out memberType);
                    yield return memberType;
                }
            }

            internal override void AppendColumnMapKey(ColumnMapKeyBuilder builder)
            {
                base.AppendColumnMapKey(builder);
                foreach (var binding in _initExpression.Bindings)
                {
                    builder.Append(",", binding.Member.DeclaringType);
                    builder.Append("." + binding.Member.Name);
                }
            }
        }

        /// <summary>
        /// Metadata for entity collection initializer.
        /// </summary>
        private class EntityCollectionInitializerMetadata : InitializerMetadata
        {
            internal EntityCollectionInitializerMetadata(Type type, NavigationProperty navigationProperty)
                : base(type)
            {
                Debug.Assert(null != navigationProperty);
                _navigationProperty = navigationProperty;
            }

            private readonly NavigationProperty _navigationProperty;

            internal override InitializerMetadataKind Kind { get { return InitializerMetadataKind.EntityCollection; } }

            /// <summary>
            /// Make sure the other metadata instance generates the same property
            /// (otherwise, we get incorrect behavior where multiple nav props return
            /// the same type)
            /// </summary>
            protected override bool IsStructurallyEquivalent(InitializerMetadata other)
            {
                // caller must ensure the type matches
                EntityCollectionInitializerMetadata otherInitializer = (EntityCollectionInitializerMetadata)other;
                return this._navigationProperty.Equals(otherInitializer._navigationProperty);
            }

            private static readonly MethodInfo s_createEntityCollectionMethod = typeof(EntityCollectionInitializerMetadata).GetMethod("CreateEntityCollection",
                BindingFlags.Static | BindingFlags.Public);

            internal override Expression Emit(Translator translator, List<TranslatorResult> propertyTranslatorResults)
            {
                Debug.Assert(propertyTranslatorResults.Count > 1, "no properties?");
                Debug.Assert(propertyTranslatorResults[1] is CollectionTranslatorResult, "not a collection?");

                Type elementType = GetElementType();
                MethodInfo createEntityCollectionMethod = s_createEntityCollectionMethod.MakeGenericMethod(elementType);

                Expression shaper = Translator.Shaper_Parameter;
                Expression owner = propertyTranslatorResults[0].Expression;

                CollectionTranslatorResult collectionResult = propertyTranslatorResults[1] as CollectionTranslatorResult;

                Expression coordinator = collectionResult.ExpressionToGetCoordinator;

                // CreateEntityCollection(shaper, owner, elements, relationshipName, targetRoleName)
                Expression result = Expression.Call(createEntityCollectionMethod,
                    shaper, owner, coordinator, Expression.Constant(_navigationProperty.RelationshipType.FullName), Expression.Constant(_navigationProperty.ToEndMember.Name));

                return result;
            }

            public static EntityCollection<T> CreateEntityCollection<T>(Shaper state, IEntityWrapper wrappedOwner, Coordinator<T> coordinator, string relationshipName, string targetRoleName)
                where T : class
            {
                if (null == wrappedOwner.Entity)
                {
                    return null;
                }
                else
                {
                    EntityCollection<T> result = wrappedOwner.RelationshipManager.GetRelatedCollection<T>(relationshipName, targetRoleName);
                    // register a handler for deferred loading (when the nested result has been consumed)
                    coordinator.RegisterCloseHandler((readerState, elements) => result.Load(elements, readerState.MergeOption));
                    return result;
                }
            }

            internal override IEnumerable<Type> GetChildTypes()
            {
                Type elementType = GetElementType();
                yield return null; // defer in determining entity type...
                yield return typeof(IEnumerable<>).MakeGenericType(elementType);
            }


            internal override void AppendColumnMapKey(ColumnMapKeyBuilder builder)
            {
                base.AppendColumnMapKey(builder);
                builder.Append(",NP" + _navigationProperty.Name);
                builder.Append(",AT", _navigationProperty.DeclaringType);
            }

            private Type GetElementType()
            {
                // POCO support requires that we allow ICollection<T> collections.  This allows a POCO collection
                // to be projected in a LINQ query.
                Type elementType;
                if (!EntityUtil.TryGetICollectionElementType(ClrType, out elementType))
                {
                    throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.ELinq_UnexpectedTypeForNavigationProperty(
                        _navigationProperty,
                        typeof(EntityCollection<>), typeof(ICollection<>),
                        ClrType));
                }
                return elementType;
            }            
        }
    }

    internal enum InitializerMetadataKind
    {
        Grouping,
        ProjectionNew,
        ProjectionInitializer,
        EntityCollection,
    }
}
