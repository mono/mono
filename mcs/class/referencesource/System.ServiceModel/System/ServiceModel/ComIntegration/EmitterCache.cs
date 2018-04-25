//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Threading;

    class EmitterCache
    {
        static EmitterCache Provider = null;
        static object initLock = new object();
        internal static EmitterCache TypeEmitter
        {
            get
            {
                lock (initLock)
                {
                    if (Provider == null)
                    {
                        EmitterCache localProvider = new EmitterCache();
                        Thread.MemoryBarrier();
                        Provider = localProvider;
                    }
                }

                if (Provider == null)
                {
                    throw Fx.AssertAndThrowFatal("Provider should not be null");
                }
                return Provider;
            }
        }

        ModuleBuilder DynamicModule;
        AssemblyBuilder assemblyBuilder;
        Dictionary<Type, Type> interfaceToClassMap;

        private EmitterCache()
        {
            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = Guid.NewGuid().ToString();
            assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            DynamicModule = assemblyBuilder.DefineDynamicModule(Guid.NewGuid().ToString());
            interfaceToClassMap = new Dictionary<Type, Type>();
        }
        private Type[] GetParameterTypes(MethodInfo mInfo)
        {
            ParameterInfo[] parameters = mInfo.GetParameters();
            Type[] typeArray = new Type[parameters.Length];
            int index = 0;
            for (; index < parameters.Length; index++)
            {
                typeArray[index] = parameters[index].ParameterType;
            }
            return typeArray;
        }

        [SuppressMessage(FxCop.Category.Usage, "CA2301:EmbeddableTypesInContainersRule", MessageId = "interfaceToClassMap", Justification = "No need to support type equivalence here.")]
        internal Type FindOrCreateType(Type interfaceType)
        {
            if (!interfaceType.IsInterface)
            {
                throw Fx.AssertAndThrow("Passed in type should be an Interface");
            }
            Type classType = null;
            lock (this)
            {
                interfaceToClassMap.TryGetValue(interfaceType, out classType);
                if (classType == null)
                {
                    TypeBuilder typeBuilder = DynamicModule.DefineType(interfaceType.Name + "MarshalByRefObject", TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract,
                       typeof(MarshalByRefObject), new Type[] { interfaceType });
                    Type[] ctorParams = new Type[] { typeof(ClassInterfaceType) };
                    ConstructorInfo classCtorInfo = typeof(ClassInterfaceAttribute).GetConstructor(ctorParams);
                    CustomAttributeBuilder attributeBuilder = new CustomAttributeBuilder(classCtorInfo,
                                                               new object[] { ClassInterfaceType.None });
                    typeBuilder.SetCustomAttribute(attributeBuilder);
                    typeBuilder.AddInterfaceImplementation(interfaceType);
                    foreach (MethodInfo mInfo in interfaceType.GetMethods())
                    {
                        MethodBuilder methodInClass = null;
                        methodInClass = typeBuilder.DefineMethod(mInfo.Name,
                        MethodAttributes.Public | MethodAttributes.Virtual |
                             MethodAttributes.Abstract | MethodAttributes.Abstract | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                        mInfo.ReturnType, GetParameterTypes(mInfo));
                    }
                    classType = typeBuilder.CreateType();
                    interfaceToClassMap[interfaceType] = classType;

                }
            }
            if (classType == null)
            {
                throw Fx.AssertAndThrow("Class Type should not be null at this point");
            }
            return classType;
        }
    }
}
