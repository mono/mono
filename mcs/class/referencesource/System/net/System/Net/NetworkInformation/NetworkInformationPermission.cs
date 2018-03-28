//------------------------------------------------------------------------------
// <copyright file="NetworkInformationPermission.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.NetworkInformation {

    using System.Collections;
    using System.Security;
    using System.Security.Permissions;
    using System.Globalization;
    using System.Threading;

    [Flags]
    public enum NetworkInformationAccess {None = 0, Read=1, Ping = 4};


    [   AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor |
                        AttributeTargets.Class  | AttributeTargets.Struct      |
                        AttributeTargets.Assembly,
                        AllowMultiple = true, Inherited = false )]
    
    [Serializable]
    public sealed class NetworkInformationPermissionAttribute: CodeAccessSecurityAttribute
    {
        private const string strAccess     = "Access";
        private string  access = null;


        public NetworkInformationPermissionAttribute( SecurityAction action ): base( action )
        {
        }
        

        public string Access{
            get{
                return access;
            }
            set{
                access = value;
            }
        }

        public override IPermission CreatePermission()
        {
            NetworkInformationPermission perm = null;
            if (Unrestricted) {
                perm = new NetworkInformationPermission(PermissionState.Unrestricted);
            }
            else {
                perm = new NetworkInformationPermission(PermissionState.None);
                if (access != null) {
                    if (0 == string.Compare(access, "Read", StringComparison.OrdinalIgnoreCase)) {
                        perm.AddPermission(NetworkInformationAccess.Read);
                    }
                    else if (0 == string.Compare(access, "Ping", StringComparison.OrdinalIgnoreCase)) {
                        perm.AddPermission(NetworkInformationAccess.Ping);
                    }
                    else if (0 == string.Compare(access, "None", StringComparison.OrdinalIgnoreCase)) {
                        perm.AddPermission(NetworkInformationAccess.None);
                    }
                    else {
                        throw new ArgumentException(SR.GetString(SR.net_perm_invalid_val, strAccess, access));
                    }
                }
            }
            return perm;
        }

    }



    [Serializable]
    public sealed class NetworkInformationPermission : CodeAccessPermission, IUnrestrictedPermission {

        NetworkInformationAccess access;
        bool unrestricted;
        
        public NetworkInformationPermission(PermissionState state) {
            if (state == PermissionState.Unrestricted){
                access = NetworkInformationAccess.Read | NetworkInformationAccess.Ping;
                unrestricted = true;
            }
            else{
                access = NetworkInformationAccess.None;
            }
        }
        
        internal NetworkInformationPermission(bool unrestricted) {
            if (unrestricted){
                access = NetworkInformationAccess.Read | NetworkInformationAccess.Ping;
                unrestricted = true;
            }
            else{
                access = NetworkInformationAccess.None;
            }
        }



        public NetworkInformationPermission(NetworkInformationAccess access) {
            this.access = access;
        }

        public NetworkInformationAccess Access{
            get{
                return access;
            }
        }

        public void AddPermission(NetworkInformationAccess access) {
            this.access|=access;
        }

        public bool IsUnrestricted() {
            return unrestricted;
        }

        public override IPermission Copy() {
            if(unrestricted){
                return new NetworkInformationPermission(true);
            }
            return new NetworkInformationPermission(access);
        }
        
        public override IPermission Union(IPermission target) {
            if (target==null) {
                return this.Copy();
            }
            NetworkInformationPermission other = target as NetworkInformationPermission;
            if(other == null) {
                throw new ArgumentException(SR.GetString(SR.net_perm_target), "target");
            }
            
            if(unrestricted || other.IsUnrestricted()){
                return new NetworkInformationPermission(true);
            }

            return new NetworkInformationPermission(this.access | other.access);
        }

        public override IPermission Intersect(IPermission target) {
            if (target == null) {
                return null;
            }

            NetworkInformationPermission other = target as NetworkInformationPermission;
            if(other == null) {
                throw new ArgumentException(SR.GetString(SR.net_perm_target), "target");
            }

            if(unrestricted && other.IsUnrestricted()){
                return new NetworkInformationPermission(true);
            }
            
            return new NetworkInformationPermission(access & other.access);
        }

        public override bool IsSubsetOf(IPermission target) {
            // Pattern suggested by security engine
            if (target == null) {
                return (access == NetworkInformationAccess.None);
            }

            NetworkInformationPermission other = target as NetworkInformationPermission;
            if (other == null) {
                throw new ArgumentException(SR.GetString(SR.net_perm_target), "target");
            }

            if(unrestricted && !other.IsUnrestricted()){
                return false;
            }
            else if ((access & other.access) == access) {
                return true;
            }
            return false;
        }

        public override void FromXml(SecurityElement securityElement) {
            access = NetworkInformationAccess.None;

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
                    access = NetworkInformationAccess.Read | NetworkInformationAccess.Ping;
                    unrestricted = true;
                    return;
                }
            }

            if(securityElement.Children != null)
            {
                foreach(SecurityElement child in securityElement.Children)
                {
                    str = child.Attribute("Access");
                    if(0 == string.Compare(str, "Read", StringComparison.OrdinalIgnoreCase)){
                        access |= NetworkInformationAccess.Read;
                    }
                    else if(0 == string.Compare(str, "Ping", StringComparison.OrdinalIgnoreCase)){
                        access |= NetworkInformationAccess.Ping;
                    }
                }     
            }
        }
                                           
        public override SecurityElement ToXml() {

            SecurityElement securityElement = new SecurityElement( "IPermission" );

            securityElement.AddAttribute("class", this.GetType().FullName + ", " + this.GetType().Module.Assembly.FullName.Replace( '\"', '\'' ));
            securityElement.AddAttribute("version", "1");

            if(unrestricted){
                securityElement.AddAttribute("Unrestricted", "true");
                return securityElement;
            }

            if ((access & NetworkInformationAccess.Read) > 0) {
                SecurityElement child = new SecurityElement("NetworkInformationAccess");
                child.AddAttribute( "Access", "Read");
                securityElement.AddChild(child);
            }
            if ((access & NetworkInformationAccess.Ping) > 0) {
                SecurityElement child = new SecurityElement("NetworkInformationAccess");
                child.AddAttribute( "Access", "Ping");
                securityElement.AddChild(child);
            }
            return securityElement;
        }
    }// class NetworkInformationPermission

  
} // namespace System.Net
