//------------------------------------------------------------------------------
// <copyright file="ModelPropertyCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Activities.Presentation.Model {

    using System;
    using System.Windows;

    /// <summary>
    /// A ModelPropertyCollection contains an enumeration of properties.  
    /// It defines an enumerator and a variety of search mechanisms.
    /// </summary>
    public abstract class ModelPropertyCollection :
        ModelMemberCollection<ModelProperty, DependencyProperty> {

        /// <summary>
        /// Creates a new ModelPropertyCollection.
        /// </summary>
        protected ModelPropertyCollection() : base() { }
    }
}
