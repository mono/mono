//------------------------------------------------------------------------------
// <copyright file="PnrpPermission.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Net.PeerToPeer
{
    using System.Security;
    using System.Security.Permissions;
    using System.Globalization;

    /// <remarks>
    /// PnrpPermission atrribute
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor |
                     AttributeTargets.Class | AttributeTargets.Struct |
                     AttributeTargets.Assembly,
                     AllowMultiple = true, Inherited = false)]
    [Serializable()]
    public sealed class PnrpPermissionAttribute : CodeAccessSecurityAttribute
    {
        /// <summary>
        /// Just call base constructor
        /// </summary>
        /// <param name="action"></param>
        public PnrpPermissionAttribute(SecurityAction action) : base(action) { }

        /// <summary>
        /// As required by the SecurityAttribute class. 
        /// </summary>
        /// <returns></returns>
        public override IPermission CreatePermission() {
            if (Unrestricted) {
                return new PnrpPermission(PermissionState.Unrestricted);
            }
            else {
                return new PnrpPermission(PermissionState.None);
            }
        }
    }

    /// <remarks>
    /// Currently we only support two levels - Unrestrictred or none
    /// </remarks>
    [Serializable]
    public sealed class PnrpPermission : CodeAccessPermission, IUnrestrictedPermission
    {
        private bool m_noRestriction;

        internal static readonly PnrpPermission UnrestrictedPnrpPermission = new PnrpPermission(PermissionState.Unrestricted);

        /// <summary>
        ///    <para>
        ///       Creates a new instance of the <see cref='System.Net.PeerToPeer.PnrpPermission'/>
        ///       class that passes all demands or that fails all demands.
        ///    </para>
        /// </summary>
        public PnrpPermission(PermissionState state)
        {
            m_noRestriction = (state == PermissionState.Unrestricted);
        }

        internal PnrpPermission(bool free)
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
        ///       Creates a copy of a <see cref='System.Net.PeerToPeer.PnrpPermission'/> instance.
        ///    </para>
        /// </summary>
        public override IPermission Copy()
        {
            return new PnrpPermission(m_noRestriction);
        }

        /// <summary>
        /// <para>Returns the logical union between two <see cref='System.Net.PeerToPeer.PnrpPermission'/> instances.</para>
        /// </summary>
        public override IPermission Union(IPermission target)
        {
            // Pattern suggested by Security engine
            if (target == null) {
                return this.Copy();
            }
            PnrpPermission other = target as PnrpPermission;
            if (other == null) {
                throw new ArgumentException( SR.GetString(SR.PnrpPermission_CantUnionWithNonPnrpPermission), "target");
            }
            return new PnrpPermission(m_noRestriction || other.m_noRestriction);
        }

        /// <summary>
        /// <para>Returns the logical intersection between two <see cref='System.Net.PeerToPeer.PnrpPermission'/> instances.</para>
        /// </summary>
        public override IPermission Intersect(IPermission target)
        {
            // Pattern suggested by Security engine
            if (target == null) {
                return null;
            }
            PnrpPermission other = target as PnrpPermission;
            if (other == null) {
                throw new ArgumentException(SR.GetString(SR.PnrpPermission_CantIntersectWithNonPnrpPermission), "target");
            }
            // return null if resulting permission is restricted and empty
            // Hence, the only way for a bool permission will be.
            if (this.m_noRestriction && other.m_noRestriction) {
                return new PnrpPermission(true);
            }
            return null;
        }


        /// <summary>
        /// <para>Compares two <see cref='System.Net.PeerToPeer.PnrpPermission'/> instances.</para>
        /// </summary>
        public override bool IsSubsetOf(IPermission target)
        {
            // Pattern suggested by Security engine
            if (target == null)  {
                return m_noRestriction == false;
            }
            PnrpPermission other = target as PnrpPermission;
            if (other == null) {
                throw new ArgumentException(SR.GetString(SR.PnrpPermission_TargetNotAPnrpPermission), "target");
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
        /// Cinstrcy from a security element
        /// </summary>
        /// <param name="securityElement"></param>
        public override void FromXml(SecurityElement e)
        {
            if (e == null) {
                throw new ArgumentNullException(SR.GetString(SR.InvalidSecurityElem));
            }
            // SecurityElement must be a permission element
            if (!e.Tag.Equals("IPermission")) {
                throw new ArgumentException(SR.GetString(SR.InvalidSecurityElem), "securityElement");
            }
            string className = e.Attribute("class");
            // SecurityElement must be a permission element for this type
            if (className == null) {
                throw new ArgumentException(SR.GetString(SR.InvalidSecurityElem), "securityElement");
            }
            if (className.IndexOf(this.GetType().FullName, StringComparison.Ordinal) < 0) {
                throw new ArgumentException(SR.GetString(SR.InvalidSecurityElem), "securityElement");
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
            if (m_noRestriction) {
                securityElement.AddAttribute("Unrestricted", "true");
            }
            return securityElement;
        }
    } // class PnrpPermission
} // namespace System.Net.PeerToPeer
