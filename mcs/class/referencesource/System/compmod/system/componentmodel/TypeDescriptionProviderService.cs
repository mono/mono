//------------------------------------------------------------------------------
// <copyright file="TypeDescriptionProviderService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel.Design {
    using System;
    using System.ComponentModel;

    public abstract class TypeDescriptionProviderService {
        public abstract TypeDescriptionProvider GetProvider(object instance);
        public abstract TypeDescriptionProvider GetProvider(Type type);
    }

}
