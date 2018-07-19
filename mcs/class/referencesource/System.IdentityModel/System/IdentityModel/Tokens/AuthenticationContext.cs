//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// This class is used to specify the context of an authentication event.
    /// </summary>
    public class AuthenticationContext
    {
        Collection<string> _authorities; 
        string _contextClass;
        string _contextDeclaration;
        
        /// <summary>
        /// Creates an instance of AuthenticationContext.
        /// </summary>
        public AuthenticationContext()
        {
            _authorities = new Collection<string>();
        }

        /// <summary>
        /// The collection of authorities for resolving an authentication event.
        /// </summary>
        public Collection<string> Authorities
        {
            get { return _authorities; }
        }

        /// <summary>
        /// The context class for resolving an authentication event.
        /// </summary>
        public string ContextClass
        {
            get { return _contextClass; }
            set { _contextClass = value; }
        }

        /// <summary>
        /// The context declaration for resolving an authentication event.
        /// </summary>
        public string ContextDeclaration
        {
            get { return _contextDeclaration; }
            set { _contextDeclaration = value; }
        }
    }
}
