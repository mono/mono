#pragma warning disable 1634, 1691
using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Transactions;
using SES = System.EnterpriseServices;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime.Hosting;

namespace System.Workflow.Runtime
{
    #region TransactionalProperties class

    internal sealed class TransactionalProperties
    {
        internal TransactionProcessState TransactionState;
        internal System.Transactions.Transaction Transaction;
        internal TransactionScope TransactionScope;
        internal List<SchedulableItem> ItemsToBeScheduledAtCompletion;
        internal WorkflowQueuingService LocalQueuingService;
    }

    internal enum TransactionProcessState
    {
        Ok,
        Aborted,
        AbortProcessed
    }

    #endregion TransactionalProperties class
}