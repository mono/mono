//---------------------------------------------------------------------
// <copyright file="Translator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....], [....]
//---------------------------------------------------------------------

namespace System.Data.Objects.ELinq
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Data.Objects.DataClasses;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal sealed partial class ExpressionConverter
    {
        // Base class supporting the translation of LINQ node type(s) given a LINQ expression
        // of that type, and the "parent" translation context (the ExpressionConverter processor)
        private abstract class Translator
        {
            private readonly ExpressionType[] _nodeTypes;
            protected Translator(params ExpressionType[] nodeTypes)
            {
                _nodeTypes = nodeTypes;
            }
            // Gets LINQ node types this translator should be registed to process.
            internal IEnumerable<ExpressionType> NodeTypes { get { return _nodeTypes; } }
            internal abstract DbExpression Translate(ExpressionConverter parent, Expression linq);
            public override string ToString()
            {
                return this.GetType().Name;
            }
        }

        #region Misc
        // Typed version of Translator
        private abstract class TypedTranslator<T_Linq> : Translator
            where T_Linq : Expression
        {
            protected TypedTranslator(params ExpressionType[] nodeTypes)
                : base(nodeTypes) { }
            internal override DbExpression Translate(ExpressionConverter parent, Expression linq)
            {
                return TypedTranslate(parent, (T_Linq)linq);
            }
            protected abstract DbExpression TypedTranslate(ExpressionConverter parent, T_Linq linq);
        }
        private sealed class ConstantTranslator
            : TypedTranslator<System.Linq.Expressions.ConstantExpression>
        {
            internal ConstantTranslator()
                : base(ExpressionType.Constant) { }
            protected override DbExpression TypedTranslate(ExpressionConverter parent, ConstantExpression linq)
            {
                // Check to see if this constant corresponds to the compiled query context parameter (it
                // gets turned into a constant during funcletization and has special error handling).
                if (linq == parent._funcletizer.RootContextExpression)
                {
                    throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.ELinq_UnsupportedUseOfContextParameter(
                        parent._funcletizer.RootContextParameter.Name));
                }

                ObjectQuery queryOfT = linq.Value as ObjectQuery;
                if (null != queryOfT)
                {
                    return parent.TranslateInlineQueryOfT(queryOfT);
                }

                // If it is something we can enumerate then we can evaluate locally and send to the server
                var values = linq.Value as System.Collections.IEnumerable;
                if (values != null)
                {
                    Type elementType = TypeSystem.GetElementType(linq.Type);
                    if ((elementType != null) && (elementType != linq.Type))
                    {
                        var expressions = new List<Expression>();
                        foreach (object o in values)
                        {
                            expressions.Add(Expression.Constant(o, elementType));
                        }

                        // Invalidate the query plan every time the query is executed since it is possible
                        // to modify an element of a collection without changing the reference.
                        parent._recompileRequired = () => true;

                        return parent.TranslateExpression(Expression.NewArrayInit(elementType, expressions));
                    }
                }

                bool isNullValue = null == linq.Value;

                // Remove facet information: null instances do not constrain type facets (e.g. a null string does not restrict
                // "length" in compatibility checks)
                TypeUsage type;
                bool typeSupported = false;
                if (parent.TryGetValueLayerType(linq.Type, out type))
                {
                    // For constant values, support only primitive and enum type (this is all that is supported by CQTs)
                    // For null types, also allow EntityType. Although other types claim to be supported, they
                    // don't work (e.g. complex type, see SQL BU 543956)
                    if (Helper.IsScalarType(type.EdmType) ||
                        (isNullValue && Helper.IsEntityType(type.EdmType)))
                    {
                        typeSupported = true;
                    }
                }

                if (!typeSupported)
                {
                    if (isNullValue)
                    {
                        throw EntityUtil.NotSupported(Strings.ELinq_UnsupportedNullConstant(DescribeClrType(linq.Type)));
                    }
                    else
                    {
                        throw EntityUtil.NotSupported(Strings.ELinq_UnsupportedConstant(DescribeClrType(linq.Type)));
                    }
                }

                // create a constant or null expression depending on value
                if (isNullValue)
                {
                    return DbExpressionBuilder.Null(type);
                }
                else
                {
                    // By default use the value specified in the ConstantExpression.Value property. However,
                    // if the value was of an enum type that is not in the model its type was converted
                    // to the EdmType type corresponding to the underlying type of the enum type. In this case
                    // we also need to cast the value to the same type to avoid mismatches.
                    var value = linq.Value;
                    if (Helper.IsPrimitiveType(type.EdmType))
                    {
                        var nonNullableLinqType = TypeSystem.GetNonNullableType(linq.Type);
                        if (nonNullableLinqType.IsEnum)
                        {
                            value = System.Convert.ChangeType(linq.Value, nonNullableLinqType.GetEnumUnderlyingType(), CultureInfo.InvariantCulture);
                        }
                    }

                    return DbExpressionBuilder.Constant(type, value);
                }
            }
        }
        private sealed partial class MemberAccessTranslator
            : TypedTranslator<MemberExpression>
        {
            internal MemberAccessTranslator()
                : base(ExpressionType.MemberAccess) { }
            // attempt to translate the member access to a "regular" property, a navigation property, or a calculated
            // property
            protected override DbExpression TypedTranslate(ExpressionConverter parent, MemberExpression linq)
            {
                DbExpression propertyExpression;
                string memberName;
                Type memberType;
                MemberInfo memberInfo = TypeSystem.PropertyOrField(linq.Member, out memberName, out memberType);

                // note: we check for "regular" properties last, since the other two flavors derive
                // from this one
                if (linq.Expression != null)
                {
                    DbExpression instance = parent.TranslateExpression(linq.Expression);
                    if (TryResolveAsProperty(parent, memberInfo,
                        instance.ResultType, instance, out propertyExpression))
                    {
                        return propertyExpression;
                    }
                }

                if (memberInfo.MemberType == MemberTypes.Property)
                {
                    // Check whether it is one of the special properties that we know how to translate
                    PropertyTranslator propertyTranslator;
                    if (TryGetTranslator((PropertyInfo)memberInfo, out propertyTranslator))
                    {
                        return propertyTranslator.Translate(parent, linq);
                    }
                }

                // no other property types are supported by LINQ over entities
                throw EntityUtil.NotSupported(System.Data.Entity.Strings.ELinq_UnrecognizedMember(linq.Member.Name));
            }

            #region Static members and initializers
            private static readonly Dictionary<PropertyInfo, PropertyTranslator> s_propertyTranslators;
            private static bool s_vbPropertiesInitialized;
            private static readonly object s_vbInitializerLock = new object();

            static MemberAccessTranslator()
            {
                // initialize translators for specific properties
                s_propertyTranslators = new Dictionary<PropertyInfo, PropertyTranslator>();
                foreach (PropertyTranslator translator in GetPropertyTranslators())
                {
                    foreach (PropertyInfo property in translator.Properties)
                    {
                        s_propertyTranslators.Add(property, translator);
                    }
                }
            }

            /// <summary>
            /// Tries to get a translator for the given property info.  
            /// If the given property info corresponds to a Visual Basic property, 
            /// it also initializes the Visual Basic translators if they have not been initialized
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="propertyTranslator"></param>
            /// <returns></returns>
            private static bool TryGetTranslator(PropertyInfo propertyInfo, out PropertyTranslator propertyTranslator)
            {
                //If the type is generic, we try to match the generic property
                PropertyInfo nonGenericPropertyInfo = propertyInfo;
                if (propertyInfo.DeclaringType.IsGenericType)
                {
                    try
                    {
                        propertyInfo = propertyInfo.DeclaringType.GetGenericTypeDefinition().GetProperty(propertyInfo.Name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
                    }
                    catch (AmbiguousMatchException)
                    {
                        propertyTranslator = null;
                        return false;
                    }                   
                    if (propertyInfo == null)
                    {
                        propertyTranslator = null;
                        return false;
                    }
                }

                PropertyTranslator translatorInstance;
                if (s_propertyTranslators.TryGetValue(propertyInfo, out translatorInstance))
                {
                    propertyTranslator = translatorInstance;
                    return true;
                }

                // check if this is the visual basic assembly
                if (s_visualBasicAssemblyFullName == propertyInfo.DeclaringType.Assembly.FullName)
                {
                    lock (s_vbInitializerLock)
                    {
                        if (!s_vbPropertiesInitialized)
                        {
                            InitializeVBProperties(propertyInfo.DeclaringType.Assembly);
                            s_vbPropertiesInitialized = true;
                        }
                        // try again
                        if (s_propertyTranslators.TryGetValue(propertyInfo, out translatorInstance))
                        {
                            propertyTranslator = translatorInstance;
                            return true;
                        }
                        else
                        {
                            propertyTranslator = null;
                            return false;
                        }
                    }
                }

                if (GenericICollectionTranslator.TryGetPropertyTranslator(nonGenericPropertyInfo, out propertyTranslator))
                {
                    return true;
                }

                propertyTranslator = null;
                return false;
            }

            // Determines if the given property can be resolved as a standard or navigation property.
            private static bool TryResolveAsProperty(ExpressionConverter parent,
                MemberInfo clrMember, TypeUsage definingType, DbExpression instance, out DbExpression propertyExpression)
            {
                // retrieve members directly from row types, which are not mapped between O and C
                RowType rowType = definingType.EdmType as RowType;
                string name = clrMember.Name;

                if (null != rowType)
                {
                    EdmMember member;
                    if (rowType.Members.TryGetValue(name, false, out member))
                    {
                        propertyExpression = instance.Property(name);
                        return true;
                    }

                    propertyExpression = null;
                    return false;
                }

                // for non-row structural types, map from the O to the C layer using the perspective
                StructuralType structuralType = definingType.EdmType as StructuralType;
                if (null != structuralType)
                {
                    EdmMember member = null;
                    if (parent._perspective.TryGetMember(structuralType, name, false, out member))
                    {
                        if (null != member)
                        {
                            if (member.BuiltInTypeKind == BuiltInTypeKind.NavigationProperty)
                            {
                                NavigationProperty navProp = (NavigationProperty)member;
                                propertyExpression = TranslateNavigationProperty(parent, clrMember, instance, navProp);
                                return true;
                            }
                            else
                            {
                                propertyExpression = instance.Property(name);
                                return true;
                            }

                        }
                    }
                }

                // try to unwrap GroupBy "Key" member
                if (name == ExpressionConverter.KeyColumnName)
                {
                    // see if we can "unwrap" the current instance
                    if (DbExpressionKind.Property == instance.ExpressionKind)
                    {
                        DbPropertyExpression property = (DbPropertyExpression)instance;
                        InitializerMetadata initializerMetadata;

                        // if we're dealing with the "Group" property of a GroupBy projection, we know how to unwrap
                        // it
                        if (property.Property.Name == ExpressionConverter.GroupColumnName && // only know how to unwrap the group
                            InitializerMetadata.TryGetInitializerMetadata(property.Instance.ResultType, out initializerMetadata) &&
                            initializerMetadata.Kind == InitializerMetadataKind.Grouping)
                        {
                            propertyExpression = property.Instance.Property(ExpressionConverter.KeyColumnName);
                            return true;
                        }
                    }
                }

                propertyExpression = null;
                return false;
            }
                        
            private static DbExpression TranslateNavigationProperty(ExpressionConverter parent, MemberInfo clrMember, DbExpression instance, NavigationProperty navProp)
            {
                DbExpression propertyExpression;
                propertyExpression = instance.Property(navProp);

                // for EntityCollection navigations, wrap in "grouping" where the key is the parent
                // entity and the group contains the child entities
                // For non-EntityCollection navigations (e.g. from POCO entities), we just need the
                // enumeration, not the grouping
                if (BuiltInTypeKind.CollectionType == propertyExpression.ResultType.EdmType.BuiltInTypeKind)
                {
                    Debug.Assert(clrMember is PropertyInfo, "Navigation property was not a property; should not be allowed by metadata.");
                    Type propertyType = ((PropertyInfo)clrMember).PropertyType;
                    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(EntityCollection<>))
                    {
                        List<KeyValuePair<string, DbExpression>> collectionColumns =
                            new List<KeyValuePair<string, DbExpression>>(2);
                        collectionColumns.Add(new KeyValuePair<string, DbExpression>(
                            ExpressionConverter.EntityCollectionOwnerColumnName, instance));
                        collectionColumns.Add(new KeyValuePair<string, DbExpression>(
                            ExpressionConverter.EntityCollectionElementsColumnName, propertyExpression));
                        propertyExpression = parent.CreateNewRowExpression(collectionColumns,
                            InitializerMetadata.CreateEntityCollectionInitializer(parent.EdmItemCollection, ((PropertyInfo)clrMember).PropertyType, navProp));
                    }
                }
                return propertyExpression;
            }

            private static DbExpression TranslateCount(ExpressionConverter parent, Type sequenceElementType, Expression sequence)
            {
                // retranslate as a Count() aggregate, since the name collision prevents us
                // from calling the method directly in VB and C#
                MethodInfo countMethod;
                ReflectionUtil.TryLookupMethod(SequenceMethod.Count, out countMethod);
                Debug.Assert(null != countMethod, "Count() must exist");
                countMethod = countMethod.MakeGenericMethod(sequenceElementType);
                Expression countCall = Expression.Call(countMethod, sequence);
                return parent.TranslateExpression(countCall);
            }

            private static void InitializeVBProperties(Assembly vbAssembly)
            {
                Debug.Assert(!s_vbPropertiesInitialized);
                foreach (PropertyTranslator translator in GetVisualBasicPropertyTranslators(vbAssembly))
                {
                    foreach (PropertyInfo property in translator.Properties)
                    {
                        s_propertyTranslators.Add(property, translator);
                    }
                }
            }

            private static IEnumerable<PropertyTranslator> GetVisualBasicPropertyTranslators(Assembly vbAssembly)
            {
                yield return new VBDateAndTimeNowTranslator(vbAssembly);
            }

            private static IEnumerable<PropertyTranslator> GetPropertyTranslators()
            {
                yield return new DefaultCanonicalFunctionPropertyTranslator();
                yield return new RenameCanonicalFunctionPropertyTranslator();
                yield return new EntityCollectionCountTranslator();
                yield return new NullableHasValueTranslator();
                yield return new NullableValueTranslator();
                yield return new SpatialPropertyTranslator();
            }

            /// <summary>
            /// This method is used to determine whether client side evaluation should be done, 
            /// if the property can be evaluated in the store, it is not being evaluated on the client
            /// </summary>
            internal static bool CanFuncletizePropertyInfo(PropertyInfo propertyInfo)
            {
                PropertyTranslator propertyTranslator;
                // In most cases, we only allow funcletization of properties that could not otherwise be
                // handled by the query pipeline. ICollection<>.Count is the one exception to the rule
                // (avoiding a breaking change)
                return GenericICollectionTranslator.TryGetPropertyTranslator(propertyInfo, out propertyTranslator) ||
                    !TryGetTranslator(propertyInfo, out propertyTranslator);
            }
            #endregion

            #region Dynamic Property Translators

            private sealed class GenericICollectionTranslator : PropertyTranslator
            {
                private readonly Type _elementType;

                private GenericICollectionTranslator(Type elementType) : base(Enumerable.Empty<PropertyInfo>())
                {
                    _elementType = elementType;
                }

                internal override DbExpression Translate(ExpressionConverter parent, MemberExpression call)
                {
                    return TranslateCount(parent, _elementType, call.Expression);
                }

                internal static bool TryGetPropertyTranslator(PropertyInfo propertyInfo, out PropertyTranslator propertyTranslator)
                {
                    // Implementation note: When adding support for additional properties, use less expensive checks
                    // such as property name and return type to test for a property defined by ICollection<T> first
                    // before calling the more expensive TypeSystem.FindICollection to test whether the declaring type
                    // of the property implements ICollection<T>.

                    //
                    // Int32 Count
                    //
                    if (propertyInfo.Name == "Count" &&
                        propertyInfo.PropertyType.Equals(typeof(int)))
                    {
                        foreach (KeyValuePair<Type, Type> implementedCollectionInfo in GetImplementedICollections(propertyInfo.DeclaringType))
                        {
                            Type implementedCollection = implementedCollectionInfo.Key;
                            Type elementType = implementedCollectionInfo.Value;

                            if (propertyInfo.IsImplementationOf(implementedCollection))
                            {
                                propertyTranslator = new GenericICollectionTranslator(elementType);
                                return true;
                            }
                        }
                    }

                    // Not a supported ICollection<T> property
                    propertyTranslator = null;
                    return false;
                }

                private static bool IsICollection(Type candidateType, out Type elementType)
                {
                    if (candidateType.IsGenericType &&
                        candidateType.GetGenericTypeDefinition().Equals(typeof(System.Collections.Generic.ICollection<>)))
                    {
                        elementType = candidateType.GetGenericArguments()[0];
                        return true;
                    }
                    elementType = null;
                    return false;
                }

                private static IEnumerable<KeyValuePair<Type, Type>> GetImplementedICollections(Type type)
                {
                    Type collectionElementType;
                    if (IsICollection(type, out collectionElementType))
                    {
                        yield return new KeyValuePair<Type, Type>(type, collectionElementType);
                    }
                    else
                    {
                        foreach (Type interfaceType in type.GetInterfaces())
                        {
                            if (IsICollection(interfaceType, out collectionElementType))
                            {
                                yield return new KeyValuePair<Type, Type>(interfaceType, collectionElementType);
                            }
                        }
                    }
                }
            }

            #endregion

            #region Signature-based Property Translators
            private abstract class PropertyTranslator
            {
                private readonly IEnumerable<PropertyInfo> _properties;
                protected PropertyTranslator(params PropertyInfo[] properties) { _properties = properties; }
                protected PropertyTranslator(IEnumerable<PropertyInfo> properties) { _properties = properties; }
                internal IEnumerable<PropertyInfo> Properties { get { return _properties; } }
                internal abstract DbExpression Translate(ExpressionConverter parent, MemberExpression call);
                public override string ToString()
                {
                    return GetType().Name;
                }
            }

            private sealed class DefaultCanonicalFunctionPropertyTranslator : PropertyTranslator
            {
                internal DefaultCanonicalFunctionPropertyTranslator()
                    : base(GetProperties()) { }

                private static IEnumerable<PropertyInfo> GetProperties()
                {
                    yield return typeof(String).GetProperty("Length", BindingFlags.Public | BindingFlags.Instance);
                    yield return typeof(DateTime).GetProperty("Year", BindingFlags.Public | BindingFlags.Instance);
                    yield return typeof(DateTime).GetProperty("Month", BindingFlags.Public | BindingFlags.Instance);
                    yield return typeof(DateTime).GetProperty("Day", BindingFlags.Public | BindingFlags.Instance);
                    yield return typeof(DateTime).GetProperty("Hour", BindingFlags.Public | BindingFlags.Instance);
                    yield return typeof(DateTime).GetProperty("Minute", BindingFlags.Public | BindingFlags.Instance);
                    yield return typeof(DateTime).GetProperty("Second", BindingFlags.Public | BindingFlags.Instance);
                    yield return typeof(DateTime).GetProperty("Millisecond", BindingFlags.Public | BindingFlags.Instance);

                    yield return typeof(DateTimeOffset).GetProperty("Year", BindingFlags.Public | BindingFlags.Instance);
                    yield return typeof(DateTimeOffset).GetProperty("Month", BindingFlags.Public | BindingFlags.Instance);
                    yield return typeof(DateTimeOffset).GetProperty("Day", BindingFlags.Public | BindingFlags.Instance);
                    yield return typeof(DateTimeOffset).GetProperty("Hour", BindingFlags.Public | BindingFlags.Instance);
                    yield return typeof(DateTimeOffset).GetProperty("Minute", BindingFlags.Public | BindingFlags.Instance);
                    yield return typeof(DateTimeOffset).GetProperty("Second", BindingFlags.Public | BindingFlags.Instance);
                    yield return typeof(DateTimeOffset).GetProperty("Millisecond", BindingFlags.Public | BindingFlags.Instance);
                }

                // Default translator for method calls into canonical functions.
                // Translation:
                //      object.PropertyName  -> PropertyName(object)
                internal override DbExpression Translate(ExpressionConverter parent, MemberExpression call)
                {
                    return parent.TranslateIntoCanonicalFunction(call.Member.Name, call, call.Expression);
                }
            }

            private sealed class RenameCanonicalFunctionPropertyTranslator : PropertyTranslator
            {
                private static readonly Dictionary<PropertyInfo, string> s_propertyRenameMap = new Dictionary<PropertyInfo, string>(2);

                internal RenameCanonicalFunctionPropertyTranslator()
                    : base(GetProperties()) { }

                private static IEnumerable<PropertyInfo> GetProperties()
                {
                    yield return GetProperty(typeof(DateTime), "Now", BindingFlags.Public | BindingFlags.Static, ExpressionConverter.CurrentDateTime);
                    yield return GetProperty(typeof(DateTime), "UtcNow", BindingFlags.Public | BindingFlags.Static, ExpressionConverter.CurrentUtcDateTime);
                    yield return GetProperty(typeof(DateTimeOffset), "Now", BindingFlags.Public | BindingFlags.Static, ExpressionConverter.CurrentDateTimeOffset);

                    yield return GetProperty(typeof(TimeSpan), "Hours", BindingFlags.Public | BindingFlags.Instance, ExpressionConverter.Hour);
                    yield return GetProperty(typeof(TimeSpan), "Minutes", BindingFlags.Public | BindingFlags.Instance, ExpressionConverter.Minute);
                    yield return GetProperty(typeof(TimeSpan), "Seconds", BindingFlags.Public | BindingFlags.Instance, ExpressionConverter.Second);
                    yield return GetProperty(typeof(TimeSpan), "Milliseconds", BindingFlags.Public | BindingFlags.Instance, ExpressionConverter.Millisecond);
                }

                private static PropertyInfo GetProperty(Type declaringType, string propertyName, BindingFlags bindingFlages, string canonicalFunctionName)
                {
                    PropertyInfo propertyInfo = declaringType.GetProperty(propertyName, bindingFlages);
                    s_propertyRenameMap.Add(propertyInfo, canonicalFunctionName);
                    return propertyInfo;
                }

                // Translator for static properties into canonical functions when there is a corresponding 
                // canonical function but with a differnet name
                // Translation:
                //      object.PropertyName  -> CanonicalFunctionName(object)
                //      Type.PropertyName  -> CanonicalFunctionName()
                internal override DbExpression Translate(ExpressionConverter parent, MemberExpression call)
                {
                    PropertyInfo property = (PropertyInfo)call.Member;
                    String canonicalFunctionName = s_propertyRenameMap[property];
                    DbExpression result;
                    if (call.Expression == null)
                    {
                        result = parent.TranslateIntoCanonicalFunction(canonicalFunctionName, call);
                    }
                    else
                    {
                        result = parent.TranslateIntoCanonicalFunction(canonicalFunctionName, call, call.Expression);
                    }
                    return result;
                }
            }

            private sealed class VBDateAndTimeNowTranslator : PropertyTranslator
            {
                private const string s_dateAndTimeTypeFullName = "Microsoft.VisualBasic.DateAndTime";

                internal VBDateAndTimeNowTranslator(Assembly vbAssembly)
                    : base(GetProperty(vbAssembly)) { }

                private static PropertyInfo GetProperty(Assembly vbAssembly)
                {
                    return vbAssembly.GetType(s_dateAndTimeTypeFullName).GetProperty("Now", BindingFlags.Public | BindingFlags.Static);
                }

                // Translation:
                //      Now -> GetDate()
                internal override DbExpression Translate(ExpressionConverter parent, MemberExpression call)
                {
                    return parent.TranslateIntoCanonicalFunction(ExpressionConverter.CurrentDateTime, call);
                }
            }

            private sealed class EntityCollectionCountTranslator : PropertyTranslator
            {
                internal EntityCollectionCountTranslator()
                    : base(GetProperty()) { }

                private static PropertyInfo GetProperty()
                {
                    return typeof(EntityCollection<>).GetProperty(ExpressionConverter.s_entityCollectionCountPropertyName, BindingFlags.Public | BindingFlags.Instance);
                }

                // Translation:
                //      EntityCollection<T>.Count -> Count()
                internal override DbExpression Translate(ExpressionConverter parent, MemberExpression call)
                {
                    // retranslate as a Count() aggregate, since the name collision prevents us
                    // from calling the method directly in VB and C#
                    return MemberAccessTranslator.TranslateCount(parent, call.Member.DeclaringType.GetGenericArguments()[0], call.Expression);
                }
            }

            private sealed class NullableHasValueTranslator : PropertyTranslator
            {
                internal NullableHasValueTranslator()
                    : base(GetProperty()) { }

                private static PropertyInfo GetProperty()
                {
                    return typeof(Nullable<>).GetProperty(ExpressionConverter.s_nullableHasValuePropertyName, BindingFlags.Public | BindingFlags.Instance);
                }

                // Translation:
                //      Nullable<T>.HasValue -> Not(IsNull(arg))
                internal override DbExpression Translate(ExpressionConverter parent, MemberExpression call)
                {
                    DbExpression argument = parent.TranslateExpression(call.Expression);
                    Debug.Assert(!TypeSemantics.IsCollectionType(argument.ResultType), "Did not expect collection type");
                    return parent.CreateIsNullExpression(argument, call.Expression.Type).Not();
                }
            }

            private sealed class NullableValueTranslator : PropertyTranslator
            {
                internal NullableValueTranslator()
                    : base(GetProperty()) { }

                private static PropertyInfo GetProperty()
                {
                    return typeof(Nullable<>).GetProperty(ExpressionConverter.s_nullableValuePropertyName, BindingFlags.Public | BindingFlags.Instance);
                }

                // Translation:
                //      Nullable<T>.Value -> arg
                internal override DbExpression Translate(ExpressionConverter parent, MemberExpression call)
                {
                    DbExpression argument = parent.TranslateExpression(call.Expression);
                    Debug.Assert(!TypeSemantics.IsCollectionType(argument.ResultType), "Did not expect collection type");
                    return argument;
                }
            }
            #endregion
        }
        private sealed class ParameterTranslator
            : TypedTranslator<ParameterExpression>
        {
            internal ParameterTranslator()
                : base(ExpressionType.Parameter) { }
            protected override DbExpression TypedTranslate(ExpressionConverter parent, ParameterExpression linq)
            {
                // Bindings should be intercepted before we get to this point (in ExpressionConverter.TranslateExpression)
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.ELinq_UnboundParameterExpression(linq.Name));
            }
        }
        private sealed class NewTranslator
            : TypedTranslator<NewExpression>
        {
            internal NewTranslator()
                : base(ExpressionType.New) { }
            protected override DbExpression TypedTranslate(ExpressionConverter parent, NewExpression linq)
            {
                int memberCount = null == linq.Members ? 0 : linq.Members.Count;

                if (null == linq.Constructor ||
                    linq.Arguments.Count != memberCount)
                {
                    throw EntityUtil.NotSupported(System.Data.Entity.Strings.ELinq_UnsupportedConstructor);
                }

                parent.CheckInitializerType(linq.Type);

                List<KeyValuePair<String, DbExpression>> recordColumns =
                    new List<KeyValuePair<string, DbExpression>>(memberCount + 1);

                HashSet<string> memberNames = new HashSet<string>(StringComparer.Ordinal);
                for (int i = 0; i < memberCount; i++)
                {
                    string memberName;
                    Type memberType;
                    MemberInfo memberInfo = TypeSystem.PropertyOrField(linq.Members[i], out memberName, out memberType);
                    DbExpression memberValue = parent.TranslateExpression(linq.Arguments[i]);
                    memberNames.Add(memberName);
                    recordColumns.Add(new KeyValuePair<string, DbExpression>(memberName, memberValue));
                }

                InitializerMetadata initializerMetadata;
                if (0 == memberCount)
                {
                    // add a sentinel column because CQTs do not accept empty row types
                    recordColumns.Add(DbExpressionBuilder.True.As(KeyColumnName));
                    initializerMetadata = InitializerMetadata.CreateEmptyProjectionInitializer(parent.EdmItemCollection, linq);
                }
                else
                {
                    // Construct a new initializer type in metadata for this projection (provides the
                    // necessary context for the object materializer)
                    initializerMetadata = InitializerMetadata.CreateProjectionInitializer(parent.EdmItemCollection, linq);
                }
                parent.ValidateInitializerMetadata(initializerMetadata);

                DbNewInstanceExpression projection = parent.CreateNewRowExpression(recordColumns, initializerMetadata);

                return projection;
            }
        }
        private sealed class NewArrayInitTranslator
            : TypedTranslator<NewArrayExpression>
        {
            internal NewArrayInitTranslator()
                : base(ExpressionType.NewArrayInit) { }
            protected override DbExpression TypedTranslate(ExpressionConverter parent, NewArrayExpression linq)
            {
                if (linq.Expressions.Count > 0)
                {
                    return DbExpressionBuilder.NewCollection(linq.Expressions.Select(e => parent.TranslateExpression(e)));
                }

                TypeUsage typeUsage;
                if (typeof(byte[]) == linq.Type)
                {
                    TypeUsage type;
                    if (parent.TryGetValueLayerType(typeof(byte), out type))
                    {
                        typeUsage = TypeHelpers.CreateCollectionTypeUsage(type);
                        return DbExpressionBuilder.NewEmptyCollection(typeUsage);
                    }
                }
                else
                {
                    if (parent.TryGetValueLayerType(linq.Type, out typeUsage))
                    {
                        return DbExpressionBuilder.NewEmptyCollection(typeUsage);
                    }
                }

                throw EntityUtil.NotSupported(Strings.ELinq_UnsupportedType(DescribeClrType(linq.Type)));
            }
        }
        private sealed class ListInitTranslator
            : TypedTranslator<ListInitExpression>
        {
            internal ListInitTranslator()
                : base(ExpressionType.ListInit ) { }
            protected override DbExpression TypedTranslate(ExpressionConverter parent, ListInitExpression linq)
            {
                // Ensure requirements: one list initializer argument and a default constructor.
                if ((linq.NewExpression.Constructor != null) && (linq.NewExpression.Constructor.GetParameters().Length != 0))
                {
                    throw EntityUtil.NotSupported(System.Data.Entity.Strings.ELinq_UnsupportedConstructor);
                }

                if (linq.Initializers.Any(i => i.Arguments.Count != 1))
                {
                    throw EntityUtil.NotSupported(System.Data.Entity.Strings.ELinq_UnsupportedInitializers);
                }

                return DbExpressionBuilder.NewCollection(linq.Initializers.Select(i => parent.TranslateExpression(i.Arguments[0])));
            }
        }
        private sealed class MemberInitTranslator
            : TypedTranslator<MemberInitExpression>
        {
            internal MemberInitTranslator()
                : base(ExpressionType.MemberInit) { }
            protected override DbExpression TypedTranslate(ExpressionConverter parent, MemberInitExpression linq)
            {
                if (null == linq.NewExpression.Constructor ||
                    0 != linq.NewExpression.Constructor.GetParameters().Length)
                {
                    throw EntityUtil.NotSupported(System.Data.Entity.Strings.ELinq_UnsupportedConstructor);
                }

                parent.CheckInitializerType(linq.Type);

                List<KeyValuePair<String, DbExpression>> recordColumns =
                    new List<KeyValuePair<string, DbExpression>>(linq.Bindings.Count + 1);
                MemberInfo[] members = new MemberInfo[linq.Bindings.Count];

                HashSet<string> memberNames = new HashSet<string>(StringComparer.Ordinal);
                for (int i = 0; i < linq.Bindings.Count; i++)
                {
                    MemberAssignment binding = linq.Bindings[i] as MemberAssignment;
                    if (null == binding)
                    {
                        throw EntityUtil.NotSupported(System.Data.Entity.Strings.ELinq_UnsupportedBinding);
                    }
                    string memberName;
                    Type memberType;
                    MemberInfo memberInfo = TypeSystem.PropertyOrField(binding.Member, out memberName, out memberType);

                    DbExpression memberValue = parent.TranslateExpression(binding.Expression);
                    memberNames.Add(memberName);
                    members[i] = memberInfo;
                    recordColumns.Add(new KeyValuePair<string, DbExpression>(memberName, memberValue));
                }

                InitializerMetadata initializerMetadata;

                if (0 == recordColumns.Count)
                {
                    // add a sentinel column because CQTs do not accept empty row types
                    recordColumns.Add(DbExpressionBuilder.Constant(true).As(KeyColumnName));
                    initializerMetadata = InitializerMetadata.CreateEmptyProjectionInitializer(parent.EdmItemCollection, linq.NewExpression);
                }
                else
                {
                    // Construct a new initializer type in metadata for this projection (provides the
                    // necessary context for the object materializer)
                    initializerMetadata = InitializerMetadata.CreateProjectionInitializer(parent.EdmItemCollection, linq, members);
                }
                parent.ValidateInitializerMetadata(initializerMetadata);
                DbNewInstanceExpression projection = parent.CreateNewRowExpression(recordColumns, initializerMetadata);

                return projection;
            }
        }
        private sealed class ConditionalTranslator : TypedTranslator<ConditionalExpression>
        {
            internal ConditionalTranslator()
                : base(ExpressionType.Conditional) { }
            protected override DbExpression TypedTranslate(ExpressionConverter parent, ConditionalExpression linq)
            {
                // translate Test ? IfTrue : IfFalse --> CASE WHEN Test THEN IfTrue ELSE IfFalse
                List<DbExpression> whenExpressions = new List<DbExpression>(1);
                whenExpressions.Add(parent.TranslateExpression(linq.Test));
                List<DbExpression> thenExpressions = new List<DbExpression>(1);
                thenExpressions.Add(parent.TranslateExpression(linq.IfTrue));
                DbExpression elseExpression = parent.TranslateExpression(linq.IfFalse);
                return DbExpressionBuilder.Case(whenExpressions, thenExpressions, elseExpression);
            }
        }
        private sealed class NotSupportedTranslator : Translator
        {
            internal NotSupportedTranslator(params ExpressionType[] nodeTypes)
                : base(nodeTypes)
            {
            }
            internal override DbExpression Translate(ExpressionConverter parent, Expression linq)
            {
                throw EntityUtil.NotSupported(System.Data.Entity.Strings.ELinq_UnsupportedExpressionType(linq.NodeType));
            }
        }
        private sealed class ExtensionTranslator : Translator
        {
            internal ExtensionTranslator()
                : base(EntityExpressionVisitor.CustomExpression)
            {
            }
            internal override DbExpression Translate(ExpressionConverter parent, Expression linq)
            {
                QueryParameterExpression queryParameter = linq as QueryParameterExpression;
                if (null == queryParameter)
                {
                    throw EntityUtil.NotSupported(System.Data.Entity.Strings.ELinq_UnsupportedExpressionType(linq.NodeType));
                }
                // otherwise add a new query parameter...
                parent.AddParameter(queryParameter);
                return queryParameter.ParameterReference;
            }
        }
        #endregion

        #region Binary expression translators
        private abstract class BinaryTranslator
            : TypedTranslator<System.Linq.Expressions.BinaryExpression>
        {
            protected BinaryTranslator(params ExpressionType[] nodeTypes)
                : base(nodeTypes) { }
            protected override DbExpression TypedTranslate(ExpressionConverter parent, System.Linq.Expressions.BinaryExpression linq)
            {
                return TranslateBinary(parent, parent.TranslateExpression(linq.Left), parent.TranslateExpression(linq.Right), linq);
            }
            protected abstract DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq);
        }
        private sealed class CoalesceTranslator : BinaryTranslator
        {
            internal CoalesceTranslator()
                : base(ExpressionType.Coalesce) { }
            protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
            {
                // left ?? right gets translated to:
                // CASE WHEN IsNull(left) THEN right ELSE left

                // construct IsNull
                DbExpression isNull = parent.CreateIsNullExpression(left, linq.Left.Type);

                // construct case expression
                List<DbExpression> whenExpressions = new List<DbExpression>(1);
                whenExpressions.Add(isNull);
                List<DbExpression> thenExpressions = new List<DbExpression>(1);
                thenExpressions.Add(right);
                DbExpression caseExpression = DbExpressionBuilder.Case(whenExpressions,
                    thenExpressions, left);

                return caseExpression;
            }
        }
        private sealed class AndAlsoTranslator : BinaryTranslator
        {
            internal AndAlsoTranslator()
                : base(ExpressionType.AndAlso) { }
            protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
            {
                return left.And(right);
            }
        }
        private sealed class OrElseTranslator : BinaryTranslator
        {
            internal OrElseTranslator()
                : base(ExpressionType.OrElse) { }
            protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
            {
                return left.Or(right);
            }
        }
        private sealed class LessThanTranslator : BinaryTranslator
        {
            internal LessThanTranslator()
                : base(ExpressionType.LessThan) { }
            protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
            {
                return left.LessThan(right);
            }
        }
        private sealed class LessThanOrEqualsTranslator : BinaryTranslator
        {
            internal LessThanOrEqualsTranslator()
                : base(ExpressionType.LessThanOrEqual) { }
            protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
            {
                return left.LessThanOrEqual(right);
            }
        }
        private sealed class GreaterThanTranslator : BinaryTranslator
        {
            internal GreaterThanTranslator()
                : base(ExpressionType.GreaterThan) { }
            protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
            {
                return left.GreaterThan(right);
            }
        }
        private sealed class GreaterThanOrEqualsTranslator : BinaryTranslator
        {
            internal GreaterThanOrEqualsTranslator()
                : base(ExpressionType.GreaterThanOrEqual) { }
            protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
            {
                return left.GreaterThanOrEqual(right);
            }
        }
        private sealed class EqualsTranslator : TypedTranslator<System.Linq.Expressions.BinaryExpression>
        {
            internal EqualsTranslator()
                : base(ExpressionType.Equal) { }
            protected override DbExpression TypedTranslate(ExpressionConverter parent, System.Linq.Expressions.BinaryExpression linq)
            {
                Expression linqLeft = linq.Left;
                Expression linqRight = linq.Right;

                bool leftIsNull = ExpressionIsNullConstant(linqLeft);
                bool rightIsNull = ExpressionIsNullConstant(linqRight);

                // if both values are null, short-circuit
                if (leftIsNull && rightIsNull) { return DbExpressionBuilder.True; }

                // if only one side is null, produce an IsNull statement
                if (leftIsNull) { return CreateIsNullExpression(parent, linqRight); }
                if (rightIsNull) { return CreateIsNullExpression(parent, linqLeft); }

                // create a standard equals expression, calling utility method to compensate for null equality
                DbExpression cqtLeft = parent.TranslateExpression(linqLeft);
                DbExpression cqtRight = parent.TranslateExpression(linqRight);
                EqualsPattern pattern = EqualsPattern.Store;
                if (parent._funcletizer.RootContext.ContextOptions.UseCSharpNullComparisonBehavior)
                {
                    pattern = EqualsPattern.PositiveNullEqualityComposable;
                }
                return parent.CreateEqualsExpression(cqtLeft, cqtRight, pattern, linqLeft.Type, linqRight.Type);
            }
            private static DbExpression CreateIsNullExpression(ExpressionConverter parent, Expression input)
            {
                input = UnwrapConvert(input);

                // translate input
                DbExpression inputCqt = parent.TranslateExpression(input);

                // create IsNull expression
                return parent.CreateIsNullExpression(inputCqt, input.Type);
            }
            private static bool ExpressionIsNullConstant(Expression expression)
            {
                // convert statements introduced by compiler should not affect nullness
                expression = UnwrapConvert(expression);

                // check if the unwrapped expression is a null constant
                if (ExpressionType.Constant != expression.NodeType) { return false; }
                System.Linq.Expressions.ConstantExpression constant = (System.Linq.Expressions.ConstantExpression)expression;
                return null == constant.Value;
            }
            private static Expression UnwrapConvert(Expression input)
            {
                // unwrap all converts
                while (ExpressionType.Convert == input.NodeType)
                {
                    input = ((UnaryExpression)input).Operand;
                }
                return input;
            }
        }
        private sealed class NotEqualsTranslator : TypedTranslator<System.Linq.Expressions.BinaryExpression>
        {
            internal NotEqualsTranslator()
                : base(ExpressionType.NotEqual) { }
            protected override DbExpression TypedTranslate(ExpressionConverter parent, System.Linq.Expressions.BinaryExpression linq)
            {
                // rewrite as a not equals expression
                Expression notLinq = Expression.Not(
                    Expression.Equal(linq.Left, linq.Right));
                return parent.TranslateExpression(notLinq);
            }
        }
        #endregion

        #region Type binary expression translator
        private sealed class IsTranslator : TypedTranslator<TypeBinaryExpression>
        {
            internal IsTranslator()
                : base(ExpressionType.TypeIs) { }
            protected override DbExpression TypedTranslate(ExpressionConverter parent, TypeBinaryExpression linq)
            {
                DbExpression operand = parent.TranslateExpression(linq.Expression);
                TypeUsage fromType = operand.ResultType;
                TypeUsage toType = parent.GetIsOrAsTargetType(fromType, ExpressionType.TypeIs, linq.TypeOperand, linq.Expression.Type);
                return operand.IsOf(toType);
            }
        }
        #endregion

        #region Arithmetic expressions
        private sealed class AddTranslator : BinaryTranslator
        {
            internal AddTranslator()
                : base(ExpressionType.Add, ExpressionType.AddChecked) { }
            protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
            {
                if (TypeSemantics.IsPrimitiveType(left.ResultType, PrimitiveTypeKind.String) &&
                   TypeSemantics.IsPrimitiveType(right.ResultType, PrimitiveTypeKind.String))
                {
                    // Add(string, string) => Concat(string, string)
                    return parent.CreateCanonicalFunction(ExpressionConverter.Concat, linq, left, right);
                }
                else
                {
                    return left.Plus(right);
                }
            }
        }
        private sealed class DivideTranslator : BinaryTranslator
        {
            internal DivideTranslator()
                : base(ExpressionType.Divide) { }
            protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
            {
                return left.Divide(right);
            }
        }
        private sealed class ModuloTranslator : BinaryTranslator
        {
            internal ModuloTranslator()
                : base(ExpressionType.Modulo) { }
            protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
            {
                return left.Modulo(right);
            }
        }
        private sealed class MultiplyTranslator : BinaryTranslator
        {
            internal MultiplyTranslator()
                : base(ExpressionType.Multiply, ExpressionType.MultiplyChecked) { }
            protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
            {
                return left.Multiply(right);
            }
        }
        private sealed class SubtractTranslator : BinaryTranslator
        {
            internal SubtractTranslator()
                : base(ExpressionType.Subtract, ExpressionType.SubtractChecked) { }
            protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
            {
                return left.Minus(right);
            }
        }
        private sealed class NegateTranslator : UnaryTranslator
        {
            internal NegateTranslator()
                : base(ExpressionType.Negate, ExpressionType.NegateChecked) { }
            protected override DbExpression TranslateUnary(ExpressionConverter parent, System.Linq.Expressions.UnaryExpression unary, DbExpression operand)
            {
                return operand.UnaryMinus();
            }
        }
        private sealed class UnaryPlusTranslator : UnaryTranslator
        {
            internal UnaryPlusTranslator()
                : base(ExpressionType.UnaryPlus) { }
            protected override DbExpression TranslateUnary(ExpressionConverter parent, UnaryExpression unary, DbExpression operand)
            {
                // +x = x
                return operand;
            }
        }
        #endregion

        #region Bitwise expressions
        private abstract class BitwiseBinaryTranslator : TypedTranslator<System.Linq.Expressions.BinaryExpression>
        {
            private readonly string _canonicalFunctionName;

            protected BitwiseBinaryTranslator(ExpressionType nodeType, string canonicalFunctionName)
                : base(nodeType)
            {
                _canonicalFunctionName = canonicalFunctionName;
            }

            protected override DbExpression TypedTranslate(ExpressionConverter parent, System.Linq.Expressions.BinaryExpression linq)
            {
                DbExpression left = parent.TranslateExpression(linq.Left);
                DbExpression right = parent.TranslateExpression(linq.Right);

                //If the arguments are binary we translate into logic expressions
                if (TypeSemantics.IsBooleanType(left.ResultType))
                {
                    return TranslateIntoLogicExpression(parent, linq, left, right);
                }

                //Otherwise we translate into bitwise canonical functions
                return parent.CreateCanonicalFunction(_canonicalFunctionName, linq, left, right);
            }
            protected abstract DbExpression TranslateIntoLogicExpression(ExpressionConverter parent, System.Linq.Expressions.BinaryExpression linq, DbExpression left, DbExpression right);
        }
        private sealed class AndTranslator : BitwiseBinaryTranslator
        {
            internal AndTranslator()
                : base(ExpressionType.And, ExpressionConverter.BitwiseAnd) { }
            protected override DbExpression TranslateIntoLogicExpression(ExpressionConverter parent, System.Linq.Expressions.BinaryExpression linq, DbExpression left, DbExpression right)
            {
                return left.And(right);
            }
        }
        private sealed class OrTranslator : BitwiseBinaryTranslator
        {
            internal OrTranslator()
                : base(ExpressionType.Or, ExpressionConverter.BitwiseOr) { }
            protected override DbExpression TranslateIntoLogicExpression(ExpressionConverter parent, System.Linq.Expressions.BinaryExpression linq, DbExpression left, DbExpression right)
            {
                return left.Or(right);
            }
        }
        private sealed class ExclusiveOrTranslator : BitwiseBinaryTranslator
        {
            internal ExclusiveOrTranslator()
                : base(ExpressionType.ExclusiveOr, ExpressionConverter.BitwiseXor) { }
            protected override DbExpression TranslateIntoLogicExpression(ExpressionConverter parent, System.Linq.Expressions.BinaryExpression linq, DbExpression left, DbExpression right)
            {
                //No direct translation, we translate into ((left && !right) || (!left && right))
                DbExpression firstExpression = left.And(right.Not());
                DbExpression secondExpression = left.Not().And(right);
                DbExpression result = firstExpression.Or(secondExpression);
                return result;
            }
        }
        private sealed class NotTranslator : TypedTranslator<UnaryExpression>
        {
            internal NotTranslator()
                : base(ExpressionType.Not) { }
            protected override DbExpression TypedTranslate(ExpressionConverter parent, System.Linq.Expressions.UnaryExpression linq)
            {
                DbExpression operand = parent.TranslateExpression(linq.Operand);
                if (TypeSemantics.IsBooleanType(operand.ResultType))
                {
                    return operand.Not();
                }
                return parent.CreateCanonicalFunction(ExpressionConverter.BitwiseNot, linq, operand);
            }
        }
        #endregion

        #region Unary expression translators
        private abstract class UnaryTranslator
            : TypedTranslator<System.Linq.Expressions.UnaryExpression>
        {
            protected UnaryTranslator(params ExpressionType[] nodeTypes)
                : base(nodeTypes) { }
            protected override DbExpression TypedTranslate(ExpressionConverter parent, System.Linq.Expressions.UnaryExpression linq)
            {
                return TranslateUnary(parent, linq, parent.TranslateExpression(linq.Operand));
            }
            protected abstract DbExpression TranslateUnary(ExpressionConverter parent, System.Linq.Expressions.UnaryExpression unary, DbExpression operand);
        }
        private sealed class QuoteTranslator : UnaryTranslator
        {
            internal QuoteTranslator()
                : base(ExpressionType.Quote) { }
            protected override DbExpression TranslateUnary(ExpressionConverter parent, System.Linq.Expressions.UnaryExpression unary, DbExpression operand)
            {
                // simply return the operand: expressions compilations not cached for LINQ, so
                // parameters are always bound properly
                return operand;
            }
        }
        private sealed class ConvertTranslator : UnaryTranslator
        {
            internal ConvertTranslator()
                : base(ExpressionType.Convert, ExpressionType.ConvertChecked) { }
            protected override DbExpression TranslateUnary(ExpressionConverter parent, System.Linq.Expressions.UnaryExpression unary, DbExpression operand)
            {
                Type toClrType = unary.Type;
                Type fromClrType = unary.Operand.Type;
                DbExpression cast = parent.CreateCastExpression(operand, toClrType, fromClrType);
                return cast;
            }
        }
        private sealed class AsTranslator : UnaryTranslator
        {
            internal AsTranslator()
                : base(ExpressionType.TypeAs) { }
            protected override DbExpression TranslateUnary(ExpressionConverter parent, System.Linq.Expressions.UnaryExpression unary, DbExpression operand)
            {
                TypeUsage fromType = operand.ResultType;
                TypeUsage toType = parent.GetIsOrAsTargetType(fromType, ExpressionType.TypeAs, unary.Type, unary.Operand.Type);
                return operand.TreatAs(toType);
            }
        }
        #endregion
    }
}
