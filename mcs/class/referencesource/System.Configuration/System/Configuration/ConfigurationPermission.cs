//------------------------------------------------------------------------------
// <copyright file="ConfigurationPermission.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {

    using System.Security;
    using System.Security.Permissions;
    using System.Globalization;
    using System.Diagnostics.CodeAnalysis;
    
    [AttributeUsage(AttributeTargets.All, AllowMultiple=true, Inherited=false )]
    [Serializable] 
    sealed public class ConfigurationPermissionAttribute : CodeAccessSecurityAttribute
    {
        public ConfigurationPermissionAttribute(SecurityAction action) : base(action) {}

        public override IPermission CreatePermission() {
            PermissionState state = (this.Unrestricted) ? 
                    PermissionState.Unrestricted : PermissionState.None;

            return new ConfigurationPermission(state);
        }
    }

    //
    // ConfigurationPermission is used to grant access to configuration sections that
    // would not otherwise be available if the caller attempted to read the configuration
    // files that make up configuration.
    //
    // The permission is a simple boolean one - it is either fully granted or denied.
    // This boolean state is represented by using the PermissionState enumeration.
    //
    [Serializable]
    public sealed class ConfigurationPermission : CodeAccessPermission, IUnrestrictedPermission {

        private PermissionState _permissionState;  // Unrestricted or None

        //
        // Creates a new instance of ConfigurationPermission
        // that passes all demands or that fails all demands.
        //
        public ConfigurationPermission(PermissionState state) {

            // validate state parameter
            switch (state) {
            case PermissionState.Unrestricted:
            case PermissionState.None:
                _permissionState = state;
                break;

            default:
                throw ExceptionUtil.ParameterInvalid("state");
            }
        }

        //
        // IUnrestrictedPermission interface methods
        //

        //
        // Checks the overall permission state of the object.
        //
        public bool IsUnrestricted() {
            return _permissionState == PermissionState.Unrestricted;
        }

        //
        // Creates a copy.
        //
        [SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity", Justification = "This is a standard implementation of a copy method.")]
        public override IPermission Copy () {
            return new ConfigurationPermission(_permissionState);
        }

        //
        // Returns the logical union between ConfigurationPermission instances.
        //
        [SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity", Justification = "This is a standard implementation of a union method.")]
        public override IPermission Union(IPermission target) {
            if (target == null) {
                return Copy();
            }

            if (target.GetType() !=  typeof(ConfigurationPermission)) {
                throw ExceptionUtil.ParameterInvalid("target");
            }

            // Create an Unrestricted permission if either this or other is unrestricted
            if (_permissionState == PermissionState.Unrestricted) {
                return new ConfigurationPermission(PermissionState.Unrestricted);
            }
            else {
                ConfigurationPermission other = (ConfigurationPermission) target;
                return new ConfigurationPermission(other._permissionState);
            }
        }

        //
        // Returns the logical intersection between two ConfigurationPermission instances.
        //
        [SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity", Justification = "This is a standard implementation of an intersection method.")]
        public override IPermission Intersect(IPermission target) {
            if (target == null) {
                return null;
            }

            if (target.GetType() !=  typeof(ConfigurationPermission)) {
                throw ExceptionUtil.ParameterInvalid("target");
            }

            // Create an None permission if either this or other is None
            if (_permissionState == PermissionState.None) {
                return new ConfigurationPermission(PermissionState.None);
            }
            else {
                ConfigurationPermission other = (ConfigurationPermission) target;
                return new ConfigurationPermission(other._permissionState);
            }
        }

        //
        // Compares two ConfigurationPermission instances
        //
        public override bool IsSubsetOf(IPermission target) {
            if (target == null) {
                return _permissionState == PermissionState.None;
            }

            if (target.GetType() != typeof(ConfigurationPermission)) {
                throw ExceptionUtil.ParameterInvalid("target");
            }

            ConfigurationPermission other = (ConfigurationPermission) target;
            return (_permissionState == PermissionState.None || other._permissionState == PermissionState.Unrestricted);
        }

        public override void FromXml(SecurityElement securityElement) {
            if (securityElement == null) {
                throw new ArgumentNullException(SR.GetString(SR.ConfigurationPermissionBadXml,"securityElement"));
            }

            if (!securityElement.Tag.Equals("IPermission")) {
                throw new ArgumentException(SR.GetString(SR.ConfigurationPermissionBadXml,"securityElement"));
            }

            string className = securityElement.Attribute("class");
            if (className == null) {
                throw new ArgumentException(SR.GetString(SR.ConfigurationPermissionBadXml,"securityElement"));
            }

            if (className.IndexOf(this.GetType().FullName, StringComparison.Ordinal ) < 0) {
                throw new ArgumentException(SR.GetString(SR.ConfigurationPermissionBadXml,"securityElement"));
            }

            string version = securityElement.Attribute("version");
            if (version != "1") {
                throw new ArgumentException(SR.GetString(SR.ConfigurationPermissionBadXml,"version"));
            }

            string unrestricted = securityElement.Attribute("Unrestricted");
            if (unrestricted == null) {
                _permissionState = PermissionState.None;
            }
            else {
                switch (unrestricted) {
                    case "true":
                        _permissionState = PermissionState.Unrestricted;
                        break;

                    case "false":
                        _permissionState = PermissionState.None;
                        break;

                    default:
                        throw new ArgumentException(SR.GetString(SR.ConfigurationPermissionBadXml,"Unrestricted"));
                }
            }
        }

        public override SecurityElement ToXml() {
            SecurityElement securityElement = new SecurityElement("IPermission");
            securityElement.AddAttribute("class", this.GetType().FullName + ", " + this.GetType().Module.Assembly.FullName.Replace( '\"', '\'' ));
            securityElement.AddAttribute("version", "1");
            if (IsUnrestricted()) {
                securityElement.AddAttribute("Unrestricted", "true");
            }

            return securityElement;
        }
    }
}

