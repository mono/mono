// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

//
//  StorePermissionAttribute.cs
//

namespace System.Security.Permissions {
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )] 
    [Serializable()]
    public sealed class StorePermissionAttribute : CodeAccessSecurityAttribute {
        StorePermissionFlags m_flags = StorePermissionFlags.NoFlags;

        public StorePermissionAttribute(SecurityAction action) : base(action) {}

        public StorePermissionFlags Flags {
            get { return m_flags; }
            set { 
                StorePermission.VerifyFlags(value);
                m_flags = value;
            }
        }

        public bool CreateStore {
            get { return (m_flags & StorePermissionFlags.CreateStore) != 0; }
            set { m_flags = value ? m_flags | StorePermissionFlags.CreateStore : m_flags & ~StorePermissionFlags.CreateStore; }
        }

        public bool DeleteStore {
            get { return (m_flags & StorePermissionFlags.DeleteStore) != 0; }
            set { m_flags = value ? m_flags | StorePermissionFlags.DeleteStore : m_flags & ~StorePermissionFlags.DeleteStore; }
        }

        public bool EnumerateStores {
            get { return (m_flags & StorePermissionFlags.EnumerateStores) != 0; }
            set { m_flags = value ? m_flags | StorePermissionFlags.EnumerateStores : m_flags & ~StorePermissionFlags.EnumerateStores; }
        }

        public bool OpenStore {
            get { return (m_flags & StorePermissionFlags.OpenStore) != 0; }
            set { m_flags = value ? m_flags | StorePermissionFlags.OpenStore : m_flags & ~StorePermissionFlags.OpenStore; }
        }

        public bool AddToStore {
            get { return (m_flags & StorePermissionFlags.AddToStore) != 0; }
            set { m_flags = value ? m_flags | StorePermissionFlags.AddToStore : m_flags & ~StorePermissionFlags.AddToStore; }
        }

        public bool RemoveFromStore {
            get { return (m_flags & StorePermissionFlags.RemoveFromStore) != 0; }
            set { m_flags = value ? m_flags | StorePermissionFlags.RemoveFromStore : m_flags & ~StorePermissionFlags.RemoveFromStore; }
        }

        public bool EnumerateCertificates {
            get { return (m_flags & StorePermissionFlags.EnumerateCertificates) != 0; }
            set { m_flags = value ? m_flags | StorePermissionFlags.EnumerateCertificates : m_flags & ~StorePermissionFlags.EnumerateCertificates; }
        }

        public override IPermission CreatePermission() {
            if (Unrestricted)
                return new StorePermission(PermissionState.Unrestricted);
            else
                return new StorePermission(m_flags);
        }
    }
}
