namespace System.Runtime.CompilerServices
{
	partial class RuntimeFeature
	{
#if !DISABLE_REMOTING
		internal static bool IsRemotingSupported => true;
#endif

#if !DISABLE_SECURITY
		internal static bool IsSecuritySupported => true;
#endif
	}
}
