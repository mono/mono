//------------------------------------------------------------------------------
// <copyright file="TokenBinding.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Security.Authentication.ExtendedProtection
{

    public enum TokenBindingType
    {
        Provided = 0,
        Referred = 1
    };

    public class TokenBinding
    {
        internal TokenBinding(TokenBindingType bindingType, byte[] rawData)
        {
            BindingType = bindingType;
            _rawTokenBindingId = rawData;
        }

        private byte[] _rawTokenBindingId = null;

        public byte[] GetRawTokenBindingId()
        {
            return (_rawTokenBindingId != null) ? (byte[])_rawTokenBindingId.Clone() : null;
        }

        public TokenBindingType BindingType
        {
            get;
            private set;
        }
    }
} 
