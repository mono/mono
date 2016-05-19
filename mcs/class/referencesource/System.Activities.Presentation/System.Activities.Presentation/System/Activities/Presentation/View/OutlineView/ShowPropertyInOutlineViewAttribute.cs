//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.View.OutlineView
{
    using System;

    /// <summary>
    /// Shows the property and its value in outline view.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ShowPropertyInOutlineViewAttribute : Attribute
    {
        /// <summary>
        /// Initialize the instance of ShowPropertyInOutlineViewAttribute.
        /// </summary>
        public ShowPropertyInOutlineViewAttribute()          
        {
            this.CurrentPropertyVisible = true;
            this.DuplicatedChildNodesVisible = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the property should be visible in outline view.
        /// </summary>        
        public bool CurrentPropertyVisible { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to skip child nodes that are visible elsewhere in the outline view
        /// </summary>
        public bool DuplicatedChildNodesVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display a perfix of child node.
        /// </summary>
        public string ChildNodePrefix { get; set; }
    }
}
