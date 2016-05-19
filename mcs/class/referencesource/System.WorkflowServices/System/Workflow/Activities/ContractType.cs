//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

#pragma warning disable 1634, 1691
namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.ServiceModel;

    internal sealed class ContractType : Type, ICloneable
    {
        private static readonly char[] elementDecorators = new char[] { '[', '*', '&' };
        private static readonly char[] nameSeparators = new char[] { '.', '+' };

        private Attribute[] attributes = null;
        private ConstructorInfo[] constructors = null;
        private EventInfo[] events = null;
        private FieldInfo[] fields = null;
        private string fullName;
        private Guid guid = Guid.Empty;
        private MethodInfo[] methods = null;

        private string name;
        private Type[] nestedTypes = Type.EmptyTypes;
        private PropertyInfo[] properties = null;
        private TypeAttributes typeAttributes;

        internal ContractType(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                    "name", SR2.GetString(SR2.Error_ArgumentValueNullOrEmptyString));
            }

            this.fullName = name;

            this.name = this.fullName;

            // detect first bracket, any name seperators after it are part of a generic parameter...  
            int idx = name.IndexOf('[');

            // Get the name after the last dot
            if (idx != -1)
            {
                idx = this.name.Substring(0, idx).LastIndexOfAny(nameSeparators);
            }
            else
            {
                idx = this.name.LastIndexOfAny(nameSeparators);
            }

            if (idx != -1)
            {
                this.name = this.fullName.Substring(idx + 1);
            }

            this.typeAttributes = TypeAttributes.Interface |
                TypeAttributes.Sealed |
                TypeAttributes.Public |
                TypeAttributes.Abstract;

            this.attributes = new Attribute[] { new ServiceContractAttribute() };
            this.methods = new MethodInfo[0];
        }

        public override Assembly Assembly
        {
            get
            {
                return null;
            }
        }

        public override string AssemblyQualifiedName
        {
            get
            {
                return this.FullName;
            }
        }

        public override Type BaseType
        {
            get
            {
                return null;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return null;
            }
        }

        public override string FullName
        {
            get
            {
                return this.fullName;
            }
        }

        public override Guid GUID
        {
            get
            {
                if (this.guid == Guid.Empty)
                {
                    this.guid = Guid.NewGuid();
                }

                return this.guid;
            }
        }

        public override Module Module
        {
            get
            {
                return null;
            }
        }

        public override string Name
        {
            get
            {
                return this.name;
            }
        }

        public override string Namespace
        {
            get
            {
                if (this.fullName == Name)
                {
                    return string.Empty;
                }

                return this.fullName.Substring(0, this.fullName.Length - Name.Length - 1);
            }
        }

        public override RuntimeTypeHandle TypeHandle
        {
            get
            {
#pragma warning suppress 56503
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new NotImplementedException(SR2.GetString(SR2.Error_RuntimeNotSupported)));
            }
        }

        public override Type UnderlyingSystemType
        {
            get
            {
                return this;
            }
        }

        public object Clone()
        {
            return this;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            ContractType contract = obj as ContractType;
            if (contract == null)
            {
                return false;
            }

            if (string.Compare(this.AssemblyQualifiedName, contract.AssemblyQualifiedName, StringComparison.Ordinal) != 0 ||
                this.methods.Length != contract.methods.Length)
            {
                return false;
            }

            foreach (MethodInfo methodInfo in this.methods)
            {
                if (this.GetMemberHelper<MethodInfo>(BindingFlags.Public | BindingFlags.Instance,
                    new MemberSignature(methodInfo),
                    ref contract.methods) == null)
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetArrayRank()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                SR2.GetString(SR2.Error_CurrentTypeNotAnArray));
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            return GetMembersHelper<ConstructorInfo>(bindingAttr, ref this.constructors, false);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return GetCustomAttributes(typeof(object), inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("attributeType");
            }

            return ServiceOperationHelpers.GetCustomAttributes(attributeType, this.attributes);
        }

        public override MemberInfo[] GetDefaultMembers()
        {
            // Get all of the custom attributes
            DefaultMemberAttribute attr = null;

            for (Type t = this; t != null; t = t.BaseType)
            {
                object[] attrs = GetCustomAttributes(typeof(DefaultMemberAttribute), false);
                if (attrs != null && attrs.Length > 0)
                {
                    attr = attrs[0] as DefaultMemberAttribute;
                }

                if (attr != null)
                {
                    break;
                }
            }

            if (attr == null)
            {
                return new MemberInfo[0];
            }

            String defaultMember = attr.MemberName;
            MemberInfo[] members = GetMember(defaultMember);
            if (members == null)
            {
                members = new MemberInfo[0];
            }
            return members;
        }

        public override Type GetElementType()
        {
            return null;
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            return GetMemberHelper<EventInfo>(bindingAttr, new MemberSignature(name, null, null), ref this.events);
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            return GetMembersHelper<EventInfo>(bindingAttr, ref this.events, true);
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            return GetMemberHelper<FieldInfo>(bindingAttr, new MemberSignature(name, null, null), ref this.fields);
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            return GetMembersHelper<FieldInfo>(bindingAttr, ref this.fields, true);
        }

        public override int GetHashCode()
        {
            return this.name.GetHashCode();
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                    "name", SR2.GetString(SR2.Error_ArgumentValueNullOrEmptyString));
            }

            if (string.Compare(this.name, name, StringComparison.Ordinal) == 0)
            {
                return this;
            }
            return null;
        }

        public override Type[] GetInterfaces()
        {
            return Type.EmptyTypes;
        }

        public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
        {
            List<MemberInfo> members = new List<MemberInfo>();

            // Methods
            if ((type & MemberTypes.Method) != 0)
            {
                members.AddRange(GetMembersHelper<MethodInfo>(bindingAttr, new MemberSignature(name, null, null), ref this.methods));
            }

            // Constructors
            if ((type & MemberTypes.Constructor) != 0)
            {
                members.AddRange(GetMembersHelper<ConstructorInfo>(bindingAttr, new MemberSignature(name, null, null), ref this.constructors));
            }

            // Properties
            if ((type & MemberTypes.Property) != 0)
            {
                members.AddRange(GetMembersHelper<PropertyInfo>(bindingAttr, new MemberSignature(name, null, null), ref this.properties));
            }

            // Events
            if ((type & MemberTypes.Event) != 0)
            {
                members.AddRange(GetMembersHelper<EventInfo>(bindingAttr, new MemberSignature(name, null, null), ref this.events));
            }

            // Fields
            if ((type & MemberTypes.Field) != 0)
            {
                members.AddRange(GetMembersHelper<FieldInfo>(bindingAttr, new MemberSignature(name, null, null), ref this.fields));
            }

            // Nested types
            if ((type & MemberTypes.NestedType) != 0)
            {
                members.AddRange(GetMembersHelper<Type>(bindingAttr, new MemberSignature(name, null, null), ref this.nestedTypes));
            }

            return members.ToArray();
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            ArrayList members = new ArrayList();

            members.AddRange(GetMethods(bindingAttr));
            members.AddRange(GetProperties(bindingAttr));
            members.AddRange(GetEvents(bindingAttr));
            members.AddRange(GetFields(bindingAttr));
            members.AddRange(GetNestedTypes(bindingAttr));

            return (MemberInfo[])members.ToArray(typeof(MemberInfo));
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            return GetMembersHelper<MethodInfo>(bindingAttr, ref this.methods, true);
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            return null;
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            return Type.EmptyTypes;
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            return GetMembersHelper<PropertyInfo>(bindingAttr, ref this.properties, true);
        }

        public override object InvokeMember(string name, BindingFlags bindingFlags, Binder binder, object target, object[] providedArgs, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParams)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                new NotImplementedException(SR2.GetString(SR2.Error_RuntimeNotSupported)));
        }

        public override bool IsAssignableFrom(Type type)
        {
            if (type == null)
            {
                return false;
            }
            if (!(type is ContractType))
            {
                return false;
            }
            return (this.Equals((Object)type));
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("attributeType");
            }
            return ServiceOperationHelpers.IsDefined(attributeType, attributes);
        }

        public override bool IsSubclassOf(Type type)
        {
            return false;
        }

        public override Type MakeByRefType()
        {
            return this;
        }

        public override string ToString()
        {
            return this.FullName;
        }

        internal void AddMethod(ContractMethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("methodInfo");
            }

            MemberSignature signature = new MemberSignature(methodInfo);
            if (this.GetMemberHelper<MethodInfo>(BindingFlags.Public | BindingFlags.Instance,
                signature,
                ref this.methods) != null)
            {
                return;
            }
            else
            {
                List<MethodInfo> localMethods = new List<MethodInfo>();
                if (this.methods != null)
                {
                    localMethods.AddRange(this.methods);
                }
                localMethods.Add(methodInfo);
                this.methods = new MethodInfo[localMethods.Count];
                localMethods.CopyTo(this.methods);
            }
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            return typeAttributes;
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            return GetMemberHelper<ConstructorInfo>(bindingAttr, new MemberSignature(null, types, null), ref this.constructors);
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            return GetMemberHelper<MethodInfo>(bindingAttr, new MemberSignature(name, types, null), ref this.methods);
        }

        protected override PropertyInfo GetPropertyImpl(String name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            return GetMemberHelper<PropertyInfo>(bindingAttr, new MemberSignature(name, types, null), ref this.properties);
        }

        protected override bool HasElementTypeImpl()
        {

            int elementCharPosition = Name.LastIndexOfAny(elementDecorators);
            return (elementCharPosition != -1);
        }

        protected override bool IsArrayImpl()
        {
            int elementCharPosition = Name.LastIndexOfAny(elementDecorators);
            if ((elementCharPosition != -1) && (Name[elementCharPosition] == '['))
            {
                return true;
            }

            return false;

        }

        protected override bool IsByRefImpl()
        {
            return false;
        }


        protected override bool IsCOMObjectImpl()
        {
            return false;
        }

        protected override bool IsContextfulImpl()
        {
            return false;
        }

        protected override bool IsMarshalByRefImpl()
        {
            return false;
        }

        protected override bool IsPointerImpl()
        {
            return (this.name[this.name.Length - 1] == '*');
        }

        protected override bool IsPrimitiveImpl()
        {
            return false;
        }

        private bool FilterMember(MemberInfo memberInfo, BindingFlags bindingFlags)
        {
            bool isPublic = false;
            bool isStatic = false;

            if (this.IsInterface)
            {
                isPublic = true;
                isStatic = false;
            }
            else if (memberInfo is MethodBase)
            {
                isPublic = (memberInfo as MethodBase).IsPublic;
                isStatic = (memberInfo as MethodBase).IsStatic;
            }
            else if (memberInfo is FieldInfo)
            {
                isPublic = (memberInfo as FieldInfo).IsPublic;
                isStatic = (memberInfo as FieldInfo).IsStatic;
            }
            else if (memberInfo is PropertyInfo)
            {
                // Property public\static attributes can be fetched using the accessors 
                PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                MethodInfo accessorMethod = null;
                if (propertyInfo.CanRead)
                {
                    accessorMethod = propertyInfo.GetGetMethod(true);
                }
                else
                {
                    accessorMethod = propertyInfo.GetSetMethod(true);
                }
                if (accessorMethod != null)
                {
                    isPublic = accessorMethod.IsPublic;
                    isStatic = accessorMethod.IsStatic;
                }
            }
            else if (memberInfo is Type)
            {
                isPublic = (memberInfo as Type).IsPublic || (memberInfo as Type).IsNestedPublic;
                // No static check.
                return ((((isPublic) && ((bindingFlags & BindingFlags.Public) != 0)) || ((!isPublic) && ((bindingFlags & BindingFlags.NonPublic) != 0))));
            }

            return ((((isPublic) && ((bindingFlags & BindingFlags.Public) != 0)) || ((!isPublic) && ((bindingFlags & BindingFlags.NonPublic) != 0))) && (((isStatic) && ((bindingFlags & BindingFlags.Static) != 0)) || ((!isStatic) && ((bindingFlags & BindingFlags.Instance) != 0))));
        }

        //private MemberInfo[] GetBaseMembers(Type type, Type baseType, BindingFlags bindingAttr)
        //{
        //    MemberInfo[] members = null;
        //    if (type == typeof(PropertyInfo))
        //    {
        //        members = baseType.GetProperties(bindingAttr);
        //    }
        //    else if (type == typeof(EventInfo))
        //    {
        //        members = baseType.GetEvents(bindingAttr);
        //    }
        //    else if (type == typeof(ConstructorInfo))
        //    {
        //        members = baseType.GetConstructors(bindingAttr);
        //    }
        //    else if (type == typeof(MethodInfo))
        //    {
        //        members = baseType.GetMethods(bindingAttr);
        //    }
        //    else if (type == typeof(FieldInfo))
        //    {
        //        members = baseType.GetFields(bindingAttr);
        //    }
        //    else if (type == typeof(Type))
        //    {
        //        members = baseType.GetNestedTypes(bindingAttr);
        //    }

        //    return members;
        //}

        // generic method that implements all GetXXX methods
        private T GetMemberHelper<T>(BindingFlags bindingAttr, MemberSignature memberSignature, ref T[] members)
            where T : MemberInfo
        {
            if (members != null)
            {
                // search the local type
                foreach (T memberInfo in members)
                {
                    MemberSignature candididateMemberSignature = new MemberSignature(memberInfo);
                    if (candididateMemberSignature.FilterSignature(memberSignature) && FilterMember(memberInfo, bindingAttr))
                    {
                        return memberInfo;
                    }
                }
            }

            return null;
        }

        // generic method that implements all GetXXXs methods
        private T[] GetMembersHelper<T>(BindingFlags bindingAttr, ref T[] members, bool searchBase)
            where T : MemberInfo
        {
            Dictionary<MemberSignature, T> membersDictionary = new Dictionary<MemberSignature, T>();

            if (members != null)
            {
                // get local properties
                foreach (T memberInfo in members)
                {
                    MemberSignature memberSignature = new MemberSignature(memberInfo);

                    if ((FilterMember(memberInfo, bindingAttr)) && (!membersDictionary.ContainsKey(memberSignature)))
                    {
                        membersDictionary.Add(new MemberSignature(memberInfo), memberInfo);
                    }
                }
            }

            if (searchBase && (bindingAttr & BindingFlags.DeclaredOnly) == 0)
            {
                // FlattenHierarchy is required to return static members from base classes.
                if ((bindingAttr & BindingFlags.FlattenHierarchy) == 0)
                {
                    bindingAttr &= ~BindingFlags.Static;
                }

                //    Type baseType = BaseType;
                //    if (baseType != null)
                //    {
                //        T[] baseMembers = GetBaseMembers(typeof(T), baseType, bindingAttr) as T[];

                //        foreach (T memberInfo in baseMembers)
                //        {
                //            // We should not return private members from base classes. Note: Generics requires us to use "as".
                //            if ((memberInfo is FieldInfo && (memberInfo as FieldInfo).IsPrivate) || (memberInfo is MethodBase && (memberInfo as MethodBase).IsPrivate) || (memberInfo is Type && (memberInfo as Type).IsNestedPrivate))
                //            {
                //                continue;
                //            }

                //            // verify a member with this signature was not already created
                //            MemberSignature memberSignature = new MemberSignature(memberInfo);

                //            if (!membersDictionary.ContainsKey(memberSignature))
                //            {
                //                membersDictionary.Add(memberSignature, memberInfo);
                //            }
                //        }
                //    }
            }

            List<T> memberCollection = new List<T>(membersDictionary.Values);
            return memberCollection.ToArray();
        }

        private T[] GetMembersHelper<T>(BindingFlags bindingAttr, MemberSignature memberSignature, ref T[] members)
            where T : MemberInfo
        {
            List<T> memberCandidates = new List<T>();
            foreach (T memberInfo in this.GetMembersHelper<T>(bindingAttr, ref members, true))
            {
                MemberSignature candididateMemberSignature = new MemberSignature(memberInfo);
                if (candididateMemberSignature.FilterSignature(memberSignature))
                {
                    memberCandidates.Add(memberInfo);
                }
            }
            return memberCandidates.ToArray();
        }

        internal class MemberSignature
        {
            private string name = null;
            private Type[] parameters = null;
            private Type returnType = null;

            internal MemberSignature(MemberInfo memberInfo)
            {
                this.name = memberInfo.Name;

                if (memberInfo is MethodBase)
                {
                    List<Type> typeCollection = new List<Type>();

                    // method/constructor arguments
                    foreach (ParameterInfo parameterInfo in (memberInfo as MethodBase).GetParameters())
                    {
                        typeCollection.Add(parameterInfo.ParameterType);
                    }

                    this.parameters = typeCollection.ToArray();

                    if (memberInfo is MethodInfo)
                    {
                        this.returnType = ((MethodInfo)memberInfo).ReturnType;
                    }
                }
                else if (memberInfo is PropertyInfo)
                {
                    PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                    List<Type> typeCollection = new List<Type>();

                    // indexer arguments
                    foreach (ParameterInfo parameterInfo in propertyInfo.GetIndexParameters())
                    {
                        typeCollection.Add(parameterInfo.ParameterType);
                    }

                    this.parameters = typeCollection.ToArray();

                    // return type for property
                    this.returnType = propertyInfo.PropertyType;
                }

            }

            internal MemberSignature(string name, Type[] parameters, Type returnType)
            {
                this.name = name;
                this.returnType = returnType;
                if (parameters != null)
                {
                    this.parameters = (Type[])parameters.Clone();
                }
            }

            public string Name
            {
                get
                {
                    return name;
                }
            }

            public Type[] Parameters
            {
                get
                {
                    if (parameters == null)
                    {
                        return null;
                    }

                    return (Type[])parameters.Clone();
                }
            }

            public Type ReturnType
            {
                get
                {
                    return returnType;
                }
            }

            public override bool Equals(object obj)
            {
                MemberSignature memberSignature = obj as MemberSignature;

                if ((memberSignature == null) ||
                    (this.name != memberSignature.Name) ||
                    (this.returnType != memberSignature.ReturnType))
                {
                    return false;
                }

                if ((this.Parameters == null) && (memberSignature.Parameters != null) ||
                    (this.Parameters != null) && (memberSignature.Parameters == null))
                {
                    return false;
                }

                if (this.Parameters != null)
                {
                    if (this.parameters.Length != memberSignature.parameters.Length)
                    {
                        return false;
                    }

                    for (int loop = 0; loop < this.parameters.Length; loop++)
                    {
                        if (this.parameters[loop] != memberSignature.parameters[loop])
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            // this method will filter using a mask signautre. only non-null mask members are used to filter 
            // the signature, the rest are ignored
            public bool FilterSignature(MemberSignature maskSignature)
            {
                if (maskSignature == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("maskSignature");
                }

                if (((maskSignature.Name != null) && (this.name != maskSignature.name)) ||
                    ((maskSignature.returnType != null) && (this.returnType != maskSignature.returnType)))
                {
                    return false;
                }

                if (maskSignature.parameters != null)
                {
                    if (this.parameters == null)
                    {
                        return false;
                    }

                    if (this.parameters.Length != maskSignature.parameters.Length)
                    {
                        return false;
                    }

                    for (int loop = 0; loop < this.parameters.Length; loop++)
                    {
                        if (!this.parameters[loop].Equals(maskSignature.parameters[loop]))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

            public override string ToString()
            {
                string str = string.Empty;

                if (returnType != null)
                {
                    str = returnType.FullName + " ";
                }

                if (name != null && name.Length != 0)
                {
                    str += name;
                }

                if (parameters != null && parameters.Length > 0)
                {
                    str += "(";

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (i > 0)
                        {
                            str += ", ";
                        }

                        if (parameters[i] != null)
                        {
                            if (parameters[i].GetType() != null && parameters[i].GetType().IsByRef)
                            {
                                str += "ref ";
                            }

                            str += parameters[i].FullName;
                        }
                    }

                    str += ")";
                }

                return str;
            }
        }
    }
}
