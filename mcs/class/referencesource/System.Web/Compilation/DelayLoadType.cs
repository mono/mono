//------------------------------------------------------------------------------
// <copyright file="DelayLoadType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Reflection;

namespace System.Web.Compilation {
    internal class DelayLoadType : Type {
        private Type _type;
        private string _assemblyName;
        private string _typeName;

        public DelayLoadType(string assemblyName, string typeName) {
            _assemblyName = assemblyName;
            _typeName = typeName;
        }

        internal static bool Enabled {
            get {
                return BuildManagerHost.InClientBuildManager;
            }
        }

        public Type Type {
            get {
                if (_type == null) {
                    Assembly a = Assembly.Load(_assemblyName);
                    _type = a.GetType(_typeName);
                }

                return _type;
            }
        }

        public string AssemblyName {
            get {
                return _assemblyName;
            }
        }

        public string TypeName {
            get {
                return _typeName;
            }
        }

        public override Assembly Assembly {
            get {
                return Type.Assembly;
            }
        }

        public override string AssemblyQualifiedName {
            get {
                return Type.AssemblyQualifiedName;
            }
        }

        public override Type BaseType {
            get {
                return Type.BaseType;
            }
        }

        public override string FullName {
            get {
                return Type.FullName;
            }
        }

        public override Guid GUID {
            get {
                return Type.GUID;
            }
        }

        protected override TypeAttributes GetAttributeFlagsImpl() {
            return Type.Attributes;
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) {
            return Type.GetConstructor(bindingAttr, binder, callConvention, types, modifiers);
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) {
            return Type.GetConstructors(bindingAttr);
        }

        public override Type GetElementType() {
            return Type.GetElementType();
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr) {
            return Type.GetEvent(name, bindingAttr);
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr) {
            return Type.GetEvents(bindingAttr);
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr) {
            return Type.GetField(name, bindingAttr);
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr) {
            return Type.GetFields(bindingAttr);
        }

        public override Type GetInterface(string name, bool ignoreCase) {
            return Type.GetInterface(name, ignoreCase);
        }

        public override Type[] GetInterfaces() {
            return Type.GetInterfaces();
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr) {
            return Type.GetMembers(bindingAttr);
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) {
            return Type.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr) {
            return Type.GetMethods(bindingAttr);
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr) {
            return Type.GetNestedType(name, bindingAttr);
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr) {
            return Type.GetNestedTypes(bindingAttr);
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr) {
            return Type.GetProperties(bindingAttr);
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers) {
            return Type.GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
        }

        protected override bool HasElementTypeImpl() {
            return Type.HasElementType;
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters) {
            return Type.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
        }

        protected override bool IsArrayImpl() {
            return Type.IsArray;
        }

        protected override bool IsByRefImpl() {
            return Type.IsByRef;
        }

        protected override bool IsCOMObjectImpl() {
            return Type.IsCOMObject;
        }

        protected override bool IsPointerImpl() {
            return Type.IsPointer;
        }

        protected override bool IsPrimitiveImpl() {
            return Type.IsPrimitive;
        }

        public override Module Module {
            get {
                return Type.Module;
            }
        }

        public override string Namespace {
            get {
                return Type.Namespace;
            }
        }

        public override Type UnderlyingSystemType {
            get {
                return Type.UnderlyingSystemType;
            }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
            return Type.GetCustomAttributes(attributeType, inherit);
        }

        public override object[] GetCustomAttributes(bool inherit) {
            return Type.GetCustomAttributes(inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit) {
            return Type.IsDefined(attributeType, inherit);
        }

        public override string Name {
            get {
                return Type.Name;
            }
        }
    }
}
