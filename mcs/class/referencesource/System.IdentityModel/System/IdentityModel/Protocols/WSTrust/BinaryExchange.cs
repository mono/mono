//-----------------------------------------------------------------------
// <copyright file="BinaryExchange.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    /// <summary>
    /// Represents the contents of the BinaryExchange element.  
    /// </summary>
    public class BinaryExchange
    {
        byte[] _binaryData;
        Uri _valueType;
        Uri _encodingType;

        /// <summary>
        /// Creates an instance of <see cref="BinaryExchange"/>
        /// </summary>
        /// <param name="binaryData">Binary data exchanged.</param>
        /// <param name="valueType">Uri representing the value type of the binary data.</param>
        /// <exception cref="ArgumentNullException">Input parameter 'binaryData' or 'valueType' is null.</exception>
        public BinaryExchange( byte[] binaryData, Uri valueType )
            : this( binaryData, valueType, new Uri( WSSecurity10Constants.EncodingTypes.Base64 ) )
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="BinaryExchange"/>
        /// </summary>
        /// <param name="binaryData">Binary data exchanged.</param>
        /// <param name="valueType">Uri representing the value type of the binary data.</param>
        /// <param name="encodingType">Encoding type to be used for encoding teh </param>
        /// <exception cref="ArgumentNullException">Input parameter 'binaryData', 'valueType' or 'encodingType' is null.</exception>
        public BinaryExchange( byte[] binaryData, Uri valueType, Uri encodingType )
        {
            if ( binaryData == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "binaryData" );
            }

            if ( valueType == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "valueType" );
            }

            if ( encodingType == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "encodingType" );
            }

            if ( !valueType.IsAbsoluteUri )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument( "valueType", SR.GetString( SR.ID0013 ) );
            }

            if ( !encodingType.IsAbsoluteUri )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument( "encodingType", SR.GetString( SR.ID0013 ) );
            }

            _binaryData = binaryData;
            _valueType = valueType;
            _encodingType = encodingType;
        }

        /// <summary>
        /// Gets the Binary Data.
        /// </summary>
        public byte[] BinaryData
        {
            get { return _binaryData; }
        }

        /// <summary>
        /// Gets the ValueType Uri.
        /// </summary>
        public Uri ValueType
        {
            get { return _valueType; }
        }

        /// <summary>
        /// Gets the EncodingType Uri.
        /// </summary>
        public Uri EncodingType
        {
            get { return _encodingType; }
        }
    }
}
