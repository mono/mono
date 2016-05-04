//------------------------------------------------------------------------------
// <copyright file="IEventPropertyService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel.Design {
    using System;
    using System.Collections;
    using System.ComponentModel;
    using Microsoft.Win32;

    /// <devdoc>
    /// <para>Provides a set of useful methods for binding <see cref='System.ComponentModel.EventDescriptor'/> objects to user code.</para>
    /// </devdoc>
    [System.Runtime.InteropServices.ComVisible(true)]
    public interface IEventBindingService {

        /// <devdoc>
        ///     This creates a name for an event handling method for the given component
        ///     and event.  The name that is created is guaranteed to be unique in the user's source
        ///     code.
        /// </devdoc>
        string CreateUniqueMethodName(IComponent component, EventDescriptor e);
        
        /// <devdoc>
        ///     Retrieves a collection of strings.  Each string is the name of a method
        ///     in user code that has a signature that is compatible with the given event.
        /// </devdoc>
        ICollection GetCompatibleMethods(EventDescriptor e);
        
        /// <devdoc>
        ///     For properties that are representing events, this will return the event
        ///     that the property represents.
        /// </devdoc>
        EventDescriptor GetEvent(PropertyDescriptor property);

        /// <devdoc>
        ///    <para>Converts a set of event descriptors to a set of property descriptors.</para>
        /// </devdoc>
        PropertyDescriptorCollection GetEventProperties(EventDescriptorCollection events);

        /// <devdoc>
        ///    <para>
        ///       Converts a single event to a property.
        ///    </para>
        /// </devdoc>
        PropertyDescriptor GetEventProperty(EventDescriptor e);
        
        /// <devdoc>
        ///     Displays the user code for the designer.  This will return true if the user
        ///     code could be displayed, or false otherwise.
        /// </devdoc>
        bool ShowCode();
        
        /// <devdoc>
        ///     Displays the user code for the designer.  This will return true if the user
        ///     code could be displayed, or false otherwise.
        /// </devdoc>
        bool ShowCode(int lineNumber);
        
        /// <devdoc>
        ///     Displays the user code for the given event.  This will return true if the user
        ///     code could be displayed, or false otherwise.
        /// </devdoc>
        bool ShowCode(IComponent component, EventDescriptor e);
    }
}

