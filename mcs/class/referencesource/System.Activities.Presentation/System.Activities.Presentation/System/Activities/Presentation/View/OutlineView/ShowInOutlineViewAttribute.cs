//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.View.OutlineView
{
    using System;

    /// <summary>
    /// Shows instances of this class in outline view.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class ShowInOutlineViewAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets this specify property name to be shown in place of the current
        /// ModelItem
        /// </summary>    
        public string PromotedProperty { get; set; }
    }
}
