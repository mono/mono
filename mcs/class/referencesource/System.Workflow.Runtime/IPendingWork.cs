using System;
using System.Collections;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Diagnostics;
using System.Transactions;

namespace System.Workflow.Runtime
{
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface IPendingWork
    {
        /// <summary>
        /// Allows pending work members to assert that they need to commit.  
        /// This is used to eliminate unnecessary commits.
        /// </summary>
        /// <param name="items">Items belonging to this pending work member</param>
        /// <returns>true if a Commit is required; false if not</returns>
        bool MustCommit(ICollection items);
        /// <summary>
        /// Commmit the work items using the transaction
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="items"></param>
        void Commit(System.Transactions.Transaction transaction, ICollection items);

        /// <summary>
        /// Perform necesssary cleanup. Called when the scope
        /// has finished processing this batch of work items
        /// </summary>
        /// <param name="succeeded"></param>
        /// <param name="items"></param>
        void Complete(bool succeeded, ICollection items);
    }
}
