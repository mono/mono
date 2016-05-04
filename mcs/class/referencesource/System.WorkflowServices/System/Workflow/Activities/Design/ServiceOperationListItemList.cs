//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Workflow.Activities.Design
{
    using System.Runtime;
    using System.ServiceModel;

    class ServiceOperationListItemList : NamedObjectList<ServiceOperationListItem>
    {
        protected override string GeneratedNameFormatResource
        {
            get
            {
                return SR2.GeneratedOperationNameFormat;
            }
        }

        protected override ServiceOperationListItem CreateObject(string name)
        {
            ServiceOperationListItem result = new WorkflowServiceOperationListItem();
            result.Name = name;
            return result;
        }

        protected override string GetName(ServiceOperationListItem obj)
        {
            Fx.Assert(obj != null, "Null object passed to ServiceOperationListItemList.GetName()");
            return obj.Name;
        }
    }
}
