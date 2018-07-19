//------------------------------------------------------------------------------
// <copyright file="DataTableTypeConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System.ComponentModel;

    internal sealed class DataTableTypeConverter : ReferenceConverter {

        // converter classes should have public ctor
        public DataTableTypeConverter() : base(typeof(DataTable)) {
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context) {
           return false;
        }
    }
}
