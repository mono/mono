// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Xaml;

namespace System.Windows.Markup
{
    public class XamlSetTypeConverterEventArgs : XamlSetValueEventArgs
    {
        public XamlSetTypeConverterEventArgs(XamlMember member, TypeConverter typeConverter, object value,
            ITypeDescriptorContext serviceProvider, CultureInfo cultureInfo) :
            base(member, value)
        {
            TypeConverter = typeConverter;
            ServiceProvider = serviceProvider;
            CultureInfo = cultureInfo;
        }

        internal XamlSetTypeConverterEventArgs(XamlMember member, TypeConverter typeConverter, object value,
            ITypeDescriptorContext serviceProvider, CultureInfo cultureInfo, object targetObject) :
            this(member, typeConverter, value, serviceProvider, cultureInfo)
        {
            TargetObject = targetObject;
        }

        public TypeConverter TypeConverter { get; private set; }
        public ITypeDescriptorContext ServiceProvider { get; private set; }
        public CultureInfo CultureInfo { get; private set; }

        internal object TargetObject { get; private set; }
        internal XamlType CurrentType { get; set; }

        public override void CallBase()
        {
            if (CurrentType != null)
            {
                XamlType baseType = CurrentType.BaseType;

                if (baseType != null)
                {
                    this.CurrentType = baseType;
                    if (baseType.SetTypeConverterHandler != null)
                    {
                        baseType.SetTypeConverterHandler(TargetObject, this);
                    }
                }
            }
        }
    }
}
