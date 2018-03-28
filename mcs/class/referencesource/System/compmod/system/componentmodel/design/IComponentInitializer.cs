//------------------------------------------------------------------------------
// <copyright file="IComponentInitializer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {

    using System;
    using System.Collections;

    /// <devdoc>
    ///     IComponentInitializer can be implemented on an object that also implements IDesigner. 
    ///     This interface allows a newly created component to be given some stock default values,
    ///     such as a caption, default size, or other values.  Recommended default values for
    ///     the component's properties are passed in as a dictionary.
    /// </devdoc>
    public interface IComponentInitializer {
        
        /// <devdoc>
        ///     This method is called when an existing component is being re-initialized.  This may occur after
        ///     dragging a component to another container, for example.  The defaultValues
        ///     property contains a name/value dictionary of default values that should be applied
        ///     to properties. This dictionary may be null if no default values are specified.
        ///     You may use the defaultValues dictionary to apply recommended defaults to proeprties
        ///     but you should not modify component properties beyond what is stored in the
        ///     dictionary, because this is an existing component that may already have properties
        ///     set on it.
        /// </devdoc>
        void InitializeExistingComponent(IDictionary defaultValues);
        
        /// <devdoc>
        ///     This method is called when a component is first initialized, typically after being first added
        ///     to a design surface.  The defaultValues property contains a name/value dictionary of default
        ///     values that should be applied to properties.  This dictionary may be null if no default values
        ///     are specified.  You may perform any initialization of this component that you like, and you
        ///     may even ignore the defaultValues dictionary altogether if you wish.  
        /// </devdoc>
        void InitializeNewComponent(IDictionary defaultValues);
    }
}

