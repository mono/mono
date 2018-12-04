// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace MS.Internal.Xaml.Parser
{
    internal enum ScannerNodeType
    {
        NONE,
        ELEMENT,
        EMPTYELEMENT,
        ATTRIBUTE,
        DIRECTIVE,
        PREFIXDEFINITION,
        PROPERTYELEMENT,
        EMPTYPROPERTYELEMENT,
        TEXT,
        ENDTAG
    };

    internal enum ScannerAttributeKind
    {
        Namespace,
        CtorDirective,
        Name,
        Directive,
        XmlSpace,
        Event,
        Property,
        AttachableProperty,
        Unknown
    };
}
