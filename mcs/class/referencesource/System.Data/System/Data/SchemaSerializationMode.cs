//------------------------------------------------------------------------------
// <copyright file="SchemaSerializationMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------
namespace System.Data {

    public enum SchemaSerializationMode { 
        IncludeSchema            = (1 << 0), //  1 0x01
        ExcludeSchema            = (1 << 1), //  2 0x02
    }

}
