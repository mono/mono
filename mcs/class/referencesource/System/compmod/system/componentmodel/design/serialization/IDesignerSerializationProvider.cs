//------------------------------------------------------------------------------
// <copyright file="IDesignerSerializationProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel.Design.Serialization {

    using System;

    /// <devdoc>
    ///     This interface defines a custom serialization provider.  This
    ///     allows outside objects to control serialization by providing
    ///     their own serializer objects.
    /// </devdoc>
    public interface IDesignerSerializationProvider {
    
        /// <devdoc>
        ///     This will be called by the serialization manager when it 
        ///     is trying to locate a serialzer for an object type.
        ///     If this serialization provider can provide a serializer
        ///     that is of the correct type, it should return it.
        ///     Otherwise, it should return null.
        ///
        ///     In order to break order dependencies between multiple
        ///     serialization providers the serialization manager will
        ///     loop through all serilaization provideres until the 
        ///     serilaizer returned reaches steady state.  Because
        ///     of this you should always check currentSerializer
        ///     before returning a new serializer.  If currentSerializer
        ///     is an instance of your serializer, then you should
        ///     either return it or return null to prevent an infinite
        ///     loop.
        /// </devdoc>
        object GetSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType);
    }
}

