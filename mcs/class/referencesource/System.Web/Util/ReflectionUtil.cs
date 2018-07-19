//------------------------------------------------------------------------------
// <copyright file="ReflectionUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Security.Permissions;

    // Provides helper methods for performing reflection over managed objects.

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    internal static class ReflectionUtil {

        // Resets an object to its "default" state, e.g. where each instance field is given the value default(TField).
        public static void Reset<T>(T obj) where T : class {
            ResetUtil<T>.ResetFn(obj);
        }

        private static class ResetUtil<T> where T : class {
            internal readonly static Action<T> ResetFn = CreateResetFn();

            private static Action<T> CreateResetFn() {
                Type targetType = typeof(T);
                DynamicMethod dynamicMethod = CreateDynamicMethodWithAssert();
                ILGenerator ilGen = dynamicMethod.GetILGenerator();

                // for each field in the target type, reset to default(TField)
                FieldInfo[] allFields = targetType.GetFields(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (FieldInfo fieldInfo in allFields) {
                    if (fieldInfo.IsInitOnly || fieldInfo.IsDefined(typeof(DoNotResetAttribute))) {
                        // This field is not eligible to be reset because it is marked readonly or [DoNotReset].
                        continue;
                    }

                    // obj.field = default(TField);
                    // Opcodes.Initobj can be used for both value and reference types; see ECMA 335, Partition III, Sec. 4.5 "initobj"
                    // (ref: http://www.ecma-international.org/publications/files/ECMA-ST/ECMA-335.pdf)
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Ldflda, fieldInfo);
                    ilGen.Emit(OpCodes.Initobj, fieldInfo.FieldType);
                }

                ilGen.Emit(OpCodes.Ret);
                // dynamicMethod = obj => {
                //   obj.field1 = default(TField1);
                //   obj.field2 = default(TField2);
                //   ...
                // };
                return (Action<T>)dynamicMethod.CreateDelegate(typeof(Action<T>));
            }

            [ReflectionPermission(SecurityAction.Assert, MemberAccess = true)] // needed to create a DynamicMethod inside the target type
            private static DynamicMethod CreateDynamicMethodWithAssert() {
                Type targetType = typeof(T);
                return new DynamicMethod(
                    name: "Reset-" + targetType.Name,
                    returnType: typeof(void),
                    parameterTypes: new Type[] { targetType },
                    owner: targetType,
                    skipVisibility: true);
            }
        }

    }
}
