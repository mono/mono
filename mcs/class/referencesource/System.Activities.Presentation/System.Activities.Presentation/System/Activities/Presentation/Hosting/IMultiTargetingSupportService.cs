//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Reflection;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldBeSpelledCorrectly,
                         Justification = "MultiTargetingSupportService is the correct name")]
    public interface IMultiTargetingSupportService
    {
        Assembly GetReflectionAssembly(AssemblyName targetAssemblyName);
        Type GetRuntimeType(Type reflectionType);
        bool IsSupportedType(Type type);
    }
}
