using System;
using System.Reflection;

// using Android.Runtime;
// using Android.Util;

namespace Mono.Unix.Android
{
	internal sealed class AndroidUtils
	{
		const string TAG = "Mono.Posix";

		delegate void DetectCPUAndArchitecture (out ushort builtForCPU, out ushort runningOnCPU, out bool is64bit);

		readonly static DetectCPUAndArchitecture detectCPUAndArchitecture;

		static AndroidUtils ()
		{
			Type androidRuntime = Type.GetType ("Android.Runtime.AndroidEnvironment, Mono.Android", true);

			MethodInfo mi = androidRuntime.GetMethod ("DetectCPUAndArchitecture", BindingFlags.NonPublic | BindingFlags.Static);
			detectCPUAndArchitecture = (DetectCPUAndArchitecture) Delegate.CreateDelegate (typeof(DetectCPUAndArchitecture), mi);
		}

		public static bool AreRealTimeSignalsSafe ()
		{
			ushort built_for_cpu;
			ushort running_on_cpu;
			bool is64bit;

			detectCPUAndArchitecture (out built_for_cpu, out running_on_cpu, out is64bit);

			// CPUArchitecture builtForCPU = Enum.IsDefined (typeof (CPUArchitecture), built_for_cpu) ? (CPUArchitecture)built_for_cpu : CPUArchitecture.Unknown;
			// CPUArchitecture runningOnCPU = Enum.IsDefined (typeof (CPUArchitecture), running_on_cpu) ? (CPUArchitecture)running_on_cpu : CPUArchitecture.Unknown;

			// Log.Info (TAG, " Built for CPU: {0}", builtForCPU);
			// Log.Info (TAG, "Running on CPU: {0}", runningOnCPU);
			// Log.Info (TAG, "64-bit process: {0}", is64bit ? "yes" : "no");

			// For now real-time signals aren't safe at all, alas
			bool safe = false;
			// Log.Info (TAG, "Real-time signals are {0}safe on this platform", safe ? String.Empty : "not ");
			
			return safe;
		}
	}
}