// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Presentation.Debug
{
    using System.Activities;
    using System.Activities.Presentation.View;
    using System.Runtime;

    /// <summary>
    /// AllowBreakpointAttribute is an attribute to describe whether a type allow a breakpoint to be set on it.
    /// </summary>
    [Fx.Tag.XamlVisible(false)]
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class AllowBreakpointAttribute : Attribute
    {
        internal static bool IsBreakpointAllowed(Type breakpointCandidateType)
        {
            return typeof(Activity).IsAssignableFrom(breakpointCandidateType) || WorkflowViewService.GetAttribute<AllowBreakpointAttribute>(breakpointCandidateType) != null;
        }
    }
}
