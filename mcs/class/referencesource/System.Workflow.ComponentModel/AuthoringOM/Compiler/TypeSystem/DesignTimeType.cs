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
    using System.Diagnostics;
    using System.Text;

    internal sealed class DesignTimeType : Type, ICloneable
    {
        #region Members and Constructors

        private static readonly char[] nameSeparators = new char[] { '.', '+' };
        private static readonly char[] elementDecorators = new char[] { '[', '*', '&' };

        private Type declaringType;
        private string fullName;
        private TypeAttributes typeAttributes;
        private ITypeProvider typeProvider;

        private Attribute[] attributes = null;
        private ConstructorInfo[] constructors = null;
        private FieldInfo[] fields = null;
        private EventInfo[] events = null;
        private PropertyInfo[] properties = null;
        private MethodInfo[] methods = null;
        private Type[] nestedTypes = new Type[0];
        private List<CodeTypeDeclaration> codeDomTypes = null; // accounting for partial types
        private CodeNamespaceImportCollection codeNamespaceImports = null;
        private Guid guid = Guid.Empty;

        internal DesignTimeType(Type declaringType,
            string typeName,
            CodeNamespaceImportCollection codeNamespaceImports,
            string namespaceName,
            ITypeProvider typeProvider)
        {
            if (typeName == null)
                throw new ArgumentNullException("typeName");

            if (codeNamespaceImports == null)
                throw new ArgumentNullException("codeNamespaceImports");

            if (typeProvider == null)
                throw new ArgumentNullException("typeProvider");

            if (namespaceName == null && declaringType == null)
                throw new InvalidOperationException(SR.GetString(SR.NamespaceAndDeclaringTypeCannotBeNull));

            typeName = Helper.EnsureTypeName(typeName);
            namespaceName = Helper.EnsureTypeName(namespaceName);

            if (declaringType == null)
            {
                if (namespaceName.Length == 0)
                    this.fullName = typeName;
                else
                    this.fullName = namespaceName + "." + typeName;
            }
            else
            {
                this.fullName = declaringType.FullName + "+" + typeName;
            }

            this.codeDomTypes = new List<CodeTypeDeclaration>();
            this.codeNamespaceImports = codeNamespaceImports;
            this.typeProvider = typeProvider;
            this.declaringType = declaringType;
            this.typeAttributes = default(TypeAttributes);
        }


        internal DesignTimeType(Type declaringType, string elementTypeFullName, ITypeProvider typeProvider)
        {
            // constructor for declaring types with element (Arrays, Pointers, ByRef). 
            if (typeProvider == null)
                throw new ArgumentNullException("typeProvider");

            if (elementTypeFullName.LastIndexOfAny(elementDecorators) == -1)
                throw new ArgumentException(SR.GetString(SR.NotElementType), "elementTypeFullName");

            if (elementTypeFullName == null)
                throw new ArgumentNullException("FullName");

            this.fullName = Helper.EnsureTypeName(elementTypeFullName);
            this.codeDomTypes = null;
            this.nestedTypes = new Type[0];
            this.codeNamespaceImports = null;
            this.typeProvider = typeProvider;
            this.declaringType = declaringType;

            // Set Attributes according to the element type attributes
            Type elementType = GetElementType();
            if (elementType == null)
                throw new ArgumentException(SR.GetString(SR.NotElementType), "elementTypeFullName");

            if (IsArray)
            {
                this.typeAttributes = elementType.Attributes & TypeAttributes.VisibilityMask | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Serializable;
            }
            else
            {
                // Pointer/ByRef attributes
                // 
                this.typeAttributes = TypeAttributes.AnsiClass;
            }
        }

        #endregion

        internal ITypeProvider Provider
        {
            get
            {
                return this.typeProvider;
            }
        }

        internal void AddCodeTypeDeclaration(CodeTypeDeclaration codeDomType)
        {
            if (codeDomType == null)
                throw new ArgumentNullException("codeDomType");

            //
            this.typeAttributes |= codeDomType.TypeAttributes & ~TypeAttributes.Public;

            this.typeAttributes |= Helper.ConvertToTypeAttributes(codeDomType.Attributes, this.declaringType);
            foreach (CodeAttributeDeclaration attribute in codeDomType.CustomAttributes)
            {
                if (string.Equals(attribute.Name, "System.SerializableAttribute", StringComparison.Ordinal) || string.Equals(attribute.Name, "System.Serializable", StringComparison.Ordinal) || string.Equals(attribute.Name, "SerializableAttribute", StringComparison.Ordinal) || string.Equals(attribute.Name, "Serializable", StringComparison.Ordinal))
                {
                    this.typeAttributes |= TypeAttributes.Serializable;
                    break;
                }
            }
            codeDomTypes.Add(codeDomType);

            this.attributes = null;
            this.constructors = null;
            this.fields = null;
            this.events = null;
            this.properties = null;
            this.methods = null;

            LoadNestedTypes(codeDomType);
        }

        #region Properties
        public override Assembly Assembly
        {
            get
            {
                // We can't provide an assembly. This is a design time only type
                return null;
            }
        }

        public override string AssemblyQualifiedName
        {
            get
            {
                // We can't provide an assembly. This is a design time only type
                return this.FullName;
            }
        }

        public override Type BaseType
        {
            get
            {
                Type type = null;

                if (this.codeDomTypes != null)
                {
                    foreach (CodeTypeDeclaration codeDomType in this.codeDomTypes)
                    {
                        // Look for candidates in the type
                        foreach (CodeTypeReference codeBaseType in codeDomType.BaseTypes)
                        {
                            Type typeCandidate = ResolveType(GetTypeNameFromCodeTypeReference(codeBaseType, this));

                            if ((typeCandidate != null) && (!typeCandidate.IsInterface))
                            {
                                type = typeCandidate;
                                break;
                            }
                        }

                        if (type != null && !type.Equals(ResolveType("System.Object")))
                            break;
                    }
                }
                if (type == null)
                {
                    // Look for implicit base class
                    if (IsArray)
                        type = ResolveType("System.Array");
                    else if (codeDomTypes != null && codeDomTypes.Count > 0)
                    {
                        if (codeDomTypes[0].IsStruct)
                            type = ResolveType("System.ValueType");
                        else if (codeDomTypes[0].IsEnum)
                            type = ResolveType("System.Enum");
                        else if ((codeDomTypes[0].IsClass) && (!IsByRef) && (!IsPointer))
                            type = ResolveType("System.Object");
                        else if (codeDomTypes[0] is CodeTypeDelegate)
                            type = ResolveType("System.Delegate");
                    }
                }
                return type;
            }
        }

        public Type GetEnumType()
        {
            if (this.codeDomTypes != null)
            {
                foreach (CodeTypeDeclaration declaration in this.codeDomTypes)
                {
                    Type enumBaseType = declaration.UserData[typeof(Enum)] as Type;
                    if (enumBaseType != null)
                    {
                        return enumBaseType;
                    }
                    else
                    {
                        if (declaration.BaseTypes.Count > 1)
                        {
                            CodeTypeReference reference = declaration.BaseTypes[1]; //the first one would be Enum
                            Type enumBaseType2 = reference.UserData[typeof(Enum)] as Type;
                            if (enumBaseType2 != null)
                                return enumBaseType2;
                        }
                    }
                }
            }

            return typeof(int); //default
        }

        public override Type DeclaringType
        {
            get
            {
                return this.declaringType;
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
                    // Set a GUID
                    // 
                    this.guid = Guid.NewGuid();

                return this.guid;
            }
        }

        public override Module Module
        {
            get
            {
                // We can't provide this. This is a design time only type
                return null;
            }
        }

        public override string Name
        {
            get
            {
                string name = this.fullName;

                // detect first bracket, any name seperators after it are part of a generic parameter...  
                int idx = name.IndexOf('[');

                // Get the name after the last dot
                if (idx != -1)
                    idx = name.Substring(0, idx).LastIndexOfAny(nameSeparators);
                else
                    idx = name.LastIndexOfAny(nameSeparators);

                if (idx != -1)
                    name = this.fullName.Substring(idx + 1);
                return name;
            }
        }

        public override string Namespace
        {
            get
            {
                if (this.fullName == Name)
                    return string.Empty;
                
                if (this.declaringType != null)
                {
                    return this.declaringType.Namespace;
                }

                return this.fullName.Substring(0, this.fullName.Length - Name.Length - 1);
            }
        }

        public override RuntimeTypeHandle TypeHandle
        {
            get
            {
                // no runtime context to our type
#pragma warning suppress 56503
                throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
            }
        }

        public override Type UnderlyingSystemType
        {
            get
            {
                return this;
            }
        }


        #endregion

        #region public methods
        public override object[] GetCustomAttributes(bool inherit)
        {
            return GetCustomAttributes(typeof(object), inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == null)
                throw new ArgumentNullException("attributeType");

            // Ensure attributes
            if (this.codeDomTypes != null && this.attributes == null)
            {
                CodeAttributeDeclarationCollection attributeDecls = new CodeAttributeDeclarationCollection();

                foreach (CodeTypeDeclaration codeType in this.codeDomTypes)
                    attributeDecls.AddRange(codeType.CustomAttributes);

                this.attributes = Helper.LoadCustomAttributes(attributeDecls, this);
            }

            // It is possible both this.codeDomTypes and this.attributes are null.  
            // For example, when constructing type with element (Array, ByRef, Pointer), we don't
            // set the typedecl because the base type is a system type.  We don't set this.attributes
            // either because we don't actually create a type of the base type.  We simply do
            // new DesignTimeType(null, name, typeProvider).  In such cases, we loose the custom attributes
            // on the base type.
            if (this.attributes != null)
                return Helper.GetCustomAttributes(attributeType, inherit, this.attributes, this);
            else
                return new object[0];
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            return GetMembersHelper<ConstructorInfo>(bindingAttr, ref this.constructors, false);
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

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            return GetMembersHelper<MethodInfo>(bindingAttr, ref this.methods, true);
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            return GetMembersHelper<PropertyInfo>(bindingAttr, ref this.properties, true);
        }

        public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
        {
            // verify arguments
            VerifyGetMemberArguments(name, bindingAttr);

            List<MemberInfo> members = new List<MemberInfo>();

            // Methods
            if ((type & MemberTypes.Method) != 0)
                members.AddRange(GetMembersHelper<MethodInfo>(bindingAttr, new MemberSignature(name, null, null), ref this.methods));

            // Constructors
            if ((type & MemberTypes.Constructor) != 0)
                members.AddRange(GetMembersHelper<ConstructorInfo>(bindingAttr, new MemberSignature(name, null, null), ref this.constructors));

            // Properties
            if ((type & MemberTypes.Property) != 0)
                members.AddRange(GetMembersHelper<PropertyInfo>(bindingAttr, new MemberSignature(name, null, null), ref this.properties));

            // Events
            if ((type & MemberTypes.Event) != 0)
                members.AddRange(GetMembersHelper<EventInfo>(bindingAttr, new MemberSignature(name, null, null), ref this.events));

            // Fields
            if ((type & MemberTypes.Field) != 0)
                members.AddRange(GetMembersHelper<FieldInfo>(bindingAttr, new MemberSignature(name, null, null), ref this.fields));

            // Nested types
            if ((type & MemberTypes.NestedType) != 0)
                members.AddRange(GetMembersHelper<Type>(bindingAttr, new MemberSignature(name, null, null), ref this.nestedTypes));

            return members.ToArray();
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            // verify arguments
            VerifyGetMemberArguments(bindingAttr);

            ArrayList members = new ArrayList();

            // 
            members.AddRange(GetMethods(bindingAttr));
            members.AddRange(GetProperties(bindingAttr));
            members.AddRange(GetEvents(bindingAttr));
            members.AddRange(GetFields(bindingAttr));
            members.AddRange(GetNestedTypes(bindingAttr));

            return (MemberInfo[])members.ToArray(typeof(MemberInfo));
        }

        public override MemberInfo[] GetDefaultMembers()
        {
            // Get all of the custom attributes
            DefaultMemberAttribute attr = null;

            for (Type t = this; t != null; t = t.BaseType)
            {
                object[] attrs = GetCustomAttributes(typeof(DefaultMemberAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    attr = attrs[0] as DefaultMemberAttribute;

                if (attr != null)
                    break;
            }

            if (attr == null)
                return new MemberInfo[0];

            String defaultMember = attr.MemberName;
            MemberInfo[] members = GetMember(defaultMember);
            if (members == null)
                members = new MemberInfo[0];
            return members;
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            return GetMemberHelper<Type>(bindingAttr, new MemberSignature(name, null, null), ref this.nestedTypes);
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            return GetMembersHelper<Type>(bindingAttr, ref this.nestedTypes, false);
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            if (this.codeDomTypes != null)
            {
                StringComparison compare = (ignoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

                // search in the type
                foreach (CodeTypeDeclaration codeDomType in this.codeDomTypes)
                {
                    foreach (CodeTypeReference codeBaseType in codeDomType.BaseTypes)
                    {
                        Type interfaceCandidate = ResolveType(GetTypeNameFromCodeTypeReference(codeBaseType, this));

                        if (interfaceCandidate != null)
                        {
                            if ((interfaceCandidate.IsInterface == true) && (string.Equals(interfaceCandidate.FullName, name, compare)))
                                return interfaceCandidate;

                            // look in base class/intefaces
                            Type baseInterfaceCandidate = interfaceCandidate.GetInterface(name, ignoreCase);

                            if (baseInterfaceCandidate != null)
                                return baseInterfaceCandidate;
                        }
                    }
                }
            }

            return null;
        }

        public override Type[] GetInterfaces()
        {
            ArrayList types = new ArrayList();

            if (this.codeDomTypes != null)
            {
                // search in the type
                foreach (CodeTypeDeclaration codeDomType in this.codeDomTypes)
                {
                    foreach (CodeTypeReference codeBaseType in codeDomType.BaseTypes)
                    {
                        Type interfaceCandidate = ResolveType(GetTypeNameFromCodeTypeReference(codeBaseType, this));

                        if (interfaceCandidate != null)
                        {
                            if ((interfaceCandidate.IsInterface == true) && (!types.Contains(interfaceCandidate)))
                                types.Add(interfaceCandidate);

                            // look in base class/intefaces
                            Type[] baseInterfaces = interfaceCandidate.GetInterfaces();

                            foreach (Type baseInterfaceCandidate in baseInterfaces)
                            {
                                if ((baseInterfaceCandidate != null) && (!types.Contains(baseInterfaceCandidate)))
                                    types.Add(baseInterfaceCandidate);
                            }
                        }
                    }
                }
            }

            return (Type[])types.ToArray(typeof(Type));
        }

        public override object InvokeMember(string name, BindingFlags bindingFlags, Binder binder, object target, object[] providedArgs, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParams)
        {
            throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
        }

        public override string ToString()
        {
            return fullName;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
                throw new ArgumentNullException("attributeType");

            // 
            this.GetCustomAttributes(true);

            if (Helper.IsDefined(attributeType, inherit, attributes, this))
                return true;

            return false;
        }

        public override Type GetElementType()
        {
            Type elementType = null;
            int elementCharPosition = fullName.LastIndexOfAny(elementDecorators);

            if (elementCharPosition >= 0)
                elementType = ResolveType(fullName.Substring(0, elementCharPosition));

            return elementType;
        }


        public override int GetArrayRank()
        {
            if (!IsArray)
                throw new ArgumentException(TypeSystemSR.GetString("Error_TypeIsNotArray"));

            int position = Name.LastIndexOf('[');
            int rank = 1;
            while (Name[position] != ']')
            {
                if (Name[position] == ',')
                    rank++;
                position++;
            }

            return rank;
        }

        public override bool IsAssignableFrom(Type c)
        {
            return TypeProvider.IsAssignable(this, c);
        }

        public override bool IsSubclassOf(Type c)
        {
            if (c == null)
                return false;

            return TypeProvider.IsSubclassOf(this, c);
        }

        public override Type MakeArrayType()
        {
            return this.typeProvider.GetType(String.Format(CultureInfo.InvariantCulture, "{0}[]", this.FullName));
        }

        #endregion

        #region Helpers

        private void VerifyGetMemberArguments(string name, BindingFlags bindingAttr)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            VerifyGetMemberArguments(bindingAttr);
        }

        private void VerifyGetMemberArguments(BindingFlags bindingAttr)
        {
            // We only support public based constructors on DesignTime type
            BindingFlags supported = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase;

            if ((bindingAttr & ~supported) != 0)
                throw new ArgumentException(TypeSystemSR.GetString("Error_GetMemberBindingOptions"));

        }
        internal Type ResolveType(string name)
        {
            Type type = null;

            //first assume full name was provided
            type = typeProvider.GetType(name);

            if (type == null && !string.IsNullOrEmpty(Namespace))
                // prefixing the namespace
                type = typeProvider.GetType(Namespace + "." + name);

            // maybe it is nested type on the current type
            if (type == null)
                type = typeProvider.GetType(fullName + "+" + name);

            // prefixing imported namespaces
            if ((type == null) && (this.codeNamespaceImports != null))
            {
                foreach (CodeNamespaceImport codeNamespaceImport in this.codeNamespaceImports)
                {
                    type = typeProvider.GetType(codeNamespaceImport.Namespace + "." + name);
                    if (type != null)
                        break;
                }
            }

            // maybe it is a fullname of a nested class
            if (type == null)
            {
                string nestedName = name;
                int indexOfFirstDot = name.IndexOf('.');
                int indexOfLastDot = -1;
                while (((indexOfLastDot = nestedName.LastIndexOf('.')) != indexOfFirstDot) && (type == null))
                {
                    nestedName = nestedName.Substring(0, indexOfLastDot) + "+" + nestedName.Substring(indexOfLastDot + 1);
                    type = typeProvider.GetType(nestedName);
                }
            }

            // 
            return type;
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
            else if (memberInfo is DesignTimeEventInfo)
            {
                isPublic = (memberInfo as DesignTimeEventInfo).IsPublic;
                isStatic = (memberInfo as DesignTimeEventInfo).IsStatic;
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
                    accessorMethod = propertyInfo.GetGetMethod(true);
                else
                    accessorMethod = propertyInfo.GetSetMethod(true);
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

        // generic method that implements all GetXXXs methods
        private T[] GetMembersHelper<T>(BindingFlags bindingAttr, ref T[] members, bool searchBase)
            where T : MemberInfo
        {
            // verify arguments
            VerifyGetMemberArguments(bindingAttr);

            EnsureMembers(typeof(T));

            Dictionary<MemberSignature, T> membersDictionary = new Dictionary<MemberSignature, T>();

            // get local properties
            foreach (T memberInfo in members)
            {
                MemberSignature memberSignature = new MemberSignature(memberInfo);

                if ((FilterMember(memberInfo, bindingAttr)) && (!membersDictionary.ContainsKey(memberSignature)))
                    membersDictionary.Add(new MemberSignature(memberInfo), memberInfo);
            }

            if (searchBase && (bindingAttr & BindingFlags.DeclaredOnly) == 0)
            {
                // FlattenHierarchy is required to return static members from base classes.
                if ((bindingAttr & BindingFlags.FlattenHierarchy) == 0)
                    bindingAttr &= ~BindingFlags.Static;

                Type baseType = BaseType;
                if (baseType != null)
                {
                    T[] baseMembers = GetBaseMembers(typeof(T), baseType, bindingAttr) as T[];

                    foreach (T memberInfo in baseMembers)
                    {
                        // We should not return private members from base classes. Note: Generics requires us to use "as".
                        if ((memberInfo is FieldInfo && (memberInfo as FieldInfo).IsPrivate) || (memberInfo is MethodBase && (memberInfo as MethodBase).IsPrivate) || (memberInfo is Type && (memberInfo as Type).IsNestedPrivate))
                            continue;

                        // verify a member with this signature was not already created
                        MemberSignature memberSignature = new MemberSignature(memberInfo);

                        if (!membersDictionary.ContainsKey(memberSignature))
                            membersDictionary.Add(memberSignature, memberInfo);
                    }
                }
            }

            List<T> memberCollection = new List<T>(membersDictionary.Values);
            return memberCollection.ToArray();
        }

        private MemberInfo[] GetBaseMembers(Type type, Type baseType, BindingFlags bindingAttr)
        {
            MemberInfo[] members = null;
            if (type == typeof(PropertyInfo))
                members = baseType.GetProperties(bindingAttr);
            else if (type == typeof(EventInfo))
                members = baseType.GetEvents(bindingAttr);
            else if (type == typeof(ConstructorInfo))
                members = baseType.GetConstructors(bindingAttr);
            else if (type == typeof(MethodInfo))
                members = baseType.GetMethods(bindingAttr);
            else if (type == typeof(FieldInfo))
                members = baseType.GetFields(bindingAttr);
            else if (type == typeof(Type))
                members = baseType.GetNestedTypes(bindingAttr);

            return members;
        }

        private T[] GetMembersHelper<T>(BindingFlags bindingAttr, MemberSignature memberSignature, ref T[] members)
            where T : MemberInfo
        {
            List<T> memberCandidates = new List<T>();
            foreach (T memberInfo in this.GetMembersHelper<T>(bindingAttr, ref members, true))
            {
                MemberSignature candididateMemberSignature = new MemberSignature(memberInfo);
                if (candididateMemberSignature.FilterSignature(memberSignature))
                    memberCandidates.Add(memberInfo);
            }
            return memberCandidates.ToArray();
        }

        // generic method that implements all GetXXX methods
        private T GetMemberHelper<T>(BindingFlags bindingAttr, MemberSignature memberSignature, ref T[] members)
            where T : MemberInfo
        {
            // verify arguments
            VerifyGetMemberArguments(bindingAttr);

            EnsureMembers(typeof(T));

            // search the local type
            foreach (T memberInfo in members)
            {
                MemberSignature candididateMemberSignature = new MemberSignature(memberInfo);
                if (candididateMemberSignature.FilterSignature(memberSignature) && FilterMember(memberInfo, bindingAttr))
                    return memberInfo;
            }

            if ((bindingAttr & BindingFlags.DeclaredOnly) == 0)
            {
                // serach base types

                // FlattenHierarchy is required to return static members from base classes.
                if ((bindingAttr & BindingFlags.FlattenHierarchy) == 0)
                    bindingAttr &= ~BindingFlags.Static;

                Type baseType = BaseType;
                if (baseType != null)
                {
                    T memberInfo = (T)GetBaseMember(typeof(T), baseType, bindingAttr, memberSignature);

                    if (memberInfo != null)
                    {
                        // We should not return private members from base classes. Note: Generics requires us to use "as".
                        if ((memberInfo is FieldInfo && (memberInfo as FieldInfo).IsPrivate) || (memberInfo is MethodBase && (memberInfo as MethodBase).IsPrivate) || (memberInfo is Type && (memberInfo as Type).IsNestedPrivate))
                            return null;

                        return memberInfo;
                    }
                }
            }

            return null;
        }

        internal MemberInfo GetBaseMember(Type type, Type baseType, BindingFlags bindingAttr, MemberSignature memberSignature)
        {
            if (memberSignature == null)
                throw new ArgumentNullException("memberSignature");

            if (baseType == null)
                return null;

            MemberInfo member = null;

            if (typeof(PropertyInfo).IsAssignableFrom(type))
            {
                if (memberSignature.Parameters != null)
                    member = baseType.GetProperty(memberSignature.Name, bindingAttr, null, memberSignature.ReturnType, memberSignature.Parameters, null);
                else
                    member = baseType.GetProperty(memberSignature.Name, bindingAttr);
            }
            else if (typeof(EventInfo).IsAssignableFrom(type))
                member = baseType.GetEvent(memberSignature.Name, bindingAttr);
            else if (typeof(ConstructorInfo).IsAssignableFrom(type))
                member = baseType.GetConstructor(bindingAttr, null, memberSignature.Parameters, null);
            else if (typeof(MethodInfo).IsAssignableFrom(type))
            {
                if (memberSignature.Parameters != null)
                    member = baseType.GetMethod(memberSignature.Name, bindingAttr, null, memberSignature.Parameters, null);
                else
                    member = baseType.GetMethod(memberSignature.Name, bindingAttr);
            }
            else if (typeof(FieldInfo).IsAssignableFrom(type))
                member = baseType.GetField(memberSignature.Name, bindingAttr);
            else if (typeof(Type).IsAssignableFrom(type))
                member = baseType.GetNestedType(memberSignature.Name, bindingAttr);

            return member;
        }

        internal static string GetTypeNameFromCodeTypeReference(CodeTypeReference codeTypeReference, DesignTimeType declaringType)
        {
            StringBuilder typeName = new StringBuilder();

            if (codeTypeReference.ArrayRank == 0)
            {
                Type resolvedType = null;
                if (declaringType != null)
                    resolvedType = declaringType.ResolveType(codeTypeReference.BaseType);
                if (resolvedType != null)
                    typeName.Append(resolvedType.FullName);
                else
                    typeName.Append(codeTypeReference.BaseType);

                if ((codeTypeReference.TypeArguments != null) && (codeTypeReference.TypeArguments.Count > 0))
                {
                    if (codeTypeReference.BaseType.IndexOf('`') == -1)
                        typeName.Append(string.Format(CultureInfo.InvariantCulture, "`{0}", new object[] { codeTypeReference.TypeArguments.Count }));
                    typeName.Append("[");
                    foreach (CodeTypeReference typeArgument in codeTypeReference.TypeArguments)
                    {
                        typeName.Append("[");
                        typeName.Append(GetTypeNameFromCodeTypeReference(typeArgument, declaringType));
                        typeName.Append("],");
                    }
                    typeName.Length = typeName.Length - 1; //remove the last comma
                    typeName.Append("]");
                }
            }
            else
            {
                typeName.Append(GetTypeNameFromCodeTypeReference(codeTypeReference.ArrayElementType, declaringType));

                // Build array decoration (ByRefs and Pointers are part of the the BaseType) 
                typeName.Append("[");
                for (int loop = 0; loop < codeTypeReference.ArrayRank - 1; loop++)
                    typeName.Append(',');

                typeName.Append("]");
            }

            return typeName.ToString();
        }

        #endregion

        #region implementation overrides

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

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            return typeAttributes;
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
                return true;

            return false;

        }

        protected override bool IsByRefImpl()
        {
            return (this.fullName[this.fullName.Length - 1] == '&');
        }

        public override Type MakeByRefType()
        {
            return this.ResolveType(this.fullName + "&");
        }


        protected override bool IsCOMObjectImpl()
        {
            //
            return false;
        }

        protected override bool IsContextfulImpl()
        {
            //
            return false;
        }

        protected override bool IsMarshalByRefImpl()
        {
            //
            return false;
        }

        protected override bool IsPointerImpl()
        {
            return (this.fullName[this.fullName.Length - 1] == '*');
        }

        protected override bool IsPrimitiveImpl()
        {
            return false;
        }

        #endregion

        #region Loaders

        private void EnsureMembers(Type type)
        {
            if ((type == typeof(PropertyInfo)) && (this.properties == null))
                this.properties = GetCodeDomMembers<PropertyInfo>().ToArray();
            else if ((type == typeof(FieldInfo)) && (this.fields == null))
                this.fields = GetCodeDomMembers<FieldInfo>().ToArray();
            else if ((type == typeof(ConstructorInfo)) && (this.constructors == null))
                this.constructors = GetCodeDomConstructors().ToArray();
            else if ((type == typeof(EventInfo)) && (this.events == null))
                this.events = GetCodeDomMembers<EventInfo>().ToArray();
            else if ((type == typeof(MethodInfo)) && (this.methods == null))
            {
                EnsureMembers(typeof(PropertyInfo));
                EnsureMembers(typeof(EventInfo));

                List<MethodInfo> methodCollection = GetCodeDomMembers<MethodInfo>();
                MethodInfo methodInfo = null;

                foreach (PropertyInfo propertyInfo in this.properties)
                {
                    if ((methodInfo = propertyInfo.GetGetMethod()) != null)
                        methodCollection.Add(methodInfo);

                    if ((methodInfo = propertyInfo.GetSetMethod()) != null)
                        methodCollection.Add(methodInfo);
                }

                foreach (EventInfo eventInfo in this.events)
                {
                    if ((methodInfo = eventInfo.GetAddMethod()) != null)
                        methodCollection.Add(methodInfo);

                    if ((methodInfo = eventInfo.GetRemoveMethod()) != null)
                        methodCollection.Add(methodInfo);

                    if ((methodInfo = eventInfo.GetRaiseMethod()) != null)
                        methodCollection.Add(methodInfo);
                }

                this.methods = methodCollection.ToArray();
            }
        }

        private List<T> GetCodeDomMembers<T>()
            where T : MemberInfo
        {
            List<T> memberCollection = new List<T>();

            if (this.codeDomTypes != null)
            {
                foreach (CodeTypeDeclaration codeDomType in codeDomTypes)
                {
                    //!!! Work around for supporting delegates
                    if (codeDomType is CodeTypeDelegate && typeof(T) == typeof(MethodInfo))
                    {
                        CodeMemberMethod invokeMethod = new CodeMemberMethod();
                        invokeMethod.Name = "Invoke";
                        invokeMethod.Attributes = MemberAttributes.Public;
                        foreach (CodeParameterDeclarationExpression parameterDecl in ((CodeTypeDelegate)codeDomType).Parameters)
                            invokeMethod.Parameters.Add(parameterDecl);
                        invokeMethod.ReturnType = ((CodeTypeDelegate)codeDomType).ReturnType;
                        memberCollection.Add((T)CreateMemberInfo(typeof(MethodInfo), invokeMethod));
                    }

                    foreach (CodeTypeMember member in codeDomType.Members)
                    {
                        T memberInfo = (T)CreateMemberInfo(typeof(T), member);

                        if (memberInfo != null)
                            memberCollection.Add(memberInfo);
                    }
                }
            }
            return memberCollection;
        }

        // CFx bug 461 
        // The code dom being generated by VsCodeDomParser.cs does not
        // add default constructors for classes unless they
        // exist in the source code. Unfortunately, this cannot be easily
        // fixed in the CodeDomParser because the code dom returned by that
        // class is expected to kept in sync with the real source code. 
        // So we cannot "fabricate" a default constructor there without
        // breaking lots of assumptions elsewhere in the code. 
        // Instead, we add a default constructor here, if necessary.
        private List<ConstructorInfo> GetCodeDomConstructors()
        {
            List<ConstructorInfo> constructors = GetCodeDomMembers<ConstructorInfo>();
            // we only add a default constructor if 
            // this is a struct or is a non-static\non-abstract class and it doesn't have any 
            // constructors
            //  * Note - static classes are represented as Abstract and Sealed
            //           abstract classes are represented as Abstract; thus we will check just for that flag
            if (this.IsValueType || ((constructors.Count == 0) && !this.IsAbstract))
            {
                CodeConstructor codeConstructor = new CodeConstructor();
                codeConstructor.Attributes = MemberAttributes.Public;
                ConstructorInfo constructorInfo = new DesignTimeConstructorInfo(this, codeConstructor);
                constructors.Add(constructorInfo);
            }
            return constructors;
        }

        private void LoadNestedTypes(CodeTypeDeclaration codeDomType)
        {
            List<Type> localMembers = new List<Type>();

            foreach (Type t in this.nestedTypes)
                localMembers.Add(t);

            foreach (CodeTypeMember member in codeDomType.Members)
            {
                if (!(member is CodeTypeDeclaration))
                    continue;

                CodeTypeDeclaration codeType = member as CodeTypeDeclaration;
                Type partialType = null;
                foreach (Type nestedType in localMembers)
                {
                    if (nestedType.Name.Equals(Helper.EnsureTypeName(codeType.Name)))
                    {
                        partialType = nestedType;
                        break;
                    }
                }
                if (partialType == null)
                {
                    partialType = new DesignTimeType(this, codeType.Name, this.codeNamespaceImports, this.fullName, this.typeProvider);
                    localMembers.Add(partialType);
                    ((TypeProvider)this.typeProvider).AddType(partialType);
                }
                ((DesignTimeType)partialType).AddCodeTypeDeclaration(codeType);
            }
            this.nestedTypes = localMembers.ToArray();
        }

        private MemberInfo CreateMemberInfo(Type memberInfoType, CodeTypeMember member)
        {
            MemberInfo memberInfo = null;

            if ((memberInfoType == typeof(PropertyInfo)) && (member is CodeMemberProperty))
                memberInfo = new DesignTimePropertyInfo(this, member as CodeMemberProperty);
            else if ((memberInfoType == typeof(EventInfo)) && (member is CodeMemberEvent))
                memberInfo = new DesignTimeEventInfo(this, member as CodeMemberEvent);
            else if ((memberInfoType == typeof(FieldInfo)) && (member is CodeMemberField))
                memberInfo = new DesignTimeFieldInfo(this, member as CodeMemberField);
            else if ((memberInfoType == typeof(ConstructorInfo)) && ((member is CodeConstructor) || (member is CodeTypeConstructor)))
                memberInfo = new DesignTimeConstructorInfo(this, member as CodeMemberMethod);
            else if ((memberInfoType == typeof(MethodInfo)) && (member.GetType() == typeof(CodeMemberMethod)))
                memberInfo = new DesignTimeMethodInfo(this, member as CodeMemberMethod);
            return memberInfo;
        }

        #endregion

        #region MemberSignature class
        // Uniquely identify a memberInfo
        internal class MemberSignature
        {
            #region Members and Constructors

            private string name = null;
            private Type[] parameters = null;
            private Type returnType = null;
            readonly int hashCode;

            internal MemberSignature(MemberInfo memberInfo)
            {
                this.name = memberInfo.Name;

                if (memberInfo is MethodBase)
                {
                    List<Type> typeCollection = new List<Type>();

                    // method/constructor arguments
                    foreach (ParameterInfo parameterInfo in (memberInfo as MethodBase).GetParameters())
                        typeCollection.Add(parameterInfo.ParameterType);

                    this.parameters = typeCollection.ToArray();

                    if (memberInfo is MethodInfo)
                        this.returnType = ((MethodInfo)memberInfo).ReturnType;
                }
                else if (memberInfo is PropertyInfo)
                {
                    PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                    List<Type> typeCollection = new List<Type>();

                    // indexer arguments
                    foreach (ParameterInfo parameterInfo in propertyInfo.GetIndexParameters())
                        typeCollection.Add(parameterInfo.ParameterType);

                    this.parameters = typeCollection.ToArray();

                    // return type for property
                    this.returnType = propertyInfo.PropertyType;
                }

                this.hashCode = this.GetHashCodeImpl();
            }

            internal MemberSignature(string name, Type[] parameters, Type returnType)
            {
                this.name = name;
                this.returnType = returnType;
                if (parameters != null)
                    this.parameters = (Type[])parameters.Clone();

                this.hashCode = this.GetHashCodeImpl();
            }

            #endregion

            #region properties

            public string Name
            {
                get
                {
                    return name;
                }
            }

            public Type ReturnType
            {
                get
                {
                    return returnType;
                }
            }

            public Type[] Parameters
            {
                get
                {
                    if (parameters == null)
                        return null;

                    return (Type[])parameters.Clone();
                }
            }


            #endregion

            #region Comparison

            public override bool Equals(object obj)
            {
                MemberSignature memberSignature = obj as MemberSignature;

                if ((memberSignature == null) || (this.name != memberSignature.name) || (this.returnType != memberSignature.returnType))
                    return false;

                if ((this.Parameters == null) && (memberSignature.Parameters != null) ||
                    (this.Parameters != null) && (memberSignature.Parameters == null))
                    return false;

                if (this.Parameters != null)
                {
                    if (this.parameters.Length != memberSignature.parameters.Length)
                        return false;

                    for (int loop = 0; loop < this.parameters.Length; loop++)
                    {
                        if (this.parameters[loop] != memberSignature.parameters[loop])
                            return false;
                    }
                }

                return true;
            }

            // this method will filter using a mask signautre. only non-null mask members are used to filter 
            // the signature, the rest are ignored
            public bool FilterSignature(MemberSignature maskSignature)
            {
                if (maskSignature == null)
                    throw new ArgumentNullException("maskSignature");

                if (((maskSignature.Name != null) && (this.name != maskSignature.name)) ||
                    ((maskSignature.returnType != null) && (this.returnType != maskSignature.returnType)))
                    return false;

                if (maskSignature.parameters != null)
                {
                    if (this.parameters == null)
                        return false;

                    if (this.parameters.Length != maskSignature.parameters.Length)
                        return false;

                    for (int loop = 0; loop < this.parameters.Length; loop++)
                    {
                        if (!this.parameters[loop].Equals(maskSignature.parameters[loop]))
                            return false;
                    }
                }

                return true;
            }

            public override string ToString()
            {
                string str = string.Empty;

                if (returnType != null)
                    str = returnType.FullName + " ";

                if (name != null && name.Length != 0)
                    str += name;

                if (parameters != null && parameters.Length > 0)
                {
                    str += "(";

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (i > 0)
                            str += ", ";

                        if (parameters[i] != null)
                        {
                            if (parameters[i].GetType() != null && parameters[i].GetType().IsByRef)
                                str += "ref ";

                            str += parameters[i].FullName;
                        }
                    }

                    str += ")";
                }

                return str;
            }

            public override int GetHashCode()
            {
                return this.hashCode;
            }

            int GetHashCodeImpl()
            {
                int hashCode = 0;

                if (this.name != null)
                {
                    hashCode = name.GetHashCode();
                }

                if (this.parameters != null && this.parameters.Length > 0)
                {
                    for (int i = 0; i < this.parameters.Length; i++)
                    {
                        if (this.parameters[i] != null)
                        {
                            hashCode ^= this.parameters[i].GetHashCode();
                        }
                    }
                }

                if (this.returnType != null)
                {
                    hashCode ^= this.returnType.GetHashCode();
                }

                return hashCode;
            }

            #endregion
        }
        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return this;
        }
        #endregion
    }
}
