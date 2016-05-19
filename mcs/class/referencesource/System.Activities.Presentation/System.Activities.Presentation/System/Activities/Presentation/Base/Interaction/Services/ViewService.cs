//------------------------------------------------------------------------------
// <copyright file="ViewService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Activities.Presentation.Services {

    using System;
    using System.Windows;
    using System.Activities.Presentation.Model;

    /// <summary>
    /// This service allows you to move between the model and the view.
    /// </summary>
    public abstract class ViewService {

        /// <summary>
        /// Constructs a new ViewService.
        /// </summary>
        protected ViewService() {
        }

        /// <summary>
        /// Returns the model corresponding to the view, or null if 
        /// there is no model matching the view.
        /// </summary>
        /// <param name="view">
        /// The view object you wish to find the model for.
        /// </param>
        /// <returns>
        /// The corresponding model, or null if there is no model for the
        /// given view object.
        /// </returns>
        /// <exception cref="ArgumentNullException">if view is null.</exception>
        public abstract ModelItem GetModel(DependencyObject view);

        /// <summary>
        /// Returns the view corresponding to the given model.  This 
        /// can return null if there is no view for the model.
        /// </summary>
        /// <param name="model">
        /// The model to return the view object for.
        /// </param>
        /// <returns>
        /// The view for this model, or null if there is no view.
        /// </returns>
        /// <exception cref="ArgumentNullException">if model is null.</exception>
        /// <exception cref="ArgumentException">if model does not represent a valid model for this service.</exception>
        public abstract DependencyObject GetView(ModelItem model);
    }
}
