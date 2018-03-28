// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

//
//  TypeDescriptorPermissionAttribute.cs
//

namespace System.Security.Permissions {
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )] 
    [Serializable()]
    public sealed class TypeDescriptorPermissionAttribute : CodeAccessSecurityAttribute {
        TypeDescriptorPermissionFlags m_flags = TypeDescriptorPermissionFlags.NoFlags;

        public TypeDescriptorPermissionAttribute(SecurityAction action) : base(action) {}

        public TypeDescriptorPermissionFlags Flags {
            get { return m_flags; }
            set { 
                TypeDescriptorPermission.VerifyFlags(value);
                m_flags = value;
            }
        }

        public bool RestrictedRegistrationAccess {
            get { return (m_flags & TypeDescriptorPermissionFlags.RestrictedRegistrationAccess) != 0; }
            set { m_flags = value ? m_flags | TypeDescriptorPermissionFlags.RestrictedRegistrationAccess : m_flags & ~TypeDescriptorPermissionFlags.RestrictedRegistrationAccess; }
        }

        public override IPermission CreatePermission() {
            if (Unrestricted)
                return new TypeDescriptorPermission(PermissionState.Unrestricted);
            else
                return new TypeDescriptorPermission(m_flags);
        }
    }
}
