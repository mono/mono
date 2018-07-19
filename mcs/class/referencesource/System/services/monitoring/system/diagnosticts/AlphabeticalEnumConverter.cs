//------------------------------------------------------------------------------
// <copyright file="AlphabeticalEnumConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {

    using System.Collections;
    using System.ComponentModel;
    using System;
    using System.Globalization;
    

    /// <internalonly/>
    /// <devdoc>
    ///    <para>
    ///       Provides a type converter to
    ///       convert ???? objects to and from various other representations.
    ///    </para>
    /// </devdoc>
    internal class AlphabeticalEnumConverter : EnumConverter {

        public AlphabeticalEnumConverter(Type type) : base(type) {
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            if (Values == null) {
                Array objValues = Enum.GetValues(EnumType);
                //IComparer comparer = Comparer;
                object[] names = new object[objValues.Length];
                for (int i = 0; i < names.Length; i++)
                    names[i] = ConvertTo(context, null, objValues.GetValue(i), typeof(string));
                Array.Sort(names, objValues, 0, objValues.Length, System.Collections.Comparer.Default);
                Values = new StandardValuesCollection(objValues);
            }
            return Values;
        }

    }

}
