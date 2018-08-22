using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Net;
	
namespace Android.Runtime {
	public static class AndroidEnvironment
	{
		// This is invoked by
		// mscorlib.dll!System.AndroidPlatform.GetDefaultSyncContext()
		// DO NOT REMOVE
		static SynchronizationContext GetDefaultSyncContext ()
		{
			return null; //we don't really care
		}

		static bool TrustEvaluateSsl (List <byte[]> certsRawData)
		{
			return true;
		}

		static IWebProxy GetDefaultProxy ()
		{
			return null;
		}

		static byte[] CertStoreLookup (long hash, bool userStore)
		{
			return null;
		}
	}
}
