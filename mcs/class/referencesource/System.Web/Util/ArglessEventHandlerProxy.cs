//------------------------------------------------------------------------------
// <copyright file="ArglessEventHandlerProxy.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Security.Permissions;
    using System.Security;

/*
 * Proxy that provides EventHandler and redirects it to arg-less method on the given object
 */
    internal class ArglessEventHandlerProxy {
        private Object _target;
        private MethodInfo _arglessMethod;

        internal ArglessEventHandlerProxy(Object target, MethodInfo arglessMethod) {
            Debug.Assert(arglessMethod.GetParameters().Length == 0);

            _target = target;
            _arglessMethod = arglessMethod;
        }

        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.RestrictedMemberAccess)]
        internal void Callback(Object sender, EventArgs e) {
            _arglessMethod.Invoke(_target, new Object[0]);
        }

        internal EventHandler Handler {
            get {
                return new EventHandler(Callback);
            }
        }
    }

    internal delegate void VoidMethod();

    internal class CalliEventHandlerDelegateProxy {
        private delegate void ParameterlessDelegate();
        private delegate void ParameterfulDelegate(object sender, EventArgs e);

        private IntPtr _functionPointer;
        private Object _target;
        private bool _argless;

        internal CalliEventHandlerDelegateProxy(Object target, IntPtr functionPointer, bool argless) {
            _argless = argless;
            _target = target;
            _functionPointer = functionPointer;
        }

        internal void Callback(Object sender, EventArgs e) {
            if (_argless) {
                ParameterlessDelegate del = FastDelegateCreator<ParameterlessDelegate>.BindTo(_target, _functionPointer);
                del();
            }
            else {
                ParameterfulDelegate del = FastDelegateCreator<ParameterfulDelegate>.BindTo(_target, _functionPointer);
                del(sender, e);
            }
        }

        internal EventHandler Handler {
            get {
                return new EventHandler(Callback);
            }
        }
    }

#if LCG_Implementation
    internal delegate void ArglessMethod(IntPtr methodPtr, Object target);
    internal delegate void EventArgMethod(IntPtr methodPtr, Object target, Object source, EventArgs e);

    internal class CalliHelper {
        internal static ArglessMethod ArglessFunctionCaller;
        internal static EventArgMethod EventArgFunctionCaller;

        // Make sure we have reflection permission to use ReflectionEmit and access method
        [ReflectionPermission(SecurityAction.Assert, Flags = ReflectionPermissionFlag.ReflectionEmit | ReflectionPermissionFlag.MemberAccess)]
        static CalliHelper() {
            // generate void(void) calli
            DynamicMethod dm = new DynamicMethod("void_calli",
                                                    typeof(void),
                                                    new Type[] { typeof(IntPtr) /* function ptr */, typeof(Object) /* target */},
                                                    typeof(CalliHelper).Module);
            ILGenerator ilg = dm.GetILGenerator();

            ilg.Emit(OpCodes.Ldarg_1);
            ilg.Emit(OpCodes.Ldarg_0);
            ilg.EmitCalli(OpCodes.Calli, CallingConventions.HasThis, typeof(void), new Type[0], null);
            ilg.Emit(OpCodes.Ret);
            ArglessFunctionCaller = (ArglessMethod)dm.CreateDelegate(typeof(ArglessMethod));

            // generate void(Object, EventArgs) calli
            dm = new DynamicMethod("eventarg_calli",
                                    typeof(void),
                                    new Type[] { typeof(IntPtr) /* function ptr */, typeof(Object) /* target */, typeof(Object) /* sender */, typeof(EventArgs) },
                                    typeof(CalliHelper).Module);
            ilg = dm.GetILGenerator();

            ilg.Emit(OpCodes.Ldarg_1);
            ilg.Emit(OpCodes.Ldarg_2);
            ilg.Emit(OpCodes.Ldarg_3);
            ilg.Emit(OpCodes.Ldarg_0);
            ilg.EmitCalli(OpCodes.Calli, CallingConventions.HasThis, typeof(void), new Type[] { typeof(object) /* sender */, typeof(EventArgs) }, null);
            ilg.Emit(OpCodes.Ret);
            EventArgFunctionCaller = (EventArgMethod)dm.CreateDelegate(typeof(EventArgMethod));
        }
    }
#endif //LCG_Implementation

#if  Reflection_Emit_Implementation
    internal delegate void ArglessMethod(IntPtr methodPtr, Object target);
    internal delegate void EventArgMethod(IntPtr methodPtr, Object target, Object source, EventArgs e);

    internal class CalliHelper {

        internal static ArglessMethod ArglessFunctionCaller;
        internal static EventArgMethod EventArgFunctionCaller;

        // Make sure we have reflection permission to use ReflectionEmit and access method
        [ReflectionPermission(SecurityAction.Assert, Flags = ReflectionPermissionFlag.ReflectionEmit | ReflectionPermissionFlag.MemberAccess)]
        static CalliHelper() {

            AssemblyName an = new AssemblyName("CalliHelper");
            AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            ModuleBuilder mb = ab.DefineDynamicModule("CalliHelper.dll");

            ConstructorInfo ci = typeof(CLSCompliantAttribute).GetConstructor(new Type[] { typeof(bool) });
            CustomAttributeBuilder cb = new CustomAttributeBuilder(ci, new object[] { true });
            mb.SetCustomAttribute(cb);

            TypeBuilder tb = mb.DefineType("System.Web.Util.CalliHelper", TypeAttributes.NotPublic | TypeAttributes.Sealed);
            MethodBuilder methb = tb.DefineMethod("EventArgMethod", MethodAttributes.Assembly | MethodAttributes.Static, typeof(void), new Type[] { typeof(IntPtr), typeof(object), typeof(object), typeof(EventArgs) });
            ILGenerator ilg = methb.GetILGenerator();

            ilg.Emit(OpCodes.Ldarg_1);
            ilg.Emit(OpCodes.Ldarg_2);
            ilg.Emit(OpCodes.Ldarg_3);
            ilg.Emit(OpCodes.Ldarg_0);
            ilg.EmitCalli(OpCodes.Calli, CallingConventions.HasThis, typeof(void), new Type[] { typeof(object), typeof(EventArgs) }, null);
            ilg.Emit(OpCodes.Ret);

            methb = tb.DefineMethod("ArglessMethod", MethodAttributes.Assembly | MethodAttributes.Static, typeof(void), new Type[] { typeof (IntPtr), typeof(object) });
            ilg = methb.GetILGenerator();

            ilg.Emit(OpCodes.Ldarg_1);
            ilg.Emit(OpCodes.Ldarg_0);
            ilg.EmitCalli(OpCodes.Calli, CallingConventions.HasThis, typeof(void), new Type[0], null);
            ilg.Emit(OpCodes.Ret);

            Type t = tb.CreateType();
            ArglessFunctionCaller = (ArglessMethod)Delegate.CreateDelegate(typeof(ArglessMethod), t, "ArglessMethod", true);
            EventArgFunctionCaller = (EventArgMethod)Delegate.CreateDelegate(typeof(EventArgMethod), t, "EventArgMethod", true);

            //ab.Save("CalliHelper.dll");
        }
    }
#endif // Reflection_Emit_Implementation
}
