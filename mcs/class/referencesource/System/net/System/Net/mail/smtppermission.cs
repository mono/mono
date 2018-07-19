//------------------------------------------------------------------------------
// <copyright file="SmtpPermission.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Mail {

    using System.Collections;
    using System.Security;
    using System.Security.Permissions;
    using System.Globalization;
    using System.Threading;

    /// <include file='doc\SmtpPermission.uex' path='docs/doc[@for="SmtpPermissionFlag"]/*' />
    public enum SmtpAccess { None = 0, Connect = 1, ConnectToUnrestrictedPort = 2};


    /// <include file='doc\SmtpPermission.uex' path='docs/doc[@for="SmtpPermissionAttribute"]/*' />
    [   AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor |
                        AttributeTargets.Class  | AttributeTargets.Struct      |
                        AttributeTargets.Assembly,
                        AllowMultiple = true, Inherited = false )]
    
    [Serializable]
    public sealed class SmtpPermissionAttribute: CodeAccessSecurityAttribute
    {
        private const string strAccess     = "Access";
        private string  access = null;


        /// <include file='doc\SmtpPermission.uex' path='docs/doc[@for="SmtpPermissionAttribute.SmtpPermissionAttribute"]/*' />
        public SmtpPermissionAttribute(SecurityAction action): base( action )
        {
        }
        

        public string Access {
            get{
                return access;
            }
            set{
                access = value;
            }
        }

        /// <include file='doc\SmtpPermission.uex' path='docs/doc[@for="SmtpPermissionAttribute.CreatePermission"]/*' />
        public override IPermission CreatePermission()
        {
            SmtpPermission perm = null;
            if (Unrestricted) {
                perm = new SmtpPermission(PermissionState.Unrestricted);
            }
            else {
                perm = new SmtpPermission(PermissionState.None);
                if (access != null) {
                    if (0 == string.Compare(access, "Connect", StringComparison.OrdinalIgnoreCase)) {
                        perm.AddPermission(SmtpAccess.Connect);
                    }
                    else if (0 == string.Compare(access, "ConnectToUnrestrictedPort", StringComparison.OrdinalIgnoreCase)) {
                        perm.AddPermission(SmtpAccess.ConnectToUnrestrictedPort);
                    }
                    else if (0 == string.Compare(access, "None", StringComparison.OrdinalIgnoreCase)) {
                        perm.AddPermission(SmtpAccess.None);
                    }
                    else {
                        throw new ArgumentException(SR.GetString(SR.net_perm_invalid_val, strAccess, access));
                    }
                }
            }
            return perm;
        }

    }



    /// <include file='doc\SmtpPermission.uex' path='docs/doc[@for="SmtpPermission"]/*' />
    [Serializable]
    public sealed class SmtpPermission: CodeAccessPermission, IUnrestrictedPermission {

        SmtpAccess access;
        private bool unrestricted;
        
        /// <include file='doc\SmtpPermission.uex' path='docs/doc[@for="SmtpPermission.SmtpPermission"]/*' />
        public SmtpPermission(PermissionState state) {
            if (state == PermissionState.Unrestricted){
                access = SmtpAccess.ConnectToUnrestrictedPort;
                unrestricted = true;
            }
            else{
                access = SmtpAccess.None;
            }
        }
        
        public SmtpPermission(bool unrestricted) {
            if (unrestricted){
                access = SmtpAccess.ConnectToUnrestrictedPort;
                this.unrestricted = true;
            }
            else{
                access = SmtpAccess.None;
            }
        }



        /// <include file='doc\SmtpPermission.uex' path='docs/doc[@for="SmtpPermission.SmtpPermission1"]/*' />
        public SmtpPermission(SmtpAccess access) {
            this.access = access;
        }

        public SmtpAccess Access {
            get{
                return access;
            }
        }
        
        /// <include file='doc\SmtpPermission.uex' path='docs/doc[@for="SmtpPermission.AddPermission"]/*' />
        public void AddPermission(SmtpAccess access) {
            if (access > this.access)
                this.access = access;
        }

        /// <include file='doc\SmtpPermission.uex' path='docs/doc[@for="SmtpPermission.IsUnrestricted"]/*' />
        public bool IsUnrestricted() {
            return unrestricted;
        }

        /// <include file='doc\SmtpPermission.uex' path='docs/doc[@for="SmtpPermission.Copy"]/*' />
        public override IPermission Copy() {
            if(unrestricted){
                return new SmtpPermission(true);
            }
            return new SmtpPermission(access);
        }
        
        /// <include file='doc\SmtpPermission.uex' path='docs/doc[@for="SmtpPermission.Union"]/*' />
        public override IPermission Union(IPermission target) {
            if (target==null) {
                return this.Copy();
            }
            SmtpPermission other = target as SmtpPermission;
            if(other == null) {
                throw new ArgumentException(SR.GetString(SR.net_perm_target), "target");
            }

            if(unrestricted || other.IsUnrestricted()){
                return new SmtpPermission(true);
            }
            
            return new SmtpPermission(this.access > other.access ? this.access : other.access);
        }

        /// <include file='doc\SmtpPermission.uex' path='docs/doc[@for="SmtpPermission.Intersect"]/*' />
        public override IPermission Intersect(IPermission target) {
            if (target == null) {
                return null;
            }

            SmtpPermission other = target as SmtpPermission;
            if(other == null) {
                throw new ArgumentException(SR.GetString(SR.net_perm_target), "target");
            }

            if(IsUnrestricted() && other.IsUnrestricted()){
                return new SmtpPermission(true);
            }
            
            return new SmtpPermission(this.access < other.access ? this.access : other.access);
        }

        /// <include file='doc\SmtpPermission.uex' path='docs/doc[@for="SmtpPermission.IsSubsetOf"]/*' />
        public override bool IsSubsetOf(IPermission target) {
            // Pattern suggested by security engine
            if (target == null) {
                return (access == SmtpAccess.None);
            }

            SmtpPermission other = target as SmtpPermission;
            if (other == null) {
                throw new ArgumentException(SR.GetString(SR.net_perm_target), "target");
            }

            if(unrestricted && !other.IsUnrestricted()){
                return false;
            }

            return (other.access >= access);
        }

        /// <include file='doc\SmtpPermission.uex' path='docs/doc[@for="SmtpPermission.FromXml"]/*' />
        public override void FromXml(SecurityElement securityElement) {
            if (securityElement == null) {
               throw new ArgumentNullException("securityElement");
            }
            if (!securityElement.Tag.Equals("IPermission")) {
                throw new ArgumentException(SR.GetString(SR.net_not_ipermission), "securityElement");
            }

            string className = securityElement.Attribute("class");

            if (className == null) {
                throw new ArgumentException(SR.GetString(SR.net_no_classname), "securityElement");
            }
            if (className.IndexOf(this.GetType().FullName) < 0) {
                throw new ArgumentException(SR.GetString(SR.net_no_typename), "securityElement");
            }

            String str = securityElement.Attribute("Unrestricted");
            if (str != null) {
                if (0 == string.Compare( str, "true", StringComparison.OrdinalIgnoreCase)){
                    access = SmtpAccess.ConnectToUnrestrictedPort;
                    unrestricted = true;
                    return;
                }
            }

            str = securityElement.Attribute("Access");
            if (str != null) {
               if(0 == string.Compare(str, "Connect", StringComparison.OrdinalIgnoreCase)){
                   access = SmtpAccess.Connect;
               }
               else if(0 == string.Compare(str, "ConnectToUnrestrictedPort", StringComparison.OrdinalIgnoreCase)){
                   access = SmtpAccess.ConnectToUnrestrictedPort;
               }
               else if(0 == string.Compare(str, "None", StringComparison.OrdinalIgnoreCase)){
                   access = SmtpAccess.None;
               }
               else{
                   throw new ArgumentException(SR.GetString(SR.net_perm_invalid_val_in_element), "Access");
               }
            }
        }
                                           
        /// <include file='doc\SmtpPermission.uex' path='docs/doc[@for="SmtpPermission.ToXml"]/*' />
        public override SecurityElement ToXml() {

            SecurityElement securityElement = new SecurityElement( "IPermission" );

            securityElement.AddAttribute("class", this.GetType().FullName + ", " + this.GetType().Module.Assembly.FullName.Replace( '\"', '\'' ));
            securityElement.AddAttribute("version", "1");

            if(unrestricted){
                securityElement.AddAttribute("Unrestricted", "true");
                return securityElement;
            }

            if (access == SmtpAccess.Connect) {
                securityElement.AddAttribute("Access", "Connect");
            }
            else if (access == SmtpAccess.ConnectToUnrestrictedPort) {
                securityElement.AddAttribute("Access", "ConnectToUnrestrictedPort");
            }
            return securityElement;
        }
    }// class SmtpPermission

  
} // namespace System.Net
