using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class CrashyCrashy {
    [DllImport ("libtest")]
    public static extern void libtest_MerpCrashOnForeignThread ();
     
    public static void Main () {
	string configDir = "./merp-crash-test/";
	System.IO.Directory.CreateDirectory (configDir);
	SetupCrash (configDir);
	Console.WriteLine ("in managed");
	Go ();
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    public static void Go () {
	libtest_MerpCrashOnForeignThread ();
    }
  
    public static void SetupCrash (string configDir)
    {
	var monoType = Type.GetType ("Mono.Runtime", false);
	var m = monoType.GetMethod("EnableMicrosoftTelemetry", BindingFlags.NonPublic | BindingFlags.Static);

	// This leads to open -a /bin/cat, which errors out, but errors
	// in invoking merp are only logged errors, not fatal assertions.
	var merpGUIPath = "/bin/cat";
	var appBundleId = "com.xam.Minimal";
	var appSignature = "Test.Xam.Minimal";
	var appVersion = "123456";
	var eventType = "AppleAppCrash";
	var appPath = "/where/mono/lives";
	var m_params = new object[] { appBundleId, appSignature, appVersion, merpGUIPath, eventType, appPath, configDir };
	
	m.Invoke(null, m_params);	

	DumpLogSet ();
    }

	static void DumpLogSet ()
	{
		var monoType = Type.GetType ("Mono.Runtime", false);
		var convert = monoType.GetMethod("EnableCrashReportLog", BindingFlags.NonPublic | BindingFlags.Static);
		convert.Invoke(null, new object[] { "./" });
	}
}
