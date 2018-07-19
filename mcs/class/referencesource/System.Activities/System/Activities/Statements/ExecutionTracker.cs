//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    sealed class ExecutionTracker
    {
        List<CompensationTokenData> executionOrderedList;

        public ExecutionTracker()
        {
            this.executionOrderedList = new List<CompensationTokenData>();
        }

        public int Count
        {
            get
            {
                return this.executionOrderedList.Count;
            }
        }

        [DataMember(Name = "executionOrderedList")]
        internal List<CompensationTokenData> SerializedExecutionOrderedList
        {
            get { return this.executionOrderedList; }
            set { this.executionOrderedList = value; }
        }

        public void Add(CompensationTokenData compensationToken)
        {
            this.executionOrderedList.Insert(0, compensationToken);
        }

        public void Remove(CompensationTokenData compensationToken)
        {
            this.executionOrderedList.Remove(compensationToken);
        }

        public CompensationTokenData Get()
        {
            if (Count > 0)
            {
                return this.executionOrderedList[0];
            }
            else
            {
                return null;
            }
        }
    }
}
