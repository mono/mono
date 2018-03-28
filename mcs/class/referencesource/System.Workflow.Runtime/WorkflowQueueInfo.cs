// ****************************************************************************
// Copyright (C) Microsoft Corporation.  All rights reserved.
//

using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Workflow.Runtime;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime.Hosting;

namespace System.Workflow.Runtime
{
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowQueueInfo
    {
        private IComparable _queueName;
        private ICollection _items;
        private ReadOnlyCollection<string> _subscribedActivityNames;

        internal WorkflowQueueInfo(IComparable queueName, ICollection items, ReadOnlyCollection<string> subscribedActivityNames)
        {
            _queueName = queueName;
            _items = items;
            _subscribedActivityNames = subscribedActivityNames;
        }

        public IComparable QueueName { get { return _queueName; } }
        public ICollection Items { get { return _items; } }
        public ReadOnlyCollection<string> SubscribedActivityNames { get { return _subscribedActivityNames; } }
    }
}
