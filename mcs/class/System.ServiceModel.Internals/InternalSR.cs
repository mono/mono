namespace System.Runtime {

	internal static class InternalSR {
		public static string ArgumentNullOrEmpty(string paramName)
		{
			return string.Format ("{0} is null or empty");
		}
		
		public static string AsyncEventArgsCompletedTwice(Type t)
		{
			return string.Format ("AsyncEventArgs completed twice for {0}", t);
		}

		public static string AsyncEventArgsCompletionPending(Type t)
		{
			return string.Format ("AsyncEventArgs completion pending for {0}", t);
		}
		
		public static string BufferAllocationFailed(int size)
		{
			return string.Format ("Buffer allocation of size {0} failed", size);
		}
		
		public static string BufferedOutputStreamQuotaExceeded(int maxSizeQuota)
		{
			return string.Format ("Buffered output stream quota exceeded (maxSizeQuota={0})", maxSizeQuota);
		}

		public static string CannotConvertObject(object source, Type t)
		{
			return string.Format ("Cannot convert object {0} to {1}", source, t);
		}
		
		public static string EtwAPIMaxStringCountExceeded(object max)
		{
			return string.Format ("ETW API max string count exceeded {0}", max);
		}
		
		public static string EtwMaxNumberArgumentsExceeded(object max)
		{
			return string.Format ("ETW max number arguments exceeded {0}", max);
		}
		
		public static string EtwRegistrationFailed(object arg)
		{
			return string.Format ("ETW registration failed {0}", arg);
		}
		
		public static string FailFastMessage(string description)
		{
			return string.Format ("Fail fast: {0}", description);
		}
		
		public static string InvalidAsyncResultImplementation(Type t)
		{
			return string.Format ("Invalid AsyncResult implementation: {0}", t);
		}
		
		public static string LockTimeoutExceptionMessage (object timeout)
		{
			return string.Format ("Lock timeout {0}", timeout);
		}
		
		public static string ShipAssertExceptionMessage(object description)
		{
			return string.Format ("Ship assert exception {0}", description);
		}
		
		public static string TaskTimedOutError (object timeout)
		{
			return string.Format ("Task timed out error {0}", timeout);
		}
		
		public static string TimeoutInputQueueDequeue(object timeout)
		{
			return string.Format ("Timeout input queue dequeue {0}", timeout);
		}
		
		public static string TimeoutMustBeNonNegative(object argumentName, object timeout)
		{
			return string.Format ("Timeout must be non-negative {0} and {1}", argumentName, timeout);
		}
		
		public static string TimeoutMustBePositive(string argumentName, object timeout)
		{
			return string.Format ("Timeout must be positive {0} {1}", argumentName, timeout);
		}
		
		public static string TimeoutOnOperation(object timeout)
		{
			return string.Format ("Timeout on operation {0}", timeout);
		}
		
		public static string AsyncResultCompletedTwice (Type t)
		{
			return string.Format ("AsyncResult Completed Twice for {0}", t);
		}
		
		public const string ActionItemIsAlreadyScheduled = "Action Item Is Already Scheduled";
		public const string AsyncCallbackThrewException = "Async Callback Threw Exception";
		public const string AsyncResultAlreadyEnded = "Async Result Already Ended";
		public const string BadCopyToArray = "Bad Copy To Array";
		public const string BufferIsNotRightSizeForBufferManager = "Buffer Is Not Right Size For Buffer Manager";
		public const string DictionaryIsReadOnly = "Dictionary Is Read Only";
		public const string InvalidAsyncResult = "Invalid Async Result";
		public const string InvalidAsyncResultImplementationGeneric = "Invalid Async Result Implementation Generic";
		public const string InvalidNullAsyncResult = "Invalid Null Async Result";
		public const string InvalidSemaphoreExit = "Invalid Semaphore Exit";
		public const string KeyCollectionUpdatesNotAllowed = "Key Collection Updates Not Allowed";
		public const string KeyNotFoundInDictionary = "Key Not Found In Dictionary";
		public const string MustCancelOldTimer = "Must Cancel Old Timer";
		public const string NullKeyAlreadyPresent = "Null Key Already Present";
		public const string ReadNotSupported = "Read Not Supported";
		public const string SFxTaskNotStarted = "SFx Task Not Started";
		public const string SeekNotSupported = "Seek Not Supported";
		public const string ThreadNeutralSemaphoreAborted = "Thread Neutral Semaphore Aborted";
		public const string ValueCollectionUpdatesNotAllowed = "Value Collection Updates Not Allowed";
		public const string ValueMustBeNonNegative = "Value Must Be Non Negative";
	}
}

