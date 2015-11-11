using System;
using System.Xml.Linq;

	public static class SRCore
	{
		// external/referencesource/System.Runtime.DurableInstancing$ grep -R SRCore. | sed -e "s/.*SRCore.\([a-zA-Z0-9_]*\).*/\\1/" | sort | uniq | sed -e "s/\(.*\)/public const string \1 = \"\1\";/"
public const string AsyncTransactionException = "AsyncTransactionException";
public const string BindLockRequiresCommandFlag = "BindLockRequiresCommandFlag";
public const string BindReclaimedLockException = "BindReclaimedLockException";
public const string BindReclaimSucceeded = "BindReclaimSucceeded";
public const string CannotAcquireLockDefault = "CannotAcquireLockDefault";
public static string CannotAcquireLockSpecific (object arg1) { return "CannotAcquireLockSpecific"; }
public static string CannotAcquireLockSpecificWithOwner (object arg1, object arg2) { return "CannotAcquireLockSpecificWithOwner"; }
public const string CannotCompleteWithKeys = "CannotCompleteWithKeys";
public const string CannotCreateContextWithNullId = "CannotCreateContextWithNullId";
public const string CannotInvokeBindingFromNonBinding = "CannotInvokeBindingFromNonBinding";
public const string CannotInvokeTransactionalFromNonTransactional = "CannotInvokeTransactionalFromNonTransactional";
public const string CannotReplaceTransaction = "CannotReplaceTransaction";
public const string CommandExecutionCannotOverlap = "CommandExecutionCannotOverlap";
public const string CompletedMustNotHaveAssociatedKeys = "CompletedMustNotHaveAssociatedKeys";
public const string ContextAlreadyBoundToInstance = "ContextAlreadyBoundToInstance";
public const string ContextAlreadyBoundToLock = "ContextAlreadyBoundToLock";
public const string ContextAlreadyBoundToOwner = "ContextAlreadyBoundToOwner";
public const string ContextMustBeBoundToInstance = "ContextMustBeBoundToInstance";
public const string ContextMustBeBoundToOwner = "ContextMustBeBoundToOwner";
public const string ContextNotFromThisStore = "ContextNotFromThisStore";
public const string DoNotCompleteTryCommandWithPendingReclaim = "DoNotCompleteTryCommandWithPendingReclaim";
public const string ExecuteMustBeNested = "ExecuteMustBeNested";
public static string GenericInstanceCommand (object arg1) { return "GenericInstanceCommand"; }
public const string GenericInstanceCommandNull = "GenericInstanceCommandNull";
public const string GuidCannotBeEmpty = "GuidCannotBeEmpty";
public const string HandleFreed = "HandleFreed";
public const string HandleFreedBeforeInitialized = "HandleFreedBeforeInitialized";
public static string InitialMetadataCannotBeDeleted (object arg1) { return "InitialMetadataCannotBeDeleted"; }
public const string InstanceCollisionDefault = "InstanceCollisionDefault";
public static string InstanceCollisionSpecific (Guid arg1) { return "InstanceCollisionSpecific"; }
public const string InstanceCompleteDefault = "InstanceCompleteDefault";
public static string InstanceCompleteSpecific (Guid arg1) { return "InstanceCompleteSpecific"; }
public const string InstanceHandleConflictDefault = "InstanceHandleConflictDefault";
public static string InstanceHandleConflictSpecific (Guid arg1) { return "InstanceHandleConflictSpecific"; }
public const string InstanceKeyRequiresValidGuid = "InstanceKeyRequiresValidGuid";
public const string InstanceLockLostDefault = "InstanceLockLostDefault";
public static string InstanceLockLostSpecific (Guid arg1) { return "InstanceLockLostSpecific"; }
public const string InstanceNotReadyDefault = "InstanceNotReadyDefault";
public static string InstanceNotReadySpecific (Guid arg1) { return "InstanceNotReadySpecific"; }
public const string InstanceOperationRequiresInstance = "InstanceOperationRequiresInstance";
public const string InstanceOperationRequiresLock = "InstanceOperationRequiresLock";
public const string InstanceOperationRequiresNotCompleted = "InstanceOperationRequiresNotCompleted";
public const string InstanceOperationRequiresNotUninitialized = "InstanceOperationRequiresNotUninitialized";
public const string InstanceOperationRequiresOwner = "InstanceOperationRequiresOwner";
public const string InstanceOwnerDefault = "InstanceOwnerDefault";
public static string InstanceOwnerSpecific (Guid arg1) { return "InstanceOwnerSpecific"; }
public const string InstanceStoreBoundSameVersionTwice = "InstanceStoreBoundSameVersionTwice";
public const string InvalidInstanceState = "InvalidInstanceState";
public const string InvalidKeyArgument = "InvalidKeyArgument";
public const string InvalidLockToken = "InvalidLockToken";
public const string KeyAlreadyAssociated = "KeyAlreadyAssociated";
public const string KeyAlreadyCompleted = "KeyAlreadyCompleted";
public const string KeyAlreadyUnassociated = "KeyAlreadyUnassociated";
public const string KeyCollisionDefault = "KeyCollisionDefault";
public static string KeyCollisionSpecific (object arg1) { return "KeyCollisionSpecific"; }
public static string KeyCollisionSpecific (object arg1, object arg2, object arg3) { return "KeyCollisionSpecific"; }
public static string KeyCollisionSpecificKeyOnly (object arg1) { return "KeyCollisionSpecificKeyOnly"; }
public const string KeyCompleteDefault = "KeyCompleteDefault";
public static string KeyCompleteSpecific (object arg1) { return "KeyCompleteSpecific"; }
public const string KeyNotAssociated = "KeyNotAssociated";
public const string KeyNotCompleted = "KeyNotCompleted";
public const string KeyNotReadyDefault = "KeyNotReadyDefault";
public static string KeyNotReadySpecific (object arg1) { return "KeyNotReadySpecific"; }
public const string LoadedWriteOnlyValue = "LoadedWriteOnlyValue";
public const string MayBindLockCommandShouldValidateOwner = "MayBindLockCommandShouldValidateOwner";
public const string MetadataCannotContainNullKey =  "MetadataCannotContainNullKey";
public static string MetadataCannotContainNullValue (object arg1) { return "MetadataCannotContainNullValue"; }
public const string MustSetTransactionOnFirstCall = "MustSetTransactionOnFirstCall";
public static string NameCollisionOnCollect (XName arg1, object arg2) { return "NameCollisionOnCollect"; }
public static string NameCollisionOnMap (XName arg1, object arg2) { return "NameCollisionOnMap"; }
public const string OnCancelRequestedThrew = "OnCancelRequestedThrew";
public const string OnFreeInstanceHandleThrew = "OnFreeInstanceHandleThrew";
public static string OutsideInstanceExecutionScope (object arg1) { return "OutsideInstanceExecutionScope"; }
public static string OutsideTransactionalCommand (object arg1) { return "OutsideTransactionalCommand"; }
public const string OwnerBelongsToWrongStore = "OwnerBelongsToWrongStore";
public static string PersistencePipelineAbortThrew (object arg1) { return "PersistencePipelineAbortThrew"; }
public static string ProviderDoesNotSupportCommand (object arg1) { return  "ProviderDoesNotSupportCommand"; }
public const string StoreReportedConflictingLockTokens = "StoreReportedConflictingLockTokens";
public const string TimedOutWaitingForLockResolution = "TimedOutWaitingForLockResolution";
public const string TransactionInDoubtNonHost = "TransactionInDoubtNonHost";
public const string TransactionRolledBackNonHost = "TransactionRolledBackNonHost";
public const string TryCommandCannotExecuteSubCommandsAndReduce = "TryCommandCannotExecuteSubCommandsAndReduce";
public const string UninitializedCannotHaveData = "UninitializedCannotHaveData";
public const string WaitAlreadyInProgress = "WaitAlreadyInProgress";
public static string WaitForEventsTimedOut (TimeSpan arg1) { return "WaitForEventsTimedOut"; }
	}

