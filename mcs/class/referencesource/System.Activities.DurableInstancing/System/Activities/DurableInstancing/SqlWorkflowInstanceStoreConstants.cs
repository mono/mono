//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Runtime.DurableInstancing;
    using System.Xml.Linq;

    static class SqlWorkflowInstanceStoreConstants
    {
        public static readonly TimeSpan MaxHostLockRenewalPulseInterval = TimeSpan.FromSeconds(30);        
        public static readonly TimeSpan DefaultTaskTimeout = TimeSpan.FromSeconds(30);
        public static readonly TimeSpan LockOwnerTimeoutBuffer = TimeSpan.FromSeconds(30);
        public static readonly XNamespace WorkflowNamespace = XNamespace.Get("urn:schemas-microsoft-com:System.Activities/4.0/properties");
        public static readonly XNamespace DurableInstancingNamespace = XNamespace.Get("urn:schemas-microsoft-com:System.ServiceModel.Activities.DurableInstancing/SqlWorkflowInstanceStore");
        public static readonly XName LastUpdatePropertyName = WorkflowNamespace.GetName("LastUpdate");
        public static readonly XName PendingTimerExpirationPropertyName = WorkflowNamespace.GetName("TimerExpirationTime");
        public static readonly XName BinaryBlockingBookmarksPropertyName = WorkflowNamespace.GetName("Bookmarks");
        public static readonly XName StatusPropertyName = WorkflowNamespace.GetName("Status");
        public static readonly string MachineName = Environment.MachineName;
        public const string DefaultSchema = "[System.Activities.DurableInstancing]";
        public const InstanceCompletionAction DefaultInstanceCompletionAction = InstanceCompletionAction.DeleteAll;
        public const InstanceEncodingOption DefaultInstanceEncodingOption = InstanceEncodingOption.GZip;
        public const InstanceLockedExceptionAction DefaultInstanceLockedExceptionAction = InstanceLockedExceptionAction.NoRetry;
        public const string ExecutingStatusPropertyValue = "Executing";
        public const int DefaultStringBuilderCapacity = 512;
        public const int MaximumStringLengthSupported = 450;
        public const int MaximumPropertiesPerPromotion = 32;
    };
}
