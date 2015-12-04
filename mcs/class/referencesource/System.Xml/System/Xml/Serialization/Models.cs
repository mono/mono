//------------------------------------------------------------------------------
// <copyright file="Models.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {

    using System;
    using System.Reflection;
    using System.Collections;
    using System.Diagnostics;

    // These classes define the abstract serialization model, e.g. the rules for WHAT is serialized.  
    // The answer of HOW the values are serialized is answered by a particular reflection importer 
    // by looking for a particular set of custom attributes specific to the serialization format
    // and building an appropriate set of accessors/mappings.

    internal class ModelScope {
        TypeScope typeScope;
        Hashtable models = new Hashtable();
        Hashtable arrayModels = new Hashtable();

        internal ModelScope(TypeScope typeScope) {
            this.typeScope = typeScope;
        }

        internal TypeScope TypeScope {
            get { return typeScope; }
        }

        internal TypeModel GetTypeModel(Type type) {
            return GetTypeModel(type, true);
        }

        internal TypeModel GetTypeModel(Type type, bool directReference) {
            TypeModel model = (TypeModel)models[type];
            if (model != null) return model;
            TypeDesc typeDesc = typeScope.GetTypeDesc(type, null, directReference);

            switch (typeDesc.Kind) {
                case TypeKind.Enum:
                    model = new EnumModel(type, typeDesc, this);
                    break;
                case TypeKind.Primitive:
                    model = new PrimitiveModel(type, typeDesc, this);
                    break;
                case TypeKind.Array:
                case TypeKind.Collection:
                case TypeKind.Enumerable:
                    model = new ArrayModel(type, typeDesc, this);
                    break;
                case TypeKind.Root:
                case TypeKind.Class:
                case TypeKind.Struct:
                    model = new StructModel(type, typeDesc, this);
                    break;
                default:
                    if (!typeDesc.IsSpecial) throw new NotSupportedException(Res.GetString(Res.XmlUnsupportedTypeKind, type.FullName));
                    model = new SpecialModel(type, typeDesc, this);
                    break;
            }

            models.Add(type, model);
            return model;
        }

        internal ArrayModel GetArrayModel(Type type) {
            TypeModel model = (TypeModel)arrayModels[type];
            if (model == null) {
                model = GetTypeModel(type);
                if (!(model is ArrayModel)) {
                    TypeDesc typeDesc = typeScope.GetArrayTypeDesc(type);
                    model = new ArrayModel(type, typeDesc, this);
                }
                arrayModels.Add(type, model);
            }
            return (ArrayModel)model;
        }
    }

    internal abstract class TypeModel {
        TypeDesc typeDesc;
        Type type;
        ModelScope scope;

        protected TypeModel(Type type, TypeDesc typeDesc, ModelScope scope) {
            this.scope = scope;
            this.type = type;
            this.typeDesc = typeDesc;
        }

        internal Type Type {
            get { return type; }
        }

        internal ModelScope ModelScope {
            get { return scope; }
        }

        internal TypeDesc TypeDesc {
            get { return typeDesc; }
        }
    }

    internal class ArrayModel : TypeModel {
        internal ArrayModel(Type type, TypeDesc typeDesc, ModelScope scope) : base(type, typeDesc, scope) { }

        internal TypeModel Element {
            get { return ModelScope.GetTypeModel(TypeScope.GetArrayElementType(Type, null)); }
        }
    }

    internal class PrimitiveModel : TypeModel {
        internal PrimitiveModel(Type type, TypeDesc typeDesc, ModelScope scope) : base(type, typeDesc, scope) { }
    }

    internal class SpecialModel : TypeModel {
        internal SpecialModel(Type type, TypeDesc typeDesc, ModelScope scope) : base(type, typeDesc, scope) { }
    }

    internal class StructModel : TypeModel {

        internal StructModel(Type type, TypeDesc typeDesc, ModelScope scope) : base(type, typeDesc, scope) { }

        internal MemberInfo[] GetMemberInfos() {
            // we use to return Type.GetMembers() here, the members were returned in a different order: fields first, properties last
            // Current System.Reflection code returns members in oposite order: properties first, then fields.
            // This code make sure that returns members in the Everett order.
            MemberInfo[] members = Type.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            MemberInfo[] fieldsAndProps = new MemberInfo[members.Length];

            int cMember = 0;
            // first copy all non-property members over
            for (int i = 0; i < members.Length; i++) {
                if ((members[i].MemberType & MemberTypes.Property) == 0) {
                    fieldsAndProps[cMember++] = members[i];
                }
            }
            // now copy all property members over
            for (int i = 0; i < members.Length; i++) {
                if ((members[i].MemberType & MemberTypes.Property) != 0) {
                    fieldsAndProps[cMember++] = members[i];
                }
            }
            return fieldsAndProps;
        }

        internal FieldModel GetFieldModel(MemberInfo memberInfo) {
            FieldModel model = null;
            if (memberInfo is FieldInfo)
                model = GetFieldModel((FieldInfo)memberInfo);
            else if (memberInfo is PropertyInfo)
                model = GetPropertyModel((PropertyInfo)memberInfo);
            if (model != null) {
                if (model.ReadOnly && model.FieldTypeDesc.Kind != TypeKind.Collection && model.FieldTypeDesc.Kind != TypeKind.Enumerable)
                    return null;
            }
            return model;
        }

        void CheckSupportedMember(TypeDesc typeDesc, MemberInfo member, Type type) {
            if (typeDesc == null)
                return;
            if (typeDesc.IsUnsupported) {
                if (typeDesc.Exception == null) {
                    typeDesc.Exception = new NotSupportedException(Res.GetString(Res.XmlSerializerUnsupportedType, typeDesc.FullName));
                }
                throw new InvalidOperationException(Res.GetString(Res.XmlSerializerUnsupportedMember, member.DeclaringType.FullName + "." + member.Name, type.FullName), typeDesc.Exception);

            }
            CheckSupportedMember(typeDesc.BaseTypeDesc, member, type);
            CheckSupportedMember(typeDesc.ArrayElementTypeDesc, member, type);
        }

        FieldModel GetFieldModel(FieldInfo fieldInfo) {
            if (fieldInfo.IsStatic) return null;
            if (fieldInfo.DeclaringType != Type) return null;

            TypeDesc typeDesc = ModelScope.TypeScope.GetTypeDesc(fieldInfo.FieldType, fieldInfo, true, false);
            if (fieldInfo.IsInitOnly && typeDesc.Kind != TypeKind.Collection && typeDesc.Kind != TypeKind.Enumerable)
                return null;

            CheckSupportedMember(typeDesc, fieldInfo, fieldInfo.FieldType);
            return new FieldModel(fieldInfo, fieldInfo.FieldType, typeDesc);
        }

        FieldModel GetPropertyModel(PropertyInfo propertyInfo) {
            if (propertyInfo.DeclaringType != Type) return null;
            if (CheckPropertyRead(propertyInfo)) {
                TypeDesc typeDesc = ModelScope.TypeScope.GetTypeDesc(propertyInfo.PropertyType, propertyInfo, true, false);
                // Fix for CSDMain 100492, please contact arssrvlt if you need to change this line
                if (!propertyInfo.CanWrite && typeDesc.Kind != TypeKind.Collection && typeDesc.Kind != TypeKind.Enumerable)
                    return null;
                CheckSupportedMember(typeDesc, propertyInfo, propertyInfo.PropertyType);
                return new FieldModel(propertyInfo, propertyInfo.PropertyType, typeDesc);
            }
            return null;
        }

        //CheckProperty
        internal static bool CheckPropertyRead(PropertyInfo propertyInfo) {
            if (!propertyInfo.CanRead) return false;

            MethodInfo getMethod = propertyInfo.GetGetMethod();
            if (getMethod.IsStatic) return false;
            ParameterInfo[] parameters = getMethod.GetParameters();
            if (parameters.Length > 0) return false;
            return true;
        }
    }

    internal enum SpecifiedAccessor {
        None,
        ReadOnly,
        ReadWrite,
    }

    internal class FieldModel {
        SpecifiedAccessor checkSpecified = SpecifiedAccessor.None;
        MemberInfo memberInfo;
        MemberInfo checkSpecifiedMemberInfo;
        MethodInfo checkShouldPersistMethodInfo;
        bool checkShouldPersist;
        bool readOnly = false;
        bool isProperty = false;
        Type fieldType;
        string name;
        TypeDesc fieldTypeDesc;

        internal FieldModel(string name, Type fieldType, TypeDesc fieldTypeDesc, bool checkSpecified, bool checkShouldPersist) :
            this(name, fieldType, fieldTypeDesc, checkSpecified, checkShouldPersist, false) {
        }
        internal FieldModel(string name, Type fieldType, TypeDesc fieldTypeDesc, bool checkSpecified, bool checkShouldPersist, bool readOnly) {
            this.fieldTypeDesc = fieldTypeDesc;
            this.name = name;
            this.fieldType = fieldType;
            this.checkSpecified = checkSpecified ? SpecifiedAccessor.ReadWrite : SpecifiedAccessor.None;
            this.checkShouldPersist = checkShouldPersist;
            this.readOnly = readOnly;
        }

        internal FieldModel(MemberInfo memberInfo, Type fieldType, TypeDesc fieldTypeDesc) {
            this.name = memberInfo.Name;
            this.fieldType = fieldType;
            this.fieldTypeDesc = fieldTypeDesc;
            this.memberInfo = memberInfo;
            this.checkShouldPersistMethodInfo = memberInfo.DeclaringType.GetMethod("ShouldSerialize" + memberInfo.Name, new Type[0]);
            this.checkShouldPersist = this.checkShouldPersistMethodInfo != null;

            FieldInfo specifiedField = memberInfo.DeclaringType.GetField(memberInfo.Name + "Specified");
            if (specifiedField != null) {
                if (specifiedField.FieldType != typeof(bool)) {
                    throw new InvalidOperationException(Res.GetString(Res.XmlInvalidSpecifiedType, specifiedField.Name, specifiedField.FieldType.FullName, typeof(bool).FullName));
                }
                this.checkSpecified = specifiedField.IsInitOnly ? SpecifiedAccessor.ReadOnly : SpecifiedAccessor.ReadWrite;
                this.checkSpecifiedMemberInfo = specifiedField;
            }
            else {
                PropertyInfo specifiedProperty = memberInfo.DeclaringType.GetProperty(memberInfo.Name + "Specified");
                if (specifiedProperty != null) {
                    if (StructModel.CheckPropertyRead(specifiedProperty)) {
                        this.checkSpecified = specifiedProperty.CanWrite ? SpecifiedAccessor.ReadWrite : SpecifiedAccessor.ReadOnly;
                        this.checkSpecifiedMemberInfo = specifiedProperty;
                    }
                    if (this.checkSpecified != SpecifiedAccessor.None && specifiedProperty.PropertyType != typeof(bool)) {
                        throw new InvalidOperationException(Res.GetString(Res.XmlInvalidSpecifiedType, specifiedProperty.Name, specifiedProperty.PropertyType.FullName, typeof(bool).FullName));
                    }
                }
            }
            if (memberInfo is PropertyInfo) {
                readOnly = !((PropertyInfo)memberInfo).CanWrite;
                isProperty = true;
            }
            else if (memberInfo is FieldInfo) {
                readOnly = ((FieldInfo)memberInfo).IsInitOnly;
            }
        }

        internal string Name {
            get { return name; }
        }

        internal Type FieldType {
            get { return fieldType; }
        }

        internal TypeDesc FieldTypeDesc {
            get { return fieldTypeDesc; }
        }

        internal bool CheckShouldPersist {
            get { return checkShouldPersist; }
        }

        internal SpecifiedAccessor CheckSpecified {
            get { return checkSpecified; }
        }

        internal MemberInfo MemberInfo {
            get { return memberInfo; }
        }
        internal MemberInfo CheckSpecifiedMemberInfo {
            get { return checkSpecifiedMemberInfo; }
        }
        internal MethodInfo CheckShouldPersistMethodInfo {
            get { return checkShouldPersistMethodInfo; }
        }

        internal bool ReadOnly {
            get { return readOnly; }
        }

        internal bool IsProperty {
            get { return isProperty; }
        }
    }

    internal class ConstantModel {
        FieldInfo fieldInfo;
        long value;

        internal ConstantModel(FieldInfo fieldInfo, long value) {
            this.fieldInfo = fieldInfo;
            this.value = value;
        }

        internal string Name {
            get { return fieldInfo.Name; }
        }

        internal long Value {
            get { return value; }
        }

        internal FieldInfo FieldInfo {
            get { return fieldInfo; }
        }
    }

    internal class EnumModel : TypeModel {
        ConstantModel[] constants;

        internal EnumModel(Type type, TypeDesc typeDesc, ModelScope scope) : base(type, typeDesc, scope) { }

        internal ConstantModel[] Constants {
            get {
                if (constants == null) {
                    ArrayList list = new ArrayList();
                    FieldInfo[] fields = Type.GetFields();
                    for (int i = 0; i < fields.Length; i++) {
                        FieldInfo field = fields[i];
                        ConstantModel constant = GetConstantModel(field);
                        if (constant != null) list.Add(constant);
                    }
                    constants = (ConstantModel[])list.ToArray(typeof(ConstantModel));
                }
                return constants;
            }

        }

        ConstantModel GetConstantModel(FieldInfo fieldInfo) {
            if (fieldInfo.IsSpecialName) return null;
            return new ConstantModel(fieldInfo, ((IConvertible)fieldInfo.GetValue(null)).ToInt64(null));
        }
    }
}

