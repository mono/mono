// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// TypeDescriptorPermission.cs
//
// <OWNER>wilcob</OWNER>
//

namespace System.Security.Permissions
{
    using System;
    using System.IO;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security;
    using System.Reflection;
    using System.Globalization;
    using System.Diagnostics.Contracts;

    [Flags]
    [Serializable]
    public enum TypeDescriptorPermissionFlags
    {
        NoFlags = 0,
        RestrictedRegistrationAccess = 1
    }

    [Serializable]
    sealed public class TypeDescriptorPermission
           : CodeAccessPermission, IUnrestrictedPermission
    {
        private TypeDescriptorPermissionFlags m_flags;

        public TypeDescriptorPermission(PermissionState state)
        {
            if (state == PermissionState.Unrestricted)
            {
                SetUnrestricted(true);
            }
            else if (state == PermissionState.None)
            {
                SetUnrestricted(false);
            }
            else
            {
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidPermissionState));
            }
        }

        public TypeDescriptorPermission(TypeDescriptorPermissionFlags flag)
        {
            VerifyAccess(flag);

            SetUnrestricted(false);
            m_flags = flag;
        }

        private void SetUnrestricted(bool unrestricted)
        {
            if (unrestricted)
            {
                m_flags = TypeDescriptorPermissionFlags.RestrictedRegistrationAccess;
            }
            else
            {
                Reset();
            }
        }

        private void Reset()
        {
            m_flags = TypeDescriptorPermissionFlags.NoFlags;
        }


        public TypeDescriptorPermissionFlags Flags
        {
            set
            {
                VerifyAccess(value);
                m_flags = value;
            }

            get
            {
                return m_flags;
            }
        }


        //
        // CodeAccessPermission implementation
        //

        public bool IsUnrestricted()
        {
            return m_flags == TypeDescriptorPermissionFlags.RestrictedRegistrationAccess;
        }

        //
        // IPermission implementation
        //

        public override IPermission Union(IPermission target)
        {
            if (target == null)
                return this.Copy();

            try
            {
                TypeDescriptorPermission operand = (TypeDescriptorPermission)target;
                TypeDescriptorPermissionFlags flag_union = m_flags | operand.m_flags;
                if (flag_union == TypeDescriptorPermissionFlags.NoFlags)
                    return null;
                else
                    return new TypeDescriptorPermission(flag_union);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Argument_WrongType), this.GetType().FullName));
            }
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
                return m_flags == TypeDescriptorPermissionFlags.NoFlags;

            try
            {
                TypeDescriptorPermission operand = (TypeDescriptorPermission)target;
                TypeDescriptorPermissionFlags sourceFlag = this.m_flags;
                TypeDescriptorPermissionFlags targetFlag = operand.m_flags;
                return ((sourceFlag & targetFlag) == sourceFlag);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Argument_WrongType), this.GetType().FullName));
            }
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
                return null;

            try
            {
                TypeDescriptorPermission operand = (TypeDescriptorPermission)target;
                TypeDescriptorPermissionFlags flag_intersect = operand.m_flags & this.m_flags;
                if (flag_intersect == TypeDescriptorPermissionFlags.NoFlags)
                    return null;
                else
                    return new TypeDescriptorPermission(flag_intersect);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Argument_WrongType), this.GetType().FullName));
            }
        }

        public override IPermission Copy()
        {
            return new TypeDescriptorPermission((TypeDescriptorPermissionFlags)m_flags);
        }

        private void VerifyAccess(TypeDescriptorPermissionFlags type)
        {
            if ((type & ~TypeDescriptorPermissionFlags.RestrictedRegistrationAccess) != 0)
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Arg_EnumIllegalVal), (int)type));
            Contract.EndContractBlock();
        }

        public override SecurityElement ToXml()
        {
            SecurityElement securityElement = new SecurityElement("IPermission");
            securityElement.AddAttribute("class", this.GetType().FullName + ", " + this.GetType().Module.Assembly.FullName.Replace('\"', '\''));
            securityElement.AddAttribute("version", "1");
            if (!IsUnrestricted())
                securityElement.AddAttribute("Flags", m_flags.ToString());
            else
                securityElement.AddAttribute("Unrestricted", "true");

            return securityElement;
        }

        public override void FromXml(SecurityElement securityElement)
        {
            if (securityElement == null)
                throw new ArgumentNullException("securityElement");

            string className = securityElement.Attribute("class");
            if (className == null || className.IndexOf(this.GetType().FullName, StringComparison.Ordinal) == -1)
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidClassAttribute), "securityElement");

            string unrestricted = securityElement.Attribute("Unrestricted");
            if (unrestricted != null && String.Compare(unrestricted, "true", StringComparison.OrdinalIgnoreCase) == 0)
            {
                m_flags = TypeDescriptorPermissionFlags.RestrictedRegistrationAccess;
                return;
            }

            m_flags = TypeDescriptorPermissionFlags.NoFlags;
            String strFlags = securityElement.Attribute("Flags");
            if (strFlags != null)
            {
                TypeDescriptorPermissionFlags flags = (TypeDescriptorPermissionFlags)Enum.Parse(typeof(TypeDescriptorPermissionFlags), strFlags);
                VerifyFlags(flags);
                m_flags = flags;
            }
        }

        internal static void VerifyFlags(TypeDescriptorPermissionFlags flags)
        {
            if ((flags & ~TypeDescriptorPermissionFlags.RestrictedRegistrationAccess) != 0)
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Arg_EnumIllegalVal), (int)flags));
        }
    }
}
