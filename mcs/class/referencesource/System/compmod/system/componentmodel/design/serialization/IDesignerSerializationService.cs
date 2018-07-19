//------------------------------------------------------------------------------
// <copyright file="IDesignerSerializationService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel.Design.Serialization {
    using System.Collections;
    using System.ComponentModel;

    /// <devdoc>
    ///     This service provides a way to exchange a collection of objects
    ///     for a serializable object that represents them.  The returned
    ///     object contains live references to objects in the collection.
    ///     This returned object can then be passed to any runtime
    ///     serialization mechanism.  The object itself serializes
    ///     components the same way designers write source for them; by picking
    ///     them apart property by property.  Many objects do not support
    ///     runtime serialization because their internal state cannot be
    ///     adequately duplicated.  All components that support a designer,
    ///     however, must support serialization by walking their public
    ///     properties, methods and events.  This interface uses this
    ///     technique to convert a collection of components into a single
    ///     opaque object that does support runtime serialization.
    /// </devdoc>
    public interface IDesignerSerializationService {
    
        /// <devdoc>
        ///    <para>
        ///     Deserializes the provided serialization data object and
        ///     returns a collection of objects contained within that
        ///     data.
        ///    </para>
        /// </devdoc>
        ICollection Deserialize(object serializationData);
        
        /// <devdoc>
        ///    <para>
        ///     Serializes the given collection of objects and 
        ///     stores them in an opaque serialization data object.
        ///     The returning object fully supports runtime serialization.
        ///    </para>
        /// </devdoc>
        object Serialize(ICollection objects);
    }
}

