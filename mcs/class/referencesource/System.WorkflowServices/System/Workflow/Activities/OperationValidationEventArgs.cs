//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System.IdentityModel.Claims;
    using System.Collections.ObjectModel;

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class OperationValidationEventArgs : EventArgs
    {
        private ReadOnlyCollection<ClaimSet> claimSets;
        private bool isValid = true;

        public OperationValidationEventArgs(ReadOnlyCollection<ClaimSet> claimSets)
        {
            this.claimSets = claimSets;
        }

        public ReadOnlyCollection<ClaimSet> ClaimSets
        {
            get { return claimSets; }
        }

        public bool IsValid
        {
            get { return isValid; }
            set { isValid = value; }
        }
    }
}
