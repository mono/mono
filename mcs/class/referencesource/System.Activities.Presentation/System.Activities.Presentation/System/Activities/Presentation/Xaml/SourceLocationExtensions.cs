// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Presentation.Xaml
{
    using System;
    using System.Activities.Debugger;
    using System.Runtime;

    [Serializable]
    internal static class SourceLocationExtensions
    {
        internal static bool Contains(this SourceLocation outer, SourceLocation inner)
        {
            Fx.Assert(inner != null && outer != null, "Argument should not be null");

            if ((inner.StartLine > outer.StartLine || (inner.StartLine == outer.StartLine && inner.StartColumn >= outer.StartColumn))
             && (inner.EndLine < outer.EndLine || (inner.EndLine == outer.EndLine && inner.EndColumn <= outer.EndColumn)))
            {
                return true;
            }

            return false;
        }
    }
}
