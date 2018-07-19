//---------------------------------------------------------------------
// <copyright file="LightweightCodeGenerator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Objects
{
    using System;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Data.Objects.DataClasses;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Permissions;

    /// <summary>
    /// CodeGenerator class: use lightweight code gen to dynamically generate code to get/set properties.
    /// </summary>
    internal static class LightweightCodeGenerator
    {
        /// <summary>For an OSpace ComplexType returns the delegate to construct the clr instance.</summary>
        internal static Delegate GetConstructorDelegateForType(ClrComplexType clrType)
        {
            return (clrType.Constructor ?? (clrType.Constructor = CreateConstructor(clrType.ClrType)));
        }

        /// <summary>For an OSpace EntityType returns the delegate to construct the clr instance.</summary>
        internal static Delegate GetConstructorDelegateForType(ClrEntityType clrType)
        {
            return (clrType.Constructor ?? (clrType.Constructor = CreateConstructor(clrType.ClrType)));
        }

        /// <summary>for an OSpace property, get the property value from a clr instance</summary>
        internal static object GetValue(EdmProperty property, object target)
        {
            Func<object, object> getter = GetGetterDelegateForProperty(property);
            Debug.Assert(null != getter, "null getter");

            return getter(target);
        }

        internal static Func<object,object> GetGetterDelegateForProperty(EdmProperty property)
        {
            return property.ValueGetter ?? (property.ValueGetter = CreatePropertyGetter(property.EntityDeclaringType, property.PropertyGetterHandle));
        }

        /// <summary>for an OSpace property, set the property value on a clr instance</summary>
        /// <exception cref="System.Data.ConstraintException">
        /// If <paramref name="value"/> is null for a non nullable property.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Invalid cast of <paramref name="value"/> to property type.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// From generated enties via StructuralObject.SetValidValue.
        /// </exception>
        /// <permission cref="System.Security.Permissions.ReflectionPermission">
        /// If the property setter is not public or declaring class is not public.
        /// </permission>
        /// <permission cref="System.Security.NamedPermissionSet">
        /// Demand for FullTrust if the property setter or declaring class has a <see cref="System.Security.Permissions.SecurityAction.LinkDemand"/>
        /// </permission>
        internal static void SetValue(EdmProperty property, object target, object value)
        {
            Action<object, object> setter = GetSetterDelegateForProperty(property);
            setter(target, value);
        }

        /// <summary>For an OSpace property, gets the delegate to set the property value on a clr instance.</summary>
        internal static Action<object, object> GetSetterDelegateForProperty(EdmProperty property)
        {
            Action<object, object> setter = property.ValueSetter;
            if (null == setter)
            {
                setter = CreatePropertySetter(property.EntityDeclaringType, property.PropertySetterHandle,
                        property.Nullable);
                property.ValueSetter = setter;
            }
            Debug.Assert(null != setter, "null setter");
            return setter;
        }

        /// <summary>
        /// Gets the related end instance for the source AssociationEndMember by creating a DynamicMethod to 
        /// call GetRelatedCollection or GetRelatedReference
        /// </summary>
        internal static RelatedEnd GetRelatedEnd(RelationshipManager sourceRelationshipManager, AssociationEndMember sourceMember, AssociationEndMember targetMember, RelatedEnd existingRelatedEnd)
        {
            Func<RelationshipManager, RelatedEnd, RelatedEnd> getRelatedEnd = sourceMember.GetRelatedEnd;
            if (null == getRelatedEnd)
            {
                getRelatedEnd = CreateGetRelatedEndMethod(sourceMember, targetMember);
                sourceMember.GetRelatedEnd = getRelatedEnd;
            }
            Debug.Assert(null != getRelatedEnd, "null getRelatedEnd");

            return getRelatedEnd(sourceRelationshipManager, existingRelatedEnd);
        }

        #region Navigation Property

        internal static Action<object, object> CreateNavigationPropertySetter(Type declaringType, PropertyInfo navigationProperty)
        {
            MethodInfo mi = navigationProperty.GetSetMethod(true);
            Type realType = navigationProperty.PropertyType;

            if (null == mi)
            {
                ThrowPropertyNoSetter();
            }
            if (mi.IsStatic)
            {
                ThrowPropertyIsStatic();
            }
            if (mi.DeclaringType.IsValueType)
            {
                ThrowPropertyDeclaringTypeIsValueType();
            }
            
            // the setter always skips visibility so that we can call our internal method to handle errors
            // because CreateDynamicMethod asserts ReflectionPermission, method is "elevated" and must be treated carefully
            DynamicMethod method = CreateDynamicMethod(mi.Name, typeof(void), new Type[] { typeof(object), typeof(object) });
            ILGenerator gen = method.GetILGenerator();
            GenerateNecessaryPermissionDemands(gen, mi);

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Castclass, declaringType);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Castclass, navigationProperty.PropertyType);
            gen.Emit(OpCodes.Callvirt, mi);       // .Property =
            gen.Emit(OpCodes.Ret);

            return (Action<object, object>)method.CreateDelegate(typeof(Action<object, object>));
        }

        #endregion

        #region get the delegate

        /// <summary>Gets a parameterless constructor for the specified type.</summary>
        /// <param name="type">Type to get constructor for.</param>
        /// <returns>Parameterless constructor for the specified type.</returns>
        internal static ConstructorInfo GetConstructorForType(Type type)
        {
            System.Diagnostics.Debug.Assert(type != null);
            ConstructorInfo ci = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, System.Type.EmptyTypes, null);
            if (null == ci)
            {
                ThrowConstructorNoParameterless(type);
            }
            return ci;
        }


        /// <summary>
        /// generate a delegate equivalent to
        /// private object Constructor() { return new XClass(); }
        /// </summary>
        internal static Delegate CreateConstructor(Type type)
        {
            ConstructorInfo ci = GetConstructorForType(type);

            // because CreateDynamicMethod asserts ReflectionPermission, method is "elevated" and must be treated carefully
            DynamicMethod method = CreateDynamicMethod(ci.DeclaringType.Name, typeof(object), Type.EmptyTypes);
            ILGenerator gen = method.GetILGenerator();
            GenerateNecessaryPermissionDemands(gen, ci);

            gen.Emit(OpCodes.Newobj, ci);
            gen.Emit(OpCodes.Ret);
            return method.CreateDelegate(typeof(Func<object>));
        }

        /// <summary>
        /// generate a delegate equivalent to
        /// private object MemberGetter(object target) { return target.PropertyX; }
        /// or if the property is Nullable<> generate a delegate equivalent to
        /// private object MemberGetter(object target) { Nullable<X> y = target.PropertyX; return ((y.HasValue) ? y.Value : null); }
        /// </summary>
        private static Func<object, object> CreatePropertyGetter(RuntimeTypeHandle entityDeclaringType, RuntimeMethodHandle rmh)
        {
            if (default(RuntimeMethodHandle).Equals(rmh))
            {
                ThrowPropertyNoGetter();
            }

            Debug.Assert(!default(RuntimeTypeHandle).Equals(entityDeclaringType), "Type handle of entity should always be known.");
            var mi = (MethodInfo)MethodBase.GetMethodFromHandle(rmh, entityDeclaringType);

            if (mi.IsStatic)
            {
                ThrowPropertyIsStatic();
            }
            if (mi.DeclaringType.IsValueType)
            {
                ThrowPropertyDeclaringTypeIsValueType();
            }

            if (0 != mi.GetParameters().Length)
            {
                ThrowPropertyIsIndexed();
            }

            Type realType = mi.ReturnType;
            if ((null == realType) || (typeof(void) == realType))
            {
                ThrowPropertyUnsupportedForm();
            }
            if (realType.IsPointer)
            {
                ThrowPropertyUnsupportedType();
            }

            // because CreateDynamicMethod asserts ReflectionPermission, method is "elevated" and must be treated carefully
            DynamicMethod method = CreateDynamicMethod(mi.Name, typeof(object), new Type[] { typeof(object) });
            ILGenerator gen = method.GetILGenerator();
            GenerateNecessaryPermissionDemands(gen, mi);

            // the 'this' target pointer
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Castclass, mi.DeclaringType);
            gen.Emit(mi.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, mi);

            if (realType.IsValueType)
            {
                Type elementType;
                if (realType.IsGenericType && (typeof(Nullable<>) == realType.GetGenericTypeDefinition()))
                {
                    elementType = realType.GetGenericArguments()[0];

                    Label lableFalse = gen.DefineLabel();
                    LocalBuilder local = gen.DeclareLocal(realType);
                    gen.Emit(OpCodes.Stloc_S, local);

                    gen.Emit(OpCodes.Ldloca_S, local);
                    gen.Emit(OpCodes.Call, realType.GetMethod("get_HasValue"));
                    gen.Emit(OpCodes.Brfalse_S, lableFalse);

                    gen.Emit(OpCodes.Ldloca_S, local);
                    gen.Emit(OpCodes.Call, realType.GetMethod("get_Value"));
                    gen.Emit(OpCodes.Box, elementType = realType.GetGenericArguments()[0]);
                    gen.Emit(OpCodes.Ret);

                    gen.MarkLabel(lableFalse);
                    gen.Emit(OpCodes.Ldnull);
                }
                else
                {
                    // need to box to return value as object
                    elementType = realType;
                    gen.Emit(OpCodes.Box, elementType);
                }
            }
            gen.Emit(OpCodes.Ret);
            return (Func<object, object>)method.CreateDelegate(typeof(Func<object, object>));
        }

        /// <summary>
        /// generate a delegate equivalent to
        /// 
        /// // if Property is Nullable value type
        /// private void MemberSetter(object target, object value) {
        ///     if (AllwNull &amp;&amp; (null == value)) {
        ///         ((TargetType)target).PropertyName = default(PropertyType?);
        ///         return;
        ///     }
        ///     if (value is PropertyType) {
        ///             ((TargetType)target).PropertyName = new (PropertyType?)((PropertyType)value);
        ///         return;
        ///     }
        ///     ThrowInvalidValue(value, TargetType.Name, PropertyName);
        ///     return
        /// }
        /// 
        /// // when PropertyType is a value type
        /// private void MemberSetter(object target, object value) {
        ///     if (value is PropertyType) {
        ///             ((TargetType)target).PropertyName = (PropertyType)value;
        ///         return;
        ///     }
        ///     ThrowInvalidValue(value, TargetType.Name, PropertyName);
        ///     return
        /// } 
        /// 
        /// // when PropertyType is a reference type
        /// private void MemberSetter(object target, object value) {
        ///     if ((AllwNull &amp;&amp; (null == value)) || (value is PropertyType)) {
        ///         ((TargetType)target).PropertyName = ((PropertyType)value);
        ///         return;
        ///     }
        ///     ThrowInvalidValue(value, TargetType.Name, PropertyName);
        ///     return
        /// }
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// If the method is missing or static or has indexed parameters.
        /// Or if the delcaring type is a value type.
        /// Or if the parameter type is a pointer.
        /// Or if the method or declaring class has a <see cref="System.Security.Permissions.StrongNameIdentityPermissionAttribute"/>.
        /// </exception>
        private static Action<object, object> CreatePropertySetter(RuntimeTypeHandle entityDeclaringType, RuntimeMethodHandle rmh, bool allowNull)
        {
            MethodInfo mi;
            Type realType;
            ValidateSetterProperty(entityDeclaringType, rmh, out mi, out realType);

            // the setter always skips visibility so that we can call our internal method to handle errors
            // because CreateDynamicMethod asserts ReflectionPermission, method is "elevated" and must be treated carefully
            DynamicMethod method = CreateDynamicMethod(mi.Name, typeof(void), new Type[] { typeof(object), typeof(object) });
            ILGenerator gen = method.GetILGenerator();
            GenerateNecessaryPermissionDemands(gen, mi);

            Type elementType = realType;
            Label labelContinueNull = gen.DefineLabel();
            Label labelContinueValue = gen.DefineLabel();
            Label labelInvalidValue = gen.DefineLabel();
            if (realType.IsValueType)
            {
                if (realType.IsGenericType && (typeof(Nullable<>) == realType.GetGenericTypeDefinition()))
                {
                    elementType = realType.GetGenericArguments()[0];
                }
                else
                {   // force allowNull false for non-nullable value types
                    allowNull = false;
                }
            }

            // ((TargetType)instance)
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Castclass, mi.DeclaringType);

            // if (value is elementType) {
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Isinst, elementType);

            if (allowNull)
            {   // reference type or nullable type
                gen.Emit(OpCodes.Ldarg_1);
                if (elementType == realType)
                {
                    gen.Emit(OpCodes.Brfalse_S, labelContinueNull);             // if (null ==
                }
                else
                {
                    gen.Emit(OpCodes.Brtrue, labelContinueValue);
                    gen.Emit(OpCodes.Pop);                                      // pop Isinst

                    LocalBuilder local = gen.DeclareLocal(realType);
                    gen.Emit(OpCodes.Ldloca_S, local);                          // load valuetype&
                    gen.Emit(OpCodes.Initobj, realType);                        // init &
                    gen.Emit(OpCodes.Ldloc_0);                                  // load valuetype
                    gen.Emit(OpCodes.Br_S, labelContinueNull);
                    gen.MarkLabel(labelContinueValue);
                }
            }
            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Brfalse_S, labelInvalidValue);                     // (arg1 is Inst)

            if (elementType.IsValueType)
            {
                gen.Emit(OpCodes.Unbox_Any, elementType);                       // ((PropertyType)value)

                if (elementType != realType)
                {                                                               // new Nullable<PropertyType>
                    gen.Emit(OpCodes.Newobj, realType.GetConstructor(new Type[] { elementType }));
                }
            }
            gen.MarkLabel(labelContinueNull);
            gen.Emit(mi.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, mi);       // .Property =
            gen.Emit(OpCodes.Ret);

            // ThrowInvalidValue(value, typeof(PropertyType), DeclaringType.Name, PropertyName
            gen.MarkLabel(labelInvalidValue);
            gen.Emit(OpCodes.Pop);                                      // pop Ldarg_0
            gen.Emit(OpCodes.Pop);                                      // pop IsInst'
            gen.Emit(OpCodes.Ldarg_1);                                  // determine if InvalidCast or NullReference
            gen.Emit(OpCodes.Ldtoken, elementType);
            gen.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public));
            gen.Emit(OpCodes.Ldstr, mi.DeclaringType.Name);
            gen.Emit(OpCodes.Ldstr, mi.Name.Substring(4)); // substring to strip "set_"
            Debug.Assert(null != (Action<Object,Type,String,String>)EntityUtil.ThrowSetInvalidValue, "missing method ThrowSetInvalidValue(object,Type,string,string)");
            gen.Emit(OpCodes.Call, typeof(EntityUtil).GetMethod("ThrowSetInvalidValue", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(object),typeof(Type),typeof(string),typeof(string)},null));
            gen.Emit(OpCodes.Ret);
            return (Action<object, object>)method.CreateDelegate(typeof(Action<object, object>));
        }

        internal static void ValidateSetterProperty(RuntimeTypeHandle entityDeclaringType, RuntimeMethodHandle setterMethodHandle, out MethodInfo setterMethodInfo, out Type realType)
        {
            if (default(RuntimeMethodHandle).Equals(setterMethodHandle))
            {
                ThrowPropertyNoSetter();
            }

            Debug.Assert(!default(RuntimeTypeHandle).Equals(entityDeclaringType), "Type handle of entity should always be known.");
            setterMethodInfo = (MethodInfo)MethodBase.GetMethodFromHandle(setterMethodHandle, entityDeclaringType);

            if (setterMethodInfo.IsStatic)
            {
                ThrowPropertyIsStatic();
            }
            if (setterMethodInfo.DeclaringType.IsValueType)
            {
                ThrowPropertyDeclaringTypeIsValueType();
            }

            ParameterInfo[] parameters = setterMethodInfo.GetParameters();
            if ((null == parameters) || (1 != parameters.Length))
            {   // if no parameters (i.e. not a set_Property method), will still throw this message
                ThrowPropertyIsIndexed();
            }
            realType = setterMethodInfo.ReturnType;
            if ((null != realType) && (typeof(void) != realType))
            {
                ThrowPropertyUnsupportedForm();
            }

            realType = parameters[0].ParameterType;
            if (realType.IsPointer)
            {
                ThrowPropertyUnsupportedType();
            }
        }

        /// <summary>Determines if the specified method requires permission demands to be invoked safely.</summary>
        /// <param name="mi">Method instance to check.</param>
        /// <returns>true if the specified method requires permission demands to be invoked safely, false otherwise.</returns>
        internal static bool RequiresPermissionDemands(MethodBase mi)
        {
            System.Diagnostics.Debug.Assert(mi != null);
            return !IsPublic(mi);
        }

        private static void GenerateNecessaryPermissionDemands(ILGenerator gen, MethodBase mi)
        {
            if (!IsPublic(mi))
            {
                gen.Emit(OpCodes.Ldsfld, typeof(LightweightCodeGenerator).GetField("MemberAccessReflectionPermission", BindingFlags.Static | BindingFlags.NonPublic));
                gen.Emit(OpCodes.Callvirt, typeof(ReflectionPermission).GetMethod("Demand"));
            }
        }

        internal static bool IsPublic(MethodBase method)
        {
            return (method.IsPublic && IsPublic(method.DeclaringType));
        }

        internal static bool IsPublic(Type type)
        {
            return ((null == type) || (type.IsPublic && IsPublic(type.DeclaringType)));
        }

        /// <summary>
        /// Create delegate used to invoke either the GetRelatedReference or GetRelatedCollection generic method on the RelationshipManager.
        /// </summary>        
        /// <param name="sourceMember">source end of the relationship for the requested navigation</param>
        /// <param name="targetMember">target end of the relationship for the requested navigation</param>
        /// <returns>Delegate that can be used to invoke the corresponding method.</returns>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static Func<RelationshipManager, RelatedEnd, RelatedEnd> CreateGetRelatedEndMethod(AssociationEndMember sourceMember, AssociationEndMember targetMember)
        {
            Debug.Assert(sourceMember.DeclaringType == targetMember.DeclaringType, "Source and Target members must be in the same DeclaringType");

            EntityType sourceEntityType = MetadataHelper.GetEntityTypeForEnd(sourceMember);
            EntityType targetEntityType = MetadataHelper.GetEntityTypeForEnd(targetMember);
            NavigationPropertyAccessor sourceAccessor = MetadataHelper.GetNavigationPropertyAccessor(targetEntityType, targetMember, sourceMember);
            NavigationPropertyAccessor targetAccessor = MetadataHelper.GetNavigationPropertyAccessor(sourceEntityType, sourceMember, targetMember);

            MethodInfo genericCreateRelatedEndMethod = typeof(LightweightCodeGenerator).GetMethod("CreateGetRelatedEndMethod", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(AssociationEndMember), typeof(AssociationEndMember), typeof(NavigationPropertyAccessor), typeof(NavigationPropertyAccessor) }, null);
            Debug.Assert(genericCreateRelatedEndMethod != null, "Could not find method LightweightCodeGenerator.CreateGetRelatedEndMethod");

            MethodInfo createRelatedEndMethod = genericCreateRelatedEndMethod.MakeGenericMethod(sourceEntityType.ClrType, targetEntityType.ClrType);
            object getRelatedEndDelegate = createRelatedEndMethod.Invoke(null, new object[] { sourceMember, targetMember, sourceAccessor, targetAccessor });

            return (Func<RelationshipManager, RelatedEnd, RelatedEnd>)getRelatedEndDelegate;
        }

        private static Func<RelationshipManager, RelatedEnd, RelatedEnd> CreateGetRelatedEndMethod<TSource, TTarget>(AssociationEndMember sourceMember, AssociationEndMember targetMember, NavigationPropertyAccessor sourceAccessor, NavigationPropertyAccessor targetAccessor)
            where TSource : class
            where TTarget : class
        {
            Func<RelationshipManager, RelatedEnd, RelatedEnd> getRelatedEnd;

            // Get the appropriate method, either collection or reference depending on the target multiplicity
            switch (targetMember.RelationshipMultiplicity)
            {
                case RelationshipMultiplicity.ZeroOrOne:
                case RelationshipMultiplicity.One:
                    {
                        getRelatedEnd = (manager, relatedEnd) =>
                            manager.GetRelatedReference<TSource, TTarget>(sourceMember.DeclaringType.FullName,
                                                                          sourceMember.Name,
                                                                          targetMember.Name,
                                                                          sourceAccessor,
                                                                          targetAccessor,
                                                                          sourceMember.RelationshipMultiplicity,
                                                                          relatedEnd);
                        
                        break;
                    }
                case RelationshipMultiplicity.Many:
                    {
                        getRelatedEnd = (manager, relatedEnd) =>
                            manager.GetRelatedCollection<TSource, TTarget>(sourceMember.DeclaringType.FullName,
                                                                           sourceMember.Name,
                                                                           targetMember.Name,
                                                                           sourceAccessor,
                                                                           targetAccessor,
                                                                           sourceMember.RelationshipMultiplicity,
                                                                           relatedEnd);

                        break;
                    }
                default:
                    throw EntityUtil.InvalidEnumerationValue(typeof(RelationshipMultiplicity), (int)targetMember.RelationshipMultiplicity);
            }

            return getRelatedEnd;
        }

        private static void ThrowConstructorNoParameterless(Type type)
        {
            throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.CodeGen_ConstructorNoParameterless(type.FullName));
        }
        private static void ThrowPropertyDeclaringTypeIsValueType()
        {
            throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.CodeGen_PropertyDeclaringTypeIsValueType);
        }
        private static void ThrowPropertyUnsupportedForm()
        {
            throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.CodeGen_PropertyUnsupportedForm);
        }
        private static void ThrowPropertyUnsupportedType()
        {
            throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.CodeGen_PropertyUnsupportedType);
        }
        private static void ThrowPropertyStrongNameIdentity()
        {
            throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.CodeGen_PropertyStrongNameIdentity);
        }
        private static void ThrowPropertyIsIndexed()
        {
            throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.CodeGen_PropertyIsIndexed);
        }
        private static void ThrowPropertyIsStatic()
        {
            throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.CodeGen_PropertyIsStatic);
        }
        private static void ThrowPropertyNoGetter()
        {
            throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.CodeGen_PropertyNoGetter);
        }
        private static void ThrowPropertyNoSetter()
        {
            throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.CodeGen_PropertyNoSetter);
        }

        #endregion

        #region Lightweight code generation

        internal static readonly ReflectionPermission MemberAccessReflectionPermission = new ReflectionPermission(ReflectionPermissionFlag.MemberAccess);

        internal static bool HasMemberAccessReflectionPermission()
        {
            try
            {
                MemberAccessReflectionPermission.Demand();
                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        // we could cache more, like 'new Type[] { ... }' and 'typeof(object)'
        // but pruned as much as possible for the workingset helps, even little things

        // Assert MemberAccess to skip visibility check & ReflectionEmit so we can generate the method (make calls to EF internals).
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2128")]
        [System.Security.SecuritySafeCritical]
        [ReflectionPermission(SecurityAction.Assert, MemberAccess = true)]
        internal static DynamicMethod CreateDynamicMethod(string name, Type returnType, Type[] parameterTypes)
        {
            // Create a transparent dynamic method (Module not specified) to ensure we do not satisfy any link demands
            // in method callees.
            return new DynamicMethod(name, returnType, parameterTypes, true);
        }

        #endregion
    }
}
