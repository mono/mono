//------------------------------------------------------------------------------
// <copyright file="ClientUrlResolverWrapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Web.UI;

    internal sealed class ClientUrlResolverWrapper : IClientUrlResolver {
        private readonly Control _control;

        public ClientUrlResolverWrapper(Control control) {
            _control = control;
        }

        #region IClientUrlResolver Members
        // DevDiv Bugs 197242: AppRelativeTemplateSourceDirectory needed for
        // CompositeReference url resolution
        string IClientUrlResolver.AppRelativeTemplateSourceDirectory {
            get {
                return _control.AppRelativeTemplateSourceDirectory;
            }
        }

        string IClientUrlResolver.ResolveClientUrl(string relativeUrl) {
            IClientUrlResolver resolver = _control as IClientUrlResolver;
            if (resolver != null) {
                return resolver.ResolveClientUrl(relativeUrl);
            }
            else {
                return _control.ResolveClientUrl(relativeUrl);
            }
        }
        #endregion
    }
}
