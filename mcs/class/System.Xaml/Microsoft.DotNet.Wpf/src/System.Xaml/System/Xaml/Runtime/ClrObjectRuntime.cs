// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;
using System.Security;
using System.Xaml;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using XAML3 = System.Windows.Markup;
using System.Xaml.Schema;
using System.Text;

namespace MS.Internal.Xaml.Runtime
{
    class ClrObjectRuntime : XamlRuntime
    {
        private bool _ignoreCanConvert;
        private bool _isWriter;

        public override IAddLineInfo LineInfo { get; set; }

        public ClrObjectRuntime(XamlRuntimeSettings settings, bool isWriter)
        {
            if (settings != null)
            {
                _ignoreCanConvert = settings.IgnoreCanConvert;
            }
            _isWriter = isWriter;
        }

        private static Exception UnwrapTargetInvocationException(Exception e)
        {
            if(e is TargetInvocationException && e.InnerException != null)
            {
                return e.InnerException;
            }
            return e;
        }

        public override object CreateInstance(XamlType xamlType, object[] args)
        {
            if (xamlType.IsUnknown)
            {
                throw CreateException(SR.Get(SRID.CannotCreateBadType, xamlType.Name));
            }
            try
            {
                return CreateInstanceWithCtor(xamlType, args);
            }
            catch (MissingMethodException ex)
            {
                throw CreateException(SR.Get(SRID.NoConstructor, xamlType.UnderlyingType), ex);
            }
            catch (Exception ex)
            {
                if (CriticalExceptions.IsCriticalException(ex))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.ConstructorInvocation, xamlType.UnderlyingType), UnwrapTargetInvocationException(ex));
            }
        }

        protected virtual object CreateInstanceWithCtor(XamlType xamlType, object[] args)
        {
            return xamlType.Invoker.CreateInstance(args);
        }

        public override object CreateWithFactoryMethod(XamlType xamlType, string methodName, object[] args)
        {
            Type type = xamlType.UnderlyingType;
            if (type == null)
            {
                throw CreateException((SR.Get(SRID.CannotResolveTypeForFactoryMethod, xamlType, methodName)));
            }
            string qMethodName = type.ToString() + "." + methodName;
            object instance = null;
            try
            {
                instance = InvokeFactoryMethod(type, methodName, args);
            }
            catch (Exception e)
            {
                if (CriticalExceptions.IsCriticalException(e))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.MethodInvocation, qMethodName), UnwrapTargetInvocationException(e));
            }
            if (instance == null)
            {
                throw CreateException(SR.Get(SRID.FactoryReturnedNull, qMethodName));
            }
            return instance;
        }

        protected virtual object InvokeFactoryMethod(Type type, string methodName, object[] args)
        {
            MethodInfo method = GetFactoryMethod(type, methodName, args, BindingFlags.Public | BindingFlags.Static);
            return SafeReflectionInvoker.InvokeMethod(method, null, args);
        }

        protected MethodInfo GetFactoryMethod(Type type, string methodName, object[] args, BindingFlags flags)
        {
            MethodInfo factory = null;
            if (args == null || args.Length == 0)
            {
                factory = type.GetMethod(methodName, flags, null, Type.EmptyTypes, null);
            }
            if (factory == null)
            {
                // We go down this path even if there are no args, because we might match a params array
                MemberInfo[] members = type.GetMember(methodName, MemberTypes.Method, flags);
                MethodBase[] methods = members as MethodBase[];
                if (methods == null)
                {
                    methods = new MethodBase[members.Length];
                    Array.Copy(members, methods, members.Length);
                }
                // This method throws if it can't find a match, so factory will never be null
                factory = (MethodInfo)BindToMethod(flags, methods, args);
            }
            return factory;
        }

        protected MethodBase BindToMethod(BindingFlags bindingFlags, MethodBase[] candidates, object[] args)
        {
            object state;
            return Type.DefaultBinder.BindToMethod(
                bindingFlags, candidates, ref args, null, null, null, out state);
        }

        //CreateFromValue is expected to convert the provided value via any applicable converter (on property or type) or provide the original value if there is no converter
        public override object CreateFromValue(
                                    ServiceProviderContext serviceContext,
                                    XamlValueConverter<TypeConverter> ts, object value,
                                    XamlMember property)
        {
            // check for a few common but special case text reps.
            if (ts == BuiltInValueConverter.String || ts == BuiltInValueConverter.Object)
            {
                return value;
            }

            return CreateObjectWithTypeConverter(serviceContext, ts, value);
        }

        public override bool CanConvertToString(XAML3.IValueSerializerContext context, XAML3.ValueSerializer serializer, object instance)
        {
            try
            {
                return serializer.CanConvertToString(instance, context);
            }
            catch (Exception ex)
            {
                if (CriticalExceptions.IsCriticalException(ex))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.TypeConverterFailed2, instance, typeof(string)), ex);
            }
        }

        public override bool CanConvertFrom<T>(ITypeDescriptorContext context, TypeConverter converter)
        {
            try
            {
                return converter.CanConvertFrom(context, typeof(T));
            }
            catch (Exception ex)
            {
                if (CriticalExceptions.IsCriticalException(ex))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.CanConvertFromFailed, typeof(T), converter.GetType()), ex);
            }
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, TypeConverter converter, Type type)
        {
            try
            {
                return converter.CanConvertTo(context, type);
            }
            catch (Exception ex)
            {
                if (CriticalExceptions.IsCriticalException(ex))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.CanConvertToFailed, type, converter.GetType()), ex);
            }
        }

        public override string ConvertToString(XAML3.IValueSerializerContext context, XAML3.ValueSerializer serializer, object instance)
        {
            try
            {
                return serializer.ConvertToString(instance, context);
            }
            catch (Exception ex)
            {
                if (CriticalExceptions.IsCriticalException(ex))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.TypeConverterFailed2, instance, typeof(string)), ex);
            }
        }

        public override T ConvertToValue<T>(ITypeDescriptorContext context, TypeConverter converter, object instance)
        {
            try
            {
                return (T)converter.ConvertTo(context, TypeConverterHelper.InvariantEnglishUS, instance, typeof(T));
            }
            catch (Exception ex)
            {
                if (CriticalExceptions.IsCriticalException(ex))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.TypeConverterFailed2, instance, typeof(T)), ex);
            }
        }

        public override object GetValue(Object obj, XamlMember property, bool failIfWriteOnly)
        {
            object value;
            try
            {
                if(property.IsDirective)
                {
                    value = this.CreateInstance(property.Type, null);
                }
                else if(!failIfWriteOnly)
                {
                    try
                    {
                        value = GetValue(property, obj);
                    }
                    catch(NotSupportedException)
                    {
                        value = null;
                    }
                }
                else
                {
                    value = GetValue(property, obj);
                }
            }
            catch(Exception e)
            {
                if(CriticalExceptions.IsCriticalException(e))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.GetValue, property), UnwrapTargetInvocationException(e));
            }
            return value;
        }

        protected virtual object GetValue(XamlMember member, object obj)
        {
            return member.Invoker.GetValue(obj);
        }

        public override void SetValue(Object inst, XamlMember property, object value)
        {
            try
            {
                if(property.IsDirective)
                {
                    return;
                }
                SetValue(property, inst, value);
            }
            catch(Exception e)
            {
                if(CriticalExceptions.IsCriticalException(e))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.SetValue, property), UnwrapTargetInvocationException(e));
            }
        }

        protected virtual void SetValue(XamlMember member, object obj, object value)
        {
            member.Invoker.SetValue(obj, value);
        }

        public override void Add(object collection, XamlType collectionType, object value, XamlType valueXamlType)
        {
            try
            {
                collectionType.Invoker.AddToCollection(collection, value);
            }
            catch(Exception e)
            {
                if(CriticalExceptions.IsCriticalException(e))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.AddCollection, collectionType), UnwrapTargetInvocationException(e));
            }
        }

        public override void AddToDictionary(object collection, XamlType dictionaryType, object value, XamlType valueXamlType, object key)
        {
            try
            {
                dictionaryType.Invoker.AddToDictionary(collection, key, value);
            }
            catch(Exception e)
            {
                if(CriticalExceptions.IsCriticalException(e))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.AddDictionary, dictionaryType), UnwrapTargetInvocationException(e));
            }
        }

        public override IList<object> GetCollectionItems(object collection, XamlType collectionType)
        {
            List<object> result; 
            IEnumerator enumerator = GetItems(collection, collectionType);
            try
            {
                result = new List<object>();
                while (enumerator.MoveNext())
                {
                    result.Add(enumerator.Current);
                }
            }
            catch (Exception ex)
            {
                if (CriticalExceptions.IsCriticalException(ex))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.GetItemsException, collectionType), ex);
            }
            return result;
        }

        public override IEnumerable<DictionaryEntry> GetDictionaryItems(object dictionary, XamlType dictionaryType)
        {
            IEnumerator enumerator = GetItems(dictionary, dictionaryType);
            try
            {
                // Dictionaries are required to either give us an either:
                // - an IDictionaryEnumerator,
                // - an IEnumerator<KeyValuePair<K,V>>, or 
                // - an IEnumerator that returns DictionaryEntrys
                IDictionaryEnumerator dictionaryEnumerator = enumerator as IDictionaryEnumerator;
                if (dictionaryEnumerator != null)
                {
                    return DictionaryEntriesFromIDictionaryEnumerator(dictionaryEnumerator);
                }

                // Without a type parameter known at compile time, there's no way to access the
                // members of a generic type other than reflection. So we need this hackery to
                // convert from K,V to object,object.
                Type keyType = dictionaryType.KeyType.UnderlyingType;
                Type itemType = dictionaryType.ItemType.UnderlyingType;
                Type genericKVP = typeof(KeyValuePair<,>).MakeGenericType(keyType, itemType);
                Type genericIEnumerator = typeof(IEnumerator<>).MakeGenericType(genericKVP);
                if (genericIEnumerator.IsAssignableFrom(enumerator.GetType()))
                {
                    var openMethod = typeof(ClrObjectRuntime).GetMethod(
                        "DictionaryEntriesFromIEnumeratorKvp", BindingFlags.Static | BindingFlags.NonPublic);
                    var method = openMethod.MakeGenericMethod(new Type[] { keyType, itemType });
                    return (IEnumerable<DictionaryEntry>)method.Invoke(null, new object[] { enumerator });
                }
                else
                {
                    return DictionaryEntriesFromIEnumerator(enumerator);
                }
            }
            catch (Exception ex)
            {
                if (CriticalExceptions.IsCriticalException(ex))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.GetItemsException, dictionaryType), ex);
            }
        }

        public override int AttachedPropertyCount(object instance)
        {
            try
            {
                return AttachablePropertyServices.GetAttachedPropertyCount(instance);
            }
            catch (Exception ex)
            {
                if (CriticalExceptions.IsCriticalException(ex))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.APSException, instance));
            }
        }

        public override KeyValuePair<AttachableMemberIdentifier, object>[] GetAttachedProperties(object instance)
        {
            try
            {
                KeyValuePair<AttachableMemberIdentifier, object>[] result = null;
                int count = AttachablePropertyServices.GetAttachedPropertyCount(instance);
                if (count > 0)
                {
                    result = new KeyValuePair<AttachableMemberIdentifier, object>[count];
                    AttachablePropertyServices.CopyPropertiesTo(instance, result, 0);
                }
                return result;
            }
            catch (Exception ex)
            {
                if (CriticalExceptions.IsCriticalException(ex))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.APSException, instance));
            }
        }

        public override void SetConnectionId(object root, int connectionId, object instance)
        {
            try
            {
                System.Windows.Markup.IComponentConnector connector = root as System.Windows.Markup.IComponentConnector;
                if(connector != null)
                {
                    connector.Connect(connectionId, instance);
                }
            }
            catch(Exception e)
            {
                if(CriticalExceptions.IsCriticalException(e))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.SetConnectionId), e);
            }
        }

        public override void InitializationGuard(XamlType xamlType, object obj, bool begin)
        {
            try
            {
                ISupportInitialize supportInit = obj as ISupportInitialize;
                if(supportInit != null)
                {
                    if(begin)
                    {
                        supportInit.BeginInit();
                    }
                    else
                    {
                        supportInit.EndInit();
                    }
                }
            }
            catch(Exception e)
            {
                if(CriticalExceptions.IsCriticalException(e))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.InitializationGuard, xamlType), e);
            }
        }

        public override object CallProvideValue(XAML3.MarkupExtension me, IServiceProvider serviceProvider)
        {
            try
            {
                object val = me.ProvideValue(serviceProvider);
                return val;
            }
            catch(Exception e)
            {
                if(CriticalExceptions.IsCriticalException(e))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.ProvideValue, me.GetType()), e);
            }
        }

        public override void SetUriBase(XamlType xamlType, object obj, Uri baseUri)
        {
            try
            {
                XAML3.IUriContext uriContext = obj as XAML3.IUriContext;
                if(uriContext != null)
                {
                    uriContext.BaseUri = baseUri;
                }
            }
            catch(Exception e)
            {
                if(CriticalExceptions.IsCriticalException(e))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.AddDictionary, xamlType), e);
            }
        }

        // SetXmlInstance: receives the value as "object" so the calling code doesn't
        // need to load System.Xml.dll types to make the call.
        public override void SetXmlInstance(object inst, XamlMember property, XAML3.XData xData)
        {
            object propInstance = GetValue(inst, property, true);
            IXmlSerializable iXmlSerial = propInstance as IXmlSerializable;
            if(iXmlSerial == null)
            {
                throw CreateException((SR.Get(SRID.XmlDataNull, property.Name)));
            }

            XmlReader reader = xData.XmlReader as XmlReader;
            if(reader == null)
            {
                throw new XamlInternalException(SR.Get(SRID.XmlValueNotReader, property.Name));
            }
            try
            {
                iXmlSerial.ReadXml(reader);
            }
            catch(Exception e)
            {
                if(CriticalExceptions.IsCriticalException(e))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.SetXmlInstance, property), e);
            }
        }

        public override TConverterBase GetConverterInstance<TConverterBase>(XamlValueConverter<TConverterBase> converter)
        {
            return converter.ConverterInstance;
        }

        public override object DeferredLoad(ServiceProviderContext serviceContext,
                                             XamlValueConverter<XamlDeferringLoader> deferringLoader,
                                             XamlReader deferredContent)
        {
            try
            {
                XamlDeferringLoader converter = GetConverterInstance(deferringLoader);
                if(converter == null)
                {
                    throw new XamlObjectWriterException(SR.Get(SRID.DeferringLoaderInstanceNull, deferringLoader));
                }
                return converter.Load(deferredContent, serviceContext);
            }
            catch (Exception e)
            {
                // Reset the reader in case our caller catches and retries
                IXamlIndexingReader indexingReader = deferredContent as IXamlIndexingReader;
                if(indexingReader != null && indexingReader.CurrentIndex >= 0)
                {
                    indexingReader.CurrentIndex = -1;
                }
                if (CriticalExceptions.IsCriticalException(e) || e is XamlException)
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.DeferredLoad), e);
            }
        }

        public override XamlReader DeferredSave(IServiceProvider serviceContext,
                                                XamlValueConverter<XamlDeferringLoader> deferringLoader,
                                                object value)
        {
            try
            {
                XamlDeferringLoader converter = GetConverterInstance(deferringLoader);
                if (converter == null)
                {
                    throw new XamlObjectWriterException(SR.Get(SRID.DeferringLoaderInstanceNull, deferringLoader));
                }
                return converter.Save(value, serviceContext);
            }
            catch (Exception e)
            {
                if (CriticalExceptions.IsCriticalException(e) || e is XamlException)
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.DeferredSave), e);
            }
        }

        public override ShouldSerializeResult ShouldSerialize(XamlMember member, object instance)
        {
            try
            {
                return member.Invoker.ShouldSerializeValue(instance);
            }
            catch (Exception ex)
            {
                if (CriticalExceptions.IsCriticalException(ex))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.ShouldSerializeFailed, member));
            }
        }

        private object CreateObjectWithTypeConverter(ServiceProviderContext serviceContext,
                                     XamlValueConverter<TypeConverter> ts, object value)
        {
            TypeConverter typeConverter = GetConverterInstance(ts);

            object obj;
            if (typeConverter != null)
            {
                //We sometimes ignoreCanConvert for WPFv3 Compatibility (but only if a string is coming in)
                if (_ignoreCanConvert && value.GetType() == typeof(string))
                {
                    obj = typeConverter.ConvertFrom(serviceContext, TypeConverterHelper.InvariantEnglishUS, value);
                }
                else
                {
                    if (typeConverter.CanConvertFrom(value.GetType()))
                    {
                        obj = typeConverter.ConvertFrom(serviceContext, TypeConverterHelper.InvariantEnglishUS, value);
                    }
                    else
                    {
                        //let the value passthrough (to be set as the property value later).
                        obj = value;
                    }
                }
            }
            else
            {
                //let the value passthrough (to be set as the property value later).
                obj = value;
            }
            
            return obj;
        }

        protected virtual Delegate CreateDelegate(Type delegateType, object target, string methodName)
        {
            return SafeReflectionInvoker.CreateDelegate(delegateType, target, methodName);
        }

        internal XamlRuntimeSettings GetSettings()
        {
            return new XamlRuntimeSettings() { IgnoreCanConvert = _ignoreCanConvert };
        }

        private XamlException CreateException(string message)
        {
            return CreateException(message, null);
        }

        private XamlException CreateException(string message, Exception innerException)
        {
            XamlException ex;
            if (_isWriter)
            {
                ex = new XamlObjectWriterException(message, innerException);
            }
            else
            {
                ex = new XamlObjectReaderException(message, innerException);
            }
            return (LineInfo != null) ? LineInfo.WithLineInfo(ex) : ex;
        }

        private IEnumerator GetItems(object collection, XamlType collectionType)
        {
            IEnumerator result;
            try
            {
                result = collectionType.Invoker.GetItems(collection);
            }
            catch (Exception ex)
            {
                if (CriticalExceptions.IsCriticalException(ex))
                {
                    throw;
                }
                throw CreateException(SR.Get(SRID.GetItemsException, collectionType), UnwrapTargetInvocationException(ex));
            }
            if (result == null)
            {
                throw CreateException(SR.Get(SRID.GetItemsReturnedNull, collectionType));
            }
            return result;
        }

        private static IEnumerable<DictionaryEntry> DictionaryEntriesFromIDictionaryEnumerator(
            IDictionaryEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Entry;
            }
        }

        private static IEnumerable<DictionaryEntry> DictionaryEntriesFromIEnumerator(IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return (DictionaryEntry)enumerator.Current;
            }
        }

        private static IEnumerable<DictionaryEntry> DictionaryEntriesFromIEnumeratorKvp<TKey, TValue>(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return new DictionaryEntry(enumerator.Current.Key, enumerator.Current.Value);
            }
        }
    }
}
