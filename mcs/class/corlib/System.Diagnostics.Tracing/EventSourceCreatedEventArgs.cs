namespace System.Diagnostics.Tracing
{
	public class EventSourceCreatedEventArgs : EventArgs
	{
		public EventSource EventSource
		{
			get;
			internal set;
		}
	}
}