// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using System.IO;
using System.ComponentModel;

namespace MS.Internal.Xaml.Runtime
{
    internal interface IAddLineInfo
    {
        XamlException WithLineInfo(XamlException ex);
    }

    internal abstract class XamlRuntime
    {
        abstract public IAddLineInfo LineInfo { get; set; }

        abstract public object CreateInstance(XamlType xamlType, object[] args);

        abstract public object CreateWithFactoryMethod(XamlType xamlType, string methodName, object[] args);

        //CreateFromValue is expected to convert the provided value via any applicable converter (on property or type) or provide the original value if there is no converter
        abstract public object CreateFromValue(ServiceProviderContext serviceContext, XamlValueConverter<TypeConverter> ts,
                                               object value, XamlMember property);

        abstract public bool CanConvertToString(IValueSerializerContext context, ValueSerializer serializer, object instance);

        abstract public bool CanConvertFrom<T>(ITypeDescriptorContext context, TypeConverter converter);

        abstract public bool CanConvertTo(ITypeDescriptorContext context, TypeConverter converter, Type type);

        abstract public string ConvertToString(IValueSerializerContext context, ValueSerializer serializer, object instance);

        abstract public T ConvertToValue<T>(ITypeDescriptorContext context, TypeConverter converter, object instance);

        abstract public object DeferredLoad(ServiceProviderContext serviceContext,
                                            XamlValueConverter<XamlDeferringLoader> deferringLoader,
                                            XamlReader deferredContent);

        abstract public XamlReader DeferredSave(IServiceProvider context,
                                                XamlValueConverter<XamlDeferringLoader> deferringLoader,
                                                object value);

        public object GetValue(Object obj, XamlMember property)
        {
            return GetValue(obj, property, true);
        }

        abstract public object GetValue(Object obj, XamlMember property, bool failIfWriteOnly);

        abstract public void SetValue(Object obj, XamlMember property, object value);

        abstract public void SetUriBase(XamlType xamlType, object obj, Uri baseUri);

        abstract public void SetXmlInstance(object inst, XamlMember property, XData xData);

        abstract public void Add(object collection, XamlType collectionType, object value, XamlType valueXamlType);

        abstract public void AddToDictionary(object collection, XamlType dictionaryType, object value, XamlType valueXamlType, object key);

        abstract public IList<object> GetCollectionItems(object collection, XamlType collectionType);

        abstract public IEnumerable<DictionaryEntry> GetDictionaryItems(object dictionary, XamlType dictionaryType);

        abstract public int AttachedPropertyCount(object instance);

        abstract public KeyValuePair<AttachableMemberIdentifier, object>[] GetAttachedProperties(object instance);

        abstract public void SetConnectionId(object root, int connectionId, object instance);

        abstract public void InitializationGuard(XamlType xamlType, object obj, bool begin);

        abstract public object CallProvideValue(MarkupExtension me, IServiceProvider serviceProvider);

        abstract public ShouldSerializeResult ShouldSerialize(XamlMember member, object instance);

        abstract public TConverterBase GetConverterInstance<TConverterBase>(XamlValueConverter<TConverterBase> converter)
            where TConverterBase : class;
    }
}
