//------------------------------------------------------------------------------
// <copyright file="RoleGroup.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.ComponentModel;
    using System.Security.Principal;
    using System.Web.Security;


    /// <devdoc>
    /// Associates a collection of roles with a template.
    /// </devdoc>
    public sealed class RoleGroup {
        private ITemplate _contentTemplate;
        private string[] _roles;


        /// <devdoc>
        /// The template associated with the roles.
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(LoginView)),
        ]
        public ITemplate ContentTemplate {
            get {
                return _contentTemplate;
            }
            set {
                _contentTemplate = value;
            }
        }


        /// <devdoc>
        /// The roles associated with the template.
        /// </devdoc>
        [
        TypeConverterAttribute(typeof(StringArrayConverter)),
        ]
        public string[] Roles {
            get {
                if (_roles == null) {
                    return new string[0];
                }
                else {
                    // Must clone to preserve encapsulation
                    return (string[]) _roles.Clone();
                }
            }
            set {
                if (value == null) {
                    _roles = value;
                }
                else {
                    // Must clone to preserve encapsulation
                    _roles = (string[]) value.Clone();
                }
            }
        }


        /// <devdoc>
        /// Whether the user is in any of the roles.
        /// </devdoc>
        public bool ContainsUser(IPrincipal user) {
            if (user == null) {
                throw new ArgumentNullException("user");
            }
            if (_roles == null) {
                return false;
            }
            foreach (string role in _roles) {
                if (user.IsInRole(role)) {
                    return true;
                }
            }
            return false;
        }


        /// <devdoc>
        /// For appearance in designer collection editor.
        /// </devdoc>
        public override string ToString() {
            StringArrayConverter converter = new StringArrayConverter();
            return converter.ConvertToString(Roles);
        }
    }
}
