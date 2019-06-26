using System.Runtime.Remoting.Messaging;
using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Contexts
{
	[StructLayout (LayoutKind.Sequential)]
	public sealed class Context
	{
#region Keep this code, it is used by the runtime
#pragma warning disable 169, 414
		int domain_id;
		int context_id;
		UIntPtr static_data; /* GC-tracked */
		UIntPtr data;

		[ContextStatic]
		static object[] local_slots;
#pragma warning restore 169, 414

		internal bool NeedsContextSink => throw new PlatformNotSupportedException ();
#endregion

		Context ()
		{
			throw new PlatformNotSupportedException ();
		}
	}
}
