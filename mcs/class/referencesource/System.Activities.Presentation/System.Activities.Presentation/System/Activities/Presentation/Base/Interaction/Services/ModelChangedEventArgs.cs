//------------------------------------------------------------------------------
// <copyright file="ModelChangedEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Activities.Presentation.Services
{
    using System;
    using System.Collections.Generic;
    using System.Activities.Presentation.Model;

    /// <summary>
    /// When the model raises change events, it creates an 
    /// EventArgs that describes the change.
    /// </summary>
    public abstract class ModelChangedEventArgs : EventArgs {
        
        /// <summary>
        /// Creates a new ModelChangedEventArgs.
        /// </summary>
        protected ModelChangedEventArgs() {
        }

        /// <summary>
        /// An enumeration of objects that have been added.
        /// </summary>
        [Obsolete("Don't use this property. Use \"ModelChangeInfo\" instead.")]
        public abstract IEnumerable<ModelItem> ItemsAdded { get; }

        /// <summary>
        /// An enumeration of objects that have been removed.
        /// </summary>
        [Obsolete("Don't use this property. Use \"ModelChangeInfo\" instead.")]
        public abstract IEnumerable<ModelItem> ItemsRemoved { get; }

        /// <summary>
        /// An enumeration of properties that have been changed.
        /// </summary>
        [Obsolete("Don't use this property. Use \"ModelChangeInfo\" instead.")]
        public abstract IEnumerable<ModelProperty> PropertiesChanged { get; }

        /// <summary>
        /// A ModelChangeInfo object that contains detailed model change information.
        /// </summary>
        public virtual ModelChangeInfo ModelChangeInfo
        {
            get { return null; }
        }
    }
}
