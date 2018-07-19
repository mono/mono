//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Net;
    using System.Runtime;

    class AuthenticationSchemesBindingParameter
    {
        AuthenticationSchemes authenticationSchemes = AuthenticationSchemes.None;

        public AuthenticationSchemesBindingParameter(AuthenticationSchemes authenticationSchemes)
        {
            Fx.Assert(authenticationSchemes != AuthenticationSchemes.None, "AuthenticationSchemesBindingParameter should not be added for AuthenticationSchemes.None.");

            this.authenticationSchemes = authenticationSchemes;
        }

        public AuthenticationSchemes AuthenticationSchemes
        {
            get { return this.authenticationSchemes; }
        }

        public static bool TryExtract(BindingParameterCollection collection, out AuthenticationSchemes authenticationSchemes)
        {
            Fx.Assert(collection != null, "collection != null");
            authenticationSchemes = AuthenticationSchemes.None;
            AuthenticationSchemesBindingParameter instance = collection.Find<AuthenticationSchemesBindingParameter>();
            if (instance != null)
            {
                authenticationSchemes = instance.AuthenticationSchemes;
                return true;
            }
            return false;
        }
    }
}
