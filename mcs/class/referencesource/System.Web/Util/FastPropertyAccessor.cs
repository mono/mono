//------------------------------------------------------------------------------
// <copyright file="FastPropertyAccessor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Collections;
using System.Security;
using System.Security.Permissions;

namespace System.Web.Util {
    /*
     * Property Accessor Generator class
     *
     * The purpose of this class is to generate some IL code on the fly that can efficiently
     * access properties (and fields) of objects.  This is an alternative to using
     * very slow reflection.
     */

    internal class FastPropertyAccessor {

        private static object s_lockObject = new object();
        private static FastPropertyAccessor s_accessorGenerator;
        private static Hashtable s_accessorCache;
        private static MethodInfo _getPropertyMethod;
        private static MethodInfo _setPropertyMethod;
        private static Type[] _getPropertyParameterList = new Type[] { typeof(object) };
        private static Type[] _setPropertyParameterList = new Type[] { typeof(object), typeof(object) };
        private static Type[] _interfacesToImplement;

        private static int _uniqueId;   // Used to generate unique type ID's.

        // Property getter/setter must be public for codegen to access it.
        // Static properties are ignored, since this class only works on instances of objects.
        // Need to use DeclaredOnly to avoid AmbiguousMatchException if a property with
        // a different return type is hidden.
        private const BindingFlags _declaredFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

        private ModuleBuilder _dynamicModule = null;

        static FastPropertyAccessor() {

            // Get the SetProperty method, and make sure it has
            // the correct signature.

            _getPropertyMethod = typeof(IWebPropertyAccessor).GetMethod("GetProperty");
            _setPropertyMethod = typeof(IWebPropertyAccessor).GetMethod("SetProperty");

            // This will be needed later, when building the dynamic class.
            _interfacesToImplement = new Type[1];
            _interfacesToImplement[0] = typeof(IWebPropertyAccessor);
        }

        private static String GetUniqueCompilationName() {
            return Guid.NewGuid().ToString().Replace('-', '_');
        }

        Type GetPropertyAccessorTypeWithAssert(Type type, string propertyName,
            PropertyInfo propInfo, FieldInfo fieldInfo) {
            // Create the dynamic assembly if needed.
            Type accessorType;

            MethodInfo getterMethodInfo = null;
            MethodInfo setterMethodInfo = null;
            Type propertyType;

            if (propInfo != null) {
                // It'a a property
                getterMethodInfo = propInfo.GetGetMethod();
                setterMethodInfo = propInfo.GetSetMethod();

                propertyType = propInfo.PropertyType;
            }
            else {
                // If not, it must be a field
                propertyType = fieldInfo.FieldType;
            }

            if (_dynamicModule == null) {
                lock (this) {
                    if (_dynamicModule == null) {

                        // Use a unique name for each assembly.
                        String name = GetUniqueCompilationName();

                        AssemblyName assemblyName = new AssemblyName();
                        assemblyName.Name = "A_" + name;

                        // Create a new assembly.
                        AssemblyBuilder newAssembly =
                           Thread.GetDomain().DefineDynamicAssembly(assemblyName,
                                                                    AssemblyBuilderAccess.Run,
                                                                    null, //directory to persist assembly
                                                                    true, //isSynchronized
                                                                    null  //assembly attributes
                                                                    );

                        // Create a single module in the assembly.
                        _dynamicModule = newAssembly.DefineDynamicModule("M_" + name);
                    }
                }
            }

            // Give the factory a unique name.

            String typeName = System.Web.UI.Util.MakeValidTypeNameFromString(type.Name) +
                "_" + propertyName + "_" + (_uniqueId++);

            TypeBuilder accessorTypeBuilder = _dynamicModule.DefineType("T_" + typeName,
                                                                       TypeAttributes.Public,
                                                                       typeof(object),
                                                                       _interfacesToImplement);

            //
            // Define the GetProperty method. It must be virtual to be an interface implementation.
            //

            MethodBuilder method = accessorTypeBuilder.DefineMethod("GetProperty",
                                                                   MethodAttributes.Public |
                                                                        MethodAttributes.Virtual,
                                                                   typeof(Object),
                                                                   _getPropertyParameterList);

            // Generate IL. The generated IL corresponds to:
            //  "return ((TargetType) target).Blah;"

            ILGenerator il = method.GetILGenerator();
            if (getterMethodInfo != null) {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Castclass, type);

                // Generate the getter call based on whether it's a Property or Field
                if (propInfo != null)
                    il.EmitCall(OpCodes.Callvirt, getterMethodInfo, null);
                else
                    il.Emit(OpCodes.Ldfld, fieldInfo);

                il.Emit(OpCodes.Box, propertyType);
                il.Emit(OpCodes.Ret);

                // Specify that this method implements GetProperty from the inherited interface.
                accessorTypeBuilder.DefineMethodOverride(method, _getPropertyMethod);
            }
            else {
                // Generate IL. The generated IL corresponds to "throw new InvalidOperationException"
                ConstructorInfo cons = typeof(InvalidOperationException).GetConstructor(Type.EmptyTypes);
                il.Emit(OpCodes.Newobj, cons);
                il.Emit(OpCodes.Throw);
            }


            //
            // Define the SetProperty method. It must be virtual to be an interface implementation.
            //

            method = accessorTypeBuilder.DefineMethod("SetProperty",
                                                                   MethodAttributes.Public |
                                                                        MethodAttributes.Virtual,
                                                                   null,
                                                                   _setPropertyParameterList);

            il = method.GetILGenerator();

            // Don't generate any code in the setter if it's a readonly property.
            // We still need to have an implementation of SetProperty, but it does nothing.
            if (fieldInfo != null || setterMethodInfo != null) {

                // Generate IL. The generated IL corresponds to:
                //  "((TargetType) target).Blah = (PropType) val;"

                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Castclass, type);
                il.Emit(OpCodes.Ldarg_2);

                if (propertyType.IsPrimitive) {
                    // Primitive type: deal with boxing
                    il.Emit(OpCodes.Unbox, propertyType);

                    // Emit the proper instruction for the type
                    if (propertyType == typeof(sbyte)) {
                        il.Emit(OpCodes.Ldind_I1);
                    }
                    else if (propertyType == typeof(byte)) {
                        il.Emit(OpCodes.Ldind_U1);
                    }
                    else if (propertyType == typeof(short)) {
                        il.Emit(OpCodes.Ldind_I2);
                    }
                    else if (propertyType == typeof(ushort)) {
                        il.Emit(OpCodes.Ldind_U2);
                    }
                    else if (propertyType == typeof(uint)) {
                        il.Emit(OpCodes.Ldind_U4);
                    }
                    else if (propertyType == typeof(int)) {
                        il.Emit(OpCodes.Ldind_I4);
                    }
                    else if (propertyType == typeof(long)) {
                        il.Emit(OpCodes.Ldind_I8);
                    }
                    else if (propertyType == typeof(ulong)) {
                        il.Emit(OpCodes.Ldind_I8);  // Somehow, there is no Ldind_u8
                    }
                    else if (propertyType == typeof(bool)) {
                        il.Emit(OpCodes.Ldind_I1);
                    }
                    else if (propertyType == typeof(char)) {
                        il.Emit(OpCodes.Ldind_U2);
                    }
                    else if (propertyType == typeof(decimal)) {
                        il.Emit(OpCodes.Ldobj, propertyType);
                    }
                    else if (propertyType == typeof(float)) {
                        il.Emit(OpCodes.Ldind_R4);
                    }
                    else if (propertyType == typeof(double)) {
                        il.Emit(OpCodes.Ldind_R8);
                    }
                    else {
                        il.Emit(OpCodes.Ldobj, propertyType);
                    }
                }
                else if (propertyType.IsValueType) {
                    // Value type: deal with boxing
                    il.Emit(OpCodes.Unbox, propertyType);
                    il.Emit(OpCodes.Ldobj, propertyType);
                }
                else {
                    // No boxing involved: just generate a standard cast
                    il.Emit(OpCodes.Castclass, propertyType);
                }

                // Generate the assignment based on whether it's a Property or Field
                if (propInfo != null)
                    il.EmitCall(OpCodes.Callvirt, setterMethodInfo, null);
                else
                    il.Emit(OpCodes.Stfld, fieldInfo);
            }

            il.Emit(OpCodes.Ret);

            // Specify that this method implements SetProperty from the inherited interface.
            accessorTypeBuilder.DefineMethodOverride(method, _setPropertyMethod);

            // Bake in the type.
            accessorType = accessorTypeBuilder.CreateType();

            return accessorType;
        }

        private static void GetPropertyInfo(Type type, string propertyName, out PropertyInfo propInfo, out FieldInfo fieldInfo, out Type declaringType) {
        
            // First, try to find a property with that name.  Type.GetProperty() without BindingFlags.Declared
            // will throw AmbiguousMatchException if there is a hidden property with the same name and a
            // different type (VSWhidbey 237437).  This method finds the property with the specified name
            // on the most specific type.
            propInfo = GetPropertyMostSpecific(type, propertyName);
            fieldInfo = null;

            if (propInfo != null) {
                // Get the most base Type where the property is first declared
                MethodInfo baseCheckMethodInfo = propInfo.GetGetMethod();
                if (baseCheckMethodInfo == null) {
                    baseCheckMethodInfo = propInfo.GetSetMethod();
                }
                declaringType = baseCheckMethodInfo.GetBaseDefinition().DeclaringType;

                // DevDiv Bug 27734
                // Ignore the declaring type if it's generic
                if (declaringType.IsGenericType)
                    declaringType = type;

                // If they're different, get a new PropertyInfo
                if (declaringType != type) {
                    // We want the propertyInfo for the property specifically declared on the declaringType.
                    // So pass in the correct BindingFlags to avoid an AmbiguousMatchException, which would
                    // be thrown if the declaringType hides a property with the same name and a different type.
                    // VSWhidbey 518034
                    propInfo = declaringType.GetProperty(propertyName, _declaredFlags);
                }
            }
            else {
                // We couldn't find a property, so try a field
                // Type.GetField can not throw AmbiguousMatchException like Type.GetProperty above.
                fieldInfo = type.GetField(propertyName);

                // If we couldn't find a field either, give up
                if (fieldInfo == null)
                    throw new ArgumentException();

                declaringType = fieldInfo.DeclaringType;
            }
        }

        private static IWebPropertyAccessor GetPropertyAccessor(Type type, string propertyName) {

            if (s_accessorGenerator == null || s_accessorCache == null) {
                lock (s_lockObject) {
                    if (s_accessorGenerator == null || s_accessorCache == null) {
                        s_accessorGenerator = new FastPropertyAccessor();
                        s_accessorCache = new Hashtable();
                    }
                }
            }

            // First, check if we have it cached

            // Get a hash key based on the Type and the property name
            int cacheKey = HashCodeCombiner.CombineHashCodes(
                type.GetHashCode(), propertyName.GetHashCode());

            IWebPropertyAccessor accessor = (IWebPropertyAccessor)s_accessorCache[cacheKey];

            // It was cached, so just return it
            if (accessor != null)
                return accessor;

            FieldInfo fieldInfo = null;
            PropertyInfo propInfo = null;
            Type declaringType;

            GetPropertyInfo(type, propertyName, out propInfo, out fieldInfo, out declaringType);

            // If the Type where the property/field is declared is not the same as the current
            // Type, check if the declaring Type already has a cached accessor.  This limits
            // the number of different accessors we need to create.  e.g. Every control has
            // an ID property, but we'll end up only create one accessor for all of them.
            int declaringTypeCacheKey = 0;
            if (declaringType != type) {
                // Get a hash key based on the declaring Type and the property name
                declaringTypeCacheKey = HashCodeCombiner.CombineHashCodes(
                    declaringType.GetHashCode(), propertyName.GetHashCode());

                accessor = (IWebPropertyAccessor) s_accessorCache[declaringTypeCacheKey];

                // We have a cached accessor for the declaring type, so use it
                if (accessor != null) {

                    // Cache the declaring type's accessor as ourselves
                    lock (s_accessorCache.SyncRoot) {
                        s_accessorCache[cacheKey] = accessor;
                    }

                    return accessor;
                }
            }

            if (accessor == null) {
                Type propertyAccessorType;

                lock (s_accessorGenerator) {
                    propertyAccessorType = s_accessorGenerator.GetPropertyAccessorTypeWithAssert(
                        declaringType, propertyName, propInfo, fieldInfo);
                }

                // Create the type. This is the only place where Activator.CreateInstance is used,
                // reducing the calls to it from 1 per instance to 1 per type.
                accessor = (IWebPropertyAccessor) HttpRuntime.CreateNonPublicInstance(propertyAccessorType);
            }

            // Cache the accessor
            lock (s_accessorCache.SyncRoot) {
                s_accessorCache[cacheKey] = accessor;

                if (declaringTypeCacheKey != 0)
                    s_accessorCache[declaringTypeCacheKey] = accessor;
            }

            return accessor;
        }

        internal static object GetProperty(object target, string propName, bool inDesigner) {
            if (!inDesigner) {
                IWebPropertyAccessor accessor = GetPropertyAccessor(target.GetType(), propName);
                return accessor.GetProperty(target);
            }
            else {
                // Dev10 bug 491386 - avoid CLR code path that causes an exception when designer uses two
                // assemblies of the same name at different locations
                FieldInfo fieldInfo = null;
                PropertyInfo propInfo = null;
                Type declaringType;
                GetPropertyInfo(target.GetType(), propName, out propInfo, out fieldInfo, out declaringType);
                if (propInfo != null) {
                    return propInfo.GetValue(target, null);
                }
                else if (fieldInfo != null) {
                    return fieldInfo.GetValue(target);
                }
                throw new ArgumentException();
            }
        }

        // Finds the property with the specified name on the most specific type.
        private static PropertyInfo GetPropertyMostSpecific(Type type, string name) {
            PropertyInfo propInfo;
            Type currentType = type;

            while (currentType != null) {
                propInfo = currentType.GetProperty(name, _declaredFlags);
                if (propInfo != null) {
                    return propInfo;
                }
                else {
                    currentType = currentType.BaseType;
                }
            }

            return null;
        }

        internal static void SetProperty(object target, string propName, object val, bool inDesigner) {
            if (!inDesigner) {
                IWebPropertyAccessor accessor = GetPropertyAccessor(target.GetType(), propName);
                accessor.SetProperty(target, val);
            }
            else {
                // Dev10 bug 491386 - avoid CLR code path that causes an exception when designer uses two
                // assemblies of the same name at different locations
                FieldInfo fieldInfo = null;
                PropertyInfo propInfo = null;
                Type declaringType = null;
                GetPropertyInfo(target.GetType(), propName, out propInfo, out fieldInfo, out declaringType);
                if (propInfo != null) {
                    propInfo.SetValue(target, val, null);
                }
                else if (fieldInfo != null) {
                    fieldInfo.SetValue(target, val);
                }
                else {
                    throw new ArgumentException();
                }
            }
        }
    }

    public interface IWebPropertyAccessor {
        object GetProperty(object target);
        void SetProperty(object target, object value);
    }
}
