//------------------------------------------------------------------------------
// <copyright file="DnsPermission.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {

    using System.Security;
    using System.Security.Permissions;
    using System.Globalization;
    
    //NOTE: While DnsPermissionAttribute resides in System.DLL,
    //      no classes from that DLL are able to make declarative usage of DnsPermission.

    [   AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor |
                        AttributeTargets.Class  | AttributeTargets.Struct      |
                        AttributeTargets.Assembly,
                        AllowMultiple = true, Inherited = false )]

    [Serializable()] sealed public class DnsPermissionAttribute : CodeAccessSecurityAttribute
    {
        public DnsPermissionAttribute ( SecurityAction action ): base( action )
        {
        }

        public override IPermission CreatePermission()
        {
            if (Unrestricted) {
                return new DnsPermission( PermissionState.Unrestricted);
            }
            else {
                return new DnsPermission( PermissionState.None);
            }
        }
    }


    /// <devdoc>
    ///    <para>
    ///    </para>
    /// </devdoc>
    [Serializable]
    public sealed class DnsPermission :  CodeAccessPermission, IUnrestrictedPermission {

        private bool                    m_noRestriction;

        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the <see cref='System.Net.DnsPermission'/>
        ///       class that passes all demands or that fails all demands.
        ///    </para>
        /// </devdoc>
        public DnsPermission(PermissionState state) {
            m_noRestriction = (state==PermissionState.Unrestricted);
        }

        internal DnsPermission(bool free) {
            m_noRestriction = free;
        }

        // IUnrestrictedPermission interface methods
        /// <devdoc>
        ///    <para>
        ///       Checks the overall permission state of the object.
        ///    </para>
        /// </devdoc>
        public bool IsUnrestricted() {
            return m_noRestriction;
        }

        // IPermission interface methods
        /// <devdoc>
        ///    <para>
        ///       Creates a copy of a <see cref='System.Net.DnsPermission'/> instance.
        ///    </para>
        /// </devdoc>
        public override IPermission Copy () {
            return new DnsPermission(m_noRestriction);
        }

        /// <devdoc>
        /// <para>Returns the logical union between two <see cref='System.Net.DnsPermission'/> instances.</para>
        /// </devdoc>
        public override IPermission Union(IPermission target) {
            // Pattern suggested by Security engine
            if (target==null) {
                return this.Copy();
            }
            DnsPermission other = target as DnsPermission;
            if(other == null) {
                throw new ArgumentException(SR.GetString(SR.net_perm_target), "target");
            }
            return new DnsPermission(m_noRestriction || other.m_noRestriction);
        }

        /// <devdoc>
        /// <para>Returns the logical intersection between two <see cref='System.Net.DnsPermission'/> instances.</para>
        /// </devdoc>
        public override IPermission Intersect(IPermission target) {
            // Pattern suggested by Security engine
            if (target==null) {
                return null;
            }
            DnsPermission other = target as DnsPermission;
            if(other == null) {
                throw new ArgumentException(SR.GetString(SR.net_perm_target), "target");
            }

            // return null if resulting permission is restricted and empty
            // Hence, the only way for a bool permission will be.
            if (this.m_noRestriction && other.m_noRestriction) {
                return new DnsPermission(true);
            }
            return null;
        }


        /// <devdoc>
        /// <para>Compares two <see cref='System.Net.DnsPermission'/> instances.</para>
        /// </devdoc>
        public override bool IsSubsetOf(IPermission target) {
            // Pattern suggested by Security engine
            if (target == null) {
                return m_noRestriction == false;
            }
            DnsPermission other = target as DnsPermission;
            if (other == null) {
                throw new ArgumentException(SR.GetString(SR.net_perm_target), "target");
            }
            //Here is the matrix of result based on m_noRestriction for me and she
            //    me.noRestriction      she.noRestriction   me.isSubsetOf(she)
            //                  0       0                   1
            //                  0       1                   1
            //                  1       0                   0
            //                  1       1                   1
            return (!m_noRestriction || other.m_noRestriction);
        }

        /// <devdoc>
        /// </devdoc>
        public override void FromXml(SecurityElement securityElement) {

            if (securityElement == null)
            {
                //
                // null SecurityElement
                //

                throw new ArgumentNullException("securityElement");
            }

            if (!securityElement.Tag.Equals("IPermission"))
            {
                //
                // SecurityElement must be a permission element
                //

                throw new ArgumentException(SR.GetString(SR.net_no_classname), "securityElement");
            }

            string className = securityElement.Attribute( "class" );

            if (className == null)
            {
                //
                // SecurityElement must be a permission element for this type
                //

                throw new ArgumentException(SR.GetString(SR.net_no_classname), "securityElement");
            }


            if (className.IndexOf( this.GetType().FullName ) < 0)
            {
                //
                // SecurityElement must be a permission element for this type
                //

                throw new ArgumentException(SR.GetString(SR.net_no_typename), "securityElement");
            }

            string str = securityElement.Attribute( "Unrestricted" );
            m_noRestriction = (str!=null?(0 == string.Compare( str, "true", StringComparison.OrdinalIgnoreCase)):false);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override SecurityElement ToXml() {

            SecurityElement securityElement = new SecurityElement( "IPermission" );

            securityElement.AddAttribute( "class", this.GetType().FullName + ", " + this.GetType().Module.Assembly.FullName.Replace( '\"', '\'' ) );
            securityElement.AddAttribute( "version", "1" );

            if (m_noRestriction) {
                securityElement.AddAttribute( "Unrestricted", "true" );
            }

            return securityElement;
        }

    } // class DnsPermission


} // namespace System.Net
