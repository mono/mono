// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Internal;
using System.Reflection.Emit;
using System.Collections;

namespace System.ComponentModel.Composition
{
    // // Assume TMetadataView is
    // //interface Foo
    // //{
    // //    public typeRecord1 Record1 { get; }
    // //    public typeRecord2 Record2 { get; }
    // //    public typeRecord3 Record3 { get; }
    // //    public typeRecord4 Record4 { get; }
    // //}
    // // The class to be generated will look approximately like:
    // public class __Foo__MedataViewProxy : TMetadataView
    // {
    //     public __Foo__MedataViewProxy (IDictionary<string, object> metadata)
    //     {
    //         if(metadata == null)
    //         {
    //             throw InvalidArgumentException("metadata");
    //         }
    //         try
    //         {
    //              Record1 = (typeRecord1)Record1;
    //              Record2 = (typeRecord1)Record2;
    //              Record3 = (typeRecord1)Record3;
    //              Record4 = (typeRecord1)Record4;
    //          }
    //          catch(InvalidCastException ice)
    //          {
    //              //Annotate exception .Data with diagnostic info
    //          }
    //          catch(NulLReferenceException ice)
    //          {
    //              //Annotate exception .Data with diagnostic info
    //          }
    //     }
    //     // Interface
    //     public typeRecord1 Record1 { get; }
    //     public typeRecord2 Record2 { get; }
    //     public typeRecord3 Record3 { get; }
    //     public typeRecord4 Record4 { get; }
    // }
    internal static class MetadataViewGenerator
    {
        public const string MetadataViewType       = "MetadataViewType";
        public const string MetadataItemKey        = "MetadataItemKey";
        public const string MetadataItemTargetType = "MetadataItemTargetType";
        public const string MetadataItemSourceType = "MetadataItemSourceType";
        public const string MetadataItemValue      = "MetadataItemValue";

        private static Lock _lock = new Lock();
        private static Dictionary<Type, Type> _proxies = new Dictionary<Type, Type>();

        private static AssemblyName ProxyAssemblyName = new AssemblyName(string.Format(CultureInfo.InvariantCulture, "MetadataViewProxies_{0}", Guid.NewGuid()));
        private static AssemblyBuilder ProxyAssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(ProxyAssemblyName, AssemblyBuilderAccess.Run);
        private static ModuleBuilder ProxyModuleBuilder = ProxyAssemblyBuilder.DefineDynamicModule("MetadataViewProxiesModule");
        private static Type[] CtorArgumentTypes = new Type[] { typeof(IDictionary<string, object>) };
        private static MethodInfo _mdvDictionaryTryGet = CtorArgumentTypes[0].GetMethod("TryGetValue");
        private static readonly MethodInfo ObjectGetType = typeof(object).GetMethod("GetType", Type.EmptyTypes);

        public static Type GenerateView(Type viewType)
        {
            Assumes.NotNull(viewType);
            Assumes.IsTrue(viewType.IsInterface);

            Type proxyType;
            bool foundProxy;

            using (new ReadLock(_lock))
            {
                foundProxy = _proxies.TryGetValue(viewType, out proxyType);
            }

            // No factory exists
            if(!foundProxy)
            {
                // Try again under a write lock if still none generate the proxy
                using (new WriteLock(_lock))
                {
                    foundProxy = _proxies.TryGetValue(viewType, out proxyType);

                    if (!foundProxy)
                    {
                        proxyType = GenerateInterfaceViewProxyType(viewType);
                        Assumes.NotNull(proxyType);

                        _proxies.Add(viewType, proxyType);
                    }
                }
            }

            return proxyType;
        }

        // This must be called with _readerWriterLock held for Write
        private static Type GenerateInterfaceViewProxyType(Type viewType)
        {
            // View type is an interface let's cook an implementation
            Type proxyType;
            TypeBuilder proxyTypeBuilder;
            Type[] interfaces = { viewType };

            proxyTypeBuilder = ProxyModuleBuilder.DefineType(
                string.Format(CultureInfo.InvariantCulture, "_proxy_{0}_{1}", viewType.FullName, Guid.NewGuid()),
                TypeAttributes.Public,
                typeof(object),
                interfaces);

            // Implement Constructor
            ILGenerator proxyCtorIL = proxyTypeBuilder.CreateGeneratorForPublicConstructor(CtorArgumentTypes);
            LocalBuilder exception = proxyCtorIL.DeclareLocal(typeof(Exception));
            LocalBuilder exceptionData = proxyCtorIL.DeclareLocal(typeof(IDictionary));
            LocalBuilder sourceType = proxyCtorIL.DeclareLocal(typeof(Type));
            LocalBuilder value = proxyCtorIL.DeclareLocal(typeof(object));

            Label tryConstructView = proxyCtorIL.BeginExceptionBlock();

            // Implement interface properties
            foreach (PropertyInfo propertyInfo in viewType.GetAllProperties())
            {
                string fieldName = string.Format(CultureInfo.InvariantCulture, "_{0}_{1}", propertyInfo.Name, Guid.NewGuid());

                // Cache names and type for exception
                string propertyName = string.Format(CultureInfo.InvariantCulture, "{0}", propertyInfo.Name);

                Type[] propertyTypeArguments = new Type[] { propertyInfo.PropertyType };
                Type[] optionalModifiers = null;
                Type[] requiredModifiers = null;

#if !SILVERLIGHT
                // PropertyInfo does not support GetOptionalCustomModifiers and GetRequiredCustomModifiers on Silverlight
                optionalModifiers = propertyInfo.GetOptionalCustomModifiers();
                requiredModifiers = propertyInfo.GetRequiredCustomModifiers();
                Array.Reverse(optionalModifiers);
                Array.Reverse(requiredModifiers);
#endif
                // Generate field
                FieldBuilder proxyFieldBuilder = proxyTypeBuilder.DefineField(
                    fieldName,
                    propertyInfo.PropertyType,
                    FieldAttributes.Private);

                // Generate property
                PropertyBuilder proxyPropertyBuilder = proxyTypeBuilder.DefineProperty(
                    propertyName,
                    PropertyAttributes.None,
                    propertyInfo.PropertyType,
                    propertyTypeArguments);

                // Generate constructor code for retrieving the metadata value and setting the field
                Label doneGettingDefaultValue = proxyCtorIL.DefineLabel();

                // In constructor set the backing field with the value from the dictionary
                proxyCtorIL.Emit(OpCodes.Ldarg_1);
                proxyCtorIL.Emit(OpCodes.Ldstr, propertyInfo.Name);
                proxyCtorIL.Emit(OpCodes.Ldloca, value);
                proxyCtorIL.Emit(OpCodes.Callvirt, _mdvDictionaryTryGet);
                proxyCtorIL.Emit(OpCodes.Brtrue, doneGettingDefaultValue);

                object[] attrs = propertyInfo.GetCustomAttributes(typeof(DefaultValueAttribute), false);
                if (attrs.Length > 0)
                {
                    DefaultValueAttribute defaultAttribute = (DefaultValueAttribute)attrs[0];
                    proxyCtorIL.LoadValue(defaultAttribute.Value);
                    if ((defaultAttribute.Value != null) && (defaultAttribute.Value.GetType().IsValueType))
                    {
                        proxyCtorIL.Emit(OpCodes.Box, defaultAttribute.Value.GetType());
                    }
                    proxyCtorIL.Emit(OpCodes.Stloc, value);
                }

                proxyCtorIL.MarkLabel(doneGettingDefaultValue);

                Label tryCastValue = proxyCtorIL.BeginExceptionBlock();
                proxyCtorIL.Emit(OpCodes.Ldarg_0);
                proxyCtorIL.Emit(OpCodes.Ldloc, value);

                proxyCtorIL.Emit(propertyInfo.PropertyType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, propertyInfo.PropertyType);
                proxyCtorIL.Emit(OpCodes.Stfld, proxyFieldBuilder);
                proxyCtorIL.Emit(OpCodes.Leave, tryCastValue);

                // catch blocks for tryCast start here
                proxyCtorIL.BeginCatchBlock(typeof(NullReferenceException));
                {
                    proxyCtorIL.Emit(OpCodes.Stloc, exception);

                    proxyCtorIL.GetExceptionDataAndStoreInLocal(exception, exceptionData);
                    proxyCtorIL.AddItemToLocalDictionary(exceptionData, MetadataItemKey, propertyName);
                    proxyCtorIL.AddItemToLocalDictionary(exceptionData, MetadataItemTargetType, propertyInfo.PropertyType);
                    proxyCtorIL.Emit(OpCodes.Rethrow);
                }

                proxyCtorIL.BeginCatchBlock(typeof(InvalidCastException));
                {
                    proxyCtorIL.Emit(OpCodes.Stloc, exception);

                    proxyCtorIL.GetExceptionDataAndStoreInLocal(exception, exceptionData);
                    proxyCtorIL.AddItemToLocalDictionary(exceptionData, MetadataItemKey, propertyName);
                    proxyCtorIL.AddItemToLocalDictionary(exceptionData, MetadataItemTargetType, propertyInfo.PropertyType);
                    proxyCtorIL.Emit(OpCodes.Rethrow);
                }
                proxyCtorIL.EndExceptionBlock();


                if (propertyInfo.CanWrite)
                {
                    // The MetadataView '{0}' is invalid because property '{1}' has a property set method.
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,
                        Strings.InvalidSetterOnMetadataField,
                        viewType,
                        propertyName));
                }
                if (propertyInfo.CanRead)
                {
                    // Generate "get" method implementation.
                    MethodBuilder getMethodBuilder = proxyTypeBuilder.DefineMethod(
                        string.Format(CultureInfo.InvariantCulture, "get_{0}", propertyName),
                        MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                        CallingConventions.HasThis,
                        propertyInfo.PropertyType,
                        requiredModifiers,
                        optionalModifiers,
                        Type.EmptyTypes, null, null);

                    proxyTypeBuilder.DefineMethodOverride(getMethodBuilder, propertyInfo.GetGetMethod());
                    ILGenerator getMethodIL = getMethodBuilder.GetILGenerator();
                    getMethodIL.Emit(OpCodes.Ldarg_0);
                    getMethodIL.Emit(OpCodes.Ldfld, proxyFieldBuilder);
                    getMethodIL.Emit(OpCodes.Ret);

                    proxyPropertyBuilder.SetGetMethod(getMethodBuilder);
                }
            }

            proxyCtorIL.Emit(OpCodes.Leave, tryConstructView);

            // catch blocks for constructView start here
            proxyCtorIL.BeginCatchBlock(typeof(NullReferenceException));
            {
                proxyCtorIL.Emit(OpCodes.Stloc, exception);

                proxyCtorIL.GetExceptionDataAndStoreInLocal(exception, exceptionData);
                proxyCtorIL.AddItemToLocalDictionary(exceptionData, MetadataViewType, viewType);
                proxyCtorIL.Emit(OpCodes.Rethrow);
            }
            proxyCtorIL.BeginCatchBlock(typeof(InvalidCastException));
            {
                proxyCtorIL.Emit(OpCodes.Stloc, exception);

                proxyCtorIL.GetExceptionDataAndStoreInLocal(exception, exceptionData);
                proxyCtorIL.Emit(OpCodes.Ldloc, value);
                proxyCtorIL.Emit(OpCodes.Call, ObjectGetType);
                proxyCtorIL.Emit(OpCodes.Stloc, sourceType);
                proxyCtorIL.AddItemToLocalDictionary(exceptionData, MetadataViewType, viewType);
                proxyCtorIL.AddLocalToLocalDictionary(exceptionData, MetadataItemSourceType, sourceType);
                proxyCtorIL.AddLocalToLocalDictionary(exceptionData, MetadataItemValue, value);
                proxyCtorIL.Emit(OpCodes.Rethrow);
            }
            proxyCtorIL.EndExceptionBlock();

            // Finished implementing interface and constructor
            proxyCtorIL.Emit(OpCodes.Ret);
            proxyType = proxyTypeBuilder.CreateType();

            return proxyType;
        }
             
    }
}
