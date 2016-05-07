//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.View.OutlineView
{
    using System;

    /// <summary>
    /// Shows the property value as silbling of current instance in outline view.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ShowPropertyInOutlineViewAsSiblingAttribute : Attribute
    {
    }
}
