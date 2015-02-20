#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.Security.Permissions;
using System.Security.Principal;
using System.Runtime.Serialization;

using System.Workflow.ComponentModel;
using System.Diagnostics;

#endregion

namespace System.Workflow.Activities
{
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class WorkflowRole
    {
        public abstract String Name { set; get; }

        public abstract IList<String> GetIdentities();

        public abstract bool IncludesIdentity(String identity);
    }

    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    sealed public class WorkflowRoleCollection : List<WorkflowRole>
    {
        public WorkflowRoleCollection()
            : base()
        {
        }

        public bool IncludesIdentity(String identity)
        {
            if (identity == null)
                return false;

            foreach (WorkflowRole role in this)
            {
                if (role != null)
                {
                    if (role.IncludesIdentity(identity))
                        return true;
                }
            }

            return false;
        }
    }

    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    sealed public class ActiveDirectoryRole : WorkflowRole, ISerializable, IDisposable
    {
        private String m_name;
        private DirectoryEntry m_root;
        private List<IDirectoryOperation> m_operations;

        internal ActiveDirectoryRole(DirectoryEntry rootEntry, IDirectoryOperation operation)
        {
            if (rootEntry == null)
                throw new ArgumentNullException("rootEntry");

            this.m_root = rootEntry;

            this.m_operations = new List<IDirectoryOperation>();
            if (operation != null)
                this.m_operations.Add(operation);
        }

        internal ActiveDirectoryRole(DirectoryEntry rootEntry, ICollection<IDirectoryOperation> operations)
        {
            if (rootEntry == null)
                throw new ArgumentNullException("rootEntry");

            this.m_root = rootEntry;

            if (operations == null)
                this.m_operations = new List<IDirectoryOperation>();
            else
                this.m_operations = new List<IDirectoryOperation>(operations);
        }

        private ActiveDirectoryRole(SerializationInfo info, StreamingContext context)
        {
            this.m_name = info.GetString("m_name");
            this.m_operations = (List<IDirectoryOperation>)info.GetValue("m_operations", typeof(List<IDirectoryOperation>));

            String path = info.GetString("m_root\\path");

            this.m_root = new DirectoryEntry(path);
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("m_name", this.m_name);
            info.AddValue("m_operations", this.m_operations);

            info.AddValue("m_root\\path", this.m_root.Path);
        }

        public override String Name
        {
            get
            {
                return this.m_name;
            }

            set
            {
                this.m_name = value;
            }
        }

        public DirectoryEntry RootEntry
        {
            get
            {
                return this.m_root;
            }
        }

        internal ICollection<IDirectoryOperation> Operations
        {
            get
            {
                return this.m_operations;
            }
        }

        public ActiveDirectoryRole GetManager()
        {
            List<IDirectoryOperation> queries = new List<IDirectoryOperation>(this.Operations);
            queries.Add(new DirectoryRedirect(ActiveDirectoryRoleFactory.Configuration.DistinguishedName, ActiveDirectoryRoleFactory.Configuration.DirectReports));

            return new ActiveDirectoryRole(this.RootEntry, queries);
        }

        public ActiveDirectoryRole GetManagerialChain()
        {
            List<IDirectoryOperation> queries = new List<IDirectoryOperation>(this.Operations);
            queries.Add(new DirectoryRedirect(ActiveDirectoryRoleFactory.Configuration.DistinguishedName, ActiveDirectoryRoleFactory.Configuration.DirectReports, true));

            return new ActiveDirectoryRole(this.RootEntry, queries);
        }

        public ActiveDirectoryRole GetDirectReports()
        {
            List<IDirectoryOperation> queries = new List<IDirectoryOperation>(this.Operations);
            queries.Add(new DirectoryRedirect(ActiveDirectoryRoleFactory.Configuration.DistinguishedName, ActiveDirectoryRoleFactory.Configuration.Manager));

            return new ActiveDirectoryRole(this.RootEntry, queries);
        }

        public ActiveDirectoryRole GetAllReports()
        {
            List<IDirectoryOperation> queries = new List<IDirectoryOperation>(this.Operations);
            queries.Add(new DirectoryRedirect(ActiveDirectoryRoleFactory.Configuration.DistinguishedName, ActiveDirectoryRoleFactory.Configuration.Manager, true));

            return new ActiveDirectoryRole(this.RootEntry, queries);
        }

        public ActiveDirectoryRole GetPeers()
        {
            ICollection<DirectoryEntry> entries = this.GetEntries();

            List<IDirectoryOperation> queries = new List<IDirectoryOperation>(this.Operations);
            queries.Add(new DirectoryRedirect(ActiveDirectoryRoleFactory.Configuration.DistinguishedName, ActiveDirectoryRoleFactory.Configuration.DirectReports));
            queries.Add(new DirectoryRedirect(ActiveDirectoryRoleFactory.Configuration.DistinguishedName, ActiveDirectoryRoleFactory.Configuration.Manager));

            foreach (DirectoryEntry entry in entries)
            {
                queries.Add(new DirectoryLocalQuery(ActiveDirectoryRoleFactory.Configuration.DistinguishedName, (String)entry.Properties[ActiveDirectoryRoleFactory.Configuration.DistinguishedName][0], DirectoryQueryOperation.NotEqual));
            }

            return new ActiveDirectoryRole(this.RootEntry, queries);
        }


        public ICollection<DirectoryEntry> GetEntries()
        {
            List<DirectoryEntry> currentEntries = new List<DirectoryEntry>();
            currentEntries.Add(this.m_root);
            List<DirectoryEntry> newEntries = new List<DirectoryEntry>();

            for (int i = 0; i < this.m_operations.Count; ++i)
            {
                for (int j = 0; j < currentEntries.Count; ++j)
                {
                    this.m_operations[i].GetResult(this.m_root, currentEntries[j], newEntries);
                }

                // Swap between new and current, as the for the new iteration the 'new' of
                // now will be the current.  After the swap we clear out the 'new' list as to
                // reuse it.

                List<DirectoryEntry> tempEntries = currentEntries;
                currentEntries = newEntries;
                newEntries = tempEntries;
                newEntries.Clear();
            }

            // Remove duplicates

            Dictionary<Guid, DirectoryEntry> dFinal = new Dictionary<Guid, DirectoryEntry>();
            for (int i = 0; i < currentEntries.Count; ++i)
            {
                if (!dFinal.ContainsKey(currentEntries[i].Guid))
                    dFinal.Add(currentEntries[i].Guid, currentEntries[i]);
            }

            return dFinal.Values;
        }

        public IList<SecurityIdentifier> GetSecurityIdentifiers()
        {
            List<SecurityIdentifier> identifiers = new List<SecurityIdentifier>();

            foreach (DirectoryEntry entry in this.GetEntries())
            {
                if (entry.Properties["objectSid"] != null &&
                    entry.Properties["objectSid"].Count != 0)
                {
                    identifiers.Add(new SecurityIdentifier((byte[])(entry.Properties["objectSid"][0]), 0));
                }
                else
                {
                    WorkflowActivityTrace.Activity.TraceEvent(TraceEventType.Information, 0, "Unable to find 'objectSid' property for directory entry = {0}.", entry.Path);
                }
            }

            return identifiers;
        }

        public override IList<String> GetIdentities()
        {
            List<String> identityRefs = new List<String>();
            foreach (SecurityIdentifier entrySid in this.GetSecurityIdentifiers())
            {
                identityRefs.Add(entrySid.Translate(typeof(NTAccount)).ToString());
            }
            return identityRefs;
        }

        public override bool IncludesIdentity(String identity)
        {
            if (identity == null)
                return false;

            foreach (String roleIdentity in this.GetIdentities())
            {
                if (String.Compare(identity, roleIdentity, StringComparison.Ordinal) == 0)
                    return true;
            }

            return false;
        }

        void IDisposable.Dispose()
        {
            this.m_root.Dispose();
        }

    }
}
