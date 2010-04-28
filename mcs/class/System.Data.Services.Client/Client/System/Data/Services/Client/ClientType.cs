//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.



namespace System.Data.Services.Client
{
    #region Namespaces.

    using System;
    using System.Collections.Generic;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    #endregion Namespaces.

    [DebuggerDisplay("{ElementTypeName}")]
    internal sealed class ClientType
    {
        internal readonly string ElementTypeName;

        internal readonly Type ElementType;

        internal readonly bool IsEntityType;

        internal readonly int KeyCount;

        #region static fields

        private static readonly Dictionary<Type, ClientType> types = new Dictionary<Type, ClientType>(EqualityComparer<Type>.Default);

        private static readonly Dictionary<TypeName, Type> namedTypes = new Dictionary<TypeName, Type>(new TypeNameEqualityComparer());
        #endregion

#if ASTORIA_OPEN_OBJECT
        private readonly ClientProperty openProperties;
#endif
        private ArraySet<ClientProperty> properties;

        private ClientProperty mediaDataMember;

        private bool mediaLinkEntry;

        private EpmSourceTree epmSourceTree;
        
        private EpmTargetTree epmTargetTree;

        private ClientType(Type type, string typeName, bool skipSettableCheck)
        {
            Debug.Assert(null != type, "null type");
            Debug.Assert(!String.IsNullOrEmpty(typeName), "empty typeName");

            this.ElementTypeName = typeName;
            this.ElementType = Nullable.GetUnderlyingType(type) ?? type;
#if ASTORIA_OPEN_OBJECT
            string openObjectPropertyName = null;
#endif
            if (!ClientConvert.IsKnownType(this.ElementType))
            {
#if ASTORIA_OPEN_OBJECT
                #region OpenObject determined by walking type hierarchy and looking for [OpenObjectAttribute("PropertyName")]
                Type openObjectDeclared = this.ElementType;
                for (Type tmp = openObjectDeclared; (null != tmp) && (typeof(object) != tmp); tmp = tmp.BaseType)
                {
                    object[] attributes = openObjectDeclared.GetCustomAttributes(typeof(OpenObjectAttribute), false);
                    if (1 == attributes.Length)
                    {
                        if (null != openObjectPropertyName)
                        {
                            throw Error.InvalidOperation(Strings.Clienttype_MultipleOpenProperty(this.ElementTypeName));
                        }

                        openObjectPropertyName = ((OpenObjectAttribute)attributes[0]).OpenObjectPropertyName;
                        openObjectDeclared = tmp;
                    }
                }
                #endregion
#endif

                Type keyPropertyDeclaredType = null;
                bool isEntity = type.GetCustomAttributes(true).OfType<DataServiceEntityAttribute>().Any();
                DataServiceKeyAttribute dska = type.GetCustomAttributes(true).OfType<DataServiceKeyAttribute>().FirstOrDefault();
                foreach (PropertyInfo pinfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {






                    Type ptype = pinfo.PropertyType;                    ptype = Nullable.GetUnderlyingType(ptype) ?? ptype;

                    if (ptype.IsPointer ||
                        (ptype.IsArray && (typeof(byte[]) != ptype) && typeof(char[]) != ptype) ||
                        (typeof(IntPtr) == ptype) ||
                        (typeof(UIntPtr) == ptype))
                    {
                        continue;
                    }

                    Debug.Assert(!ptype.ContainsGenericParameters, "remove when test case is found that encounters this");

                    if (pinfo.CanRead &&
                        (!ptype.IsValueType || pinfo.CanWrite) &&
                        !ptype.ContainsGenericParameters &&
                        (0 == pinfo.GetIndexParameters().Length))
                    {
                        #region IsKey?
                        bool keyProperty = dska != null ? dska.KeyNames.Contains(pinfo.Name) : false;

                        if (keyProperty)
                        {
                            if (null == keyPropertyDeclaredType)
                            {
                                keyPropertyDeclaredType = pinfo.DeclaringType;
                            }
                            else if (keyPropertyDeclaredType != pinfo.DeclaringType)
                            {
                                throw Error.InvalidOperation(Strings.ClientType_KeysOnDifferentDeclaredType(this.ElementTypeName));
                            }

                            if (!ClientConvert.IsKnownType(ptype))
                            {
                                throw Error.InvalidOperation(Strings.ClientType_KeysMustBeSimpleTypes(this.ElementTypeName));
                            }

                            this.KeyCount++;
                        }
                        #endregion

#if ASTORIA_OPEN_OBJECT
                        #region IsOpenObjectProperty?
                        bool openProperty = (openObjectPropertyName == pinfo.Name) &&
                                              typeof(IDictionary<string, object>).IsAssignableFrom(ptype);
                        Debug.Assert(keyProperty != openProperty || (!keyProperty && !openProperty), "key can't be open type");
                        #endregion

                        ClientProperty property = new ClientProperty(pinfo, ptype, keyProperty, openProperty);

                        if (!property.OpenObjectProperty)
#else
                        ClientProperty property = new ClientProperty(pinfo, ptype, keyProperty);
#endif
                        {
                            if (!this.properties.Add(property, ClientProperty.NameEquality))
                            {
                                int shadow = this.IndexOfProperty(property.PropertyName);
                                if (!property.DeclaringType.IsAssignableFrom(this.properties[shadow].DeclaringType))
                                {                                    this.properties.RemoveAt(shadow);
                                    this.properties.Add(property, null);
                                }
                            }
                        }
#if ASTORIA_OPEN_OBJECT
                        else
                        {
                            if (pinfo.DeclaringType == openObjectDeclared)
                            {
                                this.openProperties = property;
                            }
                        }
#endif
                    }
                }

                #region No KeyAttribute, discover key by name pattern { DeclaringType.Name+ID, ID }
                if (null == keyPropertyDeclaredType)
                {
                    ClientProperty key = null;
                    for (int i = this.properties.Count - 1; 0 <= i; --i)
                    {
                        string propertyName = this.properties[i].PropertyName;
                        if (propertyName.EndsWith("ID", StringComparison.Ordinal))
                        {
                            string declaringTypeName = this.properties[i].DeclaringType.Name;
                            if ((propertyName.Length == (declaringTypeName.Length + 2)) &&
                                propertyName.StartsWith(declaringTypeName, StringComparison.Ordinal))
                            {
                                if ((null == keyPropertyDeclaredType) ||
                                    this.properties[i].DeclaringType.IsAssignableFrom(keyPropertyDeclaredType))
                                {
                                    keyPropertyDeclaredType = this.properties[i].DeclaringType;
                                    key = this.properties[i];
                                }
                            }
                            else if ((null == keyPropertyDeclaredType) && (2 == propertyName.Length))
                            {
                                keyPropertyDeclaredType = this.properties[i].DeclaringType;
                                key = this.properties[i];
                            }
                        }
                    }

                    if (null != key)
                    {
                        Debug.Assert(0 == this.KeyCount, "shouldn't have a key yet");
                        key.KeyProperty = true;
                        this.KeyCount++;
                    }
                }
                else if (this.KeyCount != dska.KeyNames.Count)
                {
                    var m = (from string a in dska.KeyNames
                             where null == (from b in this.properties
                                            where b.PropertyName == a
                                            select b).FirstOrDefault()
                             select a).First<string>();
                    throw Error.InvalidOperation(Strings.ClientType_MissingProperty(this.ElementTypeName, m));
                }
                #endregion

                this.IsEntityType = (null != keyPropertyDeclaredType) || isEntity;
                Debug.Assert(this.KeyCount == this.Properties.Where(k => k.KeyProperty).Count(), "KeyCount mismatch");

                this.WireUpMimeTypeProperties();
                this.CheckMediaLinkEntry();

                if (!skipSettableCheck)
                {
#if ASTORIA_OPEN_OBJECT
                    if ((0 == this.properties.Count) && (null == this.openProperties))
#else
                    if (0 == this.properties.Count)
#endif
                    {                        throw Error.InvalidOperation(Strings.ClientType_NoSettableFields(this.ElementTypeName));
                    }
                }
            }

            this.properties.TrimToSize();
            this.properties.Sort<string>(ClientProperty.GetPropertyName, String.CompareOrdinal);

#if ASTORIA_OPEN_OBJECT
            #region Validate OpenObjectAttribute was used
            if ((null != openObjectPropertyName) && (null == this.openProperties))
            {
                throw Error.InvalidOperation(Strings.ClientType_MissingOpenProperty(this.ElementTypeName, openObjectPropertyName));
            }

            Debug.Assert((null != openObjectPropertyName) == (null != this.openProperties), "OpenProperties mismatch");
            #endregion
#endif
            this.BuildEpmInfo(type);
        }

        internal ArraySet<ClientProperty> Properties
        {
            get { return this.properties; }
        }

        internal ClientProperty MediaDataMember
        {
            get { return this.mediaDataMember; }
        }

        internal bool IsMediaLinkEntry
        {
            get { return this.mediaLinkEntry; }
        }

        internal EpmSourceTree EpmSourceTree
        {
            get
            {
                if (this.epmSourceTree == null)
                {
                    this.epmTargetTree = new EpmTargetTree();
                    this.epmSourceTree = new EpmSourceTree(this.epmTargetTree);
                }
                
                return this.epmSourceTree;
            }
        }

        internal EpmTargetTree EpmTargetTree
        {
            get
            {
                Debug.Assert(this.epmTargetTree != null, "Must have valid target tree");
                return this.epmTargetTree;
            }
        }

        internal bool HasEntityPropertyMappings
        {
            get
            {
                return this.epmSourceTree != null;
            }
        }

        internal bool EpmIsV1Compatible
        {
            get
            {
                return !this.HasEntityPropertyMappings || this.EpmTargetTree.IsV1Compatible;
            }
        }

        internal static bool CanAssignNull(Type type)
        {
            Debug.Assert(type != null, "type != null");
            return !type.IsValueType || (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        internal static bool CheckElementTypeIsEntity(Type t)
        {
            t = TypeSystem.GetElementType(t);
            t = Nullable.GetUnderlyingType(t) ?? t;
            return ClientType.Create(t, false).IsEntityType;
        }

        internal static ClientType Create(Type type)
        {
            return Create(type, true );
        }

        internal static ClientType Create(Type type, bool expectModelType)
        {
            ClientType clientType;
            lock (ClientType.types)
            {
                ClientType.types.TryGetValue(type, out clientType);
            }

            if (null == clientType)
            {
                bool skipSettableCheck = !expectModelType;
                clientType = new ClientType(type, type.ToString(), skipSettableCheck); // ToString expands generic type name where as FullName does not
                if (expectModelType)
                {
                    lock (ClientType.types)
                    {
                        ClientType existing;
                        if (ClientType.types.TryGetValue(type, out existing))
                        {
                            clientType = existing;
                        }
                        else
                        {
                            ClientType.types.Add(type, clientType);
                        }
                    }
                }
            }

            return clientType;
        }

#if !ASTORIA_LIGHT
        internal static Type ResolveFromName(string wireName, Type userType)
#else
        internal static Type ResolveFromName(string wireName, Type userType, Type contextType)
#endif
        {
            Type foundType;

            TypeName typename;
            typename.Type = userType;
            typename.Name = wireName;

            bool foundInCache;
            lock (ClientType.namedTypes)
            {
                foundInCache = ClientType.namedTypes.TryGetValue(typename, out foundType);
            }

            if (!foundInCache)
            {
                string name = wireName;
                int index = wireName.LastIndexOf('.');
                if ((0 <= index) && (index < wireName.Length - 1))
                {
                    name = wireName.Substring(index + 1);
                }

                if (userType.Name == name)
                {
                    foundType = userType;
                }
                else
                {
#if !ASTORIA_LIGHT
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
#else
                    foreach (Assembly assembly in new Assembly[] { userType.Assembly, contextType.Assembly }.Distinct())
#endif
                    {
                        Type found = assembly.GetType(wireName, false);
                        ResolveSubclass(name, userType, found, ref foundType);

                        if (null == found)
                        {
                            Type[] types = null;
                            try
                            {
                                types = assembly.GetTypes();
                            }
                            catch (ReflectionTypeLoadException)
                            {
                            }

                            if (null != types)
                            {
                                foreach (Type t in types)
                                {
                                    ResolveSubclass(name, userType, t, ref foundType);
                                }
                            }
                        }
                    }
                }

                lock (ClientType.namedTypes)
                {
                    ClientType.namedTypes[typename] = foundType;
                }
            }

            return foundType;
        }

        internal static Type GetImplementationType(Type propertyType, Type genericTypeDefinition)
        {
            if (IsConstructedGeneric(propertyType, genericTypeDefinition))
            {   
                return propertyType;
            }
            else
            {
                Type implementationType = null;
                foreach (Type interfaceType in propertyType.GetInterfaces())
                {
                    if (IsConstructedGeneric(interfaceType, genericTypeDefinition))
                    {
                        if (null == implementationType)
                        {   
                            implementationType = interfaceType;
                        }
                        else
                        {   
                            throw Error.NotSupported(Strings.ClientType_MultipleImplementationNotSupported);
                        }
                    }
                }

                return implementationType;
            }
        }

        internal static MethodInfo GetAddToCollectionMethod(Type collectionType, out Type type)
        {
            return GetCollectionMethod(collectionType, typeof(ICollection<>), "Add", out type);
        }

        internal static MethodInfo GetRemoveFromCollectionMethod(Type collectionType, out Type type)
        {
            return GetCollectionMethod(collectionType, typeof(ICollection<>), "Remove", out type);
        }

        internal static MethodInfo GetCollectionMethod(Type propertyType, Type genericTypeDefinition, string methodName, out Type type)
        {
            Debug.Assert(null != propertyType, "null propertyType");
            Debug.Assert(null != genericTypeDefinition, "null genericTypeDefinition");
            Debug.Assert(genericTypeDefinition.IsGenericTypeDefinition, "!IsGenericTypeDefinition");

            type = null;

            Type implementationType = GetImplementationType(propertyType, genericTypeDefinition);
            if (null != implementationType)
            {
                Type[] genericArguments = implementationType.GetGenericArguments();
                MethodInfo methodInfo = implementationType.GetMethod(methodName);
                Debug.Assert(null != methodInfo, "should have found the method");

#if DEBUG
                Debug.Assert(null != genericArguments, "null genericArguments");
                ParameterInfo[] parameters = methodInfo.GetParameters();
                if (0 < parameters.Length)
                {
                    Debug.Assert(genericArguments.Length == parameters.Length, "genericArguments don't match parameters");
                    for (int i = 0; i < genericArguments.Length; ++i)
                    {
                        Debug.Assert(genericArguments[i] == parameters[i].ParameterType, "parameter doesn't match generic argument");
                    }
                }
#endif
                type = genericArguments[genericArguments.Length - 1];
                return methodInfo;
            }

            return null;
        }

        internal object CreateInstance()
        {
            return Activator.CreateInstance(this.ElementType);
        }

        internal ClientProperty GetProperty(string propertyName, bool ignoreMissingProperties)
        {
            int index = this.IndexOfProperty(propertyName);
            if (0 <= index)
            {
                return this.properties[index];
            }
#if ASTORIA_OPEN_OBJECT
            else if (null != this.openProperties)
            {
                return this.openProperties;
            }
#endif
            else if (!ignoreMissingProperties)
            {
                throw Error.InvalidOperation(Strings.ClientType_MissingProperty(this.ElementTypeName, propertyName));
            }

            return null;
        }

       private static bool IsConstructedGeneric(Type type, Type genericTypeDefinition)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(!type.ContainsGenericParameters, "remove when test case is found that encounters this");
            Debug.Assert(genericTypeDefinition != null, "genericTypeDefinition != null");

            return type.IsGenericType && (type.GetGenericTypeDefinition() == genericTypeDefinition) && !type.ContainsGenericParameters;
        }

        private static void ResolveSubclass(string wireClassName, Type userType, Type type, ref Type existing)
        {
            if ((null != type) && type.IsVisible && (wireClassName == type.Name) && userType.IsAssignableFrom(type))
            {
                if (null != existing)
                {
                    throw Error.InvalidOperation(Strings.ClientType_Ambiguous(wireClassName, userType));
                }

                existing = type;
            }
        }

        private void BuildEpmInfo(Type type)
        {
            if (type.BaseType != null && type.BaseType != typeof(object))
            {
                this.BuildEpmInfo(type.BaseType);
            }

            foreach (EntityPropertyMappingAttribute epmAttr in type.GetCustomAttributes(typeof(EntityPropertyMappingAttribute), false))
            {
                this.BuildEpmInfo(epmAttr, type);
            }
        }

        private void BuildEpmInfo(EntityPropertyMappingAttribute epmAttr, Type definingType)
        {
            ParameterExpression rsrcParam = Expression.Parameter(this.ElementType, "rsrc");
            ClientProperty rsrcProp = null;

            Expression propValReaderExpr = this.BuildPropertyReader(
                                                        rsrcParam,
                                                        this,
                                                        epmAttr.SourcePath.Split('/'),
                                                        0,
                                                        ref rsrcProp);

            Delegate dlgPropValReader = Expression.Lambda(propValReaderExpr, rsrcParam).Compile();
            this.EpmSourceTree.Add(new EntityPropertyMappingInfo { Attribute = epmAttr, PropValReader = dlgPropValReader, DefiningType = definingType });
        }

        private Expression BuildPropertyReader(Expression expr, ClientType rsrcType, String[] srcPathSegments, int currentSegment, ref ClientProperty rsrcProp)
        {
            if (currentSegment == srcPathSegments.Length)
            {
                if (Util.TypeAllowsNull(expr.Type))
                {
                    return expr;
                }
                else
                {
                    return Expression.Convert(expr, typeof(Nullable<>).MakeGenericType(expr.Type));
                }
            }

            String srcPathPart = srcPathSegments[currentSegment];

            rsrcProp = rsrcType.GetProperty(srcPathPart, true);
            if (rsrcProp == null)
            {
                throw Error.InvalidOperation(Strings.EpmSourceTree_InaccessiblePropertyOnType(srcPathPart, rsrcType.ElementTypeName));
            }

            if (rsrcProp.IsKnownType ^ (currentSegment == srcPathSegments.Length - 1))
            {
                throw Error.InvalidOperation(!rsrcProp.IsKnownType ? Strings.EpmClientType_PropertyIsComplex(rsrcProp.PropertyName) :
                                                                     Strings.EpmClientType_PropertyIsPrimitive(rsrcProp.PropertyName));
            }

            MemberExpression objectDotProp = Expression.Property(expr, srcPathPart);

            Expression recursiveExpr = this.BuildPropertyReader(
                                            objectDotProp,
                                            rsrcProp.IsKnownType ? null : ClientType.Create(rsrcProp.PropertyType),
                                            srcPathSegments,
                                            ++currentSegment,
                                            ref rsrcProp);

            BinaryExpression objectIsNull = Expression.Equal(expr, Expression.Constant(null));

            ConstantExpression nullableNull = Expression.Constant(
                            null,
                            Util.GetTypeAllowingNull(rsrcProp.PropertyType));

            return Expression.Condition(objectIsNull, nullableNull, recursiveExpr);
        }

        private int IndexOfProperty(string propertyName)
        {
            return this.properties.IndexOf(propertyName, ClientProperty.GetPropertyName, String.Equals);
        }

        private void WireUpMimeTypeProperties()
        {
            MimeTypePropertyAttribute attribute = (MimeTypePropertyAttribute)this.ElementType.GetCustomAttributes(typeof(MimeTypePropertyAttribute), true).SingleOrDefault();
            if (null != attribute)
            {
                int dataIndex, mimeTypeIndex;
                if ((0 > (dataIndex = this.IndexOfProperty(attribute.DataPropertyName))) ||
                    (0 > (mimeTypeIndex = this.IndexOfProperty(attribute.MimeTypePropertyName))))
                {
                    throw Error.InvalidOperation(Strings.ClientType_MissingMimeTypeProperty(attribute.DataPropertyName, attribute.MimeTypePropertyName));
                }

                Debug.Assert(0 <= dataIndex, "missing data property");
                Debug.Assert(0 <= mimeTypeIndex, "missing mime type property");
                this.Properties[dataIndex].MimeTypeProperty = this.Properties[mimeTypeIndex];
            }
        }

        private void CheckMediaLinkEntry()
        {
            object[] attributes = this.ElementType.GetCustomAttributes(typeof(MediaEntryAttribute), true);
            if (attributes != null && attributes.Length > 0)
            {
                Debug.Assert(attributes.Length == 1, "The AttributeUsage in the attribute definition should be preventing more than 1 per property");

                MediaEntryAttribute mediaEntryAttribute = (MediaEntryAttribute)attributes[0];
                this.mediaLinkEntry = true;

                int index = this.IndexOfProperty(mediaEntryAttribute.MediaMemberName);
                if (index < 0)
                {
                    throw Error.InvalidOperation(Strings.ClientType_MissingMediaEntryProperty(
                        mediaEntryAttribute.MediaMemberName));
                }

                this.mediaDataMember = this.properties[index];
            }

            attributes = this.ElementType.GetCustomAttributes(typeof(HasStreamAttribute), true);
            if (attributes != null && attributes.Length > 0)
            {
                Debug.Assert(attributes.Length == 1, "The AttributeUsage in the attribute definition should be preventing more than 1 per property");
                this.mediaLinkEntry = true;
            }
        }

        private struct TypeName
        {
             internal Type Type;

            internal string Name;
        }

        [DebuggerDisplay("{PropertyName}")]
        internal sealed class ClientProperty
        {
            internal readonly string PropertyName;

            internal readonly Type NullablePropertyType;

            internal readonly Type PropertyType;

            internal readonly Type CollectionType;

            internal readonly bool IsKnownType;

#if ASTORIA_OPEN_OBJECT
            internal readonly bool OpenObjectProperty;
#endif

            private readonly MethodInfo propertyGetter;

            private readonly MethodInfo propertySetter;

            private readonly MethodInfo setMethod;

            private readonly MethodInfo addMethod;

            private readonly MethodInfo removeMethod;

            private readonly MethodInfo containsMethod;

             private bool keyProperty;

            private ClientProperty mimeTypeProperty;

#if ASTORIA_OPEN_OBJECT
           internal ClientProperty(PropertyInfo property, Type propertyType, bool keyProperty, bool openObjectProperty)
#else
            internal ClientProperty(PropertyInfo property, Type propertyType, bool keyProperty)
#endif
            {
                Debug.Assert(null != property, "null property");
                Debug.Assert(null != propertyType, "null propertyType");
                Debug.Assert(null == Nullable.GetUnderlyingType(propertyType), "should already have been denullified");

                this.PropertyName = property.Name;
                this.NullablePropertyType = property.PropertyType;
                this.PropertyType = propertyType;
                this.propertyGetter = property.GetGetMethod();
                this.propertySetter = property.GetSetMethod();
                this.keyProperty = keyProperty;
#if ASTORIA_OPEN_OBJECT
                this.OpenObjectProperty = openObjectProperty;
#endif

                this.IsKnownType = ClientConvert.IsKnownType(propertyType);
                if (!this.IsKnownType)
                {
                    this.setMethod = GetCollectionMethod(this.PropertyType, typeof(IDictionary<,>), "set_Item", out this.CollectionType);
                    if (null == this.setMethod)
                    {
                        this.containsMethod = GetCollectionMethod(this.PropertyType, typeof(ICollection<>), "Contains", out this.CollectionType);
                        this.addMethod = GetAddToCollectionMethod(this.PropertyType, out this.CollectionType);
                        this.removeMethod = GetRemoveFromCollectionMethod(this.PropertyType, out this.CollectionType);
                    }
                }

                Debug.Assert(!this.keyProperty || this.IsKnownType, "can't have an random type as key");
            }

            internal Type DeclaringType
            {
                get { return this.propertyGetter.DeclaringType; }
            }

            internal bool KeyProperty
            {
                get { return this.keyProperty; }
                set { this.keyProperty = value; }
            }

            internal ClientProperty MimeTypeProperty
            {
                get { return this.mimeTypeProperty; }
                set { this.mimeTypeProperty = value; }
            }

            internal static bool GetKeyProperty(ClientProperty x)
            {
                return x.KeyProperty;
            }

            internal static string GetPropertyName(ClientProperty x)
            {
                return x.PropertyName;
            }

            internal static bool NameEquality(ClientProperty x, ClientProperty y)
            {
                return String.Equals(x.PropertyName, y.PropertyName);
            }

            internal object GetValue(object instance)
            {
                Debug.Assert(null != instance, "null instance");
                Debug.Assert(null != this.propertyGetter, "null propertyGetter");
                return this.propertyGetter.Invoke(instance, null);
            }

            internal void RemoveValue(object instance, object value)
            {
                Debug.Assert(null != instance, "null instance");
                Debug.Assert(null != this.removeMethod, "missing removeMethod");

                Debug.Assert(this.PropertyType.IsAssignableFrom(instance.GetType()), "unexpected collection instance");
                Debug.Assert((null == value) || this.CollectionType.IsAssignableFrom(value.GetType()), "unexpected collection value to add");
                this.removeMethod.Invoke(instance, new object[] { value });
            }

#if ASTORIA_OPEN_OBJECT
            internal void SetValue(object instance, object value, string propertyName, ref object openProperties, bool allowAdd)
#else
            internal void SetValue(object instance, object value, string propertyName, bool allowAdd)
#endif
            {
                Debug.Assert(null != instance, "null instance");
                if (null != this.setMethod)
                {
#if ASTORIA_OPEN_OBJECT
                    if (this.OpenObjectProperty)
                    {
                        if (null == openProperties)
                        {
                            if (null == (openProperties = this.propertyGetter.Invoke(instance, null)))
                            {
                                throw Error.NotSupported(Strings.ClientType_NullOpenProperties(this.PropertyName));
                            }
                        }

                        ((IDictionary<string, object>)openProperties)[propertyName] = value;
                    }
                    else
#endif
                    {
                        Debug.Assert(this.PropertyType.IsAssignableFrom(instance.GetType()), "unexpected dictionary instance");
                        Debug.Assert((null == value) || this.CollectionType.IsAssignableFrom(value.GetType()), "unexpected dictionary value to set");

                        this.setMethod.Invoke(instance, new object[] { propertyName, value });
                    }
                }
                else if (allowAdd && (null != this.addMethod))
                {
                    Debug.Assert(this.PropertyType.IsAssignableFrom(instance.GetType()), "unexpected collection instance");
                    Debug.Assert((null == value) || this.CollectionType.IsAssignableFrom(value.GetType()), "unexpected collection value to add");

                    if (!(bool)this.containsMethod.Invoke(instance, new object[] { value }))
                    {
                        this.addMethod.Invoke(instance, new object[] { value });
                    }
                }
                else if (null != this.propertySetter)
                {
                    Debug.Assert((null == value) || this.PropertyType.IsAssignableFrom(value.GetType()), "unexpected property value to set");

                   this.propertySetter.Invoke(instance, new object[] { value });
                }
                else
                {
                    throw Error.InvalidOperation(Strings.ClientType_MissingProperty(value.GetType().ToString(), propertyName));
                }
            }
        }

        private sealed class TypeNameEqualityComparer : IEqualityComparer<TypeName>
        {
           public bool Equals(TypeName x, TypeName y)
            {
                return (x.Type == y.Type && x.Name == y.Name);
            }

            public int GetHashCode(TypeName obj)
            {
                return obj.Type.GetHashCode() ^ obj.Name.GetHashCode();
            }
        }
    }
}
