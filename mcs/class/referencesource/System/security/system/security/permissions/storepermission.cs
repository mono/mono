// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

//
//  StorePermission.cs
//

namespace System.Security.Permissions {
    using System.Globalization;

    [Serializable()] 
    public sealed class StorePermission : CodeAccessPermission, IUnrestrictedPermission {
        private StorePermissionFlags m_flags;

        public StorePermission (PermissionState state) {
            if (state == PermissionState.Unrestricted)
                m_flags = StorePermissionFlags.AllFlags;
            else if (state == PermissionState.None)
                m_flags = StorePermissionFlags.NoFlags;
            else
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidPermissionState));
        }

        public StorePermission (StorePermissionFlags flag) {
            VerifyFlags(flag);
            m_flags = flag;
        }

        public StorePermissionFlags Flags {
            set {
                VerifyFlags(value);
                m_flags = value;
            }
            get {
                return m_flags;
            }
        }

        //
        // IUnrestrictedPermission implementation
        //

        public bool IsUnrestricted ()  {
            return m_flags == StorePermissionFlags.AllFlags;
        }

        //
        // IPermission implementation
        //
        
        public override IPermission Union (IPermission target) {
            if (target == null)
                return this.Copy();

            try {
                StorePermission operand = (StorePermission) target;
                StorePermissionFlags flag_union = m_flags | operand.m_flags;
                if (flag_union == StorePermissionFlags.NoFlags)
                    return null;
                else
                    return new StorePermission(flag_union);
            } 
            catch (InvalidCastException) {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Argument_WrongType), this.GetType().FullName));
            }
        }

        public override bool IsSubsetOf (IPermission target) {
            if (target == null) 
                return m_flags == StorePermissionFlags.NoFlags;

            try {
                StorePermission operand = (StorePermission) target;
                StorePermissionFlags sourceFlag = this.m_flags;
                StorePermissionFlags targetFlag = operand.m_flags;
                return ((sourceFlag & targetFlag) == sourceFlag);
            } 
            catch (InvalidCastException) {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Argument_WrongType), this.GetType().FullName));
            }
        }

        public override IPermission Intersect (IPermission target) {
            if (target == null)
                return null;

            try {
                StorePermission operand = (StorePermission) target;
                StorePermissionFlags flag_intersect = operand.m_flags & this.m_flags;
                if (flag_intersect == StorePermissionFlags.NoFlags)
                    return null;
                else
                    return new StorePermission(flag_intersect);
            } 
            catch (InvalidCastException) {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Argument_WrongType), this.GetType().FullName));
            }
        }

        public override IPermission Copy () {
            return new StorePermission((StorePermissionFlags)m_flags);
        }

        public override SecurityElement ToXml () {
            SecurityElement securityElement = new SecurityElement("IPermission");
            securityElement.AddAttribute("class", this.GetType().FullName + ", " + this.GetType().Module.Assembly.FullName.Replace('\"', '\''));
            securityElement.AddAttribute("version", "1");
            if (!IsUnrestricted()) 
                securityElement.AddAttribute("Flags", m_flags.ToString());
            else 
                securityElement.AddAttribute("Unrestricted", "true");

            return securityElement;
        }

        public override void FromXml (SecurityElement securityElement) {
            if (securityElement == null)
                throw new ArgumentNullException("securityElement");

            string className = securityElement.Attribute("class");
            if (className == null || className.IndexOf(this.GetType().FullName, StringComparison.Ordinal) == -1)
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidClassAttribute), "securityElement");

            string unrestricted = securityElement.Attribute("Unrestricted");
            if (unrestricted != null && String.Compare(unrestricted, "true", StringComparison.OrdinalIgnoreCase) == 0) {
                m_flags = StorePermissionFlags.AllFlags;
                return;
            }

            m_flags = StorePermissionFlags.NoFlags;
            String strFlags = securityElement.Attribute("Flags");
            if (strFlags != null) {
                StorePermissionFlags flags = (StorePermissionFlags) Enum.Parse(typeof(StorePermissionFlags), strFlags);
                VerifyFlags(flags);
                m_flags = flags;
            }
        }

        internal static void VerifyFlags (StorePermissionFlags flags) {
            if ((flags & ~StorePermissionFlags.AllFlags) != 0)
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Arg_EnumIllegalVal), (int)flags));
        }
    }
}
