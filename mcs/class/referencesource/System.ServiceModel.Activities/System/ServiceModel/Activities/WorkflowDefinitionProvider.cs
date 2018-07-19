//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Activities.Debugger;
    using System.Activities.DynamicUpdate;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Description;
    using System.ServiceModel.XamlIntegration;
    using System.Windows.Markup;
    using System.Xml;
    using System.Xml.Linq;
        
    class WorkflowDefinitionProvider
    {
        IDictionary<WorkflowIdentityKey, WorkflowService> definitionCollection;        
        WorkflowService defaultWorkflowService;
        WorkflowServiceVersionsCollection supportedVersions;
        WorkflowServiceHost wfsh;

        public WorkflowDefinitionProvider(WorkflowService workflowService, WorkflowServiceHost wfsh)
        {
            Fx.Assert(workflowService != null, "workflowService cannot be null!");
            Fx.Assert(wfsh != null, "wfsh cannot be null!");

            this.wfsh = wfsh;
            this.defaultWorkflowService = workflowService;
            this.definitionCollection = new Dictionary<WorkflowIdentityKey, WorkflowService>();
            this.supportedVersions = new WorkflowServiceVersionsCollection(this);
        }

        public ICollection<WorkflowService> SupportedVersions
        {
            get
            {
                return this.supportedVersions;
            }
        }

        public WorkflowIdentity DefaultDefinitionIdentity
        {
            get
            {
                return this.defaultWorkflowService.DefinitionIdentity;
            }
        }

        public void GetDefinitionIdentityMetadata(IDictionary<XName, InstanceValue> metadataCollection)
        {
            if (metadataCollection == null)
            {
                throw FxTrace.Exception.ArgumentNull("metaDataCollection");
            }
            if (!metadataCollection.ContainsKey(Workflow45Namespace.DefinitionIdentities))
            {
                Collection<WorkflowIdentity> identityCollection = new Collection<WorkflowIdentity>();
                identityCollection.Add(this.DefaultDefinitionIdentity);
                foreach (WorkflowIdentityKey identityKey in this.definitionCollection.Keys)
                {
                    identityCollection.Add(identityKey.Identity);
                }
                if (identityCollection.Count > 1 || (identityCollection.Count == 1 && identityCollection[0] != null))
                {
                    metadataCollection.Add(Workflow45Namespace.DefinitionIdentities, new InstanceValue(identityCollection));
                }
            }
        }

        void AddDefinition(WorkflowService workflowService)
        {
            if (workflowService == null)
            {
                throw FxTrace.Exception.ArgumentNull("workflowService");
            }

            WorkflowIdentityKey identityKey = new WorkflowIdentityKey(workflowService.DefinitionIdentity);
            if (object.Equals(this.DefaultDefinitionIdentity, identityKey.Identity) || this.definitionCollection.ContainsKey(identityKey))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.DuplicateDefinitionIdentity(identityKey.Identity == null ? "null" : identityKey.Identity.ToString())));
            }

            if (workflowService.Name != this.defaultWorkflowService.Name)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.DifferentWorkflowServiceNameNotSupported(workflowService.Name, this.defaultWorkflowService.Name)));
            }

            this.ThrowIfNotConfigurable();

            workflowService.ValidateForVersioning(this.defaultWorkflowService);
            this.definitionCollection.Add(identityKey, workflowService);
        }

        bool Remove(WorkflowService workflowService)
        {
            this.ThrowIfNotConfigurable();

            if (this.definitionCollection.Values.Contains(workflowService))
            {
                workflowService.DetachFromVersioning(this.defaultWorkflowService);
                return this.definitionCollection.Remove(new WorkflowIdentityKey(workflowService.DefinitionIdentity));
            }
            return false;
        }

        void Clear()
        {
            this.ThrowIfNotConfigurable();

            foreach (WorkflowService workflowService in this.definitionCollection.Values)
            {
                workflowService.DetachFromVersioning(this.defaultWorkflowService);
            }

            this.definitionCollection.Clear();
        }

        void ThrowIfNotConfigurable()
        {
            if (!this.wfsh.IsConfigurable)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.WorkflowServiceHostCannotAddOrRemoveServiceDefinitionAfterOpen));
            }
        }

        public bool TryGetDefinition(WorkflowIdentity workflowIdentity, out Activity workflowDefinition)
        {
            workflowDefinition = null;
            WorkflowService workflowService;
            bool found = false;
            if (object.Equals(workflowIdentity, this.DefaultDefinitionIdentity))
            {
                workflowDefinition = this.defaultWorkflowService.Body;
                found = true;
            }
            else if (this.definitionCollection.TryGetValue(new WorkflowIdentityKey(workflowIdentity), out workflowService))
            {
                workflowDefinition = workflowService.Body;
                found = true;
            }
            return found;
        }

        public bool TryGetDefinitionAndMap(WorkflowIdentity currentIdentity, WorkflowIdentity updatedIdentity, out Activity workflowDefinition, out DynamicUpdateMap updateMap)
        {
            WorkflowService workflowService;
            if (object.Equals(updatedIdentity, this.DefaultDefinitionIdentity))
            {
                workflowService = this.defaultWorkflowService;
            }
            else
            {
                this.definitionCollection.TryGetValue(new WorkflowIdentityKey(updatedIdentity), out workflowService);
            }

            if (workflowService != null && workflowService.UpdateMaps.TryGetValue(currentIdentity, out updateMap) && updateMap != null)
            {
                workflowDefinition = workflowService.Body;
                return true;
            }

            workflowDefinition = null;
            updateMap = null;
            return false;
        }

        class WorkflowServiceVersionsCollection : ICollection<WorkflowService>
        {
            WorkflowDefinitionProvider workflowDefinitionProvider;

            public WorkflowServiceVersionsCollection(WorkflowDefinitionProvider workflowDefinitionProvider)
            {
                this.workflowDefinitionProvider = workflowDefinitionProvider;
            }

            public void Add(WorkflowService item)
            {
                this.workflowDefinitionProvider.AddDefinition(item);
            }

            public void Clear()
            {
                this.workflowDefinitionProvider.Clear();
            }

            public bool Contains(WorkflowService item)
            {
                return this.workflowDefinitionProvider.definitionCollection.Values.Contains(item);
            }

            public void CopyTo(WorkflowService[] array, int arrayIndex)
            {
                this.workflowDefinitionProvider.definitionCollection.Values.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get
                {
                    return this.workflowDefinitionProvider.definitionCollection.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public bool Remove(WorkflowService workflowService)
            {
                return this.workflowDefinitionProvider.Remove(workflowService);
            }

            public IEnumerator<WorkflowService> GetEnumerator()
            {
                return this.workflowDefinitionProvider.definitionCollection.Values.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.workflowDefinitionProvider.definitionCollection.Values.GetEnumerator();
            }
        }


    }
}
