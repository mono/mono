namespace System.Diagnostics.Tracing
{
	partial class EventSource
	{
		private static readonly bool m_EventSourcePreventRecursion = false;

		public static void SetCurrentThreadActivityId (Guid activityId)
		{
		}

		public static void SetCurrentThreadActivityId (Guid activityId, out Guid oldActivityThatWillContinue)
		{
			throw new NotImplementedException ();
		}

		public static Guid CurrentThreadActivityId {
			get {
				throw new NotImplementedException ();
			}
		}

		private int GetParameterCount (EventMetadata eventData)
		{
			return 0;
		}

		private Type GetDataType (EventMetadata eventData, int parameterId)
		{
			return null;
		}
	}

	internal partial class ManifestBuilder
	{
		private string GetTypeNameHelper (Type type)
		{
			throw new NotImplementedException ();
		}
	}
}