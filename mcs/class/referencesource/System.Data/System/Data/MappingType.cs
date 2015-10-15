//------------------------------------------------------------------------------
// <copyright file="MappingType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">amirhmy</owner>
// <owner current="true" primary="false">markash</owner>
// <owner current="false" primary="false">jasonzhu</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    [Serializable]
    public enum MappingType {
        Element         = 1,        // Element column
        Attribute       = 2,        // Attribute column
        SimpleContent   = 3,        // SimpleContent column
        Hidden          = 4         // Internal mapping
    }
}

