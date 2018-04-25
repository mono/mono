using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;

using System.Web.Security;
using System.Security.Principal;
using System.Configuration.Provider;

namespace System.Workflow.Activities
{
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WebWorkflowRole : WorkflowRole
    {
        private string m_roleName;
        private string m_roleProvider;

        public override string Name
        {
            get
            {
                return this.m_roleName;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.m_roleName = value;
            }
        }
        public string RoleProvider
        {
            get
            {
                return this.m_roleProvider;
            }
            set
            {
                this.m_roleProvider = value;
            }
        }

        public WebWorkflowRole(string roleName)
        {
            if (null == roleName)
            {
                throw new ArgumentNullException("roleName");
            }

            this.m_roleName = roleName;
            this.m_roleProvider = null;
        }
        public WebWorkflowRole(string roleName, string provider)
        {
            if (null == roleName)
            {
                throw new ArgumentNullException("roleName");
            }

            this.m_roleName = roleName;
            this.m_roleProvider = provider;
        }

        public override IList<string> GetIdentities()
        {
            List<string> identities = new List<string>();
            System.Web.Security.RoleProvider rp = GetRoleProvider();

            identities.AddRange(rp.GetUsersInRole(Name));

            return identities;
        }

        public override bool IncludesIdentity(string identity)
        {
            System.Web.Security.RoleProvider rp = GetRoleProvider();

            return rp.IsUserInRole(identity, Name);
        }

        private System.Web.Security.RoleProvider GetRoleProvider()
        {
            if (this.RoleProvider == null)
                return System.Web.Security.Roles.Provider;

            RoleProvider rp = Roles.Providers[this.RoleProvider];
            if (rp == null)
                throw new ProviderException(SR.GetString(SR.Error_RoleProviderNotAvailableOrEnabled, this.RoleProvider));
            return rp;
        }
    }
}
