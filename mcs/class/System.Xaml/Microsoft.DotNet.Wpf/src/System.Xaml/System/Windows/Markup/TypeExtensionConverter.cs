// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Microsoft Windows Client Platform
//
//
//  Contents:  Converter to convert TypeExtensions to InstanceDescriptors

//  Created:   04/28/2005 Microsoft
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Collections.Generic;
using System.Text;
using System.Security;
using System.Xaml;

namespace System.Windows.Markup
{
    class TypeExtensionConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        ///<SecurityNote>
        ///     Critical: calls InstanceDescriptor ctor which LinkDemands
        ///     TreatAsSafe: can only make an InstanceDescriptor for TypeExtension, not an arbitrary class
        ///</SecurityNote> 
        [SecurityCritical, SecurityTreatAsSafe]
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                TypeExtension typeExtension = value as TypeExtension;
                if (typeExtension == null)
                {
                    throw new ArgumentException(SR.Get(SRID.MustBeOfType, "value", "TypeExtension")); 
                }
                return new InstanceDescriptor(typeof(TypeExtension).GetConstructor(new Type[] { typeof(Type) }),
                                              new object[] { typeExtension.Type });
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
