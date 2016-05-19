
namespace System.Activities.Presentation.Documents {

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Media;

    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Services;
    
    /// <summary>
    /// A ViewManager is a class that manages and provides the view
    /// for the designer.  The view manager is used by MarkupDocumentManager
    /// to provide the view for the designer.  
    /// </summary>
    abstract class ViewManager : IDisposable {

        /// <summary>
        /// Returns the view for the designer.  This will return null until
        /// Initialize has been called.
        /// </summary>
        public abstract Visual View { get; }

        /// <summary>
        /// Initializes this view manager with the given model tree.  
        /// </summary>
        /// <param name="context">The editing context for the designer.</param>
        /// <exception cref="ArgumentNullException">If model is null.</exception>
        public abstract void Initialize(EditingContext context);

        /// <summary>
        /// Disposes this view manager.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes this view manager.
        /// <param name="disposing">True if this object is being disposed, or false if it is finalizing.</param>
        /// </summary>
        protected virtual void Dispose(bool disposing) {
        }
    }
}
