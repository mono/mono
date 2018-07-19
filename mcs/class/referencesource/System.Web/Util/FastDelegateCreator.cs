//------------------------------------------------------------------------------
// <copyright file="FastDelegateCreator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Security.Permissions;

    // Helper utility to quickly create a Delegate by calling its unmanaged ctor

    [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
    internal static class FastDelegateCreator<TDelegate> where TDelegate : class {

        private static readonly Func<object, IntPtr, TDelegate> _factory = GetFactory();

        // obj is the 'this' pointer (potentially null)
        // method is RuntimeMethodHandle.GetFunctionPointer()
        internal static TDelegate BindTo(object obj, IntPtr method) {
            return _factory(obj, method);
        }

        // obj is the 'this' pointer (potentially null)
        // method is the MethodInfo to bind to
        internal static TDelegate BindTo(object obj, MethodInfo method) {
            return BindTo(obj, method.MethodHandle.GetFunctionPointer());
        }

        // Assert needed since we'll be asked to create delegates to potentially private / protected methods
        [ReflectionPermission(SecurityAction.Assert, MemberAccess = true)]
        private static Func<object, IntPtr, TDelegate> GetFactory() {
            // All delegates have a .ctor(object @object, IntPtr method), but since we can't call this
            // directly from C# we just need to LCG it.
            ConstructorInfo delegateCtor = typeof(TDelegate).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });

            // Define method FastCreateDelegate_X: (object, IntPtr) -> TDelegate
            DynamicMethod dynamicMethod = new DynamicMethod(
                name: "FastCreateDelegate_" + typeof(TDelegate).Name,
                returnType: typeof(TDelegate),
                parameterTypes: new Type[] { typeof(object), typeof(IntPtr) },
                owner: typeof(FastDelegateCreator<TDelegate>),
                skipVisibility: true);

            // return new TDelegate(obj, method);
            ILGenerator ilGen = dynamicMethod.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0); // Stack contains ('this')
            ilGen.Emit(OpCodes.Ldarg_1); // Stack contains ('this', 'method')
            ilGen.Emit(OpCodes.Newobj, delegateCtor); // Stack contains (delegate)
            ilGen.Emit(OpCodes.Ret);

            return (Func<object, IntPtr, TDelegate>)dynamicMethod.CreateDelegate(typeof(Func<object, IntPtr, TDelegate>));
        }

    }
}
