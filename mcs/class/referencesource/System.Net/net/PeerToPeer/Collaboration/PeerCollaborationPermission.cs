//------------------------------------------------------------------------------
// <copyright file="PeerCollaborationPermission.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.PeerToPeer.Collaboration
{
    using System.Security;
    using System.Security.Permissions;
    using System.Globalization;

    /// <remarks>
    /// PeerCollaborationPermissionAttribute atrribute
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor |
                     AttributeTargets.Class | AttributeTargets.Struct |
                     AttributeTargets.Assembly,
                     AllowMultiple = true, Inherited = false)]
    [Serializable()]
    public sealed class PeerCollaborationPermissionAttribute : CodeAccessSecurityAttribute
    {
        /// <summary>
        /// Just call base constructor
        /// </summary>
        /// <param name="action"></param>
        public PeerCollaborationPermissionAttribute(SecurityAction action) : base(action) { }

        /// <summary>
        /// As required by the SecurityAttribute class. 
        /// </summary>
        /// <returns></returns>
        public override IPermission CreatePermission()
        {
            if (Unrestricted){
                return new PeerCollaborationPermission(PermissionState.Unrestricted);
            }
            else{
                return new PeerCollaborationPermission(PermissionState.None);
            }
        }
    }

    /// <remarks>
    /// Currently we only support two levels - Unrestrictred or none
    /// </remarks>
    [Serializable]
    public sealed class PeerCollaborationPermission : CodeAccessPermission, IUnrestrictedPermission
    {
        private bool m_noRestriction;

        internal static readonly PeerCollaborationPermission UnrestrictedPeerCollaborationPermission = 
            new PeerCollaborationPermission(PermissionState.Unrestricted);

        /// <summary>
        ///    <para>
        ///       Creates a new instance of the <see cref='System.Net.PeerToPeer.Collaboration.PeerCollaborationPermission'/>
        ///       class that passes all demands or that fails all demands.
        ///    </para>
        /// </summary>
        public PeerCollaborationPermission(PermissionState state)
        {
            m_noRestriction = (state == PermissionState.Unrestricted);
        }

        internal PeerCollaborationPermission(bool free)
        {
            m_noRestriction = free;
        }

        // IUnrestrictedPermission interface methods
        /// <summary>
        ///    <para>
        ///       Checks the overall permission state of the object.
        ///    </para>
        /// </summary>
        public bool IsUnrestricted()
        {
            return m_noRestriction;
        }

        // IPermission interface methods
        /// <summary>
        ///    <para>
        ///       Creates a copy of a <see cref='System.Net.PeerToPeer..Collaboration.PeerCollaborationPermission'/> instance.
        ///    </para>
        /// </summary>
        public override IPermission Copy()
        {
            if (m_noRestriction)
                return new PeerCollaborationPermission(true);
            else
                return new PeerCollaborationPermission(false);
        }

        /// <summary>
        /// <para>Returns the logical union between two <see cref='System.Net.PeerToPeer..Collaboration.PeerCollaborationPermission'/> instances.</para>
        /// </summary>
        public override IPermission Union(IPermission target)
        {
            // Pattern suggested by Security engine
            if (target == null){
                return this.Copy();
            }
            PeerCollaborationPermission other = target as PeerCollaborationPermission;
            if (other == null){
                throw new ArgumentException(SR.GetString(SR.Collab_PermissionUnionError), "target");
            }
            return new PeerCollaborationPermission(m_noRestriction || other.m_noRestriction);
        }

        /// <summary>
        /// <para>Returns the logical intersection between two <see cref='System.Net.PeerToPeer..Collaboration.PeerCollaborationPermission'/> instances.</para>
        /// </summary>
        public override IPermission Intersect(IPermission target)
        {
            // Pattern suggested by Security engine
            if (target == null){
                return null;
            }

            PeerCollaborationPermission other = target as PeerCollaborationPermission;
            if (other == null){
                throw new ArgumentException(SR.GetString(SR.Collab_PermissionIntersectError), "target");
            }

            // return null if resulting permission is restricted and empty
            // Hence, the only way for a bool permission will be.
            if (this.m_noRestriction && other.m_noRestriction){
                return new PeerCollaborationPermission(true);
            }
            return null;
        }


        /// <summary>
        /// <para>Compares two <see cref='System.Net.PeerToPeer..Collaboration.PeerCollaborationPermission'/> instances.</para>
        /// </summary>
        public override bool IsSubsetOf(IPermission target)
        {
            // Pattern suggested by Security engine
            if (target == null){
                return m_noRestriction == false;
            }

            PeerCollaborationPermission other = target as PeerCollaborationPermission;
            if (other == null){
                throw new ArgumentException(SR.GetString(SR.Collab_BadPermissionTarget), "target");
            }

            //Here is the matrix of result based on m_noRestriction for me and she
            //    me.noRestriction      she.noRestriction   me.isSubsetOf(she)
            //                  0       0                   1
            //                  0       1                   1
            //                  1       0                   0
            //                  1       1                   1
            return (!m_noRestriction || other.m_noRestriction);
        }

        /// <summary>
        /// Copy from a security element
        /// </summary>
        /// <param name="securityElement"></param>
        public override void FromXml(SecurityElement e)
        {
            if (e == null){
                throw new ArgumentNullException("e");
            }

            // SecurityElement must be a permission element
            if (!e.Tag.Equals("IPermission"))
            {
                throw new ArgumentException(SR.GetString(SR.InvalidSecurityElem), "e");
            }
            string className = e.Attribute("class");
            // SecurityElement must be a permission element for this type
            if (className == null){
                throw new ArgumentException(SR.GetString(SR.InvalidSecurityElemNoClass), "e");
            }

            if (className.IndexOf(this.GetType().FullName, StringComparison.Ordinal) < 0){
                throw new ArgumentException(SR.GetString(SR.InvalidSecurityElemNoType), "e");
            }
            string str = e.Attribute("Unrestricted");
            m_noRestriction = (str != null ? (0 == string.Compare(str, "true", StringComparison.OrdinalIgnoreCase)) : false);
        }

        /// <summary>
        /// Copyto a security element 
        /// </summary>
        /// <returns></returns>
        public override SecurityElement ToXml()
        {
            SecurityElement securityElement = new SecurityElement("IPermission");
            securityElement.AddAttribute("class", this.GetType().FullName + ", " + this.GetType().Module.Assembly.FullName.Replace('\"', '\''));
            securityElement.AddAttribute("version", "1");
            
            if (m_noRestriction){
                securityElement.AddAttribute("Unrestricted", "true");
            }
            return securityElement;
        }
    } 
}
