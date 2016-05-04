//------------------------------------------------------------------------------
// <copyright file="IComponentChangeService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using System.Diagnostics;
    using System;
    using System.ComponentModel;

    /// <devdoc>
    /// <para>Provides an interface to add and remove the event handlers for System.ComponentModel.Design.IComponentChangeService.ComponentAdded, System.ComponentModel.Design.IComponentChangeService.ComponentAdding, System.ComponentModel.Design.IComponentChangeService.ComponentChanged, System.ComponentModel.Design.IComponentChangeService.ComponentChanging, System.ComponentModel.Design.IComponentChangeService.ComponentRemoved, System.ComponentModel.Design.IComponentChangeService.ComponentRemoving, and System.ComponentModel.Design.IComponentChangeService.ComponentRename events.</para>
    /// </devdoc>
    [System.Runtime.InteropServices.ComVisible(true)]
    public interface IComponentChangeService {

        /// <devdoc>
        /// <para>Adds an event handler for the System.ComponentModel.Design.IComponentChangeService.ComponentAdded event.</para>
        /// </devdoc>
        event ComponentEventHandler ComponentAdded;

        /// <devdoc>
        /// <para>Adds an event handler for the System.ComponentModel.Design.IComponentChangeService.ComponentAdding event.</para>
        /// </devdoc>
        event ComponentEventHandler ComponentAdding;

        /// <devdoc>
        /// <para>Adds an event handler for the System.ComponentModel.Design.IComponentChangeService.ComponentChanged event.</para>
        /// </devdoc>
        event ComponentChangedEventHandler ComponentChanged;

        /// <devdoc>
        /// <para>Adds an event handler for the System.ComponentModel.Design.IComponentChangeService.ComponentChanging event.</para>
        /// </devdoc>
        event ComponentChangingEventHandler ComponentChanging;

        /// <devdoc>
        /// <para>Adds an event handler for the System.ComponentModel.Design.IComponentChangeService.OnComponentRemoved event.</para>
        /// </devdoc>
        event ComponentEventHandler ComponentRemoved;

        /// <devdoc>
        /// <para>Adds an event handler for the System.ComponentModel.Design.IComponentChangeService.OnComponentRemoving event.</para>
        /// </devdoc>
        event ComponentEventHandler ComponentRemoving;

        /// <devdoc>
        /// <para>Adds an event handler for the System.ComponentModel.Design.IComponentChangeService.OnComponentRename event.</para>
        /// </devdoc>
        event ComponentRenameEventHandler ComponentRename;

        /// <devdoc>
        ///    <para>Announces to the component change service that a particular component has changed.</para>
        /// </devdoc>
        void OnComponentChanged(object component, MemberDescriptor member, object oldValue, object newValue);

        /// <devdoc>
        ///    <para>Announces to the component change service that a particular component is changing.</para>
        /// </devdoc>
        void OnComponentChanging(object component, MemberDescriptor member);
     }
}

