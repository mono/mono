//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;

    // NOTE: This may get generated from 'xd.xml' if we get extra performance from XML Dictionary strings,
    // which would entail elevating the most common strings into a "Main" dictionary
    static class XD
    {
        public static class Runtime
        {
            // commonly used pieces of data
            public const string Namespace = "http://schemas.datacontract.org/2010/02/System.Activities";
            public const string BookmarkManager = "BookmarkManager";
            public const string ActivityInstanceMap = "InstanceMap";
            public const string Scheduler = "Scheduler";
        }

        public static class WorkflowApplication
        {
            public const string InstanceState = "WFApplication";
        }

        public static class ActivityInstance
        {
            // commonly used pieces of data
            public const string Name = "ActivityInstance";
            public const string Children = "children";
            public const string Owner = "owner";

            // Extended Data
            public const string PropertyManager = "propertyManager";
            public const string BlockingBookmarkCount = "blockingBookmarkCount";
            public const string WaitingForTransactionContext = "waitingForTransactionContext";
            public const string FaultBookmark = "faultBookmark";
            public const string Bookmarks = "bookmarks";
            public const string ActivityReferences = "activityReferences";
        }

        public static class Executor
        {
            public const string Name = "Executor";
            public const string BookmarkManager = "bookmarkMgr";
            public const string RootInstance = "rootInstance";
            public const string RootEnvironment = "rootEnvironment";
            public const string SchedulerMember = "scheduler";
            public const string ActivityInstanceMap = "activities";
            public const string LastInstanceId = "lastInstanceId";
            public const string NextTrackingRecordNumber = "nextTrackingRecordNumber";
            public const string ExecutionState = "state";

            // extended data
            public const string BookmarkScopeManager = "bookmarkScopeManager";
            public const string IsolationBlockWaiters = "isolationBlockWaiters";
            public const string TransactionContextWaiters = "transactionContextWaiters";
            public const string PersistenceWaiters = "persistenceWaiters";
            public const string SecondaryRootInstances = "secondaryRootInstances";
            public const string MappableObjectManager = "mappableObjectManager";
            public const string CompletionException = "completionException";
            public const string ShouldRaiseMainBodyComplete = "shouldRaiseMainBodyComplete";
            public const string ExtensionParticipantObjects = "extensionParticipantObjects";
            public const string WorkflowOutputs = "workflowOutputs";
            public const string MainRootCompleteBookmark = "mainRootCompleteBookmark";
        }

        public static class CompiledLocation
        {
            public const string Name = "CompiledLocation";
        }
    }
}


