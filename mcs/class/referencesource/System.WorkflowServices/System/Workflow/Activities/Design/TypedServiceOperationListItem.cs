//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Workflow.Activities.Design
{

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.ServiceModel;

    [ListItemView(typeof(ServiceOperationViewControl))]
    [ListItemDetailView(typeof(ServiceOperationDetailViewControl))]
    internal class TypedServiceOperationListItem : ServiceOperationListItem
    {
        private TypedOperationInfo operation;

        public TypedServiceOperationListItem()
        {
            this.Operation = new TypedOperationInfo();
        }

        public override String ContractName
        {
            get
            {
                return this.operation.GetContractFullName(null);
            }
        }

        public override string Name
        {
            get
            {
                return operation.Name;
            }
            set
            {
                operation.Name = value;
            }
        }

        public TypedOperationInfo Operation
        {
            get { return operation; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                operation = value;
            }
        }

        public override string ToString()
        {
            string retval = "";
            if (Operation != null)
            {
                retval = Operation.Name;
            }
            return retval;
        }
    }
}
