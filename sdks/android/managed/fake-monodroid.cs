using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Net;
	
namespace Android.Runtime {
	public static class AndroidEnvironment {

		public const string AndroidLogAppName = "Mono.Android";

		static object lock_ = new object ();

		// This is invoked by
		// System.Core!System.AndroidPlatform.GetDefaultTimeZone ()
		// DO NOT REMOVE
		static string GetDefaultTimeZone ()
		{
			return "America/Los_Angeles"; //add glue code if you really care
		}

		// This is invoked by
		// mscorlib.dll!System.AndroidPlatform.GetDefaultSyncContext()
		// DO NOT REMOVE
		static SynchronizationContext GetDefaultSyncContext ()
		{
			return null; //we don't really care
		}

		// These are invoked by
		// System.dll!System.AndroidPlatform.getifaddrs
		// DO NOT REMOVE
		[DllImport ("__Internal")]
		static extern int monodroid_getifaddrs (out IntPtr ifap);

		static int GetInterfaceAddresses (out IntPtr ifap)
		{
			return monodroid_getifaddrs (out ifap);
		}
		
		// These are invoked by
		// System.dll!System.AndroidPlatform.freeifaddrs
		// DO NOT REMOVE
		[DllImport ("__Internal")] 
		static extern void monodroid_freeifaddrs (IntPtr ifap);

		static void FreeInterfaceAddresses (IntPtr ifap)
		{
			monodroid_freeifaddrs (ifap);
		}

		[DllImport ("__Internal")]
		static extern void _monodroid_detect_cpu_and_architecture (ref ushort built_for_cpu, ref ushort running_on_cpu, ref byte is64bit);

		static void DetectCPUAndArchitecture (out ushort builtForCPU, out ushort runningOnCPU, out bool is64bit)
		{
			ushort built_for_cpu = 0;
			ushort running_on_cpu = 0;
			byte _is64bit = 0;

			_monodroid_detect_cpu_and_architecture (ref built_for_cpu, ref running_on_cpu, ref _is64bit);
			builtForCPU = built_for_cpu;
			runningOnCPU = running_on_cpu;
			is64bit = _is64bit != 0;
		}


		static bool TrustEvaluateSsl (List <byte[]> certsRawData)
		{
			return true;
		}

		static IWebProxy GetDefaultProxy ()
		{
			return null;
		}

		static byte[] CertStoreLookup (long _1, bool _2)
		{
			return null;
		}

	}
}
