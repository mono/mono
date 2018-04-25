//------------------------------------------------------------------------------
// <copyright file="URIFormatException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System {
    using System.Runtime.Serialization;
    /// <devdoc>
    ///    <para>
    ///       An exception class used when an invalid Uniform Resource Identifier is detected.
    ///    </para>
    /// </devdoc>
    [Serializable]
    public class UriFormatException : FormatException, ISerializable {

        // constructors

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public UriFormatException() : base() {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public UriFormatException(string textString) : base(textString) {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public UriFormatException(string textString, Exception e) : base(textString, e) {
        }

        protected UriFormatException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext) {
        }

        /// <internalonly/>
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext) {
            base.GetObjectData(serializationInfo, streamingContext);
        }

        // accessors

        // methods

        // data

    }; // class UriFormatException


} // namespace System
