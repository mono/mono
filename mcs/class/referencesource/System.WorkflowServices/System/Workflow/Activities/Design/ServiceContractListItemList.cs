//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Workflow.Activities.Design
{
    using System.Runtime;
    using System.ServiceModel;
    using System.Windows.Forms;

    class ServiceContractListItemList : NamedObjectList<ServiceContractListItem>
    {
        ListBox container;

        public ServiceContractListItemList(ListBox container)
        {
            Fx.Assert(container != null, "Null container passed to ServiceContractListItemList.");
            this.container = container;
        }

        protected override string GeneratedNameFormatResource
        {
            get
            {
                return SR2.GeneratedContractNameFormat;
            }
        }

        protected override ServiceContractListItem CreateObject(string name)
        {
            ServiceContractListItem result = new ServiceContractListItem(this.container);
            result.Name = name;
            return result;
        }

        protected override string GetName(ServiceContractListItem obj)
        {
            Fx.Assert(obj != null, "Null object passed to ServiceContractListItemList.GetName()");
            return obj.Name;
        }
    }
}
