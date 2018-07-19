//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Workflow.Activities.Design
{

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Workflow.ComponentModel;
    using System.Workflow.Activities;
    using System.ComponentModel;

    [ListItemView(typeof(ServiceOperationViewControl))]
    [ListItemDetailView(typeof(ListItemViewControl))]
    internal abstract class ServiceOperationListItem : object
    {

        public CancelEventHandler Validating;
        private List<Activity> implementingActivities;

        protected ServiceOperationListItem()
        {
            this.implementingActivities = new List<Activity>();
        }

        public virtual String ContractName
        {
            get
            {
                return null;
            }
        }

        public List<Activity> ImplementingActivities
        {
            get { return implementingActivities; }
        }

        public abstract string Name
        { get; set; }
    }
}
