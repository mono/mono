//------------------------------------------------------------------------------
// <copyright file="RoleGroupCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.Collections;
    using System.ComponentModel;
    using System.Security.Principal;
    using System.Drawing.Design;
    using System.Web.Security;


    /// <devdoc>
    /// Collection of RoleGroups.
    /// </devdoc>
    [
    Editor("System.Web.UI.Design.WebControls.RoleGroupCollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))
    ]
    public sealed class RoleGroupCollection : CollectionBase {


        public RoleGroup this[int index] {
            get {
                return (RoleGroup)List[index];
            }
        }


        public void Add(RoleGroup group) {
            List.Add(group);
        }


        public void CopyTo(RoleGroup[] array, int index) {
            List.CopyTo(array, index);
        }


        public bool Contains(RoleGroup group) {
            return List.Contains(group);
        }


        /// <devdoc>
        /// The first RoleGroup that contains the user.
        /// </devdoc>
        public RoleGroup GetMatchingRoleGroup(IPrincipal user) {
            int index = GetMatchingRoleGroupInternal(user);
            if (index != -1) {
                return this[index];
            }
            return null;
        }

        /// <devdoc>
        /// Index of the first RoleGroup that contains the user.  Internal because called from LoginView.
        /// </devdoc>
        internal int GetMatchingRoleGroupInternal(IPrincipal user) {
            if (user == null) {
                throw new ArgumentNullException("user");
            }
            int i = 0;
            foreach (RoleGroup group in this) {
                if (group.ContainsUser(user)) {
                    return i;
                }
                i++;
            }
            return -1;
        }


        public int IndexOf(RoleGroup group) {
            return List.IndexOf(group);
        }


        public void Insert(int index, RoleGroup group) {
            List.Insert(index, group);
        }


        protected override void OnValidate(object value) {
            base.OnValidate(value);
            if (!(value is RoleGroup)) {
                throw new ArgumentException(SR.GetString(SR.RoleGroupCollection_InvalidType), "value");
            }
        }


        public void Remove(RoleGroup group) {
            int index = IndexOf(group);
            if (index >= 0) {
                List.RemoveAt(index);
            }
        }
    }
}
