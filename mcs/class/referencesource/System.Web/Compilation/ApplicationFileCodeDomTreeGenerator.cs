//------------------------------------------------------------------------------
// <copyright file="ApplicationFileCodeDomTreeGenerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

using System;
using System.Web.UI;

internal class ApplicationFileCodeDomTreeGenerator : BaseCodeDomTreeGenerator {

    protected ApplicationFileParser _appParser;

    internal ApplicationFileCodeDomTreeGenerator(ApplicationFileParser appParser) : base(appParser) {
        _appParser = appParser;
    }

    protected override bool IsGlobalAsaxGenerator { get { return true; } }
}

}
