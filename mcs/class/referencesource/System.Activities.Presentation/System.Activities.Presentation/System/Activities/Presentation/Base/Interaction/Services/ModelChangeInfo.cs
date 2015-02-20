//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Services
{
    using System.Activities.Presentation.Model;

    /// <summary>
    /// Contains data for detailed model change information
    /// </summary>
    public abstract class ModelChangeInfo
    {
        /// <summary>
        /// Gets model change type
        /// </summary>
        public abstract ModelChangeType ModelChangeType { get; }

        /// <summary>
        /// Gets modelitem where a model change happens
        /// </summary>
        public abstract ModelItem Subject { get; }

        /// <summary>
        /// Gets property name if it's a property change
        /// </summary>
        public abstract string PropertyName { get; }

        /// <summary>
        /// Gets key model item if it's a dictionary change
        /// </summary>
        public abstract ModelItem Key { get; }

        /// <summary>
        /// Gets old value if it's a property chagne or a dictionary value change
        /// </summary>
        public abstract ModelItem OldValue { get; }

        /// <summary>
        /// Gets object that is been added/removed or the new value if it's a property change
        /// </summary>
        public abstract ModelItem Value { get; }
    }
}
