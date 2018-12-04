// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using System.Xaml.Permissions;

namespace MS.Internal.Xaml.Runtime
{
    // Perf notes
    // - Need a perf test to decide whether it's faster to check for public visibility, or just always 
    //   fall through to the elevated case once we've determined we don't have MemberAccess permission.
    // - Consider checking ctor visibility in CreateInstance
    // - Consider checking method visibility in CreateWithFactoryMethod

    // This class wraps two runtimes: a transparent runtime (typically ClrObjectRuntime)
    // and an elevated runtime (typically DynamicMethodRuntime). The goal is to use the transparent
    // runtime when possible--i.e. if we're in Full Trust, or if all the types are public.
    //
    // We start out by forwarding all calls to the transparent runtime.
    // If a call fails with a MethodAccessException, we fall back to the elevated runtime.
    // After the first failure, we automatically go to the elevated runtime for non-public types.
    class PartialTrustTolerantRuntime : XamlRuntime
    {
        bool _memberAccessPermissionDenied;
        ClrObjectRuntime _transparentRuntime;
        ClrObjectRuntime _elevatedRuntime;
        XamlAccessLevel _accessLevel;
        XamlSchemaContext _schemaContext;

        public PartialTrustTolerantRuntime(XamlRuntimeSettings runtimeSettings, XamlAccessLevel accessLevel, XamlSchemaContext schemaContext)
        {
            _transparentRuntime = new ClrObjectRuntime(runtimeSettings, true /*isWriter*/);
            _accessLevel = accessLevel;
            _schemaContext = schemaContext;
        }

        public override IAddLineInfo LineInfo
        {
            get
            {
                return _transparentRuntime.LineInfo;
            }
            set
            {
                _transparentRuntime.LineInfo = value;
                if (_elevatedRuntime != null)
                {
                    _elevatedRuntime.LineInfo = value;
                }
            }
        }

        public override void Add(object collection, XamlType collectionType, object value, XamlType valueXamlType)
        {
            _transparentRuntime.Add(collection, collectionType, value, valueXamlType);
        }

        public override void AddToDictionary(object collection, XamlType dictionaryType, object value, XamlType valueXamlType, object key)
        {
            _transparentRuntime.AddToDictionary(collection, dictionaryType, value, valueXamlType, key);
        }

        public override object CallProvideValue(System.Windows.Markup.MarkupExtension me, IServiceProvider serviceProvider)
        {
            // Once the ME is instantiated, invocation is always a public method call
            return _transparentRuntime.CallProvideValue(me, serviceProvider);
        }

        public override object CreateFromValue(ServiceProviderContext serviceContext, XamlValueConverter<TypeConverter> ts, object value, XamlMember property)
        {
            if (!MemberAccessPermissionDenied || ts.IsPublic || !IsDefaultConverter(ts))
            {
                try
                {
                    return _transparentRuntime.CreateFromValue(serviceContext, ts, value, property);
                }
                // We don't know if MissingMethodException is due to visibility or not.
                // So we fall back to the elevated runtime, but we don't set _memberAccessPermissionDenied.
                catch (MissingMethodException)
                {
                    EnsureElevatedRuntime();
                }
                catch (MethodAccessException)
                {
                    MemberAccessPermissionDenied = true;
                }
                catch (SecurityException)
                {
                    MemberAccessPermissionDenied = true;
                }
            }
            return _elevatedRuntime.CreateFromValue(serviceContext, ts, value, property);
        }

        // The following methods are just invocations of public APIs, so no need for a partial-trust fallback
        public override int AttachedPropertyCount(object instance)
        {
            return _transparentRuntime.AttachedPropertyCount(instance);
        }

        public override KeyValuePair<AttachableMemberIdentifier, object>[] GetAttachedProperties(object instance)
        {
            return _transparentRuntime.GetAttachedProperties(instance);
        }

        public override bool CanConvertToString(IValueSerializerContext context, ValueSerializer serializer, object instance)
        {
            return _transparentRuntime.CanConvertToString(context, serializer, instance);
        }

        public override bool CanConvertFrom<T>(ITypeDescriptorContext context, TypeConverter converter)
        {
            return _transparentRuntime.CanConvertFrom<T>(context, converter);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, TypeConverter converter, Type type)
        {
            return _transparentRuntime.CanConvertTo(context, converter, type);
        }

        public override string ConvertToString(IValueSerializerContext context, ValueSerializer serializer, object instance)
        {
            return _transparentRuntime.ConvertToString(context, serializer, instance);
        }

        public override T ConvertToValue<T>(ITypeDescriptorContext context, TypeConverter converter, object instance)
        {
            return _transparentRuntime.ConvertToValue<T>(context, converter, instance);
        }

        public override object CreateInstance(XamlType xamlType, object[] args)
        {
            if (!MemberAccessPermissionDenied || xamlType.IsPublic || !HasDefaultInvoker(xamlType))
            {
                try
                {
                    return _transparentRuntime.CreateInstance(xamlType, args);
                }
                catch (XamlException ex)
                {
                    if (ex.InnerException is MethodAccessException)
                    {
                        MemberAccessPermissionDenied = true;
                    }
                    // We don't know if MissingMethodException is due to visibility or not.
                    // So we fall back to the elevated runtime, but we don't set _memberAccessPermissionDenied.
                    else if (ex.InnerException is MissingMethodException)
                    {
                        EnsureElevatedRuntime();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (SecurityException)
                {
                    MemberAccessPermissionDenied = true;
                }
            }
            return _elevatedRuntime.CreateInstance(xamlType, args);
        }

        public override object CreateWithFactoryMethod(XamlType xamlType, string methodName, object[] args)
        {
            if (!MemberAccessPermissionDenied || xamlType.IsPublic)
            {
                try
                {
                    return _transparentRuntime.CreateWithFactoryMethod(xamlType, methodName, args);
                }
                catch (XamlException ex)
                {
                    if (ex.InnerException is MethodAccessException)
                    {
                        MemberAccessPermissionDenied = true;
                    }
                    // We don't know if MissingMethodException is due to visibility or not.
                    // So we fall back to the elevated runtime, but we don't set _memberAccessPermissionDenied.
                    else if (ex.InnerException is MissingMethodException)
                    {
                        EnsureElevatedRuntime();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (SecurityException)
                {
                    MemberAccessPermissionDenied = true;
                }
            }
            return _elevatedRuntime.CreateWithFactoryMethod(xamlType, methodName, args);
        }

        public override object DeferredLoad(ServiceProviderContext serviceContext, XamlValueConverter<XamlDeferringLoader> deferringLoader, XamlReader deferredContent)
        {
            if (!MemberAccessPermissionDenied || deferringLoader.IsPublic || !IsDefaultConverter(deferringLoader))
            {
                try
                {
                    return _transparentRuntime.DeferredLoad(serviceContext, deferringLoader, deferredContent);
                }
                catch (XamlException e)
                {
                    // We don't know if MissingMethodException is due to visibility or not.
                    // So we fall back to the elevated runtime, but we don't set _memberAccessPermissionDenied.
                    if (e.InnerException is MissingMethodException)
                    {
                        EnsureElevatedRuntime();
                    }
                    else if (e.InnerException is MethodAccessException)
                    {
                        MemberAccessPermissionDenied = true;
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (SecurityException)
                {
                    MemberAccessPermissionDenied = true;
                }
            }
            return _elevatedRuntime.DeferredLoad(serviceContext, deferringLoader, deferredContent);
        }

        public override XamlReader DeferredSave(IServiceProvider context,
                                                XamlValueConverter<XamlDeferringLoader> deferringLoader,
                                                object value)
        {
            if (!MemberAccessPermissionDenied || deferringLoader.IsPublic || !IsDefaultConverter(deferringLoader))
            {
                try
                {
                    return _transparentRuntime.DeferredSave(context, deferringLoader, value);
                }
                catch (XamlException e)
                {
                    // We don't know if MissingMethodException is due to visibility or not.
                    // So we fall back to the elevated runtime, but we don't set _memberAccessPermissionDenied.
                    if (e.InnerException is MissingMethodException)
                    {
                        EnsureElevatedRuntime();
                    }
                    else if (e.InnerException is MethodAccessException)
                    {
                        MemberAccessPermissionDenied = true;
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (SecurityException)
                {
                    MemberAccessPermissionDenied = true;
                }
            }
            return _elevatedRuntime.DeferredSave(context, deferringLoader, value);
        }

        public override TConverterBase GetConverterInstance<TConverterBase>(XamlValueConverter<TConverterBase> converter)
        {
            if (!MemberAccessPermissionDenied ||  converter.IsPublic || !IsDefaultConverter(converter))
            {
                try
                {
                    return _transparentRuntime.GetConverterInstance(converter);
                }
                // We don't know if MissingMethodException is due to visibility or not.
                // So we fall back to the elevated runtime, but we don't set _memberAccessPermissionDenied.
                catch (MissingMethodException)
                {
                    EnsureElevatedRuntime();
                }
                catch (MethodAccessException)
                {
                    MemberAccessPermissionDenied = true;
                }
                catch (SecurityException)
                {
                    MemberAccessPermissionDenied = true;
                }
            }
            return _elevatedRuntime.GetConverterInstance(converter);
        }

        public override object GetValue(object obj, XamlMember property, bool failIfWriteOnly)
        {
            if (!MemberAccessPermissionDenied || property.IsReadPublic || !HasDefaultInvoker(property))
            {
                try
                {
                    return _transparentRuntime.GetValue(obj, property, failIfWriteOnly);
                }
                catch (XamlException e)
                {
                    if (e.InnerException is MethodAccessException)
                    {
                        MemberAccessPermissionDenied = true;
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (SecurityException)
                {
                    MemberAccessPermissionDenied = true;
                }
            }
            return _elevatedRuntime.GetValue(obj, property, failIfWriteOnly);
        }

        public override void InitializationGuard(XamlType xamlType, object obj, bool begin)
        {
            // Invocation is a public method call
            _transparentRuntime.InitializationGuard(xamlType, obj, begin);
        }

        public override void SetConnectionId(object root, int connectionId, object instance)
        {
            // Invocation is a public method call
            _transparentRuntime.SetConnectionId(root, connectionId, instance);
        }

        public override void SetUriBase(XamlType xamlType, object obj, Uri baseUri)
        {
            // Invocation is a public method call
            _transparentRuntime.SetUriBase(xamlType, obj, baseUri);
        }

        public override void SetValue(object obj, XamlMember property, object value)
        {
            if (!MemberAccessPermissionDenied || property.IsWritePublic || !HasDefaultInvoker(property))
            {
                try
                {
                    _transparentRuntime.SetValue(obj, property, value);
                    return;
                }
                catch (XamlException e)
                {
                    if (e.InnerException is MethodAccessException)
                    {
                        MemberAccessPermissionDenied = true;
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (SecurityException)
                {
                    MemberAccessPermissionDenied = true;
                }
            }
            _elevatedRuntime.SetValue(obj, property, value);
        }

        public override void SetXmlInstance(object inst, XamlMember property, System.Windows.Markup.XData xData)
        {
            if (!MemberAccessPermissionDenied || property.IsReadPublic)
            {
                try
                {
                    _transparentRuntime.SetXmlInstance(inst, property, xData);
                    return;
                }
                catch (XamlException e)
                {
                    if (e.InnerException is MethodAccessException)
                    {
                        MemberAccessPermissionDenied = true;
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (SecurityException)
                {
                    MemberAccessPermissionDenied = true;
                }
            }
            _elevatedRuntime.SetXmlInstance(inst, property, xData);
            return;
        }

        // No partial-trust fallback on these methods because we're not doing PT support in ObjectReader.
        public override ShouldSerializeResult ShouldSerialize(XamlMember member, object instance)
        {
            return _transparentRuntime.ShouldSerialize(member, instance);
        }

        public override IList<object> GetCollectionItems(object collection, XamlType collectionType)
        {
            return _transparentRuntime.GetCollectionItems(collection, collectionType);
        }

        public override IEnumerable<DictionaryEntry> GetDictionaryItems(object dictionary, XamlType dictionaryType)
        {
            return _transparentRuntime.GetDictionaryItems(dictionary, dictionaryType);
        }

        private bool MemberAccessPermissionDenied
        {
            get { return _memberAccessPermissionDenied; }
            set
            {
                _memberAccessPermissionDenied = value;
                if (value)
                {
                    EnsureElevatedRuntime();
                }
            }
        }

        /// <SecurityNote>
        /// Critical: Initializes critical type DynamicMethodRuntime
        /// Safe: Initializes via safe ctor, and DMR demands at all its safe entry points
        /// </SecurityNote>
        [SecuritySafeCritical]
        private void EnsureElevatedRuntime()
        {
            if (_elevatedRuntime == null)
            {
                _elevatedRuntime = new DynamicMethodRuntime(
                    _transparentRuntime.GetSettings(), _schemaContext, _accessLevel);
                _elevatedRuntime.LineInfo = LineInfo;
            }
        }

        // We should avoid keying off the type of the invoker here
        private static bool HasDefaultInvoker(XamlType xamlType)
        {
            return xamlType.Invoker.GetType() == typeof(System.Xaml.Schema.XamlTypeInvoker);
        }

        // We should avoid keying off the type of the invoker here
        private static bool HasDefaultInvoker(XamlMember xamlMember)
        {
            return xamlMember.Invoker.GetType() == typeof(System.Xaml.Schema.XamlMemberInvoker);
        }

        // We should avoid keying off the type of the converter here
        private static bool IsDefaultConverter<TConverterBase>(XamlValueConverter<TConverterBase> converter)
            where TConverterBase : class
        {
            return converter.GetType() == typeof(XamlValueConverter<TConverterBase>);
        }
    }
}
