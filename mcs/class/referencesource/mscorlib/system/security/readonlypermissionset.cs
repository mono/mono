// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
//

using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace System.Security
{
    /// <summary>
    ///     Read only permission sets are created from explicit XML and cannot be modified after creation time.
    ///     This allows us to round trip the permission set to the same XML that it was originally created
    ///     from - which allows permission sets to be created from XML representing a permission set in a
    ///     previous version of the framework to be deserialized on the current version while still
    ///     serializing back to XML that makes sense on the original framework version.
    ///     
    ///     Note that while we protect against modifications of the permission set itself (such as adding or
    ///     removing permissions), we do not make any attempt to guard against modification to the permissions
    ///     which are members of the set.  Permission accesor APIs always return a copy of the permission in
    ///     question, although it may be mutable depending upon the permission class.  If it is mutable, users
    ///     will only be modifing a copy of the permission, and not modifying the state of the
    ///     ReadOnlyPermissionSet.
    /// </summary>
    [Serializable]
    public sealed class ReadOnlyPermissionSet : PermissionSet
    {
        private SecurityElement m_originXml;

        [NonSerialized]
        private bool m_deserializing;

        public ReadOnlyPermissionSet(SecurityElement permissionSetXml)
        {
            if (permissionSetXml == null)
                throw new ArgumentNullException("permissionSetXml");

            m_originXml = permissionSetXml.Copy();
            base.FromXml(m_originXml);
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext ctx)
        {
            m_deserializing = true;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            m_deserializing = false;
        }

        public override bool IsReadOnly
        {
            get { return true; }
        }

        public override PermissionSet Copy()
        {
            return new ReadOnlyPermissionSet(m_originXml);
        }

        public override SecurityElement ToXml()
        {
            return m_originXml.Copy();
        }

        //
        // Permission access methods - since modification to a permission would result in modifying the
        // underlying permission set, we always ensure that a copy of the permission is returned rather than
        // the permission itself.
        //

        protected override IEnumerator GetEnumeratorImpl()
        {
            return new ReadOnlyPermissionSetEnumerator(base.GetEnumeratorImpl());
        }

        protected override IPermission GetPermissionImpl(Type permClass)
        {
            IPermission permission = base.GetPermissionImpl(permClass);
            return permission != null ? permission.Copy() : null;
        }

        //
        // Permission set mutation methods - all of these simply reject the attempt to modify the permission
        // set by throwing an InvalidOperationException
        //

        protected override IPermission AddPermissionImpl(IPermission perm)
        {
            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ModifyROPermSet"));
        }

        public override void FromXml(SecurityElement et)
        {
            // PermissionSet uses FromXml when it deserializes itself - so if we're deserializing, let
            // the base type recreate its state, otherwise it is invalid to modify a read only permission set
            // with a FromXml call.
            if (m_deserializing)
            {
                base.FromXml(et);
            }
            else
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ModifyROPermSet"));
            }
        }

        protected override IPermission RemovePermissionImpl(Type permClass)
        {
            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ModifyROPermSet"));
        }

        protected override IPermission SetPermissionImpl(IPermission perm)
        {
            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ModifyROPermSet"));
        }
    }

    /// <summary>
    ///     Class to enumerate permissions of a read only permission set - returning only copies of the
    ///     permissions in the underlying permission set.
    /// </summary>
    internal sealed class ReadOnlyPermissionSetEnumerator : IEnumerator
    {
        private IEnumerator m_permissionSetEnumerator;

        internal ReadOnlyPermissionSetEnumerator(IEnumerator permissionSetEnumerator)
        {
            Contract.Assert(permissionSetEnumerator != null);
            m_permissionSetEnumerator = permissionSetEnumerator;
        }

        public object Current
        {
            get
            {
                IPermission currentPermission = m_permissionSetEnumerator.Current as IPermission;
                return currentPermission != null ? currentPermission.Copy() : null;
            }
        }

        public bool MoveNext()
        {
            return m_permissionSetEnumerator.MoveNext();
        }

        public void Reset()
        {
            m_permissionSetEnumerator.Reset();
        }
    }
}
