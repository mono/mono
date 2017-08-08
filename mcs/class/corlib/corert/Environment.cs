using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace System
{
	static partial class Environment
	{
		internal static int CurrentNativeThreadId => throw new NotSupportedException ();
	}
}