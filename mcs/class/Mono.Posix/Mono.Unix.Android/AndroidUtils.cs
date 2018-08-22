using System;
using System.Reflection;
using System.Runtime.CompilerServices;

// using Android.Runtime;
// using Android.Util;

namespace Mono.Unix.Android
{
	internal sealed class AndroidUtils
	{
		const string TAG = "Mono.Posix";

		public static bool AreRealTimeSignalsSafe ()
		{
			DetectCpuAndArchitecture (out ushort built_for_cpu, out ushort running_on_cpu, out bool is64bit);

			// CPUArchitecture builtForCPU = Enum.IsDefined (typeof (CPUArchitecture), built_for_cpu) ? (CPUArchitecture)built_for_cpu : CPUArchitecture.Unknown;
			// CPUArchitecture runningOnCPU = Enum.IsDefined (typeof (CPUArchitecture), running_on_cpu) ? (CPUArchitecture)running_on_cpu : CPUArchitecture.Unknown;

			// Log.Info (TAG, " Built for CPU: {0}", builtForCPU);
			// Log.Info (TAG, "Running on CPU: {0}", runningOnCPU);
			// Log.Info (TAG, "64-bit process: {0}", is64bit);

			// For now real-time signals aren't safe at all, alas
			bool safe = false;
			// Log.Info (TAG, "Real-time signals are {0}safe on this platform", safe ? String.Empty : "not ");
			
			return safe;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void DetectCpuAndArchitecture (out ushort built_for_cpu, out ushort running_on_cpu, out bool is64bit);
	}
}