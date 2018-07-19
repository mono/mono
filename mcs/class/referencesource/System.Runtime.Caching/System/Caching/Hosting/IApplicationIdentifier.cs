// <copyright file="IApplicationIdentifier.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;

namespace System.Runtime.Caching.Hosting {
    public interface IApplicationIdentifier {
        String GetApplicationId();
    }
}
