// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Presentation
{
    using System.Activities.Debugger;

    /// <summary>
    /// Provides data for the SourceLocationUpdated event.
    /// </summary>
    public class SourceLocationUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the SourceLocationUpdatedEventArgs class.
        /// </summary>
        /// <param name="objectReference">The Guid of the ObjectReference that changed.</param>
        /// <param name="updatedSourceLocation">The updated SourceLocation.</param>
        public SourceLocationUpdatedEventArgs(Guid objectReference, SourceLocation updatedSourceLocation)
        {
            this.ObjectReference = objectReference;
            this.UpdatedSourceLocation = updatedSourceLocation;
        }

        /// <summary>
        /// Gets the Guid of the ObjectReference that changed.
        /// </summary>
        public Guid ObjectReference
        {
            get; 
            private set; 
        }

        /// <summary>
        /// Gets the updated source location.
        /// </summary>
        public SourceLocation UpdatedSourceLocation
        {
            get;
            private set;
        }
    }
}
