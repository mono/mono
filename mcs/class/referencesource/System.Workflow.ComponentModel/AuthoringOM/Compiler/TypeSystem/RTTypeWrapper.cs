#pragma warning disable 1634, 1691
namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.CodeDom;
    using System.Text;

    internal sealed class RTTypeWrapper : Type, ICloneable
    {
        #region BoundedTypeArray Comparer

        private class TypeArrayComparer : IEqualityComparer
        {
            #region IEqualityComparer Members

            bool IEqualityComparer.Equals(object x, object y)
            {
                Array xArray = x as Array;
                Array yArray = y as Array;
                if (xArray == null || yArray == null || xArray.Rank != 1 || yArray.Rank != 1)
                    return false;

                bool mismatch = false;
                if (xArray.Length == yArray.Length)
                {
                    for (int index = 0; !mismatch && index < xArray.Length; index++)
                        mismatch = (xArray.GetValue(index) != yArray.GetValue(index));
                }
                else
                {
                    mismatch = true;
                }
                return !mismatch;
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                return 0;
            }
            #endregion
        }

        #endregion

        #region Members and Constructors
        private Type runtimeType = null;
        private Type[] typeArgs = null;
        private ITypeProvider typeProvider = null;
        private Hashtable memberMapping = new Hashtable();
        private Hashtable boundedTypes = new Hashtable(new TypeArrayComparer());

        internal RTTypeWrapper(ITypeProvider typeProvider, Type runtimeType)
        {
            if (runtimeType == null)
                throw new ArgumentNullException("runtimeType");

            //we dont expect DesignTimeType to be passed to this class for wrapping purposes
            if (runtimeType.Assembly == null)
                throw new ArgumentException(SR.GetString(SR.Error_InvalidRuntimeType), "runtimeType");

            this.typeProvider = typeProvider;
            this.runtimeType = runtimeType;
        }

        internal ITypeProvider Provider
        {
            get
            {
                return this.typeProvider;
            }
        }

        private RTTypeWrapper(ITypeProvider typeProvider, Type runtimeType, Type[] typeArgs)
        {
            if (runtimeType == null)
                throw new ArgumentNullException("runtimeType");

            //we dont expect DesignTimeType to be passed to this class for wrapping purposes
            if (runtimeType.Assembly == null)
                throw new ArgumentException(SR.GetString(SR.Error_InvalidArgumentValue), "runtimeType");

            this.typeProvider = typeProvider;
            this.runtimeType = runtimeType;

            if (!IsGenericTypeDefinition)
                throw new ArgumentException(SR.GetString(SR.Error_InvalidArgumentValue), "runtimeType");

            this.typeArgs = new Type[typeArgs.Length];
            for (int i = 0; i < typeArgs.Length; i++)
            {
                this.typeArgs[i] = typeArgs[i];
                if (this.typeArgs[i] == null)
                    throw new ArgumentException(SR.GetString(SR.Error_InvalidArgumentValue), "typeArgs");
            }
        }

        #endregion

        #region Properties

        public override int MetadataToken
        {
            get { return this.runtimeType.MetadataToken; }
        }

        public override Assembly Assembly
        {
            get
            {
                if (this.typeArgs != null)
                {
                    foreach (Type type in this.typeArgs)
                        if (type.Assembly == null)
                            return null;
                }

                return this.runtimeType.Assembly;
            }
        }
        public override string AssemblyQualifiedName
        {
            get
            {
                return this.FullName + ", " + this.runtimeType.Assembly.FullName;
            }
        }
        public override Type BaseType
        {
            get
            {
                return ResolveTypeFromTypeSystem(this.runtimeType.BaseType);
            }
        }
        public override Type DeclaringType
        {
            get
            {
                if (this.runtimeType.DeclaringType == null)
                    return null;
                return this.typeProvider.GetType(this.runtimeType.DeclaringType.AssemblyQualifiedName);
            }
        }

        public override string FullName
        {
            get
            {
                StringBuilder fullName = new StringBuilder(this.runtimeType.FullName);
                if (this.typeArgs != null && this.typeArgs.Length > 0)
                {
                    fullName.Append("[");
                    for (int index = 0; index < this.typeArgs.Length; index++)
                    {
                        fullName.Append("[");
                        fullName.Append(this.typeArgs[index].AssemblyQualifiedName);
                        fullName.Append("]");
                        if (index < (this.typeArgs.Length - 1))
                            fullName.Append(",");
                    }
                    fullName.Append("]");
                }
                return fullName.ToString();
            }
        }
        public override Guid GUID
        {
            get
            {
                return this.runtimeType.GUID;
            }
        }
        public override Module Module
        {
            get
            {
                return this.runtimeType.Module;
            }
        }
        public override string Name
        {
            get
            {
                if (IsGenericType && !IsGenericTypeDefinition)
                    return GetGenericTypeDefinition().FullName.Substring(Namespace.Length + 1);
                else if (Namespace != null)
                    return FullName.Substring(Namespace.Length + 1);
                else
                    return FullName;
            }
        }
        public override string Namespace
        {
            get
            {
                return this.runtimeType.Namespace;
            }
        }
        public override RuntimeTypeHandle TypeHandle
        {
            get
            {
                return this.runtimeType.TypeHandle;
            }
        }
        public override Type UnderlyingSystemType
        {
            get
            {
                return this.runtimeType.UnderlyingSystemType;
            }
        }
        private bool IsAssignable(Type type1, Type type2)
        {
            Type typeTemp1 = ResolveTypeFromTypeSystem(type1);
            Type typeTemp2 = ResolveTypeFromTypeSystem(type2);
            return TypeProvider.IsAssignable(typeTemp1, typeTemp2);
        }

        internal Type ResolveTypeFromTypeSystem(Type type)
        {
            if (type == null)
                return null;

            if (type.IsGenericParameter)
                if (this.typeArgs == null)
                    return type;
                else
                    type = this.typeArgs[type.GenericParameterPosition];

            Type returnType = null;
            try
            {
                if (!String.IsNullOrEmpty(type.AssemblyQualifiedName))
                    returnType = this.typeProvider.GetType(type.AssemblyQualifiedName);
            }
            catch
            {
                // Work aroundh: there are certain generic types whch we are not able to resolve
                // form type system, this fix will make sure that we are in thowse cases returning 
                // the original types
            }
            if (returnType == null)
                returnType = type;

            if (returnType.IsGenericType)
                returnType = ResolveGenericTypeFromTypeSystem(returnType);

            return returnType;
        }

        internal Type ResolveGenericTypeFromTypeSystem(Type type)
        {
            if (this.runtimeType.IsGenericTypeDefinition)
            {
                Type baseType = null;
                if (!type.IsNested)
                    baseType = this.typeProvider.GetType(type.Namespace + "." + type.Name);
                else
                {
                    baseType = type;
                    string baseTypeName = type.Name;
                    while (baseType.DeclaringType != null)
                    {
                        baseType = baseType.DeclaringType;
                        baseTypeName = baseType.Name + "+" + baseTypeName;
                    }
                    baseTypeName = baseType.Namespace + "." + baseTypeName;
                    baseType = this.typeProvider.GetType(baseTypeName);
                }

                if (baseType != null)
                    return baseType.MakeGenericType(this.typeArgs);
                else
                    return type;
            }
            else
                return type;
        }
        #endregion

        #region public methods

        public override bool Equals(object obj)
        {
            Type otherType = obj as Type;
            if (otherType is RTTypeWrapper)
                otherType = ((RTTypeWrapper)otherType).runtimeType;

            return this.runtimeType == otherType;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.runtimeType.GetCustomAttributes(inherit);
        }
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this.runtimeType.GetCustomAttributes(attributeType, inherit);
        }
        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            List<ConstructorInfo> ctorInfos = new List<ConstructorInfo>();
            foreach (ConstructorInfo ctorInfo in this.runtimeType.GetConstructors(bindingAttr))
                ctorInfos.Add(EnsureConstructorWrapped(ctorInfo));
            return ctorInfos.ToArray();
        }
        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            EventInfo eventInfo = this.runtimeType.GetEvent(name, bindingAttr);
            if (eventInfo != null)
                eventInfo = EnsureEventWrapped(eventInfo);
            return eventInfo;
        }
        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            List<EventInfo> eventInfos = new List<EventInfo>();
            foreach (EventInfo eventInfo in this.runtimeType.GetEvents(bindingAttr))
                eventInfos.Add(EnsureEventWrapped(eventInfo));
            return eventInfos.ToArray();
        }
        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            FieldInfo field = this.runtimeType.GetField(name, bindingAttr);
            if (field != null)
                field = EnsureFieldWrapped(field);
            return field;
        }
        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            List<FieldInfo> fieldInfos = new List<FieldInfo>();
            foreach (FieldInfo fieldInfo in this.runtimeType.GetFields(bindingAttr))
                fieldInfos.Add(EnsureFieldWrapped(fieldInfo));
            return fieldInfos.ToArray();
        }
        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            List<MethodInfo> methodInfos = new List<MethodInfo>();
            foreach (MethodInfo methodInfo in this.runtimeType.GetMethods(bindingAttr))
                methodInfos.Add(EnsureMethodWrapped(methodInfo));
            return methodInfos.ToArray();
        }
        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            List<PropertyInfo> propInfos = new List<PropertyInfo>();
            foreach (PropertyInfo propInfo in this.runtimeType.GetProperties(bindingAttr))
                propInfos.Add(EnsurePropertyWrapped(propInfo));
            return propInfos.ToArray();
        }
        public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
        {
            List<MemberInfo> memberInfos = new List<MemberInfo>();
            foreach (MemberInfo memberInfo in this.runtimeType.GetMember(name, type, bindingAttr))
                memberInfos.Add(EnsureMemberWrapped(memberInfo));
            return memberInfos.ToArray();
        }
        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            List<MemberInfo> memberInfos = new List<MemberInfo>();
            foreach (MemberInfo memberInfo in this.runtimeType.GetMembers(bindingAttr))
                memberInfos.Add(EnsureMemberWrapped(memberInfo));
            return memberInfos.ToArray();
        }
        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            Type nestedType = this.runtimeType.GetNestedType(name, bindingAttr);
            if (nestedType != null)
                nestedType = ResolveTypeFromTypeSystem(nestedType);
            return nestedType;
        }
        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            List<Type> nestedTypes = new List<Type>();
            foreach (Type nestedType in this.runtimeType.GetNestedTypes(bindingAttr))
                nestedTypes.Add(ResolveTypeFromTypeSystem(nestedType));
            return nestedTypes.ToArray();
        }
        public override Type GetInterface(string name, bool ignoreCase)
        {
            Type itfType = this.runtimeType.GetInterface(name, ignoreCase);
            if (itfType != null)
                itfType = ResolveTypeFromTypeSystem(itfType);
            return itfType;
        }
        public override Type[] GetInterfaces()
        {
            List<Type> itfTypes = new List<Type>();
            foreach (Type itfType in this.runtimeType.GetInterfaces())
            {
                Type interfaceType = ResolveTypeFromTypeSystem(itfType);
                itfTypes.Add(interfaceType);
            }
            return itfTypes.ToArray();
        }
        public override object InvokeMember(string name, BindingFlags bindingFlags, Binder binder, object target, object[] providedArgs, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParams)
        {
            return this.runtimeType.InvokeMember(name, bindingFlags, binder, target, providedArgs, modifiers, culture, namedParams);
        }
        public override bool IsSubclassOf(Type potentialBaseType)
        {
            return System.Workflow.ComponentModel.Compiler.TypeProvider.IsSubclassOf(this.runtimeType, potentialBaseType);
        }
        public override bool IsAssignableFrom(Type c)
        {
            Type rtType = this.runtimeType;
            if (rtType.IsGenericTypeDefinition && this.IsGenericType)
                rtType = ResolveGenericTypeFromTypeSystem(rtType);

            return System.Workflow.ComponentModel.Compiler.TypeProvider.IsAssignable(rtType, c);
        }
        public override string ToString()
        {
            return this.runtimeType.ToString();
        }
        public override int GetHashCode()
        {
            return this.runtimeType.GetHashCode();
        }
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this.runtimeType.IsDefined(attributeType, inherit);
        }
        public override Type GetElementType()
        {
            return ResolveTypeFromTypeSystem(this.runtimeType.GetElementType());
        }

        #endregion

        #region Helpers

        private PropertyInfo EnsurePropertyWrapped(PropertyInfo realInfo)
        {
            PropertyInfo wrapperInfo = (PropertyInfo)this.memberMapping[realInfo];
            if (wrapperInfo == null)
            {
                wrapperInfo = new RTPropertyInfoWrapper(this, realInfo);
                this.memberMapping.Add(realInfo, wrapperInfo);
            }
            return wrapperInfo;
        }
        internal MethodInfo EnsureMethodWrapped(MethodInfo realInfo)
        {
            MethodInfo wrapperInfo = (MethodInfo)this.memberMapping[realInfo];
            if (wrapperInfo == null)
            {
                wrapperInfo = new RTMethodInfoWrapper(this, realInfo);
                this.memberMapping.Add(realInfo, wrapperInfo);
            }
            return wrapperInfo;

        }
        private MemberInfo EnsureMemberWrapped(MemberInfo memberInfo)
        {
            MemberInfo returnMemberInfo = null;
            if (memberInfo is PropertyInfo)
                returnMemberInfo = EnsurePropertyWrapped(memberInfo as PropertyInfo);
            else if (memberInfo is ConstructorInfo)
                returnMemberInfo = EnsureConstructorWrapped(memberInfo as ConstructorInfo);
            else if (memberInfo is EventInfo)
                returnMemberInfo = EnsureEventWrapped(memberInfo as EventInfo);
            else if (memberInfo is FieldInfo)
                returnMemberInfo = EnsureFieldWrapped(memberInfo as FieldInfo);
            else if (memberInfo is MethodInfo)
                returnMemberInfo = EnsureMethodWrapped(memberInfo as MethodInfo);
            return returnMemberInfo;
        }
        private ConstructorInfo EnsureConstructorWrapped(ConstructorInfo realInfo)
        {
            ConstructorInfo wrapperInfo = (ConstructorInfo)this.memberMapping[realInfo];
            if (wrapperInfo == null)
            {
                wrapperInfo = new RTConstructorInfoWrapper(this, realInfo);
                this.memberMapping.Add(realInfo, wrapperInfo);
            }
            return wrapperInfo;

        }
        private EventInfo EnsureEventWrapped(EventInfo realInfo)
        {
            EventInfo wrapperInfo = (EventInfo)this.memberMapping[realInfo];
            if (wrapperInfo == null)
            {
                wrapperInfo = new RTEventInfoWrapper(this, realInfo);
                this.memberMapping.Add(realInfo, wrapperInfo);
            }
            return wrapperInfo;
        }
        private FieldInfo EnsureFieldWrapped(FieldInfo realInfo)
        {
            FieldInfo wrapperInfo = (FieldInfo)this.memberMapping[realInfo];
            if (wrapperInfo == null)
            {
                wrapperInfo = new RTFieldInfoWrapper(this, realInfo);
                this.memberMapping.Add(realInfo, wrapperInfo);
            }
            return wrapperInfo;
        }

        #endregion

        #region Support for generics
        public override bool IsGenericTypeDefinition
        {
            get
            {
                if (this.typeArgs != null && this.typeArgs.GetLength(0) > 0)
                    return false;
                return this.runtimeType.IsGenericTypeDefinition;
            }
        }
        public override bool IsGenericParameter
        {
            get
            {
                return this.runtimeType.IsGenericParameter;
            }
        }
        public override int GenericParameterPosition
        {
            get
            {
                return this.runtimeType.GenericParameterPosition;
            }
        }
        public override bool IsGenericType
        {
            get
            {
                if (this.typeArgs != null && this.typeArgs.GetLength(0) > 0)
                    return true;
                return this.runtimeType.IsGenericType;
            }
        }
        public override bool ContainsGenericParameters
        {
            get
            {
                if (this.typeArgs != null && this.typeArgs.GetLength(0) > 0)
                    return false;
                return this.runtimeType.ContainsGenericParameters;
            }
        }
        public override Type[] GetGenericArguments()
        {
            return this.typeArgs;
        }
        public override Type GetGenericTypeDefinition()
        {
            if (this.IsGenericType)
                return this.runtimeType;
            return this;
        }
        public override Type MakeGenericType(params Type[] typeArgs)
        {
            if (typeArgs == null)
                throw new ArgumentNullException("typeArgs");

            Type[] types = new Type[typeArgs.Length];

            if (!IsGenericTypeDefinition)
                throw new ArgumentException(SR.GetString(SR.Error_InvalidArgumentValue), "typeArgs");

            for (int i = 0; i < typeArgs.Length; i++)
            {
                types[i] = typeArgs[i];
                if (types[i] == null)
                    throw new ArgumentException(SR.GetString(SR.Error_InvalidArgumentValue), "typeArgs");
            }

            Type returnType = this.boundedTypes[typeArgs] as Type;
            if (returnType == null)
            {
                // handle Nullable<T> specially
                if ((typeArgs.Length == 1) && (this.runtimeType == typeof(Nullable<>)) && !(typeArgs[0].IsEnum))
                {
                    switch (Type.GetTypeCode(typeArgs[0]))
                    {
                        case TypeCode.Boolean:
                            returnType = typeof(bool?);
                            break;
                        case TypeCode.Byte:
                            returnType = typeof(byte?);
                            break;
                        case TypeCode.Char:
                            returnType = typeof(char?);
                            break;
                        case TypeCode.DateTime:
                            returnType = typeof(DateTime?);
                            break;
                        case TypeCode.Decimal:
                            returnType = typeof(decimal?);
                            break;
                        case TypeCode.Double:
                            returnType = typeof(double?);
                            break;
                        case TypeCode.Int16:
                            returnType = typeof(short?);
                            break;
                        case TypeCode.Int32:
                            returnType = typeof(int?);
                            break;
                        case TypeCode.Int64:
                            returnType = typeof(long?);
                            break;
                        case TypeCode.SByte:
                            returnType = typeof(sbyte?);
                            break;
                        case TypeCode.Single:
                            returnType = typeof(float?);
                            break;
                        case TypeCode.UInt16:
                            returnType = typeof(ushort?);
                            break;
                        case TypeCode.UInt32:
                            returnType = typeof(uint?);
                            break;
                        case TypeCode.UInt64:
                            returnType = typeof(ulong?);
                            break;
                        default:
                            // no special handling, so make it as usual
                            returnType = new RTTypeWrapper(this.typeProvider, this.runtimeType, typeArgs);
                            break;
                    }
                }
                else
                {
                    returnType = new RTTypeWrapper(this.typeProvider, this.runtimeType, typeArgs);
                }
                this.boundedTypes[typeArgs] = returnType;
            }
            return returnType;
        }

        public override Type MakeByRefType()
        {
            return this.typeProvider.GetType(this.FullName + "&");
        }

        public override Type MakePointerType()
        {
            return this.typeProvider.GetType(this.FullName + "*");
        }

        internal void OnAssemblyRemoved(Assembly removedAssembly)
        {
            ArrayList bindingArgs = new ArrayList(this.boundedTypes.Keys);
            foreach (Type[] types in bindingArgs)
            {
                foreach (Type type in types)
                {
                    if (type.Assembly == removedAssembly)
                    {
                        this.boundedTypes.Remove(types);
                        break;
                    }
                }
            }
        }

        #endregion

        #region implementation overrides

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            foreach (ConstructorInfo ctorInfo in this.runtimeType.GetConstructors(bindingAttr))
            {
                bool mismatch = false;
                if (types != null)
                {
                    ParameterInfo[] parameters = ctorInfo.GetParameters();
                    if (parameters.GetLength(0) == types.Length)
                    {
                        for (int index = 0; !mismatch && index < parameters.Length; index++)
                            mismatch = !IsAssignable(parameters[index].ParameterType, types[index]);
                    }
                    else
                    {
                        mismatch = true;
                    }
                }
                if (!mismatch)
                    return EnsureConstructorWrapped(ctorInfo);
            }
            return null;
        }
        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            foreach (MethodInfo method in this.runtimeType.GetMethods(bindingAttr))
            {
                bool matchName = ((bindingAttr & BindingFlags.IgnoreCase) == BindingFlags.IgnoreCase) ? string.Compare(method.Name, name, StringComparison.OrdinalIgnoreCase) == 0 : string.Compare(method.Name, name, StringComparison.Ordinal) == 0;
                if (matchName)
                {
                    bool mismatch = false;
                    if (types != null)
                    {
                        ParameterInfo[] parameters = method.GetParameters();
                        if (parameters.GetLength(0) == types.Length)
                        {
                            for (int index = 0; !mismatch && index < parameters.Length; index++)
                                mismatch = !IsAssignable(parameters[index].ParameterType, types[index]);
                        }
                        else
                        {
                            mismatch = true;
                        }
                    }
                    if (!mismatch)
                        return EnsureMethodWrapped(method);
                }
            }
            return null;
        }
        protected override PropertyInfo GetPropertyImpl(String name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            foreach (PropertyInfo propInfo in this.runtimeType.GetProperties(bindingAttr))
            {
                bool matchName = ((bindingAttr & BindingFlags.IgnoreCase) == BindingFlags.IgnoreCase) ? string.Compare(propInfo.Name, name, StringComparison.OrdinalIgnoreCase) == 0 : string.Compare(propInfo.Name, name, StringComparison.Ordinal) == 0;
                if (matchName && (returnType == null || (returnType.Equals(propInfo.PropertyType))))
                {
                    bool mismatch = false;
                    if (types != null)
                    {
                        ParameterInfo[] parameters = propInfo.GetIndexParameters();
                        if (parameters.GetLength(0) == types.Length)
                        {
                            for (int index = 0; !mismatch && index < parameters.Length; index++)
                                mismatch = !IsAssignable(parameters[index].ParameterType, types[index]);
                        }
                        else
                        {
                            mismatch = true;
                        }
                    }
                    if (!mismatch)
                        return EnsurePropertyWrapped(propInfo);
                }
            }
            return null;
        }
        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            return this.runtimeType.Attributes;
        }
        protected override bool HasElementTypeImpl()
        {
            return this.runtimeType.HasElementType;
        }
        public override int GetArrayRank()
        {
            return this.runtimeType.GetArrayRank();
        }
        protected override bool IsArrayImpl()
        {
            return this.runtimeType.IsArray;
        }
        protected override bool IsByRefImpl()
        {
            return this.runtimeType.IsByRef;
        }
        protected override bool IsCOMObjectImpl()
        {
            return this.runtimeType.IsCOMObject;
        }
        protected override bool IsContextfulImpl()
        {
            return this.runtimeType.IsContextful;
        }
        protected override bool IsMarshalByRefImpl()
        {
            return this.runtimeType.IsMarshalByRef;
        }
        protected override bool IsPointerImpl()
        {
            return this.runtimeType.IsPointer;
        }
        protected override bool IsPrimitiveImpl()
        {
            return this.runtimeType.IsPrimitive;
        }
        #endregion

        #region ICloneable Members
        public object Clone()
        {
            return this;
        }
        #endregion

        #region ConstructorInfo wrapper

        private class RTConstructorInfoWrapper : ConstructorInfo
        {
            private RTTypeWrapper rtTypeWrapper = null;
            private ConstructorInfo ctorInfo = null;
            private ParameterInfo[] wrappedParameters = null;
            public RTConstructorInfoWrapper(RTTypeWrapper rtTypeWrapper, ConstructorInfo ctorInfo)
            {
                this.rtTypeWrapper = rtTypeWrapper;
                this.ctorInfo = ctorInfo;
            }
            public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
            {
                return this.ctorInfo.Invoke(invokeAttr, binder, parameters, culture);
            }
            public override MethodAttributes Attributes
            {
                get { return this.ctorInfo.Attributes; }
            }
            public override MethodImplAttributes GetMethodImplementationFlags()
            {
                return this.ctorInfo.GetMethodImplementationFlags();
            }
            public override ParameterInfo[] GetParameters()
            {
                if (this.wrappedParameters == null)
                {
                    List<ParameterInfo> parameters = new List<ParameterInfo>();
                    foreach (ParameterInfo parameter in this.ctorInfo.GetParameters())
                        parameters.Add(new RTParameterInfoWrapper(this.rtTypeWrapper, this.ctorInfo, parameter));
                    this.wrappedParameters = parameters.ToArray();
                }
                return this.wrappedParameters;
            }
            public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
            {
                return this.ctorInfo.Invoke(obj, invokeAttr, binder, parameters, culture);
            }
            public override RuntimeMethodHandle MethodHandle
            {
                get { return this.ctorInfo.MethodHandle; }
            }
            public override Type DeclaringType
            {
                get { return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.ctorInfo.DeclaringType); }
            }
            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return this.ctorInfo.GetCustomAttributes(attributeType, inherit);
            }
            public override object[] GetCustomAttributes(bool inherit)
            {
                return this.ctorInfo.GetCustomAttributes(inherit);
            }
            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return this.ctorInfo.IsDefined(attributeType, inherit);
            }
            public override MemberTypes MemberType
            {
                get { return this.ctorInfo.MemberType; }
            }
            public override string Name
            {
                get { return this.ctorInfo.Name; }
            }
            public override Type ReflectedType
            {
                get { return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.ctorInfo.ReflectedType); }
            }
        }

        #endregion

        #region FieldInfo Wrapper

        private class RTFieldInfoWrapper : FieldInfo
        {
            private RTTypeWrapper rtTypeWrapper = null;
            private FieldInfo fieldInfo = null;
            public RTFieldInfoWrapper(RTTypeWrapper rtTypeWrapper, FieldInfo fieldInfo)
            {
                this.rtTypeWrapper = rtTypeWrapper;
                this.fieldInfo = fieldInfo;
            }
            public override int MetadataToken
            {
                get { return this.fieldInfo.MetadataToken; }
            }
            public override Module Module
            {
                get { return this.fieldInfo.Module; }
            }
            public override FieldAttributes Attributes
            {
                get { return this.fieldInfo.Attributes; }
            }
            public override RuntimeFieldHandle FieldHandle
            {
                get { return this.fieldInfo.FieldHandle; }
            }
            public override Type FieldType
            {
                get { return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.fieldInfo.FieldType); }
            }
            public override object GetValue(object obj)
            {
                return this.fieldInfo.GetValue(obj);
            }
            public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
            {
                this.fieldInfo.SetValue(obj, value, invokeAttr, binder, culture);
            }
            public override Type DeclaringType
            {
                get { return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.fieldInfo.DeclaringType); }
            }
            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return this.fieldInfo.GetCustomAttributes(attributeType, inherit);
            }
            public override object[] GetCustomAttributes(bool inherit)
            {
                return this.fieldInfo.GetCustomAttributes(inherit);
            }
            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return this.fieldInfo.IsDefined(attributeType, inherit);
            }
            public override MemberTypes MemberType
            {
                get { return this.fieldInfo.MemberType; }
            }
            public override string Name
            {
                get { return this.fieldInfo.Name; }
            }
            public override Type ReflectedType
            {
                get { return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.fieldInfo.ReflectedType); }
            }
        }

        #endregion

        #region PropertyInfo Wrapper

        private class RTPropertyInfoWrapper : PropertyInfo
        {
            private PropertyInfo propertyInfo = null;
            private RTTypeWrapper rtTypeWrapper = null;
            private ParameterInfo[] wrappedParameters = null;
            public RTPropertyInfoWrapper(RTTypeWrapper rtTypeWrapper, PropertyInfo propertyInfo)
            {
                this.rtTypeWrapper = rtTypeWrapper;
                this.propertyInfo = propertyInfo;
            }

            public override PropertyAttributes Attributes
            {
                get { return this.propertyInfo.Attributes; }
            }

            public override bool CanRead
            {
                get { return this.propertyInfo.CanRead; }
            }

            public override bool CanWrite
            {
                get { return this.propertyInfo.CanWrite; }
            }
            public override MethodInfo[] GetAccessors(bool nonPublic)
            {
                List<MethodInfo> methods = new List<MethodInfo>();
                foreach (MethodInfo methodInfo in this.propertyInfo.GetAccessors(nonPublic))
                    methods.Add(this.rtTypeWrapper.EnsureMethodWrapped(methodInfo));
                return methods.ToArray();
            }
            public override MethodInfo GetGetMethod(bool nonPublic)
            {
                MethodInfo methodInfo = this.propertyInfo.GetGetMethod(nonPublic);
                if (methodInfo == null)
                    return null;
                return this.rtTypeWrapper.EnsureMethodWrapped(methodInfo);
            }
            public override ParameterInfo[] GetIndexParameters()
            {
                if (this.wrappedParameters == null)
                {
                    List<ParameterInfo> parameters = new List<ParameterInfo>();
                    foreach (ParameterInfo parameter in this.propertyInfo.GetIndexParameters())
                        parameters.Add(new RTParameterInfoWrapper(this.rtTypeWrapper, this.propertyInfo, parameter));
                    this.wrappedParameters = parameters.ToArray();
                }
                return this.wrappedParameters;
            }
            public override MethodInfo GetSetMethod(bool nonPublic)
            {
                MethodInfo methodInfo = this.propertyInfo.GetSetMethod(nonPublic);
                if (methodInfo == null)
                    return null;
                return this.rtTypeWrapper.EnsureMethodWrapped(methodInfo);
            }
            public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
            {
                return this.propertyInfo.GetValue(obj, invokeAttr, binder, index, culture);
            }
            public override Type PropertyType
            {
                get { return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.propertyInfo.PropertyType); }
            }
            public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
            {
                this.propertyInfo.SetValue(obj, value, invokeAttr, binder, index, culture);
            }
            public override Type DeclaringType
            {
                get { return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.propertyInfo.DeclaringType); }
            }
            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return this.propertyInfo.GetCustomAttributes(attributeType, inherit);
            }
            public override object[] GetCustomAttributes(bool inherit)
            {
                return this.propertyInfo.GetCustomAttributes(inherit);
            }
            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return this.propertyInfo.IsDefined(attributeType, inherit);
            }
            public override MemberTypes MemberType
            {
                get { return this.propertyInfo.MemberType; }
            }
            public override string Name
            {
                get { return this.propertyInfo.Name; }
            }
            public override Type ReflectedType
            {
                get { return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.propertyInfo.ReflectedType); }
            }
            public override int MetadataToken
            {
                get { return this.propertyInfo.MetadataToken; }
            }

            public override Module Module
            {
                get { return this.propertyInfo.Module; }
            }
        }

        #endregion

        #region MethodInfo Wrapper

        private class RTMethodInfoWrapper : MethodInfo
        {
            private MethodInfo methodInfo = null;
            private RTTypeWrapper rtTypeWrapper = null;
            private ParameterInfo[] wrappedParameters = null;
            public RTMethodInfoWrapper(RTTypeWrapper rtTypeWrapper, MethodInfo methodInfo)
            {
                this.rtTypeWrapper = rtTypeWrapper;
                this.methodInfo = methodInfo;
            }
            public override Module Module
            {
                get { return this.methodInfo.Module; }
            }
            public override MethodBody GetMethodBody()
            {
                return this.methodInfo.GetMethodBody();
            }
            public override int MetadataToken
            {
                get { return this.methodInfo.MetadataToken; }
            }
            public override CallingConventions CallingConvention
            {
                get { return this.methodInfo.CallingConvention; }
            }
            public override ParameterInfo ReturnParameter
            {
                get { return new RTParameterInfoWrapper(this.rtTypeWrapper, this, this.methodInfo.ReturnParameter); }
            }
            public override MethodInfo GetBaseDefinition()
            {
                return this.methodInfo.GetBaseDefinition();
            }
            public override Type ReturnType
            {
                get
                {
                    return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.methodInfo.ReturnType);
                }
            }
            public override ICustomAttributeProvider ReturnTypeCustomAttributes
            {
                get { return this.methodInfo.ReturnTypeCustomAttributes; }
            }
            public override MethodAttributes Attributes
            {
                get { return this.methodInfo.Attributes; }
            }
            public override MethodImplAttributes GetMethodImplementationFlags()
            {
                return this.methodInfo.GetMethodImplementationFlags();
            }
            public override ParameterInfo[] GetParameters()
            {
                if (this.wrappedParameters == null)
                {
                    List<ParameterInfo> parameters = new List<ParameterInfo>();
                    foreach (ParameterInfo parameter in this.methodInfo.GetParameters())
                        parameters.Add(new RTParameterInfoWrapper(this.rtTypeWrapper, this.methodInfo, parameter));
                    this.wrappedParameters = parameters.ToArray();
                }
                return this.wrappedParameters;
            }
            public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
            {
                return this.methodInfo.Invoke(obj, invokeAttr, binder, parameters, culture);
            }
            public override RuntimeMethodHandle MethodHandle
            {
                get { return this.methodInfo.MethodHandle; }
            }

            public override Type DeclaringType
            {
                get
                {
                    return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.methodInfo.DeclaringType);
                }
            }
            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return this.methodInfo.GetCustomAttributes(attributeType, inherit);
            }
            public override object[] GetCustomAttributes(bool inherit)
            {
                return this.methodInfo.GetCustomAttributes(inherit);
            }
            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return this.methodInfo.IsDefined(attributeType, inherit);
            }
            public override MemberTypes MemberType
            {
                get { return this.methodInfo.MemberType; }
            }
            public override string Name
            {
                get { return this.methodInfo.Name; }
            }
            public override Type ReflectedType
            {
                get
                {
                    return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.methodInfo.ReflectedType);
                }
            }
        }

        #endregion

        #region EventInfo Wrapper

        private class RTEventInfoWrapper : EventInfo
        {
            private RTTypeWrapper rtTypeWrapper = null;
            private EventInfo eventInfo = null;
            public RTEventInfoWrapper(RTTypeWrapper rtTypeWrapper, EventInfo eventInfo)
            {
                this.rtTypeWrapper = rtTypeWrapper;
                this.eventInfo = eventInfo;
            }
            public override EventAttributes Attributes
            {
                get { return this.eventInfo.Attributes; }
            }
            public override MethodInfo GetAddMethod(bool nonPublic)
            {
                MethodInfo methodInfo = this.eventInfo.GetAddMethod(nonPublic);
                if (methodInfo == null)
                    return null;
                return this.rtTypeWrapper.EnsureMethodWrapped(methodInfo);
            }
            public override MethodInfo GetRaiseMethod(bool nonPublic)
            {
                MethodInfo methodInfo = this.eventInfo.GetRaiseMethod(nonPublic);
                if (methodInfo == null)
                    return null;
                return this.rtTypeWrapper.EnsureMethodWrapped(methodInfo);
            }
            public override MethodInfo GetRemoveMethod(bool nonPublic)
            {
                MethodInfo methodInfo = this.eventInfo.GetRemoveMethod(nonPublic);
                if (methodInfo == null)
                    return null;
                return this.rtTypeWrapper.EnsureMethodWrapped(methodInfo);
            }
            public override Type DeclaringType
            {
                get { return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.eventInfo.DeclaringType); }
            }
            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return this.eventInfo.GetCustomAttributes(attributeType, inherit);
            }
            public override object[] GetCustomAttributes(bool inherit)
            {
                return this.eventInfo.GetCustomAttributes(inherit);
            }
            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return this.eventInfo.IsDefined(attributeType, inherit);
            }
            public override MemberTypes MemberType
            {
                get { return this.eventInfo.MemberType; }
            }
            public override string Name
            {
                get { return this.eventInfo.Name; }
            }
            public override Type ReflectedType
            {
                get { return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.eventInfo.ReflectedType); }
            }

            public override int MetadataToken
            {
                get { return this.eventInfo.MetadataToken; }
            }
            public override Module Module
            {
                get { return this.eventInfo.Module; }
            }
        }

        #endregion

        #region ParameterInfo wrapper

        private class RTParameterInfoWrapper : ParameterInfo
        {
            private RTTypeWrapper rtTypeWrapper = null;
            private ParameterInfo paramInfo = null;
            private MemberInfo parentMember = null;

            public RTParameterInfoWrapper(RTTypeWrapper rtTypeWrapper, MemberInfo parentMember, ParameterInfo paramInfo)
            {
                this.parentMember = parentMember;
                this.rtTypeWrapper = rtTypeWrapper;
                this.paramInfo = paramInfo;
            }
            public override ParameterAttributes Attributes
            {
                get { return this.paramInfo.Attributes; }
            }
            public override object[] GetCustomAttributes(bool inherit)
            {
                return this.paramInfo.GetCustomAttributes(inherit);
            }
            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return this.paramInfo.IsDefined(attributeType, inherit);
            }
            public override MemberInfo Member
            {
                get { return this.parentMember; }
            }
            public override Type[] GetOptionalCustomModifiers()
            {
                return this.paramInfo.GetOptionalCustomModifiers();
            }
            public override string Name
            {
                get { return this.paramInfo.Name; }
            }
            public override Type ParameterType
            {
                get { return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.paramInfo.ParameterType); }
            }
            public override int Position
            {
                get { return this.paramInfo.Position; }
            }
            public override Type[] GetRequiredCustomModifiers()
            {
                return this.paramInfo.GetRequiredCustomModifiers();
            }
            public override object DefaultValue
            {
                get
                {
#pragma warning suppress 56503
                    throw new global::System.NotImplementedException();
                }
            }
        }
        #endregion
    }
}
