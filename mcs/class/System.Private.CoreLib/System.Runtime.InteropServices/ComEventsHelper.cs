namespace System.Runtime.InteropServices
{
	public static class ComEventsHelper
	{
		public static void Combine (object rcw, Guid iid, int dispid, System.Delegate d) => throw new PlatformNotSupportedException ();

		public static Delegate Remove (object rcw, Guid iid, int dispid, System.Delegate d) => throw new PlatformNotSupportedException ();
	}
}